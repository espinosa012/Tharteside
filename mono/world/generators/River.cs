using Godot;
using Godot.Collections;
using Tartheside.mono.world.entities;

namespace Tartheside.mono.world.generators;

public partial class River : WorldGenerator
{
    private Elevation _elevation;
    private utilities.random.MFNL _continentalness;
    
    private RiverTAStar _pathfindingAStar;
    private Vector2I _worldSize;
    private Array<RiverEntity> _rivers;

    
    // TODO: hacer limpieza, QUITAR LO QUE NO SE VAYA A USAR y eliminar funciones, etc
    
    public River(int matrixSizeX, int matrixSizeY) : base(matrixSizeX, matrixSizeY)
    {}
    
    public override float GetValueAt(int x, int y) 
    {
        float trueValue = 0.999f;
        float falseValue = -1.0f;
        foreach (var river in _rivers)
            for (int i = 0; i < river.GetPointsCount(); i++)
                if (river.ContainsPoint(new Vector2I(x, y)))
                    return trueValue;
        return falseValue;
    }

    //TODO crear funcion para calcular el caudal en cierta posición x,y
    public void GenerateRiverAStar(Vector2I birthPos, Vector2I mouthPos)
    {
        RiverEntity riverEntity = new RiverEntity();
        riverEntity.SetBirthPosition(birthPos.X, birthPos.Y);
        riverEntity.SetMouthPosition(mouthPos.X, mouthPos.Y);

        foreach (var point in _pathfindingAStar.GetPath(birthPos, mouthPos))
            riverEntity.AddPoint(point);
        _rivers.Add(riverEntity);
    }

    // TODO public void Randomize() => ;
    
    // getters & setters
    public Elevation GetParameterElevation() => _elevation;
    public void SetParameterElevation(Elevation elevation) => _elevation = elevation;
    
    public utilities.random.MFNL GetParameterContinentalness() => _continentalness;
    public void SetParameterContinentalness(utilities.random.MFNL continentalness) => _continentalness = continentalness;
    
    
    // TODO: deberiamos cear los parametros de tipo numericoo para formar el astar, no el propio astar
    public void SetPathfindingAstar(RiverTAStar pathfinding) => _pathfindingAStar = pathfinding;
    public RiverTAStar GetParameterPathfindingAstar() => _pathfindingAStar;

}

