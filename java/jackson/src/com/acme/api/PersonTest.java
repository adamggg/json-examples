package com.acme.api;

import com.acme.api.Person;
//import org.codehaus.jackson.map.ObjectMapper;
import com.fasterxml.jackson.core.JsonParseException;
import com.fasterxml.jackson.databind.JsonMappingException;
import com.fasterxml.jackson.databind.ObjectMapper;

import java.io.File;
import java.io.IOException;

public class PersonTest {
  public static void main(String[] args) throws JsonParseException, JsonMappingException, IOException {
    ObjectMapper mapper = new ObjectMapper(); // can reuse, share globally
    Person person = mapper.readValue(new File("person.json"), Person.class);
    System.out.println(person.getName().getFirst());
  }
}

