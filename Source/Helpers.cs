namespace Arco;

public static class Helpers
{
    public static int GetHash(Type t)
    {
        int ret = 5381;

        foreach(char c in t.Name)
        {
            ret = ((ret << 5) + ret) ^ ret;
        }
        
        return ret;
    }
}