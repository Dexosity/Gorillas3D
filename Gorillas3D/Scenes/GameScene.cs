///***********************************************************/// 
/// - Project name: Gorillas 3D -|- Created by: Daniel Hogg - ///
/// - Version: found in console -|- Completed: TBA          - ///
///***********************************************************///
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK.Input;
using OpenCGE.Objects;
using OpenCGE.Components;
using OpenCGE.Managers;
using OpenCGE.Systems;
using OpenCGE.Scenes;
using System.IO;

namespace Gorillas3D.Scenes
{
    public class GameScene : Scene
    {
        // Game Features
        AudioContext audioContext;
        Audio[] SoundEffects;
        // Game Variables
        int[,] SkylineLayout;
        int InputID, Rounds, P1Score, P2Score;
        string Input, Angle, Power, Player1, Player2;
        bool isAI, isPlayer1, hasHitSun;
        float G1X, G1Y, G2X, G2Y, Gravity, ProjectileTime, WindSpeed, AiInputTimer, sunHitTimer;
        
        public GameScene(SceneManager sceneManager, SystemManager systemManager, EntityManager entityManager) : base(sceneManager, systemManager, entityManager)
        {
            // Set the title of the window
            sceneManager.Title = "Gorillas";
            sceneManager.Keyboard.KeyDown += Keyboard_KeyDown;
            sceneManager.camera.Position = new Vector3(1.5f, 6.0f, 15.0f);
            sceneManager.camera.Rotate(0.0f, 0.0f, 0.0f, 1.0f);
            // Set the Render and Update delegates to the Update and Render methods of this class
            sceneManager.renderer = Render;
            sceneManager.updater = Update;
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            // Load entities and systems
            CreateEntities();
            CreateSystems();
            // Variable intilisation
            audioContext =  new AudioContext();
            // Game Variables
            isAI = true;                             // Default settings have AI active - may change when settings are loaded in
            isPlayer1 = true;                        // True = player1 turn and false = player2 turn
            hasHitSun = false;                       // Used to see if banana has hit the sun so texture can be changed
            Input = ""; Angle = ""; Power = "";      // Used to allow player to type in their inputs
            InputID = 0;                             // Tracks which input is currently inputed (angle/power/ready to shoot)
            ProjectileTime = 0.0f;                   // Handles how long the banana is in flight for 
            AiInputTimer = 0.0f;                     // Used to space out AI inputs so they are not instant once p1 has finished
            sunHitTimer = 0.0f;                      // Used to time how long the suns texture has changed for when hit
            WindSpeed = GenerateWindValue(3, 6);     // Wind speeds works on scale of -1 to 1 to manipulate player power input
            // Load in game settings (names, Sounds, etc.)
            LoadGameSettings();
            LoadAudioFiles();
        }
        /// <summary>
        /// Game engine gameplay update
        /// </summary>
        /// <param name="e"></param>
        public override void Update(FrameEventArgs e)
        {
            float dt = (float)e.Time;
            // Checks if the game is over and acts accordingly
            if (Rounds <= 0) { sceneManager.NextScene(); }

            // Handle AI player turn
            if (!isPlayer1 && isAI)
            {
                AiInputTimer += dt;
                if (AiInputTimer >= 0.5f)
                {
                    GenerateAiInput(InputID);
                    AiInputTimer = 0.0f;
                    InputID++;
                } 
            }
            // Handles the banana and its animation
            Handle_BananaTrajectory(dt);
            SystemAnimator Animator = (SystemAnimator)systemManager.FindSystem("SystemAnimator");
            Animator.UpdateDeltaTime(dt);

            // Handles the hitting sun effect where its texture gets changed 
            if (hasHitSun)
            {
                sunHitTimer += dt;
                if(sunHitTimer >= 2.0f)
                {
                    ComponentTexture texture = (ComponentTexture)entityManager.FindEntity("SunFace").FindComponent(ComponentTypes.COMPONENT_TEXTURE);
                    texture.Texture = ResourceManager.LoadTexture("Textures/sun_1.png");
                    sunHitTimer = 0.0f;
                    hasHitSun = false;
                }
            }
        }
        /// <summary>
        /// Game engine graphics update (inc. UI)
        /// </summary>
        /// <param name="e"></param>
        public override void Render(FrameEventArgs e)
        {
            systemManager.ActionSystems(entityManager);

            GUI.clearColour = Color.Transparent;

            if(InputID == 0) { Angle = Input; }else
            if(InputID == 1) { Power = Input; }

            float width = sceneManager.Width, height = sceneManager.Height, fontSize = Math.Min(width, height) / 20f;
            int middlex = (int)(width / 2), middley = (int)(height / 2);

            // Draw score board
            GUI.Label(new Rectangle(0, 0 - (int)(fontSize / 4.0f), (int)width, (int)(fontSize * 2f)), P1Score + " > Score < " + P2Score, (int)(fontSize * 0.4f), StringAlignment.Center);

            // Draw in wind arrow
            bool flipped = false;
            if(WindSpeed < 0) { flipped = true; }
            
            GUI.DrawImage((int)(middlex - 16.0f), (int)(height * 0.075f), "Textures/wind.png", flipped, new Vector2(Math.Abs(WindSpeed), 0.5f));

            
            // Draws Player1 UI
            GUI.Label(new Rectangle(-middlex + (Player1.Length * 10) + (int)(width * 0.01f), 0 - (int)(fontSize / 4.0f), (int)width, (int)(fontSize * 2f)), Player1, (int)(fontSize * 0.5f), StringAlignment.Center);
            if (isPlayer1)
            {
                GUI.Label(new Rectangle(-middlex + (Player1.Length * 10) + (int)(width * 0.01f), 0 + (int)(fontSize / 1.5f), (int)width, (int)(fontSize * 2f)), "Angle: " + Angle, (int)(fontSize * 0.5f), StringAlignment.Center);
                if(InputID >= 1)
                {
                    GUI.Label(new Rectangle(-middlex + (Player1.Length * 10) + (int)(width * 0.01f), 0 + (int)(fontSize * 1.5f), (int)width, (int)(fontSize * 2f)), "Power: " + Power, (int)(fontSize * 0.5f), StringAlignment.Center);
                }
            }
            // Draws Player2 UI
            GUI.Label(new Rectangle(middlex - (Player2.Length * 10) - (int)(width * 0.01f), 0 - (int)(fontSize / 4.0f), (int)width, (int)(fontSize * 2f)), Player2, (int)(fontSize * 0.5f), StringAlignment.Center);
            if (!isPlayer1)
            {
                GUI.Label(new Rectangle(middlex - (Player2.Length * 10) - (int)(width * 0.01f), 0 + (int)(fontSize / 1.5f), (int)width, (int)(fontSize * 2f)), "Angle: " + Angle, (int)(fontSize * 0.5f), StringAlignment.Center);
                if (InputID >= 1)
                {
                    GUI.Label(new Rectangle(middlex - (Player2.Length * 10) - (int)(width * 0.01f), 0 + (int)(fontSize * 1.5f), (int)width, (int)(fontSize * 2f)), "Power: " + Power, (int)(fontSize * 0.5f), StringAlignment.Center);
                }
            }
            // Renders UI to screen
            GUI.Render();
        }

