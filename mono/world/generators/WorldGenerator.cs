using System.Text.RegularExpressions;
using Godot;

namespace Tartheside.mono.world.generators;

public partial class WorldGenerator : GodotObject
{
	private Vector2I _worldSize;    
	private Vector2I _chunkSize;    
	private int _nTiers;
	
	private float[,] _values;


	public WorldGenerator()
	{
		
	}
	
	public void SetValueAt(int x, int y)
	{
		
	}

	public void SetValueAt(Vector2I pos)
	{
		SetValueAt(pos.X, pos.Y);
	}
	
	public virtual float GetValueAt(int x, int y) => 0.0f;

	public int GetValueTierAt(Vector2I pos) =>
		GetValueTierAt(pos.X, pos.Y);
    
	public int GetValueTierAt(int x, int y) =>
		GetValueTier(GetValueAt(x, y));

	protected int GetValueTier(float value) =>
		(int)(value / (1.0f / _nTiers));
    

	// NEIGHBOUR EVALUATION (untested)
	public bool IsStepDownAtOffset(int x, int y, int xOffset = 0, int yOffset = 0) => 
		GetValueTierAt(x, y) > GetValueTierAt(x + xOffset, y + yOffset);
	
	public bool IsNStepDownAtOffset(int x, int y, int xOffset = 0, int yOffset = 0, int n = 1) => 
		GetValueTierAt(x, y) - GetValueTierAt(x + xOffset, y + yOffset) == n;
	
	public bool IsNStepUpAtOffset(int x, int y, int xOffset = 0, int yOffset = 0, int n = 1) => 
		GetValueTierAt(x + xOffset, y + yOffset) - GetValueTierAt(x, y) == n;
	
	public bool IsStepUpAtOffset(int x, int y, int xOffset = 0, int yOffset = 0) => 
		GetValueTierAt(x, y) < GetValueTierAt(x + xOffset, y + yOffset);

	public bool IsStepAtOffset(int x, int y, int xOffset = 0, int yOffset = 0) => 
		!IsNoStepAtOffset(x, y, xOffset, yOffset);

	public bool IsNoStepAtOffset(int x, int y, int xOffset = 0, int yOffset = 0) => 
		GetValueTierAt(x, y) == GetValueTierAt(x + xOffset, y + yOffset);

	
	// getters & setters	TODO: crear métodos genéricos para obtener parámetros de cualquier generador (get y set)
	public int GetParameterNTiers() => _nTiers;
	public void SetParameterNTiers(int value) => _nTiers = value;
	
	public Vector2I GetParameterWorldSize() => _worldSize;
	public void SetParameterWorldSize(Vector2I value) => _worldSize = value;
	
	public Vector2I GetParameterChunkSize() => _chunkSize;		// guardamos esto para calcular averages y demás.
	public void SetParameterChunkSize(Vector2I value) => _chunkSize = value;
	
}