# math-expression [![NuGet Version](https://buildstats.info/nuget/YuKu.MathExpression?includePreReleases=true)](https://www.nuget.org/packages/YuKu.MathExpression/)

Math-Expression is a library for parsing and compiling simple mathematical expressions.

## Supported expressions

 * Parenthesis: `(`...`)`
 * Function call: `sin`, `sqrt`, `ceiling`, `round`, ...
 * Constants: `pi`, `e`
 * Negation (unary minus): `-`x
 * Exponents (x<sup>y</sup>): x`^`y
 * Multiplication: x`*`y
 * Division: y`/`y
 * Addition: x`+`y
 * Subtraction: x`-`y
 * Number literals: `7`, `3.1415`
 * Variables: `x`, `t`, `var`, ...

Implied multiplication supported so `*` operator is optional.

Identifiers (names of functions, constants and variables) should start with letter so spaces between numbers and identifiers are optional. For example, `2pi` is the same as `2 pi`, and the same as `2*pi`.

Functions, constants and variables are case insensitive.

## Operators precedence and associativity

From highest to lowest:

| Category | Operators |
|---|---|
| Primary | function call |
| Unary | `-` |
| Exponentiation | `^` |
| Multiplication | `*`, _implied multiplication_, `/` |
| Addition | `+`, `-` |

When an operand occurs between two operators with the same precedence, the associativity of the operators controls the order in which the operations are performed:
* Except for the exponent operator (`^`), all binary operators are left-associative, meaning that operations are performed from left to right. For example, `x + y + z` is evaluated as `(x + y) + z`.
* The exponent operator (`^`) is right-associative, meaning that operations are performed from right to left. For example, `x ^ y ^ z` is evaluated as `x ^ (y ^ z)`.

Precedence and associativity can be controlled using parentheses. For example, `x + y * z` first multiplies `y` by `z` and then adds the result to `x`, but `(x + y) * z` first adds `x` and `y` and then multiplies the result by `z`.

## Functions

Any method from `System.Math` class that takes single `Double` parameter and returns `Double` can be used as a function.
Subexpresson that follows function name will be used as function parameter. For example, `sin 2t` is evaluated as `((sin(2))*t)`, but `sin(2t)` is evaluated as `(sin(2*t))`.

Public fields of `System.Math` class can be used as a constants (`pi`, `e`).

## Variables

Any number of variables can be referenced from expression. To use variables just pass their names to [`Compile`](math-expression/Extensions.cs#L16) method. Compiled function will accept same number of parameter in the same order.

Variables takes precedence over functions and constants.

## Examples

``` csharp
Func<Double, Double> func = "80(t - 3sin(2t))/(2pi)".Compile<Func<Double, Double>>("t");
Func<Double, Double> csFunc = t => 80 * (t - 3 * Math.Sin(2 * t)) / (2 * Math.PI);
Assert.AreEqual(func(1.0), csFunc(1.0));
```
