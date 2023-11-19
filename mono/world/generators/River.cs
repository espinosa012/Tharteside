using Godot;
using Godot.Collections;
using Tartheside.mono.world.entities;

namespace Tartheside.mono.world.generators;

public partial class River : WorldGenerator
{
    private Elevation _elevation;
    private utilities.random.MFNL _continentalness;
    private utilities.random.MFNL _baseNoise;
    private utilities.random.MFNL _baseElevation;
    private int _thresholdTier = 0;        // TODO parametrizar
    
    private RiverTAStar _pathfindingAStar;
    private Array<RiverEntity> _rivers = new Array<RiverEntity>();

    
    public River(int matrixSizeX, int matrixSizeY) : base(matrixSizeX, matrixSizeY)
    {}
    
    public override float GenerateValueAt(int x, int y)
    {
        float trueValue = 0.999f;
        float falseValue = -1.0f;
        
        //return RiverAlgorithm(x, y);
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
    
    private float RiverNoiseAlgorithm(int x, int y)
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
    
    public utilities.random.MFNL GetParameterContinentalness() => _continentalness;
    public void SetParameterContinentalness(utilities.random.MFNL continentalness) => _continentalness = continentalness;

    public utilities.random.MFNL GetParameterBaseElevation() => _baseElevation;
    public void SetParameterBaseElevation(utilities.random.MFNL baseElevation) => _baseElevation = baseElevation;
        
    public utilities.random.MFNL GetParameterBaseNoise() => _baseNoise;
    public void SetParameterBaseNoise(utilities.random.MFNL baseNoise) => _baseNoise = baseNoise;

    public int GetParameterThresholdTier() => _thresholdTier;   
    public void SetParameterThresholdTier(int thresholdTier) => _thresholdTier = thresholdTier;

    public void SetPathfindingAstar(RiverTAStar pathfinding) => _pathfindingAStar = pathfinding;
    public RiverTAStar GetParameterPathfindingAstar() => _pathfindingAStar;
}

