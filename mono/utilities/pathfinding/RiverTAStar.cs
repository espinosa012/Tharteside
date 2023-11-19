using Godot;
using System;
using Tartheside.mono.utilities.pathfinding;

public partial class RiverTAStar : TAStar
{
    private Tartheside.mono.world.generators.Elevation _elevation;
    public float ElevationPenalty;
    
    public RiverTAStar(Vector2I regionOrigin, Vector2I regionEnd, Tartheside.mono.world.generators.Elevation elevation = null, 
        float elevationPenalty = 1.75f) : base(regionOrigin, regionEnd)     // TODO: parametrizar ElevationPenalty
    {
        ElevationPenalty = elevationPenalty;
        if (elevation != null)
            _elevation = elevation;
    }

    public override float _ComputeCost(Vector2I fromId, Vector2I toId) => 
        ElevationPenalty * _elevation.GetValueAt(toId.X, toId.Y);
}
