using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.Graphics;
using SFML.Window;

namespace Game
{
    class GameWindow
    {
        private const int WIN_WIDTH = 1280;
        private const int WIN_HEIGHT = 720;
        private const string TITLE = "Drink & Drive";

        private RenderWindow window;
        private Color windowClearColor = Color.Black;

        public GameWindow()
        {
            // Initialization main window
            window = new RenderWindow(new VideoMode(WIN_WIDTH, WIN_HEIGHT), TITLE);
            window.Closed += new EventHandler(OnClose);
        }

        public GameWindow(uint width, uint height, string title)
        {
            // Initialization main window
            window = new RenderWindow(new VideoMode(width, height), title);
            window.Closed += new EventHandler(OnClose);
        }

        void OnClose(object sender, EventArgs e)
        {
            // Close the window when OnClose event is received
            RenderWindow window = (RenderWindow)sender;
            window.Close();
        }

        public void Draw(Drawable drawable)
        {
            window.Draw(drawable);
        }

        public void DispatchEvents()
        {
            window.DispatchEvents();
        }

        public void Display()
        {
            window.Display();
        }

        public void Clear()
        {
            window.Clear(windowClearColor);
        }

        public bool IsOpen()
        {
            return window.IsOpen;
        }
    }
}
