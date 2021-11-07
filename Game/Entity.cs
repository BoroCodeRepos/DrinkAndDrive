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
        public string name;
        public Texture texture;
        public IntRect textureRect;
        public IntRect damageRect;
        public IntRect offset;
        public RectangleShape damageBox;
        public Sprite sprite;
        public DIRECTION dir;
        public bool isAnimated;
        public Animation animation;

        public Entity(
            Texture texture,
            IntRect textureRect,
            IntRect damageRect,
            Color damageRectColor
        )
        {
            this.texture = texture;
            this.textureRect = textureRect;
            sprite = new Sprite(texture, textureRect);
            sprite.Origin = new Vector2f(
                0f,
                (float)(textureRect.Height) / 2f
            );
            isAnimated = false;

            offset.Left = textureRect.Left - damageRect.Left;
            offset.Top  = textureRect.Top  - damageRect.Top;
            damageBox = new RectangleShape(
                new Vector2f(
                    (float)damageRect.Width,
                    (float)damageRect.Height
                )
            );
            //damageBox.Origin = 
            damageBox.FillColor = damageRectColor;
            damageBox.OutlineThickness = 1f;
        }

        public void CreateAnimation()
        {

        }
    }
}
