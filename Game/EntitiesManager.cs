using System;
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
    using Entities = List<Entity>;

    class EntitiesManager
    {
        public Resources resources;    // zasoby gry
        public Entities hearts;        // kolekcja serc na jezdni
        public Entities coins;         // kolekcja coinsów na jezdni
        public Entities beers;         // kolekcja butelek na jezdni
        public Entities cars;          // kolekcja pojazdów na jezdni
        public Entity mainCar;         // samochód główny - gracza
        public EntityAnimation 
            coinsAnimation,            // animacja monet
            beersAnimation,            // animacja butelek
            heartsAnimation;           // animacja serc

        public delegate void OnCollision(Entities itemList, Entity item);
        public delegate void OnMapCollision();

        public OnCollision onHeartCollision;
        public OnCollision onCoinCollision;
        public OnCollision onBeerCollision;
        public OnCollision onCarCollision;
        public OnMapCollision onMapCollision;

        //------------------------------------------------------------------------------------
        //                          Constructor
        //------------------------------------------------------------------------------------
        public EntitiesManager(Resources resources)
        {
            this.resources = resources;

            hearts = new Entities();
            coins  = new Entities();
            beers  = new Entities();
            cars   = new Entities();

            InitAnimation("coins",  ref coinsAnimation);
            InitAnimation("beers",  ref beersAnimation);
            InitAnimation("hearts", ref heartsAnimation);
        }

        //------------------------------------------------------------------------------------
        //                          Initialization methods
        //------------------------------------------------------------------------------------
        private void InitAnimation(string attrName, ref EntityAnimation animation)
        {
            animation = new EntityAnimation();
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

        //------------------------------------------------------------------------------------
        //                          Update game methods
        //------------------------------------------------------------------------------------
        public  void Update(float dt)
        {
            // aktualizacja animacji
            UpdateAnimation(dt, ref coinsAnimation,  ref coins);
            UpdateAnimation(dt, ref beersAnimation,  ref beers);
            UpdateAnimation(dt, ref heartsAnimation, ref hearts);

            // aktualizacja ruchu głównego pojazdu
            mainCar.UpdateMovementComponent(dt);

            // aktualizacja położenia na mapie
            UpdateMapBounds();

            // sprawdzenie kolizji poza jezdnią
            UpdateMapCollision();

            // aktualizacja kolizji obiektów
            UpdateListCollisions(ref hearts, onHeartCollision);
            UpdateListCollisions(ref coins,  onCoinCollision);
            UpdateListCollisions(ref beers,  onBeerCollision);
            UpdateListCollisions(ref cars,   onCarCollision);
        }

        private void UpdateAnimation(float dt, ref EntityAnimation animation, ref Entities itemList)
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

        private void UpdateListCollisions(ref List<Entity> itemList, OnCollision onCollision)
        {
            foreach (Entity item in itemList)
                if (CollisionsCheck(item, mainCar))
                    onCollision(itemList, item);
        }

        private void UpdateMapCollision()
        {
            FloatRect carRect = mainCar.damageBox.GetGlobalBounds();
            FloatRect bgRectL = resources.dmgBoxL.GetGlobalBounds();
            FloatRect bgRectR = resources.dmgBoxR.GetGlobalBounds();
            if (carRect.Intersects(bgRectL) || carRect.Intersects(bgRectR))
                onMapCollision();
        }

        private void UpdateMapBounds()
        {
            Vector2f pos = mainCar.damageBox.Position;
            if (pos.Y < 0f)
            {
                mainCar.SetPosition(pos.X, 0f);
                mainCar.movement.velocity.Y = 0f;
            }
            pos.Y += mainCar.damageBox.Size.Y;
            if (pos.Y > resources.options.winHeight)
            {
                pos.Y = resources.options.winHeight - mainCar.damageBox.Size.Y;
                mainCar.SetPosition(pos.X, pos.Y);
                mainCar.movement.velocity.Y = 0f;
            }
        }

        //------------------------------------------------------------------------------------
        //                          Render elements on screen
        //------------------------------------------------------------------------------------
        public  void RenderEntities(ref RenderWindow window)
        {
            // wyświetlenie monet i butelek
            RenderList(ref window, ref coins);
            RenderList(ref window, ref beers);
            RenderList(ref window, ref hearts);

            // wyświetlenie samochodów
            RenderList(ref window, ref cars);
            window.Draw(mainCar.sprite);
        }

        public  void RenderDamageBoxes(ref RenderWindow window, bool showDamageBoxes)
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

        private void RenderList(ref RenderWindow window, ref Entities itemList)
        {
            foreach (Entity item in itemList)
                window.Draw(item.sprite);
        }

        //------------------------------------------------------------------------------------
        //                          Supporting methods
        //------------------------------------------------------------------------------------
        public void ClearAll()
        {
            hearts.Clear();
            coins.Clear();
            beers.Clear();
            cars.Clear();
        }

        public void SetPlayerCarById(int id)
        {
            id = id % 14;
            XmlNode movement = resources.document.GetElementsByTagName("advanced_move")[0];
            mainCar = new Entity(id, resources.carCollection);
            mainCar.damageBox.FillColor = new Color(0, 255, 255, 128);
            mainCar.SetPosition(
                resources.background.Texture.Size.X / 2f - resources.background.Origin.X,
                resources.background.Texture.Size.Y / 2f
            );
        }

        private bool CollisionsCheck(Entity A, Entity B)
        {
            FloatRect rectA = A.damageBox.GetGlobalBounds();
            FloatRect rectB = B.damageBox.GetGlobalBounds();

            if (rectA.Intersects(rectB))
                return true;

            return false;
        }
    }

    struct EntityAnimation
    {
        public int currentFrameId;
        public float currentFrameTime;

        public int framesNr;
        public float maxFrameTime;

        public IntRect currentFrame;
    }
}
