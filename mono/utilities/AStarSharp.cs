//https://github.com/davecusatis/Prototype/blob/master/prototype/Engine/AI/Astar.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace prototype.Engine.AI
{
    public class Cell
    {
        // Change this depending on what the desired size is for each element in the grid
        public static int CELLSIZE = 16;   // tile size (Creo)
        public Cell Parent;
        public Vector2 Position;
        public Vector2 Center
        {
            get
            {
                return new Vector2(Position.X + CELLSIZE / 2, Position.Y + CELLSIZE / 2);
            }
        }
        public float DistanceToTarget;
        public float Cost;
        public float F
        {
            get
            {
                if (DistanceToTarget != -1 && Cost != -1)
                    return DistanceToTarget + Cost;
                else
                    return -1;
            }
        }
        public bool Walkable;

        public Cell(Vector2 pos, bool walkable)
        {
            Parent = null;
            Position = pos;
            DistanceToTarget = -1;
            Cost = 1;
            Walkable = walkable;
        }
    }

    public class Astar
    {
        List<List<Cell>> Grid;
        int GridRows
        {
            get
            {
               return Grid[0].Count;
            }
        }
        int GridCols
        {
            get
            {
                return Grid.Count;
            }
        }

        public Astar(List<List<Cell>> grid)
        {
            Grid = grid;
        }

        public Stack<Cell> FindPath(Vector2 Start, Vector2 End)
        {
            Cell start = new Cell(new Vector2((int)(Start.X/Cell.CELLSIZE), (int) (Start.Y/Cell.CELLSIZE)), true);
            Cell end = new Cell(new Vector2((int)(End.X / Cell.CELLSIZE), (int)(End.Y / Cell.CELLSIZE)), true);

            Stack<Cell> Path = new Stack<Cell>();
            List<Cell> OpenList = new List<Cell>();
            List<Cell> ClosedList = new List<Cell>();
            List<Cell> adjacencies;
            Cell current = start;
           
            // add start node to Open List
            OpenList.Add(start);

            while(OpenList.Count != 0 && !ClosedList.Exists(x => x.Position == end.Position))
            {
                current = OpenList[0];
                OpenList.Remove(current);
                ClosedList.Add(current);
                adjacencies = GetAdjacentNodes(current);

 
                foreach(Cell n in adjacencies)
                {
                    if (!ClosedList.Contains(n) && n.Walkable)
                    {
                        if (!OpenList.Contains(n))
                        {
                            n.Parent = current;
                            n.DistanceToTarget = Math.Abs(n.Position.X - end.Position.X) + Math.Abs(n.Position.Y - end.Position.Y);
                            n.Cost = 1 + n.Parent.Cost;
                            OpenList.Add(n);
                            OpenList = OpenList.OrderBy(node => node.F).ToList<Cell>();
                        }
                    }
                }
            }
            
            // construct path, if end was not closed return null
            if(!ClosedList.Exists(x => x.Position == end.Position))
            {
                return null;
            }

            // if all good, return path
            Cell temp = ClosedList[ClosedList.IndexOf(current)];
            while(temp.Parent != start && temp != null)
            {
                Path.Push(temp);
                temp = temp.Parent;
            }
            return Path;
        }

        private List<Cell> GetAdjacentNodes(Cell n)
        {
            List<Cell> temp = new List<Cell>();

            int row = (int)n.Position.Y;
            int col = (int)n.Position.X;

            if(row + 1 < GridRows)
            {
                temp.Add(Grid[col][row + 1]);
            }
            if(row - 1 >= 0)
            {
                temp.Add(Grid[col][row - 1]);
            }
            if(col - 1 >= 0)
            {
                temp.Add(Grid[col - 1][row]);
            }
            if(col + 1 < GridCols)
            {
                temp.Add(Grid[col + 1][row]);
            }

            return temp;
        }
    }
}