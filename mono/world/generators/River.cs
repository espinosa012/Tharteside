using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Tartheside.mono.utilities.math;
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

        // TODO: en A*, comprobar que los puntos del río están dentro de los límites.
        // TODO: considerar constraints de elevaciones
        // TODO: limpiar 
        // TODO: trasladar funciones matemáticas
        // TODO: randomizar mediante el enfoque de pequeñas variaciones tanto "r" como "alpha"
        // en cada iteración del while (RandomizeInc o algo así).

        var mouthReached = false;
        var currentPoint = birthPos;
        var alpha = GetRandomAlpha();   //TODO: incializar alpha hacia donde la elevación sea menor

        var iterations = 0;
        const int maxIterations = 64; // TODO: que sea parámetro del generador
        
        while (!mouthReached)
        {
            /*  TODO: En cada iteración, necesitamos determinar en qué dirección se encuentra la menor elevación.*/
            
            alpha = RandomizeAlpha(alpha);
            var nextPoint = currentPoint + GetInc(GetRandomR(4, 15), alpha);
            foreach (var point in _pathfindingAStar.GetPath(currentPoint, nextPoint))
            {
                AddPointToRiverEntity(point, riverEntity);
                if (!Biome.IsSea(_elevation, point.X - Offset.X, point.Y - Offset.Y)) continue;
                mouthReached = true;
                break;
            }
            currentPoint = nextPoint;
            iterations++;
        } 
        var riverMouth = riverEntity.GetRiverPath().Last();
        riverEntity.SetMouthPosition(riverMouth.X, riverMouth.Y);   
        _rivers.Add(riverEntity);
    }


    private void AddPointToRiverEntity(Vector2I point, RiverEntity riverEntity)
    {
        riverEntity.AddPoint(point);
        SetValueAt(point.X, point.Y, TrueValue);
    }
    
    // TODO
    private static Vector2I GetInc(int r, float alpha) => new((int)Math.Round(r * MathNet.Numerics.Trig.Cos(alpha)),
            (int)Math.Round(r * MathNet.Numerics.Trig.Sin(alpha)));

    private int GetRandomR(int minValue, int maxValue) => MathDotNetHelper.GetRandomIntInRange(minValue, maxValue);    // TODO: implementar. los mínimos y máximos deben ser parámetros del generador
    
    private float GetRandomAlpha() => MathDotNetHelper.GetRandomFloatInRange(0, 2 * MathDotNetHelper.Pi);
    
    private Vector2I RandomizeInc(int r, float alpha) => new ((int)Math.Round(r * MathDotNetHelper.Cos(alpha)), 
            (int)Math.Round(r * MathDotNetHelper.Sin(alpha)));
    
    private int RandomizeR(int r)
    {
        // TODO: "pequeña" variación aleatoria.
        return r + MathDotNetHelper.GetRandomIntInRange(0, 8);
    }

    private float RandomizeAlpha(float alpha) =>    // TODO: el rango de variación de alpha podría ser un parámetro del generador
        alpha + MathDotNetHelper.GetRandomFloatInRange(-MathDotNetHelper.Pi / 6f, MathDotNetHelper.Pi / 6f);
    
    public bool IsValidRiverBirth(int x, int y)
    {
        return false;
    }
    
    
    
    //TODO crear funcion para calcular el caudal en cierta posición x,y
    
    


    // getters & setters
    public void SetParameterElevation(Elevation elevation) => _elevation = elevation;
    public void SetParameterRiverPathfindingElevationPenalty(float riverPathfindingElevationPenalty) =>
        _riverPathfindingElevationPenalty = riverPathfindingElevationPenalty;


    
    /* TODO:    necesitamos un método para volver a generar los mismos ríos (mismo nacimiento y desembocadura) pero con
                nuevo valor de ElevationPenalty. Lo metemos aquí, en UpdateElevationPenalty. */
    public void UpdateElevationPenalty(float newElevationPenalty)
    {
        SetParameterRiverPathfindingElevationPenalty(newElevationPenalty);
        foreach (var river in _rivers)
        {
            
        }
    }

    public override void Randomize()
    {
        // TODO: conociendo la posición de nacimiento de todos los ríos de _rivers, los volvemos a generar, de forma
        // que r y alpha serán distintos.
        var currentRivers = _rivers.Duplicate(); 
        _rivers.Clear();
        SetClearMatrix(WorldSize);
        foreach (var river in currentRivers)
            GenerateRiver(river.GetBirthPosition());
    }
    
}

