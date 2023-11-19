using Godot;
using Tartheside.mono.world;

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

	//TODO: no inicializar aquí el npc
	
	// getters & setters
	public Callable GetValueSourceByName(string name)
	{
		// TODO: aquí deberíamos consultar las matrices del generador que corresponda (si es ruido, no)
		name ??= "Elevation";
		if (_world.GetWorldGenerators().ContainsKey(name))
			return new Callable(_world.GetWorldGenerator(name), "GetValueTierAt");
		if (_world.GetWorldNoises().ContainsKey(name))
			return new Callable(_world.GetWorldNoise(name), "GetValueTierAt");
		return new Callable(_world.GetWorldNoise(name), "GetValueTierAt");
	}
	
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
	public void TileMapSetup(Vector2I offset, Vector2I chunkSize, Vector2I squareSize,
		Vector2I initChunks)
	{
		_worldSize = new Vector2I((int)_world.GetWorldParameter("WorldSizeX"),
			(int)_world.GetWorldParameter("WorldSizeY")); 
		
		//TODO: cambiar por getters y setters para acceder desde afuera
		_tileMapOffset = offset;
		_chunkSize = chunkSize;
		_squareSize = squareSize;
		_chunks = initChunks;

		InitializeChunks();
	}

	private void InitializeChunks()
	{
		RenderChunks("Elevation", 0);		// TODO: paralelizar
		RenderChunks("River", 1);
	}

	public void RenderChunks(string source, int layer)
	{
		ClearLayer(layer);
		for (var i = 0; i < _chunks.X; i++)
			for (var j = 0; j < _chunks.Y; j++)
				RenderChunk(new Vector2I(i, j), source, layer);
	}
	
	private void RenderChunk(Vector2I chunkPosition, string source, int layer) // hacer asíncrono para renderizar los chunks en paralelo
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
				FulfillSquare(squarePos, GetValueSourceByName(source), 10, layer); 
			}
		}
	}	

	private void FulfillSquare(Vector2I worldPos, Callable valueSource, int tileSetSourceId, int tileMapLayer)
	{
		// world pos: la posición del mundo recibida coincide con la de comienzo del square
		
		// TODO: no todos los generadores rellenan igual los squares. El río por ejemplo
		for (var i = 0; i < _squareSize.X; i++)
		{
			for (var j = 0; j < _squareSize.Y; j++)
			{
				Vector3I tileToPlace = GetTileToPlace(worldPos, tileSetSourceId, valueSource);
				var tilePosition = GetTilePositionByWorldPosition(worldPos, i, j);
				SetCell(tilePosition, new Vector2I(tileToPlace.X, tileToPlace.Y),
					tileToPlace.Z, tileMapLayer);
			}
		}
	}
	
	private Vector2I GetTilePositionByWorldPosition(Vector2I worldPos, int squarePosX, int squarePosY)
	{
		return new Vector2I(worldPos.X + squarePosX, worldPos.Y + squarePosY);
	}

	private Vector3I GetTileToPlace(Vector2I worldPos, int tileSetSourceId, Callable valueSourceCallable)
	{
		Vector2I worldPosition = GetWorldPosBySquare(worldPos);	// para considerar el offset
		return GetValueTileByPalette(worldPosition, valueSourceCallable, tileSetSourceId);
	}

	private Vector3I GetValueTileByPalette(Vector2I worldPos, Callable valueCallable, int tileSetSourceId)
	{
		return new Vector3I( (int) valueCallable.Call(worldPos.X, worldPos.Y), 
			0, tileSetSourceId);
	}
	
	private Vector2I GetWorldPosBySquare(Vector2I squarePos)
	{
		// consideramos offset
		return new Vector2I((squarePos.X/_squareSize.X)+_tileMapOffset.X, (squarePos.Y/_squareSize.Y)+_tileMapOffset.Y);
	}
	
	private void SetCell(Vector2I tileMapPosition, Vector2I tileSetAtlasCoordinates, int tileSetSourceId, 
		int tileMapLayer)
	{
		base.SetCell(tileMapLayer, new Vector2I(tileMapPosition.X, tileMapPosition.Y), tileSetSourceId, 
			tileSetAtlasCoordinates);
	}

	public new void ClearLayer(int layer)
	{
		RemoveLayer(layer);
		AddLayer(layer);	
	}

	public void ReloadTileMap()
	{
		Clear();
		InitializeChunks();
	}
	
	// TODO: Eventos. Llevar a clase externa 
	public override void _Input(InputEvent @event)
	{
		if (@event.IsPressed() && @event.AsText().Equals("Left Mouse Button"))	// optimizar
		{
			GetNode<TileMapEventManager>("%TileMapEventManager").HandleRightClick();
		}
	}
}
