using System;
using System.Text.RegularExpressions;
using Godot;
using Tartheside.mono.world;
using Tartheside.mono.tilemap;
using Tartheside.mono.utilities.math;
using Tartheside.mono.utilities.random;
using Tartheside.mono.world.generators;
using Tartheside.mono.world.manager;


namespace Tartheside.mono.utilities.command_line;

public partial class TCommandLine : LineEdit
{

	private World _world;	// TODO: podría no ser necesario si tomamos la referencia al mundo del tilemap
	private TMap _tileMap;
	
	public override void _Ready()
	{
		TextSubmitted += _ProcessCommand;
	}

	public void Setup()
	{
		var manager = GetParent<WorldManager>();
		_world = manager.GetWorld();
		_tileMap = manager.GetTilemap();
	}
	
	private void _ProcessCommand(string text)
	{
		var command = GetCommandAndArgs(text);
		if (HasMethod(command.Item1))
			Call(command.Item1, command.Item2);
		else if (HasMethod(command.Item1.Capitalize()))
			Call(command.Item1.Capitalize(), command.Item2);
		else if (command.Item1.Trim().StartsWith("Is") 
		         && ((Elevation)_world.GetWorldGenerator("Elevation")).HasMethod(command.Item1.Trim()))
			Is(command.Item1.Trim());
		Clear();
	}

	private static string TranslateCommandAlias(string command)
	{
		// TODO
		return "";
	}
	
	private static Tuple<string, string[]> GetCommandAndArgs(string text)
	{
		const string pattern = @"^(?<cmd>\w+)(\s+(?<args>.*))?$";
		var match = Regex.Match(text, pattern);
		if (!match.Success)
			return null;
		var args = match.Groups["args"].Value.Split();
		if (args[0] == "")
			args = null;
		return new Tuple<string, string[]>(match.Groups["cmd"].Value.Trim(), args);	
	}
    
	
	/*	COMMANDS	*/
	
	// Generator
	private void Render(string[] args)
	{
		// TODO: mejorar: hacer más flexible, comprobar que existe, etc.
		var generatorName = args[0].StripEdges();
		if (_world.GetWorldGenerators().ContainsKey(generatorName))
		{
			var layer = int.Parse(args[1].StripEdges());
			_tileMap.ClearLayer(layer);
			_tileMap.RenderChunks(generatorName, layer);			
		}
		else
			RenderNoise(args);
	}

	private void Reload(string[] args)
	{
		var generatorName = args[0].StripEdges();
		var layer = int.Parse(args[1].StripEdges());

		_tileMap.ClearLayer(layer);
		_world.GetWorldGenerator(generatorName).ReloadValueMatrix();
		_tileMap.RenderChunks(generatorName, layer);
	}
	
	private void Threshold(string[] args)
	{
		var maskGenerator = new MaskGenerator((Vector2I) _world.GetWorldParameter("WorldSize"), 
			(Vector2I) _world.GetWorldParameter("ChunkSize"), 
			(Vector2I) _world.GetWorldParameter("Offset"), 
			(int) _world.GetWorldParameter("NTiers"));
		var thresholdTierValue = int.Parse(args[1].StripEdges());
		maskGenerator.SetValueMatrix(_world.GetWorldGenerator(args[0].StripEdges()).GetValueMatrix());
		maskGenerator.ThresholdByTier(thresholdTierValue);
		_world.RemoveWorldGenerator("InverseThresholdGenerator");
		_world.AddWorldGenerator("InverseThresholdGenerator", maskGenerator);
		_tileMap.ClearLayer(0);
		_world.GetWorldGenerator("InverseThresholdGenerator").InverseThresholdByTier(thresholdTierValue);
		_tileMap.RenderChunks("InverseThresholdGenerator", 0);
	}
	
	private void InverseThreshold(string[] args)
	{
		var maskGenerator = new MaskGenerator((Vector2I) _world.GetWorldParameter("WorldSize"), 
			(Vector2I) _world.GetWorldParameter("ChunkSize"), 
			(Vector2I) _world.GetWorldParameter("Offset"), 
			(int) _world.GetWorldParameter("NTiers"));
		var thresholdTierValue = int.Parse(args[1].StripEdges());
		maskGenerator.SetValueMatrix(_world.GetWorldGenerator(args[0].StripEdges()).GetValueMatrix());
		maskGenerator.InverseThresholdByTier(thresholdTierValue);
		_world.RemoveWorldGenerator("InverseThresholdGenerator");
		_world.AddWorldGenerator("InverseThresholdGenerator", maskGenerator);
		_tileMap.ClearLayer(0);
		_world.GetWorldGenerator("InverseThresholdGenerator").InverseThresholdByTier(thresholdTierValue);
		_tileMap.RenderChunks("InverseThresholdGenerator", 0);
	}

