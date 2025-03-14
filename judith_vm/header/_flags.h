#pragma once

/// <summary>
/// Prints to console the contents of the stack after executing every instruction.
/// </summary>
//#define DEBUG_PRINT_STACK

/// <summary>
/// Prints to console each time a jasm function is called.
/// </summary>
//#define DEBUG_PRINT_CALL_STACK

/// <summary>
/// The VM will verify that the pointer to the stack is pointing to an index
/// at or above 0.
/// </summary>
#define DEBUG_CHECK_STACK_UNDERFLOW

/// <summary>
/// The VM will check that the IP is pointing to an address inside the chunk
/// being executed.
/// </summary>
#define DEBUG_CHECK_IP_UNDERFLOW

/// <summary>
/// Calls to jasm functions will check that the function has been loaded.
/// </summary>
#define DEBUG_CHECK_FUNC_REF_LOADED