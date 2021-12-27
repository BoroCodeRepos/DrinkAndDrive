using SFML.System;

namespace Game
{
    /// <summary>
    /// Klasa wyznaczająca parametry zaawansowanego ruchu obiektu. 
    /// </summary>
    public class MovementComponent
    {
        /// <summary>Zmienna przechowująca parametry przyśpieszenia obiektu.</summary>
        public Vector2f acceleration;  
        /// <summary>Zmienna przechowująca parametry hamowania obiektu.</summary>
        public Vector2f deceleration;  
        /// <summary>Zmienna przechowująca parametry aktualnej prędkości obiektu.</summary>
        public Vector2f velocity;      
        /// <summary>Zmienna przechowująca parametry maksymalnej prędkości obiektu.</summary>
        public Vector2f maxVelocity;    
        /// <summary>Zmienna przechowująca współczynniki kierunku ruchu danego obiektu.</summary>
        public Vector2f move;           

        /// <summary>
        /// Konstruktor - inicjalizacja podstawowych parametrów ruchu.
        /// </summary>
        /// <param name="acceleration">Wielkość przyśpieszenia.</param>
        /// <param name="deceleration">Wielkość hamowania.</param>
        /// <param name="maxVelocity">Maksymalna prędkość pojazdu.</param>
        public MovementComponent(Vector2f acceleration, Vector2f deceleration, Vector2f maxVelocity)
        {
            // inicjalizacja parametrów ruchu
            this.acceleration = new Vector2f(acceleration.X, acceleration.Y);
            this.deceleration = new Vector2f(deceleration.X, deceleration.Y);
            this.maxVelocity = new Vector2f(maxVelocity.X, maxVelocity.Y);
            // aktualna prędkość i współczyniki kirunku ruchu zerowe
            velocity = new Vector2f(0f, 0f);
            move = new Vector2f(0f, 0f);
        }

        /// <summary>
        /// Konstruktor - kopia parametrów z innego komponentu ruchu.
        /// </summary>
        /// <param name="component">Utworzony obiekt zaawansowanego ruchu.</param>
        public MovementComponent(MovementComponent component)
        {
            // inicjalizacja parametrów ruchu na podstawie innego obiektu
            acceleration = new Vector2f(component.acceleration.X, component.acceleration.Y);
            deceleration = new Vector2f(component.deceleration.X, component.deceleration.Y);
            maxVelocity = new Vector2f(component.maxVelocity.X, component.maxVelocity.Y);
            // aktualna prędkość i współczynniki kierunku ruchu zerowe
            velocity = new Vector2f(0f, 0f);
            move = new Vector2f(0f, 0f);
        }

        /// <summary>
        /// Główna metoda aktualizująca ruch pojazdu, zwraca aktualną prędkość w osi X i Y.
        /// </summary>
        /// <param name="dt">Czas od poprzedniego wywołania.</param>
        /// <returns>Aktualna prędkość pojazdu w osi X i Y.</returns>
        public Vector2f Update(float dt)
        {
            // aktualizacja prędkości zgodnie z przyśpieszeniem w danym kierunku (dla dwóch osi)
            velocity.X += acceleration.X * dt * move.X;
            velocity.Y += acceleration.Y * dt * move.Y;
            // sprawdzenie maksymalnej prędkości, wyznaczenie hamowania w osi X i Y
            UpdateVelocity(
                dt,
                ref velocity.X,
                ref maxVelocity.X,
                ref deceleration.X,
                move.X );
            UpdateVelocity(
                dt,
                ref velocity.Y,
                ref maxVelocity.Y,
                ref deceleration.Y,
                1f );
            // wyznaczony parametr aktualnej prędkości obiektu
            return velocity;
        }

        /// <summary>
        /// Metoda aktualizująca jedną oś prędkości obiektu.
        /// </summary>
        /// <param name="dt">Czas od poprzedniego wywołania.</param>
        /// <param name="velocity">Referencja do zmiennej aktualnej prędkości w danej osi.</param>
        /// <param name="maxVelocity">Referencja do zmiennej maksymalnej prędkości w danej osi.</param>
        /// <param name="deceleration">Referencja do zmiennej hamowania obiektu w danej osi.</param>
        /// <param name="dir">Współczynnik kierunku ruchu obiektu w danej osi.</param>
        private void UpdateVelocity(
            float dt,
            ref float velocity,
            ref float maxVelocity,
            ref float deceleration,
            float dir
        )
        {
            if (velocity > 0f)
            {
                // sprawdzenie maksymalnej prędkości obiektu w danej osi.
                if (velocity > maxVelocity)
                    velocity = maxVelocity;
                // wyznaczenie współczynnika hamowania.
                velocity -= deceleration * dt * ((dir == 0f) ? 8f : 1f);
                // sprawdzenie warunku końca przyśpieszenia w danej osi
                if (velocity < 0f)
                    velocity = 0f;
            }
            else if (velocity < 0f)
            {
                // sprawdzenie maksymalnej prędkości obiektu w danej osi.
                if (velocity < -maxVelocity)
                    velocity = -maxVelocity;
                // wyznaczenie współczynnika hamowania.
                velocity += deceleration * dt * ((dir == 0f) ? 8f : 1f);
                // sprawdzenie warunku końca przyśpieszenia w danej osi.
                if (velocity > 0f)
                    velocity = 0f;
            }
        }
    }
}
