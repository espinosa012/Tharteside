using System;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;
using Godot.Collections;
using Tartheside.mono.world;
using Tartheside.mono.world.generators;

namespace Tartheside.mono.utilities.command_line;

public partial class TCommandLine : LineEdit
{

	private World _world;
	private WorldTileMap _tileMap;
	
	public override void _Ready()
	{
		TextSubmitted += _ProcessCommand;
	}

	public void Init(World w, WorldTileMap tm)
	{
		_world = w;
		_tileMap = tm;
	}

	private void Output(string message)
	{
		GD.Print("console message >> " + message);
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
	
	
	// COMMANDS
	private void Get(string[] args)
	{
		if (_world.GetWorldNoises().ContainsKey(args[0]))
		{
			GD.Print(_world.GetWorldNoise(args[0]).GetParamValueAsString(args[1]));
		}
		else if (_world.GetWorldGenerators().ContainsKey(args[0]))
		{

		}
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
    
	private void RandomizeRiver(string[] args)
	{
		((River)_world.GetWorldGenerator("River")).Randomize();
		_tileMap.RenderChunks("River", 1);
	}
    
	private void SetRiverFreq(string[] args)
	{
		((River)_world.GetWorldGenerator("River")).GetParameterBaseNoise().Frequency = float.Parse(args[0]);
		_tileMap.RenderChunks("River", 1);
	}
    
	private void PrintNoise(string[] args)
	{
		string noise = args[0].Split()[0];
		if (!_world.GetWorldNoises().Keys.Contains(noise))
		{
			GD.Print(noise + " not available");
			return;
		}
		GD.Print(_world.GetWorldNoise(noise)); 
	}

	private void ReloadTileMap(string[] args) => _tileMap.ReloadTileMap();

	private void Exit(string[] args) => GetTree().Quit();
	
	
}