using Godot;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace Tartheside.mono.world.generators;

public partial class WorldGenerator : GodotObject
{
	private Vector2I _worldSize;    
	private Vector2I _chunkSize;    
	private Vector2I _offset;    
	private int _nTiers;
	private readonly Matrix<float> _valueMatrix;

	
	
	public WorldGenerator(int matrixSizeX, int matrixSizeY)
	{
		_valueMatrix = DenseMatrix.Build.Dense(matrixSizeX, matrixSizeY, -1.0f);
	}

	public void FillValueMatrix(int offsetX, int offsetY)
	{
		// TODO: necesitamos poder indicar qué region de la matriz queremos generar, por eficiencia.
		for (int i = offsetX; i < _worldSize.X + offsetX; i++)
		for (int j = offsetY; j < _worldSize.Y + offsetY; j++)
			_valueMatrix[i-offsetX, j-offsetY] = GenerateValueAt(i, j);
	}
	
	public void SetValueAt(int x, int y, float value) => _valueMatrix[x-_offset.X, y - _offset.Y] = value;	// TODO: da problemas con el offset

	public virtual float GetValueAt(int x, int y) => _valueMatrix[x, y];
	
	public virtual float GenerateValueAt(int x, int y) => 0.0f;
	
	public int GetValueTierAt(Vector2I pos) => GetValueTierAt(pos.X, pos.Y);
    
	//public int GetValueTierAt(int x, int y) => GetValueTier(GetValueAt(x, y));
	public int GetValueTierAt(int x, int y) => GetValueTier(GetValueAt(x, y));

	private int GetValueTier(float value) => (int)(value / (1.0f / _nTiers));

	
	// NEIGHBOUR EVALUATION (untested)
	// TODO: quitar todos los parámetros offset y usar el miembro.
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
	public Vector2I GetParameterWorldSize() => _worldSize;
	public void SetParameterWorldSize(Vector2I value) => _worldSize = value;
    
	public int GetParameterNTiers() => _nTiers;
	public void SetParameterNTiers(int value) => _nTiers = value;

	public Vector2I GetParameterChunkSize() => _chunkSize;		// guardamos esto para calcular averages y demás.
	public void SetParameterChunkSize(Vector2I value) => _chunkSize = value;
	
	public Vector2I GetParameterOffset() => _offset;
	public void SetParameterOffset(Vector2I offset) => _offset = offset;

}