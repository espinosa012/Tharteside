using System;
using Godot;
using MathNet.Numerics;

namespace Tartheside.mono.world.generators;

public partial class Latitude : WorldGenerator
{
    // TODO: expresarlos en grados para hacerlo robusto frente a cambios en el tamaño del mundo.
    public int _equatorLine;
    private int _cancerTropicLine;
    private int _capricornTropicLine;
    private int _northSubtropicLine;
    private int _southSubtropicLine;
    private int _arcticCircleLine;
    private int _antarcticCircleLine;

    public Latitude(int matrixSizeX, int matrixSizeY) : base(matrixSizeX, matrixSizeY)
    {}
    
    // testing
    public override float GenerateValueAt(int _x, int y) => GetNormalizedDistanceToEquator(y);

    public int LatitudeDegreesToY(float degrees)
    {
        // TODO: asegurare de que los grados están en el rango -90,90
        return (int) Math.Round(_worldSize.Y * (90f - degrees) / 180f);  
    } 
    
    public float YToLatitudeDegrees(int y) => 90f - 180f * y / _worldSize.Y;
    
    public float GetNormalizedDistanceToEquator(int y) => Math.Min(0.999999f, Math.Abs((y - _offset.Y) -
        (float)_equatorLine) / (_worldSize.Y / 2.0f));
    
    // TODO: regiones de latitud (parametrizar los intervalos)
    // For biome determination
    public bool IsLatitudRegionEquator(int y) => GetNormalizedDistanceToEquator(y) < 0.1f;
    public bool IsLatitudRegionTropical(int y) => GetNormalizedDistanceToEquator(y) is >= 0.1f and < 0.5f;
    public bool IsLatitudRegionSubtropical(int y) => GetNormalizedDistanceToEquator(y) is >= 0.1f and < 0.5f;
    public bool IsLatitudRegionTemperate(int y) => GetNormalizedDistanceToEquator(y) is >= 0.5f and < 0.8f;
    public bool IsLatitudRegionPolar(int y) => GetNormalizedDistanceToEquator(y) >= 0.8f;
    
    
    // getters & setters
    public void SetParameterEquatorLine(int value) => _equatorLine = value;
    public void SetParameterCancerTropicLine(int value) => _cancerTropicLine = value;
    public void SetParameterCapricornTropicLine(int value) => _capricornTropicLine = value;
    public void SetParameterNorthSubtropicLine(int value) => _northSubtropicLine = value;
    public void SetParameterSouthSubtropicLine(int value) => _southSubtropicLine = value;
    public void SetParameterArcticCircleLine(int value) => _arcticCircleLine = value;
    public void SetParameterAntarcticCircleLine(int value) => _antarcticCircleLine = value;
    
}