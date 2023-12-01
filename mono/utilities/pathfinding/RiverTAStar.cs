using Godot;
using Tartheside.mono.utilities.pathfinding;
using Tartheside.mono.world.generators;

public partial class RiverTAStar : TAStar
{
    private Elevation _elevation;
    public float ElevationPenalty;
    
    public RiverTAStar(Vector2I regionOrigin, Vector2I regionEnd, Elevation elevation, 
        float elevationPenalty) : base(regionOrigin, regionEnd)     // TODO: parametrizar ElevationPenalty
    {
        ElevationPenalty = elevationPenalty;
        if (elevation != null)
            _elevation = elevation;
    }

    public override float _ComputeCost(Vector2I fromId, Vector2I toId) => 
        ElevationPenalty * _elevation.GenerateValueAt(toId.X, toId.Y);
}
