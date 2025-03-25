# Comments
You can write single-line comments with `//` and multi-line comments with `/*` and `*/`.

# Files
Each `.jir` defines a JIR compilation unit. Each JIR compilation unit contains a list of items. Each item has a unique name. JIR does not have modules or namespaces, which means that every item must be given a unique name.

## Symbol names
Every symbol has a name, consisting of a Unicode string of characters. Every character except for `'` is allowed to appear in a symbol's name. Symbol names are always written between `'` delimiters:

```jir
function 'std/collections/List/add'
```

# Functions
Functions are an item. They are declared with the `function` keyword, followed by their name. They contain a `return_type` with the name of the type they return, a `kind` that specifies its kind (function, generator, constructor...) and a block containing their body. Their body block must start with their parameters block. Each parameter in the block contains its name and a local description.

```jir
function 'add_two_numbers' kind=function return_type='F64' {
    parameters {
        'a' type='F64' final;
        'b' type='F64' final;
    }

    // body
}
```

## Locals
Locals are variables that exist inside functions. They have a name and a description with the following properties:

* `type` **required**: the name of the local's type.
* `final`: if true, the local is assumed to never be reassigned.
* `immutable`: if true, the content of the local is assumed to not be able to change. This property cannot be used in parameters.

## Expression
Expressions evaluate to a value of a certain type. Expressions can contain other expressions.

### Literal expression
Literal expressions create a value of the given type.

```jir
'Bool' false
'I64' 337
'String' "Kevin"
```

### Identifier expression
Identifier expressions evaluate to the symbol with the name given. Local symbols are referenced directly, while global symbols are referenced with `:`.

```jir
'F64' 'a' // returns the value in the local 'a', asserting its type as 'F64'.
'F64' :'a' // returns the value in the global 'a'.
```

### Binary math operation expression
Binary math operation expressions take two values the same type and return a new value of the same type:

```jir
%add 'F64' ('F64' 'a', 'F64' 10)
```

The following operations are available:

* `%add`: Adds b to a.
* `%sub`: Subtracts b from a.
* `%mul`: Multiplies a by b.
* `%div`: Divides a by b.
* `%mod`: Does a mod b.
* `%rem`: Does a rem b.
* `%fdiv`: Divides a by b and does floor() to the result.

### Unary math operation expression
Unary math operation expressions take one value and returns a new value of the same type:

```jir
%neg 'F64' ('F64' 20)
```

The following operations are available:

* `%neg`: Returns the negative of a.

### Unary operation expression
Unary operation expressions take one value and return another value of a given type.

```jir
%box 'Box'('F64') ('F64' score)
```

The following operations are available:

* `%box`: Returns a Box containing a.

### Number cast operation expression
Numnber cast operation expressions take one expression of a given numeric type and cast it to a value of another type.

```jir
%cast 'I32' 'a' to 'I64'
```

The following operations are available:
* `%cast`: Transforms a into T1.

### Comparison operation expression
Comparison operations take two values and return a `Bool`.

```jir
%eq 'Bool' ('F64' 12, 'F64' b)
```

The following operations are available:

* `%eq`: Returns true if the value of a is equal to the value of b.
* `%neq`: Returns true if the value of a is different to the value of b.
* `%lt`: Returns true if the value of a is lesser than the value of b.
* `%le`: Returns true if the value of a is lesser than or equal to the value of b.
* `%gt`: Returns true if the value of a is greater than the value of b.
* `%ge`: Returns true if the value of a is greater than or equal to the value of b.

### Assignment expression
Assignment expressions assign the result of an expression to an existing symbol:

```jir
'a' = %add 'F64' ('F64' 'a', 'F64' 10)
```

### Call expression
Call expressions call the given function symbol:

```jir
call 'F64' :'add_two_numbers' ('F64' 'a', 'F64' 10)
```

### Calldynamic expression
Calldynamic expressions call the value of a symbol, which is assumed to contain a function:

```jir
calldynamic 'Bool' 'callback' ('String' "Kevin")
```

## Statement
The function's body contains statements:

### Expression statement
An expression statement is just an statement that evaluates the given expression and discards its result. It's creating by writing an expression followed by `;`

```jir
call 'F64' :'add_two_numbers' ('F64' 'a', 'F64' 10);
```

### Local declaration statement
The local declaration statement declares one local with one name, and optionally initializes it with a value. Reading the value of an uninitialized local is undefined behavior.

```jir
local 'c' type='F64' constant immutable init=();
```