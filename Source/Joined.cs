namespace Arco;

public class Joined
{
    public JoinedInfo root { get; set; }

    public Joined(IEnterable obj1, IEnterable obj2)
    {
        root = new JoinedInfo(obj1);
        root.next = new JoinedInfo(obj2);
    }
}

public class JoinedInfo
{
    public JoinedInfo? previous { get; set; } = null;
    public IEnterable attached { get; set; }
    public JoinedInfo? next { get; set; } = null;

    public JoinedInfo(IEnterable value)
    {
        this.attached = value;
    }
}