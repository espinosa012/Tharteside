using Godot;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace Tartheside.mono.world.generators;

public partial class BaseGenerator : GodotObject
{
	protected Vector2I WorldSize;
	protected Vector2I ChunkSize;    
	protected Vector2I Offset;
	protected int NTiers;
	protected Matrix<float> valueMatrix;	// TODO: poner readonly??? qué es eso??

	private const float InitValue = -1.0f;

	protected BaseGenerator(Vector2I worldSize, Vector2I chunkSize, Vector2I offset, int nTiers)
	{
		valueMatrix = DenseMatrix.Build.Dense(worldSize.X, worldSize.Y, InitValue);
		Setup(worldSize, chunkSize, offset, nTiers);
	}

	private void Setup(Vector2I worldSize, Vector2I chunkSize, Vector2I offset, int nTiers)
	{
		SetParameterWorldSize(worldSize);
		SetParameterOffset(chunkSize);
		SetParameterOffset(offset); 
		SetParameterNTiers(nTiers);
	}
	
	
	// Value matrix
	public void FillValueMatrix(int offsetX, int offsetY)
	{
		// TODO: necesitamos poder indicar qué region de la matriz queremos generar, por eficiencia.
		for (var i = offsetX; i < WorldSize.X + offsetX; i++)
		for (var j = offsetY; j < WorldSize.Y + offsetY; j++)
			valueMatrix[i - offsetX, j - offsetY] = GenerateValueAt(i, j);
	}

	public void ReloadValueMatrix(int offsetX, int offsetY)
	{
		valueMatrix.Clear();
		FillValueMatrix(offsetX, offsetY);
	}
	
	public void ThresholdValueMatrix(int offsetX, int offsetY, float min, float max = 1.0f)
	{
		for (var i = offsetX; i < WorldSize.X + offsetX; i++)
		for (var j = offsetY; j < WorldSize.Y + offsetY; j++)
			valueMatrix[i - offsetX, j - offsetY] = (valueMatrix[i - offsetX, j - offsetY] < min) ? 0.0f : valueMatrix[i - offsetX, j - offsetY];
	}
	
	
	// Values
	protected void SetValueAt(int x, int y, float value) => valueMatrix[x-Offset.X, y - Offset.Y] = value;	//TODO: cuidado con el offset (¿no es mejor pasárselo como a FillValueMatrix()?
	private float GetValueAt(int x, int y) => valueMatrix[x, y];
	public virtual float GenerateValueAt(int x, int y) => 0.0f;
	
	
	// Tiers
	private int GetValueTierAt(Vector2I pos) => GetValueTierAt(pos.X, pos.Y);
	public int GetValueTierAt(int x, int y) => GetValueTier(GetValueAt(x, y));
	private int GetValueTier(float value) => (int)(value / (1.0f / NTiers));

	
	// TODO: NEIGHBOUR EVALUATION 
	
	
	// TODO: crear métodos genéricos para obtener parámetros de cualquier generador (get y set)
	public void SetParameterWorldSize(Vector2I value) => WorldSize = value;
	public void SetParameterNTiers(int value) => NTiers = value;
	public void SetParameterChunkSize(Vector2I value) => ChunkSize = value;
	public void SetParameterOffset(Vector2I offset) => Offset = offset;

}