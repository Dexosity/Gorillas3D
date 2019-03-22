///***********************************************************/// 
/// - Project name: Gorillas 3D -|- Created by: Daniel Hogg - ///
/// - Version: found in console -|- Completed: TBA          - ///
///***********************************************************///
using OpenCGE.Managers;
using OpenCGE.Scenes;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using System;
using System.Drawing;
using System.IO;
using OpenTK.Input;

namespace Gorillas3D.Scenes
{
    public class GameOver : Scene
    {

        AudioContext audioContext;
        int P1Score, P2Score;
        string Player1, Player2;

        public GameOver(SceneManager sceneManager, SystemManager systemManager, EntityManager entityManager) : base(sceneManager, systemManager, entityManager)
        {
            // Set the title of the window
            sceneManager.Title = "Gorillas 3D";
            sceneManager.Keyboard.KeyDown += Keyboard_KeyDown;
            // Set the Render and Update delegates to the Update and Render methods of this class
            sceneManager.renderer = Render;
            sceneManager.updater = Update;
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            audioContext = new AudioContext();
            LoadGameSettings();
        }
        
        public override void Update(FrameEventArgs e)
        {
            float dt = (float)e.Time;
        }

        public override void Render(FrameEventArgs e)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, sceneManager.Width, 0, sceneManager.Height, -1, 100);

            GUI.clearColour = Color.Black;


            //Display the Title
            float width = sceneManager.Width, height = sceneManager.Height, fontSize = Math.Min(width, height) / 10f;
            int middlex = (int)(width / 2), middley = (int)(height / 2);
            // Draws scene title
            GUI.Label(new Rectangle(0, (int)(fontSize / 2f), (int)width, (int)(fontSize * 2f)), "Game Over!", (int)(fontSize * 0.5f), StringAlignment.Center);
            GUI.Label(new Rectangle(0, (int)(height * 0.3f), (int)width, (int)(fontSize * 2f)), "Score: ", (int)(fontSize * 0.5f), StringAlignment.Center);
            GUI.Label(new Rectangle(0, (int)(height * 0.4f), (int)width, (int)(fontSize * 2f)), Player1 + "                        " + P1Score, (int)(fontSize * 0.25f), StringAlignment.Center);
            GUI.Label(new Rectangle(0, (int)(height * 0.45f), (int)width, (int)(fontSize * 2f)), Player2 + "                        " + P2Score, (int)(fontSize * 0.25f), StringAlignment.Center);

            GUI.Label(new Rectangle(0, (int)(height * 0.75f), (int)width, (int)(fontSize * 2f)), "Press any key to continue", (int)(fontSize * 0.25f), StringAlignment.Center);
            // Renders UI to screen
            GUI.Render();
        }

        private void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            sceneManager.ChangeScene(0);
            
        }
        /// <summary>
        /// Loads in game settings from file
        /// </summary>
        private void LoadGameSettings()
        {
            StreamReader r = new StreamReader("Gorillas3D.txt");
            // Starts from 1 as first line is comment
            string[] lines = r.ReadToEnd().Split('\n');
            Player1 = lines[1].Substring(10, lines[1].ToString().Length - 11);
            Player2 = lines[2].Substring(10, lines[2].ToString().Length - 11);

            string[] scroe = lines[5].Substring(10, lines[5].ToString().Length - 11).Split('-');
            P1Score = int.Parse(scroe[0]);
            P2Score = int.Parse(scroe[1]);
            r.Close();
        }

        public override void Close()
        {
            audioContext.Dispose();
            sceneManager.Keyboard.KeyDown -= Keyboard_KeyDown;
        }
    }
}
