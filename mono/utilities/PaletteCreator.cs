using Godot;
using System;
using System.Drawing;
using System.Drawing.Imaging;

public partial class PaletteCreator : GodotObject
{
    int TileSize = 16;
    public void CreateGradientPalette(int nColors = 48, string filename="palette.png", string initialColor = "#FFFFFF", string finalColor = "#000000", string middleColor = "#749140")
    {
        // Obtenemos la lista de colores
        Godot.Collections.Array<string> paletteArray = GetGradientPaletteString(nColors, initialColor, finalColor, middleColor);

        // Creamos la imagen
        CreatePalettePng(paletteArray, filename);
    }

    
    public Godot.Collections.Array<string> GetGradientPaletteString(int nColors = 48, string initialColor = "#FFFFFF", string finalColor = "#000000", string middleColor = "#749140")   // verde
    {
        // (el color intermedio determina el tono de la paleta)
        // Godot.Color paletteColor = new Godot.Color(0.8f, 0.4f, 0.4f); // Rojo claro
        
        Godot.Collections.Array<string> paletteArray = new Godot.Collections.Array<string>();
        

        for (int i = 0; i < nColors; i++)
        {
            float t = (float)i / (nColors - 1);
            Godot.Color currentColor;

            if (t <= 0.5f)
            {
                float partialT = t * 2;
                currentColor = LerpColor(HexToColor(initialColor), HexToColor(middleColor), partialT);
            }
            else
            {
                float partialT = (t - 0.5f) * 2;
                currentColor = LerpColor(HexToColor(middleColor), HexToColor(finalColor), partialT);
            }

            paletteArray.Add(ColorToHex(currentColor));
        }

        return paletteArray;
    }
    public void CreatePalettePng(Godot.Collections.Array<string> paletteArray, string filename="palette.png")
    {   
        using (var bmp = new System.Drawing.Bitmap(TileSize * paletteArray.Count, TileSize))
        {
             using (Graphics g = Graphics.FromImage(bmp))
            {
                for (int i = 0; i < paletteArray.Count; i++)
                {
                    using (var brush = new SolidBrush(System.Drawing.ColorTranslator.FromHtml(paletteArray[i])))
                    {
                        g.FillRectangle(brush, i * TileSize, 0, TileSize, TileSize);
                    }
                }
            }
            bmp.Save("./"+filename, ImageFormat.Png);    // TODO: guardar con el nombre del middleColor, obteniéndolo el array
        }
    }

    private Godot.Color LerpColor(Godot.Color a, Godot.Color b, float t)
    {
        float _r = a.R + (b.R - a.R) * t;
        float _g = a.G + (b.G - a.G) * t;
        float _b = a.B + (b.B - a.B) * t;
        return new Godot.Color(_r, _g, _b);
    }
    private Godot.Color HexToColor(string hex)
    {   
        if (hex.StartsWith("#"))
        {
            hex = hex.Substring(1);
        }

        if (hex.Length != 6)
        {
            throw new ArgumentException("El código hexadecimal de color debe tener 6 caracteres (sin el símbolo '#').");
        }

        int r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        int g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        int b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

        return new Godot.Color(r / 255f, g / 255f, b / 255f);
    }
    private string ColorToHex(Godot.Color color)
    {
        return $"#{(int)(color.R * 255):X2}{(int)(color.G * 255):X2}{(int)(color.B * 255):X2}";
    }
}
