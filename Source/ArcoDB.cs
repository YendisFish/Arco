using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Reflection;
using Arco.Duplication;
using Arco.Native;
using DeepEqual.Syntax;
using Force.DeepCloner;

namespace Arco;

public class ArcoDB
{
    internal Dictionary<int, Dictionary<int, IEnterable>> dbMap { get; set; } = new();
    
    public void Insert<T>(T obj) where T: IEnterable
    {
        int hash = typeof(T).Name.GetHashCode();
        
        if(obj.id == null) { throw new NullReferenceException("Id was null"); }
        
        if(dbMap.ContainsKey(hash))
        {
            //vv can this be removed? vv
            //T newObj = obj.DeepClone(); // deep clone so changes in the database dont reflect in the program
            if(dbMap[hash].ContainsKey(obj.id.hash))
            {
                dbMap[hash][obj.id.hash] = obj;
            } else {
                dbMap[hash].Add(obj.id.hash, obj);
            }
        } else
        {
            dbMap.Add(hash, new());
            Dictionary<int, IEnterable> collectionMap = dbMap[hash];
            collectionMap.Add(obj.id.hash, obj);
        }
    }
    
    public T? QueryById<T>(T template) where T: IEnterable
    {
        int hash = typeof(T).Name.GetHashCode();
        
        if(template.id == null) { throw new NullReferenceException("Id was null"); }
        
        if(dbMap.ContainsKey(hash))
        {
            if(dbMap[hash].ContainsKey(template.id.hash))
            {
                return (T)dbMap[hash][template.id.hash];
            } else
            {
                return default(T);
            }
        } else {
            throw new Exception("Type of given search object not found!");
        }
    }

    public T? DeepQuery<T>(T obj, params string[] toIgnore) where T: IEnterable
    {
        int hash = typeof(T).Name.GetHashCode();
        if(dbMap.ContainsKey(hash))
        {
            foreach(KeyValuePair<int, IEnterable> vals in dbMap[hash])
            {
                //build comparison
                CompareSyntax<T, IEnterable> comparison = obj.WithDeepEqual(vals.Value);
                foreach(string ignoreable in toIgnore)
                {
                    comparison = comparison.IgnoreProperty(Comparisons.CreateMemberExpression<T>(ignoreable));
                }

                if(comparison.Compare())
                {
                    return (T)(vals.Value.DeepClone());
                }
            }
        } else
        {
            throw new Exception("Type of given search object not found!");
        }
        return default(T);
    }
    
    public void SaveState() { }
}