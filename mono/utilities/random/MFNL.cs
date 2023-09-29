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

    private string[] _valuesToSave = new string[] {
        "NoiseType", "Seed", "Frequency", "FractalType", "FractalGain", 
        "FractalLacunarity", "FractalOctaves", "FractalPingPongStrength", "FractalWeightedStrength", 
        "CellularDistanceFunction", "CellularReturnType", "CellularJitter"
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
    
    public void CreateNoiseChunk(Vector2I position, Vector2I chunkSize)
    {//TODO
        int startX = position.X * chunkSize.X;
        int endX = startX + chunkSize.X;
        int startY = position.Y * chunkSize.Y;
        int endY = startY + chunkSize.Y;
		
		for (int x = startX; x < endX; x++)
		{
			for (int y = startY; y < endY; y++)
			{
				
			}
		}
    }


    //  NOISE PARAMS
    public void UpdateParam(string param, Variant value) => Set(CamelCaseToSnakeCase(param), value);

    public Variant GetParam(string param) => Get(CamelCaseToSnakeCase(param));

    public void RandomizeSeed() => SetSeed(Rng.RandiRange(0, 999999999));   // luego hau que recargar

    public void SetSeed(int seed) => Set("Seed", seed);


    //  NOISE JSON
    public void SaveToJSON(string filename="")
    {
        Dictionary<string, string> noiseDict = new Dictionary<string, string>();
        
        foreach (string vts in _valuesToSave) noiseDict.Add(vts, Get(CamelCaseToSnakeCase(vts)).ToString());

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
    private string CamelCaseToSnakeCase(string str)  // static function
    {
        // Reemplaza los caracteres en mayúsculas con un guión bajo seguido de la misma letra en minúscula
        return Regex.Replace(str, @"([A-Z])", "_$1").TrimStart('_').ToLower();
    }

    public MFNL FromFastNoiseLite(FastNoiseLite toClone)
    {
        return (MFNL) this.MemberwiseClone();
    }

}
