using Godot;
using Tartheside.mono.utilities.random;

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
    
    public override float GetValueAt(int x, int y) => HumidityAlgorithm(x, y);

    private float HumidityAlgorithm(int x, int y) => _elevation.IsLand(x, y) ? 
        Mathf.Min(1.0f, GetContinentalnessInfluence(x, y) + GetRiverInfluence(x, y)) : 0.0f;
    
    private float GetContinentalnessInfluence(int x, int y) => 
        _continentalnessFactor * _continentalness.GetNormalizedNoise2D(x, y);
    
    private float GetRiverInfluence(int x, int y) => 0.0f;  // TODO: implementar influencia de los ríos con el enfoque AStar


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