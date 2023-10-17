using System.Diagnostics;
using System.Dynamic;
using System.IO.MemoryMappedFiles;
using System.Reflection;
using Arco.Duplication;
using Arco.Native;
using Arco.Snapshotting;
using DeepEqual.Syntax;
using Force.DeepCloner;

namespace Arco;

public class ArcoDB
{
    internal Dictionary<string, Dictionary<string, IEnterable>> dbMap { get; set; } = new();
    internal int modificationCount { get; set; } = 0;
    internal int saveFrequency { get; set; } = 25;

    public ArcoDB(int sf = 25)
    {
        saveFrequency = sf;
    }

    public void Insert<T>(T obj) where T: IEnterable
    {
        lock(dbMap)
        {
            string key = typeof(T).Name;
        
            if(obj.id == null) { throw new NullReferenceException("Id was null"); }
        
            if(dbMap.ContainsKey(key))
            {
                //vv can this be removed? vv
                T newObj = obj.DeepClone(); // deep clone so changes in the database dont reflect in the program
                if(dbMap[key].ContainsKey(obj.id.raw))
                {
                    dbMap[key][obj.id.raw] = newObj;
                } else {
                    dbMap[key].Add(obj.id.raw, newObj);
                }
            } else {
                dbMap.Add(key, new());
                Dictionary<string, IEnterable> collectionMap = dbMap[key];
                collectionMap.Add(obj.id.raw, obj);
            }   
        }
        
        if(modificationCount == saveFrequency)
        {
            SaveState();
            modificationCount = 0;
            return;
        }
        
        modificationCount = modificationCount + 1;
    }
    
    public T? QueryById<T>(T template) where T: IEnterable
    {
        string key = typeof(T).Name;
        
        if(template.id == null) { throw new NullReferenceException("Id was null"); }
        
        if(dbMap.ContainsKey(key))
        {
            if(dbMap[key].ContainsKey(template.id.raw))
            {
                return (T)dbMap[key][template.id.raw];
            } else
            {
                return default(T);
            }
        } else {
            throw new Exception("Type of given search object not found!");
        }
    }
    
    public T? QueryById<T>(ArcoId id) where T: IEnterable
    {
        string key = typeof(T).Name;
        
        if(id == null) { throw new NullReferenceException("Id was null"); }
        
        if(dbMap.ContainsKey(key))
        {
            if(dbMap[key].ContainsKey(id.raw))
            {
                return (T)dbMap[key][id.raw];
            } else {
                return default(T);
            }
        } else {
            throw new Exception("Type of given search object not found!");
        }
    }

    public T? Query<T>(T obj, params string[] toIgnore) where T: IEnterable
    {
        string key = typeof(T).Name;
        if(dbMap.ContainsKey(key))
        {
            foreach(KeyValuePair<string, IEnterable> vals in dbMap[key])
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
    
    public T[]? QueryAll<T>(T obj, params string[] toIgnore) where T: IEnterable
    {
        List<T> ret = new();
        
        string key = typeof(T).Name;
        if(dbMap.ContainsKey(key))
        {
            foreach(KeyValuePair<string, IEnterable> vals in dbMap[key])
            {
                //build comparison
                CompareSyntax<T, IEnterable> comparison = obj.WithDeepEqual(vals.Value);
                foreach(string ignoreable in toIgnore)
                {
                    comparison = comparison.IgnoreProperty(Comparisons.CreateMemberExpression<T>(ignoreable));
                }

                if(comparison.Compare())
                {
                    ret.Add((T)(vals.Value.DeepClone()));
                }
            }
        } else
        {
            throw new Exception("Type of given search object not found!");
        }

        return ret.ToArray();
    }

    public AmbiguousData DeepQuery<T>(T obj)
    {
        // function meant to return an object with all its referenced foreign objects
        throw new NotImplementedException("DeepQuery is not yet implemented!");
    }

    public void SaveState()
    {
        Snapshotter.Snapshot(this);
    }
}