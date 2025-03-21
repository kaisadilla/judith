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
func add (a, b: Num) -> Num
    return a + b
end
```

# Modularization

## Modules
Modules group top-level items. All top-level items in Judith belong to a module. Modules can be nested (e.g. `module game` and `module game::physics`).

Judith projects always have a root module, which can be a nested one (e.g. `some_dev::json`). By default, the root module matches the name of the project's folder, using `.` to divide modules (e.g. the project `some_dev.json/` would have `some_dev::json` as its root module).

Modules cannot be split between packages. If `some_dev.json` defines the modules `some_dev::json` and `some_dev::json::bson`; then a project using said library cannot define these two modules, but it can define other modules such as `some_dev::json::extra`. While extremely unlikely, if two dependencies define the same module, then at least one of them must have its root module aliased, so the modules in both of them can be referenced unambiguously.

Root module names `judith` and `std`, along with all of their nested modules, are reserved and should not be used by anyone. The name `global` cannot be used as the name of a module.

Each file may only contain one module declaration, and that declaration must precede any other item in the file (excluding import nodes, which must appear at the top of the file). By default, the folder structure of the project must match the module structure of the source files (e.g. the file `my_game/physics/RigidBody.jud` must belong to the module `module my_game::physics`), although this can be changed in the project's settings.

On top of all modules lies the global module. The global module has no name, although it can be explicitly referenced with the `global` name. (e.g. `global::Num` or `global::some_dev::json`). The global module contains only native Judith features such as primitive types.

Scripts (a single file, rather than a project) do not belong to any module, and instead use the global module.

## Imports
By default, the module a file belongs to, as well as all of its parent modules are imported into the file.

```judith
module awesome_game::physics -- this file will have access to awesome_game, too.
```

`import` is used to add additional modules to the list of available modules in the file. `import` only imports the specified module into the file, not any of its parent or child modules.

```judith
import std::collections
```

Members of a module can also be accessed explicitly, using their fully qualified name:
```judith
const game = new awesome_game::Game()
```

Explicitly accessing members like this may be necessary to resolve name conflicts:
```judith
import awesome_game

typedef struct Game end

const game = new Game() -- error, as Game here is referring to the Game struct we
                        -- just defined, which doesn't have a constructor.
const game = new awesome_game::Game() -- Fine, it's referring to the correct type.
```

## Name resolution
When referencing a name that doesn't exist in the local scope, Judith tries to resolve it in the following places:

1. If the scope is contained within another scope (e.g. a function inside another function), then the outer scope, recursively.
1. The module the file belongs to.
2. The module's parent module. This is done recursively, until the global module (inclusive) is reached.
3. All of the imported modules. No imported module has any precedence over any other, so if more than one imported module contains the name, then the reference is ambiguous and, thus, invalid.

## Namespaces
Namespaces are similar to modules, but they cannot be imported explicitly. They can be thought of as static classes in other languages when they are used as method containers (i.e. they do not have state):
```judith
namespace Math
    symbol PI = 3.1415

    func pow (a, b: Num) -> Num
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

```judith
Console::log(person?.name) -- valid, will print either the name, or `null`.
```

