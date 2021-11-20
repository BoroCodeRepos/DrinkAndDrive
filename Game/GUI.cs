using System;
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
    class Component
    {
        public enum STATE { IDLE, HOVER, ACTIVE };
        public delegate void Function(Component c);
        public Function onClick;
        public Function onMouseOver;

        public STATE state;
        public Text text;
        public RectangleShape shape;
        public Color
            textIdleColor,
            textHoverColor,
            textActiveColor;

        public Component(
            Vector2f shapeSize, 
            Vector2f centrePos, 
            string text, 
            Font font, 
            Color idle, 
            Color hover, 
            Color active
        )
        {
            state = STATE.IDLE; 
            shape = new RectangleShape(shapeSize)
            {
                FillColor = new Color(0, 0, 0, 0),
                Origin = new Vector2f(shapeSize.X / 2f, shapeSize.Y / 2f),
                Position = new Vector2f(centrePos.X, centrePos.Y)
            };

            this.text = new Text(text, font)
            {
                FillColor = new Color(idle),
                CharacterSize = 78,
                OutlineColor = new Color(Color.Black),
                OutlineThickness = 1f
            };

            Vector2f origin = new Vector2f(0f, 0f);
            origin.X = this.text.GetGlobalBounds().Width / 2f;
            origin.Y = shapeSize.Y / 2f;
            this.text.Origin = new Vector2f(origin.X, origin.Y);
            this.text.Position = new Vector2f(centrePos.X, centrePos.Y);

            textIdleColor = idle;
            textHoverColor = hover;
            textActiveColor = active;
        }

        public virtual void Update(ref RenderWindow relativeTo) { }
        public virtual void Render(ref RenderWindow window) { }
    }

    class Button : Component
    {

        public Button(
            Vector2f shapeSize, 
            Vector2f centrePos, 
            string text, 
            Font font, 
            Color idle, 
            Color hover, 
            Color active
        ) : base(shapeSize, centrePos, text, font, idle, hover, active)
        {
            this.text.CharacterSize = 78;
        }

        public override void Update(ref RenderWindow relativeTo)
        {
            Vector2i mousePos = Mouse.GetPosition(relativeTo);
            if (shape.GetGlobalBounds().Contains(mousePos.X, mousePos.Y))
            {
                text.FillColor = new Color(textHoverColor);
                onMouseOver?.Invoke(this);

                if (Mouse.IsButtonPressed(Mouse.Button.Left))
                {
                    text.FillColor = new Color(textActiveColor);
                    onClick?.Invoke(this);
                }
            }
            else
            {
                text.FillColor = new Color(textIdleColor);
            }
        }

        public override void Render(ref RenderWindow window)
        {
            window.Draw(shape);
            window.Draw(text);
        }
    }
}
