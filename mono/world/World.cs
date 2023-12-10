using System.Collections.Generic;
using System.Text.Json;
using Godot;
using Tartheside.mono.utilities.random;
using Tartheside.mono.world.generators;

namespace Tartheside.mono.world;

public class World
{
	private Dictionary<string, Variant> _worldParameters;
	private Dictionary<string, MFNL> _worldNoises;
	private Dictionary<string, BaseGenerator> _worldGenerators;

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
		//TODO: usar las X e Y que están en el json y cambiar las llamadas al parámetro "WorldSize"
		UpdateWorldParameter("WorldSize", new Vector2I((int) GetWorldParameter("WorldSizeX"), 
			(int) GetWorldParameter("WorldSizeY")));	
		UpdateWorldParameter("ChunkSize", new Vector2I((int) GetWorldParameter("ChunkSizeX"), 
			(int) GetWorldParameter("ChunkSizeY")));
		UpdateWorldParameter("SquareSize", new Vector2I((int) GetWorldParameter("SquareSizeX"), 
			(int) GetWorldParameter("SquareSizeY")));
		UpdateWorldParameter("InitChunks", new Vector2I((int) GetWorldParameter("InitChunksX"), 
			(int) GetWorldParameter("InitChunksY")));
		UpdateWorldParameter("Offset", new Vector2I((int) GetWorldParameter("OffsetX"), 
			(int) GetWorldParameter("OffsetY")));
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

	private void InitWorldGenerators() => _worldGenerators = new Dictionary<string, BaseGenerator>();

	private void LoadParametersFromJson()
	{
		var file = FileAccess.Open("res://resources/worlddata/worldparams.json", FileAccess.ModeFlags.Read);
		var noiseDict = JsonSerializer.Deserialize<Dictionary<string, string>>(file.GetAsText());
		foreach (var kvp in noiseDict) AddWorldParameter(kvp.Key,kvp.Value);
	}
	

	//  WORLD GENERATORS
	public void AddWorldGenerator(string generatorName, BaseGenerator generator) => 
		_worldGenerators[generatorName] = generator;
	
	public BaseGenerator GetWorldGenerator(string generator) => 
		_worldGenerators.ContainsKey(generator) ? _worldGenerators[generator] : null;

	private void SetGlobalGeneratorParameters(BaseGenerator generator)
	{
		// parámetros presentes en todos los generadores (atributos de la clase madre BaseGenerator)
		// puede ser un problema si se se actualizan los valores en el world después de inicializarse
		// por eso, creamos UpdateGlobalGeneratorParameters
		// Ninguno de estos valores debe cambiarse una vez el mundo se ha creado
		
		// TODO: quitar
		generator.SetParameterWorldSize((Vector2I) GetWorldParameter("WorldSize"));
		generator.SetParameterNTiers((int) GetWorldParameter("NTiers"));
		generator.SetParameterChunkSize((Vector2I) GetWorldParameter("ChunkSize"));
		generator.SetParameterOffset(new Vector2I((int) GetWorldParameter("OffsetX"), 
			(int) GetWorldParameter("OffsetY")));
	}

	private void UpdateGlobalGeneratorsParameters()
	{
		foreach (var generator in GetWorldGenerators().Values)
			SetGlobalGeneratorParameters(generator);	// TODO: si implementamos el BaseGenerator.Setup(), cambiar la llamada
	}

	public Dictionary<string, BaseGenerator> GetWorldGenerators() => _worldGenerators;

