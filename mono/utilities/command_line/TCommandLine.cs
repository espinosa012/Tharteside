using System;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;
using Tartheside.mono.world;
using Tartheside.mono.tilemap;
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

	private void Setup()
	{
		var manager = GetParent<WorldManager>();
		_world = manager.GetWorld();
		_tileMap = manager.GetTilemap();
	}
	
	public void Init(World w, TMap tm)
	{
		_world = w;
		_tileMap = tm;
	}
	
	private void _ProcessCommand(string text)
	{
		// TODO: usar expresiones regulares para formar comandos y atributos (opcinales, indicando el nombre del parámetro, etc)
		// implementar comando exit y tomar el foco con tab o Fx
		Tuple<string, string[]> command = GetCommandAndArgs(text);
		if (HasMethod(command.Item1))
			Call(command.Item1, command.Item2);
		else if (HasMethod(command.Item1.Capitalize()))
			Call(command.Item1.Capitalize(), command.Item2);
		Clear();
	}

	private Tuple<string, string[]> GetCommandAndArgs(string text)
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
    
	private void Set(string[] args)
	{
		if (_world.GetWorldNoises().ContainsKey(args[0]))
		{
			if (args[2].Contains("."))	// float
				_world.GetWorldNoise(args[0]).UpdateNoiseProperty(args[1], float.Parse(args[2]));
			else	// int
				_world.GetWorldNoise(args[0]).UpdateNoiseProperty(args[1], int.Parse(args[2]));
		}
		else if (_world.GetWorldGenerators().ContainsKey(args[0]))	//untested
		{
			_world.GetWorldGenerator(args[0]).Call("SetParameter" + args[1], float.Parse(args[2]));	// puede fallar en caso de que espere otro tipo distinto a float
		}
		else if(_world.GetWorldParameters().ContainsKey(args[0]))
		{
			_world.UpdateWorldParameter(args[0], float.Parse(args[1]));
		}
		_tileMap.ReloadTileMap();
	}
	
	private void Render(string[] args)
	{
		_tileMap.Clear();
		_tileMap.RenderChunks(args[0].Trim(), 0);
	}
	
	

	private void ReloadTileMap(string[] args) => _tileMap.ReloadTileMap();

	private void Exit(string[] args) => GetTree().Quit();
	
	
}