        /// <summary>
        /// Creates and intialises all game entities for this game scene
        /// </summary>
        private void CreateEntities()
        {
            // Loads in static entities from file
            entityManager.LoadSceneEntities("Entities/GameScene_Static.xml");
            // Randomly generates the terrain 
            // Params: Xmin, Xmax, Ymin, Ymax
            CreateSkyline(2, 5, 6, 11);
        }
        /// <summary>
        /// Creates and intialises all systems required for this game scene
        /// </summary>
        private void CreateSystems()
        {
            ISystem newSystem;
            // Creates the system to calculate the SkyBox
            newSystem = new SystemSkyBox(ref sceneManager.camera);
            systemManager.AddSystem(newSystem);

            // Creates an array of light point for use in SystemRenderer
            Vector3[] array = new Vector3[]{
                new Vector3(G1X, 13.0f, 10.0f),
                new Vector3(G2X, 13.0f, 10.0f) };
            // Creates the system to calculate all the rendering (including lighting)
            newSystem = new SystemRender(ref sceneManager.camera, array);
            systemManager.AddSystem(newSystem);
            // Creates the system to handle all collision on X&Y axis (Sets object to check collision against to banana)
            ComponentPosition pos = (ComponentPosition)entityManager.FindEntity("Banana").FindComponent(ComponentTypes.COMPONENT_POSITION);
            newSystem = new SystemColliderXY(ref entityManager, pos);
            systemManager.AddSystem(newSystem);
            // Creates the system to handle the animation of game objects
            newSystem = new SystemAnimator();
            systemManager.AddSystem(newSystem);
        }

