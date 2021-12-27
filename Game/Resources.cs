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
            // inicjalizacja dokumentu XML i fontów przed wywołaniem wątków
            InitXMLdoc();
            InitFont();
            // mutext dostępu do xml
            xmlAccess = new Mutex();
            // utworzenie wątków ładujących zasoby
            tTextures = new Thread(() => SafeExecute(LoadTextures, Program.ShowError));
            tSounds = new Thread(() => SafeExecute(LoadSounds, Program.ShowError));
            tOthers = new Thread(() => SafeExecute(LoadOthers, Program.ShowError));
            // start wątków
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
                // bezpieczne wykonanie akcji
                action.Invoke();
            }
            catch (Exception ex)
            {
                // komunikacja błędu
                handler(ex);
            }
        }

        /// <summary>
        /// Metoda ładująca wszystkie tekstury do pamięci (wykonanie w wątku).
        /// </summary>
        private void LoadTextures()
        {
            // inicjalizacja opcji i tekstur w wątku
            InitOptions();
            InitBackground();
            InitCoinsShape();
            InitTextures();
            InitNumbers();
            InitCarsCollection();
        }

        /// <summary>
        /// Metoda ładująca pozostałe elementy gry do pamięci (wykonanie w wątku).
        /// </summary>
        private void LoadOthers()
        {
            // utworzenie obiektu
            keys = new KeyBindings();
            // zablokowanie dostępu do xml
            xmlAccess.WaitOne();
            XmlNodeList keyBindingsList = document.GetElementsByTagName("keybindings");
            xmlAccess.ReleaseMutex();
            // pobranie wartości i odblokowanie xml
            foreach (XmlNode keyBindings in keyBindingsList)
            {
                // zapisanie wszystkich elementów w kolekcji
                int key = Convert.ToInt32(keyBindings.Attributes["key"].Value);
                string value = keyBindings.Attributes["value"].Value;
                keys[(Keyboard.Key)key] = value;
            }
        }

        /// <summary>
        /// Metoda ładująca dostępne dźwięki gry do pamięci (wykonanie w wątku).
        /// </summary>
        private void LoadSounds()
        {
            // utworzenie obiektu
            sounds = new Sounds();
            // pobranie nazwy wszystkich utworów z folderu
            string[] fileNames = Directory.GetFiles(C.SOUNDS_DIRECTORY_PATH, "*.*");
            foreach (var file in fileNames)
            {
                var splitedPath = file.Split('\\');
                // pobranie nazwy z rozszerzeniem
                var filename = splitedPath[splitedPath.Length - 1];
                // pobranie samej nazwy
                var key = filename.Split('.')[0];
                // utworzenie obiektu
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
            // utworzenie obiektu
            List<string> threadList = new List<string>();
            // jeżeli jakiś wątek jest przy życiu to zostanie dodany do listy
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
            // utworzenie i załadowanie dokumentu XML
            document = new XmlDocument();
            document.Load(C.XML_DOCUMENT_PATH);
        }

        /// <summary>
        /// Metoda inicjalizująca parametry głównego okna gry.
        /// </summary>
        private void InitOptions()
        {
            // utworzenie obiektu
            options = new Options();
            // blokada dostępu do zasobów XML
            xmlAccess.WaitOne();
            XmlElement root = document.DocumentElement;
            xmlAccess.ReleaseMutex();
            // zwolnienie mutexa i konwersja wartości pobranych z XML
            options.winWidth = Convert.ToUInt16(root["window"].Attributes["width"].Value);
            options.winHeight = Convert.ToUInt16(root["window"].Attributes["height"].Value);
            options.winTitle = root["window"].Attributes["title"].Value;
        }

        /// <summary>
        /// Metoda inicjalizująca obiekt tła gry (jezdni).
        /// </summary>
        private void InitBackground()
        {
            // utworzenie obiektu tła i ustalenie punktu odniesienia
            background = new Sprite(new Texture(C.BACKGROUND_TEXTURE_PATH));
            background.Origin = new Vector2f(
                (float)(background.Texture.Size.X - options.winWidth) / 2f,
                0f
            );
            // obiekt lewego pobocza
            dmgBoxL = new RectangleShape
            {
                Size = new Vector2f(272f, background.Texture.Size.Y),
                Origin = background.Origin,
                FillColor = C.ENTITY_LR_ROAD_HITBOX_COLOR,
                OutlineThickness = 1f,
                OutlineColor = new Color(Color.Black)
            };
            // obiekt prawego pobocza
            dmgBoxR = new RectangleShape
            {
                Size = new Vector2f(272f, background.Texture.Size.Y),
                Origin = new Vector2f(-871f, 0f),
                FillColor = C.ENTITY_LR_ROAD_HITBOX_COLOR,
                OutlineThickness = 1f,
                OutlineColor = new Color(Color.Black)
            };
        }

        /// <summary>
        /// Metoda inicjalizująca obiekt kształtu monet.
        /// </summary>
        private void InitCoinsShape()
        {
            // utworzenie elementu monet
            coins = new RectangleShape
            {
                Position = new Vector2f(10f, 10f),
                Size = new Vector2f(120f, 120f),
                Texture = new Texture(C.COINS_TEXTURE_PATH)
            };
        }

        /// <summary>
        /// Metoda inicjalizująca wszystkie obiekty tekstur.
        /// </summary>
        private void InitTextures()
        {
            // inicjalizacja tekstur elementów gry, cyfr i eksplozji 
            textures = new Textures
            {
                [TYPE.HEART] = new Texture(C.HEART_ANIMATION_PATH) { Smooth = true },
                [TYPE.COIN] = new Texture(C.COIN_ANIMATION_PATH) { Smooth = true },
                [TYPE.BEER] = new Texture(C.BOTTLE_CAP_ANIMATION_PATH) { Smooth = true },
                [TYPE.CAR] = new Texture(C.CARS_TEXTURE_PATH) { Smooth = true }
            };
            Tnumbers = new Texture(C.NUMBERS_TEXTURE_PATH)
            {
                Smooth = true
            };

            Texplosion = new Texture(C.EXPLOSION_ANIMATION_PATH)
            {
                Smooth = true
            };
            Tfilter = new Texture(C.FILTER_TEXTURE_PATH)
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
                // utworzenie obiektu
                carCollection = new Entities();
                // blokada dostępu do XML
                xmlAccess.WaitOne();
                XmlNodeList carsList = document.GetElementsByTagName("car");
                xmlAccess.ReleaseMutex();
                // zwolnienie dostępu do XML
                IntRect textureRect, damageRect;
                foreach (XmlNode car in carsList)
                {
                    // przegląd przez wszystkie samochody i zapisanie ich obiektów w pamięci
                    // pobranie i analiza wartości IntRect z atrybutów
                    textureRect = ParseAttributeRect(car.Attributes["texture_rect"]);
                    damageRect  = ParseAttributeRect(car.Attributes["damage_rect"]);
                    // utworzenie obiektu pojazdu
                    Entity entity = new Entity(GetTexture(TYPE.CAR), textureRect, damageRect, TYPE.CAR);
                    // utworzenie movement komponentu
                    entity.CreateMovementCompontent(
                        ParseAttributeMoving(car.Attributes["acceleration"]),
                        ParseAttributeMoving(car.Attributes["deceleration"]),
                        ParseAttributeMoving(car.Attributes["max_velocity"])
                    );
                    // dodanie do kolekcji
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
            // utworzenie i dodanie do kolekcji kolejnych cyfr
            numbers = new Numbers();
            for (int i = 0; i < 11; i++)
                numbers.Add(i, new Sprite(Tnumbers, new IntRect(i*64, 0, 64, 82)));
        }

        /// <summary>
        /// Metoda inicjalizująca obiekt fontu.
        /// </summary>
        private void InitFont()
        {
            font = new Font(C.FONT_PATH);
        }

        /// <summary>
        /// Metoda analizująca parametry prostokąta ze wskazanego atrybutu xml.
        /// </summary>
        /// <param name="attribute">Atrybut dokumentu XML, na którym ma zajść analiza.</param>
        /// <returns>Zwraca przeanalizowany parametr.</returns>
        private IntRect ParseAttributeRect(XmlAttribute attribute)
        {
            // analiza wartości atrybutu XML i przejście na wartości IntRect
            string[] values = attribute.Value.Split(' ');
            IntRect rect;
            // pobranie kolejnych wartości z atrybutu
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
            // analiza atrybutu XML i przejście na wartości wektora
            string[] values = attribute.Value.Split(' ');
            Vector2f vector;
            // pierwsza zmienna X, druga Y
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
