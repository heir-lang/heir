using Heir.Types;

namespace Heir.Runtime.Intrinsics;

public abstract class IntrinsicLibrary(string name, InterfaceType type)
    : IntrinsicValue<InterfaceType>(name, type, true);