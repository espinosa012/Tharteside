using System;
using Godot;

namespace Tartheside.mono;

public partial class WorldTileMap : TileMap
{
	private Vector2I WorldSize { get; set; }
	private Vector2I TileMapOffset { get; set; }
	private Vector2I ChunkSize { get; set; }
	private Vector2I SquareSize { get; set; }
	private Vector2I Chunks { get; set; }	// Chunks que se inicializarán al principio

	private World _world;
	

	public void SetWorldSize(Vector2I newSize)
	{
		WorldSize = newSize;
	}

	public void SetTileMapOffset(Vector2I newOffset)
	{
		TileMapOffset = newOffset;
	}

	public void SetTileMapChunks(Vector2I newSize)
	{
		Chunks = newSize;
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
	
	public void TileMapSetup(Vector2I worldSize, Vector2I offset, Vector2I chunkSize, Vector2I squareSize, 
		Vector2I initChunks, bool displayBorders = false)
	{
		WorldSize = worldSize;
		TileMapOffset = offset;
		ChunkSize = chunkSize;
		SquareSize = squareSize;
		Chunks = initChunks;
		
		InitializeChunks(displayBorders);
	}

	private void InitializeChunks(bool displayBorders = false, String property = "")
	{
		for (var i = 0; i < Chunks.X; i++)
		{
			for (var j = 0; j < Chunks.Y; j++)
			{
				RenderChunk(new Vector2I(i, j), displayBorders, property);		
			}
		}
	}
	
	private void RenderChunk(Vector2I chunkPosition, bool displayBorders = false, String property = "")
	{
		for (var x = chunkPosition.X * ChunkSize.X * SquareSize.X; x < ChunkSize.X * SquareSize.X + chunkPosition.X * ChunkSize.X * SquareSize.X; x+=SquareSize.X)
		{
			for (var y = chunkPosition.Y * ChunkSize.Y * SquareSize.Y; y < ChunkSize.Y * SquareSize.Y + chunkPosition.Y * ChunkSize.Y * SquareSize.Y; y+=SquareSize.Y)
			{
				var squarePos = new Vector2I(x, y);	// posición en el mundo de la celda superior izquierda del cuadro
				FulfillSquare(squarePos, displayBorders, property);	// escalado del mapa
			}
		}
	}
	
	private void FulfillSquare(Vector2I worldPos, bool displayBorders = false, String property = "")
	{
		// world pos: la posición del mundo recibida coincide con la de comienzo del square
		for (var i = 0; i < SquareSize.X; i++)
		{
			for (var j = 0; j < SquareSize.Y; j++)
			{
				var terrainTileToPlace = GetTileToPlace(worldPos, property);
				var newWorldPosition = new Vector2I(worldPos.X + i, worldPos.Y + j);
				SetCell(newWorldPosition, new Vector2I(terrainTileToPlace.X, terrainTileToPlace.Y), 
					terrainTileToPlace.Z);
				if (displayBorders)
				{
					FulfillSquareObstacles(newWorldPosition);
				}
			}
		}
	}

	private Vector3I GetTileToPlace(Vector2I worldPos, String property = "")
	{
		if (property != "")
		{
			const int tileSetSourceId = 10;
			return new Vector3I(_world.GetValueTierAt(worldPos, property), 0, tileSetSourceId);
		}

		return GetTerrainTileToPlace(worldPos);
	}
	
	private Vector3I GetTerrainTileToPlace(Vector2I worldPos)
	{
		worldPos = GetWorldPosBySquare(worldPos); 	
		var tileSetSourceId = 10;
		var valueTier = GetElevationTierByWorldPos(worldPos);
		
		if (_world.IsTerrainSea(worldPos.X, worldPos.Y))
		{
			return new Vector3I(3, 0, 1);
		}
		if (_world.IsTerrainBeach(worldPos.X, worldPos.Y))
		{
			return new Vector3I(0, 0, 1);
		}
		if (_world.IsTerrainRock(worldPos.X, worldPos.Y))
		{
			return new Vector3I(5, 5, 5);
		}

		return new Vector3I(valueTier, 0, tileSetSourceId);
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
	
	private int GetElevationTierByWorldPos(Vector2I worldPosition)
	{
		return _world.GetValueTierAt(worldPosition.X, worldPosition.Y);
	}
	
	private void SetCell(Vector2I tileMapPosition, Vector2I tileSetAtlasCoordinates, int tileSetSourceId = 9, 
		int tileMapLayer = 0)
	{
		base.SetCell(tileMapLayer, new Vector2I(tileMapPosition.X, tileMapPosition.Y), tileSetSourceId, 
			tileSetAtlasCoordinates);
	}
	

	/// <summary>
	/// Indica con qué posición del WORLD se corresponde la posición del TILEMAP que indiquemos.
	/// Es decir, devuelve la posición del SQUARE al que pertenece tileMapCell
	/// </summary>
	/// <param name="tileMapCell"></param>
	/// <returns></returns>
	private Vector2I GetWorldPositionByTileMapPosition(Vector2I tileMapCell)
	{
		// devuelve la posición de la esquina superior izq del cuadro al que pertenece la tile
		return new Vector2I((int)Math.Floor(((decimal)tileMapCell.X/SquareSize.X)), 
			(int)Math.Floor(((decimal)tileMapCell.Y/SquareSize.Y)));
	}

	/// <summary>
	/// Determina si la posición del TILEMAP que indiquemos se corresponde con una frontera entre niveles de elevación
	/// </summary>
	/// <param name="tileMapCell">TILEMAP position (not WORLD position)</param>
	/// <returns></returns>
	private bool TileMapCellIsBorder(Vector2I tileMapCell)
	{
		
		// tomamos la posición del square al que pertenece la tile (esa posición coincide con la esquina superior
		// izq del cuadro)
		var squarePos = GetWorldPositionByTileMapPosition(tileMapCell); 
		
		// calculamos posición relativa de la celda dentro del square
		var relativeTileMapCellPos = tileMapCell - new Vector2I(squarePos.X * SquareSize.X, 
			squarePos.Y * SquareSize.Y);
		
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
		       (isStepDownNorthWest && relativeTileMapCellPos is { X: 0, Y: 0 }) ||
		       (isStepDownNorthEast && relativeTileMapCellPos.X == SquareSize.X - 1 && relativeTileMapCellPos.Y == 0) ||
		       (isStepDownSouthWest && relativeTileMapCellPos.X == 0 && relativeTileMapCellPos.Y == SquareSize.Y - 1) ||
		       (isStepDownSouthEast && relativeTileMapCellPos.X == SquareSize.X - 1 
		                            && relativeTileMapCellPos.Y == SquareSize.Y - 1);
		
	}
	
	private bool IsStepDownAtOffset(Vector2I squarePos, Vector2I offset)
	{
		return GetElevationTierByWorldPos(squarePos + TileMapOffset) > GetElevationTierByWorldPos(squarePos + offset + TileMapOffset);
	}

	private bool IsStepUpAtOffset(Vector2I squarePos, Vector2I offset)
	{
		return GetElevationTierByWorldPos(squarePos + TileMapOffset) < GetElevationTierByWorldPos(squarePos + offset + TileMapOffset);
	}
	
	private bool IsStepAtOffset(Vector2I squarePos, Vector2I offset)
	{
		return GetElevationTierByWorldPos(squarePos + TileMapOffset) != GetElevationTierByWorldPos(squarePos + offset + TileMapOffset);
	}
	
	
	
	public void ReloadTileMap(string property = "", bool displayBorders = false)
	{
		Clear();
		InitializeChunks(displayBorders, property);
	}
	
}
