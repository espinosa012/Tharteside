using Godot;
using System;

// algoritmo marzo '23

public partial class Elevation : WorldGenerator
{
    private MFNL _baseElevation;
    private MFNL _continentalness;
    private MFNL _peaksAndValleys;
    private MFNL _volcanicIslands;

    private float _minContinentalHeight;    
    private float _continentalScaleValue;    
    private float _seaScaleValue;    
    private float _islandScaleValue;    
    private float _islandThresholdLevel;    
    private float _outToSeaFactor;    


    public override float GetValueAt(int x, int y)
	{
		if (IsLand(x, y))
		{
			return IsVolcanicIsland(x, y) ? GetVolcanicIslandElevation(x, y) : 
				GetContinentalElevation(x, y);
		}
		return 0.0f;	// el valor de elevación de las celdas marítimas debe ser dinámico, permitiendo distintas profundidades
	}

    
    public float GetVolcanicIslandElevation(int x, int y)	// march23
	{
		return _peaksAndValleys.GetNormalizedNoise2D(x, y) * 
		        _continentalScaleValue * _baseElevation.GetNormalizedNoise2D(x, y) *
		        _volcanicIslands.GetNormalizedNoise2D(x, y) * _islandScaleValue;
	}

    public float GetContinentalElevation(int x, int y)	// march23
	{
		return Math.Min(1f, Math.Max(_minContinentalHeight, _peaksAndValleys.GetNormalizedNoise2D(x, y) * 
			_continentalScaleValue * _baseElevation.GetNormalizedNoise2D(x, y) -
			_seaScaleValue *_continentalness.GetNormalizedNoise2D(x, y)));    
	}

    
    public bool IsLand(int x, int y)
	{
		return IsContinentalLand(x, y) || IsVolcanicIsland(x, y);
    }

    public bool IsContinentalLand(int x, int y)
	{
		return _baseElevation.GetNormalizedNoise2D(x, y) - _minContinentalHeight > _seaScaleValue * _continentalness.GetNormalizedNoise2D(x, y);
	}

    public bool IsVolcanicLand(int x, int y)
	{
		return (_volcanicIslands.GetNormalizedNoise2D(x, y) > _islandThresholdLevel);
	}

    public bool IsVolcanicIsland(int x, int y)
	{
		return IsOutToSea(x, y) && IsVolcanicLand(x, y);
	}

    public bool IsOutToSea(int x, int y)
	{
		// devuelve si está lo suficientemente mar adentro según el factor OutToSeaFactor
		return _baseElevation.GetNormalizedNoise2D(x, y) - _minContinentalHeight < (float) _seaScaleValue * _continentalness.GetNormalizedNoise2D(x, y) * _outToSeaFactor;
	}



    public double GetSlope(Vector2I origin, Vector2I dest)	// untested
	{
		return (Math.Sqrt((origin - dest).LengthSquared()) / (GetValueAt(origin.X, origin.Y) - GetValueAt(dest.X, dest.Y)));
	}


    // getters & setters
    public MFNL GetNoiseBaseElevation() => _baseElevation;
    public void SetNoiseBaseElevation(MFNL noise) => _baseElevation = noise;
    public MFNL GetNoiseContinentalness() => _continentalness;
    public void SetNoiseContinentalness(MFNL noise) => _continentalness = noise;
    public MFNL GetNoisePeaksAndValleys() => _peaksAndValleys;
    public void SetNoisePeaksAndValleys(MFNL noise) => _peaksAndValleys = noise;
    public MFNL GetNoiseVolcanicIslands() => _volcanicIslands;
    public void SetNoiseVolcanicIslands(MFNL noise) => _volcanicIslands = noise;



    public float GetParameterMinContinentalHeight() => _minContinentalHeight;
    public void SetParameterMinContinentalHeight(float value) => _minContinentalHeight = value;

    public float GetParameterContinentalScaleValue() => _continentalScaleValue;
    public void SetParameterContinentalScaleValue(float value) => _continentalScaleValue = value;

    public float GetParameterSeaScaleValue() => _seaScaleValue;
    public void SetParameterSeaScaleValue(float value) => _seaScaleValue = value;

    public float GetParameterIslandScaleValue() => _islandScaleValue;
    public void SetParameterIslandScaleValue(float value) => _islandScaleValue = value;

    public float GetParameterIslandThresholdLevel() => _islandThresholdLevel;
    public void SetParameterIslandThresholdLevel(float value) => _islandThresholdLevel = value;
    
    public float GetParameterOutToSeaFactor() => _outToSeaFactor;
    public void SetParameterOutToSeaFactor(float value) => _outToSeaFactor = value;
}

