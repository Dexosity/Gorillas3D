///***********************************************************/// 
/// - Project name: Gorillas 3D -|- Created by: Daniel Hogg - ///
/// - Version: found in console -|- Completed: TBA          - ///
///***********************************************************///
using OpenCGE.Managers;
using OpenCGE.Objects;
using System;
using System.Collections.Generic;

namespace Gorillas3D
{
    class Game
    {
        [STAThread]
        static void Main(string[] args)
        {
            string Version = "1.0.5";
            // This creates a list for the game engine to have access to the custom scenes
            List<string> SceneList = new List<string>();
            // Alternatly you could ignore the config file and manualy type these in to pass to SceneManager
            WindowConfig config = new WindowConfig();
            // Loops through all scenes specified in the config
            for (int i = 0; i < config.Scenes.Length; i++)
            {
                Type type = Type.GetType("Gorillas3D.Scenes." + config.Scenes[i]);
                SceneList.Add(type.AssemblyQualifiedName);
            }
            // Output version
            Console.WriteLine("Gorillas 3D by DH -> Version: " + Version);
            // Initlises your game with all your specified scenes
            using (var game = new SceneManager(SceneList.ToArray()))
                game.Run();
        }
    }
}
