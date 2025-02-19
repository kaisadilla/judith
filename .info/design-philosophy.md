# Why Judith?
In the last two decades, developers of all kinds have reached certain agreements about what makes good code; about which features make development easy and which ones turn code into a minefield. The problem is that most programming languages in the scene are older than that and lack many of the features that make a codebase robust, easy to understand and hard to break. Moreover, they carry a lot of legacy features and design decisions that their designers have come to regret, but that they can't remove anymore.

Many of the newer languages solve some of these problems, but in my opinion they are too conservative and end up only adopting one or two new features while copying the rest of the "Java model" verbatim.

Judith aims to solve these problems by presenting a new language built from scratch, that takes the risk to do things differently if the "normal" way doesn't carry its weight. There's no value in creating yet another language that is just Java with a few quirks, because it's simply not worth it to start from scratch just to enjoy one nice feature.

Judith is a one-man project and, as such, I don't expect it to succeed in any way, but I think it's a good enough model for a modern high-level language to deserve being made public and, hopefully, inspire someone to create a successful language inspired by it.

# Design principles

## Judith is a high-level language
Judith is a high-level language, and "high-level" means that developers are only concerned with writing down what they want their program to do. This means that just providing you with the tools you need to write a program is not enough - any language can do that, that's like the bare minimum to be a language. Instead, Judith's syntax is designed to be easy and pleasant to work with.

