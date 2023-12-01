using System.Collections.Generic;
using System.Text.Json;
using Godot;
using Tartheside.mono.utilities.random;
using Tartheside.mono.world.generators;

namespace Tartheside.mono.world;

public class World
{
	// WORLD PARAMETERS AND NOISES
	private Dictionary<string, Variant> _worldParameters;
	private Dictionary<string, MFNL> _worldNoises;
	private Dictionary<string, WorldGenerator> _worldGenerators;

	// CONSTRUCTOR
	public World()
	{
		InitParameters();
		InitNoises();
		InitWorldGenerators();
	}

	private void InitParameters()
	{
		_worldParameters = new Dictionary<string, Variant>();
		LoadParametersFromJson();
		UpdateWorldParameter("WorldSize", new Vector2I((int) GetWorldParameter("WorldSizeX"), 
			(int) GetWorldParameter("WorldSizeY")));	//TODO: usar las X e Y que están en el json y cambiar las llamadas al parámetro "WorldSize"
	}
	
	private void InitNoises()
	{
		_worldNoises = new Dictionary<string, MFNL>();
		
		var baseElevation = new MFNL("BaseElevation", (int) GetWorldParameter("NTiers"));
		baseElevation.LoadFromJson("BaseElevation");
		AddWorldNoise("BaseElevation", baseElevation);

		var continentalness = new MFNL("Continentalness", (int) GetWorldParameter("NTiers"));
		continentalness.LoadFromJson("Continentalness");
		AddWorldNoise("Continentalness", continentalness);

		var peaksAndValleys = new MFNL("PeaksAndValleys", (int) GetWorldParameter("NTiers"));
		peaksAndValleys.LoadFromJson("PeaksAndValleys");
		AddWorldNoise("PeaksAndValleys", peaksAndValleys);

		var volcanicIslands = new MFNL("VolcanicIslands", (int) GetWorldParameter("NTiers"));
		volcanicIslands.LoadFromJson("VolcanicIslands");
		AddWorldNoise("VolcanicIslands", volcanicIslands);
	}

	private void InitWorldGenerators()
	{
		_worldGenerators = new Dictionary<string, WorldGenerator>();
	}

	private void LoadParametersFromJson()
	{
		var file = FileAccess.Open("res://resources/worlddata/worldparams.json", FileAccess.ModeFlags.Read);
		var noiseDict = JsonSerializer.Deserialize<Dictionary<string, string>>(file.GetAsText());
		foreach (var kvp in noiseDict) AddWorldParameter(kvp.Key,kvp.Value);
	}
	

	//  WORLD GENERATORS
	private void AddWorldGenerator(string generatorName, WorldGenerator generator) => 
		_worldGenerators[generatorName] = generator;
	
	public WorldGenerator GetWorldGenerator(string generator) => 
		_worldGenerators.ContainsKey(generator) ? _worldGenerators[generator] : null;

	private void SetGlobalGeneratorParameters(WorldGenerator generator)
	{
		// parámetros presentes en todos los generadores (atributos de la clase madre WorldGenerator)
		// puede ser un problema si se se actualizan los valores en el world después de inicializarse
		// por eso, creamos UpdateGlobalGeneratorParameters
		// Ninguno de estos valores debe cambiarse una vez el mundo se ha creado
		
		// TODO: quitar
		generator.SetParameterWorldSize(new Vector2I((int) GetWorldParameter("WorldSizeX"), (int) GetWorldParameter("WorldSizeY")));
		generator.SetParameterNTiers((int) GetWorldParameter("NTiers"));
		generator.SetParameterChunkSize((Vector2I) GetWorldParameter("ChunkSize"));
		generator.SetParameterOffset(new Vector2I((int) GetWorldParameter("OffsetX"), (int) GetWorldParameter("OffsetY")));
	}

	private void UpdateGlobalGeneratorsParameters()
	{
		foreach (var generator in GetWorldGenerators().Values)
			SetGlobalGeneratorParameters(generator);
	}

	public Dictionary<string, WorldGenerator> GetWorldGenerators() => _worldGenerators;

	// init world generators (¿no debería ir mejor en el manager?)
	public void InitLatitude()
	{
		Latitude latitudeGenerator = new Latitude((int) GetWorldParameter("WorldSizeX"), 
			(int) GetWorldParameter("WorldSizeY"));
		SetGlobalGeneratorParameters(latitudeGenerator);
		latitudeGenerator.SetParameterEquatorLine((int) GetWorldParameter("EquatorLine"));
		AddWorldGenerator("Latitude", latitudeGenerator);
		//TODO: intervalos de regiones como parámetros
	}
	
