using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Tartheside.mono.world;
using Tartheside.mono.tilemap;
using Tartheside.mono.utilities.random;
using Tartheside.mono.world.generators;

namespace Tartheside.mono.ui;

public partial class MFNLEditor : Control
{
	private MarginContainer _domainWarp; 
	private MarginContainer _domainWarpFractal; 
	private MarginContainer _fractal; 
	private MarginContainer _general; 
	private MarginContainer _cellular;
	private OptionButton _sourceSelector;
	
	private MFNL _noise;
	private TMap _tileMap;
	
	
	// TODO: offsetX y offsetY como miembros de clase.
	
	
	// TODO: no actualizar el ruido al cambiar los inputs, sino al hacer click en Update. Leer valores de los inputs
	
	
	public override void _Ready()
	{
		_noise = new MFNL();	// TODO: no guardar noise, sino NoiseGenerator
		
		_domainWarp = GetNode<MarginContainer>("%DomainWarp");
		_domainWarpFractal = GetNode<MarginContainer>("%DomainWarpFractal");
		_fractal = GetNode<MarginContainer>("%Fractal");
		_general = GetNode<MarginContainer>("%General");
		_cellular = GetNode<MarginContainer>("%Cellular");
		_sourceSelector = GetNode<OptionButton>("%SourceSelector");
		
		TileMapWindowSetUp();
		UiSetUp();
		
		_OnGenerateButtonPressed(_noise);
		SignalsSetup();
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
	
	private void UpdateUi()
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
		}
	}
	
	
	// tilemap
	private void TileMapWindowSetUp()
	{
		var tileMapWindow = new Window();
		_tileMap = GD.Load<PackedScene>("res://scenes/WorldTileMap.tscn").Instantiate<TMap>();

		tileMapWindow.Size = new Vector2I(970, 870);
		tileMapWindow.Position = new Vector2I(890, 64);
		
		_tileMap.Name = "NoiseEditorTileMap";
		_tileMap.Position = new Vector2I(8, 8);

		SetWorld();
		
		tileMapWindow.AddChild(_tileMap);		
		AddChild(tileMapWindow);
		_tileMap.Setup(false);
		
	}

	private void SetWorld() => SetWorld(new World());

	private void SetWorld(World world) => _tileMap.SetWorld(world);

	private void UpdateNoise(string prop, Variant value)
	{
		var offsetX = ((Vector2I)_tileMap.GetWorld().GetWorldParameter("Offset")).X;
		var offsetY = ((Vector2I)_tileMap.GetWorld().GetWorldParameter("Offset")).Y;
		
		((NoiseGenerator)_tileMap.GetWorld().GetWorldGenerator("NoiseGenerator")).GetParameterNoise()
			.UpdateNoiseProperty(prop, value);
		((NoiseGenerator)_tileMap.GetWorld().GetWorldGenerator("NoiseGenerator")).ClearValueMatrix();
		((NoiseGenerator)_tileMap.GetWorld().GetWorldGenerator("NoiseGenerator")).FillValueMatrix(offsetX, offsetY);
	}
	
	// SIGNALS
	private void _OnGenerateButtonPressed(MFNL noise)
	{
		var generator = new NoiseGenerator(
			(Vector2I) _tileMap.GetWorld().GetWorldParameter("WorldSize"),
			(Vector2I) _tileMap.GetWorld().GetWorldParameter("ChunkSize"),
			(Vector2I) _tileMap.GetWorld().GetWorldParameter("Offset"),
			(int) _tileMap.GetWorld().GetWorldParameter("NTiers")
		);
		generator.SetParameterNoise(noise);
		generator.FillValueMatrix((int) _tileMap.GetWorld().GetWorldParameter("OffsetX"), 
			(int) _tileMap.GetWorld().GetWorldParameter("OffsetY"));
		_tileMap.GetWorld().AddWorldGenerator("NoiseGenerator", generator);
		_tileMap.RenderChunks("NoiseGenerator", 0);
		UpdateUi();
	}

	private void _OnUpdateButtonPressed()
	{
		_tileMap.Clear();
		_tileMap.RenderChunks("NoiseGenerator", 0);
	}
	
	private void _OnResetButtonPressed()
	{
		_noise = new MFNL();
		UpdateUi();
	}

	private void SignalsSetup()
	{
		// general
		((OptionButton)GetParameterInput("NoiseType")).ItemSelected += _OnNoiseTypeChanged;
		((SpinBox)GetParameterInput("Seed")).ValueChanged += _OnSeedChanged;
		((SpinBox)GetParameterInput("Frequency")).ValueChanged += _OnFrequencyChanged;
		
		// cellular
		((OptionButton)GetParameterInput("CellularDistanceFunction")).ItemSelected 
			+= _OnCellularDistanceFunctionChanged;
		((OptionButton)GetParameterInput("CellularReturnType")).ItemSelected 
			+= _OnCellularReturnTypeChanged;
		((SpinBox)GetParameterInput("CellularJitter")).ValueChanged += _OnCellularJitterChanged;

		// domain warp
		// TODO: falta domain warp enabled (bool)
		((OptionButton)GetParameterInput("DomainWarpType")).ItemSelected 
			+= _OnDomainWarpTypeChanged;
		((SpinBox)GetParameterInput("DomainWarpAmplitude")).ValueChanged 
			+= _OnDomainWarpAmplitudeChanged;
		((SpinBox)GetParameterInput("DomainWarpFrequency")).ValueChanged 
			+= _OnDomainWarpFrequencyChanged;
	
		// domain warp fractal
		((OptionButton)GetParameterInput("DomainWarpFractalType")).ItemSelected 
			+= _OnDomainWarpFractalTypeChanged;
		((SpinBox)GetParameterInput("DomainWarpFractalGain")).ValueChanged 
			+= _OnDomainWarpFractalGainChanged;
		((SpinBox)GetParameterInput("DomainWarpFractalLacunarity")).ValueChanged 
			+= _OnDomainWarpFractalLacunarityChanged;
		((SpinBox)GetParameterInput("DomainWarpFractalOctaves")).ValueChanged 
			+= _OnDomainWarpFractalOctavesChanged;
		
		// fractal
		((OptionButton)GetParameterInput("FractalType")).ItemSelected 
			+= _OnFractalTypeChanged;
		((SpinBox)GetParameterInput("FractalGain")).ValueChanged 
			+= _OnFractalGainChanged;
		((SpinBox)GetParameterInput("FractalLacunarity")).ValueChanged 
			+= _OnFractalLacunarityChanged;
		((SpinBox)GetParameterInput("FractalOctaves")).ValueChanged 
			+= _OnFractalOctavesChanged;
		((SpinBox)GetParameterInput("FractalPingPongStrength")).ValueChanged 
			+= _OnFractalPingPongStrengthChanged;
		((SpinBox)GetParameterInput("FractalWeightedStrength")).ValueChanged 
			+= _OnFractalWeightedStrengthChanged;
       
	}

	// general
	private void _OnNoiseTypeChanged(long idx) => GD.Print("_OnNoiseTypeChanged");
	private void _OnSeedChanged(double idx) => UpdateNoise("Seed", 
		((SpinBox)GetParameterInput("Seed")).Value);
	private void _OnFrequencyChanged(double idx) => UpdateNoise("Frequency", 
		((SpinBox)GetParameterInput("Frequency")).Value);
	
	// cellular
	private void _OnCellularDistanceFunctionChanged(long idx) => GD.Print("_OnCellularDistanceFunctionChanged");
	private void _OnCellularReturnTypeChanged(long idx) => GD.Print("_OnCellularReturnTypeChanged");
	private void _OnCellularJitterChanged(double idx) => UpdateNoise("CellularJitter", 
		((SpinBox)GetParameterInput("CellularJitter")).Value);

	// domain warp
	private void _OnDomainWarpTypeChanged(long idx) => GD.Print("_OnDomainWarpTypeChanged");
	private void _OnDomainWarpAmplitudeChanged(double idx) => UpdateNoise("DomainWarpAmplitude", 
		((SpinBox)GetParameterInput("DomainWarpAmplitude")).Value);
	private void _OnDomainWarpFrequencyChanged(double idx) => UpdateNoise("DomainWarpFrequency", 
		((SpinBox)GetParameterInput("DomainWarpFrequency")).Value);

	// domain warp fractal
	private void _OnDomainWarpFractalTypeChanged(long idx) => GD.Print("_OnDomainWarpFractalTypeChanged");
	private void _OnDomainWarpFractalGainChanged(double idx) => UpdateNoise("FractalGain", 
		((SpinBox)GetParameterInput("FractalGain")).Value);
	private void _OnDomainWarpFractalLacunarityChanged(double idx) => UpdateNoise("DomainWarpFractalLacunarity", 
		((SpinBox)GetParameterInput("DomainWarpFractalLacunarity")).Value);
	private void _OnDomainWarpFractalOctavesChanged(double idx) => UpdateNoise("DomainWarpFractalOctaves", 
		((SpinBox)GetParameterInput("DomainWarpFractalOctaves")).Value);

	
	// fractal
	private void _OnFractalTypeChanged(long idx) => GD.Print("_OnFractalTypeChanged");
	private void _OnFractalGainChanged(double idx) => UpdateNoise("FractalGain", 
		((SpinBox)GetParameterInput("FractalGain")).Value);
	private void _OnFractalLacunarityChanged(double idx) => UpdateNoise("FractalLacunarity", 
		((SpinBox)GetParameterInput("FractalLacunarity")).Value);
	private void _OnFractalOctavesChanged(double idx) => UpdateNoise("FractalOctaves", 
		((SpinBox)GetParameterInput("FractalOctaves")).Value);
	private void _OnFractalPingPongStrengthChanged(double idx) => UpdateNoise("FractalPingPongStrength", 
		((SpinBox)GetParameterInput("FractalPingPongStrength")).Value);
	private void _OnFractalWeightedStrengthChanged(double idx) => UpdateNoise("FractalWeightedStrength", 
		((SpinBox)GetParameterInput("FractalWeightedStrength")).Value);
	
	
	// TODO: necesitamos un método al que le pasemos alguno de los containers principales (_general, _fractal, etc) y 
	// el nombre del parámetro y devuelva el linedit (o lo que sea) correspondiente
	private Node GetParameterInput(string param)
	{
		Node toReturn = null;
		foreach (var vBoxContainer in new List<Node>(){_domainWarp, _domainWarpFractal, _fractal, _general, _cellular}
			         .Select(node => node.GetNode<VBoxContainer>("./MarginContainer/VBoxContainer")))
			foreach (var marginContainer in vBoxContainer.GetChildren())
			{
				if (!marginContainer.Name.Equals(param)) continue;
				toReturn = marginContainer.GetNode("./HBox").GetChild(0);
				break;
			}
		return toReturn;
	}
	
	
}



