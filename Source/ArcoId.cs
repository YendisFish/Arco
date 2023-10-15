namespace Arco;

public unsafe struct ArcoId
{
    internal int hash { get; set; }

    public ArcoId(string id)
    {
        hash = id.GetHashCode();
    }

    public ArcoId(long id)
    {
        string nid = id + "";
        hash = nid.GetHashCode();
    }
    
    public ArcoId(DateTime id, string format)
    {
        string nid = id.ToString(format);
        hash = nid.GetHashCode();
    }
}