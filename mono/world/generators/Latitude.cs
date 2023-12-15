using System;
using Godot;

namespace Tartheside.mono.world.generators;

public partial class Latitude : BaseGenerator
{
    // TODO: expresarlos en grados para hacerlo robusto frente a cambios en el tamaño del mundo.
    private int _equatorLine;
    private int _cancerTropicLine;
    private int _capricornTropicLine;
    private int _northSubtropicLine;
    private int _southSubtropicLine;
    private int _arcticCircleLine;
    private int _antarcticCircleLine;

    public Latitude(Vector2I worldSize, Vector2I chunkSize, Vector2I offset, int nTiers) 
        : base(worldSize, chunkSize, offset, nTiers)
    {}
    
    public override float GenerateValueAt(int _x, int y) => GetNormalizedDistanceToEquator(y - Offset.Y);
    
    public int LatitudeDegreesToY(float degrees) => (int) Math.Round(WorldSize.Y * (90f - degrees) / 180f);

    // TODO: asegurare de que los grados están en el rango -90,90
    private float YToLatitudeDegrees(int y) => 90f - 180f * y / WorldSize.Y;
    
    public float GetNormalizedDistanceToEquator(int y) => Math.Min(0.999999f, Math.Abs(y - 
        LatitudeDegreesToY(_equatorLine)) / (WorldSize.Y / 2.0f));
    
    
    // BIOME DETERMINATION
    public bool IsLatitudeRegionTropicalCancer(int y) => YToLatitudeDegrees(y) > 0 
                                                         && YToLatitudeDegrees(y) <= _cancerTropicLine;
    public bool IsLatitudeRegionTropicalCapricorn(int y) => YToLatitudeDegrees(y) < 0 
                                                            && YToLatitudeDegrees(y) >= _capricornTropicLine;
    public bool IsLatitudeRegionSubtropicalNorth(int y) => YToLatitudeDegrees(y) > _cancerTropicLine 
                                                           && YToLatitudeDegrees(y) <= _northSubtropicLine;
    public bool IsLatitudeRegionSubtropicalSouth(int y) => YToLatitudeDegrees(y) < _capricornTropicLine 
                                                           && YToLatitudeDegrees(y) >= _southSubtropicLine;
    public bool IsLatitudeRegionTemperateNorth(int y) =>  YToLatitudeDegrees(y) > _northSubtropicLine 
                                                          && YToLatitudeDegrees(y) <= _arcticCircleLine;
    public bool IsLatitudeRegionTemperateSouth(int y) => YToLatitudeDegrees(y) < _southSubtropicLine
                                                         && YToLatitudeDegrees(y) >= _antarcticCircleLine;
    public bool IsLatitudeRegionArctic(int y) => YToLatitudeDegrees(y) >= _arcticCircleLine; 
    public bool IsLatitudeRegionAntarctic(int y) => YToLatitudeDegrees(y) <= _antarcticCircleLine; 
    
    public bool IsLatitudeRegionTropical(int y) => IsLatitudeRegionTropicalCancer(y) 
                                                   || IsLatitudeRegionTropicalCapricorn(y); 
    public bool IsLatitudeRegionSubtropical(int y) => IsLatitudeRegionSubtropicalNorth(y) 
                                                      || IsLatitudeRegionSubtropicalSouth(y);
    public bool IsLatitudeRegionTemperate(int y) => IsLatitudeRegionTemperateNorth(y) 
                                                    || IsLatitudeRegionTemperateSouth(y);
    public bool IsLatitudeRegionPolar(int y) => IsLatitudeRegionArctic(y) 
                                                || IsLatitudeRegionAntarctic(y);

    
    // getters & setters
    public void SetParameterEquatorLine(int value) => _equatorLine = value;
    public void SetParameterCancerTropicLine(int value) => _cancerTropicLine = value;
    public void SetParameterCapricornTropicLine(int value) => _capricornTropicLine = value;
    public void SetParameterNorthSubtropicLine(int value) => _northSubtropicLine = value;
    public void SetParameterSouthSubtropicLine(int value) => _southSubtropicLine = value;
    public void SetParameterArcticCircleLine(int value) => _arcticCircleLine = value;
    public void SetParameterAntarcticCircleLine(int value) => _antarcticCircleLine = value;
    
}