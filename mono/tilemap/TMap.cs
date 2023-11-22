using Godot;
using Tartheside.mono.world;

namespace Tartheside.mono.tilemap;

public partial class TMap : TileMap
{
	private Vector2I _worldSize;
	private Vector2I _chunkSize;
	private Vector2I _squareSize;
	private Vector2I _chunks;
	private World _world;


	public World GetWorld() => _world;
	public void SetWorld(World world) => _world = world;
	
	public void TileMapSetup(Vector2I chunkSize, Vector2I squareSize,
		Vector2I initChunks)
	{
		_worldSize = new Vector2I((int)_world.GetWorldParameter("WorldSizeX"),
			(int)_world.GetWorldParameter("WorldSizeY")); 
		
		_chunkSize = chunkSize;
		_squareSize = squareSize;
		_chunks = initChunks;

		InitializeChunks();
	}
	
	private void InitializeChunks()
	{
		RenderChunks("Elevation", 1);
	}

	public void RenderChunks(string source, int layer)
	{
		ClearLayer(layer);
		for (var i = 0; i < _chunks.X; i++)
		for (var j = 0; j < _chunks.Y; j++)
			RenderChunk(new Vector2I(i, j), source, layer);
	}


	private void RenderChunk(Vector2I chunkPosition, string source, int layer)
	{
		// TODO: considerar en los bucles el tamaño establecido del mundo. cuando x,y esté fuera de los límites, devolvemos 0.0f
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
				var worldPos = new Vector2I(x/_squareSize.X, y/_squareSize.Y); // posición en el mundo de la celda superior izquierda del cuadro
					
				FulfillSquare(worldPos, layer);  
			}
		}
	}

	private void FulfillSquare(Vector2I worldPos, int layer)
	{
		// TODO: tomar la fuente de las tiles asignándole un nombre en editor (gradiente, rocas, árboles, río, biome, etc.)
		// podríamos separar en scripts los métodos para renderizar cada una de las sources (o métodos estáticos)
		var palette24Id = 10;
		var tier = _world.GetWorldGenerator("Elevation").GetValueTierAt(worldPos.X, worldPos.Y);
		for (int i = 0; i < _squareSize.X; i++)
		for (int j = 0; j < _squareSize.Y; j++)
			SetCell(layer, worldPos + new Vector2I(i, j), palette24Id, new Vector2I(tier, 0));	// tODO: considerar squareSize
	}
	
	public void ReloadTileMap()
	{
		Clear();
		InitializeChunks();
	}}