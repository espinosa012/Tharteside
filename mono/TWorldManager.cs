using Godot;

namespace Tartheside.mono;

public partial class TWorldManager : Node2D
{	
	[Export] public Vector2I WorldSize;
	[Export] public Vector2I TileMapOffset;
	[Export] public Vector2I ChunkSize;
	[Export] public Vector2I SquareSize;
	[Export] public Vector2I InitChunks;	// Chunks que se inicializarán al principio

	private TWorld _world;
	private TileMap _tileMap;

	public override void _Ready()
	{
		_world = new TWorld();
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
				var worldPosition = new Vector2I((x/SquareSize.X)+TileMapOffset.X, (y/SquareSize.Y)+TileMapOffset.Y);
				var valueTier = _world.GetValueTierAt(worldPosition.X, worldPosition.Y);

				FulfillSquare(new Vector2I(x, y), valueTier);	// escalado del mapa
			}
		}
	}
	public void UpdateTileMap()
	{
		_tileMap.Clear();
		TileMapSetup();
	}

	public void FulfillSquare(Vector2I squarePosition, int valueTier)
	{	
		// Comprobamos si tiene frontera con un tier distinto
		var isFrontier = valueTier == _world.GetValueTierAt(squarePosition.X + 1, squarePosition.Y) || 
		                 valueTier == _world.GetValueTierAt(squarePosition.X - 1, squarePosition.Y) ||
		                 valueTier == _world.GetValueTierAt(squarePosition.X, squarePosition.Y + 1) ||
		                 valueTier == _world.GetValueTierAt(squarePosition.X, squarePosition.Y - 1);
		for (var i = 0; i < SquareSize.X; i++)
		{
			for (var j = 0; j < SquareSize.Y; j++)
			{
				SetCell(new Vector2I(squarePosition.X+i, squarePosition.Y+j), valueTier, isFrontier);	
			}
		}
	}

	private void SetCell(Vector2I tileMapPosition, int valueTier, bool isFrontier = false)
	{
		var tileMapLayer = 0;
		var tileSetSourceId = 9;
		var tileSetAtlasCoordinates = new Vector2I(valueTier, 0);	// aqui calculamos si somos frontera y en qué dirección
		_tileMap.SetCell(tileMapLayer, new Vector2I(tileMapPosition.X, tileMapPosition.Y), tileSetSourceId, tileSetAtlasCoordinates);
	}

}