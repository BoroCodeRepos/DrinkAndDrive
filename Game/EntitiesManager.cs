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
    using EntitiesAnimation = Dictionary<TYPE, EntityAnimation>;
    using Timers = Dictionary<TYPE, TimeCounter>;

    class EntitiesManager
    {
        public Resources resources;    // zasoby gry
        public Entities entities;      // elementy gry (serca, monety, butelki i samochody)
        public Entity mainCar;         // samochód główny - gracza
        public EntitiesAnimation 
            animation;                 // animacja serc, monet i butelek
        public Timers timers;          // licznik czas do utworzenia następnego elementu               

        public delegate bool OnCollision();

        public OnCollision onHeartCollision;
        public OnCollision onCoinCollision;
        public OnCollision onBeerCollision;
        public OnCollision onCarCollision;
        public OnCollision onMapCollision;

        public double heartPercent = 1e-2d;
        public double coinPercent = 8e-2d;
        public double beerPercent = 6e-2d;
        public double carPercent = 5e-2d;
        public float offset = 0f;

        //------------------------------------------------------------------------------------
        //                          Constructor
        //------------------------------------------------------------------------------------
        public EntitiesManager(Resources resources)
        {
            this.resources = resources;

            entities = new Entities();
            animation = new EntitiesAnimation
            {
                [TYPE.COIN]  = new EntityAnimation(),
                [TYPE.BEER]  = new EntityAnimation(),
                [TYPE.HEART] = new EntityAnimation()
            };

            InitAnimation("coins",  animation, TYPE.COIN);
            InitAnimation("beers",  animation, TYPE.BEER);
            InitAnimation("hearts", animation, TYPE.HEART);

            timers = new Timers
            {
                [TYPE.COIN] = new TimeCounter(200d),
                [TYPE.BEER] = new TimeCounter(200d),
                [TYPE.CAR] = new TimeCounter(400d)
            };
        }

        //------------------------------------------------------------------------------------
        //                          Initialization methods
        //------------------------------------------------------------------------------------
        private void InitAnimation(string attrName, EntitiesAnimation Tanimation, TYPE type)
        {
            EntityAnimation animation = Tanimation[type];
            XmlElement root = resources.document.DocumentElement;
            XmlElement element = root["game"]["animation"][attrName];
            animation.framesNr = Convert.ToInt16(element.Attributes["frame_nr"].Value);
            animation.maxFrameTime = float.Parse(element.Attributes["max_frame_time"].Value);
            int width = Convert.ToInt16(element.Attributes["width"].Value);
            int height = Convert.ToInt16(element.Attributes["height"].Value);
            animation.currentFrame = new IntRect(0, 0, width, height);
            animation.currentFrameId = 0;
            animation.currentFrameTime = 0f;
            Tanimation[type] = animation;
        }

        //------------------------------------------------------------------------------------
        //                          Update game methods
        //------------------------------------------------------------------------------------
        public  void Update(float dt, float speed, float offset)
        {
            UpdateTimers(dt);                                   // aktualizacja timerów
            TryCreateEntities();                                // próba utworzenia nowych elementów gry
            DeleteEntities();                                   // usunięcie elementów poza mapą
            //UpdateAnimation(dt);                                // aktualizacja animacji
            mainCar.UpdateMove(dt, speed, 0f);                  // aktualizacja ruchu głównego pojazdu
            UpdateEntitiesMove(dt, speed, offset);              // aktualizacja ruchu elementów gry
            UpdateMapBounds();                                  // aktualizacja położenia na mapie głownego samochodu
            UpdateMapCollision();                               // sprawdzenie wyjazdu gracza poza jezdnię
            UpdateCollisions();                                 // aktualizacja kolizji obiektów
        }

        public  void UpdateAnimation(float dt)
        {
            List<TYPE> types = new List<TYPE>
            {
                TYPE.HEART,
                TYPE.COIN,
                TYPE.BEER
            };

            foreach (var type in types)
            {
                var animation = this.animation[type];
                animation.currentFrameTime += dt;

                if (animation.currentFrameTime >= animation.maxFrameTime)
                {
                    animation.currentFrameTime = 0f ;
                    animation.currentFrameId += 1;
                    if (animation.currentFrameId >= animation.framesNr)
                        animation.currentFrameId = 0;
                }

                animation.currentFrame.Left = animation.currentFrameId * animation.currentFrame.Width;
                this.animation[type] = animation;
                var animatedEntities = entities.Where(entity => entity.type == type);
                foreach (var item in animatedEntities)
                    item.sprite.TextureRect = animation.currentFrame;
            }
        }

        private void UpdateEntitiesMove(float dt, float speed, float offset)
        {
            foreach (var item in entities)
            {
                item.UpdateMove(dt, speed, offset);
            }
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

        private void UpdateTimers(float dt)
        {
            timers[TYPE.COIN].Update(dt);
            timers[TYPE.BEER].Update(dt);
            timers[TYPE.CAR].Update(dt);
        }

        private void UpdateCollisions()
        {
            Entities toDelete = new Entities();
            foreach (Entity entity in entities)
            {
                if (CollisionsCheck(mainCar, entity))
                {
                    if (entity.type == TYPE.HEART)
                        if (onHeartCollision())
                            toDelete.Add(entity);

                    if (entity.type == TYPE.COIN)
                        if (onCoinCollision())
                            toDelete.Add(entity);
                    
                    if (entity.type == TYPE.BEER)
                        if (onBeerCollision())
                            toDelete.Add(entity);

                    if (entity.type == TYPE.CAR)
                        if (onCarCollision())
                            toDelete.Add(entity);
                }
            }
            // usunięcie żądanych elementów
            foreach (Entity entity in toDelete)
                entities.Remove(entity);
        }

        private void UpdateMapCollision()
        {
            FloatRect carRect = mainCar.damageBox.GetGlobalBounds();
            FloatRect bgRectL = resources.dmgBoxL.GetGlobalBounds();
            FloatRect bgRectR = resources.dmgBoxR.GetGlobalBounds();
            if (carRect.Intersects(bgRectL) || carRect.Intersects(bgRectR))
                onMapCollision();
        }

        //------------------------------------------------------------------------------------
        //                          Render elements on screen
        //------------------------------------------------------------------------------------
        public  void RenderEntities(ref RenderWindow window)
        {
            // wyświetlenie monet, butelek, serc i  samochodów
            foreach (var entity in entities)
                window.Draw(entity.sprite);
            // wyświetlenie samochodu gracza
            window.Draw(mainCar.sprite);
        }

        public  void RenderDamageBoxes(ref RenderWindow window, bool showDamageBoxes)
        {
            if (showDamageBoxes)
            {
                window.Draw(resources.dmgBoxL);
                window.Draw(resources.dmgBoxR);
                window.Draw(mainCar.damageBox);

                foreach (Entity entity in entities)
                    window.Draw(entity.damageBox);
            }
        }

        //------------------------------------------------------------------------------------
        //                          Supporting methods
        //------------------------------------------------------------------------------------
        public  void ClearAll()
        {
            entities.Clear();
        }

        public  void SetPlayerCarById(int id)
        {
            id = id % 14;
            mainCar = new Entity(id, resources.carCollection);
            mainCar.damageBox.FillColor = new Color(0, 255, 255, 128);
            mainCar.SetPosition(
                resources.background.Texture.Size.X / 2f - resources.background.Origin.X,
                resources.background.Texture.Size.Y / 2f
            );
        }

        private void TryCreateEntities()
        {
            float offset = resources.background.Position.X;
            offset -= resources.background.Origin.X;
            // utworzenie obiektów pasów
            FloatRect[] lanes = new FloatRect[4];
            lanes[0] = new FloatRect(319f + offset, -1f, 138f, 10f);
            lanes[1] = new FloatRect(466f + offset, -1f, 153f, 10f);
            lanes[2] = new FloatRect(627f + offset, -1f, 158f, 10f);
            lanes[3] = new FloatRect(792f + offset, -1f, 144f, 10f);

            for (int i = 0; i < lanes.Length; i++)
                if (BusyCheck(lanes[i]))
                {
                    Random rand = new Random();
                    double percent = rand.NextDouble();
                    TYPE type = (TYPE)rand.Next((int)TYPE.COUNT);
                    DIRECTION dir = (i < 2) ? DIRECTION.DOWN : DIRECTION.UP;
                    lanes[i].Left -= (offset + resources.background.Origin.X);
                    Vector2f position = new Vector2f(lanes[i].Left + lanes[i].Width / 2f, 0f);
                    CheckPercentageChances(type, dir, percent, position);
                }
        }

        private bool BusyCheck(FloatRect lane)
        {
            foreach (var item in entities)
                if (lane.Intersects(item.damageBox.GetGlobalBounds()))
                    return false;

            return true;
        }

        private void CheckPercentageChances(TYPE type, DIRECTION dir, double percent, Vector2f position)
        {
            // utworzenie obiektu danego typu jeśli zostanie poprawnie określony procentowo
            bool create = false;
            double chances = 0d;
            if (type == TYPE.HEART)
                chances = heartPercent;
            if (type == TYPE.COIN)
                chances = coinPercent;
            if (type == TYPE.BEER)
                chances = beerPercent;
            if (type == TYPE.CAR)
                chances = carPercent;

            if (type != TYPE.HEART)
            {
                TimeCounter timer = timers[type];
                if (timer.IsRun())
                {
                    if (timer.GetEventStatus())
                    {
                        create = true;
                        timer.ClearEventStatus();
                    }
                }
                else
                {
                    create = true;
                    timer.Start();
                }
            }
            else
            {
                create = true;
            }

            if (create && chances > percent)
                CreateElement(position, type, dir);
        }

        private void CreateElement(Vector2f position, TYPE type, DIRECTION dir)
        {
            if (type == TYPE.CAR)
            {
                Random rnd = new Random();
                Entity car = new Entity(rnd.Next(14), resources.carCollection, false, dir);
                position.Y -= (dir == DIRECTION.UP) ? car.damageBox.Size.Y : 0f;
                car.primaryPosX = position.X;
                car.SetPosition(position.X, position.Y);
                entities.Add(car);
            }
            else
            {
                IntRect damageRect = new IntRect()
                {
                    Left = 0, Top = 0,
                    Width = animation[type].currentFrame.Width,
                    Height = animation[type].currentFrame.Height
                };
                Entity entity = new Entity(
                    resources.GetTexture(type),
                    animation[type].currentFrame,
                    damageRect,
                    type,
                    dir );
                position.Y -= (dir == DIRECTION.UP) ? entity.damageBox.Size.Y : 0f;
                entity.primaryPosX = position.X;
                entity.SetPosition(position.X, position.Y);
                entities.Add(entity);
            }
        }

        private void DeleteEntities()
        {
            Vector2f position;
            Entities toDelete = new Entities();
            foreach (Entity entity in entities)
            {
                position = entity.damageBox.Position;
                switch (entity.dir)
                {
                case DIRECTION.DOWN:
                    position.Y -= entity.damageBox.Size.Y;
                    if (position.Y > resources.options.winHeight)
                        toDelete.Add(entity);
                    break;

                case DIRECTION.UP:
                    if (position.Y > resources.options.winHeight)
                        toDelete.Add(entity);
                    break;
                }
            }
            // usunięcie wszystkich elementów
            foreach (Entity entity in toDelete)
                entities.Remove(entity);      
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
