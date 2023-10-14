using System;
using Godot;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.IO;

public partial class MFNL : FastNoiseLite
{
    string Name;
    RandomNumberGenerator Rng;
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
        
        "Frequency", "NoiseType", "Offset", "Seed"
    };
    
    // CONSTRUCTOR
    public MFNL(string name="Noise", int nTiers = 24)
    {
        Name = name;
        Rng = new RandomNumberGenerator();  // inicializamos el rng
        _nTiers = nTiers;
    }

    public void Rename(string newName)
    {
        Name = newName;
    }


    // NOISE VALUES
    public float GetAbsoluteValueNoise(int x, int y)
    {
        return Mathf.Abs(GetNoise2D(x, y));
    }

    public float GetNormalizedInverseNoise2D(int x, int y)
    {
        return 1.0f - GetNormalizedNoise2D(x, y);
    }
    
    public float GetNormalizedNoise2D(int x, int y)
    {
        return (GetNoise2D(x, y) + 1f) * 0.5f;  // pasamos del rango [-1, 1] a [0, 1]
    }

    public int GetValueTier(float value, int nTiers = 0)
    {
        nTiers = (nTiers == 0) ? _nTiers : nTiers;
        return (int)(value / (1.0f / nTiers));
    }

    public int GetValueTierAt(int x, int y)
    {
        return GetValueTier(GetNormalizedNoise2D(x, y));
    }
    
    public float GetAbsoluteNoiseValueTierAt(int x, int y, int nTiers = 0)
    {
        return GetValueTier(Mathf.Abs(GetNoise2D(x, y)), nTiers);
    }


    //  NOISE PARAMS
    public void UpdateParam(string param, Variant value) => Set(CamelCaseToSnakeCase(param), value);

    public Variant GetParam(string param) => Get(CamelCaseToSnakeCase(param));

    public void RandomizeSeed() => SetSeed(Rng.RandiRange(0, 999999999));   // luego hau que recargar

    public void SetSeed(int seed) => Seed = seed;


    //  NOISE JSON
    public void SaveToJSON(string filename="")
    {
        Dictionary<string, string> noiseDict = new Dictionary<string, string>();
        
        foreach (string vts in _noiseProperties) noiseDict.Add(vts, Get(CamelCaseToSnakeCase(vts)).ToString());

        string jsonString = JsonSerializer.Serialize(noiseDict);
        
        // Si no indicamos un nombre para el archivo json en que queremos guardar el ruido, establecemos el nombre del ruido
        if (filename == ""){filename = Name;}

        // si no viene la extensión, la indicamos
        if (!Regex.IsMatch(filename, @"\.json$", RegexOptions.IgnoreCase)){filename += ".json";}    

        // hacer con la librería de Godot
        File.WriteAllText("resources/noise/" + filename, jsonString);   
    }

    public void LoadFromJSON(string filename)
    {
        if (!Regex.IsMatch(filename, @"\.json$", RegexOptions.IgnoreCase)){filename += ".json";}    // si no viene la extensión, la indicamos
        var file = Godot.FileAccess.Open("res://resources/noise/" + filename, Godot.FileAccess.ModeFlags.Read);
		
        // formamos el diccionario
		Dictionary<string, string> noiseDict = JsonSerializer.Deserialize<Dictionary<string, string>>(file.GetAsText());
		foreach (var kvp in noiseDict) Set(CamelCaseToSnakeCase(kvp.Key), kvp.Value);
    }

    // AUX FUNCTIONS
    public string GetParamValueAsString(string param)
    {
        return Get(param).ToString();
    }
    
    private string CamelCaseToSnakeCase(string str)  // static function
    {
        // Reemplaza los caracteres en mayúsculas con un guión bajo seguido de la misma letra en minúscula
        return Regex.Replace(str, @"([A-Z])", "_$1").TrimStart('_').ToLower();
    }

    public MFNL FromFastNoiseLite(FastNoiseLite toClone)
    {
        return (MFNL) this.MemberwiseClone();
    }


    public override string ToString()
    {
        string delimeter = "----------------------------------------\n";
        string toReturn = delimeter + "----" + Name + "----\n";
        Dictionary<string, Variant> noiseDict = new Dictionary<string, Variant>();
        foreach (var prop in _noiseProperties)
        {
            toReturn += prop + ": " + Get(CamelCaseToSnakeCase(prop)) + "\n";
        }
        return toReturn + delimeter;
    }

}
