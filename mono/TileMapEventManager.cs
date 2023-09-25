using Godot;
using Godot.Collections;

public partial class TileMapEventManager : Node
{
    private TileMap _tileMap;

    public override void _Ready()
    {
        _tileMap = GetParent<TileMap>();
    }

    public void HandleRightClick()
    {
        Human testHuman = (Human) GetNode<CharacterBody2D>("../TestCharacter");
        Vector2I clickedPosition = (Vector2I)(_tileMap.GetLocalMousePosition() / _tileMap.TileSet.TileSize);
        Array<Vector2I> path = testHuman.PathfindingAstar.GetPath(testHuman.CurrentMapPosition, clickedPosition);
        //GD.Print(clickedPosition);
        //GD.Print(path);
        //testHuman.RunPath(path);
        testHuman.RunPath(path);   
    }
    
}
