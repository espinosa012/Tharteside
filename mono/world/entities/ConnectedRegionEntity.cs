using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Tartheside.mono.world.entities;

public partial class ConnectedRegionEntity : BaseEntity
{
    private List<Vector2I> _positions = new List<Vector2I>();
    private Vector2I _centroid;
    private int _islandSize;



    public void SetPositions(List<Vector2I> positions) => _positions = positions;
    public List<Vector2I> GetPositions() => _positions;

    public void SetIslandSize(int size) => _islandSize = size;
    public int GetRegionSize() => _islandSize;


    public void ComputeCentroid() => _centroid = new Vector2I(_positions.Sum(v => v.X) / _positions.Count, 
            _positions.Sum(v => v.Y)/ _positions.Count);

    public Vector2I GetCentroid() => _centroid;

}