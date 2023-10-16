namespace Arco;

public class Relatable<T> where T: IEnterable
{
    public ArcoId ForeignId { get; set; }
    internal int hash { get; set; }

    public Relatable(ArcoId value)
    {
        ForeignId = value;
        hash = typeof(T).Name.GetHashCode();
    }

    public T? GetReferencedValue(ref ArcoDB db)
    {
        return db.QueryById<T>(ForeignId);
    }
}