using Godot;
using Tartheside.mono.tilemap;
using Tartheside.mono.utilities.logger;

namespace Tartheside.mono.events;

public partial class TileMapEventManager : Node
{
    private TMap _tileMap;
    private Label PositionLabel;
    private HumanCharacter TestHumanCharacter;
    
    public override void _Ready()
    {
        _tileMap = GetParent<TMap>();
        /*TestHumanCharacter = (HumanCharacter) GetNode<CharacterBody2D>("../TestCharacter");
    PositionLabel = new Label();
    AddChild(PositionLabel);*/
    }

    public void HandleRightClick()
    {
        // TODO: no vale cuando squareSize es mayor que 1
        var clickedPosition = (Vector2I)(_tileMap.GetLocalMousePosition() / _tileMap.TileSet.TileSize);
        var worldPosition = clickedPosition + new Vector2I(
            (int)_tileMap.GetWorld().GetWorldParameter("OffsetX"),
            (int)_tileMap.GetWorld().GetWorldParameter("OffsetY"));
        TLogger.Info("Clicked position: " + worldPosition + " - Elevation value: " + 
                     _tileMap.GetWorld().GetWorldGenerator("Elevation")
                         .GetValueAt(worldPosition.X - (int)_tileMap.GetWorld().GetWorldParameter("OffsetX"), 
                             worldPosition.Y - (int)_tileMap.GetWorld().GetWorldParameter("OffsetY")) 
                     + " - Tier: " + _tileMap.GetWorld().GetWorldGenerator("Elevation")
                         .GetValueTierAt(worldPosition.X - (int)_tileMap.GetWorld().GetWorldParameter("OffsetX"), 
                             worldPosition.Y - (int)_tileMap.GetWorld().GetWorldParameter("OffsetY")));
    }
    
}