	// init world generators TODO: (¿no debería ir mejor en el manager? o en un método setup dentro del propio generator)
	public void InitElevation()
	{	
		var elevationGenerator = new Elevation(
			new Vector2I((int) GetWorldParameter("WorldSizeX"), (int) GetWorldParameter("WorldSizeY")), 
			(Vector2I) GetWorldParameter("ChunkSize"), 
			new Vector2I((int) GetWorldParameter("OffsetX"), (int) GetWorldParameter("OffsetY")), 
			(int) GetWorldParameter("NTiers"));
		
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
		var riverGenerator = new River(
			new Vector2I((int) GetWorldParameter("WorldSizeX"), (int) GetWorldParameter("WorldSizeY")), 
			(Vector2I) GetWorldParameter("ChunkSize"), 
			new Vector2I((int) GetWorldParameter("OffsetX"), (int) GetWorldParameter("OffsetY")), 
			(int) GetWorldParameter("NTiers"));
		
		riverGenerator.SetParameterElevation((Elevation) GetWorldGenerator("Elevation"));
		riverGenerator.SetParameterRiverPathfindingElevationPenalty(
			(float)GetWorldParameter("RiverPathfindingElevationPenalty"));
		
		// TODO: hacer en el propio river???
		riverGenerator.PathfindingAStarSetup();
		riverGenerator.SetParameterContinentalness(GetWorldNoise("Continentalness"));
		riverGenerator.GenerateRiver(new Vector2I(86584, 796), new Vector2I(86549, 753));
		AddWorldGenerator("River", riverGenerator);
	}

	public void InitLatitude()
	{
		var latitudeGenerator = new Latitude(
			new Vector2I((int) GetWorldParameter("WorldSizeX"), (int) GetWorldParameter("WorldSizeY")), 
			(Vector2I) GetWorldParameter("ChunkSize"), 
			new Vector2I((int) GetWorldParameter("OffsetX"), (int) GetWorldParameter("OffsetY")), 
			(int) GetWorldParameter("NTiers"));
		
		latitudeGenerator.SetParameterEquatorLine((int) GetWorldParameter("EquatorLine"));
		latitudeGenerator.SetParameterCancerTropicLine((int) GetWorldParameter("CancerTropicLine"));
		latitudeGenerator.SetParameterCapricornTropicLine((int) GetWorldParameter("CapricornTropicLine"));
		latitudeGenerator.SetParameterNorthSubtropicLine((int) GetWorldParameter("NorthSubtropicLine"));
		latitudeGenerator.SetParameterSouthSubtropicLine((int) GetWorldParameter("SouthSubtropicLine"));
		latitudeGenerator.SetParameterArcticCircleLine((int) GetWorldParameter("ArcticCircleLine"));
		latitudeGenerator.SetParameterAntarcticCircleLine((int) GetWorldParameter("AntarcticCircleLine"));

		latitudeGenerator.FillValueMatrix((int) GetWorldParameter("OffsetX"), 
			(int) GetWorldParameter("OffsetY"));
		AddWorldGenerator("Latitude", latitudeGenerator);
	}

	public void InitTemperature()
	{
		var temperatureGenerator = new Temperature(
			new Vector2I((int) GetWorldParameter("WorldSizeX"), (int) GetWorldParameter("WorldSizeY")), 
			(Vector2I) GetWorldParameter("ChunkSize"), 
			new Vector2I((int) GetWorldParameter("OffsetX"), (int) GetWorldParameter("OffsetY")), 
			(int) GetWorldParameter("NTiers"));
		temperatureGenerator.SetParameterElevation((Elevation) GetWorldGenerator("Elevation"));
		temperatureGenerator.SetParameterLatitude((Latitude) GetWorldGenerator("Latitude"));
		
		temperatureGenerator.FillValueMatrix((int) GetWorldParameter("OffsetX"), 
			(int) GetWorldParameter("OffsetY"));
		
		AddWorldGenerator("Temperature", temperatureGenerator);
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
	public void AddWorldNoise(string name, MFNL noise) => _worldNoises.Add(name, noise);

	public MFNL GetWorldNoise(string name) => _worldNoises.TryGetValue(name, out var noise) ? noise : null;

	public Dictionary<string, MFNL> GetWorldNoises() => _worldNoises;
	
	private void RandomizeWorld()
	{
		foreach (MFNL noise in _worldNoises.Values) noise.RandomizeSeed();  
	}
	
}
