using Godot;
using System;

public partial class WorldGenerator : GodotObject
{
    private Vector2I _chunkSize;    
    private int _nTiers;    


	public virtual float GetChunkAverage(int x, int y){
		return 0.0f;
	}

    public virtual float GetValueAt(int x, int y)
    {
        return 0.0f;
    }

    public int GetValueTierAt(Vector2I pos)
    {
	    return GetValueTierAt(pos.X, pos.Y);
    }
    
    public int GetValueTierAt(int x, int y)
    	{
		//  para valores en el rango 0-1
        return GetValueTier(GetValueAt(x, y));
	}

    public int GetValueTier(float value)
    {
        for (var i = 0; i < _nTiers; i++){if (value < (i + 1.0f)/(float) _nTiers){return i;}}
		return _nTiers - 1;
    }
    
    



    // NEIGHBOUR EVALUATION (untested)
	public bool IsStepDownAtOffset(int x, int y, int xOffset = 0, int yOffset = 0)
	{
		return GetValueTierAt(x, y) > GetValueTierAt(x + xOffset, y + yOffset);
	}

	public bool IsStepUpAtOffset(int x, int y, int xOffset = 0, int yOffset = 0)
	{
		return GetValueTierAt(x, y) < GetValueTierAt(x + xOffset, y + yOffset);
	}

	public bool IsStepAtOffset(int x, int y, int xOffset = 0, int yOffset = 0)
	{
		return !IsNoStepAtOffset(x, y, xOffset, yOffset);
	}

	public bool IsNoStepAtOffset(int x, int y, int xOffset = 0, int yOffset = 0)
	{
		return GetValueTierAt(x, y) == GetValueTierAt(x + xOffset, y + yOffset);
	}


    // getters & setters
    public int GetParameterNTiers() => _nTiers;
    public void SetParameterNTiers(int value) => _nTiers = value;

    public Vector2I GetParameterChunkSize() => _chunkSize;		// guardamos esto para calcular averages y demás.
    public void SetParameterChunkSize(Vector2I value) => _chunkSize = value;

}
