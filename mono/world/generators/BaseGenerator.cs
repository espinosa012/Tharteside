using System.Linq;
using Godot;
using MathNet.Numerics.LinearAlgebra;
using Tartheside.mono.utilities.math;

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
		SetClearMatrix(worldSize);
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
	protected void SetClearMatrix(Vector2I matrixSize) => 
		valueMatrix = MathDotNetHelper.GetMatrix(matrixSize, InitValue);
	
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
	public void ThresholdValueMatrixByTier(int minTier)
	{
		for (var i = Offset.X; i < WorldSize.X + Offset.X; i++)
		for (var j = Offset.Y; j < WorldSize.Y + Offset.Y; j++)
			valueMatrix[i - Offset.X, j - Offset.Y] = (GetValueTier(valueMatrix[i - Offset.X, j - Offset.Y]) <= minTier) 
				? 0.0f : valueMatrix[i - Offset.X, j - Offset.Y];
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
	public float GetValueAt(int x, int y) => valueMatrix[x, y];
	public virtual float GenerateValueAt(int x, int y) => 0.0f;
	
	
	// Tiers
	private int GetValueTierAt(Vector2I pos) => GetValueTierAt(pos.X, pos.Y);
	public int GetValueTierAt(int x, int y) => GetValueTier(GetValueAt(x, y));
	private int GetValueTier(float value) => (int)(value / (1.0f / NTiers));

	
	// TODO: NEIGHBOUR EVALUATION 
	
	
	
	// TODO: Averaging
	public float GetSubMatrixAverage(Vector2I centroid, Vector2I size) // size debería ser impar
	{	
		var subMatrix = valueMatrix.SubMatrix(centroid.X - size.X / 2,
			size.X, centroid.Y - size.Y / 2, size.Y);
		return Enumerable.Sum(subMatrix.ColumnSums()) / (size.Y * size.X);
	}
	
	// setters
	public void SetParameterWorldSize(Vector2I value) => WorldSize = value;
	public void SetParameterNTiers(int value) => NTiers = value;
	public void SetParameterChunkSize(Vector2I value) => ChunkSize = value;
	public void SetParameterOffset(Vector2I offset) => Offset = offset;
	
	public virtual void Randomize() {}
	
	
}