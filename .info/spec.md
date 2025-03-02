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

While numeric literals themselves will be reinterpreted as needed, values of a numeric type cannot be implicitly converted (e.g. once score of type `Num` is defined, you cannot assign the value of `score` to a local of type `Int`).

_For a full explanation of number conversion, see [Type casting § Number casting](#typecasting-numbers)._

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
_See [Reflection](#reflection)._

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

# Functions
Functions are defined by the keyword `func`:

```judith
func get_value_plus_10 (const value: Num) : Num
    return value + 10
end
```

Functions can return more than one value:

```judith
func get_info (id: Guid) : String, Num
    -- statements
    return name, num
end
```

When a function returns more than one value, you assign it like this:

```judith
const name, num = get_info(player_id) -- valid
const name = get_info(player_id) -- valid, 'num' is discarded
const num: Num = get_info(player_id) -- ERROR, first value is 'name' of type
                                     -- 'String'.
const _, num = get_info(player_id) -- valid, first value is assigned to discard.
```

Functions can return `const` values. These values cannot be assigned to mutable locals or fields:

```judith
func get_immutable_person (id: String) : const Person
    -- statements
    return person
end

var p = get_immutable_person("kevin") -- ERROR: 'const String' cannot be assigned
                                      -- to a variable.
const p: const Person = get_immutable_person("kevin") -- ok.
```

## Variadic functions
Variadic functions can take any number of arguments. To define a variadic function, its last parameter has to use the spread operator, and the type of said parameter has to be a type that can be initialized as a list (i.e. any type that can be initialized with [a, b, c] syntax).

```judith
func add_many_numbers (...nums: List<Num>) : Num
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
func build_person (person_ctor: Person!)
    const p = Person!(person_ctor)
    -- statements
end
```

Named constructors can also be used:

```judith
func build_person (person_ctor: Person::make_kevin!)
    const kevin = Person::make_kevin!(person_ctor)
    -- statements
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
func add (a, b: Num) : Num
    return a + b
end

func multiply (a, b: Num) : Num
    return a + b
end

func negate (a: Num) : Num
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
```

## Array
While `List<_T>` is recommended for most cases, the type `Array<_T, _count>` provides a fixed-size array, equivalent to C# arrays or C++ `std::array<T, c>`.

```judith
const scores: Array<Int, 4> = [3, 5, 9, 15]
```

## Array types
_See [User-defined types § Array type](#user-types-array)._

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
scores["Regina] -- returns 'undefined'.
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
const some_type = SomeType<int>!([5, 7, 9])
```

### Dictionary initializer expression
If the collection initializer expression contains key-value pairs (defined as `key_expr = value_expr`), then this expression is translated to repeated calls to the `set [T]` operator.

```judith
const my_dict = ["Kevin" = 7, "Ryan" = 5]
```

Is equivalent to

```judith
const my_dict = !()
my_dict["Kevin"] = 7
my_dict["Ryan"] = 5
```

# Casting and narrowing
## Type casting
Type casting in Judith is restricted to operations that change types between compatible types. There's various types of casting:

### Upcasting (`:`)
Upcasting allows a value of a subtype B to be casted to a supertype A. As B is guaranteed to be also of type A, this cast is performed at compile time at no cost.

```judith
const dog = Dog!()
const animal: Animal = dog:Animal -- valid cast, as 'Dog' is always an 'Animal'.
```

### Downcasting (`:?`, `:!`)
Downcasting allows a value of a supertype A to be casted to a subtype B. Since a value of type A is not guaranteed to be also of type B, this cast is performed at runtime. The way invalid casts are handled depends on the operator used:

#### Safe downcasting operator (`:?`)
With the safe downcasting operator, the cast will return `null` when it fails, avoiding errors. However, due to this, this expression evaluates to a nullable type, even when used on a non-nullable one:

```judith
const animal = Cat!()
const dog: Dog? = animal:?Dog -- will be null, as animal's type is 'Cat'.
```

#### Unsafe downcasting operator (`:!`)
With the unsafe downcasting operator, the cast will throw a `InvalidCastException` exception when it fails. This expression preserves the nullability of the original type:

```judith
const dog: Dog = animal:!Dog -- will throw, as animal's type is 'Cat'.
```

### Number casting
Number casting allows converting between different numeric types (Num, Int, Byte, Ui32, etc.). This type of casting uses the same operators as upcasting and downcasting (`:`, `:?`, `:!`).

Number casting always occurs at runtime, but each casting operator offers the same guarantees as it does with other types.

```judith
const num: Num = 13
const integer: Int? = num:?Int -- cast fails if num is too big for Int.
const num2: Num = integer:Num -- Errors cannot occur, every Int can be a Num.
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
| Decimal to float                                   | `:?` or `:!` |

\* BigInt is considered an integer of infinite size, so it's always bigger than any other integer type.

### Null-forgiving casting
Nullable types can be casted into their non-nullable counterparts by appending the null-forgiving operator `!` at their end. Casting a `null` value in this way will throw a `NullValueException`.

```judith
const person: Person? = null
const person_2: Person = person! -- will throw, as 'person' is null.

Console::log(person!.name) -- will throw.
```

Unsafe casting operations are extremely discouraged, as when, left unhandled, can crash a program and, when handled, can be slower than safe casting operations when they fail; as they throw exceptions rather than producing a sentinel `null`.

## <a name="types-narrowing">Type narrowing</a>
Narrowing is the process of refining a broader type into a more specific one based on control flow. While traveling through a possible execution branch of a block of code, when a certain property of a value's type is asserted, that property remains true for the remainder of that branch.

### Type checking (`is`)
With the `is` keyword, a value can be asserted to be (or not to be) of a given type:

```judith
type Animal = Dog | Cat | Rabbit | Mouse

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

func get_person (id: Id) : Person
    return get_person_by_string(id) when id is String
    return get_person_by_num(id) when id is Num
    
    -- no need to return anything
end
```

In this example, even though there's no "general" return statement, the compiler recognizes that it's impossible for the function not to have exited after the second return statement, so it doesn't require the developer to write a superfluous `return` afterwards.

# User-defined types
Developers can define their own types with the `typedef` keyword.

## Alias type
An alias type is a type that maps to another type. Aliases can be implicit or explicit.

### Implicit alias
Implicit aliases allow the defined type to be used as if it was the original type.

```judith
typedef UniqueId = Int
const id: UniqueId = 32 -- valid, as UniqueId is Int.
const integer: Int = id -- valid, as UniqueId can be used as an Int.
```

The compiler in this example sees `UniqueId` as `Int`.

### Explicit alias
Explicit aliases are equivalent to the type they are derived from, but a value whose type is the alias type cannot be used as if it was the original type.

```judith
typedef expl UniqueId = Int
const id: UniqueId = 32 -- valid, as UniqueId is initialized like Int.
const integer: Int = id -- ERROR: cannot assign 'UniqueId' to 'Int'.
const integer: Int = id:Int -- valid, explicit cast is allowed.
```

## Union type
A union type defines a type that is the union of several types. As such, a relationship is established between the Union type and the types it's derived from, where all of the derived types are also the Union type, and the Union type could be any of the derived types.

```judith
typedef Id = Num | String
var id: Id = 36 -- valid, as "Num" is an "Id".
id = "string_id -- valid, as "String" is also an "Id".
```

We can downcast a union into any of its conforming types:

```judith
const num: Num = Id:?Num
```

Unions of string literals will raise a warning, as a set is preferred:

```judith
typedef Countries = "Germany" | "Sweden" | "UK" -- WARNING: define a set instead.
```

Unions with literal `null` will raise a warning, as it would make them inferior versions of a nullable type:

```judith
typedef Animal = Dog | Cat | null -- WARNING: Don't include 'null'.
```

## Literal type
A literal type is just a literal used as a type

```judith
var country: "Germany" = "Germany" -- This value can only equal "Germany"
country = "France" -- error, String is not assignable to "Germany".
```

## Set
A set type is defined as a group of string literals. Literals used in a set use single quotes rather than double quotes.

```judith
typedef set Country
    'Germany'
    'Sweden'
    'France'
    'Italy'
    'Japan'
end

var country: Country = 'Germany'
country = "Great Britain" -- invalid, as "Great Britain" is not part of the set.
```

Set types cannot be used as a `String`, but can be upcasted into one:

```judith
const country_name: String = country:String
```

## <a name="user-types-array">Array type</a>
An array type is a type that defines an array where the type of each value inside it is explicitly defined.

```judith
const info: [String, Num] = ["Kevin", 36]
info[0] -- inferred to be of type "String".
info[1] -- inferred to be of type "Num".
info[2] -- inferred to be "undefined".

const n = get_num()
info[n] -- inferred to be of type "String | Num | undefined".
```

## Object type
An object type is a struct-like type defined by its member fields:

```judith
const anon = {
    username = "x__the_best__x",
    score = 500_000_000,
} -- type is inferred as "{username: String, score: Num}"

anon.username -- valid, evaluates to a String.
anon.id -- invalid, 'anon' doesn't contain field 'id'.
```

## Struct
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

Structs have a default constructor that contains all of its fields in order:

```judith
const p = Person!("Kevin", 28, 'Germany', 103_000d)
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

## Interface
If structs hold data, interfaces define behavior. Objects cannot be of an interface type, but instead interfaces are implemented into types to give them new functionalities.

```judith
-- By convention, interfaces are prefixed with the letter "I".
typedef interface ISummarizable
    -- Abstract methods: methods declared by the interface that must be
    -- implemented by the type.
    func summary () : Void
    
    -- As any other method, these methods may be pure or impure.
    impure func mark_as_read () : Void

    -- As interfaces cannot have member fields, any member data that wants to
    -- be guaranteed must be exposed through a getter method:
    func is_read () : Bool

    -- Concrete methods: methods provided by the interface. These methods are
    -- inherited by the types that implement the interface, so there's no need
    -- to cast them back to the interface type to use them (unless they collide
    -- with members of that type).
    func extended_summary () : Bool
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

## Class
Classes are the most complex type in Judith. They represent state machines, whose behavior is encapsulated and controlled by the class. For this purpose, classes feature privacy rules.

```judith
typedef class Person
    -- Fields contained by the class. Fields are variable inside the class, but
    -- are exposed as constant to the outside. This means that code from the
    -- outside can read their value, but cannot assign new values to it nor call
    -- any impure method in them.
    name: String

    -- A member field can be hidden from the outside altogether with the "hid"
    -- keyword. A hidden field will not be visible at all from outside the class.
    hid birth_year: Num?

    -- A member field can be exposed as a variable to the outside, allowing
    -- code from the outside to mutate its state or assign a new value to it.
    -- This can be achieved with the "pub" keyword, that makes it public.
    pub country: Country

    -- A member field can be marked as mutable with the "mut" keyword. A mutable
    -- field can be mutated even by pure methods. Semantically, mutable members
    -- are not considered part of the object's state, so they should not have
    -- any effect on the object's behavior.
    -- note that "mut" is not compatible with "static" or "const", as it wouldn't
    -- have any effect.
    mut times_age_was_read: Num = 0 -- Default initializer.

    -- Sometimes fields contain references to objects not owned by the class.
    -- Semantically, these objects should not be mutated from inside the class.
    -- To achieve this effect, using const types can help enforce this constraint,
    -- as const types cannot be mutated even by impure functions.
    -- Additionally, this approach allows consumers of the class to use const
    -- values to initialize the class.
    -- Note that this doesn't mean 'company' cannot change. As 'company' is a
    -- reference, there's no guarantee that whoever owns the reference won't
    -- mutate it. As such, it shouldn't be considered part of the class's state.
    company: const Company

    -- static members do not belong to any instance, but rather the class itself.
    static people_created: Num = 0

    -- Classes may include constructors inside their own definition:
    ctor (name: String, birth_year: Num?, country: Country, company: Company)
        -- The instance a method or constructor is running on is contained in
        -- a special local named "self".
        self.name = name
        -- "self" can be implied by simply using the accessor token "." on nothing.
        .birth_year = birth_year
        .country = country
        .company = company

        -- similarly, the class itself can be implied by using the scope
        -- resolution token "::" on nothing. "::people_created" is the same as
        -- "Person::people_created".
        ::people_created += 1
    end

    -- Classes can define their own methods. By default, functions are pure,
    -- which mean they cannot mutate their own member fields.
    func print_name ()
        Console::log(.name)
        .name = "New name" -- error: pure function cannot mutate the instance.
    end

    func get_age () : Num
        -- Methods defined inside the class can access hidden members.
        const age = .calc_age()

        -- Mutable fields can be mutated even by pure functions.
        .times_age_was_read += 1

        -- Static fields are not part of any instance's state, so they can be
        -- mutated freely even by pure functions. This specific line doesn't
        -- make much sense, but it's for demonstrative purposes.
        ::people_created += 1

        return age
    end

    -- The keyword "impure" can be used to define an impure method, which can
    -- mutate the instance's members. These methods cannot be called on instances
    -- assigned to constants, but only those assigned to variables.
    impure func relocate (new_country: Country)
        -- Valid assignment, as this is an impure function
        .country = new_country
    end

    hid func calc_age () : Num
        return Date::now().year() - .birth_year
    end

    -- Since static members are not part of any instance's state, they do not
    -- have the concept of "purity".
    static func print_people_created ()
        Console::log(::people_created)
    end
end
```

Classes are created through their constructors. Unlike structs, they cannot be created by manually assigning them values to their fields with an object initialization expression.

```judith
var kevin = Person!("Kevin", 1975, 'Germany')
```




# TODO

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