using Godot;
using System;
using Godot.Collections;
using Tartheside.mono.world.generators;

public partial class RiverEntity : WorldEntity
{
    public string Name;
    
    private Vector2I _birthPosition;
    private Vector2I _mouthPosition;

    public Array<Vector2I> RiverPath;
    
    public RiverEntity()
    {
        RiverPath = new Array<Vector2I>();
    }

    public void SetBirthPosition(int x, int y) => _birthPosition = new Vector2I(x, y);
    public void SetMouthPosition(int x, int y) => _mouthPosition = new Vector2I(x, y);

}
