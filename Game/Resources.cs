using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;

namespace Game
{
    using Entities = List<Entity>;
    using KeyBindings = Dictionary<Keyboard.Key, string>;
    using Numbers = Dictionary<int, Sprite>;
    using Sounds = Dictionary<string, Sound>;
    using Textures = Dictionary<TYPE, Texture>;

    /// <summary>
    /// Klasa inicjalizująca i przechowująca zasoby gry.
    /// </summary>
    public class Resources
    {
        /// <summary>
        /// Klasa pomocnicza, przechowująca parametry okna.
        /// </summary>
        public class Options
        {
            /// <summary>Zmienna przechowująca szerokość okna.</summary>
            public uint winWidth;
            /// <summary>Zmienna przechowująca wysokość okna.</summary>
            public uint winHeight;
            /// <summary>Zmienna przechowująca tytuł okna.</summary>
            public string winTitle;
        }

        /// <summary>Obiekt dokumentu XML.</summary>
        public XmlDocument document;        
        /// <summary>Wyświetlany obiekt tła gry.</summary>
        public Sprite background;           
        /// <summary>Obiekt lewego pobocza jezdni - obszar niedozwolony.</summary>
        public RectangleShape dmgBoxL;
        /// <summary>Obiekt prawego pobocza jezdni - obszar niedozwolony.</summary>
        public RectangleShape dmgBoxR;     
        /// <summary>Obiekt z symbolem monet.</summary>
        public RectangleShape coins;       
        /// <summary>Tekstura cyfr (do odliczania, wyświetlania wyniku i poziomu alkoholu).</summary>
        public Texture Tnumbers;            
        /// <summary>Tekstura eksplozji.</summary>
        public Texture Texplosion;         
        /// <summary>Tekstura filtru zmiany percepcji.</summary>
        public Texture Tfilter;            
        /// <summary>Kolekcja wszystkich pojazdów.</summary>
        public Entities carCollection;     
        /// <summary>Obiekt opcji okna.</summary>
        public Options options;             
        /// <summary>Kolekcja tekstur: monety, serca, kapsle i samochody.</summary>
        public Textures textures;         
        /// <summary>Słownik z dostępnymi cyframi.</summary>
        public Numbers numbers;            
        /// <summary>Obiekt dostępnego fontu.</summary>
        public Font font;                   
        /// <summary>Słownik z obsługiwanymi klawiszami.</summary>
        public KeyBindings keys;            
        /// <summary>Słownik z dostępnymi dźwiękami.</summary>
        public Sounds sounds;               
        /// <summary>Wątek do inicjalizacji tekstur.</summary>
        Thread tTextures;
        /// <summary>Wątek do inicjalizacji pozostałych zasobów.</summary>
        Thread tOthers;
        /// <summary>Wątek do inicjalizacji dźwięków.</summary>
        Thread tSounds;
        /// <summary>Mutex dostępu do dokumentu XML.</summary>
        Mutex xmlAccess;

        /// <summary>
        /// Metoda inicjalizująca zasoby i wątki.
        /// </summary>
        public void Init()
        {
            InitXMLdoc();
            InitFont();
            xmlAccess = new Mutex();
            tTextures = new Thread(() => SafeExecute(LoadTextures, Program.ShowError));
            tSounds = new Thread(() => SafeExecute(LoadSounds, Program.ShowError));
            tOthers = new Thread(() => SafeExecute(LoadOthers, Program.ShowError));

            tTextures.Start();
            tSounds.Start();
            tOthers.Start();
        }

