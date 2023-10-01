using System.Collections.Generic;
using Godot;

namespace Tartheside.mono;

public partial class WorldManager : Node2D
{	
	[Export] public string HeightMap;
	[Export] public string ProceduralSource;
	[Export] public Vector2I WorldSize;
	[Export] public Vector2I TileMapOffset;
	[Export] public Vector2I ChunkSize;
	[Export] public Vector2I SquareSize;
	[Export] public Vector2I Chunks;	

	private World _world;
	private WorldTileMap _tileMap;
	private TCommandLine _commandLine;
	
	public override void _Ready()
	{
		InitializeWorld();
		InitializeTileMap();	// TODO: cambiar el enfoque. No habrá un tilemap, sino una lista
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
		_tileMap.TileMapSetup(WorldSize, TileMapOffset, ChunkSize, SquareSize, Chunks);
	}

	
	private void InitializeWorld()
	{
		_world = new World();
		_world.UpdateWorldParameter("WorldSize", WorldSize);
		_world.UpdateWorldParameter("ChunkSize", ChunkSize);
		
		_world.InitElevation();
		_world.InitRiver();
		//_world.InitTemperature();
		//_world.InitTerrain();
		//_world.InitHeightMap(HeightMap + ".png");
	}
	
	
	// info
	public Dictionary<string, Variant> GetWorldParameters()
	{
		return _world.GetWorldParameters();
	}

	public Dictionary<string, MFNL> GetWorldNoises()
	{
        return _world.GetWorldNoises();
    }


}