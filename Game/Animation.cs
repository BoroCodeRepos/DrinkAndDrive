using SFML.Graphics;
using SFML.System;

using System.Collections.Generic;

namespace Game
{
    /// <summary>
    /// Statyczna klasa animacji wybuchu po kolizji pojazdu gracza.
    /// </summary>
    static public class Animation
    {
        /// <summary>
        /// Obiekt tekstury wybuchu.
        /// </summary>
        static public Texture texture;
        /// <summary>
        /// Wielkość tekstury animacji.
        /// </summary>
        static public int tSize;
        /// <summary>
        /// Lista animowanych obiektów.
        /// </summary>
        static public List<AnimationState> states;

        /// <summary>
        /// Metoda tworząca nowy obiekt animacji wybuchu.
        /// </summary>
        /// <param name="position">Wskazanie pozycji wyświetlania animacji.</param>
        static public void Create(Vector2f position)
        {
            states.Add(new AnimationState(12, 65f, position));
        }

        /// <summary>
        /// Aktualizacja stanów animacji.
        /// </summary>
        /// <param name="dt">DeltaTime (czas ponownego wywołania)</param>
        static public void Update(float dt)
        {
            // inicjalizacja obiektów do usunięcia
            List<AnimationState> toDelete = new List<AnimationState>();
            foreach (var state in states)
            {
                // zwiększenie czasu ramki
                state.currentFrameTime += dt;
                // sprawdzenie warunku wyświetlenia następnej ramki animacji
                if (state.currentFrameTime > state.maxFrameTime)
                {
                    // aktualizacja czasu ramki
                    state.currentFrameTime -= state.maxFrameTime;
                    // sprawdzenie warunku końca animacji
                    if (++state.currentFrameId >= state.framesNr)
                        // koniec animacji - obiekt do usunięcia
                        toDelete.Add(state);
                    // aktualizacja obecnej ramki
                    state.currentFrame = new IntRect(tSize * state.currentFrameId, 0, tSize, tSize);
                }
            }
            // usunięcie obiektów
            foreach (var state in toDelete)
                states.Remove(state);
        }

        /// <summary>
        /// Metoda renderująca animacje we wskazanym oknie.
        /// </summary>
        /// <param name="window">Referencja do głównego okna gry.</param>
        static public void Render(ref RenderWindow window)
        {
            // utworzenie obiektu sprita
            Sprite s = new Sprite(texture);
            foreach (var state in states)
            {
                // aktualizacja parametrów sprita
                s.Position = new Vector2f(state.position.X, state.position.Y);
                s.Origin = new Vector2f(tSize / 2f, tSize / 2f);
                s.TextureRect = state.currentFrame;
                // wyświetlenie sprita na ekranie
                window.Draw(s);
            }
        }
    }
}
