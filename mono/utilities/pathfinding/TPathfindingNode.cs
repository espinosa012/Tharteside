using Godot;
using System;
using System.Collections.Generic;

public partial class TPathfindingNode : GodotObject
{
    public int X { get; set; }
    public int Y { get; set; }
    public Vector2I Position => new Vector2I(X, Y);
    public TPathfindingNode Target;
    public int G { get; set; }
    public int Heuristic { get; set; }      // siempre menor o igual que la distancia mínima real (considerando obstáculos)
    public int F;   // G+H  
    public TPathfindingNode Parent { get; set; }


    public TPathfindingNode(Vector2I position, TPathfindingNode parent, TPathfindingNode target)
    {
        SetPosition(position);
        SetTarget(target);
        SetParent(parent);
        SetHeuristic(target);
        SetF();
    }

    public void SetTarget(TPathfindingNode target)
    {
        target ??= this;
        Target = target;
    }
    
    public void SetParent(TPathfindingNode parent)
    {
        Parent = parent;
        SetG();
    }
        

    
    public void SetPosition(Vector2I position)
    {
        X = position.X;
        Y = position.Y;
    }

    public void SetG() => 
        G = GetManhattanDistance(new Vector2I(X, Y), new Vector2I(Parent.X, Parent.Y));
    
    public void SetHeuristic(TPathfindingNode target) => 
        GetManhattanDistance(new Vector2I(target.X, target.Y), new Vector2I(X, Y));
    public void SetF() => F = G + Heuristic;

    public int GetF() => F;
    
    
    public bool IsEqual(TPathfindingNode node) => node.X == X && node.Y == Y;
    
    public Vector2I GetPositon() => Position;
    
    
    
    
    
    
    // heuristic functions
    public int GetManhattanDistance(Vector2I origin, Vector2I target)
    {
        return target.X - origin.X + target.Y - origin.Y;
    }

    public List<TPathfindingNode> GetNeighbours()
    {
        List<TPathfindingNode> toReturn = new List<TPathfindingNode>();
        foreach (Vector2I neighbour in new[] {Vector2I.Up, Vector2I.Down, Vector2I.Left, Vector2I.Right})
        {
            toReturn.Add(new TPathfindingNode(GetPositon() + neighbour, this, Target));
        }

        return toReturn;
    }
    
    
    
    
}
