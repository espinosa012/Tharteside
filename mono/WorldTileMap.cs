using System;
using Godot;

namespace Tartheside.mono;

public partial class WorldTileMap : TileMap
{
	private TileMapEventManager _eventManager;
	private Vector2I _worldSize;
	private Vector2I _tileMapOffset;
	private Vector2I _chunkSize;
	private Vector2I _squareSize;
	private Vector2I _chunks; // Chunks que se inicializarán al principio
	private World _world;

	// getters & setters
	public World GetWorld() => _world;
	public void SetWorld(World world) => _world = world;

	public Vector2I GetWorldSize() => _worldSize;
	public void SetWorldSize(Vector2I newSize) => _worldSize = newSize;

	public Vector2I GetTileMapOffset() => _tileMapOffset;
	public void SetTileMapOffset(Vector2I newOffset) => _tileMapOffset = newOffset;

	public Vector2I GetTileMapChunks() => _chunks;
	public void SetTileMapChunks(Vector2I newSize) => _chunks = newSize;

	public Vector2I GetChunkSize() => _chunkSize;
	public void SetChunkSize(Vector2I newSize) => _chunkSize = newSize;

	public Vector2I GetSquareSize() => _squareSize;
	public void SetSquareSize(Vector2I newSize) => _squareSize = newSize;

	// tilemap
	public void TileMapSetup(Vector2I worldSize, Vector2I offset, Vector2I chunkSize, Vector2I squareSize,
		Vector2I initChunks)
	{
		_worldSize = worldSize;
		_tileMapOffset = offset;
		_chunkSize = chunkSize;
		_squareSize = squareSize;
		_chunks = initChunks;

		InitializeChunks();
	}

	private void InitializeChunks()
	{
		for (var i = 0; i < _chunks.X; i++)
		{
			for (var j = 0; j < _chunks.Y; j++)
			{
				RenderChunk(new Vector2I(i, j));
			}
		}
	}

	public void RenderChunk(Vector2I chunkPosition)
	{
		for (var x = chunkPosition.X * _chunkSize.X * _squareSize.X;
		     x < _chunkSize.X * _squareSize.X +
		     chunkPosition.X * _chunkSize.X * _squareSize.X;
		     x += _squareSize.X)
		{
			for (var y = chunkPosition.Y * _chunkSize.Y * _squareSize.Y;
			     y < _chunkSize.Y * _squareSize.Y +
			     chunkPosition.Y * _chunkSize.Y * _squareSize.Y;
			     y += _squareSize.Y)
			{
				var squarePos = new Vector2I(x, y); // posición en el mundo de la celda superior izquierda del cuadro
				// escalado del mapa
				FulfillSquareTerrain(squarePos); // debajo irían el resto de capas (minas, etc.) 
			}
		}
	}

	private void FulfillSquareTerrain(Vector2I worldPos, int tileMapLayer = 0)
	{
		// world pos: la posición del mundo recibida coincide con la de comienzo del square
		for (var i = 0; i < _squareSize.X; i++)
		{
			for (var j = 0; j < _squareSize.Y; j++)
			{
				Vector3I terrainTileToPlace = GetTerrainTileToPlace(worldPos);
				var tilePosition = GetTilePositionByWorldPositon(worldPos, i, j);
				SetCell(tilePosition, new Vector2I(terrainTileToPlace.X, terrainTileToPlace.Y),
					terrainTileToPlace.Z, tileMapLayer);
			}
		}
	}

	private void FulfillSquareElevationStep(Vector2I worldPos, int tileMapLayer = 1)
	{//TODO
		Vector2I worldPosition = GetWorldPosBySquare(worldPos);	// para considerar el offset
		Elevation elevation = (Elevation) _world.GetWorldParameter("Elevation");
		Vector2I[] neighbours = {Vector2I.Up, Vector2I.Down, Vector2I.Left, Vector2I.Right};
		
		foreach (var pos in neighbours)
		{
			if (elevation.IsNStepUpAtOffset(worldPos.X, worldPos.Y, pos.X, pos.Y, 1))
			{
				
			}	
		}
	}
	
	private Vector2I GetTilePositionByWorldPositon(Vector2I worldPos, int squarePosX, int squarePosY)
	{
		return new Vector2I(worldPos.X + squarePosX, worldPos.Y + squarePosY);
	}

	private Vector3I GetTerrainTileToPlace(Vector2I worldPos)
	{
		Vector2I worldPosition = GetWorldPosBySquare(worldPos);	// para considerar el offset
		var valueSource = _world.GetWorldNoise("Continentalness");	// caepta cualquier calse que implemente un GetValueTier para dos enteros
		return GetValueTileByPalette(worldPosition, new Callable(valueSource, "GetValueTierAt"), 10);
	}

	private Vector3I GetElevationStepTileToPlace(Vector2I offset)
	{
		// devolvemos Vector3I(atlasCoord.X, atlasCoord.Y, tileSetSourceId)
		// recorremos sólo las celdas del square que se corresponaan en función de la dirección del escalón (offset)
		int tileSetSourceId = 5;

		if (offset == Vector2I.Up)	return new Vector3I(1, 5, tileSetSourceId);
		if (offset == Vector2I.Down)	return new Vector3I(1, 3, tileSetSourceId);
		if (offset == Vector2I.Left)	return new Vector3I(2, 4, tileSetSourceId);
		if (offset == Vector2I.Right)	return new Vector3I(0, 4, tileSetSourceId);
		
		// no debe devolver esto nunca, se llama sólo cuando sabemos que es step
		return Vector3I.Zero;

	}

	public Vector3I GetValueTileByPalette(Vector2I worldPos, Callable valueCallable, int tileSetSourceId)
	{
		return new Vector3I( (int) valueCallable.Call(worldPos.X, worldPos.Y), 
			0, tileSetSourceId);
	}

	
	private Vector2I GetWorldPosBySquare(Vector2I squarePos)
	{
		return new Vector2I((squarePos.X/_squareSize.X)+_tileMapOffset.X, (squarePos.Y/_squareSize.Y)+_tileMapOffset.Y);
	}
	
	private void SetCell(Vector2I tileMapPosition, Vector2I tileSetAtlasCoordinates, int tileSetSourceId, 
		int tileMapLayer)
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
		// con el enfoque de Squares, todas los tiles (posiciones de tilemap) de un square, pertenecerán a la misma 
		// world_position.
		// devuelve la posición de la esquina superior izq del cuadro al que pertenece la tile
		return new Vector2I((int)Math.Floor(((decimal)tileMapCell.X/_squareSize.X)), 
			(int)Math.Floor(((decimal)tileMapCell.Y/_squareSize.Y)));
	}
	
	public void ReloadTileMap()
	{
		Clear();
		InitializeChunks();
	}
	
	// Eventos. Llevar a clase externa 
	public override void _Input(InputEvent @event)
	{
		if (@event.IsPressed() && @event.AsText().Equals("Left Mouse Button"))	// optimizar
		{
			GetNode<TileMapEventManager>("%TileMapEventManager").HandleRightClick();
		}
	}
}
