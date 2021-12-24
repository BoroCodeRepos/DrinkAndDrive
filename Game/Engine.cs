using SFML.Graphics;
using SFML.System;
using SFML.Window;

using System;
using System.Collections.Generic;

namespace Game
{
    /// <summary>
    /// Klasa sterująca całą logiką gry.
    /// </summary>
    class Engine
    {
        //------------------------------------------------------------------------------------
        //                          Variables of game
        //------------------------------------------------------------------------------------
        /// <summary>Obiekt zasobów gry.</summary>
        Resources resources;        
        /// <summary>Obiekt zarządzający elementami gry.</summary>
        EntitiesManager eManager;   
        /// <summary>Obiekt filtru ściemniającego pole do wyświetlenia menu.</summary>
        Shader shader;             
        /// <summary>Obiekt filtru do zmiany percepcji gracza.</summary>
        Filter filter;              
        /// <summary>Obiekt zarządzający menu gry.</summary>
        Menu menu;                  
        /// <summary>Zmienna wskazująca koniec gry.</summary>
        bool gameOver;              
        /// <summary>Zmienna wskazująca wyświetlenie modelu uszkodzeń.</summary>
        bool showDamageBoxes;       
        /// <summary>Zmienna stanu określająca przerwę w grze.</summary>
        bool pause;                 
        /// <summary>
        /// Zmienna stanu wstrzymania gry na czas wyświetlenia animacji wybuchu.
        /// </summary>
        bool wait;
        /// <summary>Zmienna trwania czasu animacji wybuchu.</summary>
        float waitTime;
        /// <summary>Zmienna wskazująca inicjalizację gry - początkowe odliczanie.</summary>
        bool initialization;       
        /// <summary>Zmienna trwania czasu inicjalizacji - poczatkowe odliczanie.</summary>
        float iniTime;                     

        //------------------------------------------------------------------------------------
        //                          Variables of alcohol level
        //------------------------------------------------------------------------------------
        /// <summary>Obiekt zliczający czas jazdy po alkoholu.</summary>
        public TimeCounter alcoTime;
        /// <summary>Obiekt zliczający czas do obliżenia stanu alkoholu.</summary>
        TimeCounter alcoTimeToStep;
        /// <summary>Zadany poziom alkoholu.</summary>
        float alcoLevel;            
        /// <summary>Aktualny stan alkoholu we krwi.</summary>
        float currLevel;           
        /// <summary>Zmienna określająca krok poziomu alkoholu.</summary>
        float alcLevelStep;      

        //------------------------------------------------------------------------------------
        //                          Variables of game result
        //------------------------------------------------------------------------------------
        /// <summary>Obiekt zliczający czas gry.</summary>
        public TimeCounter gameTime;
        /// <summary>Aktualny wynik gracza.</summary>
        public int score;           
        /// <summary>Aktualna ilość żyć gracza.</summary>
        int lives;                              
        /// <summary>Aktualna prędkość gracza.</summary>
        float speed;                
        /// <summary>Zmienna prędkości inicjalizacyjnej gracza.</summary>
        float startSpeed;           
        
        //------------------------------------------------------------------------------------
        //                          Constructor
        //------------------------------------------------------------------------------------
        /// <summary>Konstruktor silnika gry - inicjalizacja obiektów.</summary>
        /// <param name="resources">Referencja do obiektu zasobów gry.</param>
        public Engine(Resources resources)
        {
            // określenie odtwarzanej muzyki.
            resources.sounds["start"].Play();
            resources.sounds["menu_music"].Loop = true;
            resources.sounds["bg_sound"].Loop = true;
            resources.sounds["traffic_noise"].Loop = true;
            resources.sounds["engine_sound"].Loop = true;
            resources.sounds["engine_sound"].Play();
            // inicjalizacja podstawowych parametrów
            initialization = true;
            gameOver = false;
            iniTime = 0f;
            this.resources = resources;
            try
            {
                // próba utworzenia elementów gry
                InitGameResources();
                InitAlcoVars();
                InitEntitesManager();
                InitPlayerCar();
            }
            catch(Exception exception)
            {
                // komunikacja o niepowodzeniu
                Program.ShowError(exception);
            }
        }

