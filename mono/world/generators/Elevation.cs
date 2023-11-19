using System;
using Tartheside.mono.utilities.random;

// algoritmo marzo '23

namespace Tartheside.mono.world.generators;

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

	public override float GetValueAt(int x, int y) => IsLand(x, y) ? IsVolcanicIsland(x, y) ? 
				GetVolcanicIslandElevation(x, y) : GetContinentalElevation(x, y) : 0.0f;  
    
	private float GetVolcanicIslandElevation(int x, int y) => _peaksAndValleysNoise.GetNormalizedNoise2D(x, y) * 
		       _continentalScaleValue * _baseElevationNoise.GetNormalizedNoise2D(x, y) *
		       _volcanicIslandsNoise.GetNormalizedNoise2D(x, y) * _islandScaleValue;
				// march '23

	private float GetContinentalElevation(int x, int y)	=> 
		Math.Min(1f, Math.Max(_minContinentalHeight, _peaksAndValleysNoise.GetNormalizedNoise2D(x, y) * 
			_continentalScaleValue * _baseElevationNoise.GetNormalizedNoise2D(x, y) - _seaScaleValue * 
			_continentalnessNoise.GetNormalizedNoise2D(x, y)));    
    
	public bool IsLand(int x, int y) => IsContinentalLand(x, y) || IsVolcanicIsland(x, y);

	public bool IsContinentalLand(int x, int y) => 
		_baseElevationNoise.GetNormalizedNoise2D(x, y) - _minContinentalHeight > _seaScaleValue * 
		_continentalnessNoise.GetNormalizedNoise2D(x, y);

	public bool IsVolcanicLand(int x, int y) => 
		(_volcanicIslandsNoise.GetNormalizedNoise2D(x, y) > _islandThresholdLevel);

	public bool IsVolcanicIsland(int x, int y) => IsOutToSea(x, y) && IsVolcanicLand(x, y);

	public bool IsOutToSea(int x, int y) => _baseElevationNoise.GetNormalizedNoise2D(x, y) - _minContinentalHeight < 
	                                        _seaScaleValue * _continentalnessNoise.GetNormalizedNoise2D(x, y) * 
	                                        _outToSeaFactor; // devuelve si está lo suficientemente mar adentro según el factor OutToSeaFactor


	// getters & setters
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