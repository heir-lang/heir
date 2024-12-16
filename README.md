# Heir
toy language part 9999999

## Contributions
Though I cannot guarantee the lifetime or dedication to this project, contributions are welcome and encouraged and I will look at them as soon as I see them.

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
    - [x] arithmetic (`+`, `-`, `*`, `/`, `//`, `%`, `^`)
      - [x] increment (`++`, `--`)
      - [x] compound assignment (`+=`, `-=`, `*=`, `/=`, `//=`, `%=`, `^=`)
- [ ] parsing
    - [ ] control flow
        - [ ] if 
        - [ ] while 
        - [ ] for 
        - [ ] switch
    - [ ] blocks
    - [x] variable declaration
    - [x] statements
    - [ ] indexing (`a["b"]`)
    - [ ] dot access (`a.b`)
    - [ ] method calls
    - [x] compound assignment
    - [x] assignment
    - [x] identifiers
    - [x] unary operations
    - [x] binary operations
    - [x] parenthesized expressions
    - [x] literals
- [ ] scopes
- [ ] resolving
- [ ] binding
- [ ] typechecking
- [ ] bytecode generator
- [ ] hvm (heir virtual machine)