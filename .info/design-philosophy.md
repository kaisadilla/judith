Nothing, as I've literally just started it.

That aside, Judith is a high-level, statically-typed programming language that draws inspiration from Rust, TypeScript, Lua and C#.

Judith takes huge inspiration from the exhaustive and flexible type systems in Rust and TypeScript, that allow developers to use composition effectively in place of inheritance. Judith includes type aliases, TS-like union types, structs as POD types, Interface as functionality-only types and classes for encapsulated machine states. Judith takes a new approach to classes by stripping inheritance out of it (that is, classes cannot have parent classes) and greatly simplifying other aspects of it.

Judith takes inspiration from Lua's simplicity, conciseness and syntax. While Judith is very rich in features and syntactic sugar (which is quite the contrary to what Lua offers), these features are not redundant: they've been carefully selected to maximize expressiveness and minimize boilerplate, while not becoming rendundant. Judith has been designed with the ideal that everything should have one obvious correct way to be done. Developers should take decisions about their algorithms, not about how to represent them in the language.

Judith takes inspiration from C#'s philosophy of making your life easier. You should be able to simply state what you want and have it work. Want a POD type holding first name, last name and age? Then all you should need to write is these three names and their type. Nothing more, nothing less.

Judith takes inspiration from C++'s and Rust's mutability, and that's why locals (as variables are called in Judith) are explicitly divided into `const` (immutable) and `var` (mutable), and member functions are divided into pure and impure (`impure`). In Judith, mutability is opt-in, allowing developers to express intent.

Judith takes inspiration from C#'s nullability and extends it by making nullability part of a type's definition. `Person?` is not just a hint, it's a completely separate type from `Person`. Of course, you can transform `Person?`s into `Person`s, but you do it explicitly and, if you decide to ignore the possibility of `Person?` being null... well, now you can't say you didn't expect it.

Finally, Judith aims to do away with some of the legacy features most programming languages have inherited from their ancestors. Here are some of my worst enemies in this regard:

- Why does C# have a `switch` statement that looks and works exactly like C's? You are not writing C. C's `switch` is a very thin abstraction over assembly. Even C# devs think that isn't right for their language, which is why they made it a compile-error not to `break` from non-empty sections of the bloc... well, at this point, why have a C-like `switch` at all?
- `goto`? No high-level programmer should ever need to use that.
- Allowing `null` everyone? Nope! If you want `null` to be a valid value, then you explicitly state so in the local's type.
- Casts and implicit conversions? I've never seen a worse design in modern languages. "Cast" can mean anything from transforming a child class into its parent type to transforming one class into another because someone decided that it makes sense. Well, we don't do that here. Judith features a syntax for upcasting (when you transform a child type into a parent type) and downcasting (when you try to transform a parent type into a child type). The rest is done explicitly via constructors, because that's what casting between two unrelated types is. As a bonus, I got rid of the ugly `((IPerson)employee).get_birthday()` C-like syntax every other language chose to keep and replaced it with a far more concise `employee:IPerson.get_birthday()`. No need to use two (!) set of parentheses when you aren't even grouping anything.