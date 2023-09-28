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

	public override void _Ready()
	{
		InitializeWorld();
		InitializeTileMap();	// TODO: cambiar el enfoque. No habrá un tilemap, sino una lista
	}

	private WorldTileMap GetWorldTileMapFromTscn() =>
		GD.Load<PackedScene>("res://scenes/WorldTileMap.tscn").Instantiate<WorldTileMap>();
	
	private void InstantiateTileMap(string name = "Tilemap")
	{
		_tileMap = GetWorldTileMapFromTscn();
		_tileMap.Name = name;
		_tileMap.Position = new Vector2I(0, 0);
		AddChild(_tileMap);	// crear nodo de control	
	}
	
	private void InitializeTileMap()
	{
		InstantiateTileMap();
		_tileMap.SetWorld(_world);
		_tileMap.SetProceduralSource(ProceduralSource);
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