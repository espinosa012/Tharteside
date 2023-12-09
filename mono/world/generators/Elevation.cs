using System;
using Godot;
using Tartheside.mono.utilities.random;

// algoritmo marzo '23

namespace Tartheside.mono.world.generators;

public partial class Elevation : BaseGenerator
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

	public Elevation(Vector2I worldSize, Vector2I chunkSize, Vector2I offset, int nTiers) : 
		base(worldSize, chunkSize, offset, nTiers)
	{}
	
	public override float GenerateValueAt(int x, int y) => IsLand(x, y) ? IsVolcanicIsland(x, y) ? 
				GetVolcanicIslandElevation(x, y) : GetContinentalElevation(x, y) : 0.0f;  
    
	private float GetVolcanicIslandElevation(int x, int y) => _peaksAndValleysNoise.GetNormalizedNoise2D(x, y) * 
		       _continentalScaleValue * _baseElevationNoise.GetNormalizedNoise2D(x, y) *
		       _volcanicIslandsNoise.GetNormalizedNoise2D(x, y) * _islandScaleValue;

	private float GetContinentalElevation(int x, int y)	=> 
		Math.Min(1f, Math.Max(_minContinentalHeight, _peaksAndValleysNoise.GetNormalizedNoise2D(x, y) * 
			_continentalScaleValue * _baseElevationNoise.GetNormalizedNoise2D(x, y) - _seaScaleValue * 
			_continentalnessNoise.GetNormalizedNoise2D(x, y)));    
    
	
	// For biome determination
	public bool IsLand(int x, int y) => IsContinentalLand(x, y) || IsVolcanicIsland(x, y);

	public bool IsContinentalLand(int x, int y) => 
		_baseElevationNoise.GetNormalizedNoise2D(x, y) - _minContinentalHeight > _seaScaleValue * 
		_continentalnessNoise.GetNormalizedNoise2D(x, y);

	public bool IsVolcanicLand(int x, int y) => 
		(_volcanicIslandsNoise.GetNormalizedNoise2D(x, y) > _islandThresholdLevel);

	public bool IsVolcanicIsland(int x, int y) => IsOutToSea(x, y) && IsVolcanicLand(x, y);

	public bool IsOutToSea(int x, int y) => _baseElevationNoise.GetNormalizedNoise2D(x, y) - _minContinentalHeight < 
	                                        _seaScaleValue * _continentalnessNoise.GetNormalizedNoise2D(x, y) * 
	                                        _outToSeaFactor; // devuelve si está lo suficientemente mar adentro según
															 // el factor OutToSeaFactor
	
	
	// getters & setters
	public void SetParameterBaseElevationNoise(MFNL value) => _baseElevationNoise = value;
	public void SetParameterContinentalnessNoise(MFNL value) => _continentalnessNoise = value;
	public void SetParameterPeaksAndValleysNoise(MFNL value) => _peaksAndValleysNoise = value;
	public void SetParameterVolcanicIslandsNoise(MFNL value) => _volcanicIslandsNoise = value;
	public void SetParameterMinContinentalHeight(float value) => _minContinentalHeight = value;
	public void SetParameterContinentalScaleValue(float value) => _continentalScaleValue = value;
	public void SetParameterSeaScaleValue(float value) => _seaScaleValue = value;
	public void SetParameterIslandScaleValue(float value) => _islandScaleValue = value;
	public void SetParameterIslandThresholdLevel(float value) => _islandThresholdLevel = value;
	public void SetParameterOutToSeaFactor(float value) => _outToSeaFactor = value;
	
}