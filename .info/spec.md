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

## Module
Modules organize top-level items into separate regions. All top-level items in Judith belong to a module. If no module is defined in a file, then items declared in that file will be included in the global module. Libraries are not allowed to include anything in the global module.

Each file may only contain one module declaration, and that declaration must precede any other item in the file (excluding import nodes, which must appear at the top of the file).

When a file is included in a module, that module is implicitly imported into the file.
```judith
module awesome_game -- all items in this file will be inside this module.
```

Modules can contain other modules. In this case, the module and all of its ancestors are imported into the file
```judith
module awesome_game::physics -- this file will have access to awesome_game, too.
```

Members of a module can also be accessed explicitly, using their fully qualified name:
```judith
const game = awesome_game::Game!()
```

Explicitly accessing members like this may be necessary to resolve name conflicts:
```judith
import awesome_game

typedef struct Game end

const game = Game!() -- error, as Game here is referring to the Game struct we
                     -- just defined, which doesn't have a constructor.
const game = awesome_game::Game!() -- Fine, it's referring to the correct type.
```

The global module can be accessed with the name `global`:
```judith
module awesome_game
const global_game = global::Game {} -- This refers to the `Game` struct we
                                    -- just defined in the global module.
```

## Namespace
Namespaces are similar to modules, but they cannot be imported explicitly. They can be thought of as static classes in other languages when they are used as method containers (i.e. they do not have state):
```judith
namespace Math
    symbol PI = 3.1415

    func pow (a, b: Num)
        var r = a
        for i in 1..b do
            r *= a
        end

        return r
    end
end
```

You can access members of a namespace like this:
```judith
    const tau = Math::PI * 2
    const tau_squared = Math::pow(tau, 2)
```

# Symbol
Symbols are aliases for literals. As such, symbols do not have any associated type, and at compile time any use of them is directly replaced by the literal they represent
```judith
symbol PI = 3.1415
```

Now, we have a symbol named "PI" that the compiler will understand as `3.1415` whenever encountered.
```judith
const radius = 5.5
const circumference = 2 * PI * radius -- equivalent to 2 * 3.1415 * radius.
```

Symbols are not macros. Any expression that can be evaluated at compile-time can be used as a symbol, and it's evaluated value will be used as the meaning of said symbol:
```judith
symbol SIZE = 5 + 3
const size = SIZE * 2 -- this is evaluated to 8 * 2 (= 16), not 5 + 3 * 2 (= 11).
```

## Enumerate
Enumerate is a feature used to generate integer symbols automatically. These symbols will be part of the scope in which the enumerate is defined:
```judith
enumerate
    STATUS_CONTINUE = 100 -- you can specify where enumerate starts counting
    STATUS_SWITCHING_PROTOCOLS -- 101
    STATUS_PROCESSING
    STATUS_EARLY_HINTS
    STATUS_OK = 200 -- you can jump to higher numbers
    STATUS_CREATED -- 201
    STATUS_ACCEPTED
    STATUS_NON_AUTHORITATIVE_INFORMATION
    STATUS_NO_CONSENT
end
```

These symbols would now be in the global scope:
```judith
const status: Num = STATUS_OK -- equals 200.
```

You can encapsulate the symbols defined by enumerate into its own scope, by naming the enumerate:
```judith
enumerate Direction
    NORTH
    EAST
    SOUTH
    WEST
end

const direction: Num = Direction::WEST
```

Note that enumerates are not types, they are simply syntactic sugar to declare multiple symbols that form a succession of integers, without having to explicit assign each of them a value. The above enumerate would compile to:
```judith
namespace Direction
    symbol NORTH = 0
    symbol EAST = 1
    symbol SOUTH = 2
    symbol WEST = 3
end
```

# Locals and types
## Mutability
Locals in Judith are divided in constant (`const`) or variable (`var`), depending on whether they can be mutated or not. Constants in Judith cannot be mutated in any way, meaning that not only they cannot be reassigned, but also methods that mutate the value they hold can't be used either.
```judith
const name = "Kevin" -- constant local, cannot be mutated.
name = "Steve" -- ERROR: cannot assign to 'name' after initialization.

var name = "Alyce" -- variable local, can be mutated.
name = "Kevin" -- ok.
```

