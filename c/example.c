// standard stuff
#include <stdlib.h>
#include <string.h>
#include <stdio.h>

// curl
#include <curl/curl.h>
#include <curl/easy.h>

// json parser
#include <json/json.h>

// resource to get/set
#define RESOURCE "/geolocation.json"
#define TRACKS "/tracks.json"

static int writeFn(void* buf, size_t len, size_t size, void* userdata) {
    size_t sLen;

    // if this is zero, then it's done
    // we don't do any special processing on the end of the stream
    if (len * size > 0) {
        // get the length of the current string
        sLen = strlen((char*)userdata);

        // copy the data from the buffer
        // there are no checks here, but the buffer should be big enough
        strncpy(&((char*)userdata)[sLen], (char*)buf, (len * size));
    }

    return len * size;
}

int getSettings(char* url, char* data) {
    int res = -1;
    CURL* pCurl = curl_easy_init();

    if (!pCurl) {
        return 0;
    }

    // setup curl
    curl_easy_setopt(pCurl, CURLOPT_URL, url);
    curl_easy_setopt(pCurl, CURLOPT_WRITEFUNCTION, writeFn);
    // we don't care about progress
    curl_easy_setopt(pCurl, CURLOPT_NOPROGRESS, 1);
    curl_easy_setopt(pCurl, CURLOPT_FAILONERROR, 1);
    curl_easy_setopt(pCurl, CURLOPT_WRITEDATA, data);

    // set a 1 second timeout
    curl_easy_setopt(pCurl, CURLOPT_TIMEOUT, 1);

    // synchronous, but we don't really care
    res = curl_easy_perform(pCurl);

    // cleanup after ourselves
    curl_easy_cleanup(pCurl);

    return res;
}

int getSettingsGzip(char* url, char* data) {
    int res = -1;
    CURL* pCurl = curl_easy_init();

    int res1;

    if (!pCurl) {
        return 0;
    }

    // setup curl
    curl_easy_setopt(pCurl, CURLOPT_URL, url);
    curl_easy_setopt(pCurl, CURLOPT_WRITEFUNCTION, writeFn);
    // we don't care about progress
    curl_easy_setopt(pCurl, CURLOPT_NOPROGRESS, 1);
    curl_easy_setopt(pCurl, CURLOPT_FAILONERROR, 1);
    curl_easy_setopt(pCurl, CURLOPT_WRITEDATA, data);

    // add the gzip header
    curl_easy_setopt(pCurl, CURLOPT_ACCEPT_ENCODING, "gzip;q=1.0");

    // set a 1 second timeout
    curl_easy_setopt(pCurl, CURLOPT_TIMEOUT, 1);

    // synchronous, but we don't really care
    res = curl_easy_perform(pCurl);

    // cleanup after ourselves
    curl_easy_cleanup(pCurl);

    return res;
}

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

int postSettings(char* url, char* data) {
    int res = -1;
    char tmp[2048];
    CURL* pCurl = curl_easy_init();

    // we need to set headers later
    struct curl_slist* headers = NULL;

    if (!pCurl) {
        return 0;
    }

    // we'll use data to store the result
    memset(tmp, 0, 2048);

    // add the application/json content-type
    // so the server knows how to interpret our HTTP POST body
    headers = curl_slist_append(headers, "Content-Type: application/json");

    // setup curl
    curl_easy_setopt(pCurl, CURLOPT_URL, url);
    curl_easy_setopt(pCurl, CURLOPT_POST, 1);
    curl_easy_setopt(pCurl, CURLOPT_POSTFIELDSIZE, strlen(data));
    curl_easy_setopt(pCurl, CURLOPT_HTTPHEADER, headers);
    curl_easy_setopt(pCurl, CURLOPT_READFUNCTION, read_callback);
    curl_easy_setopt(pCurl, CURLOPT_READDATA, data);
    curl_easy_setopt(pCurl, CURLOPT_WRITEFUNCTION, writeFn);
    curl_easy_setopt(pCurl, CURLOPT_WRITEDATA, tmp);
    // we don't care about progress
    curl_easy_setopt(pCurl, CURLOPT_NOPROGRESS, 1);
    curl_easy_setopt(pCurl, CURLOPT_FAILONERROR, 1);

    // set a 1 second timeout
    curl_easy_setopt(pCurl, CURLOPT_TIMEOUT, 1);

    // synchronous, but we don't really care
    res = curl_easy_perform(pCurl);

    // cleanup after ourselves
    curl_easy_cleanup(pCurl);
    curl_slist_free_all(headers);

    // copy the response to data
    strcpy(data, tmp);
    return res;
}

void handleSettings(char* data) {
    struct json_object* settingsJson;
    struct json_object* results;
    struct json_object* altitudeObj;
    double alt;

    // parse the string into json
    settingsJson = json_tokener_parse(data);

    // get the results object
    // this is common among all outputs
    results = json_object_object_get(settingsJson, "result");

    // get the altitude as a double
    // we'll change this conditionally and give it back
    altitudeObj = json_object_object_get(results, "altitude");

    // parse it as a double
    alt = json_object_get_double(altitudeObj);

    printf("Current altitude: %f\n", alt);

    // do some work with this
    // for this example, we'll just do something useless
    if (alt > 2000) {
        // something to signify that this worked
        alt = 747;
    } else {
        // increment to show that this worked
        alt += 1000;
    }

    printf("New altitude: %f\n\n", alt);

    altitudeObj = json_object_new_double(alt);
    json_object_object_add(results, "altitude", altitudeObj);

    // return the updated info
    strcpy(data, json_object_to_json_string(results));
}

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

