## Roadmap
- Lexing
  - Literals
    - [ ] multi-line string
- Parsing
  - [ ] Warn when using an expression as a statement which is not a return value and isn't something like an `AssignmentOp` or `Invocation`
  - Types
    - [ ] Generics
    - [ ] Literal
    - [ ] Array
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
  - [ ] Array literals
  - [x] Enums
  - [ ] `inline`
    - [x] Variables 
    - [x] Enums
    - [ ] Functions
- Reflection/type features
  - [ ] `value is Type` 
  - [ ] Type narrowing
  - [ ] Type aliases
  - [ ] `typeof`
  - [x] `nameof`
- Runtime/HVM
  - [ ] intrinsics (built-ins)
    - [ ] injectable libraries
    - [x] functions
    - [ ] classes
    - [ ] types