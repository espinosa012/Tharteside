using Godot;
using System;

public partial class RNG
{
    private RandomNumberGenerator _rng;
    
    public RNG(int seed = 1308)
    {
        _rng = new RandomNumberGenerator();
    }
    
}
