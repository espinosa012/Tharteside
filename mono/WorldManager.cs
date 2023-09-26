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
		InitializeTileMap();
	}

	private WorldTileMap GetWorldTileMapFromTscn()
	{
		PackedScene tileMapScene = GD.Load<PackedScene>("res://scenes/WorldTileMap.tscn");
		return tileMapScene.Instantiate<WorldTileMap>();
	}
	
	private void InstantiateTileMap()
	{
		_tileMap = GetWorldTileMapFromTscn();
		_tileMap.Position = new Vector2I(0, 0);
		AddChild(_tileMap);	// crear nodo de control	
	}
	
	private void InitializeTileMap()
	{
		InstantiateTileMap();
		//_tileMap = (WorldTileMap) GetNode<TileMap>("TileMap"); // en el futuro, formar desde código
		_tileMap.SetWorld(_world);
		_tileMap.SetProceduralSource(ProceduralSource);
		_tileMap.TileMapSetup(WorldSize, TileMapOffset, ChunkSize, SquareSize, Chunks);
	}

	
	private void InitializeWorld()
	{
		_world = new World();
		_world.InitElevation();
		//_world.InitTemperature();
		//_world.InitTerrain();
		//_world.InitHeightMap(HeightMap + ".png");
		
		_world.UpdateWorldParameter("WorldSize", WorldSize);
		_world.UpdateWorldParameter("ChunkSize", ChunkSize);
	}
	
	
	
	public void SetWorldSize(Vector2I newSize)
	{
		_tileMap.SetWorldSize(newSize);
	}

	public void SetTileMapOffset(Vector2I newOffset)
	{
		_tileMap.SetTileMapOffset(newOffset);
	}

	public void SetTileMapChunks(Vector2I newSize)
	{
		_tileMap.SetTileMapChunks(newSize);
	}
	
	public void SetChunkSize(Vector2I newSize)
	{
		_tileMap.SetChunkSize(newSize);
	}
	
	public void SetSquareSize(Vector2I newSize)
	{
		_tileMap.SetSquareSize(newSize);
	}

	public void ReloadTileMap()
	{
		_tileMap.ReloadTileMap();
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