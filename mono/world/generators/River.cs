using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using MathNet.Numerics;
using Tartheside.mono.utilities.math;
using Tartheside.mono.world.biomes;
using Tartheside.mono.world.entities;

namespace Tartheside.mono.world.generators;

// TODO: quizás con la Entity no haría falta un generador como tal.

public partial class River : BaseGenerator
{
    private Elevation _elevation;
    private float _riverPathfindingElevationPenalty;
    
    private RiverTAStar _pathfindingAStar;
    private Array<RiverEntity> _rivers;

    private const float TrueValue = 0.999f;
    

    // Generators parameters
    private int _maxIterations = 256; // TODO: que sea parámetro del generador y considerar en el while
    private int _minRValue = 2;
    private int _maxRValue = 6; 
    private float _minRandomAlpha = 0f;
    private float _maxRandomAlpha = MathDotNetHelper.Pi;
    
    public River(Vector2I worldSize, Vector2I chunkSize, Vector2I offset, int nTiers) 
        : base(worldSize, chunkSize, offset, nTiers)
    {
        _rivers = new Array<RiverEntity>();
        PathfindingAStarSetup();
    }

    public void PathfindingAStarSetup() => _pathfindingAStar = new RiverTAStar(Offset, 
        Offset + WorldSize, _elevation, _riverPathfindingElevationPenalty);

    // Generating rivers
    public void GenerateRiver(Vector2I birthPos)
    {
        var riverEntity = new RiverEntity();
        riverEntity.SetBirthPosition(birthPos.X, birthPos.Y);

        // TODO: en A*, comprobar que los puntos del río están dentro de los límites.
        // TODO: considerar constraints de elevaciones

        var mouthReached = false;
        var currentPosition = birthPos;

        var r = 0;
        var alpha = GetRandomAlpha();   //TODO: inicializar alpha hacia donde la elevación sea menor
        var iterations = 0;
        
        while (!mouthReached)
        {
            r = GetRandomR(_minRValue, _maxRValue);   // TODO: máximo y mínimo deberían ser parámetros del generador.
            alpha = GetDescendantAlpha(alpha, r, currentPosition);
            var nextPoint = currentPosition + GetInc(r, alpha);

            foreach (var point in _pathfindingAStar.GetPath(currentPosition, nextPoint))
            {
                AddPointToRiverEntity(point, riverEntity);  // TODO: añadir los puntos a la matriz sólo si el río es validado
                if (!Biome.IsSea(_elevation, point.X - Offset.X, point.Y - Offset.Y)) continue;
                mouthReached = true;
                break;
            }
            currentPosition = nextPoint;
            iterations++;   // TODO: si pasamos del número máximo de iteraciones, el río no será válido 
        }

        if (ValidateRiver(riverEntity))
        {
            var riverMouth = riverEntity.GetRiverPath().Last(); // TODO: pasarle Vector2I a SetMouthPosition 
            riverEntity.SetMouthPosition(riverMouth.X, riverMouth.Y);   
            _rivers.Add(riverEntity);            
        }
        

    }

    private void AddPointToRiverEntity(Vector2I point, RiverEntity riverEntity)
    {
        riverEntity.AddPoint(point);
        SetValueAt(point.X, point.Y, TrueValue);
    }
    
    private static Vector2I GetInc(int r, float alpha) => new((int)Math.Round(r * MathDotNetHelper.Cos(alpha)),
            (int)Math.Round(r * MathDotNetHelper.Sin(alpha)));

    private float GetDescendantAlpha(float alpha, int r, Vector2I currentPosition)
    {
        const float minAlphaVariation = 0;  // TODO: que sean parámetros
        const float maxAlphaVariation = MathDotNetHelper.Pi;
        var availableElevations = new List<float>();
        var availableAngles = new List<float>();
        
        // TODO: experimentar con los rangos de variación.
        var minElevation = _elevation.GetValueAt(currentPosition.X - Offset.X, currentPosition.Y - Offset.Y);
        foreach (float angle in Generate.LinearSpaced(24,  alpha-minAlphaVariation, alpha+maxAlphaVariation))
        {// TODO: cuidado con ese 24.
            var nextPoint = currentPosition + GetInc(r, angle) - Offset;
            var incElevation = _elevation.GetValueAt(nextPoint.X, nextPoint.Y); 
            // TODO: fallará si nextPoint está fuera del rango de la matriz
            
            if (!(incElevation < minElevation)) continue;
            
            availableElevations.Add(incElevation);
            availableAngles.Add(angle);
            //return angle; // no funciona mal si devolvemos el primer menor encontrado...
        }

        if (availableAngles.Count != 0)
        {
            return availableAngles[0];
            /*
                 TODO: ahora mismo estamos devolviendo la primera ocurrencia de ángulo correspondiente a una elevación
                 menor a la anterior. Esto podría parametrizarse y devolver, por ejemplo, el ángulo correspondiente a la
                 menor de todas las elevaciones disponibles. (con un diccionario).
            */
        }
        return alpha;
    }

    private int GetRandomR(int minValue, int maxValue) => MathDotNetHelper.GetRandomIntInRange(minValue, maxValue); // TODO: minValue y maxValue deberían ser parámetros del generador si los usamos en la generación.

    private float GetRandomAlpha() => MathDotNetHelper.GetRandomFloatInRange(_minRandomAlpha, _maxRandomAlpha);
    
    private int RandomizeR(int r, int minChange, int maxChange) => 
        r + MathDotNetHelper.GetRandomIntInRange(minChange, maxChange);
    private float RandomizeAlpha(float alpha) =>    // TODO: el rango de variación de alpha podría ser un parámetro del generador
        alpha + MathDotNetHelper.GetRandomFloatInRange(-MathDotNetHelper.Pi / 4f, MathDotNetHelper.Pi / 4f);

    private bool ValidateRiver(RiverEntity riverToValidate)
    {
        // TODO: comprobamos si es válido en función de las reglas que establezcamos.
        return true;
    }
    
    private bool IsValidRiverBirth(int x, int y)
    {
        return false;
    }
    
    
    //TODO crear funcion para calcular el caudal en cierta posición x,y
    
    


    // getters & setters
    public void SetParameterElevation(Elevation elevation) => _elevation = elevation;
    public void SetParameterRiverPathfindingElevationPenalty(float riverPathfindingElevationPenalty) =>
        _riverPathfindingElevationPenalty = riverPathfindingElevationPenalty;


    
    /* TODO:    necesitamos un método para volver a generar los mismos ríos (mismo nacimiento y desembocadura) pero con
                nuevo valor de ElevationPenalty. Lo metemos aquí, en UpdateElevationPenalty. Quizás no la misma 
                desembocadura. */
    public void UpdateElevationPenalty(float newElevationPenalty)
    {
        SetParameterRiverPathfindingElevationPenalty(newElevationPenalty);
        foreach (var river in _rivers)
        {
            
        }
    }

    public override void Randomize()
    {
        // TODO: conociendo la posición de nacimiento de todos los ríos de _rivers, los volvemos a generar, de forma
        // que r y alpha serán distintos.
        var currentRivers = _rivers.Duplicate(); 
        _rivers.Clear();
        SetClearMatrix(WorldSize);
        foreach (var river in currentRivers)
            GenerateRiver(river.GetBirthPosition());
    }
    
    // TODO: necesitamos implementar de alguna manera persistencia para los ríos.
}

