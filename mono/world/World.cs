using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using System.Reflection;

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
	
	// podriamos usar un rng para obtener valores uniformes aleatorios pero determinsitas para un x,y


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
		AddWorldParameter("ChunkSize", new Vector2I(16, 16));
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

	public Dictionary<String, MFNL> GetWorldNoises()
	{
		return _worldNoises;
	}

	public Dictionary<string, Variant> GetWorldParameters()
	{
		return _worldParameters;
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


	// NEIGHBOUR EVALUATION (untested)
	public bool IsStepDownAtOffset(int x, int y, int xOffset = 0, int yOffset = 0, string property = "")
	{
		return GetValueTierAt(x, y, property) > GetValueTierAt(x + xOffset, y + yOffset, property);
	}

	public bool IsStepUpAtOffset(int x, int y, int xOffset = 0, int yOffset = 0, string property = "")
	{
		return GetValueTierAt(x, y, property) < GetValueTierAt(x + xOffset, y + yOffset, property);
	}

	public bool IsStepAtOffset(int x, int y, int xOffset = 0, int yOffset = 0, string property = "")
	{
		return !IsNoStepAtOffset(x, y, xOffset, yOffset, property);
	}

	public bool IsNoStepAtOffset(int x, int y, int xOffset = 0, int yOffset = 0, string property = "")
	{
		return GetValueTierAt(x, y, property) == GetValueTierAt(x + xOffset, y + yOffset, property);
	}


	// TERRAIN
	public bool IsTerrainSea(int x, int y)
	{
		// podríamos desomponer el tier 0 para distinguir varios niveles de profundidad de mar, obtener bioma orilla (mareas), etc
		return GetValueTierAt(x, y, "GetElevation") == 0;
	}

	public bool IsTerrainBeach(int x, int y)
	{
		return GetValueTierAt(x, y, "GetElevation") == 1;
	}

	public bool IsTerrainRock(int x, int y)
	{
		return IsTerrainMountainRock(x, y);
	}

	public bool IsTerrainMountainRock(int x, int y)
	{
		bool isAboveMinimunElevation = GetValueTierAt(x, y) > 5;
		bool isAboveMinimumPeaksAndValleys = GetWorldNoise("PeaksAndValleys").GetNormalizedNoise2D(x, y) > 0.3;
		bool isSlopeRight = (IsStepDownAtOffset(x, y, 1, 0) && IsStepDownAtOffset(x, y, 2, 0));
		bool isSlopeLeft = (IsStepDownAtOffset(x, y, -1, 0) && IsStepDownAtOffset(x, y, -2, 0));
		bool isSlopeUp = (IsStepDownAtOffset(x, y, 0, -1) && IsStepDownAtOffset(x, y, 0, -2));
		bool isSlopeDown = (IsStepDownAtOffset(x, y, 0, 1) && IsStepDownAtOffset(x, y, 0, 2));	// con la pendiente que se exija podemos regular la densidad

		return isAboveMinimunElevation && isAboveMinimumPeaksAndValleys && ((isSlopeRight || isSlopeLeft) && (isSlopeDown || isSlopeUp));
	}

	public bool IsTerrainRiver(int x, int y)
	{
		// Si un punto cumple con los requisitos para ser el nacimiento de unr ío, lo será (entre otras cosas, que no haya muchos nacimientos cerca)

		// Para cada punto de nacimiento, se obtendrá el punto de desembocadura de forma determinista, en función de condiciones de pendiente y cercanía del mar
	
		// El camino del río se obtendrá también de forma determinista, utilizando A* y con criterios deterministas para establecer los obstáculos.

		return false;
	}

	// RIVERS
	public bool IsValidRiverBirth(int x, int y)
	{

	}

	
	// CHUNKS
	public Vector2I GetChunkByWorldPosition(int x, int y)
	{
		return new Vector2I((int) Math.Floor((double) (x / ((Vector2I) GetWorldParameter("ChunkSize")).X)), (int) Math.Floor((double) (y / ((Vector2I) GetWorldParameter("ChunkSize")).Y)));
	}



	// TIERS
	private int GetValueTier(float value)
	{
		//  para valores en el rango 0-1
		for (var i = 0; i < (int) GetWorldParameter("NTiers"); i++){if (value < (i + 1.0f)/(float) GetWorldParameter("NTiers")){return i;}}
		return (int) GetWorldParameter("NTiers") - 1;
	}

	public int GetValueTierAt(int x, int y, string property = "")
	{
		// si property coincide con alguno de los noises, devolvemos el valor de dicho noise
		if (_worldNoises.Keys.Contains(property))
		{
			return GetValueTier(GetWorldNoise(property).GetNormalizedNoise2D(x, y));
		} 
		// si coincide con alguno de los metodos Is_ o Get_, devolvemos el valor
		if (property.Contains("Get"))
		{
			try {
				return GetValueTier((float) typeof(World).GetMethod(property).Invoke(this, new object[] { x, y }));
			} catch (NullReferenceException) {
				return GetValueTier(GetElevation(x, y));
			}
		}

		if (property.Contains("Is"))
		{
			try
			{
				return GetValueTier(((bool) typeof(World).GetMethod(property).Invoke(this, new object[] { x, y }) ? 0.70f : 0.0f ));
			} catch (NullReferenceException) {
				return GetValueTier(GetElevation(x, y));
			}
		}
		return GetValueTier(GetElevation(x, y));
	}

	public int GetValueTierAt(Vector2I pos, string property = "")
	{
		return GetValueTierAt(pos.X, pos.Y, property);
	}
	
	
	// no debería ser GetValueTier, sino GetElevationTier, GetTemperatureTier, etc.
}