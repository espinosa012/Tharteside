using Godot;
using Tartheside.mono.utilities.pathfinding;

public partial class Human : CharacterBody2D
{
    // El Human será asignado a un Sprite2D para mostrarlo en el mundo
    private TileMap _tileMap;
    public TAStar PathfindingAstar;
    public Vector2I CurrentMapPosition;

    public override void _Ready()
    {
        _tileMap = GetParent<TileMap>();
        CurrentMapPosition = (Vector2I)(Position / _tileMap.TileSet.TileSize);
        PathfindingAstar = new TAStar(CurrentMapPosition, CurrentMapPosition + new Vector2I(128, 128));
    }

    
    

}
