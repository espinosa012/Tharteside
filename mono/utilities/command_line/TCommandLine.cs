using System;
using Godot;
using System.Text.RegularExpressions;
using System.Linq;
using Godot.Collections;
using Tartheside.mono;
using Tartheside.mono.world.generators;


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
		GD.Print(_world);
	}
	
	
	private void _ProcessCommand(string text)
	{
		// usar expresiones regulares para formar comandos y atributos (opcinales, indicando el nombre del parámetro, etc)
		//PrintNoise(text.Trim());
		//ClearLayer(text);	
		// implementar comando exit y tomar el foco con tab o Fx

		Tuple<string, string[]> command = GetCommandAndArgs(text);
		Call(command.Item1);
		Clear();
	}

	private Tuple<string, string[]> GetCommandAndArgs(string text)
	{
		string pattern = @"^(?<cmd>\w+)(\s+(?<args>.*))?$";
		Match match = Regex.Match(text, pattern);
		if (!match.Success)
			return null;
		return new Tuple<string, string[]>(match.Groups["cmd"].Value.Trim(), match.Groups["args"].Value.Split());	
	}
	
    // COMMANDS
    private void RandomizeRiver()
    {
	    ((River)_world.GetWorldGenerator("River")).Randomize();
	    _tileMap.RenderChunks("River", 1);
    }
    
    private void PrintNoise(string args)
    {
	    GD.Print(args);
	    string noise = args.Split()[0];
	    if (!_world.GetWorldNoises().Keys.Contains(noise))
	    {
		    GD.Print(noise + " not available");
		    return;
	    }
	    GD.Print(_world.GetWorldNoise(noise)); 
    }
	
	
	
	
}
