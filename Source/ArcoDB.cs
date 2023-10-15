using System.IO.MemoryMappedFiles;
using Arco.Native;

namespace Arco;

public class ArcoDB
{
    internal Dictionary<int, Dictionary<Guid, IEnterable>> dbMap { get; set; } = new();
    
    public void Insert<T>(T obj) where T: IEnterable
    {
        int hash = typeof(T).Name.GetHashCode();
        if(dbMap.ContainsKey(hash))
        {
            dbMap[hash].Add(obj.id, obj);
        } else
        {
            dbMap.Add(hash, new());
            Dictionary<Guid, IEnterable> collectionMap = dbMap[hash];
            collectionMap.Add(obj.id, obj);
        }
    }
    
    public T Query<T>(T template) where T: IEnterable
    {
        int hash = typeof(T).Name.GetHashCode();
        if(dbMap.ContainsKey(hash))
        {
            return (T)dbMap[hash][template.id];
        } else
        {
            throw new Exception("Type of given search object not found!");
        }

        return default(T);
    }
    
    public void SaveState() { }
}