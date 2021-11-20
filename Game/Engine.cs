using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Game
{
    class Engine
    {
        //------------------------------------------------------------------------------------
        //                          Variables of game
        //------------------------------------------------------------------------------------
        Resources resources;        // zasoby gry
        EntitiesManager eManager;   // manager elementów gry
        Shader shader;              // shader object
        bool showDamageBoxes;       // true <= pokazuje modele uszkodzeń samochodów i jezdni
        bool pause;                 // true <= pauza
        bool gameOver;              // koniec gry <= wyświetlenie wyników
        bool initialization;        // inicjalizacja gry
        float iniTime;              // czas inicjalizacji

        //------------------------------------------------------------------------------------
        //                          Variables of alcohol level
        //------------------------------------------------------------------------------------
        TimeCounter alcoTime;       // czas jazdy po alkoholu
        TimeCounter alcoTimeToStep; // czas do obniżenia alkoholu we krwi
        float alcoLevel;            // aktualny poziom alkoholu we krwi

        //------------------------------------------------------------------------------------
        //                          Variables of game result
        //------------------------------------------------------------------------------------
        TimeCounter gameTime;       // czas gry
        int lives;                  // ilość żyć
        int level;                  // aktualny poziom gry
        int score;                  // aktualny wynik gracza
        float speed;                // szybkość przesuwania się drogi po ekranie
        
        //------------------------------------------------------------------------------------
        //                          Constructor
        //------------------------------------------------------------------------------------
        public Engine(Resources resources)
        {
            initialization = true;
            gameOver = false;
            iniTime = -0.7f;
            this.resources = resources;
            try
            {
                InitGameResources();
                InitAlcoVars();
                InitEntitesManager();
                InitPlayerCar();
            }
            catch(Exception exception)
            {
                Program.ShowError(exception);
            }
        }

        //------------------------------------------------------------------------------------
        //                          Initialization methods
        //------------------------------------------------------------------------------------
        private void InitGameResources()
        {
            score = 0;
            level = 1;
            showDamageBoxes = false;
            pause = false;
            speed = .1f;
            lives = 3;

            shader = new Shader(resources.options.winWidth, resources.options.winHeight, true);
            shader.SetUp(255, 1, .011f);
            shader.SetState(STATE.CLOSING);

            gameTime = new TimeCounter();
        }

        private void InitAlcoVars()
        {
            XmlElement root = resources.document.DocumentElement;
            XmlElement soberingUpElement = root["game"]["sobering_up"];
            //alcoLevelStep = float.Parse(
            //    soberingUpElement.Attributes["step_alkohol_level"].Value
            //);
            alcoTime = new TimeCounter();
            alcoTimeToStep = new TimeCounter(1d);
            alcoTimeToStep.SetEventTime(
                float.Parse(soberingUpElement.Attributes["step_time"].Value)
            );
            alcoLevel = 0f;
        }

        private void InitEntitesManager()
        {
            eManager = new EntitiesManager(resources)
            {
                onHeartCollision = new EntitiesManager.OnCollision(OnHeartCollision),
                onCoinCollision  = new EntitiesManager.OnCollision(OnCoinCollision),
                onBeerCollision  = new EntitiesManager.OnCollision(OnBeerCollision),
                onCarCollision   = new EntitiesManager.OnCollision(OnCarCollision),
                onMapCollision   = new EntitiesManager.OnCollision(OnMapCollision)
            };
        }

        private void InitPlayerCar()
        {
            eManager.SetPlayerCarById(2);
        }

        //------------------------------------------------------------------------------------
        //                          Update game methods
        //------------------------------------------------------------------------------------
        public  void Update(float dt)
        {
            // aktualizacja zasobów
            UpdateInitialization(dt);
            UpdateBackground(dt);
            gameTime.Update(dt);
            alcoTime.Update(dt);

            // aktualizacja elementów gry
            eManager.UpdateAnimation(dt);
            if (!initialization && !pause && !gameOver)
                eManager.Update(dt, speed, CalcOffset());
            shader.Update(dt);

            // aktualizacja liczników
            UpdateAlcoTime(dt);
        }

        private void UpdateInitialization(float dt)
        {
            if (initialization)
            {
                iniTime += dt / 1000f;

                if (iniTime >= 4f)
                {
                    initialization = false;
                    gameTime.Start();
                }
            }
        }

        private void UpdateBackground(float dt)
        {
            Vector2f curr_pos = resources.background.Position;
            resources.background.Position = new Vector2f(
                CalcOffset(),
                (curr_pos.Y + dt * speed) % resources.background.Texture.Size.Y
            );
            resources.dmgBoxL.Position = new Vector2f(
                resources.background.Position.X, 0f
            );
            resources.dmgBoxR.Position = new Vector2f(
                resources.background.Position.X, 0f
            );
        }

        private void UpdateGameTime(float dt)
        {
            gameTime.Update(dt);
        }

        private void UpdateAlcoTime(float dt)
        {
            alcoTime.Update(dt);
            alcoTimeToStep.Update(dt);
            if (alcoTimeToStep.GetEventStatus())
            {
                alcoTimeToStep.ClearEventStatus();
                alcoLevel -= 0.1f;
                if (alcoLevel <= 0f)
                {
                    alcoLevel = 0f;
                    alcoTime.Stop();
                    alcoTimeToStep.Stop();
                    alcoTimeToStep.ClearTime();
                }
            }
        }

        //------------------------------------------------------------------------------------
        //                          Render elements on screen
        //------------------------------------------------------------------------------------
        public  void Render(ref RenderWindow window)
        {
            // wyswietlenie tła
            RenderBackground(ref window);

            // wyświetlenie elementów gry
            eManager.RenderEntities(ref window);
            eManager.RenderDamageBoxes(ref window, showDamageBoxes);

            // wyświetlenie interfejsu
            RenderInterface(ref window);
            RenderShader(ref window);
            RenderInitCounting(ref window);
            RenderPauseMenu(ref window);
        }

        private void RenderBackground(ref RenderWindow window)
        {
            Vector2f curr_pos = resources.background.Position;
            Vector2f temp_pos = curr_pos;
            // with current position
            window.Draw(resources.background);
            // with position - size.Y
            resources.background.Position = new Vector2f(
                curr_pos.X,
                curr_pos.Y - resources.background.Texture.Size.Y
            );
            window.Draw(resources.background);
            // with position + size.Y
            resources.background.Position = new Vector2f(
                curr_pos.X,
                curr_pos.Y + resources.background.Texture.Size.Y
            );
            window.Draw(resources.background);
            resources.background.Position = curr_pos;
        }

        private void RenderInterface(ref RenderWindow window)
        {
            // score
            window.Draw(resources.coins);
            string scoreStr = score.ToString();
            int N = scoreStr.Length;
            for (int i = 0; i < N; i++)
            {
                char c = scoreStr[i];
                int result = Convert.ToInt16(c.ToString());
                Sprite num = new Sprite(resources.numbers[result])
                {
                    Position = new Vector2f(150f + (float)i * 64f, 25f)
                };
                window.Draw(num);
            }

            // hearts
            TYPE type = TYPE.HEART;
            Sprite heart = new Sprite(resources.GetTexture(type), eManager.animation[type].currentFrame);
            for (int i = 0; i < lives; i++)
            {
                heart.Position = new Vector2f(
                    (float)resources.options.winWidth - 90f,
                    10f + i * 80f
                );
                window.Draw(heart);
            }
        }

        private void RenderShader(ref RenderWindow window)
        {
            window.Draw(shader.GetShape());
        }

        private void RenderInitCounting(ref RenderWindow window)
        {
            if (initialization)
            {
                int nr = 3 - (int)iniTime;
                IntRect origin = resources.numbers[nr].TextureRect;
                Console.WriteLine("{0}", origin);
                Sprite number = new Sprite(resources.numbers[nr])
                {
                    Origin = new Vector2f(origin.Width / 2f, origin.Height / 2f),
                    Position = new Vector2f(resources.options.winWidth / 2f, resources.options.winHeight / 2f),
                    Scale = new Vector2f(4f, 4f)
                };
                window.Draw(number);
            }
        }

        private void RenderPauseMenu(ref RenderWindow window)
        {
            if (pause)
            {

            }
        }

        //------------------------------------------------------------------------------------
        //                          Supporting methods
        //------------------------------------------------------------------------------------
        public  void OnKeyPressed(object sender, KeyEventArgs key)
        {
            RenderWindow window = (RenderWindow)sender;

            if (key.Code == Keyboard.Key.B)
                showDamageBoxes = (!showDamageBoxes);

            if (key.Code == Keyboard.Key.P || key.Code == Keyboard.Key.Escape)
                if (true)
                {
                    pause = (!pause);
                    shader.SetUp(210, 10, .002f);
                    if (pause)
                    {
                        shader.SetState(STATE.OPENING);
                        gameTime.Stop();
                    }
                    else
                    {
                        shader.SetState(STATE.CLOSING);
                        gameTime.Start();
                    }
                }

            if (key.Code == Keyboard.Key.A || key.Code == Keyboard.Key.Left)
                eManager.mainCar.DirectionMove(-1, 0);

            if (key.Code == Keyboard.Key.D || key.Code == Keyboard.Key.Right)
                eManager.mainCar.DirectionMove(1, 0);

            if (key.Code == Keyboard.Key.W || key.Code == Keyboard.Key.Up)
                eManager.mainCar.DirectionMove(0, -1);

            if (key.Code == Keyboard.Key.S || key.Code == Keyboard.Key.Down)
                eManager.mainCar.DirectionMove(0, 1);
        }

        public  void OnKeyReleased(object sender, KeyEventArgs key)
        {
            if (key.Code == Keyboard.Key.A || key.Code == Keyboard.Key.Left)
                eManager.mainCar.DirectionMove(1, 0);

            if (key.Code == Keyboard.Key.D || key.Code == Keyboard.Key.Right)
                eManager.mainCar.DirectionMove(-1, 0);

            if (key.Code == Keyboard.Key.W || key.Code == Keyboard.Key.Up)
                eManager.mainCar.DirectionMove(0, 1);

            if (key.Code == Keyboard.Key.S || key.Code == Keyboard.Key.Down)
                eManager.mainCar.DirectionMove(0, -1);
        }

        public  void OnClose(object sender, EventArgs e)
        {
            RenderWindow window = (RenderWindow)sender;
            window.Close();
        }

        public float CalcOffset()
        {
            double time = alcoTime.GetCurrentTime() / 1000d;
            float amp = 10 * (alcoLevel + level / 1000f) % resources.background.Origin.X;
            float sin_func = (float)Math.Sin((alcoLevel * time) % (2d * Math.PI));
            return amp * sin_func;
        }

        private bool OnHeartCollision()
        {
            if (lives < 3)
            {
                lives++;
                return true;
            }
            return false;
        }

        private bool OnCoinCollision()
        {
            score++;
            speed += score * 0.0001f;
            return true;
        }

        private bool OnBeerCollision()
        {
            alcoLevel += 0.4f + score / 1000f;
            alcoTime.Start();
            alcoTimeToStep.Start();
            alcoTimeToStep.ClearTime();
            
            return true;
        }

        private bool OnCarCollision()
        {
            //LoseLive();
            return true;
        }

        private bool OnMapCollision()
        {
            LoseLive();
            return false;
        }

        private void LoseLive()
        {
            lives--;
            if (lives == 0)
            {
                lives++;
            }
            else
            {
                eManager.ClearAll();
                eManager.mainCar.SetPosition(
                    resources.background.Texture.Size.X / 2f - resources.background.Origin.X,
                    resources.background.Texture.Size.Y / 2f   
                );
                eManager.mainCar.movement.velocity = new Vector2f(0f, 0f);
                eManager.mainCar.SetRotation(0f);
            }
        }
    }
}
