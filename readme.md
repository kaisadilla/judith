**`DISCLAIMER`** Judith is a WIP started very recently. It's not even in pre-alpha. It doesn't even exist in a meaningful way. This project was started in February 2025. For the time being, all you'll find in this repo is an exhaustive description of the language, and some messy source code that can compile a very small subset of the language.

<div align="center">
  <h1>Judith Programming Language</h1>
</div>

## About Judith
Judith is a high-level, statically typed, compiled, general-purpose language. Judith is designed with the goal of writing maintainable, correct code, learning from the mistakes made by older languages. Judith's design has been carefully crafted to create a language that is pleasant, easy and intuitive to write. Judith makes no attempt to be a small language: quite the opposite, its syntax is purposefully extensive as to ensure that everything can be done in a streamlined and concise way.

## Judith's main features

- **Managed language**: By default, Judith is a garbage-collected language. You can, however, manage memory manually.
- **Written with ergonomics in mind**: every feature is designed to require as little boilerplate at possible, and usually the best way to do things is also the one that requires the least typing!
- **Mutability syntax**: Variables explicitly state whether they hold mutable or immutable values. Mutability is absolute, meaning that inner values contained in fields are also guaranteed not to change.
- **[Ergonomic I promise] ownership semantics**: not related to memory management, but instead to who is allowed to mutate a value.
- **Nullable types**: Clear distinction between non-nullabe `Int` and nullable `Int?`
- **Error handling**: exceptions are not `thrown`, but returned as regular values. Exception syntax, however, makes error handling extremely concise: with just a couple of characters, you can pass down the exception, deal with it, coalesce it to `null` or pretty promise there is no error.
- **Compiled into assembly and into JavaScript** _(during early development, assembly compilation will be done by targeting C)_.
- **Safe**: no memory leaks, no unexpected nulls, no data races.
- **Batteries included**: Judith's design features everything you'd expect from a modern language.
- **Convention over configuration**: Judith tries to have a "default" way to do everything, not bothering developers with inane choices they don't really care about.
- **Highly opinionated**: because reading code is way easier when everyone is writing it in the same way.
- **Built-in JavaScript and TypeScript compatibility when compiling into JavaScript**.
- **Unsafe Judith**: for when you really need to tell the CPU how to deal with each byte.
- **Preprocessor directives**: not like C 🤮, but like C# 🥰.
- **Almost-zero-overhead abstractions**: clean, ergonomic code is prioritized over performance, but that doesn't mean Judith compiles high-level constructs in a naïve way! High-level constructs have been carefully designed to easily map to low-level constructs while introducing as little overhead as possible.
- **~G~coroutines!** WARNING: May or may not be inspired by Go.

## Comprehensive explanation
You can read the entire informal specification [here](.info/spec.md). Note that, while the design of the language is in a very advanced stage, it's still subject to big changes, and inconsistencies in this document may appear as a result of it, which are eventually removed when as I periodically revise the entire document. The informal specification is not a tutorial, but rather a detailed explanation of each feature. The informal specification is not concerned with the implementation details of the features.

## How to get
You don't. Development of the project just started.

# Judith's design philosophy.
Read [.info/design-philosophy.jud](.info/design-philosophy.md) for a very long and boring look into my views on programming languages and how these shape Judith's design.
