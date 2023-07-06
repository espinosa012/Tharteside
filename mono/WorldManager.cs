using Godot;

namespace Tartheside.mono;

public partial class WorldManager : Node2D
{	
	[Export] public Vector2I WorldSize;
	[Export] public Vector2I TileMapOffset;
	[Export] public Vector2I ChunkSize;
	[Export] public Vector2I SquareSize;
	[Export] public Vector2I InitChunks;	// Chunks que se inicializarán al principio

	private World _world;
	private TileMap _tileMap;

	public override void _Ready()
	{
		_world = new World();
		_tileMap = GetNode<TileMap>("TileMap");
		TileMapSetup();
		
		// formar tilemap y tileset desde código

		// UI
		var panel = GetNode<BasicWorldPanel>("%BasicWorldPanel");
		panel.SetTWorldManager(this);
		panel.InitializeUI();
		panel.ConnectButtonSignal();
	}


	private void TileMapSetup()
	{	
		for (int i = 0; i < InitChunks.X; i++)
		{
			for (int j = 0; j < InitChunks.Y; j++)
			{
				RenderChunk(new Vector2I(i, j));		
			}
		}
	}

	private void RenderChunk(Vector2I chunkPosition)
	{
		for (var x = chunkPosition.X * ChunkSize.X * SquareSize.X; x < ChunkSize.X * SquareSize.X + chunkPosition.X * ChunkSize.X * SquareSize.X; x+=SquareSize.X)
		{
			for (var y = chunkPosition.Y * ChunkSize.Y * SquareSize.Y; y < ChunkSize.Y * SquareSize.Y + chunkPosition.Y * ChunkSize.Y * SquareSize.Y; y+=SquareSize.Y)
			{
				var squarePos = new Vector2I(x, y);
				var valueTier = GetValueTierByWorldPos(GetWorldPosBySquare(squarePos));
				FulfillSquare(new Vector2I(x, y), valueTier);	// escalado del mapa
			}
		}
	}

	private Vector2I GetWorldPosBySquare(Vector2I squarePos)
	{
		return new Vector2I((squarePos.X/SquareSize.X)+TileMapOffset.X, (squarePos.Y/SquareSize.Y)+TileMapOffset.Y);
	}

	private int GetValueTierByWorldPos(Vector2I worldPosition)
	{
		return _world.GetValueTierAt(worldPosition.X, worldPosition.Y);
	}
	
	public void UpdateTileMap()
	{
		_tileMap.Clear();
		TileMapSetup();
	}

	private void FulfillSquare(Vector2I worldPos, int valueTier)
	{
		var isStepDownNorth = IsStepDownNorth(worldPos, valueTier);
		var isStepDownSouth = IsStepDownSouth(worldPos, valueTier);
		var isStepDownWest = IsStepDownWest(worldPos, valueTier);
		var isStepDownEast = IsStepDownEast(worldPos, valueTier);
		
		var isStepDownNorthWest = IsStepDownNorthWest(worldPos, valueTier);
		var isStepDownNorthEast = IsStepDownNorthEast(worldPos, valueTier);
		var isStepDownSouthWest = IsStepDownSouthWest(worldPos, valueTier);
		var isStepDownSouthEast = IsStepDownSouthEast(worldPos, valueTier);

		var isSideBorder = IsSideBorder(worldPos, valueTier);
		
		for (var i = 0; i < SquareSize.X; i++)
		{
			for (var j = 0; j < SquareSize.Y; j++)
			{
				var tileSetSource = 5;
				var tileSetAtlasCoordinates = new Vector2I(1, 1);

				if (isStepDownNorth && j == 0)
				{
					tileSetAtlasCoordinates = new Vector2I(1, 3);
					tileSetSource = 5;
				}

				if (isStepDownSouth && j == SquareSize.Y - 1)
				{
					tileSetAtlasCoordinates = new Vector2I(1, 5);
					tileSetSource = 5;
				}

				if (isStepDownWest && i == 0)
				{
					tileSetAtlasCoordinates = new Vector2I(0, 4);
					tileSetSource = 5;
					if (IsStepDownNorth(worldPos, valueTier) && j == 0)
					{
						tileSetAtlasCoordinates = new Vector2I(0, 3);
					}
					if (IsStepDownSouth(worldPos, valueTier) && j == SquareSize.Y - 1)
					{
						tileSetAtlasCoordinates = new Vector2I(0, 5);
					}
				}

				if (isStepDownEast && i == SquareSize.X - 1)
				{
					tileSetAtlasCoordinates = new Vector2I(2, 4);
					tileSetSource = 5;
					if (IsStepDownNorth(worldPos, valueTier) && j == 0)
					{
						tileSetAtlasCoordinates = new Vector2I(2, 3);
					}
					if (IsStepDownSouth(worldPos, valueTier) && j == SquareSize.Y - 1)
					{
						tileSetAtlasCoordinates = new Vector2I(2, 5);
					}
				}

				if (!isSideBorder)
				{
					if (isStepDownNorthWest && i == 0 && j == 0)
					{
						tileSetAtlasCoordinates = new Vector2I(3, 2);
						tileSetSource = 5;
					}
				
					if (isStepDownSouthWest && i == 0 && j == SquareSize.Y - 1)
					{
						tileSetAtlasCoordinates = new Vector2I(3, 3);
						tileSetSource = 5;
					}
				
					if (isStepDownNorthEast && i == SquareSize.X - 1 && j == 0)
					{
						tileSetAtlasCoordinates = new Vector2I(4, 2);
						tileSetSource = 5;
					}

					if (isStepDownSouthEast && i == SquareSize.X - 1 && j == SquareSize.Y - 1)
					{
						tileSetAtlasCoordinates = new Vector2I(4, 3);
						tileSetSource = 5;
					}
				}
				
				
				SetCell(new Vector2I(worldPos.X+i, worldPos.Y+j), tileSetAtlasCoordinates, tileSetSource);	
			}
		}
	}

