using System.Collections.Generic;
using Godot;
using Tartheside.mono.utilities.command_line;
using Tartheside.mono.world;

namespace Tartheside.mono;

public partial class WorldManager : Node2D
{	
	//[Export] public string HeightMap;
	[Export] private Vector2I _worldSize;
	[Export] private Vector2I _tileMapOffset;
	[Export] private Vector2I _chunkSize;
	[Export] private Vector2I _squareSize;
	[Export] private Vector2I _chunks;	

	private World _world;
	private WorldTileMap _tileMap;
	private TCommandLine _commandLine;
	
	public override void _Ready()
	{
		InitializeWorld();
		InitializeTileMap();	
		InitializeCommandLine();
	}

	private void InitializeCommandLine()
	{
		_commandLine = (TCommandLine) GetNode<LineEdit>("cmd");
		_commandLine.Init(_world, _tileMap);
	}
	
	private WorldTileMap GetWorldTileMapFromTscn() =>
		GD.Load<PackedScene>("res://scenes/WorldTileMap.tscn").Instantiate<WorldTileMap>();
	
	private void InstantiateTileMap(string name = "Tilemap")
	{
		Window tileMapWindow = new Window();
		tileMapWindow.Size = new Vector2I(1480, 1480);
		tileMapWindow.Position = new Vector2I(64, 84);
		_tileMap = GetWorldTileMapFromTscn();
		_tileMap.Name = name;
		_tileMap.Position = new Vector2I(0, 0);
		tileMapWindow.AddChild(_tileMap);	// crear nodo de control	
		AddChild(tileMapWindow);
	}
	
	private void InitializeTileMap()
	{
		InstantiateTileMap();
		_tileMap.SetWorld(_world);
		_tileMap.TileMapSetup(_worldSize, _tileMapOffset, _chunkSize, _squareSize, _chunks);
	}
	
	private void InitializeWorld()
	{
		_world = new World();
		_world.UpdateWorldParameter("WorldSize", _worldSize);
		_world.UpdateWorldParameter("ChunkSize", _chunkSize);
		
		_world.InitElevation();
		_world.InitRiver();
		//_world.InitHumidity();
		//_world.InitTemperature();
		//_world.InitTerrain();
		//_world.InitHeightMap(HeightMap + ".png");
	}


}