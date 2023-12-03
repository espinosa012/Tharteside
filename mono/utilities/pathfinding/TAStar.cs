using System;
using Godot;
using Godot.Collections;

namespace Tartheside.mono.utilities.pathfinding;

public partial class TAStar : AStarGrid2D
{
    // con set_point_weight_scale y _compute_cost (x, y) aplicamos el g cost (barro, etc)
    public TAStar(Vector2I regionOrigin, Vector2I regionEnd)
    {
        DefaultEstimateHeuristic = Heuristic.Manhattan;
        DiagonalMode = DiagonalModeEnum.Never;
        ChangeRegion(regionOrigin, regionEnd);
    }

    public override float _ComputeCost(Vector2I fromId, Vector2I toId)
    {
        return 1.0f;    // for testing
    }

    // Region
    private Rect2I GetRegionByOriginAndEndPositions(Vector2I regionOrigin, Vector2I regionEnd) => 
        new Rect2I(regionOrigin, Math.Abs(regionEnd.X - regionOrigin.X), Math.Abs(regionEnd.Y - regionOrigin.Y));
 
    private void ChangeRegion(Rect2I newRegion)
    {
        Region = newRegion;
        Update();
    }

    public void ChangeRegion(Vector2I origin, Vector2I end) => 
        ChangeRegion(GetRegionByOriginAndEndPositions(origin, end));
    
    // Obstacles
    public void AddObstacle(int x, int y) => AddObstacle(new Vector2I(x, y));
    
    public void AddObstacle(Vector2I pos) => SetPointSolid(pos);

    public void RemoveObstacle(int x, int y) => RemoveObstacle(new Vector2I(x, y));
    
    public void RemoveObstacle(Vector2I pos) => SetPointSolid(pos, false);
    

    // Path
    public Array<Vector2I> GetPath(Vector2I origin, Vector2I target)
    {
        Array<Vector2I> path;
        try
        {
            path = GetIdPath(origin, target);
        }
        catch (Exception e)
        {
            path = null;
        }
        return path;
    }
    
    
}