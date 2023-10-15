using System.IO.MemoryMappedFiles;
using Arco.Native;

namespace Arco;

public class ArcoDB
{
    internal Dictionary<int, Dictionary<int, IEnterable>> dbMap { get; set; } = new();
    
    public void Insert<T>(T obj) where T: IEnterable
    {
        int hash = typeof(T).Name.GetHashCode();
        
        if(dbMap.ContainsKey(hash))
        {
            dbMap[hash].Add(obj.id.hash, obj);
        } else
        {
            dbMap.Add(hash, new());
            Dictionary<int, IEnterable> collectionMap = dbMap[hash];
            collectionMap.Add(obj.id.hash, obj);
        }
    }
    
    public T Query<T>(T template) where T: IEnterable
    {
        int hash = typeof(T).Name.GetHashCode();
        if(dbMap.ContainsKey(hash))
        {
            return (T)dbMap[hash][template.id.hash];
        } else
        {
            throw new Exception("Type of given search object not found!");
        }

        return default(T);
    }
    
    public void SaveState() { }
}