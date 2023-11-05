using Godot;
using System;
using Tartheside.mono.utilities.pathfinding;

public partial class RiverTAstar : TAStar
{
    private Elevation _elevation;
    
    public RiverTAstar(Vector2I regionOrigin, Vector2I regionEnd, Elevation elevation = null) : base(regionOrigin, regionEnd)
    {
        if (elevation != null)
            _elevation = elevation;
    }

    public override float _ComputeCost(Vector2I fromId, Vector2I toId)
    {
        return 1.75f * _elevation.GetValueAt(toId.X, toId.Y);   // TODO: parametrizar
    }
}
