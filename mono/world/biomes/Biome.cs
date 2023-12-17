using Tartheside.mono.world.generators;

namespace Tartheside.mono.world.biomes;

public static class Biome
{
    public static bool IsSea(Elevation elevation, int x, int y) => elevation.GetValueTierAt(x, y) == 0;

    
}