	private void RangeThresholdByTier(string[] args)
	{
		// TODO: mejorar: hacer más flexible, comprobar que existe, capa por defecto, podemos indicar máximo, etc.
		var maskGenerator = new MaskGenerator((Vector2I) _world.GetWorldParameter("WorldSize"), 
			(Vector2I) _world.GetWorldParameter("ChunkSize"), 
			(Vector2I) _world.GetWorldParameter("Offset"), 
			(int) _world.GetWorldParameter("NTiers"));
		maskGenerator.SetValueMatrix(_world.GetWorldGenerator(args[0].StripEdges()).GetValueMatrix());
		maskGenerator.RangeThresholdByTier(int.Parse(args[1].StripEdges()), int.Parse(args[2].StripEdges()));
		_world.RemoveWorldGenerator("RangeThresholdGenerator");
		_world.AddWorldGenerator("RangeThresholdGenerator", maskGenerator);
		_tileMap.ClearLayer(0);
		_tileMap.RenderChunks("RangeThresholdGenerator", 0);
		
		
		
	}
	
	// Mask
	private void Is(string method)
	{
		var maskGenerator = new MaskGenerator((Vector2I) _world.GetWorldParameter("WorldSize"), 
			(Vector2I) _world.GetWorldParameter("ChunkSize"), 
			(Vector2I) _world.GetWorldParameter("Offset"), 
			(int) _world.GetWorldParameter("NTiers"));
		maskGenerator.SetParentGenerator(_world.GetWorldGenerator("Elevation"));
		maskGenerator.SetMaskMethod(method); 
		
		maskGenerator.FillValueMatrix();
		
		_world.AddWorldGenerator(method, maskGenerator);
		
		_tileMap.ClearLayer(0);
		_world.GetWorldGenerator("Elevation").ThresholdByTier(0);
		_tileMap.RenderChunks(method, 0);
	}
	
	
	// Elevation
	private void RandomizeElevation(string[] _args)
	{
		_world.GetWorldGenerator("Elevation").Randomize(0);	// TODO: usar semilla
		_world.GetWorldGenerator("Elevation").ReloadValueMatrix();
		_tileMap.Clear();
		_tileMap.RenderChunks("Elevation", 0);
	}
	
	// River
	private void GenerateRiver(string[] args)
	{
		var x = int.Parse(args[0].StripEdges());
		var y = int.Parse(args[1].StripEdges());
		((River)_world.GetWorldGenerator("River")).GenerateRiver(new Vector2I(x, y));
		_tileMap.ClearLayer(1);
		_tileMap.RenderChunks("River", 1);
	}

	private void RandomizeRiver(string[] _args)
	{
		((River)_world.GetWorldGenerator("River")).Randomize(0);
		_tileMap.ClearLayer(1);
		_tileMap.RenderChunks("River", 1);
	}
	
	// Noise
	private void RenderNoise(string[] args)
	{
		var noiseName = args[0];
		var noiseGenerator = new NoiseGenerator((Vector2I) _world.GetWorldParameter("WorldSize"), 
			(Vector2I) _world.GetWorldParameter("ChunkSize"), 
			(Vector2I) _world.GetWorldParameter("Offset"), 
			(int) _world.GetWorldParameter("NTiers"));
		var noise = new MFNL();
		noise.LoadFromJson(noiseName);
		noiseGenerator.SetParameterNoiseObject(noise);
		noiseGenerator.FillValueMatrix();
		_world.AddWorldGenerator(noiseName, noiseGenerator);
		_tileMap.Clear();
		_tileMap.RenderChunks(noiseName, 0);
	}

	private void GetIslands(string[] args)
	{
		var regions = _world.GetWorldGenerator(args[0])
			.GetConnectedRegions(int.Parse(args[1].StripEdges()), int.Parse(args[2].StripEdges()), 
				int.Parse(args[3].StripEdges()));
		foreach (var region in regions)
			region.ComputeCentroid();
	}

	private void Sobel(string[] _args)
	{
		var sobelGenerator = new BaseGenerator((Vector2I) _world.GetWorldParameter("WorldSize"), 
                                               			(Vector2I) _world.GetWorldParameter("ChunkSize"), 
                                               			(Vector2I) _world.GetWorldParameter("Offset"), 
                                               			(int) _world.GetWorldParameter("NTiers"));
		sobelGenerator.SetValueMatrix(MathDotNetHelper.GetSobelMatrix(_world.GetWorldGenerator("Elevation").GetValueMatrix()));
		_world.AddWorldGenerator("Sobel", sobelGenerator);
		_tileMap.Clear();
		_tileMap.RenderChunks("Sobel", 0);
	}
	
	// Tilemap
	private void ClearLayer(string[] args) => _tileMap.ClearLayer(int.Parse(args[2].StripEdges()));
	
	
	
	
	
	private void Exit(string[] _args) => GetTree().Quit();
	
}