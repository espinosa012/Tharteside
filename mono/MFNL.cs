using Godot;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.IO;

public partial class MFNL : FastNoiseLite
{
    string Name;
    RandomNumberGenerator Rng;


    // CONSTRUCTOR
    public MFNL(string name="Noise")
    {
        Name = name;
        Rng = new RandomNumberGenerator();  // inicializamos el rng
    }

    public void Rename(string newName)
    {
        Name = newName;
    }


    // NOISE VALUES

    public float GetNormalizedNoise2D(int x, int y)
    {
        return (GetNoise2D(x, y) + 1f) * 0.5f;  // pasamos del rango [-1, 1] a [0, 1]
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
    public void UpdateParam(string param, Variant value)
    {
        Set(CamelCaseToSnakeCase(param), value);
    }

    public Variant GetParam(string param)
    {
        return Get(CamelCaseToSnakeCase(param));
    }

    public void RandomizeSeed()
    {
        UpdateParam("Seed", Rng.RandiRange(0, 999999999));        
    }

    public void SetSeed(int seed)
    {
        UpdateParam("Seed", seed);
    }



    //  NOISE JSON
    public void SaveToJSON(string filename="")
    {
        Dictionary<string, string> noiseDict = new Dictionary<string, string>();
        string[] valuesToSave = new string[] {"NoiseType", "Seed", "Frequency", "FractalType", "FractalGain", "FractalLacunarity", "FractalOctaves", "FractalPingPongStrength", "FractalWeightedStrength", "CellularDistanceFunction", "CellularReturnType", "CellularJitter"};
        foreach (string vts in valuesToSave)
        {
            noiseDict.Add(vts, Get(CamelCaseToSnakeCase(vts)).ToString());
        }

        string jsonString = JsonSerializer.Serialize(noiseDict);
        
        // Si no indicamos un nombre para el archivo json en que queremos guardar el ruido, establecemos el nombre del ruido
        if (filename == ""){filename = Name;}

        if (!Regex.IsMatch(filename, @"\.json$", RegexOptions.IgnoreCase)){filename += ".json";}    // si no viene la extensión, la indicamos
		GD.Print(filename);

        File.WriteAllText("resources/noise/" + filename, jsonString);   // hacer con la librería de Godot
    }

    public void LoadFromJSON(string filename)
    {
        if (!Regex.IsMatch(filename, @"\.json$", RegexOptions.IgnoreCase)){filename += ".json";}    // si no viene la extensión, la indicamos
        var file = Godot.FileAccess.Open("res://resources/noise/" + filename, Godot.FileAccess.ModeFlags.Read);
		// formamos el diccionario
		Dictionary<string, string> noiseDict = JsonSerializer.Deserialize<Dictionary<string, string>>(file.GetAsText());
		foreach (var kvp in noiseDict)
		{
			Set(CamelCaseToSnakeCase(kvp.Key), kvp.Value);
		}
    }



    // AUX FUNCTIONS
    public string CamelCaseToSnakeCase(string str)  // static function
    {
        // Reemplaza los caracteres en mayúsculas con un guión bajo seguido de la misma letra en minúscula
        return Regex.Replace(str, @"([A-Z])", "_$1").TrimStart('_').ToLower();
    }


}
