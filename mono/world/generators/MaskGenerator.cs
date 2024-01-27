using Godot;
using Tartheside.mono.utilities.logger;

namespace Tartheside.mono.world.generators;

public partial class MaskGenerator : BaseGenerator
{
    private BaseGenerator _parentGenerator;
    private string _maskMathod;
    
    private float _trueValue = 0.99f;
    private float _falseValue = -1.0f;
    
    public MaskGenerator(Vector2I worldSize, Vector2I chunkSize, Vector2I offset, int nTiers) : 
        base(worldSize, chunkSize, offset, nTiers)
    {}

    /*public override float GenerateValueAt(int x, int y) => 
        (bool) _parentGenerator.Call(_maskMathod, new []{x, y}) ? _trueValue : _falseValue;  
    public override float GenerateValueAt(int x, int y) => 
        ((Elevation) _parentGenerator).IsVolcanicLand(x, y) ? _trueValue : _falseValue;*/

    public override float GenerateValueAt(int x, int y)
    {
        switch (_maskMathod)
        {
            case "IsLand":
                return ((Elevation) _parentGenerator).IsLand(x, y) ? _parentGenerator.GenerateValueAt(x, y) : _falseValue;
            case "IsContinentalLand":
                return ((Elevation) _parentGenerator).IsContinentalLand(x, y) ? _parentGenerator.GenerateValueAt(x, y) : _falseValue;
            case "IsVolcanicLand":
                return ((Elevation) _parentGenerator).IsVolcanicLand(x, y) ? _parentGenerator.GenerateValueAt(x, y) : _falseValue;
            case "IsVolcanicIsland":
                return ((Elevation) _parentGenerator).IsVolcanicIsland(x, y) ? _parentGenerator.GenerateValueAt(x, y) : _falseValue;
            case "IsOutToSea":
                return ((Elevation) _parentGenerator).IsOutToSea(x, y) ? _parentGenerator.GenerateValueAt(x, y) : _falseValue;
        }
        TLogger.Info("Not valid mask method name: " + _maskMathod);
        return _falseValue;
    }
    

    
    public void SetParentGenerator(BaseGenerator generator) => _parentGenerator = generator;

    public void SetMaskMethod(string method) => _maskMathod = method;


}