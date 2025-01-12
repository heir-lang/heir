namespace Heir.CodeGeneration
{
    public enum OpCode : byte
    {
        BEGINSCOPE,
        ENDSCOPE,

        PUSH,
        PUSHOBJECT,
        PUSHNONE,
        POP,
        JMP,
        JZ,
        JNZ,
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
}
