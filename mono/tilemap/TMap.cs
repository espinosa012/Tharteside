using Godot;
using Tartheside.mono.events;
using Tartheside.mono.world;

namespace Tartheside.mono.tilemap;

public partial class TMap : TileMap
{
	private Vector2I _worldSize;
	private Vector2I _chunkSize;
	private Vector2I _squareSize;
	private Vector2I _chunks;
	private World _world;

	// TODO: crear un Tilemap utilities para renderizar rios, minas, etc
	public World GetWorld() => _world;
	
	public void SetWorld(World world) => _world = world;
	
	public void Setup()	// TODO: podríamos pasarle el mundo como argumento y llamar desde aquí a SetWorld()
	{
		_worldSize = new Vector2I((int)_world.GetWorldParameter("WorldSizeX"),
			(int)_world.GetWorldParameter("WorldSizeY"));
		_chunkSize = new Vector2I((int)_world.GetWorldParameter("ChunkSizeX"),
			(int)_world.GetWorldParameter("ChunkSizeY"));
		_squareSize = new Vector2I((int)_world.GetWorldParameter("SquareSizeX"),
			(int)_world.GetWorldParameter("SquareSizeY"));
		_chunks = new Vector2I((int)_world.GetWorldParameter("InitChunksX"),
			(int)_world.GetWorldParameter("InitChunksY"));
	}
	
	public void InitializeChunks()
	{
		//TODO: ¿quizás la generación por sources deberíamos hacerla a nivel de square y no de chunk?
		// TODO: es necesario que el tilemap tenga tantas capas como generadores queramos representar (a la vez)
		RenderChunks("Elevation", 0);
		RenderChunks("River", 1);
		//RenderChunks("Latitude", 0);
		//RenderChunks("Temperature", 0);
	}

	public void RenderChunks(string source, int layer)
	{
		// TODO: tal y como está ahora (17/12/23) el parámetro WorldSize no tiene influencia en el tilemap, sólo en el 
		// tamaño de la matriz. Sóol influye el tamaño de los chunks y el número de chunks. 
		// TODO: quizá habría que limitar el número de chunks automáticamente en función del tamaño establecido del mundo
		ClearLayer(layer);	
		for (var i = 0; i < _chunks.X; i++)
		for (var j = 0; j < _chunks.Y; j++)
			RenderChunk(new Vector2I(i, j), source, layer);
	}

	private void RenderChunk(Vector2I chunkPosition, string source, int layer)
	{
		// TODO: considerar en los bucles el tamaño establecido del mundo. cuando x,y esté fuera de los límites, devolvemos 0.0f
		for (var x = chunkPosition.X * _chunkSize.X * _squareSize.X;
		     x < _chunkSize.X * _squareSize.X + chunkPosition.X * _chunkSize.X * _squareSize.X;
		     x += _squareSize.X)
			for (var y = chunkPosition.Y * _chunkSize.Y * _squareSize.Y;
			     y < _chunkSize.Y * _squareSize.Y + chunkPosition.Y * _chunkSize.Y * _squareSize.Y;
			     y += _squareSize.Y)
				RenderSquare(new Vector2I(x / _squareSize.X, y / _squareSize.Y), layer, source);  
	}

	private void RenderSquare(Vector2I worldPos, int layer, string source)
	{
		// podríamos separar en scripts los métodos para renderizar cada una de las sources (o métodos estáticos)
		var palette24Id = 10;
		var tier = _world.GetWorldGenerator(source).GetValueTierAt(worldPos.X, worldPos.Y);
		for (int i = 0; i < _squareSize.X; i++)
			for (int j = 0; j < _squareSize.Y; j++)
				SetCell(layer, GetTilePositionByWorldPosition(new Vector2I(worldPos.X * _squareSize.X, 
						worldPos.Y * _squareSize.Y), i, j), 
					palette24Id, new Vector2I(tier, 0));
	}
	
	private static Vector2I GetTilePositionByWorldPosition(Vector2I mapPos, int squarePosX, int squarePosY) => 
		mapPos + new Vector2I(squarePosX, squarePosY);
	

	
	// TODO: Eventos. Llevar a clase externa 
	public override void _Input(InputEvent @event)
	{
		if (@event.IsPressed() && @event.AsText().Equals("Left Mouse Button"))	// optimizar
			GetNode<TileMapEventManager>("%TileMapEventManager").HandleRightClick();
	}
	
	
}