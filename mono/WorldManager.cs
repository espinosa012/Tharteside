using System.Collections.Generic;
using Godot;

namespace Tartheside.mono;

public partial class WorldManager : Node2D
{	
	[Export] public string HeightMap;
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

	private void InitializeTileMap()
	{
		_tileMap = (WorldTileMap) GetNode<TileMap>("TileMap"); // en el futuro, formar desde código
		_tileMap.SetWorld(_world);
		_tileMap.TileMapSetup(WorldSize, TileMapOffset, ChunkSize, SquareSize, Chunks);
	}
	
	private void InitializeWorld()
	{
		_world = new World();
		_world.InitElevation();
		//_world.InitTemperature();
		//_world.InitTerrain();
		_world.InitHeightMap();
		
		_world.UpdateWorldParameter("WorldSize", WorldSize);
		_world.UpdateWorldParameter("ChunkSize", ChunkSize);
		((HeightMap) _world.GetWorldGenerator("HeightMap")).LoadHeighMap(HeightMap + ".png");;
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