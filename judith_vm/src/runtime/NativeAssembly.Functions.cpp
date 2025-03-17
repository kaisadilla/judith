#include "runtime/NativeAssembly.functions.hpp"
#include "VM.hpp"
#include <iostream>
#include "runtime/object/Object.hpp"
#include "runtime/object/StringObject.hpp"

void NativeFunctions::errorFunc (VM& vm) {}

void NativeFunctions::print (VM& vm) {
    Value ptr = vm.popValue();

    if (ptr.asObjectPtr->objectType == ObjectType::UTF8_STRING) {
        std::cout << ptr.asStringPtr->string;
    }
    else {
        std::cout << "<unknown value>";
    }
}

void NativeFunctions::println (VM& vm) {
    print(vm);
    std::cout << std::endl;
}

void NativeFunctions::readln (VM& vm) {
    std::string utf8input;
    std::getline(std::cin, utf8input);

    vm.pushValue(Value{
        .asStringPtr = new StringObject(utf8input),
    });
}
