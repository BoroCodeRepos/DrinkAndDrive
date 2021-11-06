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
        public Texture texture;
        public IntRect textureRect;
        public FloatRect bounds;
        public RectangleShape damageBox;
        public Sprite sprite;
        public bool isAnimated;
        public Animation animation;

        public Entity(
            Texture texture,
            IntRect textureRect,
            FloatRect bounds,
            RectangleShape damageBox
        )
        {
            this.texture = texture;
            this.textureRect = textureRect;
            this.bounds = bounds;
            this.damageBox = damageBox;
            this.sprite = new Sprite(texture, textureRect);
            this.isAnimated = false;
        }

        public void CreateAnimation()
        {

        }
    }
}
