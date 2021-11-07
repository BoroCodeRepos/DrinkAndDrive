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
        TimeCounter gameTime;       // czas gry
        float level;                // aktualny poziom gry
        float score;                // aktualny wynik gracza
        float speed;                // szybkość przesuwania się drogi po ekranie
        bool showDamageBoxes;       // true <= pokazuje modele uszkodzeń samochodów i jezdni
        List<Entity> coins;         // kolekcja coinsów na jezdni
        List<Entity> beers;         // kolekcja butelek na jezdni
        List<Entity> cars;          // kolekcja pojazdów na jezdni
        Entity mainCar;             // samochód główny - gracza
        Animation coinsAnimation,   // animacja monet i butelek
            beersAnimation;

        //------------------------------------------------------------------------------------
        //                          Variables of alcohol level
        //------------------------------------------------------------------------------------
        TimeCounter alcoTime;       // czas jazdy po alkoholu
        TimeCounter alcoTimeToStep; // czas do obniżenia alkoholu we krwi
        float alcoLevelStep;        // krok obniżenia alkoholu
        float alcoLevel;            // aktualny poziom alkoholu we krwi

        public Engine(Resources resources)
        {
            this.resources = resources;
            try
            {
                InitGameResources();
                InitCoinsAnimation();
                InitBeersAnimation();
                InitAlcoVars();
            }
            catch(Exception exception)
            {
                Program.ShowError(exception);
            }
        }

        private void InitGameResources()
        {
            score = 0f;
            showDamageBoxes = false;
            speed = .1f;

            gameTime = new TimeCounter();
            alcoTime = new TimeCounter();
            alcoTime.Start();
            //mainCar = new Entity();
            coins = new List<Entity>();
            beers = new List<Entity>();
            cars = new List<Entity>();
            coinsAnimation = new Animation();
            beersAnimation = new Animation();
        }

        private void InitCoinsAnimation()
        {
            XmlElement root = resources.document.DocumentElement;
            XmlElement coinsElement = root["game"]["animation"]["coins"];
            coinsAnimation.framesNr = Convert.ToInt16(coinsElement.Attributes["frame_nr"].Value);
            coinsAnimation.maxFrameTime = float.Parse(coinsElement.Attributes["max_frame_time"].Value);
            int width = Convert.ToInt16(coinsElement.Attributes["width"].Value);
            int height = Convert.ToInt16(coinsElement.Attributes["height"].Value);
            coinsAnimation.currentFrame = new IntRect(0, 0, width, height);
            coinsAnimation.currentFrameId = 0;
            coinsAnimation.currentFrameTime = 0f;
        }

        private void InitBeersAnimation()
        {
            XmlElement root = resources.document.DocumentElement;
            XmlElement beersElement = root["game"]["animation"]["beers"];
            beersAnimation.framesNr = Convert.ToInt16(beersElement.Attributes["frame_nr"].Value);
            beersAnimation.maxFrameTime = float.Parse(beersElement.Attributes["max_frame_time"].Value);
            int width = Convert.ToInt16(beersElement.Attributes["width"].Value);
            int height = Convert.ToInt16(beersElement.Attributes["height"].Value);
            beersAnimation.currentFrame = new IntRect(0, 0, width, height);
            beersAnimation.currentFrameId = 0;
            beersAnimation.currentFrameTime = 0f;
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
            alcoLevel = 0.0f;
        }

        public void Update(float dt)
        {
            gameTime.Update(dt);
            alcoTime.Update(dt);
            UpdateBackground(dt);
            UpdateCars(dt);
            UpdateList(dt, coins);
            UpdateList(dt, beers);
            UpdateAnimation(dt, ref coinsAnimation, ref coins);
            UpdateAnimation(dt, ref beersAnimation, ref beers);
        }

        private void UpdateBackground(float dt)
        {
            double time = alcoTime.GetCurrentTime() / 1000d;
            Vector2f curr_pos = resources.background.Position;
            resources.background.Position = new Vector2f(
                10*alcoLevel * (float)Math.Sin((alcoLevel*time) % 2d*Math.PI),
                (curr_pos.Y + dt * speed) % resources.background.Texture.Size.Y
            );
        }

        private void UpdateCars(float dt)
        {

        }

        private void UpdateList(float dt, List<Entity> itemList)
        {

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

        public void Render(ref RenderWindow window)
        {
            RenderBackground(ref window);
            RenderList(ref window, ref coins);
            RenderList(ref window, ref beers);
            window.Draw(resources.shader);
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

        private void RenderCars(ref RenderWindow window)
        {

        }

        private void RenderList(ref RenderWindow window, ref List<Entity> itemList)
        {
            foreach (Entity item in itemList)
                window.Draw(item.sprite);
        }

        public void OnKeyPressed(object sender, KeyEventArgs key)
        {
            RenderWindow window = (RenderWindow)sender;

            if (key.Code == Keyboard.Key.Escape)
                window.Close();

            if (key.Code == Keyboard.Key.B)
                showDamageBoxes = (!showDamageBoxes);
        }

        public void OnClose(object sender, EventArgs e)
        {
            RenderWindow window = (RenderWindow)sender;
            window.Close();
        }
    }
}
