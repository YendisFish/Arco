using DeepEqual.Syntax;
using Force.DeepCloner;

namespace Arco.Duplication;

public static class Cloner
{
    public static T Clone<T>(T obj) where T: IEnterable
    {
        return obj.DeepClone();
    }
}