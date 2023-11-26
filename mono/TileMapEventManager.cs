using Godot;
using Tartheside.mono.utilities.logger;
using TMap = Tartheside.mono.tilemap.TMap;

namespace Tartheside.mono;

public partial class TileMapEventManager : Node
{
    private TMap _tileMap;
    public Label PositionLabel;
    //public HumanCharacter TestHumanCharacter;
    
    public override void _Ready()
    {
        _tileMap = GetParent<Tartheside.mono.tilemap.TMap>();
        //TestHumanCharacter = (HumanCharacter) GetNode<CharacterBody2D>("../TestCharacter");
        PositionLabel = new Label();
        AddChild(PositionLabel);
    }

    public void HandleRightClick()
    {
        // TODO: no vale cuando squareSize es mayor que 1
        var clickedPosition = (Vector2I)(_tileMap.GetLocalMousePosition() / _tileMap.TileSet.TileSize);
        var worldPosition = clickedPosition + new Vector2I(
            (int)_tileMap.GetWorld().GetWorldParameter("OffsetX"),
            (int)_tileMap.GetWorld().GetWorldParameter("OffsetY"));
        TLogger.Info("Clicked position: " + worldPosition);
    }
    
}