_See [Operations § Access operations § Null-conditional operations](#op-access-nullable) for a full explanation of these operators._

### Type narrowing:
Type narrowing in Judith allows locals to be promoted to non-nullable versions of themselves:
```judith
if person !== null then
    Console::log(person.name) -- valid, as 'person' inside this scope is not null.
end
```

_See [Types § Type narrowing](#types-narrowing) for a full explanation of Type narrowing._

## Const type
Just like nullable types, const types are a version of the type that is always immutable. As such, const types can only be assigned to constant locals and fields:

```judith
var name: const String = "Kevin" -- ERROR: a variable can't be 'const String'.
const name: const String = "Kevin" -- ok.
```

Const types allow passing references to members of a constant value without breaking immutability.

```judith
const player: Player = { username = "XxXx", person: { ... } }
var person = player.person -- ERROR: this would allow mutating 'person', which is
                           -- a member of constant 'player'.
const person = player.person -- Valid. Because 'player' is constant, the type of
                             -- its member 'person' becomes 'const Person'.
```

# <a name="primitives">Primitives</a>
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

_For a full explanation of number literals, see [Literals § Numbers]_(#literals-numbers).

### String
A string of characters.

```judith
const name: String = "Iulius"
```

_For a full explanation of string literals, see [Literals § Strings]_(#literals-strings).

### Char
A string containing exactly one character.

```judith
const separator: Char = ","
```

_For a full explanation of the type `Char`, see [Appendix § Char](#appendix-char)._

### Regex
A regular expression:

```judith
const regex: Regex = /[0-9]{9}/g
```

_For a full explanation of the type `Regex`, see [Appendix § Regex](#appendix-regex)._

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
* `Int`: An alias for an int of the native size in the platform, usually `I64`. Suffix: `i` (can be omitted).
* `Float`: An alias for a float of the native size in the platform, usually `F64`. Suffix: `f` (can be omitted).

### Num
`Num` is a special type. It represents an `F64` and gets compiled to just that, but it isn't an alias. Instead, `Num` has its own set of rules around number conversion that allows it to be used in certain places where a more refined number type is expected.

## Pseudotypes
Judith features some pseudo-types. These types represent concepts relating to types that aren't directly types and, as such, their usage varies:

* `Void`: Used to represent the absence of a type where a type has to be referenced. For example, the signature of a function that doesn't return any value needs `Void` to indicate its return type: `(Int, Int) => Void`.
* `Unknown`: Denotes the type of a value whose type is not known. A value of type `Unknown` does not allow any operation on it, other than operations that are always available regarding of type (such as testing its type with `is`, or the `str()` method). `Unknown` can be used normally as the type of a local.
* `Never`: Denotes a type that cannot exist. This type can appear when narrowing down a type until no type is left. For example, after exhausting all possible types of a union type, in the next test the value will be of type `Never`, as it can never reach that test. `Never` cannot be used as the type of a value.
* `Auto`: Denotes a type that is inferred from context in places where a type cannot be omitted. This is used for a few syntactic features where the decision to infer type is opt-in rather than opt-out (notably return types in functions).
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

While numeric literals themselves will be reinterpreted as needed, values of a numeric type cannot always be implicitly converted.

_For a full explanation of number conversion, see [Type casting § Number conversion](#casting-numbers)._

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
* `5 %i 2`: Floor division.
* `5 %m 2`: Modulo: returns 5 mod 2.
* `5 %r 2`: Remainder: returns the remainder of 5 / 2, corresponds to % in C.

By default, arithmetic operations are unchecked in Judith, and may overflow or underflow. The default debug configuration makes them checked and makes them panic when overflows or underflows occur.

Inside a checked context (using `checked()`), arithmetic operations will be checked and return exceptions when overflow or underflows occur (_see [Exceptions](#exceptions)_).

By default, arithmetic operations are checked and will throw NumberOverflowException when an underflow or overflow would occur. If done inside an `unchecked` context (using `unchecked()` expression or `unchecked` block modifier), then the underflow or overflow will be allowed to occur.

```judith
const a: Int = Int::MAX + 1 -- throws NumberOverflowException.
const a: Int = unchecked(Int::MAX + 1) -- equals -2,147,483,648.
```

## Bitwise operations
These operations are defined for numbers and return other numbers of the same type. These operators can be overloaded for other types.

* `~2`: Bitwise not (2s complement).
* `5 & 2`: Bitwise and.
* `5 | 2`: Bitwise or.
* `5 ^ 2`: Bitwise xor.
* `5 << 2`: Left shift.
* `5 >> 2`: Right shift.
* `5 >>> 2`: Zero-fill right shift.

## <a name="operations-logical">Logical operations</a>
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
const boxed_num = new Box<Num>(5)
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

### Dynamic access operator: `.[]`
_See [Reflection § Dynamic access operator](#reflection-dao)._

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

# Control structures
## If
```judith
if element == 'Water' then
    -- statements
elsif element == 'Earth' then
    -- statements
else
    -- statements
end
```

The body of an `if`, `elsif` or `else` block can be an arrow body:
```judith
if element == 'Water' => my_water_func()
elsif element == 'Earth' => my_earth_func()
else => my_other_func()
```

Arrow and block bodies cannot be combined.

## Match
Match is a powerful structure that can be used for pattern matching. Match is exhaustive, meaning that every possibility must be explicitly handled for a match expression to be correct.
```judith
match country do
    'Germany' then -- when country equals 'Germany'.
        -- statements
    end
    'France' then
        -- statements
    end
    'Italy', 'Spain', 'Sweden', 'Greece' then -- when country equals any of these
                                              -- four values
        -- statements
    end
    else -- any other value not explicitly handled above.
        -- statements
    end
```

Match can also match types:
```judith
match animal do
    is Dog then -- When 'animal' is of type 'Dog'.
        animal.bark() -- Here, 'animal' is narrowed to 'Dog'.
    end
    is Cat then
        animal.meow() -- Here, 'animal' is narrowed to 'Cat'.
    end
    is Rabbit, is Squirrel, is Rat then -- When animal is any of these types.
        animal.jump() -- Here, 'animal' is narrowed to 'Rabbit | Squirrel | Rat'.
                      -- We can use 'jump' because all 3 types have that method.
    end
    else end -- An empty else block, for when we don't wanna do anything with
             -- values that aren't explicitly handled.
```

Match can match expressions:
```judith
match score do
    < 5 => Console::log("Terrible score.")
    -- If the number is < 5, it will match the previous statement and won't reach
    -- this part:
    < 10 => Console::log("Decent score!")
    else => Console::log("AWESOME SCORE!!)
```

Match can match destructured objects:
```judith
const values = [3, 0]

match score do
    [0, y] => Console::log("first value is zero and the second is " + y)
    [x, 0] => Console::log("second value is zero and the first is " + y)
    else => Console::log("Contains no zeroes!")
end
```

Match clauses can have guards:
```judith
const values = [3, 0]

match score do
    [0, y] => Console::log("first value is zero and the second is " + y)
    [x, 0] => Console::log("second value is zero and the first is " + y)
    else => Console::log("Contains no zeroes!")
end
```

Match clauses can have `when` guards:
```judith
match num do
    n when n > 0 => Console::log("Congratulations on having points.")
    n when n < 0 => Console::log(f"How did you manage to get {n}?")
    else => Console::log("Zero points exactly")
end
```

## Do
Do is a simple block that executes one. While `do` itself doesn't add anything to a program, it can be used to define blocks with modifiers such as `unsafe` or `unchecked`. Like other control structures, `do` is an expression and can yield a result.

```judith
do
    -- statements
end
```

## Loop
Defines an infinite loop, can only be stopped with `break`.

```judith
loop
    -- statements
end
```

## While

```judith
while i < 32 do
    -- statements
end
```

## Foreach
Foreach loops execute once for each item in a collection. The part before "in" is an initialization, not an assignment, it will always initialize a new local even if you omit "const".
```judith
for item in array do
    -- statements
end
```

### Ranges
Ranges are a special syntax designed to work well with foreach loops. Ranges are lists that contain a set of numbers.
```judith
const range = Range(0, 10) -- 'start' is inclusive, 'end' is exclusive.
```

Ranges have a custom syntax built into the language:
```judith
const range = 0..10 -- same as Range(0, 10)
```

Range can have a step:
```judith
const range = Range(9, -1, -1) -- third argument is the step (-1).
const range = 9..-1 step -1 -- same as above.
```

Loop through a range form A to B:
```judith
for i in 0..20 do
    -- statements
end
```

Loop through 20, 18, 16, 14, 12, 10, 8, 6, 4, 2:
```judith
for i in 20..0 step -2 do
    -- statements
end
```

## Arrow bodies
All structures can use arrow bodies anywhere where a block body is valid.

## Control structures as expressions
Control structures in Judith are expressions, not statements. This means that they evaluate to a value that can be then assigned. This is determined by the `yield` keyword, which works in the same way as `return` does inside a function. Just like functions, control structures may not `yield` any value, in which case they evaluate to the type `Void` which cannot be assigned.
```judith
const msg = if score > 10 then
    yield "Congratulations! You beat the game!"
else
    yield "Sorry, try again."
end
```

When using arrow bodies, the expression that makes up the body is considered the yield value:
```judith
const msg = if score > 10 => "Congratulations! You beat the game!"
    else => "Sorry, try again."
```
## Statement structures
Judith features a few control structures that are statements rather than expressions:

## When
`when` is the short-form version of `if`.
```judith
return when result === null
```
When statements only execute when the condition given is met. Unlike `if`, When  statements can't have alternates.

When statements are statements, not expressions, and as such they don't evaluate to anything nor they can be used in places where an expression is expected.

## Jumptable
Jumptables are the equivalent to C's `switch`.
```judith
jumptable name do -- the jumptable "do" forms a single, shared scope.
    case "Christian": -- each case is the entry point for a given value
    case "Jennifer": -- this means that a new entry point doesn't stop the
                     -- previous value from executing.
    case "Alice":
        Console::log("You are fired.") -- "Christian", "Jennifer" and "Alice"
                                       -- will make it here.
        break -- break exits the block as normal, so the next Console::log won't
              -- be reached by these three.
    case "Kevin":
        Console::log("You are promoted and...")
        -- there's nothing here, so "Kevin" will jump into "Sylvia"'s section
    case "Sylvia":
        Console::log("Your work day has been reduced!") -- Both "Kevin" and
                                                        -- "Sylvia" make it here.
        goto case "Alice" -- "Kevin" and "Sylvia" jump back to "Alice"'s section
                     -- and get fired anyway :(
end
```

Jumptables are not named `switch` to discourage developers coming from languages like C from using it over `match`. Jumptables are a niche feature that is seldom needed to express logic concisely.

# Static variables
Static variables are variables that exist outside functions, shared globally by all parts of the program. Static variables are declared with the `static`.

```judith
module game;

static const player_name = "Kevin"
static var max_score = 0
```

# Functions
Functions are defined by the keyword `func`:

```judith
func get_value_plus_10 (const value: Num) -> Num
    return value + 10
end
```

Functions may return a value, or no value at all. In this case, their return type is `Void`.

```judith
func hello_world () -> Void
    Console::log("Hello world")
end
```

Functions can return `const` values. These values cannot be assigned to mutable locals or fields:

```judith
func get_immutable_person (id: String) -> const Person
    -- statements
    return person
end

var p = get_immutable_person("kevin") -- ERROR: 'const String' cannot be assigned
                                      -- to a variable.
const p: const Person = get_immutable_person("kevin") -- ok.
```

If a function's return type is ommited, it is assumed to be `Void`.

```judith
func add (a, b: Num)
    return a + b -- ERROR: 'add' doesn't return a value.
end
```

To infer the return type of a function, an explicit 'Auto' must be used.

```judith
func add (a, b: Num) -> Auto -- inferred to be 'Num'
    return a + b
end
```

## Function overloading
Judith does not allow function overloading.

## Variadic functions
Variadic functions can take any number of arguments. To define a variadic function, its last parameter has to use the spread operator, and the type of said parameter has to be a type that can be initialized as a list (i.e. any type that can be initialized with [a, b, c] syntax).

```judith
func add_many_numbers (...nums: List<Num>) -> Num
    var sum = 0
    for n in nums do sum += n end
    return sum
end
```

You can call a variadic function by passing each value as a parameter:

```judith
const sum = add_many_numbers(3, 5, 2, 4, -7, 9, 4, 11)
```

Or you can ignore that it's a variadic function and pass it a collection compatible with its variadic parameter, as if it was a normal parameter (in this case, List<Num>):


```judith
const my_list: List<Num> = [1, -2, 5, 6.6, 0.4, 2]
const sum = add_many_numbers(my_list)
```

Functions can omit the `const` keyword in their parameters, but not their type annotation. Functions can also omit the return type annotation:

```judith
func add (a, b: Num) -- 'a' and 'b' are implicitly 'const'.
    return a + b -- add(a, b) infers that it returns 'Num' from here.
end
```

## Constructor parameters
Function can have constructor parameters. A constructor parameter expands to any valid set of parameters that can construct an object:

```judith
func build_person (person_ctor: new Person)
    const p = new Person(person_ctor)
    -- statements
end
```

Named constructors can also be used:

```judith
func build_person (person_ctor: new Person::make_kevin)
    const kevin = new Person::make_kevin(person_ctor)
    -- statements
end
```

## In parameters
`in` parameters take ownership of the reference they are given, meaning that once a reference is passed as an in parameter, the local holding that value (if any) cannot be used anymore. This allows functions to guarantee that a reference they get will not be accessed or mutated from the outside afterwards.

```judith
func set_world (in var world: World)
    -- statements
end
```

The `in` keyword must be used on the call expression, too, to make it clear that whatever is being passed as an argument cannot be accessed anymore

```judith
var world = new World("test", world_settings)
set_world(in world)
world.start() -- ERROR local 'world' is no longer accessible.
```

## Return type `Never`
The return type of a function can be `Never`. This means that the function never returns (because it always panics, contains an infinite loop, etc). `return` statements are explicitly forbidden inside `Never` and, if the function has a path that can return, it will result in a compile error.

```judith
func start () -> Never
    loop {
        -- statements
    }
end
```

## Generators
Generators are a special type of function that uses `yield` statements to return values, one by one, when iterated

```judith
generator get_single_digit_numbers () -- return type: IEnumerable<Num>
    for i in 0..10 do
        yield i
    end
end

const gen: IEnumerable<Num> = get_single_digit_numbers()
const a = gen.next() -- equals 0
const b = gen.next() -- equals 1

for i in gen do -- equals 2, then 3, 4, 5, 6, 7, 8, 9
    -- statements
end
```

## First-class functions
Functions in Judith are first-class, which means they can be treated like any other value (assigned to locals and fields, passed as parameters, etc.).

A function's type is defined by its signature:

```judith
func add (a, b: Num) -> Num
    return a + b
end

func multiply (a, b: Num) -> Num
    return a + b
end

func negate (a: Num) -> Num
    return -a
end
```

These types are expressed like `(param types) => return type`

```judith
var binary_op: (Num, Num) => Num = add -- valid assignment
binary_op = multiply -- valid, as multiple matches the signature used as type.
binary_op = negate -- ERROR - negate's signature is '(Num) => Num'.
```

You can call a value of a function type as if it was a function:

```judith
binary_op(5, 8) -- Returns '40', as "binary_op" points to "multiply()".
```

## Lambda functions
Functions can be defined with lambda syntax. Functions defined as lambda syntax are expressions and evaluate to the function itself.

```judith
const add = (a: Num, b: Num) -> Num
    return a + b
end
```

In this example, we can see every feature of a lambda function, although most of them can be omitted. The type annotation in the parameters are only necessary when the type of said parameters cannot be inferred from context. Return type can also be omitted - unlike regular function syntax, return type `Void` is not implied, but instead the return type is inferred from usage. Finally, the body of the function can be replaced with an arrow expression. In this case, the value of the expression will become the return type of the lambda function.

With all of these features, this is the shortest way to write a lambda function:

```judith
const add: (Num, Num) => Num = (a, b) => a + b
```

## Function composition

### Function chaining
Functions can be combined with the `.and_then` method. This method returns a new function that calls the function followed by a new function with the same signature:

```judith
func increase_by_2 (x: Num) -> Num return x + 2 end
func multiply_by_3 (x: Num) -> Num return x * 3 end

const increase_and_multiply = increase_by_2.and_then(multiply_by_3)

Console::log(increase_and_multiply(10)) -- prints 36 (result of (10 + 2) * 3).
```

### Partial resolution
Functions can be partially resolved by providing it some of its parameters, but not others. The result of partially calling a function is a new function that requires the missing parameters:

```judith
func add (x: Num, y: Num) -> Num return x + y end

const add_2 = add(?, 2) -- add_2's type is '(Num) => Num'.

Console::log(add_2(5)) -- prints 7.
```

This can be combined with function chaining:

```judith
const increase_and_multiply = add_2(?, 2).and_then(mult(?, 3))
Console::log(increase_and_multiply(10)) -- prints 36 (same as above).
```

Under the hood, partial call syntax is just syntactic sugar for lambda functions. This:

```judith
const add_2 = add(?, 2)
```

is equivalent to this:

```judith
const add_2 = (x: Int) => add(x, 2)
```

# Local shadowing
Judith allows local fields to be shadowed:

```judith
var score: Num = 6
score = 8
const score: String = score.str() -- starting here, "score" will refer to this
                                  -- new String local.

score = 8 ERROR: Score is constant and its type is 'String'.
```

This special case

```judith
var score: Num = 6
score = 8
const score = score
```
Can be simplified to:

```judith
var score: Num = 6
score = 8
const score -- skipping initialization assumes it's initialized with the value
            -- of the local that it's being shadowed.
```

Shadowing a constant with a variable is allowed, but results in a compiler warning.

# Arrays and collections
## List
Lists are the most basic collection in Judith. They are a dynamically sized collection of values of a given type.

```judith
const scores: List<Num> = [3, 5, 9, 15]

-- List indices start at 0:
scores[0] -- Returns 3.
scores[4] -- Returns 'undefined', as only indices 0 through 3 exist in the list.
scores[^1] -- Returns the first value from the end, that is the value at
           -- index 3: 15.
scores[1..3] -- Returns the values in the range given, that is values at 1 and
             -- 2: [5, 9].
scores[1..^1] -- Same as 1..3
```

## Array
While `List<_T>` is recommended for most cases, the type `Array<_T, _count>` provides a fixed-size array, equivalent to C# arrays or C++ `std::array<T, c>`.

```judith
const scores: Array<Int, 4> = [3, 5, 9, 15]
```

## Array types
_See [User-defined types § Array type](#types-array)._

## Dictionary
Dictionaries are the basic collection of key-value pairs.

```judith
const scores: Dictionary<String, Num> = [
    "Kevin" = 7,
    "Ryan" = 5,
    "Steve" = 11,
    "Alice" = 8,
]
```

Dictionaries are accessed by key:
```judith
scores["Ryan"] -- returns 5.
scores["Regina"] -- returns 'undefined'.
```

## Collection initializer expression
The collection initializer expression `[ ... ]` is used to initialize any type that implements the necessary indexers:

### List initializer expression
A collection initializer expression behaves as a list when it contains values separated by commas (e.g. `[5, 7, 9, 11]`). In this case, this expression translates to a nameless constructor that takes a List<_T> as its only parameter.

```judith
const some_type: MyType<int> = [5, 7, 9]
```

Is equivalent to

```judith
const some_type = new SomeType<int>([5, 7, 9])
```

### Dictionary initializer expression
If the collection initializer expression contains key-value pairs (defined as `key_expr = value_expr`), then this expression is translated to repeated calls to the `set [T]` operator.

```judith
const my_dict = ["Kevin" = 7, "Ryan" = 5]
```

Is equivalent to

```judith
const my_dict = new Dictionary<String, Num>()
my_dict["Kevin"] = 7
my_dict["Ryan"] = 5
```

## Collections, enumerables, iterators and more
TODO

# Casting and narrowing
## Type casting
Type casting in Judith is restricted to operations that change types between compatible types. There's various types of casting:

### Upcasting (`:`)
Upcasting allows a value of a subtype B to be casted to a supertype A. As B is guaranteed to be also of type A, this cast is performed at compile time at no cost.

```judith
const dog = new Dog()
const animal: Animal = dog:Animal -- valid cast, as 'Dog' is always an 'Animal'.
```

### Downcasting (`:?`, `:!`)
Downcasting allows a value of a supertype A to be casted to a subtype B. Since a value of type A is not guaranteed to be also of type B, this cast is performed at runtime. The way invalid casts are handled depends on the operator used:

#### Safe downcasting operator (`:?`)
With the safe downcasting operator, the cast will return `null` when it fails, avoiding errors. However, due to this, this expression evaluates to a nullable type, even when used on a non-nullable one:

```judith
const animal = new Cat()
const dog: Dog? = animal:?Dog -- will be null, as animal's type is 'Cat'.
```

#### Unsafe downcasting operator (`:!`)
With the unsafe downcasting operator, the cast will panic when it fails. This expression preserves the nullability of the original type:

```judith
const dog: Dog = animal:!Dog -- will panic, as animal's type is 'Cat'.
```

### <a name="casting-numbers">Number conversion</a>
Some number formats can be implicitly converted into others, while others require explicit casting. The rule of thumb is that a number can only be promoted (that is, casted into a wider format):

* Numbers can be implicitly converted into a bigger size of the same kind of number (e.g. `I16` into `I32`, `Ui8` into `Ui64` or `F32` into `F64`). Signed integers can be converted into `BigInt`.
* Signed and unsigned integers can be converted into floats (e.g. `I32` into `F32` or `Ui16` into `F64`). This conversion can actually be lossy, but that's considered a loss of precision rather than a meaningless conversion.
* Alias types (`Byte`, `Int` and `Float`) follow the rules of the type they are aliasing.
* It is not possible to implicitly convert a `Decimal` into any other format, or vice versa.

When a number cannot be implicitly converted into another format, a explicit casting can be used. This type of casting uses the same operators as upcasting and downcasting (`:`, `:?`, `:!`).

Number casting always occurs at runtime, but each casting operator offers the same guarantees as it does with other types.

```judith
const float: Float = 13
const integer: Int? = num:?Int -- cast fails if float is too big for Int.
const float2: Float = integer:Float -- Errors cannot occur, every Int can be a Float.
```

Keep in mind that, in the example above, it is possible for an integer type casted to a floating-point type to lose precision, which is considered acceptable and not an error when transforming an integer into a float.

When casting is done in an unchecked context, every numeric cast uses `:`, as every cast in this context is a valid cast, even if the results make no mathematical sense.

When casting is done in a checked context, then the need to use `:` or `:?` / `:!` is determined by the following rules. In case two rules contradict each other, downcasting operators prevail over upcasting ones:

| Cast                                               | Operator     |
|------                                              |--------------|
| Smaller size to bigger size*                       | `:`          |
| Bigger* size to smaller size                       | `:?` or `:!` |
| Signed integer to unsigned integer, and vice versa | `:?` or `:!` |
| Integer to float                                   | `:`          |
| Integer to decimal                                 | `:`          |
| Float to integer                                   | `:?` or `:!` |
| Float to decimal                                   | `:`          |
| Decimal to integer                                 | `:?` or `:!` |
| Decimal to float                                   | `:`          |

\* BigInt is considered an integer of infinite size, so it's always bigger than any other integer type.

Keep in mind that most casts done with `:` in a checked context are ones that could be done with an implicit conversion, so these cast will be redundant.

#### Num

`Num` follows a special set of rules:
* `Num` can always be implicitly transformed into any other type, and any type can be transformed into `Num`.

### Null-forgiving casting
Nullable types can be casted into their non-nullable counterparts by appending the null-forgiving operator `!` at their end. Casting a `null` value in this way will panic.

```judith
const person: Person? = null
const person_2: Person = person! -- will panic, as 'person' is null.

Console::log(person!.name) -- will panic.
```

Unsafe casting operations should only be used when their failure justifies crashing the whole program.

## <a name="types-narrowing">Type narrowing</a> WIP
Narrowing is the process of refining a broader type into a more specific one based on control flow. While traveling through a possible execution branch of a block of code, when a certain property of a value's type is asserted, that property remains true for the remainder of that branch.

Type narrowing works by implicitly creating new variables of a more refined type (only when needed). This means that assertions made by type narrowing are always safe.

### Type checking (`is`)
With the `is` keyword, a value can be asserted to be (or not to be) of a given type:

```judith
type Animal = Dog | Cat | Rabbit | Mouse
const animal = get_animal()

if animal is Dog
    animal.bark() -- 'animal' is of type 'Dog' here.
end
```

Inside the `if` scope, the local `animal` of type `Animal` is asserted to be of type `Dog`, so it gets promoted to such type, which means calling `Dog.bark` becomes valid.


```judith
if animal is not Dog
    animal:?Dog?.bark() -- ERROR: 'animal' is of type Cat | Rabbit | Mouse here.
end
```

Similarly, inside this `if` scope, we have asserted that `animal` is not of type `Dog`. While we don't know its exact type yet, we know it's not a `Dog` type and, as such, it gets promoted to a union type of the remaining possibilities. `Dog` is a subtype of `Animal`, but not of this union type (that explicitly excludes it), so `animal` inside this scope cannot be downcasted to `Dog`.

### Member checking (`has`)
With the `has` keyword, a value can be asserted to have (or not to have) access to a given member. In the following example, `Animal` is defined as `Dog | Cat | Rabbit | Kangaroo | Mouse | Tiger`. Among these types, only `Rabbit` and `Kangaroo` contain the `jump` member method:

```judith
if animal has "jump"
    animal.jump(3) -- 'animal' here is 'Rabbit | Kangaroo', as they are the only
                   -- possible types that have a "jump" member.
end
```

### Null checking (`=== null`, `!== null`)
Checking for `null` values also contributes to type narrowing:

```judith
const animal: Animal? = get_animal()
return when animal === null

animal.eat() -- 'animal' here is promoted to 'Animal'.
```

### Discriminator member (`a.b == c`, `a.b != c`)
When the value of a member in two different types doesn't admit the same types of values, testing the value contained in that member also helps discard types that wouldn't admit such value:


```judith
typedef Circle kind: "circle", radius: Num end
typedef Square kind: "square", side: Num end

func get_area (shape: Circle | Square)
    if shape.kind == "circle" then
        return Math::PI * shape.radius * shape.radius -- 'shape' is 'Circle'.
    end
end
```

Here, `shape` is inferred to be of type `Circle` inside the `if` scope, as it's the only possible type in `Circle | Square` that can have a `kind` member with the value of `"Circle"`.

Keep in mind that values of an unsealed interface cannot be narrowed down, as the amount of available subtypes is not known at compile time.

When matching types, unreachable paths do not require superfluous code:

```judith
typedef Id = String | Num

func get_person (id: Id) -> Person
    return get_person_by_string(id) when id is String
    return get_person_by_num(id) when id is Num
    
    -- no need to return anything
end
```

In this example, even though there's no "general" return statement, the compiler recognizes that it's impossible for the function not to have exited after the second return statement, so it doesn't require the developer to write a superfluous `return` afterwards.

# Types
Types in Judith are divided between value types and reference types. Value types are allocated on the stack, and locals contain the value itself. Reference types are allocated on the heap, and locals contain pointers to the actual value. The behavior of both kinds of types in Judith is mostly the same, as Judith abstracts away implementation details like memory management. However, they present some differences:
* Only reference types can be compared by reference (`===` and `!==`).

## Primitive types
_For a full explanation of primitive types, see [Primitives]_(#primitives).

* `Bool`: A boolean value: true or false.
* `Num`
* `String`\*
* `Char`
* `Regex`\*
* `I8`, `I16`, `I32`, `I64`, `BigInt`
* `Ui8`, `Ui16`, `Ui32`, `Ui64`
* `F32`, `F64`
* `Decimal`

_\* Type is a reference type._

## Function types
Function types are types that define functions. The type of a function depends on its signature (type of each of its parameter and return type). Function types are expressed with the following syntax: `(<"!" if can return exceptions><parameter types separated by commas>) -> <return type>`. For example:

```judith
func get_person (id: Num, office: Office) -> Person end
func !get_person_2 (id: Num, office: Office) -> Person end

var getter: (Num, Office) -> Person = get_person -- valid
getter = get_person_2 -- invalid, as signatures don't match.

var getter2: !(Num, Office) -> Person = get_person_2 -- valid, because it has "!".
getter2 = get_person -- valid, as "(Num, Office) -> Person" can be assigned
                     -- to "!(Num, Office) -> Person".

```

Values of a function type are _callable_, which mean that they can be used in call expressions: `<callable>(<arglist>)`. Function types are always reference types.

## <a name="types-array">Array type</a>
An array type (not to be confused with the type `Array<_T>`) is a type that defines a group of values of specific types. Array types are similar to tuple types in other languages.

```judith
const info: [String, Num] = ["Kevin", 36]
info[0] -- inferred to be of type "String".
info[1] -- inferred to be of type "Num".
info[2] -- inferred to be "undefined".

const n = get_num()
info[n] -- inferred to be of type "String | Num | undefined".
```

When all of the members of an array share the same type, it can be defined with the following syntax:

```judith
const five_names: String[5] = ["Kevin", "Alyce", "Grant", "Ruby", "Annie"]
```

We may want to initialize many indices to the same value. This can be achieved by using the `;` token and defining the value that will fill the rest:
```judith
const ten_numbers: Num[10] = [3, 5; 9] -- produces [3, 5, 9, 9, 9, 9, 9, 9, 9, 9].
const ten_42s: Num[10] = [; 42] -- produces [42, 42, 42, 42, 42, 42, 42, 42, 42, 42].
```

Note that the length of an array type is always known at compile time, so this syntax cannot be used dynamically:

```judith
const amount = get_amount()
const nums: Num[amount] = [; 42] -- ERROR: array length must be known at compile time.
```

## Object type
TODO: Explain that object types are dictionaries of key-value pairs with constraints. Explain that object types can be used as structural types.

An object type is a struct-like type defined by its member fields:

```judith
const anon = {
    username = "x__the_best__x",
    score = 500_000_000,
} -- type is inferred as "{username: String, score: Num}"

anon.username -- valid, evaluates to a String.
anon.id -- invalid, 'anon' doesn't contain field 'id'.
```

## Literal type
A literal type is just a literal used as a type

```judith
var country: "Germany" = "Germany" -- This value can only equal "Germany"
country = "France" -- error, String is not assignable to "Germany".
```

## Union type
A union type defines a type that is the union of several types. As such, a relationship is established between the Union type and the types it's derived from, where all of the derived types are also the Union type, and the Union type could be any of the derived types.

```judith
var id: Num | String = 36 -- valid, as "Num" is a member of "Num | String".
id = "string_id" -- valid, as "String" is also a "Num | String".
```

We can downcast a union into any of its conforming types:

```judith
const num: Num = id:?Num
```

Unions of string literals will raise a warning, as a set is preferred:

```judith
typedef Countries = "Germany" | "Sweden" | "UK" -- WARNING: define a set instead.
```

Unions with literal `null` will raise a warning, as it would make them inferior versions of a nullable type:

```judith
typedef Animal = Dog | Cat | null -- WARNING: Don't include 'null'.
```

## User-defined types
Developers can define their own types with the `typedef` keyword.

### Alias type
An alias type is a type that maps to another type. Aliases can be implicit or explicit. Any type can be aliased, including unions, array types, function types, etc.

#### Implicit alias
Implicit aliases allow the defined type to be used as if it was the original type.

```judith
typedef UniqueId = Int
const id: UniqueId = 32 -- valid, as UniqueId is Int.
const integer: Int = id -- valid, as UniqueId can be used as an Int.
```

The compiler in this example sees `UniqueId` as `Int`.

#### Explicit alias
Explicit aliases are equivalent to the type they are derived from, but a value whose type is the alias type cannot be used as if it was the original type.

```judith
typedef expl UniqueId = Int
const id: UniqueId = 32 -- valid, as UniqueId is initialized like Int.
const integer: Int = id -- ERROR: cannot assign 'UniqueId' to 'Int'.
const integer: Int = id:Int -- valid, explicit cast is allowed.
```

### Option
An option type is defined as a group of string literals. Literals used in an option are not strings, and so use single quotes rather than double quotes.

```judith
typedef option Country
    'Germany'
    'Sweden'
    'France'
    'Italy'
    'Japan'
    'Other' default -- You can optionally mark one value as default.
end

var country: Country = 'Germany'
country = 'Great Britain' -- invalid, as 'Great Britain' is not part of the option.
```

Option types cannot be used as a `String`, but can be upcasted into one:

```judith
const country_name: String = country:String
```

Each option can include additional data as an array or object type:

```judith
typedef option IpAddress
    'V4'[Byte, Byte, Byte, Byte]
    'V6'[String]
end

const home: IpAddress = 'V4' -- ERROR: Missing additional data.
const home: IpAddress = 'V4'[127, 0, 0, 1] -- correct
const loopback: IpAddress = 'V6'["::1"] -- correct
```

You can combine variants with different kinds of types:

```judith
typedef Message
    'quit'
    'move'{ x: Num, y: Num }
    'write'[String]
    'change_color'[Num, Num, Num]
end
```

This option has four variants with different types:
* `'quit'` contains no data.
* `'move'` contains an object type with fields `x` and `y` of type `Num`.
* `'write'` contains an array with a single `String`.
* `'change_color'` contains an array of three `Num`.

You can use `match` to pattern match an option type:

```judith
match message do
    'quit' then -- Basic variant with no additional data.
        Console::log("Connection ended.")
    end
    'move'(pos) then -- 'Move' object gets assigned to 'pos'.
        move_obj(pos.x, pos.y)
    end
    'write'(txt) then -- 'Write'[0] gets assigned to 'txt'.
        Console::log(txt)
    end
    'change_color'(r, g, b) then -- 'change_color'[0] gets assigned to r,
                                -- [1] to g, [2] to 'b'.
        paint_obj(r, g, b, 1)
    end
end
```

Options are designed to be efficient, compiling to as little as it can. An option set without additional data becomes a single `Int`, while ones with data become an `Int` and a pointer.

Usually the type of an option's variant can be inferred from usage. However, for cases where this isn't true, the variant can be qualified:

```judith
const country = Country::'Germany' -- 'country' is inferred to be of type 'Country'.
```

### Struct
Structs define an object that contains a number of member fields. They are used to define POD (plain old data) types.

```judith
typedef struct Person
    name: String
    age: Num
    country: Country
    salary: Decimal
end
```

Structs are then constructed like objects

```judith
const person = Person {
    name = "Kevin",
    age = 28,
    country = 'Germany',
    salary = 103_000d,
}
```

Structs can have default values and optional keys:

```judith
typedef struct Person
    name: String -- regular, mandatory field.
    country: Country = 'Other' -- optional field that, if not assigned, will
                               -- be initialized to the value 'Other'.
    age?: Num -- optional field. This field may or may not exist in a given
              -- instance. When it doesn't exist, its value is "undefined".
    salary: Decimal? -- regular, mandatory field of nullable type "Decimal?".
end
```

Structs implicitly define a constructor that contains all of its fields in order:

```judith
const p = new Person("Kevin", 28, 'Germany', 103_000d)
```

Structs can extend other structs:

```judith
typedef struct Employee
    Person -- Including the name of another struct (without defining a member)
           -- means Employee will have all of the fields of that struct.
    role: CompanyRole
    antiquity: Num
    salary: Num -- this name clashes with Person.salary, and it doesn't even
                -- have the same type. In this case, both "salary" members exist.
end

const employee: Employee = -- init
employee.country -- Valid access, "country" is a member of Employee (via Person).
employee.salary -- Type: Num, this is the "salary" defined by Employee.
employee:Person.salary -- We can still cast employee to person to access
                       -- Person.salary (type: Decimal).
```

### Interface
If structs hold data, interfaces define behavior. Objects cannot be of an interface type, but instead interfaces are implemented into types to give them new functionalities.

**Important note**: Judith interfaces follow the orphan rule, meaning that you can only implement an interface for a type if your project defines either the interface or the type. This, combined with the lack of circular references between dependencies, guarantees that only a single implementation of a specific interface for a specific type can exist in a given project. You can work around the orphan rule by defining an alias for the type you don't own.

```judith
-- By convention, interfaces are prefixed with the letter "I".
typedef interface ISummarizable
    -- Abstract methods: methods declared by the interface that must be
    -- implemented by the type.
    func summary () -> Void
    
    -- As any other method, these methods may be pure or impure.
    impure func mark_as_read () -> Void

    -- As interfaces cannot have member fields, any member data that wants to
    -- be guaranteed must be exposed through a getter method:
    func is_read () -> Bool

    -- Concrete methods: methods provided by the interface. These methods are
    -- inherited by the types that implement the interface, so there's no need
    -- to cast them back to the interface type to use them (unless they collide
    -- with members of that type).
    func extended_summary () -> Bool
        -- Note that the interface still has access to a "self". This "self" is
        -- the object this method is being called off, casted to the interface
        -- type.
        return .summary() + " (Read more...)"
    end
end
```

Interfaces are implemented like this:

```judith
-- Here we define two structs for two different kinds of objects: newspaper
-- articles and blog posts.
typedef struct Article
    headline: String
    country: Country
    author: Person
    content: String
    is_read: Bool
end

typedef struct Post
    username: String
    content: String
    comments: Num
    is_read: Bool
end

-- Now, we'll implement ISummarizable for both types:
impl ISummarizable for Article
    func summary ()
        -- Here we are implementing ISummarizable methods for instances of
        -- Article specifically, so "self" is of type Article. Thus, .content
        -- refers to Article.content, not ISummarizable.content (which doesn't
        -- exist).
        return .content.substr(100) + "..."
    end

    func get_is_read ()
        return .is_read
    end

    impure func mark_as_read ()
        .is_read = true
    end

    -- Note that extended_summary cannot be implemented, as that's a method
    -- owned and implemented by ISummarizable.
end

impl ISummarizable for Post
    -- ...
end
```

Interfaces are used like this:

```judith
const article: Article = --{}
const post: Post = --{}

Console::log(article.summary()) -- valid, we are calling ISummarizable.summary()
Console::log(post.summary()) -- valid, we are calling ISummarizable.summary()
```

Interface types become the supertype of any type that implements them, and can be casted as such:

```judith
const summarizable: ISummarizable = article -- valid, article is ISummarizable
article:ISummarizable -- upcast at compile time.
summarizable:?Article -- downcast at runtime, fails if summarizable is a Post.
```

If member identifier collisions occur between two interfaces implemented by a specific type (e.g. Post implements two different interfaces that define "summary()"), then that ambiguity is resolved by upcasting the object:

```judith
post:ISummarizable.summary() -- calling ISummarizable.summary()
post:IOther.summary() -- calling IOther.summary()
```

If the collision occurs between the type itself and an interface, then the type itself takes precedence. If Post contained summary() itself then:

```judith
post.summary() -- calling Post.summary()
post:ISummarizable.summary() -- calling ISummarizable.summary()
```

Note that, as interfaces are open-ended by design, it is not possible to exhaustively test an interface type, because nothing guarantees that new subtypes of the interface will be created by other libraries or at runtime.

### Class
Classes are the most complex type in Judith. They represent state machines, whose behavior is encapsulated and controlled by the class. For this purpose, classes feature privacy rules and methods.

#### Fields
Just like structs, classes contain fields. However, class fields behave in a different way:

* By default, a member field is considered **mutable** when used inside the class, but **immutable** when used outside.

```judith
typedef class Person
    name: String
end

var person: Person = get_person()
Console::log(person.name) -- valid
person.name = "Steve" -- ERROR: 'person.name' is immutable.
```

* Class fields can be hidden from the outside with the `hid` keyword. A hidden field cannot be accessed from the outside at all.

```judith
typedef class Person
    -- ...
    hid birth_year: Num?
end

Console::log(person.birth_year) -- ERROR: 'person.birth_year' is not accessible
                                -- from outside the class.
```

* Class fields can be exposed as mutable variables with the keyword `pub`.

```judith
typedef class Person
    -- ...
    pub country: Country
end

person.country = 'Italy' -- valid, 'person.country' is mutable.
```

* Class fields can be marked as "always mutable" with the `var` keyword. A mutable field can be mutated even when the instance is constant. Semantically, always mutable members represent values that are not part of the instance's state.

Note that mutable fields are still exposed as constants to the outside, unless explicitly marked as `pub`.

```judith
typedef class Person
    -- ...
    var times_age_was_read: Num = 0 -- Default initializer.
end
```

* Class fields can be marked as "always immutable" with the `const` keyword. This acts in the opposite way as `var`: a `const` field can never be mutated, not even when the instance of the class is mutable. Semantically, always immutable members represent parts of the instance's state that are not controlled by the class. In the following example, although `company` is part of a `Person`'s state, the person itself cannot change the state of the company.

Note that always immutable reference type fields (`company` in this case) can still mutate, if the instance it points to is also pointed to by a mutable variable somewhere else. This makes semantic sense, because a company can still change over time, and these changes can still affect the person's behavior; even if the person itself cannot mutate the company in any way.

```judith
typedef class Person
    -- ...
    const company: Company
end
```

Note that `const` is not compatible with `pub`, as `pub` wouldn't have any effect.

#### Member methods
Classes can define member methods. Member methods are nearly identical to extension methods, with the exception that, because they are defined inside the class, they interact differently with the class's fields, as explained in the previous section.

_See [Types § Associated items § Extension methods](#types-associated-extmethods) for a full explanation of methods._

```judith
typedef class Person
    -- ...
    func print_name (self)
        Console::log(.name)
    end

    func get_age (self) -> Num
        -- Methods defined inside the class can access hidden members.
        const age = .calc_age()

        -- Mutable fields can be mutated even when 'self' is immutable.
        .times_age_was_read += 1

        return age
    end

    -- An impure member method. Can change "name" as it's a member method and
    -- takes 'self' as a mutable parameter.
    func rename (var self, name: String)
        .name = name
    end
end
```

Member methods can be hidden with the `hid` keyword, just like member fields.

```judith
typedef class Person
    -- ...
    hid func calc_age (self) -> Num
        return Date::now().year() - .birth_year
    end
end

p.calc_age() -- ERROR: 'person.birth_year' is not accessible from outside the class.
```

#### Constructing an instance of a class
Unlike structs, classes don't implement the empty constructor by default. This means that a class must define, at least, one member constructor method.

```judith
    ctor (name: String, birth_year: Num?, country: Country, company: Company)
        .name = name
        .birth_year = birth_year
        .country = country
        .company = company
    end
```

Classes cannot be constructed like objects, but must instead call a constructor to be initialized. However, this constructor can be then accompanied by an object initializer to initialize any fields that haven't been initialized yet.

**Important note:** if a field doesn't have a default value, nor a value assigned to it in a constructor, then that field can be assigned a value from the outside during initialization. This applies even to hidden fields. For example, given this constructor:

```judith
    ctor (name: String)
        .name = name
    end
```

This construction is valid:

```judith
const p = Person::("Kevin") {
    birth_year = 1970, -- valid even though 'birth_year' is hidden.
    country = 'Germany',
    company = get_company(), -- valid even though 'company' is not public.
}
```

This special behavior occurs _only_ during construction. i.e. assigning a value to `p.company` afterwards would be a compile error.

#### Member static variables
Static variables can be defined directly inside a class. They still work in the same away as any other static variable, with the difference that they are exposed as **immutable** to the outside.

```judith
typedef class Person
    -- ...

    static people_created: Num = 0

    ctor ()
        -- ...
        people_created += 1 -- valid assignment.
    end
end

Person::people_created += 1 -- ERROR: 'Person::people_created' is immutable.
Console::log(Person::people_created) -- valid.
```

#### Member functions
When a function defined inside a class doesn't define `self` as its first parameter, it remains a function rather than becoming a method. However, as it's defined inside the class, these functions get to interact with member static variables as mutable:

```judith
    func reset_people_created ()
        ::people_created = 0 -- valid, as it's done from inside the class.
    end
end
```

#### Summary
The class we created above can be used like this:

```judith
const company = Company::get_by_name("Ajax")
var kevin = new Person("Kevin", 1975, 'Germany', company)

Console::log(kevin.name) -- valid, as "name" is visible from the outside.
kevin.name == "George" -- invalid, as "name" is constant to the outside.
kevin.country = 'Italy' -- valid, as "country" is variable to the outside.

kevin.print_name() -- valid call, will print "Kevin"
kevin.calc_age() -- invalid call, 'calc_age' is a hidden member.
kevin.relocate('France') -- valid call, impure functions (that mutate state)
                         -- can be called in variables.
kevin.print_people_created() -- invalid call, "kevin" object doesn't have a member
                             -- with that name, as that method belongs to Person.

const kevin -- redeclare it as a constant
kevin.relocate('France') -- error, relocate is an impure function that would
                         -- mutate this constant.
kevin.print_name() -- still valid, as it's a pure function.

Person::print_people_created() -- valid, as this method belongs to Person.
```

#### Implementing interfaces
Classes can implement interfaces in their own declaration. As always, this means that the implementation of the interface gets privileged access to the class's member fields.

```judith
typedef class Announcement
    organization: Organization
    text: String
    is_read: Bool

    impl ISummarizable
        func summary ()
            return .text.substr(100) + "..."
        end

        func get_is_read ()
            return .is_read
        end

        impure func mark_as_read ()
            .is_read = true
        end
    end

    -- constructor, methods and stuff.
end
```

These types are not special nor restricted in any way. They are used as normal, as if the class they are defined in was their namespace:

```judith
const vehicleType: Vehicle::Type = 'Boat';
const id: Vehicle::Id = {
    registrar: "Kevin",
    license: 42069,
};
```

## Associated items
Top-level items can be associated with a specific type. Defining a top-level item in this way will make it accessible by qualifying it with the Type's name, _as if it was defined inside the type itself_.

To define associated items, the name given to these items must be qualified with the type to which the item is associated. The following example associates a static variable called `empty` to `String`.

```judith
static const String::empty = ""

const name = String::empty -- 'empty' can now be used as if it was part of 'String'.
```

You can define all kinds of top-level items as associated items:

```judith
typedef set Vehicle::Type
    'Car'
    'Boat'
    'Airplane'
    'Train'
end

typedef struct Vehicle
    type: Vehicle::Type -- you can use it inside the type.
end

const vehicle_type: Vehicle::Type = 'Boat'. -- or outside, too.
```

Associated items belong to whichever module they are defined in, not to the type they are associated with, which means that the relevant module has to be imported into the file for the associated item to be available. Also, just like extension methods, type-associated items can still be qualified with their modules:

```judith
module some_mod

typedef set Vehicle::Type
    'Horse'
    'Carriage'
end

-- some other file

const vehicle_type: some_mod::Vehicle::Type = 'Horse'
```

### Extension methods
Extension methods are a special case of associated functions. When an associate function defines `self` as its first parameter, it becomes an extension method. `self` is always of the type the function is associated with, and that type can be omitted when defining the parameter. Whether the `self` parameter is taken as `var` or `const` determines whether the method is pure or impure. The name of the method qualifies it into the type it's being implemented in:

```judith
func Vec2::normal (self) -> Vec2
    const mag = self.magnitude()
    return self / mag
end
```

Inside a method, `.` and `::` can be used without any preceding name. If done so, `self` is implied for `.` and the type's name is implied for `::`. The previous method can thus be implemented as such:

```judith
met func Vec2::normal (self) -> Vec2
    const mag = .magnitude() -- ".magnitude" is equivalent to "self.magnitude".
    return self / mag
end
```

Methods can then be called as if they were members of instances:

```judith
const p = Vec2 { x = 5, y = 3 }
const normalized = p.normal() -- "p" becomes the argument for the "self" parameter.
```

### Extension constructors
Although constructors cannot be top-level items outside classes by themselves (as they need somethign to construct), they can be defined as associated items. A constructor is always a function (never a generator), and always return an instance of the type they are implemented for. Unlike methods, constructors do not take a `self` parameter. Instead, `self` is implicitly created at the start of the constructor, with uninitialized fields. Unlike regular methods, constructors can have no name at all.

```judith
ctor Vec2 (x, y: Num)
    .x = x
    .y = y
end
```

Constructors are called like normal functions (with the small exception that they may have no name at all).

```judith
const point = Vec2::(10, 16)
```

Constructors are allowed to leave certain fields uninitialized, although this will force the caller to complement the function call with a partial initializer constructor.

```judith
ctor Vec2::with_x (x: Num)
    .x = x
    -- notice that we are not initializing "y" here.
end

const point = Vec2::with_x(5) -- ERROR: Incomplete initialization.

const point = Vec2::with_x(5) {
    y = 4,
} -- valid, as we are initializing the fields the constructor left uninitialized.
```

`self` is the return value of a constructor, which means that reassigning `self` is a valid way to construct an object.

```judith
ctor Num::parse (str: String)
    -- logic
    self = result -- this will make "result" the value 'constructed' by this ctor.
end
```

If the type the constructor belongs to can only be instantiated with a constructor method (as it's commonly the case with classes), `self` will not be created implicitly, and thus will be unavailable until a value is assigned to it:

```judith
ctor Person::make_local_kevin (country: Country)
    .country = country -- ERROR: "self" is uninitialized.

    self = Person::("Kevin", 17, country) -- initialize "self" explicitly.

    Console::log(.country) -- valid, as "self" is initialized.
end
```

The same happens when a type cannot be instanced directly.

```judith
typedef Animal = Dog | Cat

ctor Animal::make_adorable ()
    Console::log(self) - ERROR: What is 'self' here???
    
    self = Cat { name = "Macbeth", evil = true }

    Console::log(self) -- valid, as 'self' is now a Cat.
end

const macbeth = Animal::make_adorable() -- "macbeth" is of type "Animal".
```

## Default values
Types in Judith can have default values. The default value of a type can be obtained with the `default` keyword (which can be qualified if needed with the name of the type, e.g. `String::default`).

These are the default values of each type (or lack of):

* **`Bool`**: `false`
* **Numeric types**: `0`
* **`String`**: `""`
* **`Char`**: no default value
* **`Regex`**: no default value
* **Nullable types**: `null`
* **Function types**: no default value
* **Array type**: If all of its members have a default type, then an array where each index contains the default type for that index's type.
* **Object type and struct types**: If all of their fields have a default type, then an object or struct instance where each field is initialized to its default value.
* **Literal type**: itself.
* **Union type**: no default value
* **Alias type**: the default value of the aliased type, if any.
* **Option type**: no default value, unless one option is marked as `default`.
* **Interface type**: no default value.
* **Class type**: if the default constructor is defined, then the result of calling such constructor. Else, no default value.

You can add or override the default value of a type by implementing the `IDefault` interface to it.

## Constructing structs and classes
Every field inside a struct of a class must be explicitly initialized at some point during the construction process. In Judith, these values are never inferred, not even when the type of a field has an available default value.

```judith
typedef struct Car
    make: String
    origin: Country?
    price: Decimal = 20_000m
    color?: Color
end

ctor Car (make: String)
    .make = make
end

const my_car = Car::("Honda") {
    origin = 'Japan',
}
```

In this example, `my_car` is initialized correctly, following this logic:

* `make` was assigned a value (`"Honda"`) in the constructor.
* `origin` was assigned a value (`"Japan"`) in the object initializer.
* `price` has a default value (20,000), and since it hasn't been assigned a value anywhere, that default value becomes the assigned value.
* `color` is an optional field. It has not been assigned anywhere so its value is `undefined`.

If `Car` had an extra field `max_speed: Num`, then this initialization would be invalid, as `max_speed` would not be assigned any value in this construction.

# Operator overloading
Some operators in Judith can be overloaded. This is done with the `oper` keyword, which defines a function that acts as the overloaded operation. Most operators are defined as functions, but a few of them are defined as member methods. Binary operations can be made symmetric with the `#symmetric` directive. When an overloaded operator is marked as symmetric, it will automatically generate an identical overload where the operands are inversed. For example, an overload of `Vec2 + Quaternion` marked as symmetric will create a new function `Quaternion + Vec2` with the exact same body.

## Arithmetic operators
`+`, `-`, `*`, `/`, `%i`, `%m` and `%r`

These operators take two values of any type and return a new value of any type.

```judith
#symmetric
oper + (a: Fraction, b: Num) -> Fraction
    return {
        num: a + (b * den),
        den: den,
    }
end
```

## Unary operators
`-` and `~`.

```judith
oper - (a: Vec3)
    return new Vec3(-a.x, -a.y, -a.z)
end
```

## Value equals and approximate
`==`, `!=`, `~~` and `!~`

These operators take two values of any type and return a `Bool`.

Overloading `==` will implicitly overload `!=` as `not (a == b)`. The same will occur with `~~` and `!~` as `not (a ~~ b)`. Overloading `!=` and `!~` is still allowed, so a more efficient operation can be implemented (or a different behavior, if it makes sense).

```judith
#symmetric
oper == (a, b: Vec3)
    return a.x == b.x and a.y == b.y and a.z == b.z
end
```

## Comparison operators
`<`, `<=`, `>` and `>=`

These operators take two values of any type and return a `Bool`.

Overloading just some of these operators is enough to get the full set. Overloading just `<` will define `<=` as `a < b or a == b`, `>` as `not (a < b or a == b)` and `>=` as `not (a < b)`. Same goes for `>`. In every case, though, more of these operators can be overloaded to refine their behavior.

```judith
#symmetric
oper < (a, b: Vec3)
    return a.mod < b.mod
end
```

## Accessor operators
Operators `[]` and `->` can be overloaded as member methods of a type.

### Indexing operator
This operator can take any number of arguments of any type, and return a value of any type.

```judith
class SomeCollection
    -- ...

    oper [] (a, b: Int) -- can be called as collection[5, 10]
        return .arr[(a * .width) + b]
    end
end
```

### Unwrap operator
This operator doesn't take any argument, and returns a value of any type.

```judith
class NumWrapper
    -- ...

    oper ->
        return .val
    end
end
```

## Operators that can't be overloaded.
Any operator not listed above cannot be overloaded. Among them, these are some of the most notable non-overloadable operators:

* Reference equals: `===` and `!==`.
* Call: `()`.
* Dynamic access operator: `.[]`
* Object constructor: `{}`.
* Logical operators: `and` and `or`.
* Range: `..`.
* Null-coalescing operator: `??`.

# Templates
**`EXPERIMENTAL`**

Templates are Judith's way to implement generics. They are type-safe and only allow operations that can be proven correct.

## Type parameters

To create a template for a top-level item, simply prefix it with the `template` declarator:

```judith
template<_T>
typedef struct NamedValue
    name: String
    value: _T -- "_T" is the type provided by the template.
end
```

You can then use the template struct by providing it with a type:

```judith
const val = NamedValue<Num> { -- here, we specify that "_T" will be "Num".
    name: "Score",
    value: 73 -- here, "value" is of type "Num".
}
```

You can refine the kind of type parameter the template takes:

```judith
template<_T: struct> -- _T has to be a struct
template<_T: class> -- _T has to be a class
template<_T: value> -- _T has to be a value type
template<_T: reference> -- _T has to be a reference type
```

## Template rules

While defining `NamedValue<_T>`, we can only do operations that we know are valid. The following example is invalid, because it makes incorrect assumptions about `_T`:

```judith
func NamedValue<_T>::increase_by (b: _T) -> _T
    return .value += b -- ERROR: type "_T" does not define operator '+'.
end
```

We can constrain our templates (and thus, guarantee that certain operations will exist) by stating template rules with the `rule` keyword:

```judith
template<_T>
rule<_T + _T> -- We establish the rule that the overload _T + _T exists.
typedef struct NamedValue
(...)
```

With this rule, the extension method we defined above (`increase_by`) would become valid, as now we've guaranteed that we'll be able to sum two `_T` together. However, this also means that now `NamedValue` cannot be used with any value that cannot be added together:

```judith
var val: NamedValue<Num> -- valid, as Num + Num exists.
var val: NamedValue<Bool> -- ERROR: Bool + Bool doesn't exist.
```

Other available rules for type parameters are:

`_T` has a method with the given signature:

```judith
rule<_T.some_method(Num, Num) -> Void>
```

`_T` has an associated function with the given signature:

```judith
rule<_T::some_func(String) -> String>
```

`_T` has an associated type:

```judith
rule<_T::Pool>
```

 - This associated type can also have its own rules:

```judith
rule<!_T::Pool.pool_method(_T) -> Void>
```

Operator overload exists:

```judith
rule<_T * Num>
```

`_T` is of a given type:

```judith
rule<_T is String | Num>
```

## Constant parameters
Aside from types, template parameters can also represent compile-time constants. To achieve this, simply specify the type of the constant accepted by that parameter:

```judith
template<_T, _divisor: _T>
rule<_T / _T>
func n_divide (dividend: _T) -> _T
    return dividend / _divisor
end
```

We can now use it with any compile-time constant:

```judith
const res = n_divide<5>(20) -- valid, returns "4".
const res = n_divide<get_num()>(20) -- ERROR: Template parameter must be resolved
                                    -- at compile time.
```

## Constant parameter template rules
You can define rules that apply tests to the constant parameter:

```judith
template<_T, _divisor: _T>
rule<_T / _T>
rule<_divisor != 0> -- cannot use '0' for '_divisor'
func n_divide (dividend: _T) - _T
(...)

const res = n_divide<0>(10) -- ERROR - '0' doesn't satisfy this rule.
```

## Template specialization
Templates can be specialized, which allows certain arguments to behave differently from others. Any specialization must be defined in the same module as the template itself.

```judith
template<_T, _exp: Num>
rule<_T * _T>
rule<_exp >= 0>
func pow (var val: _T) -> _T
    for i in 0..(_exp - 1) do
        val *= _val
    end

    return val
end
```

This function is called as `pow<b>(a)` to do a^b, but has one flaw: when _exp is `0`, it should return `1`, but instead it returns `val`. We can fix this by specializing the template for when `_exp` = `0`.

To define a specialization, you still use `template<>`, but you skip any parameters that will be specialized (even if that means `template<>` is left empty). The specialized parameter appears instead after the name of the item (just as when you use a template item). Keep in mind that template specializations do not inherit the rules of the original template, since we may not need the same constrains.

```judith
template<_T> -- we don't specify "_exp", because that's the value we'll specialize.
-- here we don't define a rule for _T * _T since we don't need it.
rule<_T = 1> -- but we define a rule specifying that we can assign "1" to a type _T.
func pow<0> (val: _T) -> _T
    return 1
end
```

# <a name="exceptions">Exceptions</a>
Judith has a feature called 'exception handling' designed to handle errors. Despite its name, this feature is not related to the C++ / C# / Java exception system.

**Exceptions in Judith are regular values returned by functions**; and the exception handling system is ultimately syntactic sugar to return exception values and handle them ergonomically.

## Exception option type
To start with exceptions, we have to introduce a special option type called `Exception`. This type mostly works like any regular option type, with one important difference: it's an open set, meaning that variants can be added to it from the outside. This way, we can define our own exceptions and introduce them into the set:

```judith
Exception::'file_not_found'
```

As exceptions are variants of an option type, you can tack additional data to them:

```judith
Exception::'index_out_of_bounds'[Int]
Exception::'access_denied'{ user: String, server_message: String }
```

Similar to associated items, exceptions are owned by the module where they are defined, rather than the 'Exception' option type. This means that you can only use options in the modules you import, and that name collisions can be resolved by fully qualifying them.

## Functions with exceptions
When a function can return exceptions, these exceptions become part of the return type of the function.

To make a function able to return exceptions, use the `!` token before it's name:

```judith
func !get_person (index: Int) -> Person
    -- ...
end
```

This `get_person` function is now marked as able to return exceptions. This makes the signature of the function `!(Int) -> Person`, which is not compatible with `(Int) -> Person`. This means that `get_person`'s return type is actually `Person | Exception`.

Before continuing the explanation, we have to introduce one important detail: every single function implicitly defines its own exception option type, which is automatically built from all the possible exceptions that can be returned by that particular function. These option types are special because their variants are just a subset of the variants that exist in `Exception`. This feature is important because it allows Judith to always know which exceptions are possible.

## Returning exceptions
Exceptions are returned just like any other value:

```judith
func !get_person (index: Int) -> Person
    -- Here, we return an exception.
    return Exception::'index_out_of_bounds'[index] when index >= people.count

    return people[index]
end

func !divide (dividend, divisor: Int) -> Int
    return Exception::'divide_by_zero' when divisor == 0

    return dividend / divisor
end
```

In these examples, the return type of `get_person` is `Person | Exception`, where `Exception` is a subset that can only contain `Exception::'index_out_of_bounds'[Int]`. On the other hand, `divide`'s return type is `Int | Exception`, but here `Exception` is a different subset that can only contain `Exception::'divide_by_zero'`.

## Handling functions with exceptions
When calling a function with exceptions, there's multiple ways in which we can handle them.

### Treating exceptions are normal values
If we use a function normally, then we receive a value that is the union of its regular return type and the subset of Exception that the function may return.

It is important to mention that, even though we refer to all subsets with `Exception`, the compiler automatically infers which subset is used by each value.

```judith
func do_something ()
    const res = divide(15, 0) -- "res" type is "Int | Exception".

    if res is Int then -- we have to check the type at runtime.
        set_score(res)
    end

    -- we ignored the exceptions just like it was any normal value.
end
```

### Handling exceptions when received (`catch`)
Another way to handle exceptions is to do something about them when they are received. To do this, append a `catch` block to the faulty expression to handles the exceptional values.

In this first example, we simply log that an error has occurred and end the function early. By doing this, we guarantee that the result of the expression will not contain exceptions.

```judith
func do_something ()
    const res = divide(15, 0) catch ex do
        Console::log("Error occurred")
        return
    end -- here "res" is narrowed to Int.

    set_score(res)
end
```

In this second example, instead of returning on exception, we `yield` a new value as the result of the expression. Just like before, this also guarantees that the result of the expression does not contain exceptions, this time because we overrode the exception value with a regular `Int` value.

```judith
func do_something ()
    const res = divide(15, 0) catch ex do
        Console::log("Error occurred")
        yield 0
    end -- here "res" is narrowed to Int.

    set_score(res)
end
```

In this third example we log the error, but don't deal with the exception. Unlike the previous examples, here we are not "fixing" the exception, so `res` will still potentially contain exceptions.

```judith
func do_something ()
    const res = divide(15, 0) catch ex do
        Console::log("Error occurred")
    end -- here "res" is NOT narrowed, it's still "Int | Exception"
        -- as we didn't deal with it.

    set_score(res) -- ERROR: Cannot use "Int | Exception" as "Int".
end
```

### Passing down exceptions (`try`)
Sometimes we can't deal with an exception, but we don't want to suppress it either. In this case, we want to pass down the exception to the caller. To do this, we use `try` before the faulty expression. When we use `try` and the expression returns an exception, our function automatically returns that exception, too.

```judith
func !do_something ()
    const res = try divide(15, 0) -- here "res" is of type Int.

    set_score(res)
end
```

In this example, we are using `divide()` normally, ignoring its exceptions. The type of the expression is, thus, `Int`, just as if no exceptions could happen. If `divide()` returns an exception, however, `do_something` will immediately return that exception itself. Due to this behavior, `Exception::divide_by_zero` is added to the list of Exception variants `do_something` may return.

All of this means that the above snippet is equivalent to this:
```judith
func !do_something ()
    const res = divide(15, 0)
    return res when res is Exception

    set_score(res)
end
```

### Converting exceptions into null (`try?`)
By using `try?`, we can ignore exceptions altogether by transforming every faulty value into `null`, which allows us to use syntactic features designed for it.

```judith
func do_something ()
    const res = try? divide(15, 0) -- here "res" is of type "Int?".

    set_score(res ?? 0)
```

### Forgiving the exception (`try!`)
We can tell the compiler that a faulty expression won't return an expression with `try!`. If the expresion returns an exception, the program will panic.

```judith
func do_something ()
    const res = try! divide(15, 0) -- here, "res" is of type "Int". If "divide()"
                                   -- returns an exception, the program will panic.

    set_score(res)
end
```

## Exceptions on functions that don't return values (i.e. Void)
When a function that doesn't return any value can return exceptions, then correct (i.e. non-faulty) executions of the function will return `undefined`, which is the idiomatic value that represents the lack of a value.

```judith
const ret_val = do_something() -- "ret_val"'s type "Undefined | Exception".
```

Note that you are not required to assign the result of a function to anything to handle its exceptions:

```judith
do_something() catch ex => Console::log("An error happened.")
```

## Accessing the stack trace
All `Exception` variants contain a field called `stack_trace`, which automatically collects information about the call stack when created.

```judith

do_something() catch ex do
    Console::log(ex.stack_trace)
    panic "do_something() didn't do its thing."
end

```

## Panic
Sometimes errors are unrecoverable. In this case, we can crash the program immediately with a message by using the `panic` statement. The `panic` statement can take an expression and will terminate the program immediately, showing a message first if an expression was given to it.

```judith
func solve_p_np_problem ()
    panic "Not yet implemented."
end
```

In this example, calling `solve_p_np_problem()` will immediatelly cause the program to show a message saying "Not yet implemented." and then exit.

Note that there's no way to recover from a `panic`, as by definition `panic` it used to deal with unrecoverable errors.

# Destructuring and spreading
Destructuring is a feature that allows the developer to unpack values from collections and values that contain members.

Destructuring can be done in two ways: by destructuring content, or by destructuring members:

## Content destructuring (`[a, b...]`)
Content destructuring assigns values contained in an enumerable collection, in whichever order that collection enumerates them. A type is a enumerable collection if it imlpements the IEnumerable interface.

```judith
const countries = ['Japan', 'China', 'South Korea', 'Taiwan']

const [ japan, china ] = countries -- The first two elements of the arrea are
                                   -- assigned to the declared locals.
```

The last local declared can capture all remaining values if expressed with `...`, like this:

```judith
const [ japan, china, ...others ] = countries 
```

Here, the value of others is an array that contains `['South Korea', 'Taiwan']`.

## Member destructuring (`{a, b...}`)
Member destructuring assigns values contained in member fields of a type. Unlike content destructuring, this is resolved at compile time.

```judith
const person = Person {
    name: "Kevin",
    age: 39,
    country: 'Germany'
}

const { name, age } = person -- The members Person.name and Person.age are
                             -- assigned to name and age.
```

Locals created by member destructuring do not need to use the original member's name:

```judith
const { name => person_name } = person
```

It is a compile-time error to destructure a member that doesn't exist.

```judith
const { name, city } = person -- ERROR: Person.city doesn't exist
const { name } = 2 -- ERROR: Num.name doesn't exist
```

When destructuring a member method, the method remains bound to the instance they were destructured from

```judith
const { get_nth_birthday } = person -- ok.
get_nth_birthday(80) -- calls person.get_nth_birthday
```

Just like with content destructuring, you can capture all remaining members with a `...` local. In this case, the remaining local will be an object type.

```judith
const { name, ...rest } = person -- 'rest' contains { age = 39, country = 'Germany' }.
```

## Spread
Spreading works similarly to destructuring, and allows developers to spread the contents of a collection or members of a value into a place that expects multiple values. Whether a spread is a content spread or a member spread depends on the context in which they are used:

```judith
const europe = ['Germany', 'France', 'Italy']
const eurasia = [...europe, 'China', 'Japan', 'Thailand']
```

In the example above, `europe` is being spread in a place that expects values. As such, a content spread occurs and the 3 members in the `europe` array become the 3 first members of the `eurasia` array.

```judith
const person = Person { name = "Kevin", age = 32, email = "kevin@gmail.com" }
const contact_info = { email = "kevin@kevin.kev", phone = "555-31-21-11" }

const full_person = {
    name = "someone",
    ...person, -- person.name replaces the declared name field
    ...contact_info, -- contact_info.email replaces person.email
    address = "Hermann-Hesse-Straße 1"
}
```

When composing a new object by destructuring others, each new initialization takes precedence over all previous initializations. In this example, even though we assigned a value to `name` in the first line, spreaing person's fields (`...person`) overrides this initialization with the value of `person.name`. Then, `...contact.info` overrides the initialization of `email` provided by `person.email`.

The spread operator is useful to create shallow copies of collections and objects, and constness is enforced here.

```judith
const europe = ['Germany', 'France', 'Italy']
var europe_copy = [...europe] -- valid, as everything copied is a value type.

const people = [new Person(...), new Person(...)]
var people_copy = [...people] -- ERROR - cannot assign const reference type to var.
```

# Reflection
Judith supports reflection natively when compiled to certain targets (notably JASM). Although Judith is statically-typed and doesn't use source names in regular execution, these names are still stored as metadata, which allows for their resolution at runtime.

Let's assume this definition:

```judith
typedef struct Person
    name: String
    age: Num
end
```

## <a name="reflection-dao">Dynamic access operator (`.[]`)</a>
The dynamic index operator `.[]` can be used on any object to try to retrieve the value of the given member.

```judith
const p = new Person("Kevin", 36) -- the struct type defined above.
p.["age"] -- access field "age" via reflection at runtime, returns '36'.
```

When resolving a member that doesn't exist, a special value is returned: `undefined`. `undefined` is not `null`, but instead a sentinel value that indicates that whatever is being accessed doesn't exist (in other words, it's not defined).

```judith
person.["favorite_dish"] -- returns "undefined", since that field doesn't exist.
```

Member methods can also be accessed via reflection:

```judith
person.["get_birthday"]?() -- has to be called with '?()' as it may be undefined.
```

Extension methods, however, cannot be found via reflection, as they are not part of an object's definition.

```judith
impl func Person::print_name () end
person.["print_name"] -- "undefined", as "print_name" is not part of Person.
```

The dynamic access operator is resolved at runtime, so any expression is valid:

```judith
const field = "age"
person.[field] -- returns 36, which is the value of 'person.age'
```

However, since non-`Dynamic` objects cannot have non-`String` members, using a non-`String` expression in a dynamic access in anything other than a `Dynamic` object will result in a compile-time warning, as the value is guaranteed to be `undefined`.


The return value of this operation is `unknown | undefined`, as it is not possible to determine at compile time if the value is defined or not, nor the type of the value if it would be defined.

```judith
const val = person.[field]
const collective_years_of_experience: Num = 164

if val is Num then
    collective_years_of_experience += val -- valid, because 'val' has been narrowed
                                          -- down to 'Num'.
else
    val *= 2 -- ERROR - 'val' here is 'unknown | undefined'.
end
```

Note that dynamically accessing a value of type unknown is a valid operation:

```judith
if val !== undefined then
    val.["month"] -- may or may not work, depending on the type "val" belongs to.
end
```

You cannot, however, dynamically access `undefined`, as `undefined` cannot have members:

```judith
val.["month"] - ERROR: cannot dynamically access "undefined".
```

You can, however, use the null-conditional version of the dynamic access operator to safely chain accesses:

```judith
val.["person"]?.["age"] -- if val.["person"] returns "undefined" or "null", then
                        -- ?.["age"] won't be called, and the result will be
                        -- "undefined" or "null" (depending on the value of
                        -- val.["person"]).
```

## Querying information about an object
Judith allows querying an object for information about its types and composition at runtime. Let's define a type to query information about it:

```judith
typedef class Person
    name: String
    last_name: String
    age: Num
    country: Country

    static count: Num

    func print_age ()
        -- statements
    end

    static func print_count ()
        -- statements
    end
end
```

Use `membersof()` to retrieve a list of all members of the given instance or type:

```judith
const p = new Person()

membersof(p) -- returns ["name", "last_name", "age", "country", "print_age"]
membersof(Person) -- returns ["count", "print_count"]
```

Usually, this expression is not that useful, since it mixes methods with fields, which you don't usually want to treat in the same way. To refine your query, you can use `fieldsof()` and `methodsof()` to retrieve fields and methods separately:

```judith
fieldsof(p) -- returns ["name", "last_name", "age", "country"]
fieldsof(Person) -- returns ["count"]
methodsof(p) -- returns ["print_age"]
methodsof(Person) -- returns ["print_count"]
```

You can use `typeof()` to receive a `TypeMetadata` object that contains information about the specific type of that object (even when it's assigned to a local of a different type).

```judith
typedef Employee = Person | String
const employee: Employee = new Person()
typeof(employee) -- returns the TypeMetadata of 'Person', not 'Employee'.
membersof(employee) -- returns its members as 'Person', not as 'Employee'.
```

When trying to retrieve the metadata of a specific type, rather than the type of the instance, you can use `typeof()` directly on the type:

```judith
const type_data = typeof(Employee) -- the TypeMetadata of 'Employee'.
```

Note that extension methods, constructors and interfaces will not be returned by these operations, as they aren't actually bound to their types.

# Dynamic objects
Judith features a special type of object called `Dynamic`. Dynamic objects work differently to others, as their members are late-bound (i.e. they are resolved at runtime). These objects are designed for interoperability with dynamic languages and services that don't follow type-friendly standards. They should not be used as a way to bypass the type system.

Since these objects are late-bound, they are significantly less performant than a statically-typed object.

The `Dynamic` type is special in that it is not allowed to have any extension methods, constructors or interfaces implemented into it.

```judith
const anything: String = "absolutely"
const person = new Person("Kevin")

const my_obj: Dynamic = {
    [anything] = "can", -- at runtime, creates a field named "absolutely"
    go = "here" -- 'go' acts as a string defining this field's key.
    [7] = 31 -- any type of value can act as a key.
    [person] = "even references" -- absurd but technically valid.
}
```

Since their values are late-bound, they can only be accessed with the dynamic access operator (`.[]`).

```judith
my_obj.go -- error, "my_obj" doesn't have members.
my_obj["go"] -- valid, returns "here"
```

# Hidden elements (`hid`)
The keyword `hid` allows developers to restrict the visibility of an object. When used inside a class, as mentioned above, makes the member invisible to the outside. The same applies to namespace members. When used on a top-level item (such as a function, a symbol or a typedef), it confines that definition to the file in which it is defined.

# Unsafe Judith
For performance reasons, Judith offers a set of lower-level features to deal with memory manually. However, most of these features can only be used in `unsafe` contexts.

# Appendix

## Condition evaluation
Structures that evaluate a condition (such as `if` or `while`) use truthy and falsey values, as defined in [Operations § Logical operations](operations-logical).

```judith
const zero: Num = 0
const empty_str: String = ""
const kw_false: Bool = false
const kw_null: String? = null
const obj: Object = {}

if zero => Console::log("Zero!") -- false, 0 doesn't pass the test.
if empty_str => Console::log("Empty string!") -- true, "" passes the test.
if kw_false => Console::log("Empty string!") -- false, false fails the test.
if kw_null => Console::log("Empty string!") -- false, null fails the test.
if obj.a => Console::log("Empty string!") -- false, obj.a is "undefined" and that
                                          -- fails the test.
```

### Nullable booleans and numbers
A special situation occurs with nullable types that can contain falsey values other than `null` or `undefined`. This is the case with `Bool?`, which can be `false`; and numeric types, which can be `0`. In this case, without a explicit comparison, it is not possible to determine if the user was trying to test for a `null` or for a `false` / `0`. As such, the compiler will emit a warning, suggesting the use of an explicit comparison.

```judith
const val: Bool? = false

if val then end -- WARNING: Use a explicit comparison.
if val == true then end -- No warning.
```

## `.str()` method
`str()` is a special method that is always defined for every type, even `Undefined` and `Dynamic`. As such, it is _always_ available, even on `Unknown` types or nullable ones, and never produces an error.

By default, `str()` is defined internally in the JuVM, and its behavior depends on the value on which it's called:

* Primitive types: will return a string containing the value itself. E.g. `5` will return `"5"`, `false` will return `"false"` and `undefined` will return `"undefined"`.
* Array types: will return a string that starts with `[`, then enumerates every element of the array (as a string), separated by commas and spaces, and ends with `]`. E.g. `[3,5,2]` will produce `"[3, 5, 2]"`.
* Dictionary types: same as array types, but will return key value pairs in a `key => value` format. E.g. `["Kevin"=>500,"John"=>200]` will return `"[\"Kevin\" => 500, \"John\" => 200]"`.
* Object types, regardless of their kind (struct, class, etc.): will produce a string wrapped with `{` and `}`, that maps member names to the string representation of their values. If the member is a function, it will be mapped to a signature and, if the member is a field of a reference type, it will be mapped to the name of the type inside angle brackets (`<>`). The string will be formatted. For example, a local of type `Employee` may produce:
```judith
"Employee {
    name = \"Kevin\",
    salary = 75000,
    company = <Company>,
    get_name = () => String,
    promote = [() => Void, (PromotionReason) => Void]
}"
```

`str()` can be defined manually as an extension method. In this case, this definition will override the internal definition of the JuVM:

```judith
impl func Employee::str ()
    return "A very good employee"
end

const emp = new Employee()
Console::log(emp.str()) -- outputs "A very good employee".
```

## Strings
Strings in Judith are encoded in UTF-8. This means that the code points that compose them don't have a fixed size: instead, each code point may take 1 to 4 bytes. The `String` type is aware of this encoding and the Unicode standard, and offers tools to properly deal with this.

When dealing with Unicode strings, there's three basic concepts that are relevant:
* The size, in bytes, of the string: this is what most languages call "length". As UTF-8 code points  have variable size, accessing the string at an arbitrary byte offset may result in an invalid string. This value is stored in `String.size`.
* The amount of code points in the string: each code point is a single Unicode entry, and can take 1 to 4 bytes. Code points are not necessarily characters, as some of them combine with others to form new characters (for example, '◌̃' `U+0303 COMBINING TILDE` and 'a' `U+0061 A` are two codepoints that combine into 'ã' (not to be confused with precomposed code point 'ã')). This value is obtained through `String.code_points()`.
* The amount of grapheme clusters in the string (this is what we'll call Unicode characters from now on): the number a human will guess when they see the string. This can be obtained through `String.count()`.

`String.length()` is purposefully left undefined so developers will pay attention to this difference. In practice, when dealing with strings that will be read by humans, `String.count()` (the amount of Unicode characters) is the value we'll want. Substrings (obtained either with `String.substr()` or with `String[Range]`) also deal in terms of Unicode characters.

Aside from `String`, Judith also features the types `AsciiString` (encoded in ASCII, prefixed with `ascii""`) and `WString` (encoded in UTF-32, prefixed with `utf32""`). These string types should not be used for general use-cases, but only when the performance or predictable byte size are required. Note that these types are not necessary to interact with files and streams that are encoded in other formats, as `String` is perfectly capable of parsing and outputting text in any format.

## <a name="appendix-char">`Char` type</a>
`Char` is a special type that represents a `String` that contains exactly one character (unicode grapheme cluster).

```judith
const separator: Char = ","
```

From the compiler's perspective, a value of type `Char` is just a string, but the analyzer will enforce that `Char` can only be assigned strings that are guaranteed to contain exactly one character (that is, either literals with only one character, or expressions that resolve to Char).
This is because Judith strings are encoded in UTF-8, which means a Char cannot be mapped to any specific integer size.

```judith
const some_char: Char = "This is a string"[5] -- returns "i" as Char.
```

`Char` can be implicitly converted to `String`, but not the other way around:

```judith
const some_str: String = some_char -- valid
const char_2: Char = some_str -- ERROR: 'String' may not be assigned to 'Char'.
```

Casting a string literal into `Char` is valid, although it should never be needed:

```judith
const separator = ";":Char
```

The types `AsciiChar` and `WChar` exist as equivalents for `AsciiString` and `WString`, respectively.

## nameof() and qnameof()
Sometimes, the name of a field, definition, etc. in the source code is needed as a `String`. This makes code more brittle, as the string doesn't have any connection to the identifier it's referring to. To avoid this problem, the expression `nameof()` produces a string literal at compile time whose content is the name of the identifier it contains:

```judith
nameof(std::collections::List) -- equals "List"
nameof(List.count) -- equals "count"

const numbers = [1, 2, 3]
nameof(numbers) -- equals "numbers"
nameof(numbers.count) -- equals "count"
```

Although a lot less common, the qualification of the name can be obtained with `qnameof()`. The amount of qualifiers included will be as many as used in the expression:

```judith
qnameof(Person.name) -- equals "Person.name"
qnameof(my_proj::hr::Person.name) -- equals "my_proj::hr::Person.name
```

## `Auto` type name
The `Auto` type name represents a type name that is inferred from the context, in places where such inferrence is not automatic. For example, a function that does not specify its return type is assumed not to return any value (i.e. inferred to return `Void`). However, you may want the function to return a value, but to have that value's type be inferred from the function's body. In this case, specifying a return value of `Auto` allows you to do that:

```judith
func get_num_or_str (val: Num) -> Auto -- type is inferred to be 'Num | String'
    return val when val < 10
    return val.str()
end
```

**_`EXPERIMENTAL`_** When initializing a value whose type is known, `Auto` represents the type of said field or local. This can be used to call constructors of said type:

```judith
var vals: Array<Num, 5>;

vals = Array<Num, 5>::fill(0) -- long form
vals = Auto::fill(0) -- inferred to be Array<Num, 5>.
```

# TODO

TODO: <a name="appendix-regex">Appendix § Regex</a>


# IGNORE - WIP

## Pointers
Unsafe Judith features two kinds of pointers: immutable and mutable pointers. Immutable pointers (signaled with `&`) allow reading data from the value they point to, but don't allow altering said value. As such, immutable pointers are unsafe to read, but cannot leave the program in an invalid state. On the other hand, mutable pointers (signaled with `*`) can mutate the memory they point to, which means they can leave the program in an invalid state that can lead to memory-related errors in safe code.

The pointer of a value can be obtained with the unary operator `&` (address operator). Creating a pointer to a reference value will mark that value as "in use", which will make the GC ignore that value in any GC operation. This will be the case until the value ceases to be referenced by any pointer.

Note that using the address operator in a reference type will return the address of the 

```judith
const p = Person::("Kevin", 41, 'Italy')

unsafe
    -- Immutable pointer.
    const personPtr: Person& = &p
    -- Mutable pointer.
    const extraUnsafePersonPtr: Person* = &p

    -- You can also dereference the result of an expression. Doing this will
    -- ensure that the value is placed on the heap.
    const anotherPtr: Person* = &Person::("George", 50, 'Estonia')
end
```

Memory cannot be freed manually. Any object referenced by a pointer will be eventually collected by the GC after every pointer to it has been deleted. This applies even to objects created directly as pointers.

## Pointer arithmetic
Pointers support addition, subtraction and value equality. Adding or subtracting a value to a pointer will add that value multiplied by the size of the data type 



---
---
---
```judith

```