using System;
using Godot;

namespace Tartheside.mono;

public partial class WorldTileMap : TileMap
{
	private Vector2I _worldSize;
	private Vector2I _tileMapOffset;
	private Vector2I _chunkSize;
	private Vector2I _squareSize;
	private Vector2I _chunks;	// Chunks que se inicializarán al principio
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
	
	private void RenderChunk(Vector2I chunkPosition)
	{
		for (var x = chunkPosition.X * _chunkSize.X * _squareSize.X; x < _chunkSize.X * _squareSize.X + 
		     chunkPosition.X * _chunkSize.X * _squareSize.X; x+=_squareSize.X)
		{
			for (var y = chunkPosition.Y * _chunkSize.Y * _squareSize.Y; y < _chunkSize.Y * _squareSize.Y + 
			     chunkPosition.Y * _chunkSize.Y * _squareSize.Y; y+=_squareSize.Y)
			{
				var squarePos = new Vector2I(x, y);	// posición en el mundo de la celda superior izquierda del cuadro
				FulfillSquare(squarePos);	// escalado del mapa
			}
		}
	}
	
	private void FulfillSquare(Vector2I worldPos)
	{
		// world pos: la posición del mundo recibida coincide con la de comienzo del square
		for (var i = 0; i < _squareSize.X; i++)
		{
			for (var j = 0; j < _squareSize.Y; j++)
			{
				Vector3I terrainTileToPlace = GetTerrainTileToPlace(worldPos);
				//Vector3I terrainTileToPlace = new Vector3I(_world.GetWorldGenerator("Temperature").GetValueTierAt(worldPos), 0, 10);
				var newWorldPosition = new Vector2I(worldPos.X + i, worldPos.Y + j);
				SetCell(newWorldPosition, new Vector2I(terrainTileToPlace.X, terrainTileToPlace.Y), 
					terrainTileToPlace.Z);
			}
		}
	}

	private Vector3I GetTerrainTileToPlace(Vector2I worldPos)
	{
		Vector2I worldPosition = GetWorldPosBySquare(worldPos); 	

		if (((Biome)_world.GetWorldGenerator("Biome")).IsTerrainSea(worldPosition.X, worldPosition.Y))  
			return new Vector3I(3, 0, 1);
		
		if (((Biome)_world.GetWorldGenerator("Biome")).IsTerrainBeach(worldPosition.X, worldPosition.Y)) 
			return new Vector3I(0, 0, 1);
		
		if (((Biome)_world.GetWorldGenerator("Biome")).IsTerrainLowland(worldPosition.X, worldPosition.Y)) 
			return new Vector3I(0, 0, 2);
		
		return GetValueTileByPalette(worldPosition, _world.GetWorldGenerator("Elevation"));
	}

	public Vector3I GetValueTileByPalette(Vector2I worldPos, WorldGenerator generator) 
	{
		int tileSetSourceId = 10;
		return new Vector3I(generator.GetValueTierAt(worldPos.X, worldPos.Y), 
			0, tileSetSourceId);
	}

	public Vector3I GetValueTileByPalette(Vector2I worldPos, MFNL noise)	// untested
	{
		int tileSetSourceId = 10;
		int nTiers = (int) _world.GetWorldParameter("NTiers");
		int tier = -1;
		for (var i = 0; i < nTiers; i++){if (noise.GetNormalizedNoise2D(worldPos.X, worldPos.Y) 
		                                     < (i + 1.0f)/nTiers){tier = i;}}
		if (tier == -1) tier = nTiers - 1;
		
		return new Vector3I(tier, 0, tileSetSourceId);
	}
	
	private Vector2I GetWorldPosBySquare(Vector2I squarePos)
	{
		return new Vector2I((squarePos.X/_squareSize.X)+_tileMapOffset.X, (squarePos.Y/_squareSize.Y)+_tileMapOffset.Y);
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
	
}
