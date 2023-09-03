using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Tartheside.mono;

public partial class World : GodotObject
{
	// WORLD PARAMETERS AND NOISES
	private Dictionary<string, Variant> _worldParameters;
	private Dictionary<string, MFNL> _worldNoises;
	private Dictionary<string, WorldGenerator> _worldGenerators;

	// CONSTRUCTOR
	public World()
	{
		InitNoisesAndParameters();
		InitWorldGenerators();
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
		
		AddWorldParameter("WorldSize", new Vector2I(1024, 1024));
		AddWorldParameter("NTiers", 24);
		AddWorldParameter("ChunkSize", new Vector2I(16, 16));
		AddWorldParameter("MinContinentalHeight", 0.023f);
		AddWorldParameter("ContinentalScaleValue", 1.22f);
		AddWorldParameter("SeaScaleValue", 1f/0.85f);
		AddWorldParameter("IslandScaleValue", 0.81f);
		AddWorldParameter("IslandThresholdLevel", 0.76f); 
		AddWorldParameter("OutToSeaFactor", 0.7f);  
	}
	
	//  WORLD GENERATORS
	private void InitWorldGenerators()
    {
        _worldGenerators = new Dictionary<string, WorldGenerator>();
		InitElevation();
		InitTemperature();
	}

	private void InitTemperature()
	{
		Temperature temperatureGenerator = new Temperature();
		temperatureGenerator.SetParameterEquatorLine((((Vector2I)GetWorldParameter("WorldSize")).Y / 2));
		temperatureGenerator.SetParameterNTiers((int) GetWorldParameter("NTiers"));
		temperatureGenerator.SetParameterChunkSize((Vector2I) GetWorldParameter("ChunkSize"));
			
		_worldGenerators.Add("Temperature", temperatureGenerator);
	}
	private void InitElevation()
	{	
		Elevation elevationGenerator = new Elevation();
		elevationGenerator.SetNoiseBaseElevation(_worldNoises["BaseElevation"]);
		elevationGenerator.SetNoiseContinentalness(_worldNoises["Continentalness"]);
		elevationGenerator.SetNoisePeaksAndValleys(_worldNoises["PeaksAndValleys"]);
		elevationGenerator.SetNoiseVolcanicIslands(_worldNoises["VolcanicIslands"]);
		
		elevationGenerator.SetParameterChunkSize((Vector2I) _worldParameters["ChunkSize"]);
		elevationGenerator.SetParameterNTiers((int) _worldParameters["NTiers"]);
		elevationGenerator.SetParameterMinContinentalHeight((float) _worldParameters["MinContinentalHeight"]);
		elevationGenerator.SetParameterContinentalScaleValue((float) _worldParameters["ContinentalScaleValue"]);
		elevationGenerator.SetParameterSeaScaleValue((float) _worldParameters["SeaScaleValue"]);
		elevationGenerator.SetParameterIslandScaleValue((float) _worldParameters["IslandScaleValue"]);
		elevationGenerator.SetParameterIslandThresholdLevel((float) _worldParameters["IslandThresholdLevel"]);
		elevationGenerator.SetParameterOutToSeaFactor((float) _worldParameters["OutToSeaFactor"]);

		_worldGenerators.Add("Elevation", elevationGenerator);
	}

	public WorldGenerator GetWorldGenerator(string generator) => _worldGenerators.ContainsKey(generator) ? _worldGenerators[generator] : null;
	
	public Dictionary<string, WorldGenerator> GetWorldGenerators() => _worldGenerators;
	
	//  WORLD PARAMETERS
	public void AddWorldParameter(string param, Variant value)
	{
		if (_worldParameters.ContainsKey(param))
		{
			UpdateWorldParameter(param, value);
		}
		else
		{
			_worldParameters[param] = value;
		}
	} 

	public void RemoveWorldParameter(string param)
	{
		if (_worldParameters.ContainsKey(param))
		{
            bool v = _worldParameters.Remove(param);
        }
	}

	public void UpdateWorldParameter(string param, Variant value) 
	{
		if (_worldParameters.ContainsKey(param)) 
		{
			_worldParameters[param] = value;
		}
	}

