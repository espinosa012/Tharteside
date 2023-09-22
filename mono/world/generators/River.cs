using Godot;
using System;

public partial class River : WorldGenerator
{

    private Elevation _elevation;
    private MFNL _continentalness;

    // considerar la pendiente, slope, que esta en elevation

    public bool IsRiver(int x, int y)
    {
        return false;
    }
    
    public bool IsValidBirth(int x, int y)
    {
        return true;
    }
    
    public bool IsValidMouth(int x, int y)
    {
        return true;
    }
    
    
    // getters & setters
    public Elevation GetParameterElevation() => _elevation;
    public void SetParameterElevation(Elevation elevation) => _elevation = elevation;
    
    public MFNL GetParameterContinentalness() => _continentalness;
    public void SetParameterContinentalness(MFNL continentalness) => _continentalness = continentalness;

    
}