### Writing code should never be painful
Want a string? `"here's a string."` "But my string has quotes!", ``use backticks`` [which some markdown processors don't show.]. Want values inside them? `f"Template {nameof(String)}s!"`. Want to paste a json inside?
```judith
const json: String = ff"""
    {
        "this is": "a raw string",
        "and you don't": "need to escape anything",
        "yet it can still have": "{{nameof(template_values)}}"
    }
"""
```
See? You don't need to adapt to how Judith likes Strings to be formatted - instead, Judith adapts to your needs.

### Implementation details are not the developer's concern
These are our concern, not theirs. Judith is a high-level language, it can afford to sacrifice some performance from time to time to make the developer's job easier. For example, all types are nullable in the exact same way: you add an `?` to their type (`Person?`, `Num?`), you check for nullability (`my_person !== null`, `my_num !== null`), you use them as non-null when nullability has been discarded. Under the hood, null in reference types and null in value types are implemented differently, but that's not the developer's problem. All they can see is that both behave in the exact same way in his code.

### Novelty budget has to be spent wisely.
When Judith does something different, it does so for a reason. Otherwise, Judith sticks to names and conventions developers already know. Did I have to choose `+` to concatenate Strings? No. But what do I win from using a new operator for it and forcing everyone to learn a new way to concatenate strings? Did I have to make my `while` block have the same name and structure as all other languages? Nope, but what do I win from changing it? Judith is not here to make you lose time. If Judith does something different, it's because there's a real benefit to doing it differently.

### Get rid of legacy features
Why does C# have a `switch` statement that looks and works exactly like C's? You are not writing C. C's `switch` is a very thin abstraction over assembly. Even C# devs think that isn't right for their language, which is why they made it a compile-error not to `break` from non-empty sections of the bloc... well, at this point, why have a C-like `switch` at all?

Judith replaces `switch` with `match`, a structure that does the same job, but does so with a syntax that is consistent with the rest of the language, that is convenient to use, and that has more and more powerful features:
```judith
match country do
    case "United States" then
        -- statements
    end
    case "Germany", "France", "Italy", "Spain" then
        Console::log("The previous case won't fall through to this one.")
    end
    else
        -- statements
    end
end
```
This match statement can also match patterns (such as `< 3`) or types (such as `is Dog`).

_\* Judith does feature a `switch`-like expression named `jumptable`. The reason it's not named `switch` is precisely to avoid misleading developers, who are used to `switch` statements in other languages, adopting it over `match` out of habit._

## Style matters
9 out of 10 developers <sup>[citation needed]</sup> agree that predictable and consistent tools and code bases are easier and nicer to work with. There's nothing more frustrating than knowing what you want to do and trying to guess how the language decided to do it. Judith is designed with a lot of attention to detail, so once you get a grasp of how Judith does things, you can correctly guess how Judith does all things.

### Judith is consistent and predictable
Types in Judith are always written in PascalCase. Locals, functions and members are always written in snake_case. All functions related to a type can be found as extension functions of said type. Want to convert `3` to `String`? `3.str()`. Want the length of an array? `arr.len`.

### Judith uses clear identifiers in its standard library.
Developers shouldn't be expected to learn weird cryptic names for hundreds of functions and methods. Some extremely common abbreviations are ok (like "str" and "concat" for "string" and "concatenate"), but this is the exception for a handful of identifiers that are usually spammed everywhere. By default, Judith uses meaningful words and expressions to name things. C named his "parse integer" function "atoi" for reasons to boring to explain, but we don't have to. To parse an number in Judith, you use Num::parse(). C uses a cryptic "fopen()" function to open a file, and an even cryptic-er "fgets()" to read a string from it, we use far more descriptive names like File::open() or File.read().

### Judith recommends a specific formatting and provides a formatting tool
Just so you don't have to. Just so you don't need to pray your boss will choose a sensible formatting standard (if they even want to choose one). Just so you don't have to pray your coworker doesn't have Opinions™ and starts prefixing all member fields with an "m_". Just so the creators of [Prettier](https://github.com/prettier/prettier) don't ever have the chance to butcher Judith source files just like they butcher JavaScript ones (the formatting guide is actually heavily inspired by Prettier, but does away with some... questionable decisions). Of course, you don't _have_ to use Judith's formatting, but the rest of us can call you a bad developer if you don't.

### Judith's syntax is designed around good formatting
Try writing a constructor with 6 long parameters that passes 4 of them to the base class in C#. You'll have to pick your poison between a line that needs 4 screens to fit into, making your constructor look like it was pasted from a PDF with random line breaks, or taking the entire height of the screen by giving each token its own line.

Judith cannot save you from 6 long parameters in a constructor, but it can ease that pain. Does the constructor for your `ProjectContractChargingPeriodProjectAccountReferenceVMFactoryBuilderStrategy` class really need to write down that entire name again? The answer is no, which is why Judith just says `ctor` instead.

## Code should clearly express intent
Code is read way more times than it is written, and thus it should be as clear as possible for the reader what each line of code is trying to achieve. When you see `void someMethod (MyClass obj)` in Java, you are missing _a lot_ of information. Can that `obj` be `null` or not? Will that `obj` be mutated inside the method? When I read the implementation of the method... should I keep track of `obj` throughout the entire body to see if it can mutate? All of these unanswered questions quickly add up, and failing to consider any of them can have catastrophic consequences. You really don't want your program to crash because you received `null` into a parameter where it _obviously_ shouldn't be, right? Moreover, all of that intent being left out of the equation promotes bad practices, such as plugging `null` into a flow that wasn't designed with that in mind, or mutating state just because it's the easy way to implement something.

### Immutability by default
Judith is built around mutability. Locals in Judith can be declared as either `const` or `var`, the former meaning that the local is immutable and the latter meaning that it can mutate. Mind you, this is true immutability, meaning that a `const` local cannot mutate the object it holds either (e.g. a const Person cannot have its field "name" reassigned). Member methods need to be marked `impure` to be able to modify state, and doing so means they can't be called from `const` locals.

Immutability promotes cleaner code with a lower cognitive complexity, as immutable locals promise the developer that tracking the state of the local is not necessary, as it cannot change.

### Explicit nullability
`null` has been called the "billion-dollar mistake" by its own creator, and for a good reason. An unexpected `null` value will almost always crash a program at best, and lead to unexpected and dangerous behavior at worst. And the only solution to avoid this is painfully checking that every single value you ever touch in your code is not `null`. 

Judith avoid this problem entirely by not allowing `null` as a value for a field or local unless explicitly stated by the developer. Judith achieves this by separating regular types (`Num`, `String`, `Person`) from nullable ones (`Num?`, `String?`, `Person?`). `null` is treated as its own type and, as such, nullable types require the developer to check for null before using the value.
```judith
const String? name = "Kevin"
function_that_expects_string(name) -- invalid, as "String?" is not compatible with "String"

if name is not null
    function_that_expects_string(name) -- valid, name is "String" here.
end
```
This eliminates the risk of unexpected `null`s by making it a compile error to introduce one into a context that doesn't account for it. It also helps reduce lower cognitive complexity as that's one (important) value the writer and the reader of the code alike don't have to care about.


## Cognitive complexity is important: prefer simplicity.
### Less is more
I like syntactic sugar. A lot. And Judith has a lot of it - I take pride of that in other sections of this document, but things should still have one obviously right way to do them. Every syntactic sugary feature I've added to Judith exists because it makes code easier to read and understand. No feature is added just to save two keystrokes at the expense of having to learn a new way to do things, nor to give developers more "freedom". Developers don't need freedom, they need tools.

### Composition over inheritance
Judith doesn't feature inheritance at all. Instead, Judith is designed around composing types. Judith features three complex types: `struct`, which stores data, `interface`, which defines behavior and `class`, which holds state and manages said state. You can implement shared behavior for multiple types by implementing an interface for each of these classes. You can specialize a `struct` by adding another `struct` to it. You can extend a `class` by having its "ancestor" be a member field of the new class. You can group types that don't share any specific behavior by creating a union type. You can extend a type's behavior with extension functions and constructors. You can define types as aliases, unions, literals and sets of literals.

This approach to types has many benefits over traditional OOP languages: different needs are solved by different structures, avoiding all the boilerplate to adapt classes for uses that don't fit classes well. Types are kept simple, functionality is decoupled from data and state machines are neatly encapsulated in classes that don't become juggernauts of dozens of fields and hundreds of unrelated methods.

### Casting is a clearly defined concept
Casting in programming is one of these things that don't really refer to anything in particular, but rather to a set of superficially similar but unrelated operations. Well, this is not the case in Judith. Judith has only two (+ two) types of casting: Upcast and downcast (+ the same concept applied to numeric types).
- "Upcast" means transforming an object of type T1 into another type T2 that type T1 belongs to. For example, if we define `typedef Id = String | Num`, then both `String` and `Num` can be upcasted into `Id` as both are guaranteed to be valid `Id` objects. This operation is always safe and occurs at compile-time.
- "Downcast" means the opposite: transforming a parent type into a child type. In this example, transforming `Id` into `String` or `Num`. Unlike upcasting, downcasting happens at runtime and is NOT a safe cast. So it should be avoided.

And what about casting between two unrelated objects? Well, do you mean constructing an object of type T1 based on the data inside an object of type T2? Judith has a feature for that: it's called constructors. Nothing stops you from defining a constructor for Person that takes Dog as its only argument and builds a person out of them. Just don't pretend you are "casting" the Dog into a Person because that's not what's happening.

Also, Judith has a neat syntax to upcast objects: `obj:Type`, which has the highest precedence out of any operator. This makes it possible to write `obj:Type.method()` rather than the ugly C-like `((Type)obj).method()`. (If you are wondering, downcasting is done like `obj:?Type`).
