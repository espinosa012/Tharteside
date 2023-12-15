using System;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;
using Tartheside.mono.world;
using Tartheside.mono.tilemap;
using Tartheside.mono.utilities.logger;
using Tartheside.mono.world.manager;


namespace Tartheside.mono.utilities.command_line;

public partial class TCommandLine : LineEdit
{

	private World _world;
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
		string[] args;
		string pattern = @"^(?<cmd>\w+)(\s+(?<args>.*))?$";
		Match match = Regex.Match(text, pattern);
		if (!match.Success)
			return null;
		args = match.Groups["args"].Value.Split();
		if (args[0] == "")
			args = null;
		return new Tuple<string, string[]>(match.Groups["cmd"].Value.Trim(), args);	
	}
    
	
	
	
	// Commands
	private void RenderGenerator(string[] args)
	{
		// TODO: mejorar: hacer más flexible, comprobar que existe, etc.
		var generatorName = args[0].StripEdges();
		var layer = int.Parse(args[1].StripEdges());
		_tileMap.ClearLayer(layer);
		_tileMap.RenderChunks(generatorName, layer);
	}

	private void UmbralizeGenerator(string[] args)
	{
		var generatorName = args[0].StripEdges();
		var layer = int.Parse(args[1].StripEdges());
		
		
	}
	
	private void Exit(string[] _args) => GetTree().Quit();
	
	
}