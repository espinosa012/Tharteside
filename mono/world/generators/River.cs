namespace Tartheside.mono.world.generators;

public partial class River : WorldGenerator
{
    
    private Elevation _elevation;
    private MFNL _continentalness;
    private MFNL _baseNoise;
    private MFNL _baseElevation;
    private int _thresholdTier = 0;     // el valor ideal dependerá de las características del ruido (freq) para
                                        // no perder continuidad.
    
    public override float GetValueAt(int x, int y)
    {
        //return 1.0f - _baseNoise.GetAbsoluteValueNoise(x, y);
        return RiverAlgorithm(x, y);
    }

    //TODO crear funcion para calcular el caudal en cierta posición x,y
    
    private float RiverAlgorithm(int x, int y)
    {   // sep23
        var isNotSea = _elevation.GetValueTierAt(x, y) != 0;
        return ((IsValidRiverPath(x, y) && isNotSea) ? 0.99999f : -1.0f);
        
        // cuando usamos squares de más de 1, tenemos que implementar la lógica para tener los valores
        // dentro del square. esto en tilemap
    }
    
    private bool IsValidRiverPath(int x, int y)
    {
        const int nTiers = 32;  // tomer el valor del parámetro.
                                // cuanto mayor, más estrechos los ríos.
        return (_baseNoise.GetAbsoluteNoiseValueTierAt(x, y, nTiers) <= _thresholdTier) 
                && !_elevation.IsVolcanicIsland(x, y);
    }

    public void Randomize()
    {
        _baseNoise.RandomizeSeed();
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

    public int GetParameterThresholdTier() => _thresholdTier;   
    public void SetParameterThresholdTier(int thresholdTier) => _thresholdTier = thresholdTier;
}

