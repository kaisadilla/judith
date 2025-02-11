# CMakeList.txt : CMake project for judith_vm, include source and define
# project specific logic here.
#
cmake_minimum_required (VERSION 3.8)

set(APP_NAME "judith_vm")

# Enable Hot Reload for MSVC compilers if supported.
if (POLICY CMP0141)
  cmake_policy(SET CMP0141 NEW)
  set(CMAKE_MSVC_DEBUG_INFORMATION_FORMAT "$<IF:$<AND:$<C_COMPILER_ID:MSVC>,$<CXX_COMPILER_ID:MSVC>>,$<$<CONFIG:Debug,RelWithDebInfo>:EditAndContinue>,$<$<CONFIG:Debug,RelWithDebInfo>:ProgramDatabase>>")
endif()

project ("judith_vm")

# Set C++ standard
set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Get all .cpp files from src/
file(GLOB_RECURSE SRC_FILES src/*.cpp)

# Get all .h files from header/
file(GLOB_RECURSE HEADER_FILES header/*.hpp)

# Add include directory
include_directories(header)

# Define the executable
add_executable(
	judith_vm
	${SRC_FILES}
	${HEADER_FILES}
	"header/root.hpp" "header/types.hpp"
	"header/VM.hpp" "header/Chunk.hpp"
	"src/VM.cpp" "src/Chunk.cpp" "src/Value.cpp"
 "src/ChunkReader.cpp" "src/utils.cpp" "src/utils/Buffer.cpp" "src/debug/chunk.cpp")

if (CMAKE_VERSION VERSION_GREATER 3.12)
  set_property(TARGET judith_vm PROPERTY CXX_STANDARD 20)
endif()

# copy the contents of the "res" file directly into the binary folder.
add_custom_command(
    TARGET ${APP_NAME} POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_directory
        "${CMAKE_CURRENT_SOURCE_DIR}/res"
        "${CMAKE_CURRENT_BINARY_DIR}/res"
)

# TODO: Add tests and install targets if needed.
