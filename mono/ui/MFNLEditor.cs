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
				
		TileMapWindowSetUp();
		UiSetUp();

		_OnGenerateButtonPressed();
		SignalsSetup();
	}

	private void UiSetUp()
	{
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

	private void UpdateNoiseProperty(string prop, Variant value)
	{
		var offsetX = ((Vector2I)_tileMap.GetWorld().GetWorldParameter("Offset")).X;
		var offsetY = ((Vector2I)_tileMap.GetWorld().GetWorldParameter("Offset")).Y;
		
		((NoiseGenerator)_tileMap.GetWorld().GetWorldGenerator("NoiseGenerator")).GetParameterNoise()
			.UpdateNoiseProperty(prop, value);
		((NoiseGenerator)_tileMap.GetWorld().GetWorldGenerator("NoiseGenerator")).ClearValueMatrix();
		((NoiseGenerator)_tileMap.GetWorld().GetWorldGenerator("NoiseGenerator")).FillValueMatrix(offsetX, offsetY);
	}

	private void UpdateNoiseFromUi()
	{
		// TODO: otra opción es coger todos los nodos "manualmente" (20 en total)
		var seedInput = _general.GetNode<SpinBox>("./MarginContainer/VBoxContainer/Seed/HBox/SeedInput");  
		var noiseTypeInput = _general.GetNode<OptionButton>("./MarginContainer/VBoxContainer/NoiseType/HBox/NoiseTypeInput");  
		var frequencyInput = _general.GetNode<SpinBox>("./MarginContainer/VBoxContainer/Frequency/HBox/FrequencyInput");		// TODO: que frequency sea LineEdit
		
		var cellularDistanceFunctionInput = 
			_cellular.GetNode<OptionButton>("./MarginContainer/VBoxContainer/CellularDistanceFunction/HBox/CellularDistanceFunctionInput");
		var cellularReturnTypeInput = 
			_cellular.GetNode<OptionButton>("./MarginContainer/VBoxContainer/CellularReturnType/HBox/CellularReturnTypeInput");
		var cellularJitterInput = 
			_cellular.GetNode<SpinBox>("./MarginContainer/VBoxContainer/CellularJitter/HBox/CellularJitterInput");  

		var domainWarpEnabledInput = 
			_domainWarp.GetNode<CheckBox>("./MarginContainer/VBoxContainer/DomainWarpEnabled/HBox/DomainWarpEnabledInput");  
		var domainWarpTypeInput = 
			_domainWarp.GetNode<OptionButton>("./MarginContainer/VBoxContainer/DomainWarpType/HBox/DomainWarpTypeInput");
		var domainWarpAmplitudeInput = 
			_domainWarp.GetNode<SpinBox>("./MarginContainer/VBoxContainer/DomainWarpAmplitude/HBox/DomainWarpAmplitudeInput");
		var domainWarpFrequencyInput = 
			_domainWarp.GetNode<SpinBox>("./MarginContainer/VBoxContainer/DomainWarpFrequency/HBox/DomainWarpFrequencyInput");

		var domainWarpFractalTypeInput = 
			_domainWarpFractal.GetNode<OptionButton>
				("./MarginContainer/VBoxContainer/DomainWarpFractalType/HBox/DomainWarpFractalTypeInput");
		var domainWarpFractalGainInput = 
			_domainWarpFractal.GetNode<SpinBox>
				("./MarginContainer/VBoxContainer/DomainWarpFractalGain/HBox/DomainWarpFractalGainInput");
		var domainWarpFractalLacunarityInput = 
			_domainWarpFractal.GetNode<SpinBox>
				("./MarginContainer/VBoxContainer/DomainWarpFractalLacunarity/HBox/DomainWarpFractalLacunarityInput");
		var domainWarpFractalOctavesInput = 
			_domainWarpFractal.GetNode<SpinBox>
				("./MarginContainer/VBoxContainer/DomainWarpFractalOctaves/HBox/DomainWarpFractalOctavesInput");
		
		var fractalTypeInput = 
			_fractal.GetNode<OptionButton>
				("./MarginContainer/VBoxContainer/FractalType/HBox/FractalTypeInput");
		var fractalGainInput = 
			_fractal.GetNode<SpinBox>
				("./MarginContainer/VBoxContainer/FractalGain/HBox/FractalGainInput");
		var fractalLacunarityInput = 
			_fractal.GetNode<SpinBox>
				("./MarginContainer/VBoxContainer/FractalLacunarity/HBox/FractalLacunarityInput");
		var fractalOctavesInput = 
			_fractal.GetNode<SpinBox>
				("./MarginContainer/VBoxContainer/FractalOctaves/HBox/FractalOctavesInput");
		var fractalPingPongStrengthInput = 
			_fractal.GetNode<SpinBox>
				("./MarginContainer/VBoxContainer/FractalPingPongStrength/HBox/FractalPingPongStrengthInput");
		var fractalWeightedStrengthInput = 
			_fractal.GetNode<SpinBox>
				("./MarginContainer/VBoxContainer/FractalWeightedStrength/HBox/FractalWeightedStrengthInput");
		
		
		
		
		// actualizar las propiedades del ruido en función de los valores de los inputs
		/*foreach (var vBoxContainer in new List<Node>(){_domainWarp, _domainWarpFractal, _fractal, _general, _cellular}
			         .Select(node => node.GetNode<VBoxContainer>("./MarginContainer/VBoxContainer")))
		foreach (var marginContainer in vBoxContainer.GetChildren())
		{
			var valueNode = marginContainer.GetNode("./HBox").GetChild(0);
			Variant value = default;
			if (valueNode.IsClass("OptionButton"))
				value = ((OptionButton)valueNode).Selected;
			else if (valueNode.IsClass("SpinBox"))
				value = ((SpinBox)valueNode).Value;
			else if (valueNode.IsClass("CheckBox"))
				value = ((CheckBox)valueNode).ButtonPressed;
			
			// TODO: obtener nombre de la propiedad en función del nodo
			GD.Print(valueNode.Name.ToString().Replace("Input", ""));
			//UpdateNoiseProperty(valueNode.Name.ToString().Replace("Input", ""), value);
		}*/
		
		
	}
	
	// SIGNALS
	private void _OnGenerateButtonPressed()
	{	
		var generator = new NoiseGenerator(
			(Vector2I) _tileMap.GetWorld().GetWorldParameter("WorldSize"),
			(Vector2I) _tileMap.GetWorld().GetWorldParameter("ChunkSize"),
			(Vector2I) _tileMap.GetWorld().GetWorldParameter("Offset"),
			(int) _tileMap.GetWorld().GetWorldParameter("NTiers")
		);
		generator.SetParameterNoise(_noise);
		generator.FillValueMatrix((int) _tileMap.GetWorld().GetWorldParameter("OffsetX"), 
			(int) _tileMap.GetWorld().GetWorldParameter("OffsetY"));
		_tileMap.GetWorld().AddWorldGenerator("NoiseGenerator", generator);
		_tileMap.RenderChunks("NoiseGenerator", 0);
		UpdateUi();
	}

	private void _OnUpdateButtonPressed()
	{
		UpdateNoiseFromUi();
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
		((CheckBox)GetParameterInput("DomainWarpEnabled")).Toggled 
			+= _OnDomainWarpEnabledChanged;
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
	private void _OnSeedChanged(double seed) => UpdateNoiseProperty("Seed", seed);
	private void _OnFrequencyChanged(double freq) => UpdateNoiseProperty("Frequency", freq);
	
	// cellular
	private void _OnCellularDistanceFunctionChanged(long idx) => GD.Print("_OnCellularDistanceFunctionChanged");
	private void _OnCellularReturnTypeChanged(long idx) => GD.Print("_OnCellularReturnTypeChanged");
	private void _OnCellularJitterChanged(double cellJitter) => UpdateNoiseProperty("CellularJitter", cellJitter);

	// domain warp
	private void _OnDomainWarpEnabledChanged(bool state) => UpdateNoiseProperty("DomainWarpEnabled", state);
	private void _OnDomainWarpTypeChanged(long idx) => GD.Print("_OnDomainWarpTypeChanged");
	private void _OnDomainWarpAmplitudeChanged(double domainWarpAmplitude) => 
		UpdateNoiseProperty("DomainWarpAmplitude", domainWarpAmplitude);
	private void _OnDomainWarpFrequencyChanged(double domainWarpFrequency) =>
		UpdateNoiseProperty("DomainWarpFrequency", domainWarpFrequency); 

	// domain warp fractal
	private void _OnDomainWarpFractalTypeChanged(long idx) => GD.Print("_OnDomainWarpFractalTypeChanged");
	private void _OnDomainWarpFractalGainChanged(double domainWarpFractalGain) => 
		UpdateNoiseProperty("FractalGain", domainWarpFractalGain);
	private void _OnDomainWarpFractalLacunarityChanged(double domainWarpFractalLacunarity) =>
		UpdateNoiseProperty("DomainWarpFractalLacunarity", domainWarpFractalLacunarity);
	private void _OnDomainWarpFractalOctavesChanged(double domainWarpFractalOctaves) =>
		UpdateNoiseProperty("DomainWarpFractalOctaves", domainWarpFractalOctaves); 
	
	// fractal
	private void _OnFractalTypeChanged(long idx) => GD.Print("_OnFractalTypeChanged");
	private void _OnFractalGainChanged(double fractalGain) => UpdateNoiseProperty("FractalGain", fractalGain);
	private void _OnFractalLacunarityChanged(double fractalLacunarity) =>
		UpdateNoiseProperty("FractalLacunarity", fractalLacunarity); 
	private void _OnFractalOctavesChanged(double idx) => UpdateNoiseProperty("FractalOctaves", 
		((SpinBox)GetParameterInput("FractalOctaves")).Value);
	private void _OnFractalPingPongStrengthChanged(double fractalPingPongStrength) =>
		UpdateNoiseProperty("FractalPingPongStrength", fractalPingPongStrength);
	private void _OnFractalWeightedStrengthChanged(double fractalWeightedStrength) =>
		UpdateNoiseProperty("FractalWeightedStrength", fractalWeightedStrength); 
	
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



