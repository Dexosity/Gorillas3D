///***********************************************************/// 
/// - Project name: Gorillas 3D -|- Created by: Daniel Hogg - ///
/// - Version: found in console -|- Completed: TBA          - ///
///***********************************************************///
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using System;
using System.Drawing;
using System.IO;
using OpenCGE.Objects;
using OpenCGE.Managers;
using OpenCGE.Scenes;

namespace Gorillas3D.Scenes
{
    public class IntroScene : Scene
    {

        AudioContext audioContext;

        float timer = 14.0f;
        float animation = 1.5f;
        string gorillas_img, Player1, Player2;

        public IntroScene(SceneManager sceneManager, SystemManager systemManager, EntityManager entityManager) : base(sceneManager, systemManager, entityManager)
        {
            // Set the title of the window
            sceneManager.Title = "Gorillas 3D";
            // Set the Render and Update delegates to the Update and Render methods of this class
            sceneManager.renderer = Render;
            sceneManager.updater = Update;


            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            LoadGameSettings();
            audioContext = new AudioContext();

            Audio Intro = new Audio(ResourceManager.LoadAudio("Audio/intro_music.wav"));
            Intro.UpdateEmitterPosition(sceneManager.camera.Position);
            Intro.VolumeOfAudio = 0.15f;
            Intro.PlayAudio();

            gorillas_img = "intro_gorillas1";
        }
        
        public override void Update(FrameEventArgs e)
        {
            float dt = (float)e.Time;

            timer -= dt;
            animation -= dt;
            // When timer hits 0 then change scene to gamescene
            if(timer <= 0.0f)
            {
                sceneManager.ChangeScene(1);
            }
            // Changes the images to make an animation effect similar to original game
            if(animation <= 0.0f)
            {
                if (gorillas_img.Contains("1")) { gorillas_img = "intro_gorillas2"; }else
                    if (gorillas_img.Contains("2")){ gorillas_img = "intro_gorillas1";  }
                animation = 1.0f;
            }
        }

        public override void Render(FrameEventArgs e)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, sceneManager.Width, 0, sceneManager.Height, -1, 100);

            GUI.clearColour = Color.DarkBlue;


            //Display the Title
            float width = sceneManager.Width, height = sceneManager.Height, fontSize = Math.Min(width, height) / 10f;
            int middlex = (int)(width / 2), middley = (int)(height / 2);
            // Draws scene title
            GUI.Label(new Rectangle(0, (int)(fontSize / 2f), (int)width, (int)(fontSize)), "Gorillas 3D", (int)(fontSize * 0.5f), StringAlignment.Center);
            GUI.Label(new Rectangle(0, (int)(middley * 0.4f), (int)width, (int)(fontSize)), "STARRING", (int)(fontSize * 0.25f), StringAlignment.Center);
            GUI.Label(new Rectangle(0, (int)(middley * 0.5f), (int)width, (int)(fontSize)), Player1 + " AND " + Player2, (int)(fontSize * 0.20f), StringAlignment.Center);
            GUI.DrawImage(middlex - (44 * 4), middley, "Textures/" + gorillas_img + ".png", false, new Vector2(4.0f));
            // Renders UI to screen
            GUI.Render();
        }
        // Loads in game settings from file
        private void LoadGameSettings()
        {
            StreamReader r = new StreamReader("Gorillas3D.txt");
            // Starts from 1 as first line is comment
            string[] lines = r.ReadToEnd().Split('\n');

            Player1 = lines[1].Substring(10, lines[1].ToString().Length - 11);
            Player2 = lines[2].Substring(10, lines[2].ToString().Length - 11);
            r.Close();
        }

        public override void Close()
        {
            audioContext.Dispose();
        }
    }
}
