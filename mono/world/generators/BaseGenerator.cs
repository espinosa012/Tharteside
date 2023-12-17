using Godot;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace Tartheside.mono.world.generators;

public partial class BaseGenerator : GodotObject
{
	protected Vector2I WorldSize;
	private Vector2I ChunkSize;    
	protected Vector2I Offset;
	private int NTiers;
	private Matrix<float> valueMatrix;	
	
	private const float InitValue = -1.0f;

	protected BaseGenerator(Vector2I worldSize, Vector2I chunkSize, Vector2I offset, int nTiers)
	{
		GetClearMatrix(worldSize);
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
	protected void GetClearMatrix(Vector2I matrixSize) => valueMatrix = DenseMatrix.Build.Dense(matrixSize.X, 
		matrixSize.Y, InitValue);
	
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

	
	// Thresholding
	public void ThresholdValueMatrixByValue(float minValue)
	{
		for (var i = Offset.X; i < WorldSize.X + Offset.X; i++)
		for (var j = Offset.Y; j < WorldSize.Y + Offset.Y; j++)
			valueMatrix[i - Offset.X, j - Offset.Y] = (valueMatrix[i - Offset.X, j - Offset.Y] < minValue) ? 
				0.0f : valueMatrix[i - Offset.X, j - Offset.Y];
	}	// TODO: ¿hace falta o es suficiente con el de tier?

	public void ThresholdValueMatrixByTier(int minTier)
	{
		for (var i = Offset.X; i < WorldSize.X + Offset.X; i++)
		for (var j = Offset.Y; j < WorldSize.Y + Offset.Y; j++)
			valueMatrix[i - Offset.X, j - Offset.Y] = (GetValueTier(valueMatrix[i - Offset.X, j - Offset.Y]) <= minTier) ? 
				0.0f : valueMatrix[i - Offset.X, j - Offset.Y];
	}

	public void InverseThresholdingByTier(int maxTier)
	{
		for (var i = Offset.X; i < WorldSize.X + Offset.X; i++)
		for (var j = Offset.Y; j < WorldSize.Y + Offset.Y; j++)
			valueMatrix[i - Offset.X, j - Offset.Y] = (GetValueTier(valueMatrix[i - Offset.X, j - Offset.Y]) > maxTier) ? 
				-1.0f : valueMatrix[i - Offset.X, j - Offset.Y];
	}
	
	// Values
	protected void SetValueAt(int x, int y, float value) => valueMatrix[x-Offset.X, y - Offset.Y] = value;
	private float GetValueAt(int x, int y) => valueMatrix[x, y];
	public virtual float GenerateValueAt(int x, int y) => 0.0f;
	
	
	// Tiers
	private int GetValueTierAt(Vector2I pos) => GetValueTierAt(pos.X, pos.Y);
	public int GetValueTierAt(int x, int y) => GetValueTier(GetValueAt(x, y));
	private int GetValueTier(float value) => (int)(value / (1.0f / NTiers));

	
	// TODO: NEIGHBOUR EVALUATION 
	
	
	// TODO: Averaging
	
	
	public void SetParameterWorldSize(Vector2I value) => WorldSize = value;
	public void SetParameterNTiers(int value) => NTiers = value;
	public void SetParameterChunkSize(Vector2I value) => ChunkSize = value;
	public void SetParameterOffset(Vector2I offset) => Offset = offset;

	
	
	public virtual void Randomize() {}
	
	
}