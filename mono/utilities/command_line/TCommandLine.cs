using Godot;
using System;
using System.Linq;
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
		Call(text.Split(" ")[0].Trim());
		Clear();
	}
	
	
    // COMMANDS
    private void RandomizeRiver()
    {
	    ((River)_world.GetWorldGenerator("River")).Randomize();
	    _tileMap.ClearLayer(1);
	    _tileMap.InitializeChunks();
    }
    
    private void ClearLayer(string layer)
    {
	    _tileMap.ClearLayer(int.Parse(layer.Replace("ClearLayer", "")));
    }
    
    private void PrintNoise(string noise)
    {
	    if (!_world.GetWorldNoises().Keys.Contains(noise))
	    {
		    GD.Print(noise + " not available");
		    return;
	    }
	    GD.Print(_world.GetWorldNoise(noise)); 
    }
	
	
	
	
}
