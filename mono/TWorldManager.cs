using Godot;
using Godot.Collections;

public partial class TWorldManager : Node2D
{	
	[Export] public Vector2I WorldSize;
	[Export] public Vector2I TileMapOffset;
	[Export] public Vector2I SquareSize;
	[Export] public Vector2I ChunkSize;
    Array<Vector2I> Chunks = new Array<Vector2I>();     


	public TWorld TWorld;
	public TileMap TileMap;

	public override void _Ready()
	{

		TWorld = new TWorld(WorldSize.X, WorldSize.Y);
		TileMap = GetNode<TileMap>("TileMap");
		TileMapSetup();
		// formar tilemap y tileset desde código


		// UI
		BasicWorldPanel panel = GetNode<BasicWorldPanel>("%BasicWorldPanel");
		panel.SetTWorldManager(this);
		panel.InitializeUI();
		panel.ConnectButtonSignal();
	}


	public void TileMapSetup()
	{	
		int valueTier;
		Vector2I worldPosition;
		
		for (int x = 0; x < WorldSize.X * SquareSize.X; x+=SquareSize.X)
		{
			for (int y = 0; y < WorldSize.Y * SquareSize.Y; y+=SquareSize.Y)
			{
				worldPosition = new Vector2I((x/SquareSize.X)+TileMapOffset.X, (y/SquareSize.Y)+TileMapOffset.Y); 	// posición en el generador
				valueTier = TWorld.GetValueTierAt(worldPosition.X, worldPosition.Y);

				FulfillSquare(new Vector2I(x, y), valueTier);	// escalado del mapa
			}
		}
	}	

	public void UpdateTileMap()
	{
		TileMap.Clear();
		TileMapSetup();
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
	public void FulfillSquare(Vector2I squarePosition, int valueTier)
	{	
		// Comprobamos si tiene frontera con un tier distinto
		bool isFrontier = (valueTier != TWorld.GetValueTierAt(squarePosition.X + 1, squarePosition.Y) || valueTier != TWorld.GetValueTierAt(squarePosition.X, squarePosition.Y + 1)) || valueTier != TWorld.GetValueTierAt(squarePosition.X - 1, squarePosition.Y) || valueTier != TWorld.GetValueTierAt(squarePosition.X, squarePosition.Y-1);
		for (int i = 0; i < SquareSize.X; i++)
		{
			for (int j = 0; j < SquareSize.Y; j++)
			{
				SetCell(new Vector2I(squarePosition.X+i, squarePosition.Y+j), valueTier);
			}
		}
	}



}
