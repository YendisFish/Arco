namespace Arco;

public interface IEnterable
{
    public ArcoId? id { get; set; }
}

public class Enterable : IEnterable
{
    public ArcoId id { get; set; }
}