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

        public double heartPercent = .5d;
        public double coinPercent = .5d;
        public double beerPercent = .5d;
        public double carPercent = .5d;

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
        public  void Update(float dt, float speed)
        {
            CreateEntities();                                           // próba utworzenia nowych elementów gry
            DeleteEntities(ref hearts);                                 // usunięcie elementów poza mapą
            DeleteEntities(ref coins);
            DeleteEntities(ref beers);
            DeleteEntities(ref cars);
            UpdateAnimation(dt, ref coinsAnimation,  ref coins);        // aktualizacja animacji
            UpdateAnimation(dt, ref beersAnimation,  ref beers);
            UpdateAnimation(dt, ref heartsAnimation, ref hearts);
            mainCar.UpdateMovementComponent(dt);                        // aktualizacja ruchu głównego pojazdu
            UpdateEntitiesMove(dt, speed, ref hearts);                  // aktualizacja ruchu elementów gry
            UpdateEntitiesMove(dt, speed, ref coins);
            UpdateEntitiesMove(dt, speed, ref beers);
            UpdateEntitiesMove(dt, speed, ref cars);
            UpdateMapBounds();                                          // aktualizacja położenia na mapie głownego samochodu
            UpdateMapCollision();                                       // sprawdzenie wyjazdu gracza poza jezdnię
            UpdateListCollisions(ref hearts, onHeartCollision);         // aktualizacja kolizji obiektów
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

        private void UpdateEntitiesMove(float dt, float speed, ref List<Entity> itemList)
        {
            foreach (var item in itemList)
                item.UpdateMove(dt, speed);
        }

        private void UpdateListCollisions(ref List<Entity> itemList, OnCollision onCollision)
        {
            foreach (Entity item in itemList)
            {
                if (CollisionsCheck(item, mainCar))
                {
                    onCollision(itemList, item);
                    return;
                }
            }    
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

                foreach (Entity heart in hearts)
                    window.Draw(heart.damageBox);
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

        public void CreateEntities()
        {
            Vector2f offset = resources.background.Position;
            offset.X -= resources.background.Origin.X;
            offset.Y -= resources.background.Origin.Y;
            // utworzenie obiektów pasów
            FloatRect[] lanes = new FloatRect[4];
            lanes[0] = new FloatRect(new Vector2f(319f + offset.X, -1f), new Vector2f(138f, 10f));
            lanes[1] = new FloatRect(new Vector2f(466f + offset.X, -1f), new Vector2f(153f, 10f));
            lanes[2] = new FloatRect(new Vector2f(627f + offset.X, -1f), new Vector2f(158f, 10f));
            lanes[3] = new FloatRect(new Vector2f(792f + offset.X, -1f), new Vector2f(144f, 10f));

            for (int i = 0; i < lanes.Length; i++)
            {
                if (BusyCheck(lanes[i]))
                {
                    Random rand = new Random();
                    double percent = rand.NextDouble();
                    TYPE type = (TYPE)rand.Next((int)TYPE.COUNT);
                    DIRECTION dir = (i < 2) ? DIRECTION.DOWN : DIRECTION.UP;
                    Vector2f position = new Vector2f(lanes[i].Left + lanes[i].Width / 2f, 0f );
                    
                    // utworzenie obiektu danego typu jeśli zostanie poprawnie określony procentowo
                    switch (type)
                    {
                    case TYPE.HEART:
                        if (heartPercent > percent)
                            CreateElement(ref hearts, heartsAnimation, resources.Thearts, position, type, dir);
                        break;

                    case TYPE.COIN:
                        if (coinPercent > percent)
                            CreateElement(ref coins, coinsAnimation, resources.TcoinAnimated, position, type, dir);
                        break;

                    case TYPE.BEER:
                        if (beerPercent > percent)
                            CreateElement(ref beers, beersAnimation, resources.TbeerAnimated, position, type, dir);
                        break;

                    case TYPE.CAR:
                        if (carPercent > percent)
                        {
                            Entity car = new Entity(rand.Next(14), resources.carCollection, dir);
                            position.Y -= (dir == DIRECTION.UP) ? car.damageBox.Size.Y : 0f;
                            car.SetPosition(position.X, position.Y);
                            cars.Add(car);
                        }
                        break;
                    }
                }
            }
        }

        private void DeleteEntities(ref Entities itemList)
        {
            Vector2f position;
            foreach (Entity item in itemList)
            {
                position = item.damageBox.Position;
                switch (item.dir)
                {
                    case DIRECTION.DOWN:
                        position.Y -= item.damageBox.Size.Y;
                        if (position.Y > resources.options.winHeight)
                        {
                            itemList.Remove(item);
                            return;
                        } 
                        break;

                    case DIRECTION.UP:
                        position.Y += item.damageBox.Size.Y;
                        if (position.Y < 0f)
                        {
                            itemList.Remove(item);
                            return;
                        }
                        break;

                    default: break;
                }
            }
        }

        public  void SetPlayerCarById(int id)
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

        private bool BusyCheck(FloatRect lane)
        {
            foreach (var item in hearts)
                if (lane.Intersects(item.damageBox.GetGlobalBounds()))
                    return false;

            foreach (var item in coins)
                if (lane.Intersects(item.damageBox.GetGlobalBounds()))
                    return false;

            foreach (var item in beers)
                if (lane.Intersects(item.damageBox.GetGlobalBounds()))
                    return false;

            foreach (var item in cars)
                if (lane.Intersects(item.damageBox.GetGlobalBounds()))
                    return false;

            return true;
        }

        private void CreateElement(
            ref Entities list, 
            EntityAnimation animation,
            Texture texture,
            Vector2f position, 
            TYPE type, 
            DIRECTION dir
        )
        {
            IntRect damageRect = new IntRect()
            {
                Left = 0, Top = 0,
                Width = animation.currentFrame.Width,
                Height = animation.currentFrame.Height
            };
            Entity entity = new Entity(
                texture,
                animation.currentFrame,
                damageRect,
                new Color(255, 255, 255, 128),
                type,
                dir
            );
            position.Y -= (dir == DIRECTION.UP) ? entity.damageBox.Size.Y : 0f;
            entity.SetPosition(position.X, position.Y);
            list.Add(entity);
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
