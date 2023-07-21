using Godot;

namespace Tartheside.mono;

public partial class WorldManager : Node2D
{	
	[Export] public Vector2I WorldSize;
	[Export] public Vector2I TileMapOffset;
	[Export] public Vector2I ChunkSize;
	[Export] public Vector2I SquareSize;
	[Export] public bool DisplayBorders;
	[Export] public Vector2I Chunks;	// Chunks que se inicializarán al principio

	private World _world;
	private WorldTileMap _tileMap;

	public override void _Ready()
	{
		_world = new World();
		_tileMap = (WorldTileMap) GetNode<TileMap>("TileMap"); // en el futuro, formar desde código
		_tileMap.SetWorld(_world);
		_tileMap.TileMapSetup(WorldSize, TileMapOffset, ChunkSize, SquareSize, Chunks, DisplayBorders);

		// UI
		var panel = GetNode<BasicWorldPanel>("%BasicWorldPanel");
		panel.SetTWorldManager(this);
		panel.InitializeUi();
		panel.ConnectButtonSignal();
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

	public void ReloadTileMap(string property = "")
	{
		_tileMap.ReloadTileMap(property, DisplayBorders);
	}

}