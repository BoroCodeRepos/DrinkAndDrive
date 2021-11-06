using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Game
{
    class Engine
    {
        Resources resources;
        float score, multiplier;
        bool showDamageBoxes;
        List<Vector2f> coins;
        List<Vector2f> beers;

        public Engine(Resources resources)
        {
            this.resources = resources;
            score = 0.0f;
            multiplier = 0.0f;
            showDamageBoxes = false;
            coins = new List<Vector2f>();
            beers = new List<Vector2f>();

        }

        public void Update(float dt)
        {

        }

        public void Render(RenderWindow window)
        {
            window.Draw(resources.background_1);
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
