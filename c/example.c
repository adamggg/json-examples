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
    CURL* pCurl = curl_easy_init();

    // we need to set headers later
    struct curl_slist* headers = NULL;

    if (!pCurl) {
        return 0;
    }

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

    printf("New altitude: %f\n", alt);

    altitudeObj = json_object_new_double(alt);
    json_object_object_add(results, "altitude", altitudeObj);

    // return the updated info
    strcpy(data, json_object_to_json_string(results));
}

int main(int argc, char** argv) {
    int res;
    int iLen;

    // should be big enough for most things
    // more flexibility should be added for a real application
    char url[256];
    char data[2048];

    // print usage if the input doesn't match what is expected
    if (argc != 2) {
        printf("Usage: example <url>\n");
        return -1;
    }

    // clear our memory
    memset(url, 0, sizeof(url));
    memset(data, 0, sizeof(data));

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

    // make sense of the data received and make changes
    handleSettings(data);

    // settings can only be set by posting to /resource/settings
    iLen = strlen(url);
    strcpy(&url[iLen], "/settings");

    // set our new and improved settings
    postSettings(url, data);

    return res;
}