	public void InitTemperature()
	{
		Temperature temperatureGenerator = new Temperature((int) GetWorldParameter("WorldSizeX"), 
			(int) GetWorldParameter("WorldSizeY"));
		SetGlobalGeneratorParameters(temperatureGenerator);
		temperatureGenerator.SetParameterElevation((Elevation) GetWorldGenerator("Elevation"));
		temperatureGenerator.SetParameterLatitude((Latitude) GetWorldGenerator("Latitude"));
		
		AddWorldGenerator("Temperature", temperatureGenerator);
	}
	
	public void InitElevation()
	{	
		var elevationGenerator = new Elevation((int) GetWorldParameter("WorldSizeX"), 
			(int) GetWorldParameter("WorldSizeY"));
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
		
		elevationGenerator.FillValueMatrix((int) GetWorldParameter("OffsetX"), 
			(int) GetWorldParameter("OffsetY"));
		AddWorldGenerator("Elevation", elevationGenerator);
	}

	public void InitRiver()
	{
		var riverGenerator = new River((int) GetWorldParameter("WorldSizeX"), 
			(int) GetWorldParameter("WorldSizeY"));
		SetGlobalGeneratorParameters(riverGenerator);
		riverGenerator.SetParameterElevation((Elevation) GetWorldGenerator("Elevation"));
		
		// TODO: hacer en el propio river???
		riverGenerator.SetPathfindingAStar();
		riverGenerator.SetParameterContinentalness(GetWorldNoise("Continentalness"));
		riverGenerator.GenerateRiverAStar(new Vector2I(86584, 796), new Vector2I(86549, 753));
		AddWorldGenerator("River", riverGenerator);
	}
	
	public void InitHumidity()
	{
		Humidity humidityGenerator = new Humidity((int) GetWorldParameter("WorldSizeX"), 
			(int) GetWorldParameter("WorldSizeY"));
		SetGlobalGeneratorParameters(humidityGenerator);
		humidityGenerator.SetParameterElevation((Elevation) GetWorldGenerator("Elevation"));
		humidityGenerator.SetParameterContinentalness(GetWorldNoise("Continentalness"));
		humidityGenerator.SetParameterRiver((River) GetWorldGenerator("River"));
		humidityGenerator.SetParameterContinentalnessFactor((float) GetWorldParameter("HumidityContinentalnessFactor"));
		humidityGenerator.SetParameterRiverFactor((float) GetWorldParameter("HumidityRiverFactor"));
		AddWorldGenerator("Humidity", humidityGenerator);
	}
	
	// HEIGHTMAP
	public void AddHeightMap(string generatorName, string filename)
	{
		// podremos utilizar heightmaps deterministas para distintos propoósitos
		HeightMap heightMapGenerator = new HeightMap((int) GetWorldParameter("WorldSizeX"), 
			(int) GetWorldParameter("WorldSizeY"));
		heightMapGenerator.LoadHeighMap(filename);
		SetGlobalGeneratorParameters(heightMapGenerator);
		AddWorldGenerator(generatorName, heightMapGenerator);
	}
	
	//  WORLD PARAMETERS
	private void AddWorldParameter(string param, Variant value)
	{
		if (_worldParameters.ContainsKey(param))
			UpdateWorldParameter(param, value);
		else
			_worldParameters[param] = value;
	} 

	public void UpdateWorldParameter(string param, Variant value) 
	{
		if (_worldParameters.ContainsKey(param))
		{
			_worldParameters[param] = value;
			UpdateGlobalGeneratorsParameters();
			foreach (var generator in _worldGenerators.Values)
				generator.Call("SetParameter"+param, value);
		}
		else
			AddWorldParameter(param, value);	// TODO: ¿se deberia poder desde fuera? AddWorldParameter es privado  
	}

	public Variant GetWorldParameter(string param) => _worldParameters[param];

	public Dictionary<string, Variant> GetWorldParameters() => _worldParameters;

	
	//  WORLD NOISE
	private void AddWorldNoise(string name, MFNL noise) => _worldNoises.Add(name, noise);

	public MFNL GetWorldNoise(string name)
	{
		if (_worldNoises.TryGetValue(name, out var noise)) {return noise;}
		return null;
	}

	public Dictionary<string, MFNL> GetWorldNoises() => _worldNoises;
	
	private void RandomizeWorld()
	{
		foreach (MFNL noise in _worldNoises.Values)
			noise.RandomizeSeed();  
	}
	


}
