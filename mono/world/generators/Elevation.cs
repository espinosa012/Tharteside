using Godot;
using System;

// algoritmo marzo '23

public partial class Elevation : WorldGenerator
{
    private MFNL _baseElevationNoise;
    private MFNL _continentalnessNoise;
    private MFNL _peaksAndValleysNoise;
    private MFNL _volcanicIslandsNoise;

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
		return _peaksAndValleysNoise.GetNormalizedNoise2D(x, y) * 
		        _continentalScaleValue * _baseElevationNoise.GetNormalizedNoise2D(x, y) *
		        _volcanicIslandsNoise.GetNormalizedNoise2D(x, y) * _islandScaleValue;
	}

    public float GetContinentalElevation(int x, int y)	// march23
	{
		return Math.Min(1f, Math.Max(_minContinentalHeight, _peaksAndValleysNoise.GetNormalizedNoise2D(x, y) * 
			_continentalScaleValue * _baseElevationNoise.GetNormalizedNoise2D(x, y) -
			_seaScaleValue *_continentalnessNoise.GetNormalizedNoise2D(x, y)));    
	}

    
    public bool IsLand(int x, int y)
	{
		return IsContinentalLand(x, y) || IsVolcanicIsland(x, y);
    }

    public bool IsContinentalLand(int x, int y)
	{
		return _baseElevationNoise.GetNormalizedNoise2D(x, y) - _minContinentalHeight > _seaScaleValue * _continentalnessNoise.GetNormalizedNoise2D(x, y);
	}

    public bool IsVolcanicLand(int x, int y)
	{
		return (_volcanicIslandsNoise.GetNormalizedNoise2D(x, y) > _islandThresholdLevel);
	}

    public bool IsVolcanicIsland(int x, int y)
	{
		return IsOutToSea(x, y) && IsVolcanicLand(x, y);
	}

    public bool IsOutToSea(int x, int y)
	{
		// devuelve si está lo suficientemente mar adentro según el factor OutToSeaFactor
		return _baseElevationNoise.GetNormalizedNoise2D(x, y) - _minContinentalHeight < (float) _seaScaleValue * _continentalnessNoise.GetNormalizedNoise2D(x, y) * _outToSeaFactor;
	}



    public double GetSlope(Vector2I origin, Vector2I dest)	// untested
	{
		return (Math.Sqrt((origin - dest).LengthSquared()) / (GetValueAt(origin.X, origin.Y) - GetValueAt(dest.X, dest.Y)));
	}


    // getters & setters (uno por cada parámetro que queremos hacer accesibles)
    public MFNL GetParameterBaseElevationNoise() => _baseElevationNoise;
    public void SetParameterBaseElevationNoise(MFNL value) => _baseElevationNoise = value;
    
    public MFNL GetParameterContinentalnessNoise() => _continentalnessNoise;
    public void SetParameterContinentalnessNoise(MFNL value) => _continentalnessNoise = value;
    
    public MFNL GetParameterPeaksAndValleysNoise() => _peaksAndValleysNoise;
    public void SetParameterPeaksAndValleysNoise(MFNL value) => _peaksAndValleysNoise = value;
    
    public MFNL GetParameterVolcanicIslandsNoise() => _volcanicIslandsNoise;
    public void SetParameterVolcanicIslandsNoise(MFNL value) => _volcanicIslandsNoise = value;

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