        #region Gameplay handlers
        /// <summary>
        /// This method should be called on every game update, it checks if a banana is thrown and works outs its trajectory and handles its collision response
        /// </summary>
        /// <param name="dt">delta time from game update frame</param>
        private void Handle_BananaTrajectory(float dt)
        {
            // Checks if the player has entered both angle and power inputs before firing banana
            if (InputID >= 2)
            {
                // Trigger throwing sound effect (uses timer so will only trigger once per throw)
                if(ProjectileTime <= 0.0f)
                {
                    SoundEffects[0].UpdateEmitterPosition(sceneManager.camera.Position);
                    SoundEffects[0].VolumeOfAudio = 0.4f;
                    SoundEffects[0].PlayAudio();
                }
                // Gains access to the collision details
                SystemColliderXY collide = (SystemColliderXY)systemManager.FindSystem("SystemColliderXY");
                // Collision action variables
                float angle, power;
                // Converts player input (string) to usable float values
                if (Angle == "") { angle = 0.0f; }
                else { angle = (float)((Math.PI / 180) * float.Parse(Angle)); }
                if (Power == "") { power = 0.0f; }
                else { power = float.Parse(Power) / 5.0f; }
                // Generic variables for both players inputs
                float playerX = 0.0f, playerY = 0.0f;
                // These values are used for when the banana spawns in so it doesnt trigger on the player immediatly
                if (isPlayer1) { playerX = G1X; playerY = G1Y + 1.5f; }
                else { playerX = G2X; playerY = G2Y + 1.5f; }
                // Based on which player is active the values will be edited accordingly
                if (isPlayer1)
                {
                    if(WindSpeed < 0){ power -= power * WindSpeed; }
                    else { power += power * (1.0f - WindSpeed); }
                }
                else
                {
                    if (WindSpeed > 0) { power -= power * WindSpeed; }
                    else { power += power * (1.0f - WindSpeed); }
                }
                // This is necassary for when the right player goes as the X values 
                // needs to be inverted but you dont want to edit orignal timer
                float time = ProjectileTime;
                // Checks if its player 2 and inverts time
                if (!isPlayer1) { time *= -1; }

                // Banana Trajectory
                // X = current xPos, Y = current yPos, P = power, T = time of travel and G = gravity
                // x = X+P*COS(A)*T 
                // y = Y+P*SIN(A)*T-G*T*T/2
                float X = playerX + power * (float)Math.Cos(angle) * time;
                float Y = (playerY + 2.0f) + power * (float)Math.Sin(angle) * ProjectileTime - Gravity * ProjectileTime * (ProjectileTime / 2);

                // If time is 0 then this is the start of a turn so trigger should be cleared
                if (time == 0)
                {
                    // This is required as the engine stores the last position of the banana
                    // so when it moves up to above the player it causes a collision hit with 
                    // entities as it moves to its new psoition
                    collide.UpdateLastPosition(new Vector3(playerX, playerY, -23.0f));
                    collide.ClearLastTrigger();
                }
                // Increment the projectile time by time of this update pass (used for trajectory)
                ProjectileTime += dt;

                HandleBananaMovement(ref collide, new Vector2(X, Y));
                
            }
        }
        /// <summary>
        /// Handles the projectile entity movement and collision triggers
        /// </summary>
        /// <param name="pCollide">The system collider being used</param>
        /// <param name="pTrajectory">Trajectory values calculated</param>
        private void HandleBananaMovement(ref SystemColliderXY pCollide, Vector2 pTrajectory)
        {
            // Used to time out banana if trajectory takes it out of camera view
            float MaxTravelTime = 6.0f;
            // Updates the bananas position with next increment of the trajectory calculation
            ComponentPosition pos = (ComponentPosition)entityManager.FindEntity("Banana").FindComponent(ComponentTypes.COMPONENT_POSITION);
            pos.Position = new Vector3(pTrajectory.X, pTrajectory.Y, pos.Position.Z);
            // Updates the collision system with new position for checks
            pCollide.UpdatePosition(pos.Position);
            // Used to determine whether collision happened
            bool hasCollided = false;
            // If collision system has found a trigger then take action
            if (pCollide.GetEntityCollisionTrigger() != null && pCollide.GetEntityCollisionTrigger() != "")
            {
                if (pCollide.GetEntityCollisionTrigger().Contains("Gorilla"))
                {
                    SoundEffects[2].UpdateEmitterPosition(sceneManager.camera.Position);
                    SoundEffects[2].PlayAudio();
                    EndRound(pCollide.GetEntityCollisionTrigger());
                    pCollide.ClearLastDestructable();
                }
                else if (pCollide.GetEntityCollisionTrigger().Contains("Sun"))
                {
                    ComponentTexture texture = (ComponentTexture)entityManager.FindEntity("SunFace").FindComponent(ComponentTypes.COMPONENT_TEXTURE);
                    texture.Texture = ResourceManager.LoadTexture("Textures/sun_2.png");
                    hasHitSun = true;
                    pCollide.ClearLastTrigger();
                }
                else
                {
                    SoundEffects[1].UpdateEmitterPosition(pos.Position);
                    SoundEffects[1].VolumeOfAudio = 0.4f;
                    SoundEffects[1].PlayAudio();
                    // Remove all building blocks around hit point
                    DestroySkyline(pCollide.GetEntityCollisionTrigger());
                    // Clear collision system ready for next collision detection
                    pCollide.ClearLastDestructable();
                    // Updates the collision system with new banana  position
                    pos.Position = new Vector3(0.0f, -50.0f, pos.Position.Z);
                    pCollide.UpdatePosition(pos.Position);
                    // Sets collision to true so game can progress to next player
                    hasCollided = true;
                }

            }
            // If the banana has flown too long, fallen too low or has collided then progress to next player
            if (ProjectileTime >= MaxTravelTime || pTrajectory.Y < -5.0f || hasCollided)
            {
                // Used to determine which input is being entered (set to zero as current player is done)
                InputID = 0;
                // Readies time value for next trajectory
                ProjectileTime = 0.0f;
                // Switches which player is active
                if (isPlayer1) { isPlayer1 = false; } else { isPlayer1 = true; }
            }
        }
        /// <summary>
        /// Called when the game round is over and finds out which player won to adjust scores accordingly
        /// </summary>
        /// <param name="entity">Entity that was hit by the projectile</param>
        private void EndRound(string entity)
        {
            // Determines which player should reciever the score point
            // Entity names are Gorilla1 and Gorilla2 regardless of names chosen
            if (entity.Contains("1"))
            { P2Score++; }
            else if (entity.Contains("2"))
            { P1Score++; }
            // Writes the new score to file and reloads the gamescene to force a new round
            WriteGameSettings();
            sceneManager.ChangeScene(1);
        }
        #endregion
       
