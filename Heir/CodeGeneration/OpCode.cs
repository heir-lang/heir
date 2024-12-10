namespace Heir.CodeGeneration
{
    public enum OpCode : byte
    {
        PUSH,
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

        // BSHL,
        // BSHR,
        BNOT,
        BAND,
        BOR,
        BXOR,

        AND,
        OR,
        NOT,
        LT,
        LTE,
        EQ,

        NOOP
    }
}
