using SFML.Graphics;
using SFML.System;

namespace Game
{
    public class Filter
    {
        RectangleShape shape;
        Vector2f primarySize;
        bool visible;

        public float minScale = 0.6f;
        public float maxScale = 2.5f;
        public float curScale = 0f;
        public float setScale = 0f;
        public float stepScale = 0.001f;

        public Filter(Texture t, uint width, uint height)
        {
            shape = new RectangleShape()
            {
                Texture = t,
            };
            primarySize = new Vector2f(t.Size.X, t.Size.Y);
            visible = false;
            setScale = maxScale;
            curScale = maxScale;
            UpdateSize();
        }

        public void CalcScale(float drinkLevel)
        {
            float level = (drinkLevel > 9.99f) ? 9.99f : drinkLevel;
            float scale = maxScale - level / 9.99f * (maxScale - minScale);

            setScale = scale;
        }

        public void SetVisible(bool visible)
        {
            this.visible = visible;
        }

        public bool GetVisible()
        {
            return visible;
        }

        public void Update(Vector2f position, Vector2f carSize)
        {
            shape.Position = new Vector2f(
                position.X, 
                position.Y + carSize.Y / 2f
            );

            if (curScale > setScale)
            {
                curScale -= stepScale;
                if (curScale < setScale)
                    curScale = setScale;
            }
            else if (curScale < setScale)
            {
                curScale += stepScale;
                if (curScale > setScale)
                    curScale = setScale;
            }

            UpdateSize();
        }

        private void UpdateSize()
        {
            Vector2f newSize = new Vector2f(primarySize.X * curScale, primarySize.Y * curScale);
            shape.Size = new Vector2f(newSize.X, newSize.Y);
            shape.Origin = new Vector2f(newSize.X / 2f, newSize.Y / 2f);
        }

        public void Render(ref RenderWindow window)
        {
            if (visible)
                window.Draw(shape);
        }
    }
}