        #region Terrain handlers
        /// <summary>
        /// Procedually generates a building terrain using supplied values are lower and upper limits (for X and Y)
        /// </summary>
        /// <param name="pXmin">Thinest possible size for a building</param>
        /// <param name="pXmax">Tickest possible size for a building</param>
        /// <param name="pYmin">Lowest possible size for a building</param>
        /// <param name="pYmax">Highest possible size for a building</param>
        private void CreateSkyline(int pXmin, int pXmax, int pYmin, int pYmax)
        {
            // Creates the building prefab entities
            // Z pos = -25 from camera position to fill screen needs 50m long so 55-60m to cover
            float length = 27.5f;       // center is 0 so far right is half the length
            float travel = -27.5f;      // Starting point to spawn in building entities
            float Gorilla1_Y = 0.0f, Gorilla2_Y = 0.0f;     // Gorilla 1 height position and horizontal position
            float Gorilla1_X = 0.0f, Gorilla2_X = 0.0f;     // Gorilla 2 height position and horizontal position            
            float WindowScaler = 0.5f;

            Random rnd = new Random();  // Used to find a random width and height of a building

            int EntityID = 1;

            SkylineLayout = new int[(int)pYmax * 2, 64];
            
            // Array to hold all textures for new building entities
            string[,] TexturesVarients = new string[,] {
                { "building_red_off", "building_red_on" },
                { "building_blue_off", "building_blue_on" },
                { "building_gray_off", "building_gray_on" }
            };
            // Cycle through spawning in new buildings untill full screen width is filled with buildings
            while (travel < length)
            {
                float startY = -12.0f;
                // Randomise width and height of current prefab
                int width = rnd.Next(pXmin, pXmax);
                int height = rnd.Next(pYmin, pYmax);
                // This spawn the building within the boundary using the random width (as pos is center of entity)
                //travel += (float)(width);
                // Finds a spot for the gorillas far enough in from the screen edge and finds the height to place them at later on
                if (travel > -21.0f && Gorilla1_Y == 0.0f) { Gorilla1_Y = startY + (height * 2.0f); Gorilla1_X = travel + width; }
                else
                if (travel > 16.0f && Gorilla2_Y == 0.0f) { Gorilla2_Y = startY + (height * 2.0f); Gorilla2_X = travel + width; ; }

                float BuildingStickOut = (rnd.Next(1, 4) * 0.5f);
                int WindowColour = rnd.Next(3);

                /* Procedually Generate new building terrain
                 * 
                 *  For every layer up (y)
                 *      For every block across the building (x)
                 *          For every block along the building (z)
                 *              Add new block cube in that spot then more over to next
                 *              
                 *   This algorithm will work up layer by layer creating each floor of the building
                 *   for each floor it works across to the right 1m then through the building by 5m (z) (All buildings are 5m deep)
                 *   after all that it moves to the next one to the right and repeats above step
                 *   Once that layer is finished it moves up to the next until all upper limits are hit and a new building is made
                 *   It will then move to the next building and repeat until area is filled with buildings
                 */

                // Creates a holder variable which will be reused for each entity being created and overritten each time
                Entity newEntity;
                // These components are the same for every block in the terrain, therefore its not efficent to create new ones for each entity
                ComponentGeometry Geo = new ComponentGeometry("Geometry/CubeGeometry.txt");
                ComponentTransform Trans = new ComponentTransform(new Vector3(WindowScaler), new Vector3(0.0f));

                for (int y = 0; y < height / WindowScaler; y++)
                {
                    for (int x = 0; x < width / WindowScaler; x++)
                    {
                        for (int z = 0; z < 5; z++)
                        {
                            newEntity = new Entity("Building" + EntityID.ToString());
                            newEntity.AddComponent(new ComponentPosition(new Vector3(travel + x, startY + y, (-25.0f + z))));
                            newEntity.AddComponent(Trans);
                            newEntity.AddComponent(Geo);
                            // Collision detection looks at a two axis version of the map (X and Y) so adding this next line cuts out 4/5th of collision checking
                            if (z == 0){ newEntity.AddComponent(new ComponentCollider(false)); }

                            if ((z > 0 && z < 4) && (x > 0 && x < width / WindowScaler - 1))
                            { newEntity.AddComponent(new ComponentTexture("Textures/" + "building_inside" + ".jpg")); }
                            else
                            { newEntity.AddComponent(new ComponentTexture("Textures/" + TexturesVarients[WindowColour, rnd.Next(2)] + ".jpg")); }
                            
                            
                            entityManager.AddEntity(newEntity); // Adds new random generated entity to entitymanager
                            if (z == 0)
                            {
                                SkylineLayout[y, (int)((travel + length) + x)] = EntityID;
                            }
                            EntityID++;
                        }
                    }
                }

                travel += (float)width * 2;             // Readys position for next building
            }
            // The following code uses that found X and Y values to relocated the gorilla entities into a safe spot
            // This ensures they spawn ontop of a building at its height and are not floating or stuck inside a building
            ComponentPosition pos;
            ComponentTransform trans;
            pos = (ComponentPosition)entityManager.FindEntity("Gorilla1").FindComponent(ComponentTypes.COMPONENT_POSITION);
            trans = (ComponentTransform)entityManager.FindEntity("Gorilla1").FindComponent(ComponentTypes.COMPONENT_TRANSFORM);
            pos.Position = new Vector3(Gorilla1_X, Gorilla1_Y + trans.Transform.ExtractScale().Y - WindowScaler, pos.Position.Z);
            G1X = Gorilla1_X;
            G1Y = pos.Position.Y;
            pos = (ComponentPosition)entityManager.FindEntity("Gorilla2").FindComponent(ComponentTypes.COMPONENT_POSITION);
            trans = (ComponentTransform)entityManager.FindEntity("Gorilla2").FindComponent(ComponentTypes.COMPONENT_TRANSFORM);
            pos.Position = new Vector3(Gorilla2_X, Gorilla2_Y + trans.Transform.ExtractScale().Y - WindowScaler, pos.Position.Z);
            G2X = Gorilla2_X;
            G2Y = pos.Position.Y;
        }
        /// <summary>
        /// Destroyes the terrain in a sphere shape emitting for provided entity
        /// </summary>
        /// <param name="entity">Entity to emit destruction from (entity hit by projectile)</param>
        private void DestroySkyline(string entity)
        {
            // Gets the entity name/id of the center building block
            int buildingID = int.Parse(entity.Substring(8, entity.Length - 8));
            // Accesses that entities position component to get its value
            ComponentPosition pos = (ComponentPosition)entityManager.FindEntity(entity).FindComponent(ComponentTypes.COMPONENT_POSITION);
            // Uses the 2D array (SkylineLayout) and the position value the following will check for a building block ID
            // If the ID comes back as 0 then no entity is present there, anything else equals the block ID 
            // The following checks for entities that exist for the orignal entity, above, below and either side of it
            int centerID = SkylineLayout[(int)(pos.Position.Y + 12), (int)(pos.Position.X + 27.5)];
            int bottomID = SkylineLayout[(int)(pos.Position.Y + 11), (int)(pos.Position.X + 27.5)];
            int topID = SkylineLayout[(int)(pos.Position.Y + 13), (int)(pos.Position.X + 27.5)];
            int rightID = SkylineLayout[(int)(pos.Position.Y + 12), (int)(pos.Position.X + 28.5)];
            int leftID = SkylineLayout[(int)(pos.Position.Y + 12), (int)(pos.Position.X + 26.5)];
            // The following checks for the corner entities beside the ones above (forming a squre shape check)
            int bottomRID = SkylineLayout[(int)(pos.Position.Y + 11), (int)(pos.Position.X + 28.5)];
            int bottomLID = SkylineLayout[(int)(pos.Position.Y + 11), (int)(pos.Position.X + 26.5)];
            int topRID = SkylineLayout[(int)(pos.Position.Y + 13), (int)(pos.Position.X + 28.5)];
            int topLID = SkylineLayout[(int)(pos.Position.Y + 13), (int)(pos.Position.X + 26.5)];
            // The remaining checks the center entities on all four sides of the square checked above
            int outterBotID = SkylineLayout[(int)(pos.Position.Y + 10), (int)(pos.Position.X + 27.5)];
            int outterTopID = SkylineLayout[(int)(pos.Position.Y + 14), (int)(pos.Position.X + 27.5)];
            int outterRightID = SkylineLayout[(int)(pos.Position.Y + 12), (int)(pos.Position.X + 29.5)];
            int outterLeftID = SkylineLayout[(int)(pos.Position.Y + 12), (int)(pos.Position.X + 25.5)];
            /*       x
             *     x x x      This is the shape that will get cut out of the terrain
             *   x x x x x    if the corrisponding location has an entity ID in the 2D array
             *     x x x      The Z axis is handled in the loops below and choosing which
             *       x        layer will be included or ignored based on the distance from the center entity.
             */

            // This will be based to the collision system so all entities can be removed from furture checks
            List<string> names = new List<string>();
            // Removes the center block (and each entity along the Z axis from it)
            for (int i = 0; i < 5; i++)
            {
                // Step1: check if the entity exists
                if(centerID != 0)
                {
                    // Step2: remove that entity from the game scene
                    entityManager.RemoveEntity(entityManager.FindEntity("Building" + (centerID + i)));
                    // Step3: update list so system collision can be informed of the changes
                    names.Add("Building" + (centerID + i));
                }               
            }
            // Step4: Set that location in the 2D array to 0 as this entity no longer exists
            SkylineLayout[(int)(pos.Position.Y + 12), (int)(pos.Position.X + 27.5)] = 0;
            
            // The above 4 steps are repeated for each part of the destruction pattern
            // The for loops just allow for the entities along the Z axis to be affect also
            for (int i = 0; i < 3; i++)
            {
                // Handles the bottom cubes from center
                if (bottomID != 0)
                {
                    entityManager.RemoveEntity(entityManager.FindEntity("Building" + (bottomID + 2 + i)));
                    names.Add("Building" + (bottomID + 2 + i));
                    SkylineLayout[(int)(pos.Position.Y + 11), (int)(pos.Position.X + 27.5)] = 0;
                    if (bottomRID != 0 && i == 0)
                    {
                        entityManager.RemoveEntity(entityManager.FindEntity("Building" + (bottomRID + 4)));
                        SkylineLayout[(int)(pos.Position.Y + 11), (int)(pos.Position.X + 28.5)] = 0;
                        names.Add("Building" + (bottomRID + 4));
                    }
                    if (bottomLID != 0 && i == 0)
                    {
                        entityManager.RemoveEntity(entityManager.FindEntity("Building" + (bottomLID + 4)));
                        SkylineLayout[(int)(pos.Position.Y + 11), (int)(pos.Position.X + 26.5)] = 0;
                        names.Add("Building" + (bottomLID + 4));
                    }
                    if (outterBotID != 0 && i == 0)
                    {
                        entityManager.RemoveEntity(entityManager.FindEntity("Building" + (outterBotID + 4)));
                        SkylineLayout[(int)(pos.Position.Y + 10), (int)(pos.Position.X + 27.5)] = 0;
                        names.Add("Building" + (outterBotID + 4));
                    }
                }
                // Handles the top cubes from center
                if (topID != 0)
                {
                    entityManager.RemoveEntity(entityManager.FindEntity("Building" + (topID + 2 + i)));
                    SkylineLayout[(int)(pos.Position.Y + 13), (int)(pos.Position.X + 27.5)] = 0;
                    names.Add("Building" + (topID + 2 + i));
                    if (topRID != 0 && i == 0)
                    {
                        entityManager.RemoveEntity(entityManager.FindEntity("Building" + (topRID + 4)));
                        SkylineLayout[(int)(pos.Position.Y + 13), (int)(pos.Position.X + 28.5)] = 0;
                        names.Add("Building" + (topRID + 4));
                    }
                    if (topLID != 0 && i == 0)
                    {
                        entityManager.RemoveEntity(entityManager.FindEntity("Building" + (topLID + 4)));
                        SkylineLayout[(int)(pos.Position.Y + 13), (int)(pos.Position.X + 26.5)] = 0;
                        names.Add("Building" + (topLID + 4));
                    }
                    if (outterTopID != 0 && i == 0)
                    {
                        entityManager.RemoveEntity(entityManager.FindEntity("Building" + (outterTopID + 4)));
                        SkylineLayout[(int)(pos.Position.Y + 14), (int)(pos.Position.X + 27.5)] = 0;
                        names.Add("Building" + (outterTopID + 4));
                    }
                }
                // Handles the right cubes from center
                if (rightID != 0)
                {
                    entityManager.RemoveEntity(entityManager.FindEntity("Building" + (rightID + 2 + i)));
                    SkylineLayout[(int)(pos.Position.Y + 12), (int)(pos.Position.X + 28.5)] = 0;
                    names.Add("Building" + (rightID + 2 + i));
                    if (outterRightID != 0 && i == 0)
                    {
                        entityManager.RemoveEntity(entityManager.FindEntity("Building" + (outterRightID + 4)));
                        SkylineLayout[(int)(pos.Position.Y + 12), (int)(pos.Position.X + 29.5)] = 0;
                        names.Add("Building" + (outterRightID + 4));
                    }
                }
                // handles the left cubes from center
                if (leftID != 0)
                {
                    entityManager.RemoveEntity(entityManager.FindEntity("Building" + (leftID + 2 + i)));
                    SkylineLayout[(int)(pos.Position.Y + 12), (int)(pos.Position.X + 26.5)] = 0;
                    names.Add("Building" + (leftID + 2 + i));
                    if (outterLeftID != 0 && i == 0)
                    {
                        entityManager.RemoveEntity(entityManager.FindEntity("Building" + (outterLeftID + 4)));
                        SkylineLayout[(int)(pos.Position.Y + 12), (int)(pos.Position.X + 25.5)] = 0;
                        names.Add("Building" + (outterLeftID + 4));
                    }
                }
                // Gain acccess to the collider system and update it with the list of entities that have been removed
                SystemColliderXY collider = (SystemColliderXY)systemManager.FindSystem("SystemColliderXY");
                collider.RemoveDestructables(names.ToArray());
            }
        }
        #endregion

