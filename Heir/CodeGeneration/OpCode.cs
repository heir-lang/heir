namespace Heir.CodeGeneration;

public enum OpCode : byte
{
    BEGINSCOPE,
    ENDSCOPE,

    PUSH,
    PUSHOBJECT,
    PUSHNONE,
    POP,
    JMP, // jump to the instruction at the provided offset
    JZ, // jump if value on stack is false or 0
    JNZ, // jump if value on stack is true or 1
    SWAP,
    DUP,

    STORE,
    LOAD,
    RETURN,
    PROC,
    CALL,

    INDEX,
    CONCAT,
    ADD,
    SUB,
    MUL,
    DIV,
    IDIV,
    POW,
    MOD,
    UNM,

    BNOT,
    BAND,
    BOR,
    BXOR,
    BSHL,
    BSHR,

    AND,
    OR,
    NOT,
    LT,
    LTE,
    EQ,

    NOOP,
    EXIT
}