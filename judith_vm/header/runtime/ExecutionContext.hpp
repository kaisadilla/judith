#pragma once

#include "root.hpp"
#include <filesystem>

namespace fs = std::filesystem;

class ExecutionContext {
public:
    /// <summary>
    /// The directory where the program being executed is in.
    /// </summary>
    fs::path appDirectory;
    /// <summary>
    /// The directory the program is being run at.
    /// </summary>
    fs::path executionDirectory;

public:
    ExecutionContext (fs::path appDirectory) : appDirectory(appDirectory) {}
};