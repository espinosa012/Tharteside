using Godot;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace Tartheside.mono.world.generators;

public partial class WorldGenerator : GodotObject
{
	protected Vector2I _worldSize;    
	protected Vector2I _chunkSize;    
	protected Vector2I _offset;    
	protected int _nTiers;
	private readonly Matrix<float> _valueMatrix;

	private const float InitValue = -1.0f;

	protected WorldGenerator(int matrixSizeX, int matrixSizeY)
	{
		_valueMatrix = DenseMatrix.Build.Dense(matrixSizeX, matrixSizeY, InitValue);
	}

	public void FillValueMatrix(int offsetX, int offsetY)
	{
		// TODO: necesitamos poder indicar qué region de la matriz queremos generar, por eficiencia.
		for (int i = offsetX; i < _worldSize.X + offsetX; i++)
		for (int j = offsetY; j < _worldSize.Y + offsetY; j++)
			_valueMatrix[i-offsetX, j-offsetY] = GenerateValueAt(i, j);
	}

	protected void SetValueAt(int x, int y, float value) => _valueMatrix[x-_offset.X, y - _offset.Y] = value;

	private float GetValueAt(int x, int y) => _valueMatrix[x, y];
	
	public virtual float GenerateValueAt(int x, int y) => 0.0f;

	private int GetValueTierAt(Vector2I pos) => GetValueTierAt(pos.X, pos.Y);
    
	public int GetValueTierAt(int x, int y) => GetValueTier(GetValueAt(x, y));

	private int GetValueTier(float value) => (int)(value / (1.0f / _nTiers));

	
	// TODO: NEIGHBOUR EVALUATION 
	
	
	// TODO: crear métodos genéricos para obtener parámetros de cualquier generador (get y set)
	public Vector2I GetParameterWorldSize() => _worldSize;
	public void SetParameterWorldSize(Vector2I value) => _worldSize = value;
    
	public int GetParameterNTiers() => _nTiers;
	public void SetParameterNTiers(int value) => _nTiers = value;

	public Vector2I GetParameterChunkSize() => _chunkSize;		// guardamos esto para calcular averages y demás.
	public void SetParameterChunkSize(Vector2I value) => _chunkSize = value;
	
	public Vector2I GetParameterOffset() => _offset;
	public void SetParameterOffset(Vector2I offset) => _offset = offset;

}