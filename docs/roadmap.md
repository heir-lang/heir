## Roadmap
- Lexing
  - Literals
    - [ ] multi-line string
- Parsing
  - [ ] Warn when using an expression as a statement which is not a return value and isn't something like an `AssignmentOp` or `Invocation`
  - Types
    - [x] Generics
    - [ ] Literal
    - [x] Array
    - [ ] Interface index signatures
    - [ ] Interface method signatures
    - [ ] Class
  - Control flow
    - [x] `if`
    - [x] `while`
    - [ ] `for`
    - [ ] `switch`
    - [x] `break`
    - [x] `continue`
  - [ ] Lambdas
  - [x] Array literals
  - [x] Enums
  - [ ] `inline`
    - [x] Variables 
    - [x] Enums
    - [ ] Functions
- Reflection/type features
  - [ ] `value is Type` 
  - [ ] Type narrowing
  - [ ] Type aliases
  - [ ] Type casting
  - [ ] `typeof`
  - [x] `nameof`
- Runtime/HVM
  - [ ] intrinsics (built-ins)
    - [ ] injectable libraries
    - [x] functions
    - [ ] classes
    - [ ] types
- Unit Tests
  - [ ] Parsing/binding/typechecking type parameters/arguments