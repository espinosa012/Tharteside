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

	private Button _loadNoiseButton;
	private Button _saveNoiseButton;
	private LineEdit _noiseFilenameLineEdit;
	private FileDialog _fileDialog;
	
	private World _world;
	private NoiseGenerator _noiseGenerator;
	private TMap _tileMap;
	
	public override void _Ready()
	{
		_world = new World();
		NoiseGeneratorSetup();
		_world.AddWorldGenerator("NoiseGenerator", _noiseGenerator);
		TileMapSetup();
		TileMapWindowSetUp();
		_tileMap.RenderChunks("NoiseGenerator", 0);
		UiSetUp();
		UpdateUiToNoise();
	}

	
	
	// SETUP
	private void TileMapSetup()
	{
		_tileMap = GD.Load<PackedScene>("res://scenes/WorldTileMap.tscn").Instantiate<TMap>();
		_tileMap.Name = "NoiseEditorTileMap";
		_tileMap.SetWorld(_world);
		_tileMap.Setup();
	}

	private void NoiseGeneratorSetup()
	{
		_noiseGenerator = new NoiseGenerator(
			(Vector2I) _world.GetWorldParameter("WorldSize"),
			(Vector2I) _world.GetWorldParameter("ChunkSize"),
			(Vector2I) _world.GetWorldParameter("Offset"),
			(int) _world.GetWorldParameter("NTiers")
		);
		var noise = new MFNL();
		noise.RandomizeSeed();
		_noiseGenerator.SetParameterNoiseObject(noise);
		_noiseGenerator.FillValueMatrix();
	}
	
	private void TileMapWindowSetUp()
	{
		var tileMapWindow = new Window();
		tileMapWindow.Size = new Vector2I(970, 870);
		tileMapWindow.Position = new Vector2I(890, 64);
		
		tileMapWindow.AddChild(_tileMap);		
		AddChild(tileMapWindow);
	}
	
	private void UiSetUp()
	{
		_loadNoiseButton = GetNode<Button>("%LoadNoiseButton");
		_saveNoiseButton = GetNode<Button>("%SaveNoiseButton");
		_noiseFilenameLineEdit = GetNode<LineEdit>("%NoiseFilenameLineEdit");
		_fileDialog = GetNode<FileDialog>("%FileDialog");
		FileDialogSetup();
		
		_domainWarp = GetNode<MarginContainer>("%DomainWarp");
		_domainWarpFractal = GetNode<MarginContainer>("%DomainWarpFractal");
		_fractal = GetNode<MarginContainer>("%Fractal");
		_general = GetNode<MarginContainer>("%General");
		_cellular = GetNode<MarginContainer>("%Cellular");
		_sourceSelector = GetNode<OptionButton>("%SourceSelector");
		
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
		// TODO
		/*foreach (var generator in _tileMap.GetWorld().GetWorldGenerators().Keys)
			_sourceSelector.AddItem(generator);	// no hay generadores, se inicializan en el manager*/
	}

	private void FileDialogSetup()
	{
		_fileDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
		_fileDialog.ClearFilters();
		_fileDialog.AddFilter("*.json", "JSON files");
		_fileDialog.CurrentDir = "res://resources/noise";
	}
	
	
	
	// UPDATE
	private void UpdateUiToNoise()
	{
		var noise = _noiseGenerator.GetParameterNoiseObject();
		foreach (var prop in noise.GetNoiseProperties())
			SetParameterInput(prop, noise.GetNoiseProperty(prop));
	}
	
	private void UpdateNoiseProperty(string prop, Variant value)
	{
		((NoiseGenerator)_tileMap.GetWorld().GetWorldGenerator("NoiseGenerator")).GetParameterNoiseObject()
			.UpdateNoiseProperty(prop, value);
		((NoiseGenerator)_tileMap.GetWorld().GetWorldGenerator("NoiseGenerator")).ReloadValueMatrix();
	}

	private void UpdateNoiseFromUi()
	{
		// TODO: optimizar, limpiar.
		foreach (var noiseProp in _noiseGenerator.GetParameterNoiseObject().GetNoiseProperties())
			_noiseGenerator.GetParameterNoiseObject().UpdateNoiseProperty(noiseProp, GetParameterInputValue(noiseProp));
	}
	
	
	
	
	// SIGNALS
	private void _OnResetButtonPressed()
	{	
		_noiseGenerator.SetParameterNoiseObject(new MFNL());
		_noiseGenerator.ReloadValueMatrix();
		_tileMap.Clear();
		_tileMap.RenderChunks("NoiseGenerator", 0);
		UpdateUiToNoise();
	}

	private void _OnUpdateButtonPressed()
	{
		UpdateNoiseFromUi();
		_noiseGenerator.ReloadValueMatrix();
		_tileMap.Clear();
		_tileMap.RenderChunks("NoiseGenerator", 0);
	}

	private void _OnRandomizeSeedButtonPressed()
	{
		_noiseGenerator.GetParameterNoiseObject().RandomizeSeed();
		((NoiseGenerator)_tileMap.GetWorld().GetWorldGenerator("NoiseGenerator")).ReloadValueMatrix();
		
		_tileMap.Clear();
		_tileMap.RenderChunks("NoiseGenerator", 0);
		((SpinBox)GetParameterInput("Seed")).Value = _noiseGenerator.GetParameterNoiseObject().Seed;
		
	}

	private void _OnLoadNoiseButtonPressed()
	{
		_fileDialog.Popup();
	}
	
	private void _OnSaveNoiseButtonPressed()
	{
		var filename = _noiseFilenameLineEdit.Text.StripEdges();
		if (filename == "")
			filename = (string)GetParameterInputValue("Seed");
		_noiseGenerator.GetParameterNoiseObject().SaveToJson(filename);
	}

	private void _OnFileDialogFileSelected(string filename)
	{
		_noiseGenerator.GetParameterNoiseObject().LoadFromJson(filename); 
		_noiseGenerator.ReloadValueMatrix();
		UpdateUiToNoise();
		_tileMap.Clear();
		_tileMap.RenderChunks("NoiseGenerator", 0);
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

	private Variant GetParameterInputValue(string param)
	{
		var node = GetParameterInput(param);
		Variant toReturn = default;
		if (node.IsClass("SpinBox"))
			toReturn = ((SpinBox)node).Value;
		else if (node.IsClass("OptionButton"))
			toReturn = ((OptionButton)node).Selected;
		else if (node.IsClass("CheckBox"))
			toReturn = ((CheckBox)node).ButtonPressed;
		return toReturn;
	}
	
	
}



