using System.Collections;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using Arco;
using Arco.Duplication;

ArcoDB db = new();

for(int i = 0; i < 1000000; i++)
{
    MyObj obj = new();
    db.Insert(obj);
}

MyObj objs = new();
objs.a = 200;
db.Insert(objs);

MyObj obj2 = new();
obj2.id = objs.id;
obj2.a = 100;

Stopwatch w = new();

w.Start();
MyObj? obj3 = db.Query(objs);
w.Stop();

Console.WriteLine(obj3!.a);
Console.WriteLine(w.Elapsed.Nanoseconds);

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