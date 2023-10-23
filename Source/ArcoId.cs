namespace Arco;

public class ArcoId
{
    public string raw { get; set; }

    public static ArcoId Default => new ArcoId(Guid.NewGuid().ToString());
    
    public ArcoId(string id)
    {
        raw = id;
    }

    public ArcoId(long id)
    {
        raw = id + "";
    }
    
    public ArcoId(DateTime id, string format)
    {
        raw = id.ToString(format);
    }
}