using Godot;
using Tartheside.mono.utilities.random;

namespace Tartheside.mono.world.generators;

public partial class NoiseGenerator : BaseGenerator
{
    private MFNL _noise;

    public NoiseGenerator(Vector2I worldSize, Vector2I chunkSize, Vector2I offset, int nTiers) 
        : base(worldSize, chunkSize, offset, nTiers)
    {}
    
    // TODO: podríamos de alguna manera indicar qué método de MFNL queremos usar para generar valores
    // (siempre normalizados)
    
    public override float GenerateValueAt(int x, int y) => _noise.GetNormalizedNoise2D(x, y);
    
    public void SetParameterNoise(MFNL noise) => _noise = noise;
}