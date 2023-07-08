using System;
using Godot;

namespace Tartheside.mono;

public partial class WorldTileMap : TileMap
{
	private Vector2I WorldSize { get; set; }
	private Vector2I TileMapOffset { get; set; }
	private Vector2I ChunkSize { get; set; }
	private Vector2I SquareSize { get; set; }
	private Vector2I InitChunks { get; set; }	// Chunks que se inicializarán al principio

	private World _world;
	
	
	public override void _Ready()
	{
	}

	public void SetWorldSize(Vector2I newSize)
	{
		WorldSize = newSize;
	}

	public void SetTileMapOffset(Vector2I newOffset)
	{
		TileMapOffset = newOffset;
	}

	public void SetChunkSize(Vector2I newSize)
	{
		ChunkSize = newSize;
	}
	
	public void SetSquareSize(Vector2I newSize)
	{
		SquareSize = newSize;
	}
	
	public void SetWorld(World world)
	{
		_world = world;
	}
	
	public void TileMapSetup(Vector2I worldSize, Vector2I offset, Vector2I chunkSize, Vector2I squareSize, Vector2I initChunks)
	{
		WorldSize = worldSize;
		TileMapOffset = offset;
		ChunkSize = chunkSize;
		SquareSize = squareSize;
		InitChunks = initChunks;
		
		InitializeChunks();
	}

	private void InitializeChunks()
	{
		for (var i = 0; i < InitChunks.X; i++)
		{
			for (var j = 0; j < InitChunks.Y; j++)
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
				var squarePos = new Vector2I(x, y);	// posición en el mundo de la celda superior izquierda del cuadro
				FulfillSquare(squarePos);	// escalado del mapa
			}
		}
	}
	
	private void FulfillSquare(Vector2I worldPos)
	{
		// world pos: la posición del mundo recibida coincide con la de comienzo del square
		var valueTier = GetValueTierByWorldPos(GetWorldPosBySquare(worldPos));
		for (var i = 0; i < SquareSize.X; i++)
		{
			for (var j = 0; j < SquareSize.Y; j++)
			{
				var tileSetSource = 10;
				var tileSetAtlasCoordinates = new Vector2I(valueTier, 0);
				var newWorldPosition = new Vector2I(worldPos.X + i, worldPos.Y + j);
				SetCell(newWorldPosition, tileSetAtlasCoordinates, tileSetSource);
				FulfillSquareObstacles(newWorldPosition);
			}
		}
	}

	private void FulfillSquareObstacles(Vector2I worldPos)
	{
		if (TileMapCellIsBorder(worldPos))
		{
			SetCell(worldPos, new Vector2I(5, 5), 5, 1);
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
	
	private void SetCell(Vector2I tileMapPosition, Vector2I tileSetAtlasCoordinates, int tileSetSourceId = 9, int tileMapLayer = 0)
	{
		base.SetCell(tileMapLayer, new Vector2I(tileMapPosition.X, tileMapPosition.Y), tileSetSourceId, 
			tileSetAtlasCoordinates);
	}
	

	
	// Borders
	private bool TileMapCellIsBorder(Vector2I tileMapCell)
	{
		
		// tomamos la posición del square al que pertenece la tile (esa posición coincide con la esquina superior izq del cuadro)
		var squarePos = new Vector2I((int)Math.Floor(((decimal)tileMapCell.X/SquareSize.X)), 
			(int)Math.Floor(((decimal)tileMapCell.Y/SquareSize.Y))); 
		
		// calculamos posición relativa de la celda dentro del square
		var relativeTileMapCellPos = tileMapCell - new Vector2I(squarePos.X * SquareSize.X, squarePos.Y * SquareSize.Y);
		
		// comprobamos los bordes del square correspondiente
		var isStepDownNorth = IsStepDownAtOffset(squarePos, new Vector2I(0, -1));
		var isStepDownSouth = IsStepDownAtOffset(squarePos, new Vector2I(0, 1)); 
		var isStepDownWest = IsStepDownAtOffset(squarePos, new Vector2I(-1, 0)); 
		var isStepDownEast = IsStepDownAtOffset(squarePos, new Vector2I(1, 0));
		var isStepDownNorthWest = IsStepDownAtOffset(squarePos, new Vector2I(-1, -1));
		var isStepDownNorthEast = IsStepDownAtOffset(squarePos, new Vector2I(1, -1));
		var isStepDownSouthWest = IsStepDownAtOffset(squarePos, new Vector2I(-1, 1));
		var isStepDownSouthEast = IsStepDownAtOffset(squarePos, new Vector2I(1, 1));
		
		return (isStepDownNorth && relativeTileMapCellPos.Y == 0) ||
		       (isStepDownSouth && relativeTileMapCellPos.Y == SquareSize.Y - 1) ||
		       (isStepDownWest && relativeTileMapCellPos.X == 0) ||
		       (isStepDownEast && relativeTileMapCellPos.X == SquareSize.X - 1) ||
		       (isStepDownNorthWest && relativeTileMapCellPos.X == 0 && relativeTileMapCellPos.Y == 0) ||
		       (isStepDownNorthEast && relativeTileMapCellPos.X == SquareSize.X - 1 && relativeTileMapCellPos.Y == 0) ||
		       (isStepDownSouthWest && relativeTileMapCellPos.X == 0 && relativeTileMapCellPos.Y == SquareSize.Y - 1) ||
		       (isStepDownSouthEast && relativeTileMapCellPos.X == SquareSize.X - 1 && relativeTileMapCellPos.Y == SquareSize.Y - 1);
		
	}

	private bool IsStepDownAtOffset(Vector2I squarePos, Vector2I offset)
	{
		return GetValueTierByWorldPos(squarePos + TileMapOffset) > GetValueTierByWorldPos(squarePos + offset + TileMapOffset);
	}
	
	
	public void UpdateTileMap()
	{
		Clear();
		InitializeChunks();
	}
	
}