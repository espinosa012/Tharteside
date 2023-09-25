using Godot;
using System;
using System.Collections.Generic;

public partial class Latitude : WorldGenerator
{
    private int _equatorLine;
    
    
    
    public override float GetValueAt(int x, int y) => 
        GetNormalizedSignedDistanceToEquator(y);
    
    public float GetValueDegrees(int y) =>
        90.0f * GetNormalizedSignedDistanceToEquator(y);
    
    private float GetNormalizedSignedDistanceToEquator(int y) => 
        (float) (y - _equatorLine) / _equatorLine;
    
    private float GetNormalizedDistanceToEquator(int y) => 
        (float) Math.Abs(y - _equatorLine) / _equatorLine;


    // regiones de latitud (parametrizar los intervalos)
    public bool IsLatitudRegionEquator(int y)
    {
        return GetNormalizedDistanceToEquator(y) < 0.1f;
    }
    
    public bool IsLatitudRegionTropical(int y)
    {
        float eqDist = GetNormalizedDistanceToEquator(y);
        return eqDist >= 0.1f && eqDist < 0.5f;
    }
    
    public bool IsLatitudRegionSubtropical(int y)
    {
        float eqDist = GetNormalizedDistanceToEquator(y);
        return eqDist >= 0.1f && eqDist < 0.5f;
    }
    
    public bool IsLatitudRegionTemperate(int y)
    {
        float eqDist = GetNormalizedDistanceToEquator(y);
        return eqDist >= 0.5f && eqDist < 0.8f;
    }
    
    public bool IsLatitudRegionPolar(int y)
    {
        return GetNormalizedDistanceToEquator(y) >= 0.8f;
    }
    
    // getters & setters
    public int GetParameterEquatorLine() => _equatorLine;
    public void SetParameterEquatorLine(int value) => _equatorLine = value;
    
}
