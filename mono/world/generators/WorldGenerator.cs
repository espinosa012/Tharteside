using System.Text.RegularExpressions;
using Godot;

namespace Tartheside.mono.world.generators;

public partial class WorldGenerator : GodotObject
{
	private Vector2I _worldSize;    
	private Vector2I _chunkSize;    
	private int _nTiers;
	private float[,] _values;
	
	
	public virtual float GetChunkAverage(int x, int y){
		return 0.0f;
	}//TODO
	
	public virtual float GetRegionAverage(Vector2I center, Vector2I size){
		return 0.0f;
	}//TODO
	
	public virtual float GetValueAt(int x, int y) => 0.0f;

	public int GetValueTierAt(Vector2I pos) =>
		GetValueTierAt(pos.X, pos.Y);
    
	public int GetValueTierAt(int x, int y) =>
		GetValueTier(GetValueAt(x, y));

	public int GetValueTier(float value) =>
		(int)(value / (1.0f / _nTiers));
    

	// NEIGHBOUR EVALUATION (untested)
	public bool IsStepDownAtOffset(int x, int y, int xOffset = 0, int yOffset = 0)
	{
		return GetValueTierAt(x, y) > GetValueTierAt(x + xOffset, y + yOffset);
	}

	public bool IsNStepDownAtOffset(int x, int y, int xOffset = 0, int yOffset = 0, int n = 1)
	{
		return GetValueTierAt(x, y) - GetValueTierAt(x + xOffset, y + yOffset) == n;
	}
	
	public bool IsNStepUpAtOffset(int x, int y, int xOffset = 0, int yOffset = 0, int n = 1)
	{
		return GetValueTierAt(x + xOffset, y + yOffset) - GetValueTierAt(x, y) == n;
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


	// getters & setters	TODO: crear métodos genéricos para obtener parámetros de cualquier generador (get y set)
	public Vector2I GetParameterWorldSize() => _worldSize;
	public void SetParameterWorldSize(Vector2I value) => _worldSize = value;
    
	public int GetParameterNTiers() => _nTiers;
	public void SetParameterNTiers(int value) => _nTiers = value;

	public Vector2I GetParameterChunkSize() => _chunkSize;		// guardamos esto para calcular averages y demás.
	public void SetParameterChunkSize(Vector2I value) => _chunkSize = value;

    
    
	// utilities
    
	private string CamelCaseToSnakeCase(string str)  => 
		Regex.Replace(str, @"([A-Z])", "_$1").TrimStart('_').ToLower();// llevar a clase utilities
    
    
	public void GeneratorToPng()
	{
	    
	}

	public void ChunkToPng(Vector2I chunkPos)
	{
	    
	}
}