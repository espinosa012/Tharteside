using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using Tartheside.mono.utilities.pathfinding;

public partial class HumanCharacter : CharacterBody2D
{
    // El Human será asignado a un Sprite2D para mostrarlo en el mundo
    private TileMap _tileMap;
    public TAStar PathfindingAstar;
    
    [Export] public Vector2I InitialMapPosition;
    public Vector2I CurrentMapPosition;
    public Vector2I TargetMapPosition;
    public bool IsMoving;

    // Human Attributes
    public float MaxSpeed = 40.0f;  // esto perteneceria al Human, no al character
    
    public override void _Ready()
    {
        _tileMap = GetParent<TileMap>();    // !!!
        PathfindingAstar = new TAStar(CurrentMapPosition, 
            CurrentMapPosition + new Vector2I(128, 128));   // atención al tamaño de la región A*
        MoveToMapPosition(InitialMapPosition);
        IsMoving = false;
    }

    public Vector2I MapToGlobalPosition(Vector2I mapPos) => 
        mapPos * _tileMap.TileSet.TileSize;

    private void UpdateCurrentMapPosition() => 
        CurrentMapPosition = (Vector2I)(Position / _tileMap.TileSet.TileSize);
    
    public async void RunPath(Array<Vector2I> path)
    {
        IsMoving = true;
        for (var i = 0; i < path.Count; i++)
        {
            TargetMapPosition = path[i];
            while (!CurrentMapPosition.Equals(TargetMapPosition))
                await MoveToAdjacentPosition(TargetMapPosition);
        }
        IsMoving = false;
        PathfindingAstar.ChangeRegion(CurrentMapPosition - new Vector2I(64, 64), CurrentMapPosition + new Vector2I(64, 64));
    }
    
    private void MoveToMapPosition(Vector2I mapPos)
    {
        // lo usamos con A*. mapPos será siempre adyacente a CurrentMapPosition
        Position = MapToGlobalPosition(mapPos);
        UpdateCurrentMapPosition();
    }

    private async Task MoveToAdjacentPosition(Vector2I mapPos)
    {
        // A*
        MoveToMapPosition(mapPos);
        await WaitSeconds(1.0f/MaxSpeed);
    }
    
    private async Task WaitSeconds(double time)  =>
        await ToSignal(GetTree().CreateTimer(time), SceneTreeTimer.SignalName.Timeout);
    
}