        //------------------------------------------------------------------------------------
        //                          Initialization methods
        //------------------------------------------------------------------------------------
        /// <summary>Metoda inicjalizująca wewnętrzne zasoby gry.</summary>
        private void InitGameResources()
        {
            score = 0;
            gameOver = false;
            showDamageBoxes = false;
            pause = false;
            startSpeed = 0.1f;
            speed = startSpeed;
            lives = 3;
            wait = false;
            waitTime = 0f;
            Animation.states = new List<AnimationState>();
            Animation.texture = resources.Texplosion;
            Animation.tSize = 96;

            shader = new Shader(resources.options.winWidth, resources.options.winHeight, 210, true);
            shader.SetUp(210, 1, .005f);

            filter = new Filter(resources.Tfilter);

            gameTime = new TimeCounter();
            menu = new Menu(this, resources);
        }

        /// <summary>Metoda inicjalizująca zmienne dotyczące poziomu alkoholu.</summary>
        private void InitAlcoVars()
        {
            alcoTime = new TimeCounter();
            alcoTimeToStep = new TimeCounter();
            alcoTimeToStep.SetEventTime(5d);
            alcoLevel = 0f;
            currLevel = 0f;
            alcLevelStep = 0.001f;
        }

        /// <summary>Metoda inicjalizująca managera elementów gry.</summary>
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

        /// <summary>Metoda inicjalizująca pojazd gracza.</summary>
        private void InitPlayerCar()
        {
            Random rnd = new Random();
            eManager.SetPlayerCarById(rnd.Next(5));
        }

        //------------------------------------------------------------------------------------
        //                          Update game methods
        //------------------------------------------------------------------------------------
        /// <summary>
        /// Głowna metoda aktualizująca stan gry.
        /// </summary>
        /// <param name="dt">Czas od poprzedniego wywołania.</param>
        /// <param name="window">Referencja do obiektu głównego okna gry.</param>
        public void Update(float dt, ref RenderWindow window)
        {
            // aktualizacja zasobów
            UpdateInitialization(dt);
            UpdateHeartChances();

            if (!pause && !gameOver && !wait)
            {
                UpdateBackground(dt);
                gameTime.Update(dt);
            }
            else menu.Update(ref window);
            
            // aktualizacja elementów gry
            Animation.Update(dt);
            eManager.UpdateAnimation(dt);
            if (!initialization && !pause && !gameOver && !wait)
                eManager.Update(dt, speed, CalcOffset());
            
            shader.Update(dt);
            filter.Update(eManager.mainCar.GetPosition(), eManager.mainCar.GetSize());

            // aktualizacja liczników
            UpdateAlcoTime(dt);
            UpdateLosingLife(dt);
        }

        /// <summary>
        /// Metoda aktualizująca odliczanie po starcie gry.
        /// </summary>
        /// <param name="dt">Czas od poprzedniego wywołania.</param>
        private void UpdateInitialization(float dt)
        {
            if (initialization)
            {
                iniTime += dt / 1000f;

                if (iniTime > 2f)
                    shader.SetState(STATE.CLOSING);

                if (iniTime >= 4f)
                {
                    resources.sounds["traffic_noise"].Play();
                    resources.sounds["bg_sound"].Play();
                    initialization = false;
                    gameTime.Start();
                }
            }
        }

        /// <summary>
        /// Metoda aktualizująca tło (jezdnię) - ruch w osi X i Y.
        /// </summary>
        /// <param name="dt">Czas od poprzedniego wywołania.</param>
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

