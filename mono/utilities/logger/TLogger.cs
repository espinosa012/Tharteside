using System;
using Godot;

namespace Tartheside.mono.utilities.logger;

public partial class TLogger : GodotObject
{

    public static void Info(string message)
    {
        string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string logMessage = $"[{timeStamp}] [INFO] {message}";
        GD.Print(logMessage);
    }
    
    
}