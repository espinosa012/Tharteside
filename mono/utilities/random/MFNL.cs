using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Godot;

namespace Tartheside.mono.utilities.random;

public partial class MFNL : FastNoiseLite
{
    private string _name;
    private RandomNumberGenerator _rng;
    private int _nTiers;

    private string[] _noiseProperties = {
        "CellularDistanceFunction", "CellularJitter", "CellularReturnType",
        
        "DomainWarpEnabled", 
        "DomainWarpType", "DomainWarpAmplitude", "DomainWarpFrequency",
        "DomainWarpFractalType", "DomainWarpFractalGain", "DomainWarpFractalLacunarity", "DomainWarpFractalOctaves", 
        
        "FractalType",
        "FractalGain", "FractalLacunarity", "FractalOctaves",
        "FractalPingPongStrength", "FractalType", "FractalWeightedStrength", 
        "FractalWeightedStrength", 
        
        "Frequency", "NoiseType", "Seed"
    };
    
    // CONSTRUCTOR
    public MFNL(string name="Noise", int nTiers = 24)
    {
        _name = name;
        _rng = new RandomNumberGenerator();  // inicializamos el rng
        _nTiers = nTiers;
    }

    // NOISE VALUES
    public float GetAbsoluteValueNoise(int x, int y) => Mathf.Abs(GetNoise2D(x, y));

    public float GetNormalizedInverseNoise2D(int x, int y) => 1.0f - GetNormalizedNoise2D(x, y);
    
    public float GetNormalizedNoise2D(int x, int y) => (GetNoise2D(x, y) + 1f) * 0.5f;

    private int GetValueTier(float value, int nTiers = 0)
    {
        nTiers = (nTiers == 0) ? _nTiers : nTiers;
        return (int)(value / (1.0f / nTiers));
    }

    public int GetValueTierAt(int x, int y) => GetValueTier(GetNormalizedNoise2D(x, y)); 
    
    public float GetAbsoluteNoiseValueTierAt(int x, int y, int nTiers = 0) => 
        GetValueTier(Mathf.Abs(GetNoise2D(x, y)), nTiers);


    //  NOISE PARAMS
    public void UpdateNoiseProperty(string param, Variant value) => Set(CamelCaseToSnakeCase(param), value);

    public Variant GetNoiseProperty(string param) => Get(CamelCaseToSnakeCase(param));

    public void RandomizeSeed() => SetSeed(_rng.RandiRange(0, 999999999));   // luego hau que recargar

    public void SetSeed(int seed) => Seed = seed;


    //  NOISE JSON
    public void SaveToJson(string filename="")
    {
        var noiseDict = _noiseProperties.ToDictionary(vts => vts, vts => 
            Get(CamelCaseToSnakeCase(vts)).ToString());
        var jsonString = JsonSerializer.Serialize(noiseDict);

        if (filename == ""){filename = _name;}
        if (!Regex.IsMatch(filename, @"\.json$", RegexOptions.IgnoreCase)){filename += ".json";}    
        File.WriteAllText("resources/noise/" + filename, jsonString);   // TODO: hacer con la librería de Godot
    }

    public void LoadFromJson(string filename)
    {
        if (!Regex.IsMatch(filename, @"\.json$", RegexOptions.IgnoreCase)){filename += ".json";}    // si no viene la extensión, la indicamos
        var file = Godot.FileAccess.Open("res://resources/noise/" + filename, Godot.FileAccess.ModeFlags.Read);
        var noiseDict = JsonSerializer.Deserialize<Dictionary<string, string>>(file.GetAsText());
        foreach (var kvp in noiseDict) Set(CamelCaseToSnakeCase(kvp.Key), kvp.Value);
    }

    // AUX FUNCTIONS
    public string GetParamValueAsString(string param) => Get(param).ToString();
    
    private static string CamelCaseToSnakeCase(string str) 
        => Regex.Replace(str, @"([A-Z])", "_$1").TrimStart('_').ToLower();

    public MFNL FromFastNoiseLite(FastNoiseLite toClone) => (MFNL) MemberwiseClone();

    public string[] GetNoiseProperties() => _noiseProperties;

}