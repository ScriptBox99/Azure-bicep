
# Bicep Type System
Because programs written in Bicep transpile primarily into JSON, it was clear from the beginning that Bicep has to support all data types that are available in JSON. We also wanted to provide stronger typing and validation so the users' errors are caught early in the development process and more failed deployments are avoided. These goals are very similar to the goals of the [TypeScript](https://www.typescriptlang.org/) language, which adds strong typing to JavaScript and compiles into the same.

As a result, the Bicep type system implements a small subset of the capabilities present in the TypeScript type system that are needed to support Bicep scenarios. If you are familiar with TypeScript and JSON, many of these concepts will feel familiar.

## Data Types
Bicep has a [Structural type system](https://en.wikipedia.org/wiki/Structural_type_system). In other words, the assignability of types in Bicep is determined by the definition (or structure) of the type rather than its name. All types in Bicep do have names, but these are used as shorthand type identifiers in error messages and hover texts.

The Bicep type system does not perform any automatic type conversion or coercion. Any such operations must be declared explicitly.

All values in Bicep have one of the following types:

### Simple Types
| Name | Description |
|:-|:-|
| `any` | The value can be anything. All non-error types are assignable to `any` type. Values of `any` type are assignable to all non-error types. |
| `error` | The expression has an error. No type (including another `error` type) can be assigned to a value of `error` type and `error` types cannot be assigned to any other type, including `error` type. |
| `string` | Represents arbitrary text. Equivalent to JSON strings. |
| `number` | **This type is not yet implemented (#486)** Represents a floating point number or a big integer. Equivalent to the JSON 
| `int` | Represents a 32-bit integer. Is a subtype of `number`. There is no equivalent type in JSON. |
`number` type. |
| `bool` | Represents a boolean `true`/`false` value. Equivalent to booleans in JSON. |
| `null` | Equivalent to `null` values in JSON. |

### Objects

| `object` | Represents an object with properties. Equivalent to objects in JSON. |

### Arrays
Arrays in Bicep are similar to JSON arrays. The `array` type represents an array of items where each item is of the `any` type. More strongly typed arrays may arise in various expressions. For example, an array of strings would be denoted by `string[]`.

### Literal Types

### Union Types

## Type Assignability


## Contextual Types vs. Assigned Types


| Name | Description |
|:-|:-|
| `resource` | All resource types are subtypes of this type. |
| `module` | All module types are subtypes of this type. |




## Resource Types

## Module Types

## Types used in declarations


## Type Checking


## IntelliSense Completions




