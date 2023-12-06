using System;

namespace Tartheside.mono.world.generators;

public partial class Latitude : WorldGenerator
{
    // TODO: expresarlos en grados para hacerlo robusto frente a cambios en el tamaño del mundo.
    private int _equatorLine;
    private int _cancerTropicLine;
    private int _capricornTropicLine;
    private int _northSubtropicLine;
    private int _southSubtropicLine;
    private int _arcticCircleLine;
    private int _antarcticCircleLine;
       
    public Latitude(int matrixSizeX, int matrixSizeY) : base(matrixSizeX, matrixSizeY)
    {}
    
    public override float GenerateValueAt(int x, int y) => 
        GetNormalizedSignedDistanceToEquator(y);

    private float GetValueDegrees(int y) =>
        90.0f * GetNormalizedSignedDistanceToEquator(y);
    
    private float GetNormalizedSignedDistanceToEquator(int y) => 
        (float) (y - _equatorLine) / _equatorLine;
    
    public float GetNormalizedDistanceToEquator(int y) => 
        (float) Math.Abs(y - _equatorLine) / _equatorLine;


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