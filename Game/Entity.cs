using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.Graphics;
using SFML.Window;
using SFML.System;

namespace Game
{
    class Entity
    {
        public uint id;
        public Texture texture;
        public IntRect textureRect;
        public FloatRect bounds;
        public RectangleShape damageBox;
        public Sprite sprite;
    }
}
