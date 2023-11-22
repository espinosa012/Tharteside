using System;

namespace Tartheside.mono.world.generators;

public partial class Temperature : WorldGenerator
{
    private Elevation _elevation;
    private Latitude _latitude;
    
    
    public Temperature(int matrixSizeX, int matrixSizeY) : base(matrixSizeX, matrixSizeY)
    {}
    
    public override float GenerateValueAt(int x, int y)
    {
        // TODO: utilizar valores de elevación, regiones de latitud, humedad, etc. (¿¿temperatura dinámica -> generadores dinámicos??)
        return GetNormalizedDistanceToEquator(y);
    }

    private float GetNormalizedDistanceToEquator(int y) => (float) Math.Abs(y - _latitude.GetParameterEquatorLine()) 
                                                         / _latitude.GetParameterEquatorLine();

    // getters & setters
    public Latitude GetParameterLatitude() => _latitude;
    public void SetParameterLatitude(Latitude latitude) => _latitude = latitude;
    
    public Elevation GetParameterElevation() => _elevation;
    public Elevation SetParameterElevation(Elevation elevation) => _elevation = elevation;

}