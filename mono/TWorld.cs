using Godot;
using System;
using System.Collections.Generic;

public partial class TWorld : GodotObject
{
	// WORLD PARAMETERS AND NOISES
	// public Vector2I WorldSize;
	public Dictionary<string, Variant> WorldParameters;
	public Dictionary<string, MFNL> WorldNoises;



	// CONSTRUCTOR
	public TWorld(int worldsizeX, int worldSizeY)
	{
		// SetWorldSize(worldsizeX, worldSizeY);
        InitNoisesAndParameters();
	}

	//  INIT METHODS
	// public void SetWorldSize(int newWorldsizeX, int newWorldSizeY)
	// {
	// 	WorldSize = new Vector2I(newWorldsizeX, newWorldSizeY);
	// }

	public void InitNoisesAndParameters()
	{
		
		// Init noise
		WorldNoises = new Dictionary<string, MFNL>();
		
		// Formamos los objetos de ruido desde los .json correspondientes
		MFNL BaseElevation = new MFNL("BaseElevation");
		BaseElevation.LoadFromJSON("BaseElevation");
		AddWorldNoise("BaseElevation", BaseElevation);

		MFNL Continentalness = new MFNL("Continentalness");
		Continentalness.LoadFromJSON("Continentalness");
		AddWorldNoise("Continentalness", Continentalness);

		MFNL PeaksAndValleys = new MFNL("PeaksAndValleys");
		PeaksAndValleys.LoadFromJSON("PeaksAndValleys");
		AddWorldNoise("PeaksAndValleys", PeaksAndValleys);

		MFNL VolcanicIslands = new MFNL("VolcanicIslands");
		VolcanicIslands.LoadFromJSON("VolcanicIslands");
		AddWorldNoise("VolcanicIslands", VolcanicIslands);


		// Init parameters (cargar desde json)
		WorldParameters = new Dictionary<string, Variant>();
		
		AddWorldParameter("MinContinentalHeight", 0.023f);
		AddWorldParameter("ContinentalScaleValue", 1.22f);
		AddWorldParameter("SeaScaleValue", 1f/0.85f);
		AddWorldParameter("IslandScaleValue", 0.81f);
		AddWorldParameter("IslandThresholdLevel", 0.76f); 
		AddWorldParameter("OutToSeaFactor", 0.7f);  

	}

	//  WORLD PARAMETERS
	public void AddWorldParameter(string param, Variant value)
	{
		WorldParameters.Add(param, value);
	}
	public void UpdateWorldParameter(string param, Variant value)
	{
		if (WorldParameters.ContainsKey(param)) {WorldParameters[param] = value;}
	}
	public Variant GetWorldParameter(string param)
	{
		return WorldParameters[param];
	}

	//  WORLD NOISE
	public void AddWorldNoise(string name, MFNL noise)
	{
		WorldNoises.Add(name, noise);
	}
	public MFNL GetWorldNoise(string name)
	{
		if (!WorldNoises.ContainsKey(name)) {return null;}
		return WorldNoises[name];
	}
	public void RandomizeWorld()
	{
		// randomiza las semillas de todos los objetos de ruido
		foreach (MFNL noise in WorldNoises.Values)
		{
			noise.RandomizeSeed();  // se le puede pasar una semilla en concreto
		}
	}



	// ELEVATION
	public float GetElevation(int x, int y)
	{
		// algoritmo marzo '23
		if (IsOverSeaLevel(x, y))
		{   
			if (IsVolcanicIsland(x, y)) // celdas de islas volcánicas
			{
				return GetVolcanicIslandElevation(x, y);
			}
			else // celdas continentales
			{
				return GetContinentalElevation(x, y);            
			}
		}
		else{   // celdas de mar
			return 0.0f;
		}
	}
	
	public float GetVolcanicIslandElevation(int x, int y)
	{
		return (GetWorldNoise("PeaksAndValleys").GetNormalizedNoise2D(x, y) * (float) GetWorldParameter("ContinentalScaleValue") * GetWorldNoise("BaseElevation").GetNormalizedNoise2D(x, y) * GetWorldNoise("VolcanicIslands").GetNormalizedNoise2D(x, y) * (float) GetWorldParameter("IslandScaleValue"));
	}
	public float GetContinentalElevation(int x, int y)
	{
		return Math.Min(1f, Math.Max((float) GetWorldParameter("MinContinentalHeight"), GetWorldNoise("PeaksAndValleys").GetNormalizedNoise2D(x, y) * (float) GetWorldParameter("ContinentalScaleValue") * (GetWorldNoise("BaseElevation").GetNormalizedNoise2D(x, y) - (float) GetWorldParameter("SeaScaleValue") * GetWorldNoise("Continentalness").GetNormalizedNoise2D(x, y))));    // devolvemos un valor en el rango [MinContinentalHeight, 1.0]
	}
	
	public bool IsOverSeaLevel(int x, int y)
	{
		return IsVolcanicIsland(x, y) || IsContinentalOverSeaLevel(x, y);
	}
	public bool IsContinentalOverSeaLevel(int x, int y)
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




	public int GetValueTier(float value, int nTiers=48)
	{
		//  para valores en el rango 0-1
		for (int i = 0; i < nTiers; i++){if (value < ((float) i + 1.0f)/(float) nTiers){return i;}}
		return nTiers-1;
	}

}
