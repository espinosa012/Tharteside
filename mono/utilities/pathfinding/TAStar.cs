using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class TAStar : GodotObject
{
    private TPathfindingNode _origin;
    private TPathfindingNode _target;
    private TPathfindingNode _current;

    private bool _targetFound;
    
    private List<TPathfindingNode> _openNodes; 
    public List<TPathfindingNode> Path;


    public TAStar(Vector2I origin, Vector2I target)
    {
        _openNodes = new List<TPathfindingNode>();
        _targetFound = false;
        Path = new List<TPathfindingNode>();
        _target = new TPathfindingNode(new Vector2I(target.X, target.Y), null, null);
        _origin = new TPathfindingNode(new Vector2I(origin.X, origin.Y), null, _target);
        
        
        Path.Add(_origin);
    }


    public void FindPath()
    {

        while (true)
        {
            
        }
    }
    
    
    
    public void FindPath_()
    {
        // A* elige siempre la celda vecina con menor coste F
        _current = Path[0];
        List<TPathfindingNode> neighbours;
        while (!_targetFound)
        {
            // 1. Evaluamos todas las celdas vecinas de la celda actual
            neighbours = _current.GetNeighbours();
            foreach (TPathfindingNode neighbour in neighbours)
                OpenNode(neighbour);    // las incluimos en el registro de celdas abiertas        
            
            
            
            // 2. Seleccionamos la de menor coste F (que sea navegable)
            var nextNode = SelectNextNode(neighbours);
            Path.Add(nextNode);
            
            // 3. Comprobamos si hemos llegado al objetivo
            if (nextNode.IsEqual(_target))
                _targetFound = true;

            _current = nextNode;

        }
    }

    public TPathfindingNode SelectNextNode(List<TPathfindingNode> possibleNodes)
    {
        // falta comprobar que sea navegable. Además, el nodo devuelto estará en la lista de nodos abiertos
        return possibleNodes.OrderBy(node => node.GetF()).ToList()[0]; //mejorar: si dos tienen el mismo F, se considera el de H más bajo
    }

    public void OpenNode(TPathfindingNode nodeToOpen)
    {
        _openNodes.Add(nodeToOpen);
        _openNodes = _openNodes.OrderBy(node => node.GetF()).ToList();
    }       
    

    public bool IsWalkable(int x, int y) => true;
    public bool IsObstacle(int x, int y) => false;


}
