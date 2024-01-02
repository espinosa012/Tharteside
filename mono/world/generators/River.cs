using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using MathNet.Numerics;
using Tartheside.mono.utilities.math;
using Tartheside.mono.utilities.random;
using Tartheside.mono.world.biomes;
using Tartheside.mono.world.entities;
using Vector2I = Godot.Vector2I;

namespace Tartheside.mono.world.generators;

public partial class River : BaseGenerator
{
    private RiverTAStar _pathfindingAStar;
    private Array<RiverEntity> _rivers;

    private const float TrueValue = 0.999f;
    
    // Generators parameters
    private Elevation _elevation;
    private MFNL _continentalnessNoise;
    private float _riverPathfindingElevationPenalty;
    
    // TODO: inicializar en el manager y leer desde json
    private int _maxIterations = 256; 
    private int _minRValue = 2;
    private int _maxRValue = 6; 
    private float _minRandomAlpha = 0f;
    private float _maxRandomAlpha = 2f * MathDotNetHelper.Pi;
    private float _minAlphaVariation = 0f;
    private float _maxAlphaVariation = 2f * MathDotNetHelper.Pi;  // TODO: en el rango +-MathDotNetHelper.Pi/4f se consiguen resultados interesantes

    private RandomNumberGenerator _rng;
    
    public River(Vector2I worldSize, Vector2I chunkSize, Vector2I offset, int nTiers) 
        : base(worldSize, chunkSize, offset, nTiers)
    {
        _rivers = new Array<RiverEntity>();
        _rng = new RandomNumberGenerator();
        PathfindingAStarSetup();
    }

    public void PathfindingAStarSetup() => _pathfindingAStar = new RiverTAStar(Offset, 
        Offset + WorldSize, _elevation, _riverPathfindingElevationPenalty);

    public void SpawnRivers()
    {
        /*GenerateRiver(new Vector2I(86828, 846));    // es una isla, no debe fallar
        GenerateRiver(new Vector2I(86579, 854));    // se puede ir de los límites de A*
        GenerateRiver(new Vector2I(86956, 648));    // se puede ir de los límites de A*
        GenerateRiver(new Vector2I(86956, 648));    // se puede ir de los límites de A*
        GenerateRiver(new Vector2I(86977, 847));*/    
        
        const int iInc = 64;
        const int jInc = 64; // TODO: parametrizar. Divisor de WorldSize

        for (var i = Offset.X; i < WorldSize.X + Offset.X; i++)
        {
            for (var j = Offset.Y; j < WorldSize.Y + Offset.Y; j++)
            {
                var birthPos = new Vector2I(i, j);
                if (IsValidBirth(birthPos))
                {
                    //GD.Print(i, " ", j);
                    GenerateRiver(birthPos);
                }
                j += jInc;    
            }
            i += iInc;
        }
    }

    
    // Generating rivers
    public void GenerateRiver(Vector2I birthPos)
    {
        var riverEntity = new RiverEntity();
        riverEntity.SetBirthPosition(birthPos.X, birthPos.Y);

        // TODO: considerar constraints de elevaciones

        var mouthReached = false;
        var currentPosition = birthPos;
        var nextPoint = birthPos;

        var alpha = GetRandomAlpha();   //TODO: inicializar alpha hacia donde la elevación sea menor ??
        var iterations = 0;
        
        while (!mouthReached && _pathfindingAStar.ContainsPoint(nextPoint) && riverEntity.IsValid())
        {
            var r = GetRandomR(_minRValue, _maxRValue);
            alpha = GetDescendantAlpha(alpha, r, currentPosition);
            nextPoint = GetUpdatedNextPoint(currentPosition, r, alpha);
            
            foreach (var point in GetStepPath(currentPosition, nextPoint))
            {
                AddPointToRiverEntity(point, riverEntity);  // TODO: añadir los puntos a la matriz sólo si el río es validado
                if (!IsValidMouth(point)) continue;
                mouthReached = true;
                break;
            }
            currentPosition = nextPoint;
            iterations++;   // TODO: si pasamos del número máximo de iteraciones, el río no será válido 
        }

        if (ValidateRiver(riverEntity))
        {
            // TODO: los ríos no deben contener más de N veces el mismo punto (parámetro de validación)
            
            var riverMouth = riverEntity.GetRiverPath().Last(); // TODO: pasarle Vector2I a SetMouthPosition 
            riverEntity.SetMouthPosition(riverMouth.X, riverMouth.Y);   
            _rivers.Add(riverEntity);
            
            // TODO: Añadir los puntos a la matriz aquí, sólo si el río se valida (no en AddPointToRiverEntity())
            // untested
            var path = riverEntity.GetRiverPath();
            foreach (var point in path)
                SetValueAt(point.X, point.Y, TrueValue);    // TODO: quitar de aquí. Añadir puntos a la matriz cuando estemos seguros de que el río es válido.
        }
    }

