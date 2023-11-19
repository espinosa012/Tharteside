using System;

namespace Tartheside.mono.world.generators;

public partial class Latitude : WorldGenerator
{
    private int _equatorLine;
    
    
    public Latitude(int matrixSizeX, int matrixSizeY) : base(matrixSizeX, matrixSizeY)
    {}
    
    public override float GenerateValueAt(int x, int y) => 
        GetNormalizedSignedDistanceToEquator(y);
    
    public float GetValueDegrees(int y) =>
        90.0f * GetNormalizedSignedDistanceToEquator(y);
    
    private float GetNormalizedSignedDistanceToEquator(int y) => 
        (float) (y - _equatorLine) / _equatorLine;
    
    private float GetNormalizedDistanceToEquator(int y) => 
        (float) Math.Abs(y - _equatorLine) / _equatorLine;


    // TODO: regiones de latitud (parametrizar los intervalos)
    public bool IsLatitudRegionEquator(int y)
    {
        return GetNormalizedDistanceToEquator(y) < 0.1f;
    }
    
    public bool IsLatitudRegionTropical(int y)
    {
        return GetNormalizedDistanceToEquator(y) is >= 0.1f and < 0.5f;
    }
    
    public bool IsLatitudRegionSubtropical(int y)
    {
        return GetNormalizedDistanceToEquator(y) is >= 0.1f and < 0.5f;
    }
    
    public bool IsLatitudRegionTemperate(int y)
    {
        return GetNormalizedDistanceToEquator(y) is >= 0.5f and < 0.8f;
    }
    
    public bool IsLatitudRegionPolar(int y)
    {
        return GetNormalizedDistanceToEquator(y) >= 0.8f;
    }
    
    // getters & setters
    public int GetParameterEquatorLine() => _equatorLine;
    public void SetParameterEquatorLine(int value) => _equatorLine = value;
    
}