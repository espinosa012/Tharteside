using System.Collections.Generic;
using System.Linq;
using Godot;
using MathNet.Numerics.LinearAlgebra;
using Tartheside.mono.utilities.math;
using Tartheside.mono.world.entities;

namespace Tartheside.mono.world.generators;

public partial class BaseGenerator : GodotObject
{
	protected int Seed;	// TODO: para persistencia. Usar en Randomize() 
	
	protected Vector2I WorldSize;
	private Vector2I ChunkSize;    
	protected Vector2I Offset;
	private int NTiers;
	protected Matrix<float> valueMatrix;	
	
	private const float InitValue = -1.0f;

	public BaseGenerator(Vector2I worldSize, Vector2I chunkSize, Vector2I offset, int nTiers)
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
		// TODO:¿si empezamos el bucle en 0?
		for (var i = Offset.X; i < WorldSize.X + Offset.X; i++)
		for (var j = Offset.Y; j < WorldSize.Y + Offset.Y; j++)
			valueMatrix[i - Offset.X, j - Offset.Y] = GenerateValueAt(i, j);
	}

	public void SetValueMatrix(Matrix<float> newMatrix) => valueMatrix = newMatrix;

	public Matrix<float> GetValueMatrix() => valueMatrix.Clone();
	
	public void ReloadValueMatrix()
	{
		valueMatrix.Clear();
		FillValueMatrix();
	}

	
	// Thresholding
	public void ThresholdByTier(int minTier)
	{
		for (var i = Offset.X; i < WorldSize.X + Offset.X; i++)
		for (var j = Offset.Y; j < WorldSize.Y + Offset.Y; j++)
			valueMatrix[i - Offset.X, j - Offset.Y] = (GetValueTier(valueMatrix[i - Offset.X, j - Offset.Y]) <= minTier) 
				? 0.0f : valueMatrix[i - Offset.X, j - Offset.Y];
	}

	public void InverseThresholdByTier(int maxTier)
	{
		for (var i = Offset.X; i < WorldSize.X + Offset.X; i++)
		for (var j = Offset.Y; j < WorldSize.Y + Offset.Y; j++)
			valueMatrix[i - Offset.X, j - Offset.Y] = (GetValueTier(valueMatrix[i - Offset.X, j - Offset.Y]) > maxTier) ? 
				-1.0f : valueMatrix[i - Offset.X, j - Offset.Y];
	}

	public void RangeThresholdByTier(int minTier, int maxTier)
	{
		ThresholdByTier(minTier);
		InverseThresholdByTier(maxTier);
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
	
	
	// ConnectedRegionEntity detection
	public List<ConnectedRegionEntity> GetConnectedRegions(int minTier, int maxTier, int minimumRegionSize = 25)	// TODO: de momento, nos basamos en tiers mínimo y máximo. Puede ser cualquier condition(x, y), incluso una función lambda que pasemos como parámetro.
	{
		// TODO: tiene el problema de que necesita que el generador esté umbralizado para no petar.
		var toReturn = new List<ConnectedRegionEntity>();
		
		var visited = new int[valueMatrix.RowCount, valueMatrix.ColumnCount];

		for (var i = 0; i < valueMatrix.RowCount; i++)
		{
			for (var j = 0; j < valueMatrix.ColumnCount; j++)
			{
				var ijValue = GetValueTierAt(i, j);
				var condition = ijValue >= minTier && ijValue <= maxTier;
				if (!condition || visited[i, j] != 0) continue;
				
				var region = new ConnectedRegionEntity();
				var islandCoordinates = new List<Vector2I>();
				var islandSize = ConnectRegionDFS(visited, i, j, islandCoordinates);

				if (islandSize <= minimumRegionSize) continue;
				
				region.SetPositions(islandCoordinates);
				region.SetIslandSize(islandSize);
				toReturn.Add(region);
			}			
		}

		return toReturn;
	}
	
	private int ConnectRegionDFS(int[,] visited, int row, int col, 
		List<Vector2I> islandCoordinates)
	{
		if (row < 0 || row >= valueMatrix.RowCount || col < 0 || col >= valueMatrix.ColumnCount 
		    || valueMatrix[row, col] == 0 || visited[row, col] == 1) return 0;	
		
		visited[row, col] = 1;
		var size = 1;
		
		islandCoordinates.Add(new Vector2I(row+Offset.X, col+Offset.Y));
		
		size += ConnectRegionDFS(visited, row - 1, col, islandCoordinates); 
		size += ConnectRegionDFS(visited, row + 1, col, islandCoordinates); 
		size += ConnectRegionDFS(visited, row, col - 1, islandCoordinates); 
		size += ConnectRegionDFS(visited, row, col + 1, islandCoordinates); 
		// no se computan las diagonales
		
		return size;
	}
	
	
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
	
	// TODO: random
	public virtual void Randomize(int seed) {}
	
	
}