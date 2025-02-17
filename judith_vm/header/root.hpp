#pragma once

#include "_flags.h"
#include "types.hpp"

#include <iostream>
#include <string>
#include <vector>
#include <list>

//#define u_ptr std::unique_ptr
#define make_u std::make_unique
template<typename T> using u_ptr = std::unique_ptr<T>;