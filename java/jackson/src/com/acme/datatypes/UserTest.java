package com.acme.datatypes;

import com.acme.datatypes.User;
// the old namespace mentioned in the original tutorial is outdated
//import org.codehaus.jackson.map.ObjectMapper;
import com.fasterxml.jackson.core.JsonParseException;
import com.fasterxml.jackson.databind.JsonMappingException;
import com.fasterxml.jackson.databind.ObjectMapper;

import java.io.File;
import java.net.URL;
import java.io.IOException;

public class UserTest {
  public static void main(String[] args) throws JsonParseException, JsonMappingException, IOException {
    File jsonFile = new File("user.json");
    URL jsonUrl = new URL("https://raw.github.com/gist/2481734/bc37d3aa3521cd2645243d68663686e6dcce75bf/user.json");
    String jsonStr = 
      "{\"name\":{\"first\":\"Joe\",\"last\":\"Sixpack\"},\"gender\":\"MALE\",\"verified\":false,\"userImage\":\"Rm9vYmFyIQ==\"}";
    User user = null;

    ObjectMapper mapper = new ObjectMapper(); // can reuse, share globally

    user = mapper.readValue(jsonFile, User.class);
    System.out.println(user.getName().getFirst());

    user = mapper.readValue(jsonUrl, User.class);
    System.out.println(user.getName().getLast());

    user = mapper.readValue(jsonStr, User.class);
    System.out.println(user.getGender());
  }
}

