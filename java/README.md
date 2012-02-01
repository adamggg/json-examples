The Java example code is located in a [Git repository here]().

This example uses the following dependencies:

> [json-lib](http://json-lib.sourceforge.net/)- has other dependencies; check the project page
> 
> [google-gson](http://code.google.com/p/google-gson/)

**Installation**

This example depends on the required jars being in `./lib`. Necessary dependencies are:

- commons-beanutils-1.8.x: JSON-Lib
- commons-collections-3.2.x: JSON-Lib
- commons-lang-2.5+: JSON-Lib
- commons-logging-1.1.1: JSON-Lib
- ezmorph-1.0.6: JSON-Lib
- json-lib-2.4-jdk15.jar: JSON-Lib
- gson-2.1.jar: Google GSON

There are examples for both libraries, so only one is necessary. I'd recommend gson, since it's supported by Google and its only dependency is itself.

**Running the Example**

Since java doesn't compile the libraries into a single file, we'll have to include them in our classpath at runtime:

    java -cp .:lib/* example <url to Spotter>

### Code Walkthrough ###

There are four major functions:

- `getSettings`: make HTTP request to spotter and get the settings
- `setSettings`: make HTTP request to spotter to set settings
- `handleSettingsJsonLib`: JSON example using Json-Lib
- `handleSettingsGson`: JSON example using Gson

#### getSettings ####

Start off by creating a URL object, being sure to tack on the desired resource want in the constructor:

    URL tUrl = new URL(url + RESOURCE);

Open the connection:

    URLConnection req = tUrl.openConnection();

Get the data from the response stream:

    BufferedReader in = new BufferedReader(new InputStreamReader(req.getInputStream()));

    StringBuilder sb = new StringBuilder();
    String input;
    while ((input = in.readLine()) != null) {
        sb.append(input);
    }

That's it! The `input` string now contains the response from the Spotter; we use a JSON library below to manipulate it.

#### setSettings ####

This is much the same as the `getSettings` example, except we need to make sure the resource is `/settings` and that we set some headers. To start off, we get the bytes from our new settings:

    // we use UTF-8 because that's what the spotters use internally
    byte[] bytes = settings.getBytes("UTF-8");

Then we set up our connection:

    URL tUrl = new URL(url + RESOURCE + "/settings");
    HttpURLConnection req = (HttpURLConnection)tUrl.openConnection();
    req.setDoOutput(true);

    req.setRequestMethod("GET");
    req.setRequestProperty("Content-Type", "application/json");
    req.setFixedLengthStreamingMode(bytes.length);

You'll notice a few differences here. First of all, we use an `HttpURLConnection` instead of the `URLConnection`. In `getSettings`, we could have used a `HttpURLConnection` object instead, but we didn't need the features, so it was simpler to not typecast it.

Here, however, we need to be able to set the HTTP method and a few headers, so we use the real structure.

Next, we send our data up to the Spotter:

    DataOutputStream out = new DataOutputStream(req.getOutputStream());
    out.write(bytes, 0, bytes.length);
    out.flush();
    out.close();

And then get the response as before:

    BufferedReader in = new BufferedReader(new InputStreamReader(req.getInputStream()));

    StringBuilder sb = new StringBuilder();
    String input;
    while ((input = in.readLine()) != null) {
        sb.append(input);
    }

#### handleSettingsJsonLib ####

Json-lib has a lot of features, but we'll only use a small subset for this example. It is one of the more popular JSON libraries, probably because it is based on the [reference implementation](http://www.json.org/java) by Douglas Crockford.

To start off, we'll use `JSONSerializer` to parse our settings into a `JSONObject`:

    JSONObject obj = (JSONObject)JSONSerializer.toJSON(settings);

Then get the `result` property:

    JSONObject res = obj.getJSONObject("result");

Then we'll grab the altitude property and do some work with it:

    double alt = res.getDouble("altitude");

When we're done, just put it back:

    res.put("altitude", alt);

To grab the data as a string, just call it's `toString` method. That's it!

#### handleSettingsGson ####

Google-Gson is Google's implementation of a JSON parser. This is my preferred library for parsing JSON in Java mostly because it has no dependencies.

Gson has a way to serialize JSON directly to a Java Object (using a class definition), but we'll just use a simple iterative implementation for this example:

First, create the parser and parse the data into a JsonObject:

    JsonParser parser = new JsonParser();
    JsonObject obj = parser.parse(settings).getAsJsonObject();

Then pull the `result` object out of the data:

    JsonObject result = obj.getAsJsonObject("result");

Once we have this, we grab the altitude property and do some work with it:

    float alt = result.get("altitude").getAsFloat();

And when we're done, we'll put it back in:

    result.addProperty("altitude", alt);

To grab the data as a string, just call its `toString` method. That's it, we're done!

