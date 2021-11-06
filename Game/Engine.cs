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

        private struct SoberingUp
        {
            public float timeToStep;
            public float alkoholLevelStep;
            public float currentAlkoholLevel;
        }

        private struct Movement
        {
            public float aUp;
            public float aDown;
            public float aLeft;
            public float aRight;

            public float vUp;
            public float vDown;
            public float vLeft;
            public float vRight;

            public float maxVelocityUp;
            public float maxVelocityDown;
            public float maxVelocityLeft;
            public float maxVelocityRight;
        }

        Resources resources;
        float score, multiplier;
        bool showDamageBoxes;

        List<Entity> coins;
        List<Entity> beers;
        Animation coinsAnimation, beersAnimation;
        SoberingUp sobering;
        Movement movement;

        public Engine(Resources resources)
        {
            this.resources = resources;
            score = 0f;
            multiplier = 0f;
            showDamageBoxes = false;

            coins = new List<Entity>();
            beers = new List<Entity>();
            coinsAnimation = new Animation();
            beersAnimation = new Animation();
            sobering = new SoberingUp();

            try
            {
                InitCoinsAnimation();
                InitBeersAnimation();
                InitSoberingUp();
            }
            catch(Exception exception)
            {
                Program.ShowError(exception);
            }

            //
            coins.Add(new Entity(
                    resources.coinAnimated,
                    coinsAnimation.currentFrame,
                    new FloatRect(),
                    new RectangleShape()
                )
            );
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

        private void InitSoberingUp()
        {
            XmlElement root = resources.document.DocumentElement;
            XmlElement soberingUpElement = root["game"]["sobering_up"];
            sobering.alkoholLevelStep = float.Parse(
                soberingUpElement.Attributes["step_alkohol_level"].Value
            );
            sobering.timeToStep = float.Parse(
                soberingUpElement.Attributes["step_time"].Value
            );
            sobering.currentAlkoholLevel = 0f;
        }

        public void Update(float dt)
        {
            UpdateCars(dt);
            UpdateList(dt, coins);
            UpdateList(dt, beers);
            UpdateAnimation(dt, ref coinsAnimation, ref coins);
            //UpdateAnimation(dt, beersAnimation, beers);
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

        public void Render(RenderWindow window)
        {
            //window.Draw(resources.coins);
            RenderList(window, coins);
            RenderList(window, beers);
        }

        private void RenderCars(RenderWindow window)
        {

        }

        private void RenderList(RenderWindow window, List<Entity> itemList)
        {
            foreach (Entity item in itemList)
                window.Draw(item.sprite);
        }

        public void OnKeyPressed(object sender, KeyEventArgs key)
        {
            RenderWindow window = (RenderWindow)sender;

            if (key.Code == Keyboard.Key.Escape)
                window.Close();


        }

        public void OnClose(object sender, EventArgs e)
        {
            RenderWindow window = (RenderWindow)sender;
            window.Close();
        }
    }
}
