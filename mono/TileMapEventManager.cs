using Godot;
using System;

public partial class TileMapEventManager : Node
{
    private TileMap _tileMap;

    public override void _Ready()
    {
        _tileMap = GetParent<TileMap>();
    }

    public void HandleRightClick()
    {
        GD.Print( (Vector2I) (_tileMap.GetLocalMousePosition() / _tileMap.TileSet.TileSize));
    }
    
}
