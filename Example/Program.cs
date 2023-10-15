using Arco;

ArcoDB db = new();

MyObj obj = new MyObj();
db.Insert(obj);

MyObj obj2 = db.Query(obj);
Console.WriteLine(obj2.x);

Test test = new Test();
db.Insert(test);

Test test2 = db.Query(test);
Console.WriteLine(test2.x);

class MyObj : IEnterable
{
    public Guid id { get; set; } = Guid.NewGuid();
    public string x = "Hello, World!";
}

class Test : IEnterable
{
    public Guid id { get; set; } = Guid.NewGuid();
    public int x = 5;
}