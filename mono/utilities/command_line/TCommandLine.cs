using System;
using System.Text.RegularExpressions;
using Godot;
using Tartheside.mono.world;
using Tartheside.mono.tilemap;
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
		// TODO: mejorar: hacer más flexible, comprobar que existe, capa por defecto, podemos indicar máximo, etc.
		var generatorName = args[0].StripEdges();
		var thresholdTierValue = int.Parse(args[1].StripEdges());
		var layer = int.Parse(args[2].StripEdges());
		_tileMap.ClearLayer(layer);
		_world.GetWorldGenerator(generatorName).ThresholdValueMatrixByTier(thresholdTierValue);
		_tileMap.RenderChunks(generatorName, layer);
	}
	
	private void InverseThreshold(string[] args)
	{
		// TODO: mejorar: hacer más flexible, comprobar que existe, capa por defecto, podemos indicar máximo, etc.
		var generatorName = args[0].StripEdges();
		var thresholdTierValue = int.Parse(args[1].StripEdges());
		var layer = int.Parse(args[2].StripEdges());
		_tileMap.ClearLayer(layer);
		_world.GetWorldGenerator(generatorName).InverseThresholdingByTier(thresholdTierValue);
		_tileMap.RenderChunks(generatorName, layer);
	}

	
	// Elevation
	private void RandomizeElevation(string[] _args)
	{
		_world.GetWorldGenerator("Elevation").Randomize();
		_world.GetWorldGenerator("Elevation").ReloadValueMatrix();
		_tileMap.Clear();
		_tileMap.RenderChunks("Elevation", 0);
	}
	
	// River
	
	
	
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
	
	
	// Tilemap
	private void ClearLayer(string[] args) => _tileMap.ClearLayer(int.Parse(args[2].StripEdges()));
	
	
	
	
	
	private void Exit(string[] _args) => GetTree().Quit();
	
}