using System;
using System.Collections.Generic;
using Godot;

namespace Tartheside.mono;

public partial class World : GodotObject
{
	// WORLD PARAMETERS AND NOISES
	private Dictionary<string, Variant> _worldParameters;
	private Dictionary<string, MFNL> _worldNoises;

	// CONSTRUCTOR
	public World()
	{
		InitNoisesAndParameters();
	}

	private void InitNoisesAndParameters()
	{
		// Init noise
		_worldNoises = new Dictionary<string, MFNL>();
		
		// Formamos los objetos de ruido desde los .json correspondientes
		var baseElevation = new MFNL("BaseElevation");
		baseElevation.LoadFromJSON("BaseElevation");
		AddWorldNoise("BaseElevation", baseElevation);

		var continentalness = new MFNL("Continentalness");
		continentalness.LoadFromJSON("Continentalness");
		AddWorldNoise("Continentalness", continentalness);

		var peaksAndValleys = new MFNL("PeaksAndValleys");
		peaksAndValleys.LoadFromJSON("PeaksAndValleys");
		AddWorldNoise("PeaksAndValleys", peaksAndValleys);

		var volcanicIslands = new MFNL("VolcanicIslands");
		volcanicIslands.LoadFromJSON("VolcanicIslands");
		AddWorldNoise("VolcanicIslands", volcanicIslands);

		// Init parameters (cargar desde json)
		_worldParameters = new Dictionary<string, Variant>();
		
		AddWorldParameter("NTiers", 24);
		AddWorldParameter("MinContinentalHeight", 0.023f);
		AddWorldParameter("ContinentalScaleValue", 1.22f);
		AddWorldParameter("SeaScaleValue", 1f/0.85f);
		AddWorldParameter("IslandScaleValue", 0.81f);
		AddWorldParameter("IslandThresholdLevel", 0.76f); 
		AddWorldParameter("OutToSeaFactor", 0.7f);  
	}

	//  WORLD PARAMETERS
	private void AddWorldParameter(string param, Variant value)
	{
		_worldParameters.Add(param, value);
	}

	private void UpdateWorldParameter(string param, Variant value)
	{
		if (_worldParameters.ContainsKey(param)) {_worldParameters[param] = value;}
	}

	private Variant GetWorldParameter(string param)
	{
		return _worldParameters[param];
	}

	//  WORLD NOISE
	private void AddWorldNoise(string name, MFNL noise)
	{
		_worldNoises.Add(name, noise);
	}

	private MFNL GetWorldNoise(string name)
	{
		if (!_worldNoises.ContainsKey(name)) {return null;}
		return _worldNoises[name];
	}

	private void RandomizeWorld()
	{
		// randomiza las semillas de todos los objetos de ruido
		foreach (MFNL noise in _worldNoises.Values)
		{
			noise.RandomizeSeed();  // se le puede pasar una semilla en concreto
		}
	}
	
	// TEMPERATURE
	private float GetTemperature(int x, int y, int worldYLength = 512)
	{
		return GetNormalizedDistanceToEquator(y, worldYLength);
	}
	
	private float GetNormalizedDistanceToEquator(int y, int worldYLength = 512)
	{
		return (float) Math.Abs(y - worldYLength) / worldYLength;
	}
	
	

	// ELEVATION
	public float GetElevation(int x, int y)
	{
		// algoritmo marzo '23
		if (IsLand(x, y))
		{
			return IsVolcanicIsland(x, y) ? GetVolcanicIslandElevation(x, y) : 
				GetContinentalElevation(x, y);
		}
		return 0.0f;	// el valor de elevación de las celdas marítimas debe ser dinámico, permitiendo distintas profundidades
	}

	public float GetVolcanicIslandElevation(int x, int y)
	{
		return (GetWorldNoise("PeaksAndValleys").GetNormalizedNoise2D(x, y) * 
		        (float) GetWorldParameter("ContinentalScaleValue") * 
		        GetWorldNoise("BaseElevation").GetNormalizedNoise2D(x, y) * 
		        GetWorldNoise("VolcanicIslands").GetNormalizedNoise2D(x, y) * 
		        (float) GetWorldParameter("IslandScaleValue"));
	}

	public float GetContinentalElevation(int x, int y)
	{
		return Math.Min(1f, Math.Max((float) GetWorldParameter("MinContinentalHeight"), 
			GetWorldNoise("PeaksAndValleys").GetNormalizedNoise2D(x, y) * 
			(float) GetWorldParameter("ContinentalScaleValue") * 
			(GetWorldNoise("BaseElevation").GetNormalizedNoise2D(x, y) - 
			 (float) GetWorldParameter("SeaScaleValue") * 
			 GetWorldNoise("Continentalness").GetNormalizedNoise2D(x, y))));    
		// devolvemos un valor en el rango [MinContinentalHeight, 1.0]
	}

	public bool IsLand(int x, int y)
	{
		return IsVolcanicIsland(x, y) || IsContinentalLand(x, y);
	}

	public bool IsContinentalLand(int x, int y)
	{
		return GetWorldNoise("BaseElevation").GetNormalizedNoise2D(x, y) - (float) GetWorldParameter("MinContinentalHeight") > (float) GetWorldParameter("SeaScaleValue") * GetWorldNoise("Continentalness").GetNormalizedNoise2D(x, y);
	}

	public bool IsOutToSea(int x, int y)
	{
		// devuelve si está lo suficientemente mar adentro según el factor OutToSeaFactor
		return GetWorldNoise("BaseElevation").GetNormalizedNoise2D(x, y) - (float) GetWorldParameter("MinContinentalHeight") < (float) GetWorldParameter("SeaScaleValue") * GetWorldNoise("Continentalness").GetNormalizedNoise2D(x, y) * (float) GetWorldParameter("OutToSeaFactor");
	}

	public bool IsVolcanicLand(int x, int y)
	{
		return (GetWorldNoise("VolcanicIslands").GetNormalizedNoise2D(x, y) > (float) GetWorldParameter("IslandThresholdLevel"));
	}

	public bool IsVolcanicIsland(int x, int y)
	{
		return IsOutToSea(x, y) && IsVolcanicLand(x, y);
	}


	// NEIGHBOUR EVALUATION
	
	// BIOME
	public bool IsBiomeSea(int x, int y)
	{
		float seaLevel = 0.045f;
		return GetElevation(x, y) <= seaLevel;
	}
	
	// TIERS
	public int GetValueTier(float value)
	{
		//  para valores en el rango 0-1
		for (var i = 0; i < (int) GetWorldParameter("NTiers"); i++){if (value < (i + 1.0f)/(float) GetWorldParameter("NTiers")){return i;}}
		return (int) GetWorldParameter("NTiers") - 1;
	}

	public int GetValueTierAt(int x, int y, string property = "")
	{
		return GetValueTier(GetElevation(x, y));
	}

}