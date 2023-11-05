using Godot;
using Godot.Collections;
using Tartheside.mono;

public partial class TileMapEventManager : Node
{
    private WorldTileMap _tileMap;
    public Label PositionLabel;
    public HumanCharacter TestHumanCharacter;
    
    public override void _Ready()
    {
        _tileMap = GetParent<WorldTileMap>();
        TestHumanCharacter = (HumanCharacter) GetNode<CharacterBody2D>("../TestCharacter");
        PositionLabel = new Label();
        AddChild(PositionLabel);
    }

    public void HandleRightClick()
    {
        Vector2I clickedPosition = (Vector2I)(_tileMap.GetLocalMousePosition() / _tileMap.TileSet.TileSize);
        Array<Vector2I> path = TestHumanCharacter.PathfindingAstar.GetPath(TestHumanCharacter.CurrentMapPosition, clickedPosition);

        PositionLabel.Position = _tileMap.GetLocalMousePosition();
        PositionLabel.Text = (clickedPosition + _tileMap.GetTileMapOffset()).ToString();
        
        TestHumanCharacter.RunPath(path);   
    }
    
}
