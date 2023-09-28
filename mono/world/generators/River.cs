using Godot;
using System;
using Tartheside.mono.utilities.pathfinding;

public partial class River : WorldGenerator
{

    private Elevation _elevation;
    private MFNL _continentalness;
    private MFNL _baseElevation;
    private TAStar _pathfindingAstar;
    
    public River()
    {
        _pathfindingAstar = new TAStar(new Vector2I(), new Vector2I());
    }
        
    
    // considerar la pendiente, slope, que esta en elevation
    public bool IsRiver(int x, int y)
    {
        return _baseElevation.GetAbsoluteNoiseValueTierAt(x, y) == 0;
    }
    
    public bool IsValidBirth(int x, int y)
    {
        return true;
    }
    
    public bool IsValidMouth(int x, int y)
    {
        return true;
    }


    public void RandomizeAvailablePath()
    {
        // randomizamos sólo el noise correspondientes al absolute
        _baseElevation.RandomizeSeed();
        // como está ahora mismo, si había ríos previamente creados, éstos sobrevivirán.
    }
    
    
    // getters & setters
    public Elevation GetParameterElevation() => _elevation;
    public void SetParameterElevation(Elevation elevation) => _elevation = elevation;
    
    public MFNL GetParameterContinentalness() => _continentalness;
    public void SetParameterContinentalness(MFNL continentalness) => _continentalness = continentalness;

    public MFNL GetParameterBaseElevation() => _baseElevation;
    public void SetParameterBaseElevation(MFNL baseElevation) => _baseElevation = (MFNL) baseElevation.Duplicate();

    
}