        #region ValueGenerators
        /// <summary>
        /// Generates windspeed value based on supplied values and random direction
        /// </summary>
        /// /// <param name="min">Minimum speed value</param>
        /// /// <param name="max">Maximum speed value</param>
        private float GenerateWindValue(int min, int max)
        {
            Random rnd = new Random();
            float value = rnd.Next(min, max) / 10.0f;
            float sign = rnd.Next(1, 3);
            if (sign > 1) { value *= -1.0f; }
            return value;
        }
        /// <summary>
        /// Generates random values for AI's angle and power inputs using a range that allows the chance of winning
        /// </summary>
        /// <param name="ID">Input id e.g. 0 = angle, 1 = power</param>
        private void GenerateAiInput(int ID)
        {
            Random rnd = new Random();
            switch (ID)
            {
                case 0:
                    Angle = rnd.Next(30, 71).ToString();
                    break;
                case 1:
                    if (WindSpeed > 0.0f) { Power = (rnd.Next(40, 66) / WindSpeed).ToString(); }
                    else { Power = Math.Round((rnd.Next(40, 66) * (Math.Abs(WindSpeed) + 0.3f))).ToString(); }
                    break;
            }
        }
        #endregion

        #region File loading/writing
        /// <summary>
        /// Loads in all audio files required for this game scene
        /// </summary>
        private void LoadAudioFiles()
        {
            SoundEffects = new Audio[3];
            // Id 0 = Effect to play when a player/ai intiates a throwing of a banana
            SoundEffects[0] = new Audio(ResourceManager.LoadAudio("Audio/throw.wav"));
            // Id 1 = Effect play when a banana hits/collides with a building
            SoundEffects[1] = new Audio(ResourceManager.LoadAudio("Audio/hit_building.wav"));
            // id 2 = Effect to play when a banana hits/collides with a player/ai
            SoundEffects[2] = new Audio(ResourceManager.LoadAudio("Audio/hit_gorilla.wav"));
        }
        /// <summary>
        /// Loads in all game settings into global variables 
        /// </summary>
        private void LoadGameSettings()
        {
            StreamReader r = new StreamReader("Gorillas3D.txt");
            // Starts from 1 as first line is comment
            string[] lines = r.ReadToEnd().Split('\n');
            // Used to find out whether AI is required or not
            int NumOfPlayers = int.Parse(lines[0].Substring(10, lines[0].ToString().Length - 11));
            if(NumOfPlayers > 1) { isAI = false; }
            // Load in player names
            Player1 = lines[1].Substring(10, lines[1].ToString().Length - 11);
            Player2 = lines[2].Substring(10, lines[2].ToString().Length - 11);
            // Load in number of rounds and gravity value for them
            Rounds = int.Parse(lines[3].Substring(10, lines[3].ToString().Length - 11));
            Gravity = float.Parse(lines[4].Substring(10, lines[4].ToString().Length - 11));
            // Loads in score to display on UI element
            string[] score = lines[5].Substring(10, lines[5].ToString().Length - 11).Split('-');
            P1Score = int.Parse(score[0]);
            P2Score = int.Parse(score[1]);
            // Closes file
            r.Close();
        }
        /// <summary>
        /// Writes all game settings (player names, rounds and score) to the settings file
        /// </summary>
        private void WriteGameSettings()
        {
            StreamReader r = new StreamReader("Gorillas3D.txt");
            // Starts from 1 as first line is comment
            string[] lines = r.ReadToEnd().Split('\n');
            // Updates the rounds and score based on which round just finished and the score after it
            string newRounds = lines[3].Substring(0, 10) + (Rounds - 1);
            string newScore = lines[5].Substring(0, 10) + P1Score + "-" + P2Score;
            lines[3] = newRounds;
            lines[5] = newScore;
            r.Close();
            File.WriteAllText("Gorillas3D.txt", string.Empty);
            StreamWriter w = new StreamWriter("Gorillas3D.txt");
            for (int i = 0; i < lines.Length; i++)
            {
                w.WriteLine(lines[i]);
            }
            w.Close();
        }
        #endregion

