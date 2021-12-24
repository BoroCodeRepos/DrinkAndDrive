using SFML.Graphics;
using SFML.System;

namespace Game
{
    /// <summary>
    /// Klasa odpowiedzialna za efekt zmianę percepcji.
    /// </summary>
    public class Filter
    {
        /// <summary>Kształt filtru.</summary>
        RectangleShape shape;
        /// <summary>Wielkość filtru.</summary>
        Vector2f primarySize;
        /// <summary>Zmienna stanu widzialności filtru.</summary>
        bool visible;
        /// <summary>Stała przechowująca minimalną dostępną skalę filtru.</summary>
        public const float minScale = 0.6f;
        /// <summary>Stała przechowująca maksylaną dostępną skalę filtru.</summary>
        public const float maxScale = 2.5f;
        /// <summary>Stała przechowująca krok skali filtru.</summary>
        public const float stepScale = 0.001f;
        /// <summary>Zmienna aktualnej skali filtru.</summary>
        public float curScale = 0f;
        /// <summary>Zmienna zadanej skali filtru.</summary>
        public float setScale = 0f;

        /// <summary>
        /// Konstruktor - inicjalizowanie parametrów.
        /// </summary>
        /// <param name="t">Tekstura filtru.</param>
        public Filter(Texture t)
        {
            shape = new RectangleShape()
            {
                Texture = t,
            };
            primarySize = new Vector2f(t.Size.X, t.Size.Y);
            visible = false;
            setScale = maxScale;
            curScale = maxScale;
            Vector2f newSize = new Vector2f(primarySize.X * curScale, primarySize.Y * curScale);
            shape.Size = new Vector2f(newSize.X, newSize.Y);
            shape.Origin = new Vector2f(newSize.X / 2f, newSize.Y / 2f);
        }

        /// <summary>
        /// Metoda wyznaczająca zadaną skalę filtru.
        /// </summary>
        /// <param name="drinkLevel">Aktualny poziom alkoholu.</param>
        public void CalcScale(float drinkLevel)
        {
            float level = (drinkLevel > 9.99f) ? 9.99f : drinkLevel;
            float scale = maxScale - level / 9.99f * (maxScale - minScale);

            setScale = scale;
        }

        /// <summary>
        /// Metoda ustawiająca widzialność filtru.
        /// </summary>
        /// <param name="visible">Widzialność filtru.</param>
        public void SetVisible(bool visible)
        {
            this.visible = visible;
        }

        /// <summary>
        /// Metoda zwracająca widzialność filtru.
        /// </summary>
        /// <returns>Widzialność filtru.</returns>
        public bool GetVisible()
        {
            return visible;
        }

        /// <summary>
        /// Metoda aktualizująca filtr.
        /// </summary>
        /// <param name="position">Aktualna pozycja pojazdu.</param>
        /// <param name="carSize">Wielkość tekstury pojazdu.</param>
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

            Vector2f newSize = new Vector2f(primarySize.X * curScale, primarySize.Y * curScale);
            shape.Size = new Vector2f(newSize.X, newSize.Y);
            shape.Origin = new Vector2f(newSize.X / 2f, newSize.Y / 2f);
        }

        /// <summary>
        /// Metoda renderująca filtr we wskazanym oknie.
        /// </summary>
        /// <param name="window">Referencja do głównego okna gry.</param>
        public void Render(ref RenderWindow window)
        {
            if (visible)
                window.Draw(shape);
        }
    }
}
