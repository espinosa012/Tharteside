using Godot;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

public partial class HeightMap : WorldGenerator
{

    private string _imageFilename;
    private Image<Rgba32> _heightMap;

    public HeightMap()
    {
        
    }

    public override float GetValueAt(int x, int y)
    {
        if (x >= _heightMap.Width || y >= _heightMap.Height || x < 0 || y < 0)
            return 0.0f;
        
        Rgba32 pixelColor = _heightMap[x, y];
        float value = NormalizePixelValue(pixelColor.R);
        return value;
    }
    
    public void LoadHeighMap(string filename)
    {
        // resources/heightmap
        SetParameterImageFilename(filename);
        string heightMapFolder = "res://resources/heightmap/";  // parametrizar
        string path = ProjectSettings.GlobalizePath(heightMapFolder + _imageFilename);
        
        _heightMap = Image.Load<Rgba32>(path);    // cuidado con el formato
    }

    public float NormalizePixelValue(int pixelValue) => pixelValue / 255.0f;

    public void ResizeHeightMap(Vector2I newSize)
    {
        _heightMap.Mutate(x => x.Resize(new Size(newSize.X, newSize.Y)));
    }

    
    
    
    // getters & setters (uno por cada parámetro que queremos hacer accesibles)

    public string GetParameterImagePath() => _imageFilename;
    public void SetParameterImageFilename(string newPath) => _imageFilename = newPath;
    
    
    
}