    private static Vector2I GetUpdatedNextPoint(Vector2I currentPosition, int r, float alpha) =>
        currentPosition + GetInc(r, alpha);
    
    private Vector2I GetSafeNextPoint(Vector2I nextPoint)
    {
        var nextPoint_safe = new Vector2I(Math.Min(nextPoint.X, WorldSize.X + Offset.X-1), 
            Math.Min(nextPoint.Y, WorldSize.Y + Offset.Y-1));        
        return new Vector2I(Math.Max(Offset.X, nextPoint_safe.X), Math.Max(Offset.Y, nextPoint_safe.Y));
    }

    private Array<Vector2I> GetStepPath(Vector2I currentPosition, Vector2I nextPoint) => 
        _pathfindingAStar.GetPath(currentPosition, GetSafeNextPoint(nextPoint));
    
    private void AddPointToRiverEntity(Vector2I point, RiverEntity riverEntity)
    {
        //if (riverEntity.ContainsPoint(point)) return;     // TODO: podríamos usar el número de puntos repetidos para la validación
        // TODO: por eficiencia, aquí deberíamos hacer las comprobaciones pertienentes y poner a false el ISVAlid de la entidad si procede.
        riverEntity.AddPoint(point);
    }
    
    private static Vector2I GetInc(int r, float alpha) => new((int)Math.Round(r * MathDotNetHelper.Cos(alpha)),
            (int)Math.Round(r * MathDotNetHelper.Sin(alpha)));

    private float GetDescendantAlpha(float alpha, int r, Vector2I currentPosition)
    {
        var currentElevation = _elevation.GetValueAt(currentPosition.X - Offset.X, currentPosition.Y - Offset.Y);
        foreach (float angle in Generate.LinearSpaced(24,  alpha+_minAlphaVariation, alpha+_maxAlphaVariation))// TODO: cuidado con ese 24 a pelo(parametrizar).
        {
            var inc = GetInc(r, angle);
            
            var nextPoint = currentPosition + inc - Offset;
            // Nos aseguramos de que nextPoint está dentro de los límites del mundo.
            var nextPoint_safe = new Vector2I(Math.Min(nextPoint.X, WorldSize.X-1), 
                Math.Min(nextPoint.Y, WorldSize.Y-1)); 
            nextPoint_safe = new Vector2I(Math.Max(0, nextPoint_safe.X), Math.Max(0, nextPoint_safe.Y));
            
            // TODO si nextPoint no está dentro de la matriz, tomamos el punto más cercano a nextPoint que esté dentro de la matriz
            var incElevation = _elevation.GetValueAt(nextPoint_safe.X, nextPoint_safe.Y);
            if (!(incElevation < currentElevation)) continue;
            return angle; // no funciona mal si devolvemos el primer menor encontrado...
        }
        /*
             TODO: ahora mismo estamos devolviendo la primera ocurrencia de ángulo correspondiente a una elevación
             menor a la anterior. Esto podría parametrizarse y devolver, por ejemplo, el ángulo correspondiente a la
             menor de todas las elevaciones disponibles. (con un diccionario).
        */
        return alpha;
    }

