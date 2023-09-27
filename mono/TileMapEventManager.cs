using Godot;
using Godot.Collections;

public partial class TileMapEventManager : Node
{
    private TileMap _tileMap;
    public Label PositionLabel;
    public HumanCharacter TestHumanCharacter;
    
    public override void _Ready()
    {
        _tileMap = GetParent<TileMap>();
        TestHumanCharacter = (HumanCharacter) GetNode<CharacterBody2D>("../TestCharacter");
        PositionLabel = new Label();
        AddChild(PositionLabel);
    }

    public void HandleRightClick()
    {
        Vector2I clickedPosition = (Vector2I)(_tileMap.GetLocalMousePosition() / _tileMap.TileSet.TileSize);
        Array<Vector2I> path = TestHumanCharacter.PathfindingAstar.GetPath(TestHumanCharacter.CurrentMapPosition, clickedPosition);

        PositionLabel.Position = _tileMap.GetLocalMousePosition();
        PositionLabel.Text = clickedPosition.ToString();
        
        TestHumanCharacter.RunPath(path);   
    }
    
}