Every local in Judith has a type, and only values of that type can be assigned to it
```judith
const name: String = "Kevin" -- local of type String.
name = 3 -- ERROR: Num cannot be assigned to local of type String.
```

Even though locals always have a type in Judith, that type may be inferred from context:
```judith
var score = 12 -- "score" is of type Num, as inferred from its initialization.
var score = "Alyce" -- ERROR: String cannot be assigned to local of type Num.
```

## Nullability
Types are not nullable by default, meaning that the value `null` cannot be assigned to them:
```judith
const score: Num = null -- ERROR: 'score' can't be null.
var person: Person = null -- ERROR: 'person' can't be null.
```

Types can be made nullable with the `?` symbol:
```judith
const score: Num? = null -- ok.
var person: Person? = null -- ok.
var name = null -- ERROR: Can't infer the type of 'name' from context.
```

Nullable types cannot be used without explicitly handling the possibility of null:
```judith
const person: Person? = null
Console::log(person.name) -- ERROR: 'null' doesn't have field 'name'.
```

Judith has two features that make working with nullable types easy: null-conditional operators and type narrowing:
### Null-conditional operators:
Null-conditional operators are a variant of access operators that allow safe access to nullable values, returning `null` if that access is not possible:
Console::log(person?.name) -- valid, will print either the name, or `null`.

