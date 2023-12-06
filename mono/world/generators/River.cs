using Godot;
using Godot.Collections;
using Tartheside.mono.utilities.random;
using Tartheside.mono.world.entities;

namespace Tartheside.mono.world.generators;

public partial class River : WorldGenerator
{
    private Elevation _elevation;
    private float _riverPathfindingElevationPenalty;
    private MFNL _continentalness;
    
    private RiverTAStar _pathfindingAStar;
    private Array<RiverEntity> _rivers;

    private const float TrueValue = 0.999f;
    
    // TODO: hacer limpieza, QUITAR LO QUE NO SE VAYA A USAR y eliminar funciones, etc

    public River(int matrixSizeX, int matrixSizeY) : base(matrixSizeX, matrixSizeY)
    {
        _rivers = new Array<RiverEntity>();
        PathfindingAStarSetup();
    }

    //TODO crear funcion para calcular el caudal en cierta posición x,y
    public void GenerateRiverAStar(Vector2I birthPos, Vector2I mouthPos)
    {
        var riverEntity = new RiverEntity();
        riverEntity.SetBirthPosition(birthPos.X, birthPos.Y);
        riverEntity.SetMouthPosition(mouthPos.X, mouthPos.Y);
        foreach (var point in _pathfindingAStar.GetPath(birthPos, mouthPos))
        {
            riverEntity.AddPoint(point);
            SetValueAt(point.X, point.Y, TrueValue);
        }
        _rivers.Add(riverEntity);
        // TODO: añadir puntos a la matriz y ya no haría falta sobrescribir GetValueAt
    }

    // TODO public void Randomize() => ;
    
    // getters & setters
    public void SetParameterElevation(Elevation elevation) => _elevation = elevation;
    
    public void SetParameterContinentalness(MFNL continentalness) => _continentalness = continentalness;

    public void SetParameterRiverPathfindingElevationPenalty(float riverPathfindingElevationPenalty) =>
        _riverPathfindingElevationPenalty = riverPathfindingElevationPenalty;
    
    public void PathfindingAStarSetup() => _pathfindingAStar = new RiverTAStar(_offset, 
        _offset + _worldSize, _elevation, _riverPathfindingElevationPenalty);
     

}

