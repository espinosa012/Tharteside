using System;
using System.Collections.Generic;
using Godot;
using MathNet;
namespace Tartheside.mono;

public class World
{
	// WORLD PARAMETERS AND NOISES
	private Dictionary<string, Variant> _worldParameters;
	private Dictionary<string, MFNL> _worldNoises;
	private Dictionary<string, WorldGenerator> _worldGenerators;

	// CONSTRUCTOR
	public World()
	{
		InitNoises();
		InitParameters();
		InitWorldGenerators();
	}

	private void InitNoises()
	{
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
	}

	private void InitParameters()
	{
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
	
	private void InitWorldGenerators()
    {
        _worldGenerators = new Dictionary<string, WorldGenerator>();
		/*InitElevation();
		InitTemperature();
		InitTerrain();
		InitHeightMap();*/
    }

	//  WORLD GENERATORS
	public void SetGlobalGeneratorParameters(WorldGenerator generator)
	{
		// parámetros presentes en todos los generadores (atributos de la clase madre WorldGenerator)
		// puede ser un problema si se se actualizan los valores en el world después de inicializarse
		// por eso, creamos UpdateGlobalGeneratorParameters
		generator.SetParameterWorldSize((Vector2I) GetWorldParameter("WorldSize"));
		generator.SetParameterNTiers((int) GetWorldParameter("NTiers"));
		generator.SetParameterChunkSize((Vector2I) GetWorldParameter("ChunkSize"));
	}

	private void UpdateGlobalGeneratorsParameters()
	{
		foreach (var generator in GetWorldGenerators().Values)
		{
			SetGlobalGeneratorParameters(generator);
		}
	}
	
	
	public void InitTemperature()
	{
		Temperature temperatureGenerator = new Temperature();
		SetGlobalGeneratorParameters(temperatureGenerator);
		temperatureGenerator.SetParameterEquatorLine((((Vector2I)GetWorldParameter("WorldSize")).Y / 2));
			
		AddWorldGenerator("Temperature", temperatureGenerator);
	}
	
	public void InitElevation()
	{	
		Elevation elevationGenerator = new Elevation();
		SetGlobalGeneratorParameters(elevationGenerator);
		elevationGenerator.SetParameterBaseElevationNoise(_worldNoises["BaseElevation"]);
		elevationGenerator.SetParameterContinentalnessNoise(_worldNoises["Continentalness"]);
		elevationGenerator.SetParameterPeaksAndValleysNoise(_worldNoises["PeaksAndValleys"]);
		elevationGenerator.SetParameterVolcanicIslandsNoise(_worldNoises["VolcanicIslands"]);
		elevationGenerator.SetParameterMinContinentalHeight((float) _worldParameters["MinContinentalHeight"]);
		elevationGenerator.SetParameterContinentalScaleValue((float) _worldParameters["ContinentalScaleValue"]);
		elevationGenerator.SetParameterSeaScaleValue((float) _worldParameters["SeaScaleValue"]);
		elevationGenerator.SetParameterIslandScaleValue((float) _worldParameters["IslandScaleValue"]);
		elevationGenerator.SetParameterIslandThresholdLevel((float) _worldParameters["IslandThresholdLevel"]);
		elevationGenerator.SetParameterOutToSeaFactor((float) _worldParameters["OutToSeaFactor"]);

		AddWorldGenerator("Elevation", elevationGenerator);
	}
	
	public void InitTerrain()	//deprecated
	{
		Terrain terrainGenerator = new Terrain();
		terrainGenerator.SetParameterElevation((Elevation) GetWorldGenerator("Elevation"));
		terrainGenerator.SetParameterTemperature((Temperature) GetWorldGenerator("Temperature"));
		terrainGenerator.SetParameterPeaksAndValleys(GetWorldNoise("PeaksAndValleys"));
		terrainGenerator.SetParameterMinimumPeaksAndValleysMineralSpawnValue(0.25f);
		
		AddWorldGenerator("Terrain", terrainGenerator);
	}

	public void InitHeightMap()
	{
		HeightMap heightMapGenerator = new HeightMap();
		SetGlobalGeneratorParameters(heightMapGenerator);
		AddWorldGenerator("HeightMap", heightMapGenerator);
	}
	
	public void AddWorldGenerator(string generatorName, WorldGenerator generator) => _worldGenerators[generatorName] = generator;
	
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
		if (_worldParameters.ContainsKey(param)) _worldParameters.Remove(param);
	}

	public void UpdateWorldParameter(string param, Variant value) 
	{
		if (_worldParameters.ContainsKey(param))
		{
			_worldParameters[param] = value;
			UpdateGlobalGeneratorsParameters();
		}
	}

	public Variant GetWorldParameter(string param) => _worldParameters[param];

	public Dictionary<string, Variant> GetWorldParameters() => _worldParameters;

	
	//  WORLD NOISE
	private void AddWorldNoise(string name, MFNL noise) => _worldNoises.Add(name, noise);

	private void RemoveWorldNoise(string name)
	{
        if (_worldNoises.ContainsKey(name.StripEdges()))	_worldNoises.Remove(name);
    }

	private MFNL GetWorldNoise(string name)
	{
		if (_worldNoises.TryGetValue(name, out var noise)) {return noise;}
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
	
	
	// CHUNKS
	public Vector2I GetChunkByWorldPosition(int x, int y) => new Vector2I((int) Math.Floor((double) (x / 
		((Vector2I) GetWorldParameter("ChunkSize")).X)), (int) Math.Floor((double) (y / 
		((Vector2I) GetWorldParameter("ChunkSize")).Y)));
	
	
	// UTILITIES
	public void WorldToPng()
	{
		/*
		 *	para calcular distancia al mar, detectar islas, 
		 * 
		 */
	}
}
