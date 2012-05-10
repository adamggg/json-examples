import java.io.BufferedReader;
import java.io.DataOutputStream;
import java.io.InputStreamReader;

import java.net.URL;
import java.net.URLConnection;
import java.net.HttpURLConnection;

import net.sf.json.*;
import com.google.gson.JsonParser;
import com.google.gson.JsonObject;

import java.util.List;
import java.util.Map;

import java.nio.ByteBuffer;
import java.util.zip.GZIPInputStream;

public class example {
    private static final String RESOURCE = "/geolocation.json";

    private static String getSettings(String url) throws Exception {
        URL tUrl = new URL(url + RESOURCE);

        // open the connection
        URLConnection req = tUrl.openConnection();

        // get the response
        BufferedReader in = new BufferedReader(new InputStreamReader(req.getInputStream()));

        StringBuilder sb = new StringBuilder();
        String input;
        while ((input = in.readLine()) != null) {
            sb.append(input);
        }

        return sb.toString();
    }

    private static String getSettingsGzip(String url) throws Exception {
        boolean gzipped = false;
        URL tUrl = new URL(url + RESOURCE);

        // open the connection
        URLConnection req = tUrl.openConnection();
        // state that we'll accept gzip encoding
        req.setRequestProperty("Accept-Encoding", "gzip;q=1.0");

        // get the response headers
        Map<String, List<String>> headers = req.getHeaderFields();

        for (String key : headers.keySet()) {
            if (key != null && key.equalsIgnoreCase("Content-Encoding")) {
                for (String val : headers.get(key)) {
                    if (val.equalsIgnoreCase("gzip")) {
                        gzipped = true;
                        break;
                    }
                }
                break;
            }
        }

        if (gzipped) {
            GZIPInputStream in = new GZIPInputStream(req.getInputStream());

            ByteBuffer buf = ByteBuffer.allocate(2048);
            int b;
            int read = 0;
            while ((b = in.read()) != -1) {
                read++;
                buf.put((byte)b);
            }

            return new String(buf.array(), 0, read, "UTF-8");
        } else {
            // get the response
            BufferedReader in = new BufferedReader(new InputStreamReader(req.getInputStream()));

            StringBuilder sb = new StringBuilder();
            String input;
            while ((input = in.readLine()) != null) {
                sb.append(input);
            }

            return sb.toString();
        }
    }

    private static String setSettings(String url, String settings) throws Exception {
        byte[] bytes = settings.getBytes("UTF-8");

        URL tUrl = new URL(url + RESOURCE + "/settings");
        HttpURLConnection req = (HttpURLConnection)tUrl.openConnection();
        req.setDoOutput(true);

        req.setRequestMethod("GET");
        req.setRequestProperty("Content-Type", "application/json");
        req.setFixedLengthStreamingMode(bytes.length);

        DataOutputStream out = new DataOutputStream(req.getOutputStream());
        out.write(bytes, 0, bytes.length);
        out.flush();
        out.close();

        // get the response
        BufferedReader in = new BufferedReader(new InputStreamReader(req.getInputStream()));

        StringBuilder sb = new StringBuilder();
        String input;
        while ((input = in.readLine()) != null) {
            sb.append(input);
        }

        return sb.toString();
    }

    private static String handleSettingsJsonLib(String settings) {
        JSONObject obj = (JSONObject)JSONSerializer.toJSON(settings);
        JSONObject res = obj.getJSONObject("result");

        double alt = res.getDouble("altitude");
        if (alt > 2000) {
            alt = 747;
        } else {
            alt += 1000;
        }

        res.put("altitude", alt);

        return res.toString();
    }

    private static String handleSettingsGson(String settings) {
        JsonParser parser = new JsonParser();
        JsonObject obj = parser.parse(settings).getAsJsonObject();
        JsonObject result = obj.getAsJsonObject("result");

        float alt = result.get("altitude").getAsFloat();
        if (alt > 2000) {
            alt = 747;
        } else {
            alt += 1000;
        }

        result.addProperty("altitude", alt);
        return result.toString();
    }

    public static void main(String[] args) throws Exception {
        if (args.length != 1) {
            System.out.println("Usage: java example <url>");
            return;
        }

        // make sure our url has the correct protocol
        String url = args[0];
        if (!url.startsWith("http://")) {
            url = "http://" + url;
        }
        if (url.endsWith("/")) {
            url = url.substring(0, url.length() - 1);
        }

        String res = getSettings(url);

        System.out.println("Before:");
        System.out.println(res);
        System.out.println();

        //res = handleSettingsJsonLib(res);
        res = handleSettingsGson(res);

        System.out.println("After:");
        System.out.println(res);
        System.out.println();

        res = setSettings(url, res);

        System.out.println("POST Response:");
        System.out.println(res);
        System.out.println();

        res = getSettingsGzip(url);

        System.out.println("New Settings:");
        System.out.println(res);
    }
}
