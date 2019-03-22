///***********************************************************/// 
/// - Project name: Gorillas 3D -|- Created by: Daniel Hogg - ///
/// - Version: found in console -|- Completed: TBA          - ///
///***********************************************************///
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using System;
using System.Drawing;
using OpenTK.Input;
using System.IO;
using OpenCGE.Objects;
using OpenCGE.Managers;
using OpenCGE.Scenes;

namespace Gorillas3D.Scenes
{
    public class MainMenu : Scene
    {
        AudioContext audioContext;
        int Players, Input, TotalInputs, Rounds;
        string Player1, Player2, CurrentValue;
        float Gravity;

        public MainMenu(SceneManager sceneManager, SystemManager systemManager, EntityManager entityManager) : base(sceneManager, systemManager, entityManager)
        {
            // Set the title of the window
            sceneManager.Title = "Gorillas 3D";
            sceneManager.Icon = Icon.ExtractAssociatedIcon("Textures/banana_icon.ico");
            sceneManager.Keyboard.KeyDown += Keyboard_KeyDown;
            // Set the Render and Update delegates to the Update and Render methods of this class
            sceneManager.renderer = Render;
            sceneManager.updater = Update;
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            // Updates SceneManager that this scene is active
            sceneManager.ShowCursor = true;
            sceneManager.MouseControlledCamera = false;
            // Initliase variables and objects
            audioContext = new AudioContext();
            // Variables used to keep track of which settings are being entered
            Players = 0; Input = 0; TotalInputs = 5;
            // Variables used to allow players to enter their game settings
            Rounds = 0; Player1 = ""; Player2 = ""; CurrentValue = "0"; Gravity = 0;   
            // Plays intro audio as soon as scene has loaded in
            Audio Intro = new Audio(ResourceManager.LoadAudio("Audio/intro_effect.wav"));
            Intro.UpdateEmitterPosition(sceneManager.camera.Position);
            Intro.VolumeOfAudio = 0.4f;
            Intro.PlayAudio();
        }
        
        public override void Update(FrameEventArgs e)
        {
            float dt = (float)e.Time;
            switch (Input)
            {
                case 0:
                    Players = int.Parse(CurrentValue);
                    if(Players > 2) { Players = 2; }
                    break;
                case 1:
                    Player1 = CurrentValue;
                    break;
                case 2:
                    Player2 = CurrentValue;
                    break;
                case 3:
                    Rounds = int.Parse(CurrentValue);
                    break;
                case 4:
                    Gravity = float.Parse(CurrentValue);
                    break;
            }
        }

        public override void Render(FrameEventArgs e)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, sceneManager.Width, 0, sceneManager.Height, -1, 100);

            GUI.clearColour = Color.Black;


