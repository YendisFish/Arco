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
    internal int modificationCount { get; set; } = 0;
    internal int saveFrequency { get; set; } = 25;
    internal int threadThreshold { get; set; }

    public ArcoDB(int SaveFrequency = 25, int ThreadThreashold = 8)
    {
        saveFrequency = SaveFrequency;
        threadThreshold = ThreadThreashold;
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