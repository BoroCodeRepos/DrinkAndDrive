using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;



namespace Game
{
    using Numbers = Dictionary<int, Sprite>;
    using Entities = List<Entity>;

    class Resources
    {
        public class Options
        {
            public uint winWidth;
            public uint winHeight;
            public string winTitle;
        }

        public XmlDocument document;        // dokument XML
        public Sprite background;           // tło gry
        public RectangleShape dmgBoxL;      // lewe pobocze
        public RectangleShape dmgBoxR;      // prawe pobocze
        public RectangleShape shader;       // ściemnianie ekranu
        public RectangleShape coins;        // kształt stosu monet
        public Texture Tcars;               // textura wszystkich samochodów
        public Texture Tnumbers;            // textura z numerami
        public Texture Thearts;             // animoana textura serca
        public Texture Texplosion;          // textura eksplozji
        public Texture TcoinAnimated;       // textura animacji monety
        public Entities carCollection;      // kolekcja wszystkich dostępnych samochodów
        public Options options;             // ustawienia okna

        public Numbers numbers;             // słownik liczb

        public Resources() { }

        public void Init()
        {
            try
            {
                InitXMLdoc();
                InitOptions();
                InitBackground();
                InitShader();
                InitCoinsShape();
                InitTextures();
                InitCarsCollection();
                InitNumbers();
            }
            catch (Exception exception)
            {
                Program.ShowError(exception);
            }
        }

        private void InitXMLdoc()
        {
            document = new XmlDocument();
            document.Load("..\\..\\..\\Config.xml");
        }

        private void InitOptions()
        {
            options = new Options();
            XmlElement root = document.DocumentElement;
            options.winWidth = Convert.ToUInt16(root["window"].Attributes["width"].Value);
            options.winHeight = Convert.ToUInt16(root["window"].Attributes["height"].Value);
            options.winTitle = root["window"].Attributes["title"].Value;
        }

        private void InitBackground()
        {
            background = new Sprite(new Texture("..\\..\\..\\resource\\images\\background.png"));
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

        private void InitShader()
        {
            shader = new RectangleShape
            {
                Position = new Vector2f(0f, 0f),
                FillColor = new Color(0, 0, 0, 0),
                Size = new Vector2f(
                    (float)options.winWidth,
                    (float)options.winHeight
                )
            };
        }

        private void InitCoinsShape()
        {
            coins = new RectangleShape
            {
                Position = new Vector2f(10f, 10f),
                Size = new Vector2f(120f, 120f),
                Texture = new Texture("..\\..\\..\\resource\\images\\coins.png")
            };
        }

        private void InitTextures()
        {
            Tnumbers = new Texture("..\\..\\..\\resource\\images\\numbers.png")
            {
                Smooth = true
            };
            Tcars = new Texture("..\\..\\..\\resource\\images\\cars.png")
            {
                Smooth = true
            };
            Thearts = new Texture("..\\..\\..\\resource\\images\\heart_animated.png")
            {
                Smooth = true
            };
            TcoinAnimated = new Texture("..\\..\\..\\resource\\images\\coin_animated.png")
            {
                Smooth = true
            };
            Texplosion = new Texture("..\\..\\..\\resource\\images\\explosion_animated.png")
            {
                Smooth = true
            };
        }

        private void InitCarsCollection()
        {
            try
            {
                carCollection = new List<Entity>();
                XmlNodeList carsList = document.GetElementsByTagName("car");
                IntRect textureRect, damageRect;
                Color color = new Color(255, 255, 0, 128);
                foreach (XmlNode car in carsList)
                {
                    textureRect = ParseAttributeRect(car.Attributes["texture_rect"]);
                    damageRect  = ParseAttributeRect(car.Attributes["damage_rect"]);

                    Entity entity = new Entity(Tcars, textureRect, damageRect, color);
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

        private void InitNumbers()
        {
            numbers = new Numbers();
            for (int i = 0; i < 10; i++)
                numbers.Add(i, new Sprite(Tnumbers, new IntRect(i*64, 0, 64, 82)));
        }

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

        private Vector2f ParseAttributeMoving(XmlAttribute attribute)
        {
            string[] values = attribute.Value.Split(' ');
            Vector2f vector;
            vector.X = float.Parse(values[0]);
            vector.Y = float.Parse(values[1]);
            return vector;
        }
    }
}
