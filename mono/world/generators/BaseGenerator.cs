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
	public void FillValueMatrix()
	{
		// TODO: necesitamos poder indicar qué region de la matriz queremos generar, por eficiencia.
		for (var i = Offset.X; i < WorldSize.X + Offset.X; i++)
		for (var j = Offset.Y; j < WorldSize.Y + Offset.Y; j++)
			valueMatrix[i - Offset.X, j - Offset.Y] = GenerateValueAt(i, j);
	}

	public void ReloadValueMatrix()
	{
		valueMatrix.Clear();
		FillValueMatrix();
	}
	
	public void ThresholdValueMatrix(float min)
	{
		for (var i = Offset.X; i < WorldSize.X + Offset.X; i++)
		for (var j = Offset.Y; j < WorldSize.Y + Offset.Y; j++)
			valueMatrix[i - Offset.X, j - Offset.Y] = (valueMatrix[i - Offset.X, j - Offset.Y] < min) ? 
				0.0f : valueMatrix[i - Offset.X, j - Offset.Y];
	}
	
	
	// Values
	protected void SetValueAt(int x, int y, float value) => valueMatrix[x-Offset.X, y - Offset.Y] = value;	//TODO: cuidado con el offset (¿no es mejor pasárselo como a FillValueMatrix()?
	public float GetValueAt(int x, int y) => valueMatrix[x, y];
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

	
	
	public virtual void Randomize() {}
}