	public Variant GetWorldParameter(string param) => _worldParameters[param];

	public Dictionary<string, Variant> GetWorldParameters() => _worldParameters;

	//  WORLD NOISE
	private void AddWorldNoise(string name, MFNL noise) => _worldNoises.Add(name, noise);

	private void RemoveWorldNoise(string name)
	{
        if (_worldNoises.ContainsKey(name.StripEdges()))
        {
			_worldNoises.Remove(name);
        }
    }

	private MFNL GetWorldNoise(string name)
	{
		if (_worldNoises.ContainsKey(name)) {return _worldNoises[name];}
		return null;
	}

	public Dictionary<string, MFNL> GetWorldNoises() => _worldNoises;

	private void RandomizeWorld()
	{
		foreach (MFNL noise in _worldNoises.Values)
		{
			noise.RandomizeSeed();  
		}
	}
	
	
	// TERRAIN (esto sí va en World)
	public bool IsTerrainSea(int x, int y)
	{
		// podríamos desomponer el tier 0 para distinguir varios niveles de profundidad de mar, obtener bioma orilla (mareas), etc
		return _worldGenerators["Elevation"].GetValueTierAt(x, y) == 0;
	}

	public bool IsTerrainBeach(int x, int y)
	{
		return _worldGenerators["Elevation"].GetValueTierAt(x, y) == 1;
	}

	public bool IsTerrainLowland(int x, int y)
	{
		return _worldGenerators["Elevation"].GetValueTierAt(x, y) == 2;
	}

	public bool IsTerrainMineral(int x, int y)
	{
		int minimumMountainRockElevationTier = 3; 	// convertir en parámetro del mundo
		bool isAboveMinimunElevation = _worldGenerators["Elevation"].GetValueTierAt(x, y) >= minimumMountainRockElevationTier;
		bool isAboveMinimumPeaksAndValleys = GetWorldNoise("PeaksAndValleys").GetNormalizedNoise2D(x, y) > 0.25;	// convertir en parámetro del mundo
		bool isSlopeRight = (_worldGenerators["Elevation"].IsStepDownAtOffset(x, y, 1, 0) && _worldGenerators["Elevation"].IsStepDownAtOffset(x, y, 2, 0));
		bool isSlopeLeft = (_worldGenerators["Elevation"].IsStepDownAtOffset(x, y, -1, 0) && _worldGenerators["Elevation"].IsStepDownAtOffset(x, y, -2, 0));
		bool isSlopeUp = (_worldGenerators["Elevation"].IsStepDownAtOffset(x, y, 0, -1) && _worldGenerators["Elevation"].IsStepDownAtOffset(x, y, 0, -2));
		bool isSlopeDown = (_worldGenerators["Elevation"].IsStepDownAtOffset(x, y, 0, 1) && _worldGenerators["Elevation"].IsStepDownAtOffset(x, y, 0, 2));	// con la pendiente que se exija podemos regular la densidad
		// comprobar pendientes creo que no detecta bien los cabmios bruscos, aunque queda bien en el mapa así...

		// se le podría aplicar después un filtrado on un objeto de ruido muy granulado,que ayude a simular las vetas de mineral
		// ¿qué pasa cuando coincide con roca u otra cosa?
		// tenemos que poder regular la densidad de mineral (y de roca y de todo), quizá con un parámetro de persistencia del ruido
		return isAboveMinimunElevation && isAboveMinimumPeaksAndValleys && ((isSlopeRight || isSlopeLeft) && (isSlopeDown || isSlopeUp));
	}

	public bool IsTerrainRiver(int x, int y)
	{
		return false;
	}

	// CHUNKS
	public Vector2I GetChunkByWorldPosition(int x, int y)
	{
		return new Vector2I((int) Math.Floor((double) (x / ((Vector2I) GetWorldParameter("ChunkSize")).X)), (int) Math.Floor((double) (y / ((Vector2I) GetWorldParameter("ChunkSize")).Y)));
	}

	public float GetRandomFloatByPosition(int x, int y)		// llevar a clase Rng
	{
		RandomNumberGenerator rng = new RandomNumberGenerator();
		rng.Seed = 1308;
		return rng.Randf();
	}
}
