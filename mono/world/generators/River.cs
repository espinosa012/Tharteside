using Godot;


public partial class River : WorldGenerator
{

    private Elevation _elevation;
    private MFNL _continentalness;
    private MFNL _baseNoise;
    private MFNL _baseElevation;
    
    
    public override float GetValueAt(int x, int y)
    {
        return RiverAlgorithm(x, y);
    }

    private float RiverAlgorithm(int x, int y)
    {   // sep23
        return ((_baseNoise.GetAbsoluteNoiseValueTierAt(x, y, 32) == 0 && 
                 _elevation.GetValueTierAt(x, y) != 0) ? 0.99f : -1.0f);
    }

    
    // getters & setters
    public Elevation GetParameterElevation() => _elevation;
    public void SetParameterElevation(Elevation elevation) => _elevation = elevation;
    
    public MFNL GetParameterContinentalness() => _continentalness;
    public void SetParameterContinentalness(MFNL continentalness) => _continentalness = continentalness;

    public MFNL GetParameterBaseElevation() => _baseElevation;
    public void SetParameterBaseElevation(MFNL baseElevation) => _baseElevation = baseElevation;
        
    public MFNL GetParameterBaseNoise() => _baseNoise;
    public void SetParameterBaseNoise(MFNL baseNoise) => _baseNoise = baseNoise;
    

}
