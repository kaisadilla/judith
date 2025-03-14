#pragma once

#include <stdint.h>

/*
 * These types guarantee the fastest native type for each data type.
 * Useful to define the size needed for each variable without having to
 * consider their actual speed for the targeted architechture.
 */

typedef int_fast8_t sbyte;
typedef int_fast16_t i16;
typedef int_fast32_t i32;
typedef int_fast64_t i64;
typedef intmax_t i_max;

typedef uint_fast8_t byte;
typedef uint_fast16_t ui16;
typedef uint_fast32_t ui32;
typedef uint_fast64_t ui64;
typedef uintmax_t ui_max;

typedef float f32;
typedef double f64;
typedef long double f128;

/*
 * These types guarantee that each data type is exactly the size it indicates.
 * Useful to minimize storage.
 */

typedef char sbyte_f;
typedef int16_t i16_f;
typedef int32_t i32_f;
typedef int64_t i64_f;

typedef uint8_t byte_f;
typedef uint16_t ui16_f;
typedef uint32_t ui32_f;
typedef uint64_t ui64_f;

struct Version {
    i32 major;
    i32 minor;
    i32 patch;
    i32 build;
};