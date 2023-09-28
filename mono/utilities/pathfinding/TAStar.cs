using System;
using Godot;
using Godot.Collections;

namespace Tartheside.mono.utilities.pathfinding;

public partial class TAStar : AStarGrid2D
{
    // con _compute_cost(x, y) aplicamos el g cost /barro( 
    public TAStar(Vector2I regionOrigin, Vector2I regionEnd)
    {
        DefaultEstimateHeuristic = Heuristic.Manhattan;
        DiagonalMode = DiagonalModeEnum.Never;
        
        ChangeRegion(GetRegionByOriginAndEndPositions(regionOrigin, regionEnd));
    }

    public override float _ComputeCost(Vector2I fromId, Vector2I toId)
    {
        return base._ComputeCost(fromId, toId);
    }

    // Region
    public Rect2I GetRegionByOriginAndEndPositions(Vector2I regionOrigin, Vector2I regionEnd)
    {
        return new Rect2I(regionOrigin, Math.Abs(regionEnd.X - regionOrigin.X),
            Math.Abs(regionEnd.Y - regionOrigin.Y));
    }

    public void ChangeRegion(Rect2I newRegion)
    {
        Region = newRegion;
        Update();
    }

    // Obstacles
    public void AddObstacle(Vector2I pos)
    {
        SetPointSolid(pos);
    }

    public void RemoveObstacle(Vector2I pos)
    {
        SetPointSolid(pos, false);
    }

    // Path
    public Array<Vector2I> GetPath(Vector2I origin, Vector2I target)
    {
        return GetIdPath(origin, target);
    }
    
    
}