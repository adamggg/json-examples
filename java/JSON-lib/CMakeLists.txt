
project(example Java)
cmake_minimum_required(VERSION 2.8)

set(CMAKE_Java_LINK_EXECUTABLE "mv CMakeFiles/example.dir/example.class ./")

include_directories(lib/commons-beanutils-1.8.3.jar)
include_directories(lib/commons-collections-3.2.1.jar)
include_directories(lib/commons-lang-2.6.jar)
include_directories(lib/commons-logging-1.1.1.jar)
include_directories(lib/ezmorph-1.0.6.jar)
include_directories(lib/json-lib-2.4-jdk15.jar)

include_directories(lib/gson-2.1.jar)

add_executable(example example.java)
