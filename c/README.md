The example C code is located in a [Git repository here](https://github.com/SpotterRF/json-examples/tree/master/c).

This example will:

1. Get the `/geolocation.json` resource from a Spotter
2. Grab the 'altitude' value and do some simple modifications to it
3. Post the changed JSON data back to the Spotter

### Running the Example JSON Code

There are some prerequisits:

  * [json-c](http://oss.metaparadigm.com/json-c/): json parsing library for C
  * [libcurl](http://curl.haxx.se/libcurl/): library for making HTTP calls


#### libcurl

    sudo aptitude install libcurl-dev # or possibly libcurl4-openssl-dev
    
    # or
    # sudo yum install libcurl-devel
    # or
    # follow the instructions on <http://curl.haxx.se>

#### json-c

    wget http://oss.metaparadigm.com/json-c/json-c-0.9.tar.gz
    tar xvf json-c-0.9.tar.gz
    cd json-c-0.9
    ./configure
    make
    sudo make install

#### the main event

    git clone git@github.com:SpotterRF/json-examples.git
    cd json-examples/c/
    cmake .
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

