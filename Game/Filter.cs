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
            // utworzenie wyświetlanego kształtu w postaci prostokąta
            shape = new RectangleShape()
            {
                Texture = t, // referencja do tekstury
            };
            // wielkość początkowa filtru
            primarySize = new Vector2f(t.Size.X, t.Size.Y);
            // stan początkowy - filtr nie jest wyświetlany
            visible = false;
            // stan początkowy - maksymalna skala filtru
            setScale = maxScale;
            curScale = maxScale;
            // wyznaczenie wielkości okna od skali
            Vector2f newSize = new Vector2f(primarySize.X * curScale, primarySize.Y * curScale);
            shape.Size = new Vector2f(newSize.X, newSize.Y);
            // ustawienie punktu odniesienia na sam środek filtru
            shape.Origin = new Vector2f(newSize.X / 2f, newSize.Y / 2f);
        }

        /// <summary>
        /// Metoda wyznaczająca zadaną skalę filtru.
        /// </summary>
        /// <param name="drinkLevel">Aktualny poziom alkoholu.</param>
        public void CalcScale(float drinkLevel)
        {
            // poziom zawężany do 9.99
            float level = (drinkLevel > 9.99f) ? 9.99f : drinkLevel;
            // obliczenie skali
            float scale = maxScale - level / 9.99f * (maxScale - minScale);
            // ustawienie zadanej skali
            setScale = scale;
        }

        /// <summary>
        /// Metoda ustawiająca widzialność filtru.
        /// </summary>
        /// <param name="visible">Widzialność filtru.</param>
        public void SetVisible(bool visible)
        {
            // ustawienie widzialności filtru
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
            // aktualizacja pozycji
            // filtr podąża za samochodem
            shape.Position = new Vector2f(
                position.X, 
                position.Y + carSize.Y / 2f
            );
            // sprawdzenie warunku aktualnej skali
            if (curScale > setScale)
            {
                // gdy zadana skala jest wieksza to zwiekszamy obecną skalę
                curScale -= stepScale;
                if (curScale < setScale)
                    curScale = setScale;
            }
            else if (curScale < setScale)
            {
                // gdy zadana skala jest mniejsza to zmniejszamy obecną skalę
                curScale += stepScale;
                if (curScale > setScale)
                    curScale = setScale;
            }
            // wyznaczenie wielkości filtru do nowo obliczonej skali
            Vector2f newSize = new Vector2f(primarySize.X * curScale, primarySize.Y * curScale);
            shape.Size = new Vector2f(newSize.X, newSize.Y);
            // wyznaczenie nowego punktu odniesienia na sam środek skalowanego filtru
            shape.Origin = new Vector2f(newSize.X / 2f, newSize.Y / 2f);
        }

        /// <summary>
        /// Metoda renderująca filtr we wskazanym oknie.
        /// </summary>
        /// <param name="window">Referencja do głównego okna gry.</param>
        public void Render(ref RenderWindow window)
        {
            // jeśli ustawiono widzialność filtru, jest on wyświetlany w oknie
            if (visible)
                window.Draw(shape);
        }
    }
}
