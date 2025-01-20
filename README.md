# Heir
[![CI Status](https://github.com/R-unic/heir/actions/workflows/ci.yml/badge.svg)](https://github.com/R-unic/heir/actions/workflows)
[![Coverage Status](https://coveralls.io/repos/github/R-unic/heir/badge.svg?branch=master)](https://coveralls.io/github/R-unic/heir)

## Contributions
Though I cannot guarantee the lifetime or dedication of this project, contributions are welcome and encouraged and I will review them ASAP.

## Community
Join the Discord community!
<br>
https://discord.gg/AEbNTEVNAd

## Roadmap
- [x] lexing
  - [ ] trivia
    - [x] whitespace
    - [x] newlines
    - [x] semicolons
    - [x] comment
    - [ ] multi-line comment
  - [x] literals
    - [x] integer
      - [x] binary
      - [x] octal
      - [x] hexadecimal
    - [x] float
    - [x] string
    - [x] character
    - [x] boolean
    - [x] none
  - [x] keywords
    - [x] `let`
    - [x] `mut`
    - [x] primitive type keywords
  - [x] identifiers
  - [x] brackets (`()`, `[]`, `{}`)
  - [x] operators
    - [x] comma
    - [x] equals
    - [x] dot
    - [x] colon
    - [x] colon colon
    - [x] question mark
    - [x] null coalescing (`??`, `??=`)
    - [x] equality (`==`, `!=`, `!`, `<`, `>`, `<=`, `>=`)
    - [x] logical (`&&`, `||`) 
    - [x] bitwise (`&`, `|`, `~`) 
        - [x] shifts (`<<`, `>>`)
    - [x] arithmetic (`+`, `-`, `*`, `/`, `//`, `%`, `^`)
      - [x] increment/decrement (`++`, `--`)
      - [x] compound assignment (`+=`, `-=`, `*=`, `/=`, `//=`, `%=`, `^=`)
- [ ] parsing
    - [ ] warn when using an expression as a statement that is not a return value and isn't something like AssignmentOp or Invocation
    - [ ] types
        - [x] singular
        - [x] union
        - [x] intersection
        - [x] parenthesized
        - [ ] literal
        - [ ] array
        - [ ] function
        - [ ] interface
        - [ ] class
      - [ ] control flow
        - [x] if 
        - [ ] while 
        - [ ] for 
        - [ ] switch
    - [x] blocks
    - [x] variable declaration
    - [x] statements
    - [x] indexing (`a["b"]`)
    - [X] dot access (`a.b`)
    - [x] method calls
    - [x] method declaration
    - [ ] array literals
    - [x] object literals
    - [x] compound assignment
    - [x] assignment
    - [x] identifiers
    - [x] unary operations
    - [x] binary operations
    - [x] parenthesized expressions
    - [x] literals
- [x] scopes
- [x] resolving
- [x] binding
- [x] typechecking
- [x] bytecode generator
  - [x] bytecode binary serialization/deserialization
- [ ] intrinsics (built-ins)
    - [ ] injectable libraries
    - [x] functions
    - [ ] classes
    - [ ] types
- [ ] hvm (heir virtual machine)
    - [X] tail call optimization