	private void SetCell(Vector2I tileMapPosition, Vector2I tileSetAtlasCoordinates, int tileSetSourceId = 9)
	{
		var tileMapLayer = 0;
		_tileMap.SetCell(tileMapLayer, new Vector2I(tileMapPosition.X, tileMapPosition.Y), tileSetSourceId, 
			tileSetAtlasCoordinates);
	}
	
	// FRONTIER
	private bool IsSideBorder(Vector2I squarePos, int valueTier)
	{
		return valueTier > _world.GetValueTierAt(squarePos.X/SquareSize.X, squarePos.Y/SquareSize.Y - 1) ||
		       valueTier > _world.GetValueTierAt(squarePos.X/SquareSize.X, squarePos.Y/SquareSize.Y + 1) ||
		       valueTier > _world.GetValueTierAt(squarePos.X/SquareSize.X - 1, squarePos.Y/SquareSize.Y) ||
		       valueTier > _world.GetValueTierAt(squarePos.X/SquareSize.X + 1, squarePos.Y/SquareSize.Y);
	}
	
	private bool IsStepDownNorth(Vector2I squarePos, int valueTier)
	{
		return valueTier > _world.GetValueTierAt(squarePos.X/SquareSize.X, squarePos.Y/SquareSize.Y - 1);
	}
	
	private bool IsStepDownSouth(Vector2I squarePos, int valueTier)
	{
		return valueTier > _world.GetValueTierAt(squarePos.X/SquareSize.X, squarePos.Y/SquareSize.Y + 1);
	}
	
	private bool IsStepDownWest(Vector2I squarePos, int valueTier)
	{
		return valueTier > _world.GetValueTierAt(squarePos.X/SquareSize.X - 1, squarePos.Y/SquareSize.Y);
	}
	
	private bool IsStepDownEast(Vector2I squarePos, int valueTier)
	{
		return valueTier > _world.GetValueTierAt(squarePos.X/SquareSize.X + 1, squarePos.Y/SquareSize.Y);
	}

	private bool IsStepDownNorthWest(Vector2I squarePos, int valueTier)
	{
		return valueTier > _world.GetValueTierAt(squarePos.X/SquareSize.X - 1, squarePos.Y/SquareSize.Y - 1);
	}
	
	private bool IsStepDownNorthEast(Vector2I squarePos, int valueTier)
	{
		return valueTier > _world.GetValueTierAt(squarePos.X/SquareSize.X + 1, squarePos.Y/SquareSize.Y - 1);
	}
	
	private bool IsStepDownSouthWest(Vector2I squarePos, int valueTier)
	{
		return valueTier > _world.GetValueTierAt(squarePos.X/SquareSize.X - 1, squarePos.Y/SquareSize.Y + 1);
	}

	private bool IsStepDownSouthEast(Vector2I squarePos, int valueTier)
	{
		return valueTier > _world.GetValueTierAt(squarePos.X/SquareSize.X + 1, squarePos.Y/SquareSize.Y + 1);
	}
}