        // ------------------------------------------------------------------------------
        //                     Metody wykorzystane w wątkach
        // ------------------------------------------------------------------------------
        /// <summary>
        /// Metoda obsługująca bezpieczne wykonanie wątku z ew. komunikacją o błędzie.
        /// </summary>
        /// <param name="action">Funkcja wykonania wątku.</param>
        /// <param name="handler">Uchwyt do funkcji ew. komunikacji błędu.</param>
        private void SafeExecute(Action action, Action<Exception> handler)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                handler(ex);
            }
        }

        /// <summary>
        /// Metoda ładująca wszystkie tekstury do pamięci (wykonanie w wątku).
        /// </summary>
        private void LoadTextures()
        {
            InitOptions();
            InitBackground();
            InitCoinsShape();
            InitTextures();
            InitNumbers();
            InitFilter();
            InitCarsCollection();
        }

        /// <summary>
        /// Metoda ładująca pozostałe elementy gry do pamięci (wykonanie w wątku).
        /// </summary>
        private void LoadOthers()
        {
            InitKeyBindings();
        }

        /// <summary>
        /// Metoda ładująca dostępne dźwięki gry do pamięci (wykonanie w wątku).
        /// </summary>
        private void LoadSounds()
        {
            sounds = new Sounds();

            string[] fileNames = Directory.GetFiles("..\\..\\resource\\sounds", "*.*");
            foreach (var file in fileNames)
            {
                var splitedPath = file.Split('\\');
                var filename = splitedPath[splitedPath.Length - 1];
                var key = filename.Split('.')[0];
                sounds[key] = new Sound
                {
                    SoundBuffer = new SoundBuffer(file)
                };
            }
        }

        /// <summary>
        /// Metoda sprawdzająca status wykonania wątków łądujących zasoby do pamięci.
        /// </summary>
        /// <returns>Zwraca listę z wykonującymi się wątkami.</returns>
        public List<string> LoadingStatus()
        {
            List<string> threadList = new List<string>();

            if (tTextures.IsAlive)
                threadList.Add("Textures");

            if (tSounds.IsAlive)
                threadList.Add("Sounds");

            if (tOthers.IsAlive)
                threadList.Add("Others");

            return threadList;
        }

        // ------------------------------------------------------------------------------
        //                     Metody inicjalizujące zasoby
        // ------------------------------------------------------------------------------
        /// <summary>
        /// Metoda inicjalizująca dokument XML.
        /// </summary>
        private void InitXMLdoc()
        {
            document = new XmlDocument();
            document.Load("..\\..\\Config.xml");
        }

        /// <summary>
        /// Metoda inicjalizująca parametry głównego okna gry.
        /// </summary>
        private void InitOptions()
        {
            options = new Options();
            xmlAccess.WaitOne();
            XmlElement root = document.DocumentElement;
            xmlAccess.ReleaseMutex();
            options.winWidth = Convert.ToUInt16(root["window"].Attributes["width"].Value);
            options.winHeight = Convert.ToUInt16(root["window"].Attributes["height"].Value);
            options.winTitle = root["window"].Attributes["title"].Value;
        }

        /// <summary>
        /// Metoda inicjalizująca obiekt tła gry (jezdni).
        /// </summary>
        private void InitBackground()
        {
            background = new Sprite(new Texture("..\\..\\resource\\images\\background.png"));
            background.Origin = new Vector2f(
                (float)(background.Texture.Size.X - options.winWidth) / 2f,
                0f
            );

            dmgBoxL = new RectangleShape
            {
                Size = new Vector2f(272f, background.Texture.Size.Y),
                Origin = background.Origin,
                FillColor = new Color(255, 0, 0, 128),
                OutlineThickness = 1f,
                OutlineColor = new Color(Color.Black)
            };
            dmgBoxR = new RectangleShape
            {
                Size = new Vector2f(272f, background.Texture.Size.Y),
                Origin = new Vector2f(-871f, 0f),
                FillColor = new Color(255, 0, 0, 128),
                OutlineThickness = 1f,
                OutlineColor = new Color(Color.Black)
            };
        }

        /// <summary>
        /// Metoda inicjalizująca obiekt kształtu monet.
        /// </summary>
        private void InitCoinsShape()
        {
            coins = new RectangleShape
            {
                Position = new Vector2f(10f, 10f),
                Size = new Vector2f(120f, 120f),
                Texture = new Texture("..\\..\\resource\\images\\coins.png")
            };
        }

        /// <summary>
        /// Metoda inicjalizująca wszystkie obiekty tekstur.
        /// </summary>
        private void InitTextures()
        {
            textures = new Dictionary<TYPE, Texture>
            {
                [TYPE.HEART] = new Texture("..\\..\\resource\\images\\heart_animated.png") { Smooth = true },
                [TYPE.COIN] = new Texture("..\\..\\resource\\images\\coin_animated.png") { Smooth = true },
                [TYPE.BEER] = new Texture("..\\..\\resource\\images\\bottle_cap_animated.png") { Smooth = true },
                [TYPE.CAR] = new Texture("..\\..\\resource\\images\\cars.png") { Smooth = true }
            };
            Tnumbers = new Texture("..\\..\\resource\\images\\numbers.png")
            {
                Smooth = true
            };

            Texplosion = new Texture("..\\..\\resource\\images\\explosion_animated.png")
            {
                Smooth = true
            };
        }

        /// <summary>
        /// Metoda inicjalizująca kolekcję dostępnych pojazdów.
        /// </summary>
        private void InitCarsCollection()
        {
            try
            {
                carCollection = new Entities();
                xmlAccess.WaitOne();
                XmlNodeList carsList = document.GetElementsByTagName("car");
                xmlAccess.ReleaseMutex();
                IntRect textureRect, damageRect;
                foreach (XmlNode car in carsList)
                {
                    textureRect = ParseAttributeRect(car.Attributes["texture_rect"]);
                    damageRect  = ParseAttributeRect(car.Attributes["damage_rect"]);

                    Entity entity = new Entity(GetTexture(TYPE.CAR), textureRect, damageRect, TYPE.CAR);
                    entity.CreateMovementCompontent(
                        ParseAttributeMoving(car.Attributes["acceleration"]),
                        ParseAttributeMoving(car.Attributes["deceleration"]),
                        ParseAttributeMoving(car.Attributes["max_velocity"])
                    );
                    carCollection.Add(entity);
                }
            }
            catch (Exception exception)
            {
                Program.ShowError(exception);
            }
        }

        /// <summary>
        /// Metoda inicjalizująca słownik z cyframi.
        /// </summary>
        private void InitNumbers()
        {
            numbers = new Numbers();
            for (int i = 0; i < 11; i++)
                numbers.Add(i, new Sprite(Tnumbers, new IntRect(i*64, 0, 64, 82)));
        }

        /// <summary>
        /// Metoda inicjalizująca obiekt fontu.
        /// </summary>
        private void InitFont()
        {
            font = new Font("..\\..\\resource\\fonts\\MPLUSCodeLatin-Bold.ttf");
        }

        /// <summary>
        /// Metoda inicjalizująca teksturę filtru zmiany percepcji.
        /// </summary>
        private void InitFilter()
        {
            Tfilter = new Texture("..\\..\\resource\\images\\vignette_filter.png");
        }

        /// <summary>
        /// Metoda inicjalizująca słownik z dostępnymi klawiszami.
        /// </summary>
        private void InitKeyBindings()
        {
            keys = new KeyBindings();
            xmlAccess.WaitOne();
            XmlNodeList keyBindingsList = document.GetElementsByTagName("keybindings");
            xmlAccess.ReleaseMutex();
            foreach (XmlNode keyBindings in keyBindingsList)
            {
                int key = Convert.ToInt32(keyBindings.Attributes["key"].Value);
                string value = keyBindings.Attributes["value"].Value;
                keys[(Keyboard.Key)key] = value;
            }
        }

        /// <summary>
        /// Metoda analizująca parametry prostokąta ze wskazanego atrybutu xml.
        /// </summary>
        /// <param name="attribute">Atrybut dokumentu XML, na którym ma zajść analiza.</param>
        /// <returns>Zwraca przeanalizowany parametr.</returns>
        private IntRect ParseAttributeRect(XmlAttribute attribute)
        {
            string[] values = attribute.Value.Split(' ');
            IntRect rect;
            rect.Left   = Convert.ToInt16(values[0]);
            rect.Top    = Convert.ToInt16(values[1]);
            rect.Width  = Convert.ToInt16(values[2]);
            rect.Height = Convert.ToInt16(values[3]);
            return rect;
        }

        /// <summary>
        /// Metoda analizująca parametry wektora ze wskazanego atrybutu xml.
        /// </summary>
        /// <param name="attribute">Atrybut dokumentu XML, na którym ma zajść analiza.</param>
        /// <returns>Zwraca przeanalizowany wektor.</returns>
        private Vector2f ParseAttributeMoving(XmlAttribute attribute)
        {
            string[] values = attribute.Value.Split(' ');
            Vector2f vector;
            vector.X = float.Parse(values[0]);
            vector.Y = float.Parse(values[1]);
            return vector;
        }

        /// <summary>
        /// Metoda zwracająca teksturę w zależności od typu.
        /// </summary>
        /// <param name="type">Typ tekstury elementu.</param>
        /// <returns>Zwraca żądaną teksturę.</returns>
        public Texture GetTexture(TYPE type)
        {
            return textures[type];
        }
    }
}
