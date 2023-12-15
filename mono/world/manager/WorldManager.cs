using Godot;
using Tartheside.mono.tilemap;
using Tartheside.mono.utilities.command_line;

namespace Tartheside.mono.world.manager;

public partial class WorldManager : Node2D
{	
	private World _world;
	private TMap _tileMap;
	private TCommandLine _commandLine;
	
	public override void _Ready()
	{
		WorldSetup();
		TileMapSetup();
	}

	// World
	private void WorldSetup()
	{
		_world = new World();
		
		_world.InitElevation();
		_world.InitRiver();
		_world.InitLatitude();
		_world.InitTemperature();
	}

	// Tilemap
	private void TileMapSetup()
	{
		TileMapWindowSetup();
		_tileMap.SetWorld(_world);
		_tileMap.Setup();
		_tileMap.InitializeChunks();
	}

	private void TileMapWindowSetup(string name = "Tilemap")
	{
		var tileMapWindow = new Window();
		tileMapWindow.Size = new Vector2I(1800, 864);
		tileMapWindow.Position = new Vector2I(64, 84);
		_tileMap = GetWorldTileMapFromSceneFile();
		_tileMap.Name = name;
		_tileMap.Position = new Vector2I(0, 0);
		tileMapWindow.AddChild(_tileMap);	
		AddChild(tileMapWindow);
	}
	
	private static TMap GetWorldTileMapFromSceneFile() =>
		GD.Load<PackedScene>("res://scenes/WorldTileMap.tscn").Instantiate<TMap>();
	
	
	// Getters
	public World GetWorld() => _world;
	public TMap GetTilemap() => _tileMap;

}