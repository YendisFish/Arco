using System.Reflection;
using Arco.Duplication;
using Arco.Snapshotting;
using DeepEqual.Syntax;
using Force.DeepCloner;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json;

namespace Arco;

public class ArcoDB
{
    internal Dictionary<string, Dictionary<string, IEnterable>> dbMap { get; set; } = new();
    internal Dictionary<string, Dictionary<string, List<IEnterable>>> reverseLookup { get; set; }

    internal int modificationCount { get; set; } = 0;
    internal int saveFrequency { get; set; } = 25;
    internal int threadThreshold { get; set; }

    public ArcoDB(int SaveFrequency = 25, int ThreadThreashold = 8)
    {
        saveFrequency = SaveFrequency;
        threadThreshold = ThreadThreashold;
        reverseLookup = new();
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

        Thread reverseThread = new(new ThreadStart(() => ReverseInsert(obj)));
        reverseThread.Start();
        
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
    
    public T[]? Query<T>(T obj, params string[] toIgnore) where T: IEnterable
    {
        List<T> ret = new();
        string key = typeof(T).Name;
        
        int parts = threadThreshold;
        Dictionary<int, Dictionary<string, IEnterable>> dictPartitions = dbMap[key]
            .Select((kv, index) => new { Index = index, Key = kv.Key, Value = kv.Value })
            .GroupBy(item => item.Index % parts)
            .ToDictionary(group => group.Key, group => group.ToDictionary(item => item.Key, item => item.Value));

        List<Task> tasks = new();
        foreach(Dictionary<string, IEnterable> dict in dictPartitions.Values)
        {
            Task t = new Task(() => ThreadedQueryAll<T>(obj, toIgnore, dict, ret));
            tasks.Add(t);
            t.Start();
        }

        Task.WaitAll(tasks.ToArray());

        return ret.ToArray();
    }

    internal void ThreadedQueryAll<T>(T obj, string[] toIgnore, Dictionary<string, IEnterable> toOperate, List<T> lst) where T: IEnterable
    {
        foreach(KeyValuePair<string, IEnterable> vals in toOperate)
        {
            //build comparison
            CompareSyntax<T, IEnterable> comparison = obj.WithDeepEqual(vals.Value);
            foreach(string ignoreable in toIgnore)
            {
                comparison = comparison.IgnoreProperty(Comparisons.CreateMemberExpression<T>(ignoreable));
            }

            if(comparison.Compare())
            {
                lock(lst)
                {
                    lst.Add((T)(vals.Value.DeepClone()));
                }
            }
        }
    }

    // most recommended query method! Is extremely fast and usable!
    public T[] ReverseLookupQuery<T>(T obj, params string[] toIgnore) where T: IEnterable
    {
        lock(reverseLookup)
        {
            List<T> ret = new();

            string type = typeof(T).Name;
            PropertyInfo[] props = typeof(T).GetProperties().Where(x => toIgnore.Contains(x.Name)).ToArray();

            foreach(PropertyInfo prop in props)
            {
                object? p = prop.GetValue(obj);
                string pStr = JsonConvert.SerializeObject(p);

                Dictionary<string, List<IEnterable>> vals = new();
                if(reverseLookup.TryGetValue(pStr, out vals))
                {
                    List<IEnterable> forCurrent = new();
                    if(vals.TryGetValue(type, out forCurrent))
                    {
                        foreach(IEnterable val in forCurrent)
                        {
                            CompareSyntax<T, IEnterable> comparison = obj.WithDeepEqual(val);

                            foreach(string ignoreable in toIgnore)
                            {
                                comparison = comparison.IgnoreProperty(Comparisons.CreateMemberExpression<T>(ignoreable));
                            }

                            if(comparison.Compare())
                            {
                                ret.Add((T)val);
                            }
                        }
                    }
                }
            }
            
            return ret.ToArray();
        }
    }

    internal void ReverseInsert<T>(T obj) where T: IEnterable
    {
        ReverseRemoveExists(obj);
        
        lock(reverseLookup)
        {
            string key = typeof(T).Name;

            PropertyInfo[] props = typeof(T).GetProperties();

            foreach(PropertyInfo prop in props)
            {
                object? val = prop.GetValue(obj);
                string strVal = JsonConvert.SerializeObject(val);

                Dictionary<string, List<IEnterable>>? vals;
                if(!reverseLookup.TryGetValue(strVal, out vals))
                {
                    reverseLookup.Add(strVal, new Dictionary<string, List<IEnterable>>());
                    reverseLookup[strVal].Add(key, new List<IEnterable>());

                    vals = reverseLookup[strVal];
                }

                if (vals is null) { throw new NullReferenceException(); }

                List<IEnterable>? enterables;
                if(!vals.TryGetValue(key, out enterables))
                {
                    vals.Add(key, new List<IEnterable>());
                    enterables = vals[key];
                }
                
                if(enterables is null) { throw new NullReferenceException(); }
                
                bool contains = false;
                for(int i = 0; i < enterables.Count; i++)
                {
                    if(enterables[i].id == obj.id)
                    {
                        enterables[i] = obj;
                        contains = true;
                        break;
                    }
                }

                if(!contains) { enterables.Add(obj); }
            }
        }
    }

    internal void ReverseRemoveExists<T>(T obj) where T: IEnterable
    {
        // remove an object from its locations in the reverse lookup table if it extists within it
        lock(reverseLookup) { }
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

    public static ArcoDB Load()
    {
        throw new NotImplementedException();
        
        ArcoDB db = new();
        foreach(FileInfo fle in new DirectoryInfo("./arcodb/").GetFiles())
        {
            string TypeName = fle.Name.Replace("arco_", "").Split('.')[0]; 

            using(FileStream fs = File.OpenRead(fle.FullName)) using(BsonReader rdr = new BsonReader(fs))
            {
                JsonSerializer serializer = new();
                db.dbMap.Add(TypeName, serializer.Deserialize<Dictionary<string, IEnterable>>(rdr)!);
            }
        }
        
        return db;
    }
}