        /// <summary>
        /// Metoda aktualizująca czas jazdy pod wpływem alkoholu.
        /// </summary>
        /// <param name="dt">Czas od poprzedniego wywołania.</param>
        private void UpdateAlcoTime(float dt)
        {
            if (currLevel > alcoLevel)
            {
                currLevel -= alcLevelStep;
                if (currLevel < alcoLevel)
                    currLevel = alcoLevel;
            }
            else if (currLevel < alcoLevel)
            {
                currLevel += alcLevelStep;
                if (currLevel > alcoLevel)
                    currLevel = alcoLevel;
            }

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
                    filter.SetVisible(false);
                }
                filter.CalcScale(currLevel);
            }
        }

        /// <summary>
        /// Metoda aktualizująca czas wstrzymania elementów gry - animacja wybuchu.
        /// </summary>
        /// <param name="dt">Czas od poprzedniego wywołania.</param>
        private void UpdateLosingLife(float dt)
        {
            if (wait)
            {
                waitTime += dt;
                if (waitTime >= Animation.states[0].maxFrameTime * Animation.states[0].framesNr)
                {
                    wait = false;
                    Animation.states.Clear();
                    eManager.ClearAll();
                    eManager.mainCar.SetPosition(
                        resources.background.Texture.Size.X / 2f - resources.background.Origin.X,
                        resources.background.Texture.Size.Y / 2f
                    );
                    eManager.mainCar.movement.velocity = new Vector2f(0f, 0f);
                    eManager.mainCar.SetRotation(0f);
                    filter.Update(eManager.mainCar.GetPosition(), eManager.mainCar.GetSize());
                }
            }
            else
            {
                waitTime = 0f;
            }
        }

        /// <summary>Metoda aktualizująca szanse na utworzenie elementu serca.</summary>
        private void UpdateHeartChances()
        {
            if (lives < 3)
                eManager.heartPercent = 1e-2d;
            else
                eManager.heartPercent = 0f;
        }

        //------------------------------------------------------------------------------------
        //                          Render elements on screen
        //------------------------------------------------------------------------------------
        /// <summary>
        /// Główna metoda renderująca wszystkie obiekty na wskazanym oknie.
        /// </summary>
        /// <param name="window">Referencja do głównego okna gry.</param>
        public  void Render(ref RenderWindow window)
        {
            // wyswietlenie tła
            RenderBackground(ref window);

            // wyświetlenie elementów gry
            eManager.RenderEntities(ref window);
            eManager.RenderDamageBoxes(ref window, showDamageBoxes);
            Animation.Render(ref window);

            // wyświetlenie interfejsu
            RenderFilter(ref window);
            RenderInterface(ref window);
            RenderShader(ref window);
            RenderInitCounting(ref window);
            RenderMenu(ref window);
        }

        /// <summary>
        /// Metoda renderująca tło gry - jezdnię.
        /// </summary>
        /// <param name="window">Referencja do głównego okna gry.</param>
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

        /// <summary>
        /// Metoda renderująca filtr zmiany percepcji.
        /// </summary>
        /// <param name="window">Referencja do głównego okna gry.</param>
        private void RenderFilter(ref RenderWindow window)
        {
            filter.Render(ref window);
        }

        /// <summary>
        /// Metoda renderująca interfejs gry (wynik, ilość żyć i poziom alkoholu).
        /// </summary>
        /// <param name="window">Referencja do głównego okna gry.</param>
        private void RenderInterface(ref RenderWindow window)
        {
            // score
            window.Draw(resources.coins);
            string scoreStr = score.ToString();
            for (int i = 0; i < scoreStr.Length; i++)
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

            // alcohol level
            Sprite bottleCap = new Sprite(resources.GetTexture(TYPE.BEER), eManager.animation[TYPE.BEER].currentFrame)
            {
                Position = new Vector2f(10f, 585f),
            };
            window.Draw(bottleCap);
            List<float> posX = new List<float> { 10f, 50f, 100f, 170f };
            //float dispLevel = (alcoLevel > 10f) ? 9.99f : alcoLevel ;
            float dispLevel = (currLevel > 10f) ? 9.99f : currLevel;
            string strLevel = string.Format("{0:0.00}", dispLevel);
            for (int i = 0; i < 4; i++)
            {
                char c = strLevel[i];
                int result = (c == ',' || c == '.') ? 10 : Convert.ToInt32(c.ToString());
                Sprite num = new Sprite(resources.numbers[result])
                {
                    Position = new Vector2f(posX[i], 680f)
                };
                window.Draw(num);
            }
        }

        /// <summary>
        /// Metoda renderująca filtr przyciemnienia ekranu.
        /// </summary>
        /// <param name="window">Referencja do głównego okna gry.</param>
        private void RenderShader(ref RenderWindow window)
        {
            window.Draw(shader.GetShape());
        }

        /// <summary>
        /// Metoda renderująca odliczanie po starcie gry.
        /// </summary>
        /// <param name="window">Referencja do głównego okna gry.</param>
        private void RenderInitCounting(ref RenderWindow window)
        {
            if (initialization)
            {
                int nr = 3 - (int)iniTime;
                IntRect origin = resources.numbers[nr].TextureRect;
                Sprite number = new Sprite(resources.numbers[nr])
                {
                    Origin = new Vector2f(origin.Width / 2f, origin.Height / 2f),
                    Position = new Vector2f(resources.options.winWidth / 2f, resources.options.winHeight / 2f),
                    Scale = new Vector2f(4f, 4f)
                };
                window.Draw(number);
            }
        }

        /// <summary>
        /// Metoda renderująca menu.
        /// </summary>
        /// <param name="window">Referencja do głównego okna gry.</param>
        private void RenderMenu(ref RenderWindow window)
        {
            if (pause || gameOver)
            {
                menu.Render(ref window);

                double time = gameTime.GetCurrentTime();

                if (time > 0d && !gameOver)
                {
                    string str = string.Format(" Game Time: {0:0.00} sec", time);
                    Text text = new Text(str, resources.font, 25);
                    window.Draw(text);
                }
            }
        }

        //------------------------------------------------------------------------------------
        //                          Supporting methods
        //------------------------------------------------------------------------------------
        /// <summary>
        /// Metoda obsługująca zdarzenie naciśnięcia przycisku na klawiaturze.
        /// </summary>
        /// <param name="sender">Obiekt, na którym wystąpiło zdarzenie.</param>
        /// <param name="key">Argument zdarzenia.</param>
        public void OnKeyPressed(object sender, KeyEventArgs key)
        {
            if (key.Code == Keyboard.Key.B)
                showDamageBoxes = (!showDamageBoxes);

            if (key.Code == Keyboard.Key.P || key.Code == Keyboard.Key.Escape)
            {
                if (!initialization && !gameOver)
                {
                    pause = (!pause);
                    shader.SetUp(210, 10, .002f);
                    if (pause)
                    {
                        resources.sounds["traffic_noise"].Stop();
                        resources.sounds["bg_sound"].Stop();
                        resources.sounds["menu_music"].Play();
                        resources.sounds["menu_open"].Play();
                        shader.SetState(STATE.OPENING);
                        menu.InitMainMenu();
                    }
                    else
                    {
                        resources.sounds["menu_music"].Stop();
                        resources.sounds["menu_close"].Play();
                        resources.sounds["traffic_noise"].Play();
                        resources.sounds["bg_sound"].Play();
                        shader.SetState(STATE.CLOSING);
                    }
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

        /// <summary>
        /// Metoda obsługująca zdarzenie zwoleniani przycisku z klawiatury.
        /// </summary>
        /// <param name="sender">Obiekt, na którym wystąpiło zdarzenie.</param>
        /// <param name="key">Argument zdarzenia.</param>
        public void OnKeyReleased(object sender, KeyEventArgs key)
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

        /// <summary>
        /// Metoda obsługująca zdarzenie zamknięcia głównego okna gry.
        /// </summary>
        /// <param name="sender">Obiekt, na którym wystąpiło zdarzenie.</param>
        /// <param name="e">Argument zdarzenia.</param>
        public void OnClose(object sender, EventArgs e)
        {
            RenderWindow window = (RenderWindow)sender;
            window.Close();
        }

        /// <summary>Metoda obsługująca kontynuację gry.</summary>
        public void OnContinue()
        {
            pause = (!pause);
            resources.sounds["menu_music"].Stop();
            resources.sounds["traffic_noise"].Play();
            resources.sounds["bg_sound"].Play();
            shader.SetUp(210, 10, .002f);
            shader.SetState(STATE.CLOSING);
        }

        /// <summary>Metoda obsługująca zamknięcie gry.</summary>
        public void OnExit()
        {
            Program.window.Close();
        }

        /// <summary>Metoda obsługująca ponowny start gry.</summary>
        public void OnStartAgain()
        {
            initialization = true;
            gameOver = false;
            pause = false;
            iniTime = 0f;
            alcoLevel = 0f;
            score = 0;
            speed = startSpeed;
            shader.SetUp(210, 1, .005f);
            eManager.ClearAll();
            InitPlayerCar();
            alcoTime.ClearTime();
            alcoTime.Stop();
            alcoTimeToStep.ClearTime();
            alcoTimeToStep.Stop();
            gameTime.ClearTime();
            gameTime.Stop();
            filter.SetVisible(false);
            resources.sounds["start"].Play();
            resources.sounds["menu_music"].Stop();
        }

        /// <summary>
        /// Metoda obsługująca wybór pojazdu.
        /// </summary>
        /// <param name="id">Identyfikator wybranego pojazdu.</param>
        public void OnSelectCar(int id)
        {
            pause = false;
            eManager.ClearAll();
            eManager.SetPlayerCarById(id);
            initialization = true;
            iniTime = 0f;
            shader.SetUp(210, 1, .005f);
            resources.sounds["start"].Play();
            resources.sounds["menu_music"].Stop();
        }

        /// <summary>
        /// Metoda wyznaczająca przesunięcie elementów gry ze względu na poziom alkoholu.
        /// </summary>
        public float CalcOffset()
        {
            double time = alcoTime.GetCurrentTime();
            float amp = 10 * (currLevel + score / 1000f) % resources.background.Origin.X;
            float sin_func = (float)Math.Sin(currLevel * time);//% (2d * Math.PI)
            return amp * sin_func;
        }

        /// <summary>
        /// Metoda obsługująca zdarzenie kolizji pojazdu gracza z elementem serca.
        /// </summary>
        /// <param name="e">Wskazanie elementu, z którym zaszła kolizja.</param>
        private void OnHeartCollision(Entity e)
        {
            if (lives < 3)
            {
                resources.sounds["picked_heart"].Play();
                lives++;
                e.SetEffect();
            }
        }

        /// <summary>
        /// Metoda obsługująca zdarzenie kolizji pojazdu gracza z elementem monety.
        /// </summary>
        /// <param name="e">Wskazanie elementu, z którym zaszła kolizja.</param>
        private void OnCoinCollision(Entity e)
        {
            resources.sounds["picked_coin"].Play();
            score++;
            speed = startSpeed + score * 1E-10f;
            e.SetEffect();
        }

        /// <summary>
        /// Metoda obsługująca zdarzenie kolizji pojazdu gracza z elementem kapsla.
        /// </summary>
        /// <param name="e">Wskazanie elementu, z którym zaszła kolizja.</param>
        private void OnBeerCollision(Entity e)
        {
            resources.sounds["picked_cap"].Play();
            alcoLevel += 0.4f + score / 1000f;
            alcoTime.Start();
            alcoTimeToStep.Start();
            if (!filter.GetVisible())
            {
                filter.SetVisible(true);
            }
            else
            {
                filter.CalcScale(currLevel);
            }
            e.SetEffect();
        }

        /// <summary>
        /// Metoda obsługująca zdarzenie kolizji pojazdu gracza z elementem innego pojazdu.
        /// </summary>
        /// <param name="e">Wskazanie elementu, z którym zaszła kolizja.</param>
        private void OnCarCollision(Entity e)
        {
            resources.sounds["car_crash2"].Play();

            var offset = ((e.dir == DIRECTION.DOWN) ? 0f : e.GetSize().Y);
            Vector2f posA = e.GetPosition();
            Vector2f posB = eManager.mainCar.GetPosition();
            Vector2f posResult = new Vector2f(
                (posA.X + posB.X) / 2f, 
                (posA.Y + offset - posB.Y) / 2f + posB.Y
            );
            Animation.Create(posResult);
            e.SetEffect();
            LoseLife();
        }

        /// <summary>
        /// Metoda obsługująca zdarzenie kolizji pojazdu gracza z niedozwolonym obszarem mapy.
        /// </summary>
        /// <param name="e">Wskazanie elementu, z którym zaszła kolizja.</param>
        private void OnMapCollision(Entity e)
        {
            LoseLife();
            Vector2f pos = eManager.mainCar.GetPosition();
            Vector2f size = eManager.mainCar.GetSize();
            if (pos.X < resources.options.winWidth / 2f)
                size.X *= (-1f);
            Vector2f res = new Vector2f(pos.X + size.X / 2f, pos.Y);
            Animation.Create(res);
            resources.sounds["car_crash"].Play();
        }

        /// <summary>Metoda wywołująca utratę życia.</summary>
        private void LoseLife()
        {
            lives--;
            if (lives == 0)
            {
                gameOver = true;
                shader.SetUp(210, 10, .002f);
                shader.SetState(STATE.OPENING);
                menu.InitGameResult();
            }
            else
            {
                wait = true;
            }
        }
    }
}
