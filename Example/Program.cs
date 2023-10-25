using System.Collections;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Arco;
using Arco.Duplication;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

/*
Dictionary<string, Dictionary<string, IEnterable>> dict = new();
for(int i = 0; i < 1000; i++)
{
    dict.Add("asdfasd" + i, new Dictionary<string, IEnterable>());
    dict["asdfasd" + i].Add("asdfasdf" + i, new MyObj());
}

using(FileStream fs = File.Create("./myfile.bson")) using(BsonDataWriter wrt = new(fs))
{
    JsonSerializer s = new() { TypeNameHandling = TypeNameHandling.All };
    s.Serialize(wrt, dict);
}*/

/*
ArcoDB db = ArcoDB.Load();
//db.PrintScanR();

//MyObj[]? x = db.ReverseLookupQuery(new MyObj(), "id");

MyObj? obj = db.QueryById<MyObj>(new ArcoId("a8d4994b-989e-4d95-af1c-b061e7f2131e"));

MyObj[]? x = db.ReverseLookupQuery(obj);
MyObj[]? y = db.ReverseLookupQuery(new MyObj(), "id");


Console.WriteLine(JsonConvert.SerializeObject(obj));
Console.WriteLine(x.Length);

Console.WriteLine(JsonConvert.SerializeObject(obj));
Console.WriteLine(y.Length);

/**/


ArcoDB db = new(25, 10);
Stopwatch w = new();

for(int i = 0; i < 10000; i++)
{
    MyObj obj = new();
    //Console.WriteLine(obj.id.raw);
    db.Insert(obj);
}

MyObj ob = new MyObj();
//ob.a = 100;
db.Insert(ob);
ob.a = 100;
db.Insert(ob);

Console.WriteLine("Inserted");

w.Start();
MyObj[]? x4 = db.ReverseLookupQuery(ob, "id", "a");
w.Stop();

db.Snapshot();

Console.WriteLine(x4.Length);
Console.WriteLine(x4[0].id!.raw);
Console.WriteLine(x4[0].a);
Console.WriteLine("Search time: " + w.Elapsed.TotalSeconds + " seconds"); /**/

class MyObj : IEnterable
{
    public ArcoId? id { get; set; } = ArcoId.Default;
    public string x = "Hello, World!";
    public int a = 5;
    public List<int> b = new List<int>() { 1 , 2, 3, 4, 5, 6, 7 };
}

class Test : Enterable
{
    public ArcoId? id { get; set; } = new ArcoId(5023847);
    public int x = 100000;
}

class Obj : IEnterable
{
    public new ArcoId? id { get; set; } = ArcoId.Default;
    public byte[] x { get; set; }
    public int y { get; set; }
    public Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, int>>>>> dict { get; set; }

    public Obj()
    {
        x = new byte[100000];
        dict = new();
        
        for(int i = 0; i < 100000; i++)
        {
            x[i] = 0xFF;
        }

        for(int i = 0; i < 200; i++)
        {
            dict.Add(i + "", new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, int>>>>());
            dict[i + ""].Add(i + "", new Dictionary<string, Dictionary<string, Dictionary<string, int>>>());
            dict[i + ""][i + ""].Add(i + "", new Dictionary<string, Dictionary<string, int>>());
            dict[i + ""][i + ""][i + ""].Add(i + "", new Dictionary<string, int>());
            dict[i + ""][i + ""][i + ""][i + ""].Add(i + "", i);
        }
    }
}