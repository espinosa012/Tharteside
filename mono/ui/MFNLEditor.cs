using System;
using Godot;
using Tartheside.mono.world;
using Tartheside.mono.tilemap;
using Tartheside.mono.utilities.logger;
using Tartheside.mono.utilities.random;

namespace Tartheside.mono.ui;

public partial class MFNLEditor : Control
{
	private MarginContainer _domainWarp; 
	private MarginContainer _domainWarpFractal; 
	private MarginContainer _fractal; 
	private MarginContainer _general; 
	private MarginContainer _cellular;
	private OptionButton _sourceSelector;
	
	private utilities.random.MFNL _noise;
	private TMap _tileMap;
	
	
	public override void _Ready()
	{
		_domainWarp = GetNode<MarginContainer>("%DomainWarp");
		_domainWarpFractal = GetNode<MarginContainer>("%DomainWarpFractal");
		_fractal = GetNode<MarginContainer>("%Fractal");
		_general = GetNode<MarginContainer>("%General");
		_cellular = GetNode<MarginContainer>("%Cellular");
		_sourceSelector = GetNode<OptionButton>("%SourceSelector");
		
		TileMapWindowSetUp();
		
		UiSetUp();
	}

	private void UiSetUp()
	{
		OptionButton domainWarpType =
			GetNode<OptionButton>(
				"Margin/GlobalContainer/Right/DomainWarp/MarginContainer/VBoxContainer/DomainWarpType/HBox/DomainWarpTypeInput");
		OptionButton domainWarpFractalType = 
			GetNode<OptionButton>(
				"Margin/GlobalContainer/Right/DomainWarpFractal/MarginContainer/VBoxContainer/DomainWarpFractalType/HBox/DomainWarpFractalTypeInput");
		OptionButton fractalType =
			GetNode<OptionButton>(
				"Margin/GlobalContainer/Right/Fractal/MarginContainer/VBoxContainer/FractalType/HBox/FractalTypeInput");
		OptionButton noiseType =
			GetNode<OptionButton>(
				"Margin/GlobalContainer/Left/General/MarginContainer/VBoxContainer/NoiseType/HBox/NoiseTypeInput");
		OptionButton cellularDistanceFunction = GetNode<OptionButton>(
			"Margin/GlobalContainer/Left/Cellular/MarginContainer/VBoxContainer/CellularDistanceFunction/HBox/CellularDistanceFunctionInput");
		OptionButton returnType =
			GetNode<OptionButton>(
				"Margin/GlobalContainer/Left/Cellular/MarginContainer/VBoxContainer/CellularReturnType/HBox/CellularReturnTypeInput");
		
		domainWarpType.AddItem(FastNoiseLite.DomainWarpTypeEnum.Simplex.ToString());
		domainWarpType.AddItem(FastNoiseLite.DomainWarpTypeEnum.SimplexReduced.ToString());
		domainWarpType.AddItem(FastNoiseLite.DomainWarpTypeEnum.BasicGrid.ToString());
		
		domainWarpFractalType.AddItem(FastNoiseLite.DomainWarpFractalTypeEnum.None.ToString());
		domainWarpFractalType.AddItem(FastNoiseLite.DomainWarpFractalTypeEnum.Progressive.ToString());
		domainWarpFractalType.AddItem(FastNoiseLite.DomainWarpFractalTypeEnum.Independent.ToString());
		
		fractalType.AddItem(FastNoiseLite.FractalTypeEnum.None.ToString());
		fractalType.AddItem(FastNoiseLite.FractalTypeEnum.Fbm.ToString());
		fractalType.AddItem(FastNoiseLite.FractalTypeEnum.Ridged.ToString());
		fractalType.AddItem(FastNoiseLite.FractalTypeEnum.PingPong.ToString());
		
		noiseType.AddItem(FastNoiseLite.NoiseTypeEnum.Value.ToString());
		noiseType.AddItem(FastNoiseLite.NoiseTypeEnum.ValueCubic.ToString());
		noiseType.AddItem(FastNoiseLite.NoiseTypeEnum.Perlin.ToString());
		noiseType.AddItem(FastNoiseLite.NoiseTypeEnum.Cellular.ToString());
		noiseType.AddItem(FastNoiseLite.NoiseTypeEnum.Simplex.ToString());
		noiseType.AddItem(FastNoiseLite.NoiseTypeEnum.SimplexSmooth.ToString());
		
		cellularDistanceFunction.AddItem(FastNoiseLite.CellularDistanceFunctionEnum.Euclidean.ToString());
		cellularDistanceFunction.AddItem(FastNoiseLite.CellularDistanceFunctionEnum.EuclideanSquared.ToString());
		cellularDistanceFunction.AddItem(FastNoiseLite.CellularDistanceFunctionEnum.Manhattan.ToString());
		cellularDistanceFunction.AddItem(FastNoiseLite.CellularDistanceFunctionEnum.Hybrid.ToString());
		
		returnType.AddItem(FastNoiseLite.CellularReturnTypeEnum.CellValue.ToString());
		returnType.AddItem(FastNoiseLite.CellularReturnTypeEnum.Distance.ToString());
		returnType.AddItem(FastNoiseLite.CellularReturnTypeEnum.Distance2.ToString());
		returnType.AddItem(FastNoiseLite.CellularReturnTypeEnum.Distance2Add.ToString());
		returnType.AddItem(FastNoiseLite.CellularReturnTypeEnum.Distance2Sub.ToString());
		returnType.AddItem(FastNoiseLite.CellularReturnTypeEnum.Distance2Mul.ToString());
		returnType.AddItem(FastNoiseLite.CellularReturnTypeEnum.Distance2Div.ToString());

		foreach (var noise in _tileMap.GetWorld().GetWorldNoises().Keys)
			_sourceSelector.AddItem(noise);
		foreach (var generator in _tileMap.GetWorld().GetWorldGenerators().Keys)
			_sourceSelector.AddItem(generator);	// no hay generadores, se inicializan en el manager
	}


