using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using MathNet.Numerics;
using Tartheside.mono.utilities.logger;
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

    // TODO: refactorizar parámetros
    
    
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
        Vector2I[] seeds = {
            new Vector2I(86577, 852),
            new Vector2I(86555, 956),
            new Vector2I(86590, 870),
            new Vector2I(86577, 852),
            new Vector2I(86891, 877),
            new Vector2I(87121, 905),
            new Vector2I(86997, 900),
            new Vector2I(87065, 872),
            new Vector2I(87083, 792),
            new Vector2I(87001, 867),
            new Vector2I(86566, 1028),
            new Vector2I(86870, 733),
            new Vector2I(86998, 901),
            new Vector2I(86957, 837),
            new Vector2I(86619, 1069),
            
            new Vector2I(87316, 1085),
            new Vector2I(87313, 1075),
            new Vector2I(87262, 1027),
            new Vector2I(87287, 973), 
            new Vector2I(87348, 1008),
            new Vector2I(87365, 1017),
            new Vector2I(87377, 1017),
            new Vector2I(87465, 1101),
        };

        SpawnHighRivers();
        SpawnMediumRivers();
        SpawnLowRivers();
    }

    public void SpawnHighRivers()
    {
        // TODO: parámetros
        var minTier = 11;
        var maxTier = 16;
        var maxChunks = 384;
        var minRegionSize = 25;
            
        MaskGenerator maskGenerator = new MaskGenerator(WorldSize, ChunkSize, Offset, NTiers);
        maskGenerator.SetValueMatrix(_elevation.GetValueMatrix());
        maskGenerator.RangeThresholdByTier(minTier, maxTier);

        var connectedRegions = maskGenerator.GetConnectedRegions(minTier, maxTier, minRegionSize);
        foreach (var region in connectedRegions)
            if (region.GetRegionSize() >= minRegionSize)
                GenerateRiver(region.GetCentroid(), maxChunks);
    }
    
    public void SpawnMediumRivers()
    {
        // TODO: parámetros
        var minTier = 8;
        var maxTier = 12;
        var maxChunks = 384;
        var minRegionSize = 25;
            
        MaskGenerator maskGenerator = new MaskGenerator(WorldSize, ChunkSize, Offset, NTiers);
        maskGenerator.SetValueMatrix(_elevation.GetValueMatrix());
        maskGenerator.RangeThresholdByTier(minTier, maxTier);

        var connectedRegions = maskGenerator.GetConnectedRegions(minTier, maxTier, minRegionSize);
        foreach (var region in connectedRegions)
            if (region.GetRegionSize() >= minRegionSize)
                GenerateRiver(region.GetCentroid(), maxChunks);
    }
    
    public void SpawnLowRivers()
    {
        // TODO: parámetros
        var minTier = 6;
        var maxTier = 7;
        var maxChunks = 128;
        var minRegionSize = 25;
            
        MaskGenerator maskGenerator = new MaskGenerator(WorldSize, ChunkSize, Offset, NTiers);
        maskGenerator.SetValueMatrix(_elevation.GetValueMatrix());
        maskGenerator.RangeThresholdByTier(minTier, maxTier);

        var connectedRegions = maskGenerator.GetConnectedRegions(minTier, maxTier, minRegionSize);
        foreach (var region in connectedRegions)
            if (region.GetRegionSize() >= minRegionSize)
                GenerateRiver(region.GetCentroid(), maxChunks);
    }
    
    public void GenerateRiver(Vector2I birthPos, int maxChunks)
    {
        var currentChunkPosition = birthPos;
        for (int i = 0; i < maxChunks; i++)
        {
            var iterations = MathDotNetHelper.GetRandomIntInRange(3, 8);
            var r = MathDotNetHelper.GetRandomIntInRange(1, 3);
            
            var riverChunk = GetRiverChunk(r, iterations, currentChunkPosition);
            AddPathToMatrix(riverChunk);

            currentChunkPosition = riverChunk.GetRiverPath().Last();
            if (IsValidMouth(currentChunkPosition))    break;
        }
    }
    
    public void GenerateRiver(int birthPosX, int birthPosY, int maxCunks) => 
        GenerateRiver(new Vector2I(birthPosX, birthPosY), maxCunks);

    private RiverEntity GetRiverChunk(int r, int iterations, Vector2I initPosition)
    {
        // TODO: r podría ser un vector de longitud iterations para obtener rios de forma determinista
        // Es determinista!
        
        var riverArm = new RiverEntity();
        riverArm.SetBirthPosition(initPosition.X, initPosition.Y);

        var moutReached = false;
        var nextPoint = initPosition;
        var currentPosition = initPosition;

        var i = 0;
        while (i<iterations && !moutReached && _pathfindingAStar.ContainsPoint(nextPoint))
        {
            var alpha = GetMostDescendantAlpha(r, currentPosition, 0.0f);
            nextPoint = GetUpdatedNextPoint(currentPosition, r, alpha);

            foreach (var point in GetStepPath(currentPosition, nextPoint))
            {
                AddPointToRiverEntity(point, riverArm);
                if (IsValidMouth(point))
                {
                    moutReached = true;
                    break;
                }
            }
            currentPosition = nextPoint;
            i++;
        }
        return riverArm;
    }
    
    public void GenerateRiver_(Vector2I birthPos)   // Deprecated
    {
        var riverEntity = new RiverEntity();
        
        if (!IsValidBirth(birthPos)) riverEntity.SetValid(false); 
        riverEntity.SetBirthPosition(birthPos.X, birthPos.Y);

        // TODO: considerar constraints de elevación

        var mouthReached = false;
        var currentPosition = birthPos;
        var nextPoint = birthPos;

        var alpha = GetRandomAlpha();   //TODO: ¿inicializar alpha hacia donde la elevación sea menor?
        var iterations = 0;

        while (!mouthReached && _pathfindingAStar.ContainsPoint(nextPoint) && riverEntity.IsValid())
        {
            var r = GetRandomR(_minRValue, _maxRValue);
            alpha = GetMostDescendantAlphaInVariationRange(alpha, r, currentPosition);
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
            if (iterations >= _maxIterations) riverEntity.SetValid(false);
        }
        
        if (riverEntity.IsValid())
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
        // TODO: por eficiencia, aquí deberíamos hacer las comprobaciones pertienentes y poner a false el IsValid de la entidad si procede.   
        riverEntity.AddPoint(point);
    }
    
    private void AddPathToMatrix(RiverEntity riverEntity)
    {
        var path = riverEntity.GetRiverPath();
        foreach (var point in path)
            SetValueAt(point.X, point.Y, TrueValue); 
    }
    
    private static Vector2I GetInc(int r, float alpha) => new((int)Math.Round(r * MathDotNetHelper.Cos(alpha)),
            (int)Math.Round(r * MathDotNetHelper.Sin(alpha)));

    private float GetMostDescendantAlpha(int r, Vector2I currentPosition, float defaultAlpha = 0.0f)
    {
        var currentElevation = _elevation.GetValueAt(currentPosition.X - Offset.X, currentPosition.Y - Offset.Y);

        var availableAngles = new List<float>();
        var availableElevations = new List<float>();
        
        // TODO: cuidado con ese 24 a pelo(parametrizar).
        foreach (float angle in Generate.LinearSpaced(24, 0, 2 * MathDotNetHelper.Pi))
        {
            // Nos aseguramos de que nextPoint está dentro de los límites del mundo.
            var nextPoint = currentPosition + GetInc(r, angle) - Offset;
            var nextPoint_safe = new Vector2I(Math.Min(nextPoint.X, WorldSize.X-1), 
                Math.Min(nextPoint.Y, WorldSize.Y-1)); 
            nextPoint_safe = new Vector2I(Math.Max(0, nextPoint_safe.X), Math.Max(0, nextPoint_safe.Y));
            
            var incElevation = _elevation.GetValueAt(nextPoint_safe.X, nextPoint_safe.Y);
            if (!(incElevation < currentElevation)) continue;
            
            availableAngles.Add(angle);
            availableElevations.Add(incElevation);
        }
        return availableElevations.Count > 0 ? 
            availableAngles[availableElevations.IndexOf(availableElevations.Min())] 
            : defaultAlpha;
    }

    private List<float> GetDescendantAngles(Vector2I position, int r)
    {
        var toReturn = new List<float>();
        foreach (float angle in Generate.LinearSpaced(48, 0, 2 * MathDotNetHelper.Pi))
        {
            // Nos aseguramos de que nextPoint está dentro de los límites del mundo.
            var nextPoint = position + GetInc(r, angle) - Offset;
            var nextPoint_safe = new Vector2I(Math.Min(nextPoint.X, WorldSize.X-1), 
                Math.Min(nextPoint.Y, WorldSize.Y-1)); 
            nextPoint_safe = new Vector2I(Math.Max(0, nextPoint_safe.X), Math.Max(0, nextPoint_safe.Y));
            var incElevation = _elevation.GetValueAt(nextPoint_safe.X, nextPoint_safe.Y);
            if (!(incElevation < _elevation.GetValueAt(position.X - Offset.X, position.Y - Offset.Y))) continue;
            
            toReturn.Add(angle);
        }

        return toReturn;
    }
    
    private float GetMostDescendantAlphaInVariationRange(float alpha, int r, Vector2I currentPosition)
    {
        var currentElevation = _elevation.GetValueAt(currentPosition.X - Offset.X, currentPosition.Y - Offset.Y);
        var availableAngles = new List<float>();
        var availableElevations = new List<float>();
        
        // TODO: cuidado con ese 24 a pelo(parametrizar).
        foreach (float angle in Generate.LinearSpaced(24,  alpha+_minAlphaVariation, alpha+_maxAlphaVariation))
        {
            var nextPoint = currentPosition + GetInc(r, angle) - Offset;
            
            // Nos aseguramos de que nextPoint está dentro de los límites del mundo.
            var nextPoint_safe = new Vector2I(Math.Min(nextPoint.X, WorldSize.X-1), 
                Math.Min(nextPoint.Y, WorldSize.Y-1)); 
            nextPoint_safe = new Vector2I(Math.Max(0, nextPoint_safe.X), Math.Max(0, nextPoint_safe.Y));
            
            var incElevation = _elevation.GetValueAt(nextPoint_safe.X, nextPoint_safe.Y);
            if (!(incElevation < currentElevation)) continue;
            
            availableAngles.Add(angle);
            availableElevations.Add(incElevation);
        }
       
        return availableElevations.Count > 0 ? 
            availableAngles[availableElevations.IndexOf(availableElevations.Min())] 
            : alpha;
    }

    private static int GetRandomR(int minValue, int maxValue) => 
        MathDotNetHelper.GetRandomIntInRange(minValue, maxValue);

    private float GetRandomAlpha() => MathDotNetHelper.GetRandomFloatInRange(_minRandomAlpha, _maxRandomAlpha);
    
    private static int RandomizeR(int r, int minChange, int maxChange) => 
        r + MathDotNetHelper.GetRandomIntInRange(minChange, maxChange);
    
    private float RandomizeAlpha(float alpha) =>    // TODO: el rango de variación de alpha podría ser un parámetro del generador
        alpha + MathDotNetHelper.GetRandomFloatInRange(_minAlphaVariation, _maxAlphaVariation);

    
    private bool IsValidBirth(Vector2I position)
    {   // untested
        const int maxBirthContinentalnessTier = 2; 
        const int minBirthElevationTier = 5;
        const int minDistanceToBirth = 64;
        // TODO: parámetros del generador
        
        var positionToValidate = position - Offset;
        return !Biome.IsSea(_elevation, positionToValidate.X, positionToValidate.Y)
               && !Biome.IsVolcanicIsland(_elevation, positionToValidate.X, positionToValidate.Y);
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



    public override void Randomize(int seed)    // TODO: ¿tiene sentido? quizás mejor aleatorizar los puntos de nacimiento.
    {
        // TODO: usar semilla
        // TODO: conociendo la posición de nacimiento de todos los ríos de _rivers, los volvemos a generar, de forma
        // que r y alpha serán distintos.
        _rivers.Clear();
        SetClearMatrix(WorldSize);
        SpawnRivers();
    }
    
    // TODO: necesitamos implementar de alguna manera persistencia para los ríos.
}

