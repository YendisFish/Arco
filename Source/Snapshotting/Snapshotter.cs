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

    public static void WaitForSaveable(ArcoDB db)
    {
        while (isSaving) ;
        
        Task t = new Task(() => ThreadSnapshot(db));
        currentTask = t;
        t.Start(); 
    }
    
    internal static void ThreadSnapshot(ArcoDB db)
    {
        isSaving = true;

        lock(db.dbMap)
        {
            if(!Directory.Exists("./arcodb/"))
            {
                Directory.CreateDirectory("./arcodb/");
            }
        
            if(!File.Exists($"./arcodb/arco.bson"))
            {
                using(FileStream stream = File.Create($"./arcodb/arco.bson")) using(BsonDataWriter writer = new(stream))
                {
                    JsonSerializer serializer = new JsonSerializer() { TypeNameHandling = TypeNameHandling.All };
                    serializer.Serialize(writer, db.dbMap);
                }
                
                using(FileStream stream = File.Create($"./arcodb/arcoReverse.bson")) using(BsonDataWriter writer = new(stream))
                {
                    JsonSerializer serializer = new JsonSerializer() { TypeNameHandling = TypeNameHandling.All };
                    serializer.Serialize(writer, db.reverseLookup);
                }
            } else {
                using(FileStream stream = File.OpenWrite($"./arcodb/arco.bson")) using(BsonDataWriter writer = new(stream))
                {
                    JsonSerializer serializer = new JsonSerializer() { TypeNameHandling = TypeNameHandling.All };
                    serializer.Serialize(writer, db.dbMap);
                }
                
                using(FileStream stream = File.OpenWrite($"./arcodb/arcoReverse.bson")) using(BsonDataWriter writer = new(stream))
                {
                    JsonSerializer serializer = new JsonSerializer() { TypeNameHandling = TypeNameHandling.All };
                    serializer.Serialize(writer, db.reverseLookup);
                }
            }
        }
        
        isSaving = false;
    }
}