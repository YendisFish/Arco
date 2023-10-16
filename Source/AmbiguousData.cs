namespace Arco;

public class AmbiguousData
{
    internal Dictionary<string, object> props { get; set; }

    public object this[string field]
    {
        get
        {
            if (props.ContainsKey(field))
            {
                return props[field];
            } else {
                props.Add(field, new object());
                return props[field];
            }
        }

        set
        {
            if (props.ContainsKey(field))
            {
                props[field] = value;
            } else {
                props.Add(field, new object());
                props[field] = value;
            }
        }
    }
    
    public AmbiguousData(ArcoId id)
    {
        props = new();
        props.Add("id", id);
    }

    internal AmbiguousData()
    {
        props = new();
    }

    public void AddField(string name, object value)
    {
        if(props.ContainsKey(name))
        {
            props[name] = value;
        } else
        {
            props.Add(name, value);
        }
    }

    public void CastObjectAddField(ref object obj, string name, string value)
    {
        Dictionary<string, object> prop = (Dictionary<string, object>)obj;
        
        if(prop.ContainsKey(name))
        {
            prop[name] = value;
        } else
        {
            prop.Add(name, value);
        }
    }

    public AmbiguousData GetObjectAsAmbiguous(ref object obj)
    {
        AmbiguousData ret = new();
        ret.props = (Dictionary<string, object>)obj;

        return ret;
    }

    public AmbiguousData? GetFieldAsAmbiguous(string field)
    {
        try
        {
            AmbiguousData ret = new();
            ret.props = (Dictionary<string, object>)props[field];

            return ret;
        } catch (Exception ex)
        {
            this.AddField(field, new Dictionary<string, object>());
            
            AmbiguousData ret = new();
            ret.props = (Dictionary<string, object>)props[field];

            return ret;
        }
    }
}