See [Operations § Access operations § Null-conditional operations](#op-access-nullable) for a full explanation of these operators.

### Type narrowing:
Type narrowing in Judith allows locals to be promoted to non-nullable versions of themselves:
```judith
if person !== null then
    Console::log(person.name) -- valid, as 'person' inside this scope is not null.
end
```

See [Types § Type narrowing](#types-narrowing) for a full explanation of Type narrowing.

# Primitives
Primitives are the basic types of Judith. These types are not defined in the language itself, but implemented by the compiler and the VM.

## Basic primitives
### Bool
Either `true` or `false`.
```judith
const is_enabled: Bool = true
```

### Num
By default, a 64-bit IEEE 754 floating point number.
```judith
const score: Num = 36.8
```

For a full explanation of number literals, see [Literals § Numbers](#literals-numbers).

### String
A string of characters.
```judith
const name: String = "Iulius"
```

For a full explanation of string literals, see [Literals § Strings](#literals-strings).

### Char
A string containing exactly one character.
```judith
const separator: Char = ","
```

For a full explanation of the type `Char`, see [Appendix § Char](#appendix-char).

### Regex
A regular expression:
```judith
const regex: Regex = /[0-9]{9}/g
```

For a full explanation of the type `Regex`, see [Appendix § Regex](#appendix-regex).

## Numeric types
When the format of a numeric value is relevant, developers can use specific numeric types.

### Signed integers:
* `I8`: 1-byte signed integer. Suffix: `i8`.
* `I16`: 2-byte signed integer. Suffix: `i16`.
* `I32`: 4-byte signed integer. Suffix: `i32`.
* `I64`: 8-byte signed integer. Suffix: `i64`.

### Unsigned integers:
* `Ui8`: 1-byte unsigned integer. Suffix: `u8`.
* `Ui16`: 2-byte unsigned integer. Suffix: `u16`.
* `Ui32`: 4-byte unsigned integer. Suffix: `u32`.
* `Ui64`: 8-byte unsigned integer. Suffix: `u64`.

### Floating-point values:
* `F32`: 4-byte floating-point value. Suffix: `f32`.
* `F64`: 8-byte floating-point value. Suffix: `f64`

### Decimal
A base-10 value suited for values that cannot afford to lose precision in calculation. Suffix: `m`.

### BigInt
An arbitrarily large integer. Judith considers it as a signed integer of infinite size. Suffix: `ib`.

### Aliased numeric types
* `Byte`: An alias for the size of a byte, by default `Ui8`.
* `Int`: An alias for an int of the native size in the platform, usually `I64`.
* `Float`: An alias for a float of the native size in the platform, usually `F64`.
* `Num`: The "basic primitive" for a number is actually just an alias for `F64`.

## Pseudotypes
Judith features some pseudo-types. These types represent concepts relating to types that aren't directly types and, as such, their usage varies:

* `Void`: Used to represent the absence of a type where a type has to be referenced. For example, the signature of a function that doesn't return any value needs `Void` to indicate its return type: `(Int, Int) => Void`.
* `Unknown`: Denotes the type of a value whose type is not known. A value of type `Unknown` does not allow any operation on it, other than operations that are always available regarding of type (such as testing its type with `is`, or the `str()` method). `Unknown` can be used normally as the type of a local.
* `Never`: Denotes a type that cannot exist. This type can appear when narrowing down a type until no type is left. For example, after exhausting all possible types of a union type, in the next test the value will be of type `Never`, as it can never reach that test. `Never` cannot be used as a type anywhere.
* `Null`: A type whose only possible value is `null`.
* `<error-type>`: This type appears when something that is not a type is used as a type. In general, developers will see this type when trying to reference types that don't exist.

# Literals
## <a name="literals-numbers"></a> Numbers
Number literals are a chain of numbers:
```judith
1512 -- normal number.
```

Underscores can be used to make numbers more readable. Underscores don't have any meaning and are ignored during compilation. Underscores cannot appear at the start of a number (as it would turn it into an identifier) or following a decimal point or another underscore:
```judith
1_512 -- same as '1512'.
```

Numbers can be expressed as decimal, hexadecimal, binary and octal:
* `1512`: 1512 in decimal, base-10. No prefix needed.
* `0x5e8`: 1512 in hexadecimal, base-16. Prefix `0x`.
* `0b0101_1110_1000`: 1512 in binary, base-2. Prefix `0b`.
* `0o2750`: 1512 in octal, base-8. Prefix `0o`.

Decimals can be expressed with a decimal point:
```judith
5.53 -- Number with a decimal point.
.315 -- Numbers can be led with a decimal point.
661. -- ERROR: Numbers cannot end in a decimal point, as it would become the
     -- member access token.
```

Numbers can be expressed in scientific notation.
```judith
8.21e73
```

By default, a number literal will be of type `I64`, unless it contains a decimal point or is expressed in scientific notation, in which case it will be of type `F64`. While number types cannot be implicitly converted between each other, numeric literals will be reinterpreted into their intended usage for as long as that conversion makes sense:

```judith
const score: Int = 42 -- Here "42" is interpreted as an `Int` (i.e. `I64`).
const score: Num = 42 -- Here "42" is interpreted as a `Num` (i.e. `F64`).
const score: Byte = 42 -- Here "42" is interpreted as a `Byte` (i.e. `Ui8`).
const score: Ui64 = -5 -- ERROR: "-5" cannot be interpreted as a `Ui64`.
```

In practice, this means that developers don't need to worry about the implicit type of a numeric literal as the compiler will take care of that job.

While numeric literals themselves will be reinterpreted as needed, values of a numeric type cannot be implicitly converted (e.g. once score of type `Num` is defined, you cannot assign the value of `score` to a local of type `Int`). For a full explanation of number conversion, see [Type casting § Number casting](#typecasting-numbers).

## <a name="literals-strings"></a> Strings
String literals can be defined with either double quotes or backticks:
```judith
var valid_string: String = "This is valid."
valid_string = `This is also valid.`
```

By convention, double quotes are preferred, while backticks are used for strings that contain double quotes.

String interpolation can be achieved with the flag `f` and `{}`:
```judith
const score_str = f"Your score is {player.score}!"
```

Escaped strings (verbatim) can be achieved with the flag `e`. In verbatim strings, the backslash (`/`) doesn't escape characters. In this case, escape sequences are interpreted literally, and the only escaped characters are `"` (represented by `""`) and, in the case of interpolated strings, `{` and `}` (represented by `{{` and `}}`).
```judith
const path: String = e"C:\repos\judith\my_proj\main.jud"
```

You can escape line breaks inside strings with `\`. This will ignore the line break and all the spaces in the next line before the first non-space character:
```judith
const not_broken: String = "This string is \
                            not broken." -- Equals "This string is not broken."
```

### Raw strings
Raw strings are delimited by, at least, three double quotes / backticks on each end. The content inside these quotes must be in new lines, and that content must be indented one level deeper than the closing quotes. Indentation inside the raw string is not part of the string.
Raw strings don't need to escape double quotes / backticks that are part of the string, but have no way to represent more of these characters than the ones that form the delimiter (i.e. a raw string delimited by `"""` cannot contain `"""` inside, as there's no way to escpae that sequence).
```judith
const json_content: String = """
    {
        "type": "Text",
        "id": 6
    }
""" -- <-- Because the closing quotes are indented at level 0, the content must
    --     be indented at level 1.
```

# Operations
## Arithmetic operations
These operations are defined for numbers and return other numbers of the same type. These operators can be overloaded for other types.

* `5 + 2`: Addition.
* `5 - 2`: Substraction.
* `5 * 2`: Multiplication.
* `5 / 2`: Division.
* `5 %i 2`: Integer division.
* `5 %m 2`: Modulo: returns 5 mod 2.
* `5 %r 2`: Remainder: returns the remainder of 5 / 2, corresponds to % in C.

## Bitwise operations
These operations are defined for numbers and return other numbers of the same type. These operators can be overloaded for other types.

* `~2`: Bitwise not (2s complement).
* `5 & 2`: Bitwise and.
* `5 | 2`: Bitwise or.
* `5 ^ 2`: Bitwise xor.
* `5 << 2`: Left shift.
* `5 >> 2`: Right shift.
* `5 >>> 2`: Zero-fill right shift.

## Logical operations
These operations work in terms of "truthy" and "falsey" values. `false`, `0`, `null` and `undefined` are considered falsey, while everything else is considered truthy. This means that empty strings, arrays and objects are considered truthy (as they are still objects, why would they be considered falsey?).

* `not a`: Boolean not: returns `true` if the value is falsey, and `false` otherwise.
* `a and b`: Boolean and: Returns the first value if it's falsey, and the second value otherwise.
* `a or b`: Boolean or: Returns the first value if it's truthy, and the second value otherwise.

Like in most languages, `and` and `or` short-circuit and won't evaluate the right-hand side expression at all when the left-hand side expression is returned.

## Boolean operations
These operations evaluate to either `true` or `false`. Most of these operators can be overloaded for other types, but their return type must remain `Bool`.

* `a == b`: Value equals, can only be used when the type supports value comparison.
* `a != b`: Value not equals, the inverse of `==`.
* `a ~~ b`: Approximate, can only be used when the type implements this operation.
* `a !~ b`: Not approximate, the inverse of `~~`.
* `a === b`: Reference equals. Only allowed for reference types. This operator cannot be overloaded. Returns true when the two sides are the same object in memory.
* `a !== b`: Reference not equals, the inverse of `===`.
* `a > b`: Greater than.
* `a >= b`: Greater than or equal to.
* `a < b`: Less than.
* `a <= b`: Less than or equal to.
* `a < b < c`: Chained comparison: `b` is greater than `a` and less than `c`.

## Null-coalescing operator
The null-coalescing operator returns the value at the left if it isn't `null` or `undefined`, and the value at the right otherwise. Just like logical `and` and `or`, this operation short-circuits, meaning that the right-hand side expression won't be evaluated if the left-hand side expression is returned.
```judith
const designated_boss: Person? = null

const boss = designated_boss ?? get_new_boss() -- Will evaluate to "get_new_boss()"

var score: Num? = null
score ??= 0 -- Since "score" is null, it'll be assigned 0.
score ??= -10 -- Nothing will happen, as "score" is not null.
json_obj?.[fld_name] ?? "unknown value" -- If json_obj?.[inquired_value] doesn't
                                        -- exist, that'll evaluate to "undefined"
                                        -- and the null-coalescing operation
                                        -- will evaluate to "unknown value".

20 ?? Console::log("Unknown") -- Nothing will be logged, as this expression returns
                              -- 20 and discards the right-hand side expression.
```

## Compound assignment operations
Most binary operators in Judith support compound assignment:
```judith
i += 3 -- equivalent to i = i + 3
i *= 5 -- equivalent to i = i * 5
i %i= 9 -- equivalent to i = i %i 9
i &= 0b1100_0111 -- equivalent to i = i & 0b1100_0111
i <<= 2 -- equivalent to i = i << 2
i ??= 10 -- equivalent to i = i ?? 10
i and= "some string" -- equivalent to i = i and "some string"
```

## Access operations
Access operations are used to access a member of a value of any type:

### Member access operator: `.`
This operation accesses an instance member on an instance value, such as an instance field inside a struct:
```judith
const p = Person { name = "Kevin", age = 40 }
person.age -- accesses field "age" inside "p".
```

### Scope resolution operator: `::`
This operation accesses the member of a type, module, namespace or other construct.
```judith
Camera::main -- Accesses static member field "main" that belongs to class "Camera".
game::physics::RigidBody -- Accesses "physics" module inside "game" module, and
                         -- the class "RigidBody" inside it.
```

### Indexing operator: `[]`
This operator can be implemented by types that allow accessing values via indexing. The amount of arguments taken by this operator and the type it returns are defined by the object itself:
```judith
const people: List<String> = ["John", "Kevin", "Tali"]
people[1] - returns "Kevin", because List<_T> defines [i: Int] => _T.
```

### Unwrap operator: `->`
This operator can be implemented by types that wrap a value, to give raw access to that value:
```judith
const boxed_num = Box<Num>!(5)
boxed_num->str() -- accesses Num.str()
boxed_num.str() -- accesses Box<Num>.str()
```

### Call operator: `()`
Calls a callable value, generally a function.
```judith
init() -- calls the function "init".
(() => Console::log("e"))() -- calls the lambda function defined at the left.

const init: (c: Num) => Void = null
const crash: (c: Num) => Void = c => System::exit()
(init ?? crash)(12) -- dynamically calls whatever this expression resolves to.
```

### Runtime member access operator: `.[]`
See [Reflection](#reflection).

### <a name="op-access-nullable">Null-conditional operations</a>
Each of the access operators have a null-conditional counterpart (except for the scope resolution operator). The null-conditional version of an operators will return the value being accessed if that value is `null` or `undefined`. Otherwise, it will access the value as normal. These operators are `?.`, `?[]`, `?->`, `?()` and `?.[]`.
```judith
person?.name -- if "person" is null or undefined, returns that. Else returns the
             -- value of its "name" field.

-- Null-conditional index operator (?[])
my_list?[5] -- if "my_list" is null or undefined, returns that. Else returns the
            -- value at index 5.

-- Null-conditional unwrapper operator (?->)
boxed_val?->name -- If "boxed_val" is null or undefined, returns that. Else
                 -- returns the value of its wrapped value's "name" field.

-- Null-conditional call operator (?())
person?.print_name?() -- if "print_name" is null or undefined, returns that. Else
                      -- calls the function as normal.

-- Null-conditional dynamic index operator (?.[])
json_obj?.[inquired_value] -- if "json_obj" is null or undefined, returns that.
                           -- Else returns the dynamic indexed member (see: Reflection).
```

Note that `undefined` cannot be assigned to a value, so any expression that returns `undefined` will be transformed into `null` when assigned.

# TODO

TODO: Operators § Null-conditional operators
TODO: Types § Type narrowing
TODO: <a name="typecasting-numbers">Type casting § Number casting</a>
TODO: <a name="appendix-char">Appendix § Char</a>
TODO: <a name="appendix-regex">Appendix § Regex</a>
TODO: <a name="reflection">Reflection</a>



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