using Godot;
using System;

public partial class TWorldManager : Node2D
{	
	[Export] public Vector2I WorldSize;
	[Export] public Vector2I TileMapOffset = new Vector2I(320, 317);
	[Export] public Vector2I ChunkSize;

	public TWorld TWorld;
	public TileMap TileMap;

	public override void _Ready()
	{

		TWorld = new TWorld(WorldSize.X, WorldSize.Y);
		TileMap = GetNode<TileMap>("TileMap");
		TileMapTest();

		// UI
		BasicWorldPanel panel = GetNode<BasicWorldPanel>("%BasicWorldPanel");
		panel.SetTWorldManager(this);
		panel.InitializeUI();
		panel.ConnectButtonSignal();
	}


	public void TileMapTest()
	{	
		int valueTier;
		Vector2I worldPosition;

		for (int x = 0; x < WorldSize.X * ChunkSize.X; x+=ChunkSize.X)
		{
			for (int y = 0; y < WorldSize.Y * ChunkSize.Y; y+=ChunkSize.Y)
			{
                worldPosition = new Vector2I((x/ChunkSize.X)+TileMapOffset.X, (y/ChunkSize.Y)+TileMapOffset.Y);
				valueTier = TWorld.GetValueTier(TWorld.GetElevation(worldPosition.X, worldPosition.Y));

				// SetCell(new Vector2I(x, y), valueTier);
				FulfillChunk(new Vector2I(x, y), valueTier);
			}
		}
	}	

	public void UpdateTileMap()
	{
		TileMap.Clear();
		TileMapTest();
	}


	public void SetCell(Vector2I tileMapPosition, int valueTier)
	{
		int tileMapLayer = 0;
		int tileSetSourceId;
		Vector2I tileSetAtlasCoordinates;

		if (valueTier == 0)	// sea
		{
			tileSetSourceId = 8;
			tileSetAtlasCoordinates = new Vector2I(0, 0);
		}
		else if (valueTier == 1 || valueTier == 2)
		{
			tileSetSourceId = 8;
			tileSetAtlasCoordinates = new Vector2I(5, 0);
		}
		else if (valueTier == 3)
		{
			tileSetSourceId = 8;
			tileSetAtlasCoordinates = new Vector2I(2, 0);
		}
		else if (valueTier == 4 || valueTier == 5 || valueTier == 6)
		{
			tileSetSourceId = 0;
			tileSetAtlasCoordinates = new Vector2I(2, 0);
		}
		else if (valueTier == 7 || valueTier == 8 || valueTier == 9)
		{
			tileSetSourceId = 8;
			tileSetAtlasCoordinates = new Vector2I(3, 0);
		}
		else
		{
			tileSetSourceId = 7;
			tileSetAtlasCoordinates = new Vector2I(valueTier, 0);
		}

		TileMap.SetCell(tileMapLayer, new Vector2I(tileMapPosition.X, tileMapPosition.Y), tileSetSourceId, tileSetAtlasCoordinates);
	}
	public void FulfillChunk(Vector2I chunkPosition, int valueTier)
	{		
		for (int i = 0; i < ChunkSize.X; i++)
		{
			for (int j = 0; j < ChunkSize.Y; j++)
			{
				SetCell(new Vector2I(chunkPosition.X+i, chunkPosition.Y+j), valueTier);
			}
		}
	}
}
