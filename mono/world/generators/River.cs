using System;
using System.Linq;
using Godot;
using Godot.Collections;

namespace Tartheside.mono.world.generators;

public partial class River : WorldGenerator
{
    
    private Elevation _elevation;
    private MFNL _continentalness;
    private MFNL _baseNoise;
    private MFNL _baseElevation;
    private int _thresholdTier = 0;     // el valor ideal dependerá de las características del ruido (freq) para
                                        // no perder continuidad.

    private Array<RiverEntity> _rivers = new Array<RiverEntity>();
    
    
    public override float GetValueAt(int x, int y)
    {
        float trueValue = 0.999f;
        float falseValue = -1.0f;
        
        //return RiverAlgorithm(x, y);

        foreach (var river in _rivers)
            for (int i = 0; i < river.RiverPath.Count; i++)
                if (river.RiverPath.Contains(new Vector2I(x, y)))
                    return trueValue;
        return falseValue;
    }

    
    //TODO crear funcion para calcular el caudal en cierta posición x,y
    private float CellularAutomataRiverTest(int x, int y)
    {
        float trueValue = 0.9999f;
        float falseValue = -1.0f;

        if (x == 31014 && y == 1362)
            return trueValue;

        return falseValue;
    }

    public void GenerateRiver(Vector2I birthPos)
    {
        RiverEntity river = new RiverEntity();
        river.SetBirthPosition(birthPos.X, birthPos.Y);
        
        var worldCursor = birthPos;
        Vector2I[] neighbours = new[] {Vector2I.Left, Vector2I.Up, Vector2I.Right, Vector2I.Down};
        var nextStep = neighbours[(new Random()).Next(4)];

        //(_elevation.IsLand(worldCursor.X, worldCursor.Y))    // cambiar e implementar un tamaño máximo del path.
        for (int i = 0; i < 256; i++)
        {
            river.RiverPath.Add(worldCursor);
            var lowestNeighbourElevation = 1.0f;
            foreach (var neighbour in neighbours)
            {
                var neighbourValue = _elevation.GetValueAt(worldCursor.X + neighbour.X, worldCursor.Y + neighbour.Y);
                if (!(lowestNeighbourElevation > neighbourValue)
                    || river.RiverPath.Contains(worldCursor + neighbour)) continue;
                lowestNeighbourElevation = neighbourValue;
                nextStep = neighbour;
            }
            worldCursor += nextStep;
            // Condición de finalización
            if (_elevation.GetValueTierAt(worldCursor.X, worldCursor.Y) == 0)
                break;
        }
        river.SetMouthPosition(worldCursor.X, worldCursor.Y);
        _rivers.Add(river);
    }
    
    
    private float RiverAlgorithm(int x, int y)
    
    {   // sep23
        const int nTiers = 32;  // tomer el valor del parámetro.
        var isNotSea = _elevation.GetValueTierAt(x, y) != 0;
        return (((_baseNoise.GetAbsoluteNoiseValueTierAt(x, y, nTiers) <= _thresholdTier) 
                 && !_elevation.IsVolcanicIsland(x, y) && isNotSea) ? 0.99999f : -1.0f);
    }

    public void Randomize() => _baseNoise.RandomizeSeed();
    
    
    
    
    // getters & setters
    public Elevation GetParameterElevation() => _elevation;
    public void SetParameterElevation(Elevation elevation) => _elevation = elevation;
    
    public MFNL GetParameterContinentalness() => _continentalness;
    public void SetParameterContinentalness(MFNL continentalness) => _continentalness = continentalness;

    public MFNL GetParameterBaseElevation() => _baseElevation;
    public void SetParameterBaseElevation(MFNL baseElevation) => _baseElevation = baseElevation;
        
    public MFNL GetParameterBaseNoise() => _baseNoise;
    public void SetParameterBaseNoise(MFNL baseNoise) => _baseNoise = baseNoise;

    public int GetParameterThresholdTier() => _thresholdTier;   
    public void SetParameterThresholdTier(int thresholdTier) => _thresholdTier = thresholdTier;
}

