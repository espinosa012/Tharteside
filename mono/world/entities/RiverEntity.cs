using Godot;
using Godot.Collections;

namespace Tartheside.mono.world.entities;

public partial class RiverEntity : BaseEntity
{
    private string _name;
    
    private Vector2I _birthPosition;
    private Vector2I _mouthPosition;

    private Array<Vector2I> _riverPath = new Array<Vector2I>();
    

    public void AddPoint(Vector2I point) => _riverPath.Add(point);

    public bool ContainsPoint(Vector2I point) => _riverPath.Contains(point);

    public int GetPointsCount() => _riverPath.Count;
    
    
    // getters and setters
    public Vector2I GetBirthPosition() => _birthPosition;
    public void SetBirthPosition(int x, int y) => _birthPosition = new Vector2I(x, y);
    public Vector2I GetMouthPosition() => _mouthPosition;
    public void SetMouthPosition(int x, int y) => _mouthPosition = new Vector2I(x, y);

    public Array<Vector2I> GetRiverPath() => _riverPath;
}