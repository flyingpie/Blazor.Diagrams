using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Blazor.Diagrams.Core.Models.Base;

[JsonObject(MemberSerialization.OptIn)]
public abstract class Model
{
    private bool _visible = true;
    
    protected Model() : this(Guid.NewGuid().ToString()) { }

    protected Model(string id)
    {
        Id = id;
    }

    public event Action<Model>? Changed;
    public event Action<Model>? VisibilityChanged;

    [JsonInclude]
    public string Id { get; }

    [JsonInclude]
    public bool Locked { get; set; }

    [JsonInclude]
    public bool Visible
    {
        get => _visible;
        set
        {
            if (_visible == value)
                return;

            _visible = value;
            VisibilityChanged?.Invoke(this);
        }
    }

    public virtual void Refresh() => Changed?.Invoke(this);
}
