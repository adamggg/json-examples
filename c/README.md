The example C code is located in a [Git repository here](https://github.com/SpotterRF/json-examples/tree/master/c).

This example will:

1. Get the `/geolocation.json` resource from a Spotter
2. Grab the 'altitude' value and do some simple modifications to it
3. Post the changed JSON data back to the Spotter

### Running the Example JSON Code

There are some prerequisits:

  * [json-c](http://oss.metaparadigm.com/json-c/) (v0.9): json parsing library for C
  * [libcurl](http://curl.haxx.se/libcurl/) (v7): library for making HTTP calls
  * [cmake](http://www.cmake.org/) (v2.8): makefile generator for C

#### libcurl

    wget http://curl.haxx.se/download/curl-7.25.0.tar.gz
    tar xvf curl-7.25.0.tar.gz
    cd curl-7.25.0
    ./configure
    make
    sudo make install
    sudo ldconfig

#### json-c

    wget http://oss.metaparadigm.com/json-c/json-c-0.9.tar.gz
    tar xvf json-c-0.9.tar.gz
    cd json-c-0.9
    ./configure
    make
    sudo make install
    sudo ldconfig

#### cmake

    wget http://www.cmake.org/files/v2.8/cmake-2.8.7.tar.gz
    tar xvzf cmake-2.8.7.tar.gz
    cd cmake-2.8.7
    ./configure
    make
    sudo make install

#### the main event

    git clone git://github.com/SpotterRF/json-examples.git
    cd json-examples/c/
    rm -rf ./build
    mkdir -p ./build
    cd ./build
    cmake ..
    make
    ./example 192.168.254.254 # ip or hostname of spotter

### Example Code Walkthrough ###

The important parts are in `getSettings`, `handleSettings`, and `postSettings`, which are documented below.

Please see the entire source for details. Note that the example code is not very robust.

Please see the documentation for the libraries for information about how to properly free all used memory and for best practices for usage.

#### getSettings ####

This function takes a URL constructed like this:

> spotter.host-name.com/geolocation.json

or this:

> 192.168.254.254/geolocation.json

To start out, we initialize our CURL object that we will use for the remainder of the function:

    CURL* pCurl = curl_easy_init();

First, we give it a URL:

    curl_easy_setopt(pCurl, CURLOPT_URL, url);

Then we tell CURL that we'd like to handle our own data:

    // give CURL a callback to call on each chunk
    curl_easy_setopt(pCurl, CURLOPT_WRITEFUNCTION, writeFn);

    // pass a pointer to some data structure
    curl_easy_setopt(pCurl, CURLOPT_WRITEDATA, data);

The pointer we pass in will be passed to writeFn. For our purposes, our writeFn is really simple:

    static int writeFn(void* buf, size_t len, size_t size, void* userdata) {
        size_t sLen;

        if (len * size > 0) {
            sLen = strlen((char*)userdata);

            strncpy(&((char*)userdata)[sLen], (char*)buf, (len * size));
        }

        return len * size;
    }

This basically just copies data from CURL's buffer to our own. If we wanted to copy this somewhere special, or do incremental parsing on the data, this would be the place.

Last, we just set some safety options:

    // turn off the progress bar
    curl_easy_setopt(pCurl, CURLOPT_NOPROGRESS, 1);

    // fail when there's an error
    curl_easy_setopt(pCurl, CURLOPT_FAILONERROR, 1);

    // timeout after 1 second instead of waiting the customary 120
    curl_easy_setopt(pCurl, CURLOPT_TIMEOUT, 1);

Once all of that initialization is done, just tell CURL to perform the GET and cleanup afterwards:

    curl_easy_perform(pCurl);

    curl_easy_cleanup(pCurl);

That's it!

#### getSettingsGzip ####

This is basically the same as `getSettings`, but we include the "Allow-Encoding" header to tell the server that we will accept gzip encoding:

    curl_easy_setopt(pCurl, CURLOPT_ACCEPT_ENCODING, "gzip;q=1.0");

`libcurl` will handle everything else.

#### handleSettings ####

This is where our JSON parsing happens. It just takes the data that we CURL read in for us from the spotter.

Start out by initializing our JSON parser:
    
    struct json_object* settingsJson;
    settingsJson = json_tokener_parse(data);

Since every data packet has a 'result' property (if it was successful), we grab that:

    struct json_object* results;
    results = json_object_object_get(settingsJson, "result");

For this example, we read and modify the 'altitude' field:

    struct json_object* altitudeObj;
    altitudeObj = json_object_object_get(results, "altitude");

As you'll notice, all of the return objects for this JSON library are pointers to a predefined struct. To get data that we can use, we must tell it what to give us. since 'altitude' is a number, we'll pull it out as a double. Altertatively, we could ask the library what the type is, but we already know what we want:

    double alt;
    alt = json_object_get_double(altitudeObj);

Once we're done messing with the altitude, we go ahead and write it back to the object:

    altitudeObj = json_object_new_double(alt);
    json_object_object_add(results, "altitude", altitudeObj);

This creates a new struct and adds it back in, overwriting whatever is there. Lastly, we need to turn the object into a string so it can be sent back to the Spotter:

    strcpy(data, json_object_to_json_string(results));

#### postSettings ####

As we did in getSettings, we'll initialize a new CURL instance:

    CURL* pCurl = curl_easy_init();
    curl_easy_setopt(pCurl, CURLOPT_URL, url);
    curl_easy_setopt(pCurl, CURLOPT_NOPROGRESS, 1);
    curl_easy_setopt(pCurl, CURLOPT_FAILONERROR, 1);
    curl_easy_setopt(pCurl, CURLOPT_TIMEOUT, 1);

We'll need to tell CURL this is a POST, and since we know the size of the data we'll throw that in too:

    curl_easy_setopt(pCurl, CURLOPT_POST, 1);
    curl_easy_setopt(pCurl, CURLOPT_POSTFIELDSIZE, strlen(data));

We'll need to set the content type of the message. This is necessary for the spotter to recognize the data as JSON. Here's the syntax for that:

    struct curl_slist* headers = NULL;
    headers = curl_slist_append(headers, "Content-Type: application/json");
    curl_easy_setopt(pCurl, CURLOPT_HTTPHEADER, headers);

Just as last time, we'll define our own handler for the data, but this time it's a post:

    curl_easy_setopt(pCurl, CURLOPT_READFUNCTION, read_callback);
    curl_easy_setopt(pCurl, CURLOPT_READDATA, data);

And the function:

    static size_t read_callback(void* ptr, size_t size, size_t nmemb, void* userdata) {
        int tLen = strlen(userdata);

        if (tLen > 0) {
            // assign the string as the data to be sent
            strcpy(ptr, userdata);

            // clear the string
            ((char*)userdata)[0] = 0;
        }

        return tLen;
    }

Finally, perform the request and clean up after ourselves:

    curl_easy_perform(pCurl);
    curl_easy_cleanup(pCurl);
    curl_slist_free_all(headers);

### Iterating Over Track Object ###

The previous code examples demonstrated how to get and post settings using
libcurl, and specifically how to modify the altitude from the geolocation.json
resource. This example shows how to iterate over a more complicated json object
-- a track. The important functions from example.c are `setTrackUrl` and
`processTrack`.

#### setTrackUrl ####

This function sets the url for curl to get track information. The first
parameter to the function is an output variable that will point to the url and
the second is a pointer to the hostname or IP address of the spotter.

    #define TRACKS "/tracks.json"

    void setTrackUrl (char* url, char* host) {
        int urlLength = 0;

        // copy the host first
        strcpy(url, host);

        urlLength = strlen(url);
        // remove the trailing /
        if (url[urlLength - 1] == '/') {
            url[urlLength - 1] = '\0';
            urlLength--;
        }

        // add the resource to the end of the URL
        strcpy(&url[urlLength], TRACKS);
    }

This function generates the url necessary for the curl call and stores it in
`url`. For example, if the IP address of the Spotter were 192.168.24.107, then
`url` would be `192.168.24.107/tracks.json`.

Using this function is simple. Here is how it is used in context with the
`getSettings` functions (defined earlier) and the `processTrack` function, which
will be explained next:

    int main(int argc, char** argv) {
        int res;

        // should be big enough for most things
        // more flexibility should be added for a real application
        char trackUrl[256];
        char trackData[2048];

        // clear our memory
        memset(trackUrl, 0, sizeof(trackUrl));
        memset(trackData, 0, sizeof(trackData));

        // set the url to get track information
        // the hostname/ip is the second command line parameter
        setTrackUrl(trackUrl, argv[1]);

        // get the current track information
        res = getSettings(trackUrl, trackData);

        // iterate over all tracks
        processTrack(trackData);

        return res;
    }



#### processTrack ####

In this function, we actually iterate over all the track data. Since the track
object has a relatively large number of fields, here is an example json object
for reference:

    {
        "id": 560
      , "geolocation": {
            "latitude": 40.33057
          , "longitude": -111.678521
          , "altitude": 1475.5
          , "accuracy": null
          , "altitudeAccuracy": null
          , "bearing": null
          , "heading": 284.114807
          , "speed": 5.872576
        }
      , "observation": {
            "range": 516.981402
          , "radialVelocity": 12.678081
          , "horizontalAngle": -0.794151
          , "azimuthAngle": 358.206726
          , "verticalAngle": null
          , "altitudeAngle": null
        }
      , "stats": {
            "rcs": 0.745098
        }
      , "timestamp": 1336839367666
    }

For more details regarding the track information, please see our
[API documentation](http://dev.spotterrf.com/docs/latest/#tracks_json).

As in previous examples, we first use the `json_tokener_parse` function to parse
the track data into a json structure:


    void processTrack (char* data) {
        struct json_object* settingsJson;

        // parse the track data into json structure
        settingsJson = json_tokener_parse(data);

        ...
    }

Next, we pull out the `result` array from the json structure we just created:

    struct json_object* results;

    results = json_object_object_get(settingsJson, "result");

Please note that even though `result` is an array, we still access it with the
function `json_object_object_get`.

Then we find out what the length of the `result` array is using
`json_object_array_length` so that we can iterate over each track:

    int length = 0;
    int i = 0;

    // grab the length of the array (number of tracks)
    length = json_object_array_length(results);

    for (i = 0; i < length; i++) {
      // process each track here
    }

Within the for loop, we now can access each track object using the index `i` and
the function `json_object_array_get_idx`:

    struct json_object* currentTrack;

    currentTrack = json_object_array_get_idx(results, i);

Using `currentTrack` we can access all child objects:

    // id member
    struct json_object* id = json_object_object_get(currentTrack, "id");

    // get geolocation data
    struct json_object* geolocation = json_object_object_get(currentTrack, "geolocation");
    struct json_object* latitude = json_object_object_get(geolocation, "latitude");
    struct json_object* longitude = json_object_object_get(geolocation, "longitude");
    struct json_object* altitude = json_object_object_get(geolocation, "altitude");
    struct json_object* accuracy = json_object_object_get(geolocation, "accuracy");
    struct json_object* altitudeAccuracy = json_object_object_get(geolocation, "altitudeAccuracy");
    struct json_object* bearing = json_object_object_get(geolocation, "bearing");
    struct json_object* heading = json_object_object_get(geolocation, "heading");
    struct json_object* speed = json_object_object_get(geolocation, "speed");

    // get observation data
    struct json_object* observation = json_object_object_get(currentTrack, "observation");
    struct json_object* range = json_object_object_get(observation, "range");
    struct json_object* radialVelocity = json_object_object_get(observation, "radialVelocity");
    struct json_object* horizontalAngle = json_object_object_get(observation, "horizontalAngle");
    struct json_object* azimuthAngle = json_object_object_get(observation, "azimuthAngle");
    struct json_object* verticalAngle = json_object_object_get(observation, "verticalAngle");
    struct json_object* altitudeAngle = json_object_object_get(observation, "altitudeAngle");

    // get stats data
    struct json_object* stats = json_object_object_get(currentTrack, "stats");
    struct json_object* rcs = json_object_object_get(stats, "rcs");

    // timestamp member
    struct json_object* timestamp = json_object_object_get(currentTrack, "timestamp");

And print them to the screen, if we desire:

    printf("\nTrack\n");
    printf("    id: %d\n", json_object_get_int(id));
    printf("    geolocation:\n");
    printf("        latitude: %f\n", json_object_get_double(latitude));
    printf("        longitude: %f\n", json_object_get_double(longitude));
    printf("        altitude: %f\n", json_object_get_double(altitude));
    printf("        accuracy: %f\n", json_object_get_double(accuracy));
    printf("        altitudeAccuracy: %f\n", json_object_get_double(altitudeAccuracy));
    printf("        bearing: %f\n", json_object_get_double(bearing));
    printf("        heading: %f\n", json_object_get_double(heading));
    printf("        speed: %f\n", json_object_get_double(speed));
    printf("    observation:\n");
    printf("        range: %f\n", json_object_get_double(range));
    printf("        radialVelocity: %f\n", json_object_get_double(radialVelocity));
    printf("        horizontalAngle: %f\n", json_object_get_double(horizontalAngle));
    printf("        azimuthAngle: %f\n", json_object_get_double(azimuthAngle));
    printf("        verticalAngle: %f\n", json_object_get_double(verticalAngle));
    printf("        altitudeAngle: %f\n", json_object_get_double(altitudeAngle));
    printf("    stats:\n");
    printf("        rcs: %f\n", json_object_get_double(rcs));
    printf("    timestamp: %d\n", json_object_get_int(timestamp));

Note that to convert the JSON structure to its appropriate data type, use either
the function `json_object_get_int` or `json_object_get_double`. 