void processTrack (char* data) {
    struct json_object* settingsJson;
    struct json_object* results;
    struct json_object* currentTrack;
    struct json_object* id;

    // geolocation structures
    struct json_object* geolocation;
    struct json_object* latitude;
    struct json_object* longitude;
    struct json_object* altitude;
    struct json_object* accuracy;
    struct json_object* altitudeAccuracy;
    struct json_object* bearing;
    struct json_object* heading;
    struct json_object* speed;

    // observation structures
    struct json_object* observation;
    struct json_object* range;
    struct json_object* radialVelocity;
    struct json_object* horizontalAngle;
    struct json_object* azimuthAngle;
    struct json_object* verticalAngle;
    struct json_object* altitudeAngle;

    // stats structures
    struct json_object* stats;
    struct json_object* rcs;

    struct json_object* timestamp;
    
    int length = 0;
    int i = 0;

    // parse the data into JSON struct
    settingsJson = json_tokener_parse(data);

    /*
        select the "result" of the settings -- note there is only one "result"
        array even for multiple tracks
    */
    results = json_object_object_get(settingsJson, "result");

    // get the length (the number of tracks in the array)
    length = json_object_array_length(results);

    // iterate over the array of tracks
    for (i = 0; i < length; i++) {
        currentTrack = json_object_array_get_idx(results, i);

        id = json_object_object_get(currentTrack, "id");

        // get geolocation data
        geolocation = json_object_object_get(currentTrack, "geolocation");
        latitude = json_object_object_get(geolocation, "latitude");
        longitude = json_object_object_get(geolocation, "longitude");
        altitude = json_object_object_get(geolocation, "altitude");
        accuracy = json_object_object_get(geolocation, "accuracy");
        altitudeAccuracy = json_object_object_get(geolocation, "altitudeAccuracy");
        bearing = json_object_object_get(geolocation, "bearing");
        heading = json_object_object_get(geolocation, "heading");
        speed = json_object_object_get(geolocation, "speed");

        // get observation data
        observation = json_object_object_get(currentTrack, "observation");
        range = json_object_object_get(observation, "range");
        radialVelocity = json_object_object_get(observation, "radialVelocity");
        horizontalAngle = json_object_object_get(observation, "horizontalAngle");
        azimuthAngle = json_object_object_get(observation, "azimuthAngle");
        verticalAngle = json_object_object_get(observation, "verticalAngle");
        altitudeAngle = json_object_object_get(observation, "altitudeAngle");

        // get stats data
        stats = json_object_object_get(currentTrack, "stats");
        rcs = json_object_object_get(stats, "rcs");

        timestamp = json_object_object_get(currentTrack, "timestamp");

        printf("\nTrack\n");
        printf("    id: %s\n", json_object_get_string(id));
        printf("    geolocation:\n");
        printf("        latitude: %s\n", json_object_get_string(latitude));
        printf("        longitude: %s\n", json_object_get_string(longitude));
        printf("        altitude: %s\n", json_object_get_string(altitude));
        printf("        accuracy: %s\n", json_object_get_string(accuracy));
        printf("        altitudeAccuracy: %s\n", json_object_get_string(altitudeAccuracy));
        printf("        bearing: %s\n", json_object_get_string(bearing));
        printf("        heading: %s\n", json_object_get_string(heading));
        printf("        speed: %s\n", json_object_get_string(speed));
        printf("    observation:\n");
        printf("        range: %s\n", json_object_get_string(range));
        printf("        radialVelocity: %s\n", json_object_get_string(radialVelocity));
        printf("        horizontalAngle: %s\n", json_object_get_string(horizontalAngle));
        printf("        azimuthAngle: %s\n", json_object_get_string(azimuthAngle));
        printf("        verticalAngle: %s\n", json_object_get_string(verticalAngle));
        printf("        altitudeAngle: %s\n", json_object_get_string(altitudeAngle));
        printf("    stats:\n");
        printf("        rcs: %s\n", json_object_get_string(rcs));
        printf("    timestamp: %s\n", json_object_get_string(timestamp));
    }
}

int main(int argc, char** argv) {
    int res;
    int iLen;

    // should be big enough for most things
    // more flexibility should be added for a real application
    char url[256];
    char trackUrl[256];
    char data[2048];
    char trackData[2048];

    // print usage if the input doesn't match what is expected
    if (argc != 2) {
        printf("Usage: example <url>\n");
        return -1;
    }

    // clear our memory
    memset(url, 0, sizeof(url));
    memset(trackUrl, 0, sizeof(trackUrl));
    memset(data, 0, sizeof(data));
    memset(trackData, 0, sizeof(trackData));

    // the url is the second arg
    strcpy(url, argv[1]);
    iLen = strlen(url);
    // remove the trailing /
    if (url[iLen - 1] == '/') {
        url[iLen - 1] = '\0';
        iLen--;
    }

    // add the resource to the end of the URL
    strcpy(&url[iLen], RESOURCE);

    // get the current settings
    res = getSettings(url, data);

    // output the starting settings
    printf("Original Settings:\n%s\n\n", data);

    // make sense of the data received and make changes
    handleSettings(data);

    // settings can only be set by posting to /resource/settings
    iLen = strlen(url);
    strcpy(&url[iLen], "/settings");

    // set our new and improved settings
    postSettings(url, data);

    printf("Result from POST:\n%s\n\n", data);

    memset(data, 0, 2048);

    // get the current settings, but gzipped
    res = getSettingsGzip(url, data);

    // output our new settings
    printf("New Settings (gzipped response):\n%s\n", data);

    // set the url to get track information
    setTrackUrl(trackUrl, argv[1]);

    // get the current track information
    res = getSettings(trackUrl, trackData);

    // iterate over all tracks
    processTrack(trackData);

    return res;
}
