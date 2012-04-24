JSON and Java: An unlikely couple
===

With the use of 

http://wiki.fasterxml.com/JacksonInFiveMinutes



gist.github.com

{
  "name" : { "first" : "Joe", "last" : "Sixpack" },
  "gender" : "MALE",
  "verified" : false,
  "userImage" : "Rm9vYmFyIQ=="
}

FYI: "Rm9vYmFyIQ==" decodes to "Foobar!"... not great image data

raw: https://raw.github.com/gist/2481734/bc37d3aa3521cd2645243d68663686e6dcce75bf/user.json
    
http://jsongen.byingtondesign.com/

com.acme.api

User

User.zip

    mkdir -p json-java-example/{src,lib}
    cd json-java-example/src
    mkdir lib
    cd lib
    curl 'http://repo1.maven.org/maven2/com/fasterxml/jackson/core/jackson-databind/2.0.1/jackson-databind-2.0.1.jar' -o jackson-databind-2.0.1.jar
    curl 'http://repo1.maven.org/maven2/com/fasterxml/jackson/core/jackson-core/2.0.1/jackson-core-2.0.1.jar' -o jackson-core-2.0.1.jar
    curl 'http://repo1.maven.org/maven2/com/fasterxml/jackson/core/jackson-annotations/2.0.1/jackson-annotations-2.0.1.jar' -o jackson-annotations-2.0.1.jar
    unzip ~/Downloads/User.zip
    open com/acme/api/user.json
    open com/acme/api/UserTest.java
    
user.json

    {
      "name" : { "first" : "Joe", "last" : "Sixpack" },
      "gender" : "MALE",
      "verified" : false,
      "userImage" : "Rm9vYmFyIQ=="
    }

UserTest.java

    
    ObjectMapper mapper = new ObjectMapper(); // can reuse, share globally
    User user = mapper.readValue(new File("user.json"), User.class);