	public void SetNoiseObject(MFNL noise)
	{
		_noise = noise;
		UpdateUI();
	}

	public void UpdateUI()
	{
		foreach (var prop in _noise.GetNoiseProperties())
			SetParameterInput(prop, _noise.GetNoiseProperty(prop));
	}

	private void SetParameterInput(string param, Variant value)
	{
		MarginContainer container;
		var relativeInputNodePath = string.Format("MarginContainer/VBoxContainer/{0}/HBox/{0}Input", param);
		
		if (param.StartsWith("DomainWarpFractal"))	container = _domainWarpFractal;
		else if (param.StartsWith("DomainWarp"))	container = _domainWarp;
		else if (param.StartsWith("Fractal"))	container = _fractal;
		else if (param.StartsWith("Cellular"))	container = _cellular;
		else container = _general;
		
		// TODO: se podría mejorar
		try {	
			if (param.Contains("Type") || param.Contains("CellularDistanceFunction"))	
				container.GetNode<OptionButton>(relativeInputNodePath).Selected = (int)value;		// TODO: indicar valor
			else if (param.Contains("Enabled"))
			{
				if (param.Contains("DomainWarp"))
					container.GetNode<CheckBox>(relativeInputNodePath).ButtonPressed = (bool) value;
				else					
					container.GetNode<CheckButton>(relativeInputNodePath).ButtonPressed = (bool) value;
			}
			else
				if (param.Contains("Octaves") || param.Contains("Seed")) // int
					container.GetNode<SpinBox>(relativeInputNodePath).Value = (int) value;
				else
					container.GetNode<SpinBox>(relativeInputNodePath).Value = (float) value;
		} catch (Exception e) {
			GD.Print("Error setting param input: " + param);
			GD.Print(e);
		}
	}
	
	// tilemap
	private void TileMapWindowSetUp()
	{
		var tileMapWindow = new Window();
		_tileMap = GD.Load<PackedScene>("res://scenes/WorldTileMap.tscn").Instantiate<TMap>();

		tileMapWindow.Size = new Vector2I(720, 720);
		tileMapWindow.Position = new Vector2I(64, 84);
		
		_tileMap.Name = "NoiseEditorTileMap";
		_tileMap.Position = new Vector2I(8, 8);

		SetWorld();
		
		tileMapWindow.AddChild(_tileMap);		
		AddChild(tileMapWindow);
		
		_tileMap.RenderChunks("RiverNoise", 1);
	}

	public void SetWorld() => _tileMap.SetWorld(new World());

	public void SetWorld(World world) => _tileMap.SetWorld(world);

	
	
	// SIGNALS
	public void _OnGenerateButtonPressed()
	{
		SetNoiseObject(new MFNL());
		
	}
	
	
}

