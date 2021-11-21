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
    public class GUI
    {
        public class Component
        {
            public enum STATE { IDLE, HOVER, ACTIVE };
            public delegate void Function();
            public Function onClick;
            public Function onMouseOver;

            public STATE state;
            public SFML.Graphics.Text text;
            public RectangleShape shape;
            public Color
                textIdleColor,
                textHoverColor,
                textActiveColor,
                shapeIdleColor,
                shapeHoverColor,
                shapeActiveColor;

            public Component(
                Vector2f shapeSize,
                Color idle,
                Color hover,
                Color active
            )
            {
                state = STATE.IDLE;
                textIdleColor = idle;
                textHoverColor = hover;
                textActiveColor = active;
            }

            public virtual void Update(ref RenderWindow window) { }
            public virtual void Render(ref RenderWindow window) { }
        }

        public class Button : Component
        {

            public Button(
                Vector2f shapeSize,
                Vector2f centrePos,
                uint characterSize,
                string text,
                Font font,
                Color idle,
                Color hover,
                Color active
            ) : base(shapeSize, centrePos, text, font, idle, hover, active)
            {
                shape = new RectangleShape(shapeSize)
                {
                    FillColor = new Color(0, 0, 0, 0),
                    Origin = new Vector2f(shapeSize.X / 2f, shapeSize.Y / 2f),
                    Position = new Vector2f(centrePos.X, centrePos.Y)
                };

                this.text = new SFML.Graphics.Text(text, font)
                {
                    Position = new Vector2f(centrePos.X, centrePos.Y),
                    FillColor = new Color(idle),
                    OutlineColor = new Color(Color.Black),
                    OutlineThickness = 1f
                };
                this.text.CharacterSize = characterSize;
                Vector2f origin = new Vector2f(0f, 0f)
                {
                    X = this.text.GetGlobalBounds().Width / 2f,
                    Y = shapeSize.Y / 2f
                };
                this.text.Origin = new Vector2f(origin.X, origin.Y);
                this.text.Position = new Vector2f(centrePos.X, centrePos.Y);
            }

            public override void Update(ref RenderWindow window)
            {
                Vector2i mousePos = Mouse.GetPosition(window);
                if (shape.GetGlobalBounds().Contains(mousePos.X, mousePos.Y))
                {
                    text.FillColor = new Color(textHoverColor);
                    onMouseOver?.Invoke();

                    if (Mouse.IsButtonPressed(Mouse.Button.Left))
                    {
                        text.FillColor = new Color(textActiveColor);
                        onClick?.Invoke();
                    }
                }
                else
                {
                    text.FillColor = new Color(textIdleColor);
                }
            }

            public override void Render(ref RenderWindow window)
            {
                //shape.FillColor = new Color(Color.Red);
                window.Draw(shape);
                window.Draw(text);
            }
        }

        public class Text : Component
        {
            public Text(
                Vector2f centrePos,
                uint characterSize,
                string text,
                Font font,
                Color color
            ) : base(new Vector2f(), centrePos, text, font, color, color, color)
            {
                //base(, centrePos, text, font, color, color, color);
            }

            public override void Update(ref RenderWindow window)
            {
                base.Update(ref window);
            }

            public override void Render(ref RenderWindow window)
            {
                //shape.FillColor = new Color(Color.Red);
                window.Draw(shape);
                window.Draw(text);
            }
        }

        public class List : Component
        {
            List<ListItem> items;

            public List(
                Vector2f shapeSize,
                Vector2f centrePos,
                string text,
                Font font,
                Color idle,
                Color hover,
                Color active
            ) : base(shapeSize, centrePos, text, font, idle, hover, active)
            {

            }

            public override void Update(ref RenderWindow window)
            {
                foreach (var item in items)
                    item.Update(ref window);
            }

            public override void Render(ref RenderWindow window)
            {
                foreach (var item in items)
                    item.Render(ref window);
            }
        }

        public class ListItem : Component
        {
            public ListItem(
                Vector2f shapeSize,
                Vector2f centrePos,
                string text,
                Font font,
                Color idle,
                Color hover,
                Color active
            ) : base(shapeSize, centrePos, text, font, idle, hover, active)
            {

            }

            public override void Update(ref RenderWindow window)
            {

            }

            public override void Render(ref RenderWindow window)
            {

            }
        }
    }
}
