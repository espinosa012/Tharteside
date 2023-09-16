using Godot;
using System;

public partial class River : WorldGenerator
{

    private Elevation _elevation;
    private MFNL _continentalness;

    public bool IsValidBirth(int x, int y)
    {
        return false;
    }
    
    public bool IsValidMouth(int x, int y)
    {
        return false;
    }
    
    
    // getters & setters
    public Elevation GetParameterElevation() => _elevation;
    public void SetParameterElevation(Elevation elevation) => _elevation = elevation;
    
    public MFNL GetParameterContinentalness() => _continentalness;
    public void SetParameterContinentalness(MFNL continentalness) => _continentalness = continentalness;

    
}
