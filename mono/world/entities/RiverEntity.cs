using Godot;
using Godot.Collections;

namespace Tartheside.mono.world.entities;

public partial class RiverEntity : BaseEntity
{
    private string _name;
    
    private Vector2I _birthPosition;
    private Vector2I _mouthPosition;
    private int _length;
    
    
    private Array<Vector2I> _riverPath = new Array<Vector2I>();
    

    public void AddPoint(Vector2I point) => _riverPath.Add(point);
    public void IncrementLength() => _length++;
    public bool ContainsPoint(Vector2I point) => _riverPath.Contains(point);
    
    
    // getters and setters
    public Vector2I GetBirthPosition() => _birthPosition;
    public void SetBirthPosition(int x, int y) => _birthPosition = new Vector2I(x, y);
    public Vector2I GetMouthPosition() => _mouthPosition;
    public void SetMouthPosition(Vector2I mouth) => _mouthPosition = mouth;
    public void SetLength(int len) => _length = len;

    
    public Array<Vector2I> GetRiverPath() => _riverPath;
    
}