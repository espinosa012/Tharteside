using System;
using Godot;
using Godot.Collections;
using Tartheside.mono.world.biomes;
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
    

    public River(Vector2I worldSize, Vector2I chunkSize, Vector2I offset, int nTiers) 
        : base(worldSize, chunkSize, offset, nTiers)
    {
        _rivers = new Array<RiverEntity>();
        PathfindingAStarSetup();
    }

    public void PathfindingAStarSetup() => _pathfindingAStar = new RiverTAStar(Offset, 
        Offset + WorldSize, _elevation, _riverPathfindingElevationPenalty);

    // Generating rivers

    public void GenerateRiver(Vector2I birthPos)
    {
        var riverEntity = new RiverEntity();
        riverEntity.SetBirthPosition(birthPos.X, birthPos.Y);

        // TODO: randomizar mediante el enfoque de pequeñas variaciones tanto "r" como "alpha"
        // TODO: en A*, comprobar que los puntos del río están dentro de los límites.
        // TODO: considerar constraints de elevaciones
        // TODO: limpiar 
        var r = 8;
        var randomAlpha = (float) (new MathNet.Numerics.Random.SystemRandomSource()).NextDouble() 
                          * 2 * MathNet.Numerics.Constants.Pi;
        
        var currentPoint = birthPos;
        while (!Biome.IsSea(_elevation, currentPoint.X - Offset.X, currentPoint.Y - Offset.Y)) // TODO: cuidado offset 
        {
            var nextPoint = currentPoint + new Vector2I((int)Math.Round(r * MathNet.Numerics.Trig.Cos(randomAlpha)),
                (int)Math.Round(r * MathNet.Numerics.Trig.Sin(randomAlpha)));
            foreach (var point in _pathfindingAStar.GetPath(currentPoint, nextPoint))
            {
                if (Biome.IsSea(_elevation, point.X - Offset.X, point.Y - Offset.Y)) break;
                riverEntity.AddPoint(point);
                SetValueAt(point.X, point.Y, TrueValue);
            }
            currentPoint = nextPoint;
        }
        _rivers.Add(riverEntity);
    }

    private int RandomizeR(int r)
    {
        // TODO: "pequeña" variación aleatoria.
        return r;
    }
    
    private float RandomizeAlpha(float alpha)
    {
        // TODO: "pequeña" variación aleatoria.
        return alpha;
    }
        
    
    
    
    public bool IsValidRiverBirth(int x, int y)
    {
        return false;
    }
    
    
    
    //TODO crear funcion para calcular el caudal en cierta posición x,y
    
    


    // getters & setters
    public void SetParameterElevation(Elevation elevation) => _elevation = elevation;
    public void SetParameterRiverPathfindingElevationPenalty(float riverPathfindingElevationPenalty) =>
        _riverPathfindingElevationPenalty = riverPathfindingElevationPenalty;


    public override void Randomize()
    {
        // TODO: conociendo la posición de nacimiento de todos los ríos de _rivers, los volvemos a generar, de forma
        // que r y alpha serán distintos.
        var currentRivers = _rivers.Duplicate(); 
        _rivers.Clear();
        
        GetClearMatrix(WorldSize);
        foreach (var river in currentRivers)
            GenerateRiver(river.GetBirthPosition());
    }
}

