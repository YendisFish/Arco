using System.Collections;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using Arco;
using Arco.Duplication;

ArcoDB db = new(25, 10);
Stopwatch w = new();

for(int i = 0; i < 1000000; i++)
{
    MyObj obj = new();
    db.Insert(obj);
}

w.Start();
MyObj[]? x4 = db.Query(new MyObj(), "id");
w.Stop();

Console.WriteLine(x4!.Length);
Console.WriteLine("Search time: " + w.Elapsed.TotalSeconds);

/*
w.Start();
for(int i = 0; i < 10000000; i++)
{
    MyObj obj = new();
    db.Insert(obj);
}
w.Stop();

Console.WriteLine("write time: " + w.Elapsed.TotalSeconds + "s");
w.Reset();

MyObj objs = new();
objs.a = 200;
db.Insert(objs);

MyObj obj2 = new();
obj2.id = objs.id;
obj2.a = 100;

w.Start();
MyObj? obj3 = db.Query(objs);
w.Stop();

Console.WriteLine("query time (searching by value not key): " + w.Elapsed.TotalSeconds + "s");
w.Reset();

w.Start();
MyObj? obj4 = db.QueryById(objs);
w.Stop();

Console.WriteLine("query time (searching by key): " + w.Elapsed.TotalMicroseconds + "ns");

db.SaveState();*/

class MyObj : IEnterable
{
    public ArcoId? id { get; set; } = new ArcoId(Guid.NewGuid().ToString());
    public string x = "Hello, World!";
    public int a = 5;
    public List<int> b = new List<int>() { 1 , 2, 3, 4, 5, 6, 7 };
}

class Test : IEnterable
{
    public ArcoId? id { get; set; } = new ArcoId(5023847);
    public int x = 100000;
}