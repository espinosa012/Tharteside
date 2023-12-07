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

    public override float GenerateValueAt(int _x, int y) => 
        Math.Abs((y - _offset.Y) - _worldSize.Y / 2.0f) / (_worldSize.Y / 2.0f);

    private int DegreesToLatitude(float degrees) => 
        (int)((_worldSize.Y / 2.0f) * (1.0f - Math.Sin(degrees * Math.PI / 180.0f))); // untested

    public float GetNormalizedDistanceToEquator(int y) => (Math.Abs(y - _equatorLine) / _equatorLine) / _worldSize.Y;
    
    
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