        /// <summary>
        /// Handles all keyboard inputs for this game scene
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            string temp = "";
            // Cycle through switch statement to avoid rare key value (e.g. 1 key = Number1 but need 1)
            // This also makes it numeric only which is what is needed
            if (!isAI || isPlayer1)
            {
                switch (e.Key)
                {
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
                        if (temp.Length <= 0) { break; }
                        temp = temp.Substring(0, temp.Length - 1);
                        break;
                    // Functions
                    case Key.Enter:
                        switch (InputID)
                        {
                            case 0:
                                Angle = Input;
                                Input = "";                 // Resets input value ready for next use
                                InputID++;                  // Used to determine which input is being entered (power or angle)
                                break;
                            case 1:
                                ComponentPosition pos = (ComponentPosition)entityManager.FindEntity("Banana").FindComponent(ComponentTypes.COMPONENT_POSITION);
                                Power = Input;              // Adds to current player input to power value
                                // Checks if current player to go is Player 1
                                if (isPlayer1) { pos.Position = new Vector3(G1X, G1Y, pos.Position.Z); }
                                else { pos.Position = new Vector3(G2X, G2Y, pos.Position.Z); }
                                Input = "";                 // Resets input value ready for next used
                                InputID++;
                                break;
                        }
                        break;
                    case Key.Escape:
                        sceneManager.ChangeScene(0);
                        break;

                }
            }
            Input += temp;
        }

        public override void Close()
        {
            audioContext.Dispose();
            sceneManager.Keyboard.KeyDown -= Keyboard_KeyDown;
        }
    }
}
