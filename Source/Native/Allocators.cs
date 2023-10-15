using System.Runtime.CompilerServices;

namespace Arco.Native;

public unsafe class Allocators
{
    public static Memory<T> GenericAllocate<T>()
    {
        Memory<T> ret = new Memory<T>(new T[0]);
        return ret;
    }
}