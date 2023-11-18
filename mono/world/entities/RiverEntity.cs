using Godot;
using Godot.Collections;

namespace Tartheside.mono.world.entities;

public partial class RiverEntity : WorldEntity
{
    public string Name;
    
    private Vector2I _birthPosition;
    private Vector2I _mouthPosition;

    private Array<Vector2I> _riverPath = new Array<Vector2I>();
    

    public void AddPoint(Vector2I point)
    {
        _riverPath.Add(point);
    }

    public bool ContainsPoint(Vector2I point) => _riverPath.Contains(point);

    public int GetPointsCount() => _riverPath.Count;
    
    
    // getters and setters
    public void SetBirthPosition(int x, int y) => _birthPosition = new Vector2I(x, y);
    public void SetMouthPosition(int x, int y) => _mouthPosition = new Vector2I(x, y);

}