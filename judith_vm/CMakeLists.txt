﻿# CMakeList.txt : CMake project for judith_vm, include source and define
# project specific logic here.
#
cmake_minimum_required (VERSION 3.8)

set(APP_NAME "juvm")

# Enable Hot Reload for MSVC compilers if supported.
if (POLICY CMP0141)
  cmake_policy(SET CMP0141 NEW)
  set(CMAKE_MSVC_DEBUG_INFORMATION_FORMAT "$<IF:$<AND:$<C_COMPILER_ID:MSVC>,$<CXX_COMPILER_ID:MSVC>>,$<$<CONFIG:Debug,RelWithDebInfo>:EditAndContinue>,$<$<CONFIG:Debug,RelWithDebInfo>:ProgramDatabase>>")
endif()

project(${APP_NAME})

# Set C++ standard
set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Get all .cpp files from src/
file(GLOB_RECURSE SRC_FILES src/*.cpp)

# Get all .h files from header/
file(GLOB_RECURSE HEADER_FILES header/*.hpp)

# Add include directory
include_directories(header)

add_subdirectory("lib/unordered_dense")

# Define the executable
add_executable(
	${APP_NAME}
	${SRC_FILES}
	${HEADER_FILES}
	"header/root.hpp" "header/types.hpp"
	"header/VM.hpp" "header/runtime/Chunk.hpp"
	"src/VM.cpp" "src/runtime/Chunk.cpp" "src/runtime/Value.cpp"
  "src/utils.cpp" "src/utils/Buffer.cpp" "src/diagnostics/disassembly.cpp" "header/_flags.h" "src/runtime/Assembly.cpp"  "src/runtime/FuncRef.cpp" "src/data/StringTable.cpp" "src/data/AssemblyFile.cpp" "src/runtime/JasmFunction.cpp" "src/runtime/NativeAssembly.cpp" "src/runtime/NativeAssembly.Functions.cpp")

target_link_libraries(${APP_NAME} PRIVATE unordered_dense)

if (CMAKE_VERSION VERSION_GREATER 3.12)
  set_property(TARGET ${APP_NAME} PROPERTY CXX_STANDARD 20)
endif()

# copy the contents of the "res" file directly into the binary folder.
add_custom_command(
    TARGET ${APP_NAME} POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_directory
        "${CMAKE_CURRENT_SOURCE_DIR}/res"
        "${CMAKE_CURRENT_BINARY_DIR}/res"
)

# TODO: Add tests and install targets if needed.
