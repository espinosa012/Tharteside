using Godot;

namespace Tartheside.mono.world.generators;

public partial class Humidity : WorldGenerator
{
    
    // El valor de la humedad en cada posición estará influenciado principalmente por dos valores: la cercanía al
    // mar (Continentalness) y la cercanía a una masa de agua dulce (River)
    private Elevation _elevation; 
    private River _river; 
    private MFNL _continentalness;

    private float _continentalnessFactor;
    private float _riverFactor;
    
    public override float GetValueAt(int x, int y)
    {
        return HumidityAlgorithm(x, y);
    }


    private float HumidityAlgorithm(int x, int y)
    {   
        return _elevation.IsLand(x, y) ? GetHumidityValue(x, y) : 0.0f;
    }

    private float GetHumidityValue(int x, int y)
    {
        return Mathf.Min(1.0f, GetContinentalnessInfluence(x, y) + GetRiverInfluence(x, y));
    }
    
    private float GetContinentalnessInfluence(int x, int y)
    {
        return _continentalnessFactor * _continentalness.GetNormalizedNoise2D(x, y);
    }
    
    private float GetRiverInfluence(int x, int y)
    {
        return _riverFactor * _river.GetParameterBaseNoise().GetNormalizedInverseNoise2D(x, y);
    }
    
    
    // getters & setters
    public Elevation GetParameterElevation() => _elevation;
    public void SetParameterElevation(Elevation elevation) => _elevation = elevation;

    public River GetParameterRiver() => _river;
    public void SetParameterRiver(River river) => _river = river;
    
    public MFNL GetParameterContinentalness() => _continentalness;
    public void SetParameterContinentalness(MFNL continentalness) => _continentalness = continentalness;

    public float GetParameterContinentalnessFactor() => _continentalnessFactor;
    public void SetParameterContinentalnessFactor(float continentalnessFactor) => _continentalnessFactor = continentalnessFactor;

    public float GetParameterRiverFactor() => _riverFactor;
    public void SetParameterRiverFactor(float riverFactor) => _riverFactor = riverFactor;
    
    

}