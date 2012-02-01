cmake_minimum_required(VERSION 2.8)
project(example)

add_executable(example example.c)

target_link_libraries(example curl)
target_link_libraries(example json)
