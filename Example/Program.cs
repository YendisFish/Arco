using System.Collections;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Arco;
using Arco.Duplication;

ArcoDB db = new(25, 10);
Stopwatch w = new();

for(int i = 0; i < 100; i++)
{
    Obj obj = new();
    //Console.WriteLine(obj.id.raw);
    db.Insert(obj);
}

Obj ob = new Obj();
//ob.a = 100;
db.Insert(ob);

Console.WriteLine("Inserted");

w.Start();
Obj[]? x4 = db.ReverseLookupQuery(ob, "id");
w.Stop();

Console.WriteLine(x4.Length);
Console.WriteLine(x4[0].id!.raw);
Console.WriteLine("Search time: " + w.Elapsed.TotalSeconds + " seconds");

class MyObj : IEnterable
{
    public ArcoId? id { get; set; } = ArcoId.Default;
    public string x = "Hello, World!";
    public int a = 5;
    public List<int> b = new List<int>() { 1 , 2, 3, 4, 5, 6, 7 };
}

class Test : IEnterable
{
    public ArcoId? id { get; set; } = new ArcoId(5023847);
    public int x = 100000;
}

class Obj : IEnterable
{
    public ArcoId? id { get; set; } = ArcoId.Default;
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