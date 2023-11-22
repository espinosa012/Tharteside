using Godot;
using Tartheside.mono.utilities.command_line;
using Tartheside.mono.world;
using Tartheside.mono.tilemap;

namespace Tartheside.mono;


public partial class WorldManager : Node2D
{	
	//[Export] public string HeightMap;	// testing
	[Export] private Vector2I _tileMapOffset;	//TODO: hacer el offset por chunks, considerando squaresize
	[Export] private Vector2I _chunkSize;
	[Export] private Vector2I _squareSize;
	[Export] private Vector2I _initChunks;	

	private World _world;
	//private WorldTileMap _tileMap;
	private TMap _tileMap;
	private TCommandLine _commandLine;
	
	public override void _Ready()
	{
		InitializeWorld();
		InitializeTileMap();	
		//InitializeCommandLine();
	}

	private void InitializeCommandLine()
	{
		_commandLine = (TCommandLine) GetNode<LineEdit>("cmd");
		//_commandLine.Init(_world, _tileMap);
	}
	
	private TMap GetWorldTileMapFromTscn() =>
		GD.Load<PackedScene>("res://scenes/WorldTileMap.tscn").Instantiate<TMap>();
	
	private void InstantiateTileMap(string name = "Tilemap")
	{
		Window tileMapWindow = new Window();
		tileMapWindow.Size = new Vector2I(1480, 1480);
		tileMapWindow.Position = new Vector2I(64, 84);
		_tileMap = GetWorldTileMapFromTscn();
		_tileMap.Name = name;
		_tileMap.Position = new Vector2I(0, 0);
		tileMapWindow.AddChild(_tileMap);	// crear nodo de control	
		AddChild(tileMapWindow);
	}
	
	private void InitializeTileMap()
	{
		InstantiateTileMap();
		_tileMap.SetWorld(_world);
		_tileMap.TileMapSetup(_tileMapOffset, _chunkSize, _squareSize, _initChunks);
		// pasamos al tilemap los parámetros que no afectan a los valores generados, a excepción del tamaño, que lo
		// guardamos en el json como parámetro del mundo también (¿seguro que debería ser así?)
	}
	
	private void InitializeWorld()
	{
		_world = new World();
		_world.UpdateWorldParameter("ChunkSize", _chunkSize);	
		//TODO: lo dejamos aquí para cogerlo del editor. no afecta a los valores generados
		
		_world.InitElevation();
		_world.InitRiver();
		//_world.InitHumidity();
		//_world.InitTemperature();
		//_world.InitTerrain();
		//_world.InitHeightMap(HeightMap + ".png");
	}


}