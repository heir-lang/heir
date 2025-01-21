namespace Heir.CodeGeneration;

public enum OpCode : byte
{
    /// <summary>Creates a new scope</summary>
    BEGINSCOPE,
    /// <summary>Ends the current scope and returns to the enclosing scope</summary>
    ENDSCOPE,

    /// <summary>Pushes the given value onto the stack</summary>
    /// <param name="operand">The value to push onto the stack</param>
    PUSH,
    /// <summary>Pushes an <see cref="Runtime.Values.ObjectValue"/> onto the stack</summary>
    /// <param name="operand">A <see cref="Dictionary"/> of bytecode keys &amp; bytecode values</param>
    PUSHOBJECT,
    /// <summary>Pushes none (null) onto the stack</summary>
    PUSHNONE,
    /// <summary>Pops the last value off of the stack</summary>
    POP,
    /// <summary>Jump ahead by the given offset</summary>
    /// <param name="operand">The amount of instructions to jump ahead</param>
    JMP,
    /// <summary>Jump ahead by the given offset if the last value on the stack is false or 0</summary>
    /// <param name="operand">The amount of instructions to jump ahead</param>
    JZ,
    /// <summary>Jump ahead by the given offset if the last value on the stack is true or 1</summary>
    /// <param name="operand">The amount of instructions to jump ahead</param>
    JNZ,
    /// <summary>Swaps the positions of the last 2 elements on the stack</summary>
    SWAP,
    /// <summary>Pushes the last element in the stack onto the stack again</summary>
    DUP,

    /// <summary>
    /// Stores a variable in the current scope using the last two frames of the stack.
    /// Of those last two frames, the first is the value and the second is the name.
    /// </summary>
    /// <param name="operand">Whether to push the value of the stored variable onto the stack (false by default)</param>
    /// <example>
    /// <para>This example is produced when executing <c>let abc = 5;</c>:</para>
    /// 
    /// <para>PUSH abc</para>
    /// PUSH 5
    /// <para>STORE</para>
    /// </example>
    STORE,
    /// <summary>
    /// Loads a variable from the current scope using the last frame in the stack.
    /// The frame it uses is the name of the variable to load.
    /// </summary>
    LOAD,
    /// <summary>
    /// If the call stack is not empty, returns to the last <see cref="CallStackFrame"/>.
    /// Otherwise, it advances.
    /// </summary>
    RETURN,
    /// <summary>Pushes a <see cref="Runtime.Values.FunctionValue"/> onto the stack</summary>
    /// <param name="operand">The body of the function as bytecode</param>
    PROC,
    /// <summary>Calls the last frame in the stack (assuming it's a <see cref="Runtime.Values.FunctionValue"/>)</summary>
    /// <param name="operand">
    /// A tuple; the first item being the amount of argument instructions that come after this instruction,
    /// and the second item being a list of all parameter names on the callee.
    /// </param>
    /// <example>
    /// <para>This example is produced when executing <c>fn double(x: int) -> x * 2; double(8);</c>:</para>
    /// <para/>
    /// PUSH double
    /// <para/>
    /// PROC (bytecode)
    /// BEGINSCOPE
    /// <para>PUSH x</para>
    /// LOAD
    /// <para>PUSH 2</para>
    /// MUL
    /// <para>RETURN</para>
    /// ENDSCOPE
    /// <para/>
    /// STORE False
    /// <para>PUSH double</para>
    /// LOAD
    /// <para>CALL (1, ["x"])</para>
    /// PUSH 8
    /// <para>EXIT</para>
    /// </example>
    CALL,

    /// <summary>Indexes the second to last frame in the stack using the last frame in the stack (assuming it's an <see cref="Runtime.Values.ObjectValue"/>)</summary>
    INDEX,
    /// <summary>Concatenates the last two frames in the stack together (assuming they're <see cref="string"/>s)</summary>
    CONCAT,
    /// <summary>Adds the last two frames in the stack together (assuming they're numbers)</summary>
    ADD,
    /// <summary>Subtracts the last two frames in the stack (assuming they're numbers)</summary>
    SUB,
    /// <summary>Multiplies the last two frames in the stack (assuming they're numbers)</summary>
    MUL,
    /// <summary>Divides the last two frames in the stack (assuming they're numbers)</summary>
    DIV,
    /// <summary>Divides then floors (integer division) the last two frames in the stack (assuming they're numbers)</summary>
    IDIV,
    /// <summary>Raises the last frame in the stack to the second to last frame in the stack's power (assuming they're numbers)</summary>
    POW,
    /// <summary>Calculates the modulus of the last two frames in the stack (assuming they're numbers)</summary>
    MOD,
    /// <summary>Negates the last frame in the stack (assuming it's a number)</summary>
    UNM,
    INC,
    DEC,

    /// <summary>Calculates binary not on the last frame in the stack (assuming it's an <see cref="long"/>)</summary>
    BNOT,
    /// <summary>Calculates binary and on the last two frames in the stack (assuming they're <see cref="long"/>s)</summary>
    BAND,
    /// <summary>Calculates binary or on the last two frames in the stack (assuming they're <see cref="long"/>s)</summary>
    BOR,
    /// <summary>Calculates binary exclusive or on the last two frames in the stack (assuming they're <see cref="long"/>s)</summary>
    BXOR,
    /// <summary>Calculates bit shift left on the last two frames in the stack (assuming they're <see cref="int"/>s)</summary>
    BSHL,
    /// <summary>Calculates bit shift right on the last two frames in the stack (assuming they're <see cref="int"/>s)</summary>
    BSHR,

    /// <summary>Calculates logical and on the last two frames in the stack (assuming they're <see cref="bool"/>s)</summary>
    AND,
    /// <summary>Calculates logical or on the last two frames in the stack (assuming they're <see cref="bool"/>s)</summary>
    OR,
    /// <summary>Calculates logical not on the last frame in the stack (assuming it's a <see cref="bool"/>)</summary>
    NOT,
    /// <summary>Calculates less than (LT) on the last two frames in the stack (assuming they're numbers)</summary>
    LT,
    /// <summary>Calculates less than or equal to (LTE) on the last two frames in the stack (assuming they're numbers)</summary>
    LTE,
    /// <summary>Calculates greater than (GT) on the last two frames in the stack (assuming they're numbers)</summary>
    GT,
    /// <summary>Calculates greater than or equal to (GTE) on the last two frames in the stack (assuming they're numbers)</summary>
    GTE,
    /// <summary>Calculates equality of the last two frames in the stack</summary>
    EQ,
    /// <summary>Calculates inequality of the last two frames in the stack</summary>
    NEQ,

    /// <summary>No operation, blank instruction</summary>
    NOOP,
    /// <summary>Signal to the <see cref="VirtualMachine"/> that the program has completed.</summary>
    EXIT
}