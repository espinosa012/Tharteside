using Godot;

namespace Tartheside.mono.world.generators;

public partial class Temperature : BaseGenerator
{
    private Elevation _elevation;
    private Latitude _latitude;
    
    public Temperature(Vector2I worldSize, Vector2I chunkSize, Vector2I offset, int nTiers) 
        : base(worldSize, chunkSize, offset, nTiers)
    {}
    
    public override float GenerateValueAt(int x, int y)
    {
        // TODO: utilizar valores de elevación, regiones de latitud, humedad, etc. (¿¿temperatura dinámica -> generadores dinámicos?? en función del tiempo) 
        return _latitude.GetNormalizedDistanceToEquator(y);
    }

    // getters & setters
    public void SetParameterLatitude(Latitude latitude) => _latitude = latitude;
    public Elevation SetParameterElevation(Elevation elevation) => _elevation = elevation;

}