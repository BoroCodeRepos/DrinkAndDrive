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
        private struct Animation
        {
            public int currentFrameId;
            public float currentFrameTime;

            public int framesNr;
            public float maxFrameTime;

            public IntRect currentFrame;
        }

        //------------------------------------------------------------------------------------
        //                          Variables of game
        //------------------------------------------------------------------------------------
        Resources resources;        // zasoby gry
        List<Entity> hearts;        // kolekcja serc na jezdni
        List<Entity> coins;         // kolekcja coinsów na jezdni
        List<Entity> beers;         // kolekcja butelek na jezdni
        List<Entity> cars;          // kolekcja pojazdów na jezdni
        Entity mainCar;             // samochód główny - gracza
        Animation coinsAnimation,   // animacja monet, butelek i serc
            beersAnimation,
            heartsAnimation;
        bool showDamageBoxes;       // true <= pokazuje modele uszkodzeń samochodów i jezdni
        bool pause;                 // true <= pauza

        //------------------------------------------------------------------------------------
        //                          Variables of alcohol level
        //------------------------------------------------------------------------------------
        TimeCounter alcoTime;       // czas jazdy po alkoholu
        TimeCounter alcoTimeToStep; // czas do obniżenia alkoholu we krwi
        float alcoLevelStep;        // krok obniżenia alkoholu
        float alcoLevel;            // aktualny poziom alkoholu we krwi

        //------------------------------------------------------------------------------------
        //                          Variables of game result
        //------------------------------------------------------------------------------------
        TimeCounter gameTime;       // czas gry
        int level;                // aktualny poziom gry
        int score;                // aktualny wynik gracza
        float speed;                // szybkość przesuwania się drogi po ekranie
        int  lives;                // ilość żyć

        //------------------------------------------------------------------------------------
        //                          Constructor
        //------------------------------------------------------------------------------------
        public Engine(Resources resources)
        {
            this.resources = resources;
            try
            {
                InitGameResources();
                InitAnimation("coins",  ref coinsAnimation);
                InitAnimation("beers",  ref beersAnimation);
                InitAnimation("hearts", ref heartsAnimation);
                InitAlcoVars();
                InitCars();
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

            gameTime = new TimeCounter();
            alcoTime = new TimeCounter();
            hearts = new List<Entity>();
            coins  = new List<Entity>();
            beers  = new List<Entity>();
            cars   = new List<Entity>();
            coinsAnimation  = new Animation();
            beersAnimation  = new Animation();
            heartsAnimation = new Animation();
        }

        private void InitAnimation(string attrName, ref Animation animation)
        {
            XmlElement root = resources.document.DocumentElement;
            XmlElement element = root["game"]["animation"][attrName];
            animation.framesNr = Convert.ToInt16(element.Attributes["frame_nr"].Value);
            animation.maxFrameTime = float.Parse(element.Attributes["max_frame_time"].Value);
            int width = Convert.ToInt16(element.Attributes["width"].Value);
            int height = Convert.ToInt16(element.Attributes["height"].Value);
            animation.currentFrame = new IntRect(0, 0, width, height);
            animation.currentFrameId = 0;
            animation.currentFrameTime = 0f;
        }

        private void InitAlcoVars()
        {
            XmlElement root = resources.document.DocumentElement;
            XmlElement soberingUpElement = root["game"]["sobering_up"];
            alcoLevelStep = float.Parse(
                soberingUpElement.Attributes["step_alkohol_level"].Value
            );
            alcoTimeToStep = new TimeCounter();
            alcoTimeToStep.SetEventTime(
                float.Parse(soberingUpElement.Attributes["step_time"].Value)
            );
            alcoLevel = 0.8f;
        }

        private void InitCars()
        {
            XmlNode movement = resources.document.GetElementsByTagName("advanced_move")[0];
            mainCar = new Entity(9, resources.carCollection);
            mainCar.damageBox.FillColor = new Color(0, 255, 255, 128);
            mainCar.SetPosition(
                resources.background.Texture.Size.X /2f - resources.background.Origin.X,
                resources.background.Texture.Size.Y / 2f
            );
        }

        //------------------------------------------------------------------------------------
        //                          Update game methods
        //------------------------------------------------------------------------------------
        public  void Update(float dt)
        {
            // aktualizacja zasobów
            gameTime.Update(dt);
            alcoTime.Update(dt);
            UpdateBackground(dt);

            // aktualizacja animacji
            UpdateAnimation(dt, ref coinsAnimation, ref coins);
            UpdateAnimation(dt, ref beersAnimation, ref beers);
            UpdateAnimation(dt, ref heartsAnimation, ref hearts);

            // aktualizacja pojazdów
            mainCar.Update(dt);

            // aktualizacja koliji
            UpdateCollisions();

            // wyjazd poza obszar mapy
            UpdateMapBounds();
        }

        private void UpdateBackground(float dt)
        {
            double time = alcoTime.GetCurrentTime() / 1000d;
            float amp = 10 * (alcoLevel + level / 1000f) % resources.background.Origin.X;
            float sin_func = (float)Math.Sin((alcoLevel * time) % 2d * Math.PI);
            Vector2f curr_pos = resources.background.Position;
            resources.background.Position = new Vector2f(
                amp * sin_func,
                (curr_pos.Y + dt * speed) % resources.background.Texture.Size.Y
            );
            resources.dmgBoxL.Position = new Vector2f(
                resources.background.Position.X, 0f
            );
            resources.dmgBoxR.Position = new Vector2f(
                resources.background.Position.X, 0f
            );
        }

        private void UpdateAnimation(float dt, ref Animation animation, ref List<Entity> itemList)
        {
            animation.currentFrameTime += dt;

            if (animation.currentFrameTime >= animation.maxFrameTime)
            {
                animation.currentFrameTime = 0f;
                animation.currentFrameId += 1;
                if (animation.currentFrameId >= animation.framesNr)
                    animation.currentFrameId = 0;
            }
                
            animation.currentFrame.Left = animation.currentFrameId * animation.currentFrame.Width;

            foreach (Entity item in itemList)
                item.sprite.TextureRect = animation.currentFrame;
        }

        private void UpdateCollisions()
        {
            FloatRect carRect = mainCar.damageBox.GetGlobalBounds();
            FloatRect bgRectL = resources.dmgBoxL.GetGlobalBounds();
            FloatRect bgRectR = resources.dmgBoxR.GetGlobalBounds();
            if (carRect.Intersects(bgRectL) || carRect.Intersects(bgRectR))
                LoseLive();

            foreach (Entity car in cars)
                if (CollisionsCheck(car, mainCar))
                    LoseLive();
        }

        private void UpdateMapBounds()
        {
            FloatRect top = new FloatRect(
                new Vector2f(0f, 0f),
                new Vector2f((float)resources.options.winWidth, 1f)
            );
            FloatRect bottom = new FloatRect(
                new Vector2f(0f, (float)resources.options.winHeight),
                new Vector2f((float)resources.options.winWidth,  1f)
            );

            if (mainCar.damageBox.GetGlobalBounds().Intersects(top) || 
                mainCar.damageBox.GetGlobalBounds().Intersects(bottom))
            {
                mainCar.StopVelocityY();
            }  
        }

        //------------------------------------------------------------------------------------
        //                          Render elements on screen
        //------------------------------------------------------------------------------------
        public  void Render(ref RenderWindow window)
        {
            // wyswietlenie tła
            RenderBackground(ref window);

            // wyświetlenie monet i butelek
            RenderList(ref window, ref coins);
            RenderList(ref window, ref beers);
            RenderList(ref window, ref hearts);

            // wyświetlenie samochodów
            RenderList(ref window, ref cars);
            window.Draw(mainCar.sprite);

            // wyświetlenie damage boxów
            RenderDamageBoxes(ref window);

            // wyświetlenie interfejsu
            RenderInterface(ref window);
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

        private void RenderList(ref RenderWindow window, ref List<Entity> itemList)
        {
            foreach (Entity item in itemList)
                window.Draw(item.sprite);
        }

        private void RenderDamageBoxes(ref RenderWindow window)
        {
            if (showDamageBoxes)
            {
                window.Draw(resources.dmgBoxL);
                window.Draw(resources.dmgBoxR);
                window.Draw(mainCar.damageBox);

                foreach (Entity car in cars)
                    window.Draw(car.damageBox);

                foreach (Entity coin in coins)
                    window.Draw(coin.damageBox);

                foreach (Entity beer in beers)
                    window.Draw(beer.damageBox);
            }
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
            Sprite heart = new Sprite(resources.Thearts, heartsAnimation.currentFrame);
            for (int i = 0; i < lives; i++)
            {
                heart.Position = new Vector2f(
                    (float)resources.options.winWidth - 90f,
                    10f + i * 80f
                );
                window.Draw(heart);
            }
        }

        //------------------------------------------------------------------------------------
        //                          Supporting methods
        //------------------------------------------------------------------------------------
        public  void OnKeyPressed(object sender, KeyEventArgs key)
        {
            RenderWindow window = (RenderWindow)sender;

            //if (key.Code == Keyboard.Key.Escape)
            //    window.Close();                

            if (key.Code == Keyboard.Key.B)
                showDamageBoxes = (!showDamageBoxes);

            if (key.Code == Keyboard.Key.P || key.Code == Keyboard.Key.Escape)
                pause = (!pause);

            if (key.Code == Keyboard.Key.A || key.Code == Keyboard.Key.Left)
                mainCar.Move(-1, 0);

            if (key.Code == Keyboard.Key.D || key.Code == Keyboard.Key.Right)
                mainCar.Move(1, 0);

            if (key.Code == Keyboard.Key.W || key.Code == Keyboard.Key.Up)
                mainCar.Move(0, -1);

            if (key.Code == Keyboard.Key.S || key.Code == Keyboard.Key.Down)
                mainCar.Move(0, 1);
        }

        public  void OnKeyReleased(object sender, KeyEventArgs key)
        {
            if (key.Code == Keyboard.Key.A || key.Code == Keyboard.Key.Left)
                mainCar.Move(1, 0);

            if (key.Code == Keyboard.Key.D || key.Code == Keyboard.Key.Right)
                mainCar.Move(-1, 0);

            if (key.Code == Keyboard.Key.W || key.Code == Keyboard.Key.Up)
                mainCar.Move(0, 1);

            if (key.Code == Keyboard.Key.S || key.Code == Keyboard.Key.Down)
                mainCar.Move(0, -1);
        }

        public  void OnClose(object sender, EventArgs e)
        {
            RenderWindow window = (RenderWindow)sender;
            window.Close();
        }

        private bool CollisionsCheck(Entity A, Entity B)
        {
            FloatRect rectA = A.damageBox.GetGlobalBounds();
            FloatRect rectB = B.damageBox.GetGlobalBounds();

            if (rectA.Intersects(rectB))
                return true;

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
                cars.Clear();
                mainCar.SetPosition(
                    resources.background.Texture.Size.X / 2f - resources.background.Origin.X,
                    resources.background.Texture.Size.Y / 2f   
                );
                mainCar.movement.velocity = new Vector2f(0f, 0f);
            }
        }
    }
}
