all:
	mkdir -p classes
	javac \
		-sourcepath src \
		-classpath lib/jackson-annotations-2.0.1.jar:lib/jackson-databind-2.0.1.jar:lib/jackson-core-2.0.1.jar:. \
		src/com/acme/datatypes/UserTest.java \
		-d classes

test: all
	java \
		-cp classes:lib/jackson-core-2.0.1.jar:lib/jackson-databind-2.0.1.jar:lib/jackson-annotations-2.0.1.jar \
		com.acme.datatypes.UserTest

clean:
	rm -rf classes
