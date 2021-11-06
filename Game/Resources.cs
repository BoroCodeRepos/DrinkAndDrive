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
        public Texture cars;
        public Texture explosion;
        public Texture coins;
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

                explosion = new Texture("..\\..\\..\\resource\\images\\explosion_animated.png")
                {
                    Smooth = true
                };

                coins = new Texture("..\\..\\..\\resource\\images\\coins.png")
                {
                    Smooth = true
                };

                coinAnimated = new Texture("..\\..\\..\\resource\\images\\coin_animated.png")
                {
                    Smooth = true
                };
            }
            catch (Exception exception)
            {
                Program.ShowError(exception);
            }
        }
    }
}
