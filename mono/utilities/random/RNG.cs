using Godot;
using System;

public partial class RNG : GodotObject
{
    private RandomNumberGenerator _rng;
    UInt64 _rngOriginalState;
    
    public RNG(int seed = 1308)
    {
        _rng = new RandomNumberGenerator();
        _rngOriginalState = _rng.State;
    }
    
    public void UpdateState(int newState)
    {
        _rng.State = Convert.ToUInt64(newState);
    }
    
    
    public float GetFloatBy2DPosition(int x, int y)
    {
        _rng = new RandomNumberGenerator();
        _rng.Seed = UInt64.Parse(x.ToString() + y.ToString());
        return _rng.Randf();
    }
    
}