    private static int GetRandomR(int minValue, int maxValue) => 
        MathDotNetHelper.GetRandomIntInRange(minValue, maxValue);

    private float GetRandomAlpha() => MathDotNetHelper.GetRandomFloatInRange(_minRandomAlpha, _maxRandomAlpha);
    
    private static int RandomizeR(int r, int minChange, int maxChange) => 
        r + MathDotNetHelper.GetRandomIntInRange(minChange, maxChange);
    
    private float RandomizeAlpha(float alpha) =>    // TODO: el rango de variación de alpha podría ser un parámetro del generador
        alpha + MathDotNetHelper.GetRandomFloatInRange(_minAlphaVariation, _maxAlphaVariation);

    
    // Validation
    private bool ValidateRiver(RiverEntity riverToValidate)
    {
        // TODO: comprobamos si es válido en función de las reglas que establezcamos (longitud, PUNTOS REPETIDOS, etc)  .
        return true;
    }
    
    private bool IsValidBirth(Vector2I position)
    {   // untested
        const int maxBirthContinentalnessTier = 2; 
        const int minBirthElevationTier = 6;
        const int minDistanceToBirth = 64;
        // TODO: parámetros del generador
        
        var positionToValidate = position - Offset;
        return !Biome.IsSea(_elevation, positionToValidate.X, positionToValidate.Y)
               && !Biome.IsVolcanicIsland(_elevation, positionToValidate.X, positionToValidate.Y)
               && _elevation.GetValueTierAt(positionToValidate.X, positionToValidate.Y) >= minBirthElevationTier;
               //&& _continentalnessNoise.GetValueTierAt(positionToValidate.X, positionToValidate.Y) <= maxBirthContinentalnessTier;
        
        // TODO: comprobar cercanía a otros nacimientos con GetRiverBirthPositions
    }
    
    private bool IsValidMouth(Vector2I position)
    {
        var positionToValidate = position - Offset;
        return Biome.IsSea(_elevation, positionToValidate.X, positionToValidate.Y);
    }
    
    private IEnumerable<Vector2I> GetRiverBirthPositions() => _rivers.Select(river => river.GetBirthPosition());
    
    
    
    
    //TODO crear funcion para calcular el caudal en cierta posición x,y
    
    


    // getters & setters
    public void SetParameterElevation(Elevation elevation) => _elevation = elevation;
    public void SetParameterContinentalnessNoise(MFNL noise) => _continentalnessNoise = noise;
    public void SetParameterRiverPathfindingElevationPenalty(float riverPathfindingElevationPenalty) =>
        _riverPathfindingElevationPenalty = riverPathfindingElevationPenalty;


    
    /* TODO:    necesitamos un método para volver a generar los mismos ríos (mismo nacimiento y desembocadura) pero con
                nuevo valor de ElevationPenalty. Lo metemos aquí, en UpdateElevationPenalty. Quizás no la misma 
                desembocadura. */
    public void UpdateElevationPenalty(float newElevationPenalty)
    {
        SetParameterRiverPathfindingElevationPenalty(newElevationPenalty);
        foreach (var river in _rivers)
        {
            
        }
    }

    public override void Randomize(int seed)
    {
        // TODO: usar semilla
        // TODO: conociendo la posición de nacimiento de todos los ríos de _rivers, los volvemos a generar, de forma
        // que r y alpha serán distintos.
        var currentRivers = _rivers.Duplicate(); 
        _rivers.Clear();
        SetClearMatrix(WorldSize);
        foreach (var river in currentRivers)
            GenerateRiver(river.GetBirthPosition());
    }
    
    // TODO: necesitamos implementar de alguna manera persistencia para los ríos.
}

