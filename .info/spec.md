# Comments
## Single-line comments
Single-line comments are introduced with `--`:
```judith
-- this is a comment
```

## Multi-line comments
Multi-line comments are introduced with `--[[` and ended with `]]--`:
```judith
--[[
    This is a
    multiline comment
]]--
```

## Documentation comments (JuDoc)
JuDoc comments are introduced with `---` and follow roughly the same syntax as JavaDoc and JSDoc:
```judith
--- Adds two numbers and returns the result.
--- @param a - The first number to add.
--- @param b - The second number to add.
--- @returns The two numbers added together.
func add (a, b: Num) : Num
    return a + b
end
```

# Modularization
Modules organize top-level items into separate regions. All top-level items in Judith belong to a module. If no module is defined in a file, then items declared in that file will be included in the global module. Libraries are not allowed to include anything in the global module.

Each file may only contain one module declaration, and that declaration must precede any other item in the file (excluding import nodes, which must appear at the top of the file).

When a file is included in a module, that module is  implicitly imported into the file.
```judith
module awesome_game -- all items in this file will be inside this module.
```


---
---
---
```judith
```
```judith
```
```judith
```
```judith
```
```judith
```