
# Types
Because programs written in Bicep transpile primarily into JSON, it was clear from the beginning that we have to support all data types that are available in JSON. We also want to provide stronger validation so we can catch errors early in the process. This naturally aligns with the goals of TypeScript (which also happens to be a superset of JavaScript which is itself a superset of JSON).

As a result, the Bicep type system is heavily inspired by the TypeScript type system and JSON data types. If you are familiar with TypeScript and JSON, many of these concepts will feel familiar. Otherwise, read on to find out more.

## Basic Types
All values have one of the following types:

| Name | Description |
|:-|:-|
| `any` | The type of value cannot be determined. |
| `error` | The expression has an error.|
| `string` | Represents arbitrary text. Equivalent to JSON strings. |
| `int` | Represents a 32-bit integer. |
| `number` | **This type is not yet implemented (#486)** Represents a floating point number or a big integer. Equivalent to the JSON `number` type. |
| `bool` | Represents a boolean `true`/`false` value. |
| `object` | Represents an object with properties. Equivalent to objects in JSON. |
| `array` | Represents a list of values. |
| `null` | |



klkk

| Name | Description |
|:-|:-|
| `resource` | All resource types are subtypes of this type. |
| `module` | All module types are subtypes of this type. |

## Literal Types

## Union Types



## Resource Types

## Module Types

## Types used in declarations


similarities to json and typescript
no user-defined types

no type automatic coersion/conversion

assignability matrix


## Type Checking


## IntelliSense Completions