            //Display the Title
            float width = sceneManager.Width, height = sceneManager.Height, fontSize = Math.Min(width, height) / 25f;
            int middlex = (int)(width / 2), middley = (int)(height / 2);
            // Draws scene title
            GUI.Label(new Rectangle(0, (int)(fontSize / 2f), (int)width, (int)(fontSize * 2f)), "Gorillas 3D", (int)fontSize, StringAlignment.Center);
            GUI.Label(new Rectangle(0, (int)(middley / 2.3f), (int)width, (int)(fontSize * 2.0f)), "1 or 2 players (Default = 1): " + Players, (int)(fontSize * 0.5f), StringAlignment.Center);
            if(Input > 0) { GUI.Label(new Rectangle(0, (int)(middley / 2.0f), (int)width, (int)(fontSize * 2.0f)), "Name of Player 1 (Default = 'Player 1'): " + Player1, (int)(fontSize * 0.5f), StringAlignment.Center); }
            if(Input > 1) { GUI.Label(new Rectangle(0, (int)(middley / 1.75f), (int)width, (int)(fontSize * 2.0f)), "Name of Player 2 (Default = 'Player 2'): " + Player2, (int)(fontSize * 0.5f), StringAlignment.Center); }
            if(Input > 2) { GUI.Label(new Rectangle(0, (int)(middley / 1.55f), (int)width, (int)(fontSize * 2.0f)), "Play to how many total points (Default = 3)? " + Rounds.ToString(), (int)(fontSize * 0.5f), StringAlignment.Center); }
            if(Input > 3) { GUI.Label(new Rectangle(0, (int)(middley / 1.38f), (int)width, (int)(fontSize * 2.0f)), "Gravity in Meters/sec (Earth = 9.8)? " + Gravity.ToString(), (int)(fontSize * 0.5f), StringAlignment.Center); }
            if(Input > 4)
            {
                GUI.Label(new Rectangle(0, (int)(middley / 1.0f), (int)width, (int)(fontSize * 2.0f)), "----------------", (int)(fontSize * 0.5f), StringAlignment.Center);
                GUI.Label(new Rectangle(0, (int)(middley / 0.9f), (int)width, (int)(fontSize * 2.0f)), "V = View Intro", (int)(fontSize * 0.5f), StringAlignment.Center);
                GUI.Label(new Rectangle(0, (int)(middley / 0.85f), (int)width, (int)(fontSize * 2.0f)), "P = Play Game ", (int)(fontSize * 0.5f), StringAlignment.Center);
                GUI.Label(new Rectangle(0, (int)(middley / 0.75f), (int)width, (int)(fontSize * 2.0f)), "Your Choice? ", (int)(fontSize * 0.5f), StringAlignment.Center);
            }
            // Renders UI to screen
            GUI.Render();
        }
        /// <summary>
        /// Writes all entered settings to settings file and launches desired game scene
        /// </summary>
        /// <param name="Scene">Scene ID to be ran (ID = order in config file)</param>
        void CompileUserInputs(int Scene)
        {
            File.WriteAllText("Gorillas3D.txt", string.Empty);
            StreamWriter w = new StreamWriter("Gorillas3D.txt");
            // Handles default values if left user did not enter anything
            if(Players == 0) { Players = 1; }
            if (Player1 == "") { Player1 = "Player 1"; }
            if(Player2 == "") { Player2 = "Player 2"; }
            if(Rounds == 0) { Rounds = 3; }
            if(Gravity == 0) { Gravity = 9.8f; }
            // Writes the game settings to a file to be used in other games scenes
            w.WriteLine("Players = " + Players);
            w.WriteLine("Player1 = " + Player1);
            w.WriteLine("Player2 = " + Player2);
            w.WriteLine("Rounds  = " + Rounds);
            w.WriteLine("Gravity = " + Gravity);
            w.WriteLine("Score   = 0-0");
            // Closed the file and load next scene
            w.Close();
            // Changes scene depending on whether to show intro scene or straight to game scene
            sceneManager.ChangeScene(Scene);
        }
        /// <summary>
        /// Used to get player inputs and for validation of those inputs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            string temp = "";
            // Cycle through switch statement to avoid rare key value (e.g. 1 key = Number1 but need 1)
            // This also makes it alphanumeric only which is what is needed
            switch (e.Key)
            {
                // Letters
                case Key.A:
                case Key.B:
                case Key.C:
                case Key.D:
                case Key.E:
                case Key.F:
                case Key.G:
                case Key.H:
                case Key.I:
                case Key.J:
                case Key.K:
                case Key.L:
                case Key.M:
                case Key.N:
                case Key.O:
                case Key.Q:
                case Key.R:
                case Key.S:
                case Key.T:
                case Key.U:
                case Key.W:
                case Key.X:
                case Key.Y:
                case Key.Z:
                    if(Input < 3)
                    {
                        if (e.Shift) { temp += e.Key.ToString(); }
                        else { temp += e.Key.ToString().ToLower(); }
                    }
                    break;
                case Key.P:
                    if (Input >= TotalInputs) { CompileUserInputs(1); }
                    else
                    {
                        if (Input < 3)
                        {
                            if (e.Shift) { temp += e.Key.ToString(); }
                            else { temp += e.Key.ToString().ToLower(); }
                        }
                    }
                    break;
                case Key.V:
                    if (Input >= TotalInputs) { CompileUserInputs(3); }
                    else
                    {
                        if (Input < 3)
                        {
                            if (e.Shift) { temp += e.Key.ToString(); }
                            else { temp += e.Key.ToString().ToLower(); }
                        }
                    }
                    break;
                // Numbers
                case Key.Number0:
                    temp += "0";
                    break;
                case Key.Number1:
                    temp += "1";
                    break;
                case Key.Number2:
                    temp += "2";
                    break;
                case Key.Number3:
                    temp += "3";
                    break;
                case Key.Number4:
                    temp += "4";
                    break;
                case Key.Number5:
                    temp += "5";
                    break;
                case Key.Number6:
                    temp += "6";
                    break;
                case Key.Number7:
                    temp += "7";
                    break;
                case Key.Number8:
                    temp += "8";
                    break;
                case Key.Number9:
                    temp += "9";
                    break;
                // Format related keys
                case Key.BackSpace:
                    if(CurrentValue.Length <= 0) { break; }
                    CurrentValue = CurrentValue.Substring(0, CurrentValue.Length - 1);
                    break;
                case Key.Period:
                    temp += ".";
                    break;
                // Functions
                case Key.Enter:
                    Input++;
                    if (Input == 0 || Input > 2) { CurrentValue = "0"; }
                    else { CurrentValue = ""; }
                    break;
                case Key.Escape:
                    sceneManager.Exit();
                    break;

            }

            CurrentValue += temp;
        }   

        public override void Close()
        {
            audioContext.Dispose();
            sceneManager.Keyboard.KeyDown -= Keyboard_KeyDown;
        }
    }
}
