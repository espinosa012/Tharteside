using Godot;
using Godot.Collections;
using Tartheside.mono.world.entities;

namespace Tartheside.mono.world.generators;

// TODO: quizás con la Entity no haría falta un generador como tal.

public partial class River : BaseGenerator
{
    private Elevation _elevation;
    private float _riverPathfindingElevationPenalty;
    
    private RiverTAStar _pathfindingAStar;
    private Array<RiverEntity> _rivers;

    private const float TrueValue = 0.999f;
    
    // TODO: hacer limpieza, QUITAR LO QUE NO SE VAYA A USAR y eliminar funciones, etc

    public River(Vector2I worldSize, Vector2I chunkSize, Vector2I offset, int nTiers) 
        : base(worldSize, chunkSize, offset, nTiers)
    {
        _rivers = new Array<RiverEntity>();
        PathfindingAStarSetup();
    }

    public void PathfindingAStarSetup() => _pathfindingAStar = new RiverTAStar(Offset, 
        Offset + WorldSize, _elevation, _riverPathfindingElevationPenalty);
    
    //TODO crear funcion para calcular el caudal en cierta posición x,y
 
    public void GenerateRiver(Vector2I birthPos, Vector2I mouthPos)
    {
        // TODO: mejorar el algoritmo de generación (puntos intermedios, etc)
        var riverEntity = new RiverEntity();
        riverEntity.SetBirthPosition(birthPos.X, birthPos.Y);
        riverEntity.SetMouthPosition(mouthPos.X, mouthPos.Y);
        foreach (var point in _pathfindingAStar.GetPath(birthPos, mouthPos))
        {
            riverEntity.AddPoint(point);
            SetValueAt(point.X, point.Y, TrueValue);
        }
        _rivers.Add(riverEntity);
    }

    // TODO public void Randomize() => ;
    
    // getters & setters
    public void SetParameterElevation(Elevation elevation) => _elevation = elevation;
    public void SetParameterRiverPathfindingElevationPenalty(float riverPathfindingElevationPenalty) =>
        _riverPathfindingElevationPenalty = riverPathfindingElevationPenalty;
    
}

