using Godot;
using System;

public partial class Biome : WorldGenerator     // ¿debe ser realmente heredero de WorldGenerator?
{
    private Elevation _elevation;
    private Temperature _temperature;
    private MFNL _peaksAndValleys;
    private float _minimumPeaksAndValleysMineralSpawnValue;
    
    
    public int GetValueAt(int x, int y) // devolvemos una id que represente el bioma
    {
        return 0;
    }
    
    public bool IsTerrainSea(int x, int y)
    {
        // podríamos desomponer el tier 0 para distinguir varios niveles de profundidad de mar, obtener bioma orilla (mareas), etc
        return _elevation.GetValueTierAt(x, y) == 0;
    }
    
    public bool IsTerrainBeach(int x, int y)
    {
        return _elevation.GetValueTierAt(x, y) == 1;
    }

    public bool IsTerrainLowLand(int x, int y)
    {
        int tier = _elevation.GetValueTierAt(x, y);
        return tier is 2 or 3;
    }
    public bool IsTerrainMediumLand(int x, int y)
    {
        int tier = _elevation.GetValueTierAt(x, y);
        return tier is 4 or 5;
    }
    
    public bool IsTerrainMineral(int x, int y)
    {
        int minimumMountainRockElevationTier = 3; 	// convertir en parámetro del mundo
        bool isAboveMinimunElevation = _elevation.GetValueTierAt(x, y) >= minimumMountainRockElevationTier;
        bool isAboveMinimumPeaksAndValleys = _peaksAndValleys.GetNormalizedNoise2D(x, y) > _minimumPeaksAndValleysMineralSpawnValue;	// convertir en parámetro del mundo
        bool isSlopeRight = (_elevation.IsStepDownAtOffset(x, y, 1, 0) 
                             && _elevation.IsStepDownAtOffset(x, y, 2, 0));
        bool isSlopeLeft = (_elevation.IsStepDownAtOffset(x, y, -1, 0) 
                            && _elevation.IsStepDownAtOffset(x, y, -2, 0));
        bool isSlopeUp = (_elevation.IsStepDownAtOffset(x, y, 0, -1) 
                          && _elevation.IsStepDownAtOffset(x, y, 0, -2));
        bool isSlopeDown = (_elevation.IsStepDownAtOffset(x, y, 0, 1) 
                            && _elevation.IsStepDownAtOffset(x, y, 0, 2));	// con la pendiente que se exija podemos regular la densidad
        // comprobar pendientes creo que no detecta bien los cabmios bruscos, aunque queda bien en el mapa así...

        // se le podría aplicar después un filtrado on un objeto de ruido muy granulado,que ayude a simular las vetas de mineral
        // ¿qué pasa cuando coincide con roca u otra cosa?
        // tenemos que poder regular la densidad de mineral (y de roca y de lo que sea), quizá con un parámetro de persistencia del ruido
        return isAboveMinimunElevation && isAboveMinimumPeaksAndValleys && ((isSlopeRight || isSlopeLeft) && (isSlopeDown || isSlopeUp));
    }
    
    
    // getters & setters
    public Elevation GetParameterElevation() => _elevation;
    public void SetParameterElevation(Elevation value) => _elevation = value;

    public Temperature GetParameterTemperature() => _temperature;
    public void SetParameterTemperature(Temperature value) => _temperature = value;

    public MFNL GetParameterPeaksAndValleys() => _peaksAndValleys;
    public void SetParameterPeaksAndValleys(MFNL value) => _peaksAndValleys = value;

    public float GetParameterMinimumPeaksAndValleysMineralSpawnValue() => _minimumPeaksAndValleysMineralSpawnValue;

    public void SetParameterMinimumPeaksAndValleysMineralSpawnValue(float value) =>
        _minimumPeaksAndValleysMineralSpawnValue = value;

}
