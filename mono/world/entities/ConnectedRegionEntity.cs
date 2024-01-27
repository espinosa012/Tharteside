using System.Collections.Generic;
using Godot;

namespace Tartheside.mono.world.entities;

public partial class ConnectedRegionEntity : BaseEntity
{
    private List<Vector2I> _positions = new List<Vector2I>();
    private Vector2I _centroid;
    private int _islandSize;



    public void SetPositions(List<Vector2I> positions) => _positions = positions;
    public List<Vector2I> GetPositions() => _positions;

}