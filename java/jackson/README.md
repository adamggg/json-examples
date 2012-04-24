JSON and Java: Unlikely Bedfellows
===

Think JSON: Easy, Breezy, Beautiful. So simple and elegant.

Think Java: Strict, Harsh, Ugly. So convoluted and snarly.

This isn't your run-of-the-mill Beauty and the Beast scenario.
Under normal circumstances, these opposites don't attract.

... but they do now!

With the combined power of [JSONGen](http://jsongen.byingtondesign.com/) by [Rick Byington](http://byingtondesign.com)
and [Jackson](http://wiki.fasterxml.com/JacksonInFiveMinutes) by [FasterXML](fasterxml.com)

Compile and Run the example code
===

    git clone git://github.com/SpotterRF/json-examples.git
    cd json-examples/java/jackson
    make
    make test

3-minute Tutorial
===

We'll start with the same data in the example on [Jackson in 5 Minutes](http://wiki.fasterxml.com/JacksonInFiveMinutes).

Create an 'object' JSON (no class required!)
---

Since JSON only supports arrays, maps, numbers, booleans, and strings there is no need to define a class, yet it easy maps to a class in any language.

  1. Go to <http://gist.github.com>
  2. Enter `Jackson Java Example` as `Gist description...`
  3. Enter `person.json` as `name this file...`
  4. Enter the following as the file contents

        {
            "name" : {
                "first" : "Joe"
              , "last" : "Sixpack"
            }
          , "gender" : "MALE"
          , "verified" : false
          , "userImage" : "Rm9vYmFyIQ=="
        }

      FYI: "Rm9vYmFyIQ==" decodes to "Foobar!"... so it's not actually great image data

  5. Click `Create Private Gist` (not Gooleable) or `Create Public Gist` (if you prefer)
  6. Click `raw`
  7. Save that URL!

      (or use this public one <https://raw.github.com/gist/2481734/bc37d3aa3521cd2645243d68663686e6dcce75bf/user.json>)

      (or this private one <https://raw.github.com/gist/1e3d2350ed163a6147a7/bc37d3aa3521cd2645243d68663686e6dcce75bf/person.json>)

Create a 'class' from the JSON object (no schema required!)
---

Now we're going to create a [Bean](http://en.wikipedia.org/wiki/JavaBeans#JavaBean_Example), which is known in other languages as a Value Object or Struct.

Since the types of JSON properties are well defined by the minimalistic syntax, no schema is required. 
    
  1. Go to <http://jsongen.byingtondesign.com/>
  2. Paste the URL to the raw JSON you created earlier as `JSON URL`
  3. Enter `com.acme.datatypes` as `Java Package Name`
  4. Enter `User` as `Object Class Name`
  5. Click `Generate` to download your Java classes in a zip file (`User.zip`)

Create a test application
---

Create your project directory

    cd
    mkdir -p json-java-example/{src,lib}
    cd json-java-example

Get the Jackson jars

    pushd lib
    curl 'http://repo1.maven.org/maven2/com/fasterxml/jackson/core/jackson-databind/2.0.1/jackson-databind-2.0.1.jar' -o jackson-databind-2.0.1.jar
    curl 'http://repo1.maven.org/maven2/com/fasterxml/jackson/core/jackson-core/2.0.1/jackson-core-2.0.1.jar' -o jackson-core-2.0.1.jar
    curl 'http://repo1.maven.org/maven2/com/fasterxml/jackson/core/jackson-annotations/2.0.1/jackson-annotations-2.0.1.jar' -o jackson-annotations-2.0.1.jar
    popd

Assuming that your `User.zip` was downloaded to `~/Downloads`

    pushd src
    unzip ~/Downloads/User.zip
    popd

Create the file `com/acme/datatypes/UserTest.java`

Create the file `user.json` with the contents of the original example if you wish to use `new File("user.json")` rather than the url.

WARNING: There are a few bugs in `JSONGen`. In particular `boolean` getters are generated as `getFoo` rather than `isFoo` (as per [Java Bean](http://en.wikipedia.org/wiki/JavaBeans#JavaBean_Example). I've contacted the author and hope to hear back about the update soon. Also, if you're transmitting binary data over JSON (probably a bad idea), you'll need to manually edit the type to be `byte[]`.

Compile and Run
---

If you're using Eclipse, which you probably are since only masochists attempt running Java without it, then you already know how to hit the green run button.

But if you aren't, perhaps because you're on a headless system, your `Makefile` should look something like this:

    all:
      mkdir -p classes
      javac \
        -sourcepath src \
        -classpath lib/jackson-databind-2.0.1.jar:lib/jackson-databind-2.0.1.jar:lib/jackson-core-2.0.1.jar:. \
        src/com/acme/datatypes/UserTest.java \
        -d classes

    test: all
      java \
        -cp classes:lib/jackson-core-2.0.1.jar:lib/jackson-databind-2.0.1.jar:lib/jackson-annotations-2.0.1.jar \
        com.acme.datatypes.UserTest

    clean:
      rm -rf classes

WARNING: If you copy / paste the above you will need to convert spaces to tabs.

And then you build like so

    make
    make test

More Details
===

Jackson supports a lot of cool features, to learn more check out the documentation on the github page (a bit easier to read): <https://github.com/FasterXML/jackson-databind>
