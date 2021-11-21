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
            public Color textColor, shapeColor;

            public Component() { }

            public Component(uint characterSize, string text, Font font, Color textColor, Color shapeColor)
            {
                state = STATE.IDLE;
                this.textColor = textColor;
                this.shapeColor = shapeColor;
                this.text = new SFML.Graphics.Text(text, font);
            }

            public virtual void Update(ref RenderWindow window) { }
            public virtual void Render(ref RenderWindow window) { }
        }

        public class Button : Component
        {
            public Color
                textIdleColor,
                textHoverColor,
                textActiveColor;

            public Button(
                Vector2f shapeSize,
                Vector2f centrePos,
                uint characterSize,
                string text,
                Font font,
                Color idle,
                Color hover,
                Color active
            ) : base(characterSize, text, font, idle, new Color(Color.Transparent))
            {
                textIdleColor = idle;
                textHoverColor = hover;
                textActiveColor = active;
                shape = new RectangleShape(shapeSize)
                {
                    FillColor = new Color(0, 0, 0, 0),
                    Origin = new Vector2f(shapeSize.X / 2f, shapeSize.Y / 2f),
                    Position = new Vector2f(centrePos.X, centrePos.Y)
                };

                this.text = new SFML.Graphics.Text(text, font)
                {
                    CharacterSize = characterSize,
                    Position = new Vector2f(centrePos.X, centrePos.Y),
                    FillColor = new Color(idle),
                    OutlineColor = new Color(Color.Black),
                    OutlineThickness = 1f
                };

                Vector2f origin = new Vector2f(0f, 0f)
                {
                    X = this.text.GetGlobalBounds().Width / 2f,
                    Y = shapeSize.Y / 2f
                };
                this.text.Origin = new Vector2f(origin.X, origin.Y);
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
            ) : base(characterSize, text, font, color, new Color(Color.Transparent))
            {
                this.text = new SFML.Graphics.Text(text, font)
                {
                    CharacterSize = characterSize,
                    Position = new Vector2f(centrePos.X, centrePos.Y),
                    FillColor = new Color(color),
                    OutlineColor = new Color(Color.Black),
                    OutlineThickness = 1f,
                };

                this.shape = new RectangleShape()
                {
                    Size = new Vector2f(this.text.GetGlobalBounds().Width, this.text.GetGlobalBounds().Height),
                    Position = new Vector2f(centrePos.X, centrePos.Y),
                    FillColor = new Color(Color.Transparent),
                    Origin = new Vector2f(
                        this.text.GetGlobalBounds().Width / 2f,
                        this.text.GetGlobalBounds().Height / 2f
                    )
                };

                this.text.Origin = new Vector2f(
                    (this.text.GetGlobalBounds().Width + characterSize / 8f) / 2f,
                    (this.text.GetGlobalBounds().Height + characterSize / 2f) / 2f
                );
            }


            public override void Update(ref RenderWindow window) { }

            public override void Render(ref RenderWindow window)
            {
                //shape.FillColor = new Color(Color.Red);
                window.Draw(shape);
                window.Draw(text);
            }
        }

        public class Texture : Component
        {
            RectangleShape textureShape;
            Color OutlineIdle, OutlineHover, OutlineActive;

            public Texture(
                int id,
                List<Entity> carCollection,
                Vector2f shapeSize,
                Vector2f textureSize,
                Vector2f centrePos,
                Color OutlineIdle,
                Color OutlineHover,
                Color OutlineActive,
                float rotation = 0f
            )
            {
                this.OutlineIdle = OutlineIdle;
                this.OutlineHover = OutlineHover;
                this.OutlineActive = OutlineActive;
                shape = new RectangleShape(shapeSize)
                {
                    FillColor = new Color(0, 0, 0, 0),
                    Origin = new Vector2f(shapeSize.X / 2f, shapeSize.Y / 2f),
                    Position = new Vector2f(centrePos.X, centrePos.Y),
                    OutlineThickness = 10f,
                    OutlineColor = new Color(OutlineIdle),
                    Rotation = rotation,
                };
                Sprite car = carCollection[id].sprite;
                textureShape = new RectangleShape(textureSize)
                {
                    Origin = new Vector2f(textureSize.X / 2f, textureSize.Y / 2f),
                    Position = new Vector2f(centrePos.X, centrePos.Y),
                    Texture = new SFML.Graphics.Texture(car.Texture),
                    TextureRect = new IntRect(
                        car.TextureRect.Left,
                        car.TextureRect.Top,
                        car.TextureRect.Width,
                        car.TextureRect.Height
                    ),
                    Rotation = rotation,
                };
            }

            public override void Update(ref RenderWindow window)
            {
                Vector2i mousePos = Mouse.GetPosition(window);
                if (shape.GetGlobalBounds().Contains(mousePos.X, mousePos.Y))
                {
                    shape.OutlineColor = new Color(OutlineHover);
                    onMouseOver?.Invoke();

                    if (Mouse.IsButtonPressed(Mouse.Button.Left))
                    {
                        shape.OutlineColor = new Color(OutlineActive);
                        onClick?.Invoke();
                    }
                }
                else
                {
                    shape.OutlineColor = new Color(OutlineIdle);
                }
            }

            public override void Render(ref RenderWindow window)
            {
                window.Draw(shape);
                window.Draw(textureShape);
            }
        }

        public class List : Component
        {
            List<ListItems> items;

            public List()
            {
                items = new List<ListItems>();
            }

            public void Add(ListItems item)
            {
                items.Add(item);
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

        public class ListItems : Component
        {
            List<Component> components;

            public ListItems()
            {
                components = new List<Component>();
            }

            public void Add(Component c)
            {
                components.Add(c);
            }

            public void Clear()
            {
                components.Clear();
            }

            public override void Update(ref RenderWindow window)
            {
                foreach (var item in components)
                    item.Update(ref window);
            }

            public override void Render(ref RenderWindow window)
            {
                foreach (var item in components)
                    item.Render(ref window);
            }
        }
    }
}
