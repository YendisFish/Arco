using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Arco.Snapshotting;

public static class Snapshotter
{
    private static Task? currentTask { get; set; }
    public static bool isSaving { get; set; } = false;

    public static void Snapshot(ArcoDB db)
    {
        if(!isSaving)
        {
            Task t = new Task(() => ThreadSnapshot(db));
            currentTask = t;
            t.Start();   
        }
    }
    
    internal static void ThreadSnapshot(ArcoDB db)
    {
        isSaving = true;
        
        if(!Directory.Exists("./arcodb/"))
        {
            Directory.CreateDirectory("./arcodb/");
        }
        
        foreach(string table in db.dbMap.Keys)
        {
            if(!File.Exists($"./arcodb/arco_{table}.bson"))
            {
                using(FileStream stream = File.Create($"./arcodb/arco_{table}.bson")) using(BsonWriter writer = new(stream))
                {
                    JsonSerializer serializer = new();
                    serializer.Serialize(writer, db.dbMap[table]);
                }
            } else {
                using(FileStream stream = File.OpenWrite($"./arcodb/arco_{table}.bson")) using(BsonWriter writer = new(stream))
                {
                    JsonSerializer serializer = new();
                    serializer.Serialize(writer, db.dbMap[table]);
                }
            }
        }

        isSaving = false;
    }
}