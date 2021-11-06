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
    class Resources
    {
        public XmlDocument document;
        public Sprite background_1, background_2;
        public RectangleShape shader;
        public RectangleShape coins;
        public Texture cars;
        public Texture explosion;
        public Texture coinAnimated;

        public Resources() { }

        public void Init()
        {
            try
            {
                document = new XmlDocument();
                document.Load("..\\..\\..\\Config.xml");

                background_1 = new Sprite(new Texture("..\\..\\..\\resource\\images\\background.png"));
                background_2 = new Sprite(new Texture("..\\..\\..\\resource\\images\\background.png"));

                //  cars



                coins = new RectangleShape
                {
                    Position = new Vector2f(100f, 100f),
                    Size = new Vector2f(200f, 200f),
                    Texture = new Texture("..\\..\\..\\resource\\images\\coins.png")
                };

                coinAnimated = new Texture("..\\..\\..\\resource\\images\\coin_animated.png");
                explosion = new Texture("..\\..\\..\\resource\\images\\explosion_animated.png");
            }
            catch (Exception exception)
            {
                Program.ShowError(exception);
            }
        }
    }
}
