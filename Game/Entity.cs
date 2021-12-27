using SFML.Graphics;
using SFML.System;

using System;
using System.Collections.Generic;

namespace Game
{
    using DIR = DIRECTION;

    /// <summary>
    /// Klasa podstawowego elementu gry (monety, serca, kapsle, pojazdy).
    /// </summary>
    public class Entity
    {
        /// <summary>Typ elementu.</summary>
        public TYPE type;                    
        /// <summary>Kierunek poruszania się elementu.</summary>
        public DIR dir;                        
        /// <summary>Element wyświetlany w oknie gry.</summary>
        public Sprite sprite;                   
        /// <summary>Model kolizji elementu.</summary>
        public RectangleShape damageBox;        
        /// <summary>Komponent zaawansowanego ruchu.</summary>
        public MovementComponent movement;      
        /// <summary>Pozycja inicjalizacyjna w osi X - pozycja na pasie jezdni.</summary>
        public float primaryPosX;               
        /// <summary>Zmienna stanu efektu skalowania elementu.</summary>
        public bool effect;                     
        /// <summary>Zmienna stanu informująca o końcu efektu skalowania.</summary>
        public bool toDelete;

        //------------------------------------------------------------------------------------
        //                          Constructors
        //------------------------------------------------------------------------------------
        /// <summary>
        /// Konstruktor - inicjalizacja podstawowych parametrów (serca, monety i kapsle).
        /// </summary>
        /// <param name="texture">Tekstura elementu gry.</param>
        /// <param name="textureRect">Aktualna ramka animacji.</param>
        /// <param name="damageRect">Model kolizji elementu.</param>
        /// <param name="type">Typ elementu.</param>
        /// <param name="dir">Kierunek poruszania się elementu.</param>
        public Entity(Texture texture, IntRect textureRect, IntRect damageRect, TYPE type, DIR dir = DIR.UP)
        {
            // przypisanie typu i kierunku poruszania się elementu
            this.type = type;
            this.dir = dir;
            // utworzenie sprita jako elementu z teksturą samochodu
            sprite = new Sprite()
            {
                Texture = texture,
                TextureRect = textureRect,
                Origin = new Vector2f((float)(textureRect.Width) / 2f, 0f)
            };
            // utworzenie hitboxa elementu
            damageBox = new RectangleShape()
            {
                Size = new Vector2f(
                    (float)damageRect.Width,
                    (float)damageRect.Height
                ),
                Origin = new Vector2f(
                    (float)damageRect.Width / 2f, 0f
                ),
                OutlineThickness = 1f,
                OutlineColor = Color.Black
            };
            // utworzenie koloru, rotacji i wartości początkowych efektu
            SetColor();
            SetDirection();
            effect = false;
            toDelete = false;
        }

        /// <summary>
        /// Konstruktor - inicjalizacja podstawowych parametrów (pojazdów).
        /// </summary>
        /// <param name="carNr">Identyfikator tworzonego pojazdu.</param>
        /// <param name="carCollection">Kolekcja wszystkich pojazdów.</param>
        /// <param name="createMovementComponent">Zezwolenie na tworzenie zaawansowanego modelu ruchu.</param>
        /// <param name="dir">Kierunek poruszania się elementu.</param>
        public Entity(int carNr, List<Entity> carCollection, bool createMovementComponent = true, DIR dir = DIR.UP)
        {
            try
            {
                // próba utworzenia elementu z istniejącej kolekcji
                Entity car = carCollection[carNr];
                sprite = new Sprite(car.sprite);
                damageBox = new RectangleShape(car.damageBox);
                type = TYPE.CAR;
                effect = false;
                toDelete = false;
                this.dir = dir;
                SetRotation((dir == DIR.DOWN) ? 9000f : 0f);
                // utworzenie komponentu zaawansowanego ruchu
                if (car.movement != null && createMovementComponent)
                    movement = new MovementComponent(car.movement);
            }
            catch(Exception exception)
            {
                // ewentualna komunikacja błędu
                Program.ShowError(exception);
            }
        }

        //------------------------------------------------------------------------------------
        //                          Supporting methods
        //------------------------------------------------------------------------------------
        /// <summary>
        /// Metoda tworząca komponent zaawansowanego ruchu pojazdu.
        /// </summary>
        /// <param name="acceleration">Parametr przyśpieszania.</param>
        /// <param name="deceleration">Parametr hamowania.</param>
        /// <param name="maxVelocity">Maksymalna prędkość pojazdu.</param>
        public void CreateMovementCompontent(Vector2f acceleration, Vector2f deceleration, Vector2f maxVelocity)
        {
            // utworzenie modelu zaawansowanego ruchu
            movement = new MovementComponent(
                acceleration,
                deceleration,
                maxVelocity
            );
        }

        /// <summary>
        /// Metoda aktualizująca ruch elementu.
        /// </summary>
        /// <param name="dt">Czas od poprzedniego wywołania.</param>
        /// <param name="speed">Szybkość poruszania się jezdni.</param>
        /// <param name="offset">Przesunięcie związane z poziomem alkoholu.</param>
        public void UpdateMove(float dt, float speed, float offset)
        {
            if (movement != null)
            {
                // opis modelu zaawansowanego ruchu
                dt /= 1000f;
                // pobranie aktualnej prędkości
                var velocity = movement.Update(dt);
                // ustawienie rotacji i przesunięcia
                Move(velocity.X * dt, velocity.Y * dt);
                SetRotation(velocity.X);
            }
            else
            {
                // brak obsługi zaawansowanego ruchu
                // sprawdzenie czy jest używany efekt
                // efekt skalowania nie występuje dla samochodów 
                if (effect && type != TYPE.CAR)
                {
                    // gdy efekt występuje - zmiana skali aż do całkowitego zaniku elementu
                    Vector2f scale = sprite.Scale;
                    scale.X -= 0.08f;
                    scale.Y -= 0.08f;
                    // po całkowitym zaniku usuwamy element
                    if (scale.X < 0f || scale.Y < 0f)
                    {
                        toDelete = true;
                    }
                    else
                    {
                        sprite.Scale = scale;
                        damageBox.Scale = scale;
                    }
                }
                else if (effect && type == TYPE.CAR)
                    return;
                // przesunięcie elementu zgodnie z osią X (offset) i Y (speed)
                Vector2f position = damageBox.Position;
                speed = ((dir == DIR.UP) ? 0.6f : 1.5f) * Math.Abs(speed) * dt;
                position.X = primaryPosX + offset;
                position.Y += speed;
                SetPosition(position.X, position.Y);
            }
        }

        /// <summary>
        /// Metoda poruszająca elementem gry o zadaną wielkość.
        /// </summary>
        /// <param name="dx">Przesunięcie po osi X.</param>
        /// <param name="dy">Przesunięcie po osi Y.</param>
        public void Move(float dx, float dy)
        {
            // pobranie pozycji
            Vector2f position = damageBox.Position;
            // przesunięcie o zadaną wielkość
            position.X += dx;
            position.Y += dy;
            // ustawienie pozycji
            SetPosition(position.X, position.Y);
        }

        /// <summary>
        /// Metoda ustawiająca pozycję elementu.
        /// </summary>
        /// <param name="X">Pozycja w osi X.</param>
        /// <param name="Y">Pozycja w osi Y.</param>
        public void SetPosition(float X, float Y)
        {
            // ustawienie pozycji zarówno dla sprita jak i hitboxa
            sprite.Position = new Vector2f(X, Y);
            damageBox.Position = new Vector2f(X, Y);
        }

        /// <summary>
        /// Metoda zwracająca aktualną pozycję elementu.
        /// </summary>
        /// <returns>Aktualna pozycja elementu.</returns>
        public Vector2f GetPosition()
        {
            return damageBox.Position;
        }

        /// <summary>
        /// Metoda zwracająca wielkość modelu kolizji elementu.
        /// </summary>
        /// <returns>Rozmiar modelu kolizji.</returns>
        public Vector2f GetSize()
        {
            return damageBox.Size;
        }

        /// <summary>
        /// Metoda ustawiająca rotacje elementu o zadany kąt.
        /// </summary>
        /// <param name="angle">Kąt obrotu elementu.</param>
        public void SetRotation(float angle)
        {
            // ustawienie rotacji zarówno dla hitboxa i sprita
            sprite.Rotation = angle / 50f;
            damageBox.Rotation = angle / 50f;
        }

        /// <summary>
        /// Metoda ustawiająca kierunek zaawansowanego ruchu pojazdu.
        /// </summary>
        /// <param name="X">Kierunek w osi X.</param>
        /// <param name="Y">Kierunek w osi Y.</param>
        public void DirectionMove(float X, float Y)
        {
            // ustalenie kierunku ruchu jedynie dla zaawansowanego ruchu
            if (movement != null)
            {
                // ustalenie kierunku w osi X i Y
                movement.move.X += X;
                movement.move.Y += Y;
                // ograniczenie wartości do -1, 0 lub 1 każdej osi
                if (movement.move.X > 1f)  movement.move.X =  1f;
                if (movement.move.X < -1f) movement.move.X = -1f;
                if (movement.move.Y > 1f)  movement.move.Y =  1f;
                if (movement.move.Y < -1f) movement.move.Y = -1f;
            }
        }

        /// <summary>Metoda ustawiająca kolor modelu kolizji.</summary>
        private void SetColor()
        {
            // ustalenie koloru elementu w zależności od typu
            if (type == TYPE.HEART)
                damageBox.FillColor = C.ENTITY_HEART_HITBOX_COLOR;
            else if (type == TYPE.COIN)
                damageBox.FillColor = C.ENTITY_COIN_HITBOX_COLOR;
            else if (type == TYPE.BEER)
                damageBox.FillColor = C.ENTITY_CAP_HITBOX_COLOR;
            else if (type == TYPE.CAR)
                damageBox.FillColor = C.ENTITY_CAR_HITBOX_COLOR;
        }

        /// <summary>Metoda ustawiająca parametry ze względu na kierunek poruszania sie elementu.</summary>
        private void SetDirection()
        {
            if (type == TYPE.CAR)
            {
                // rotacja następuje dla pojazdów
                SetRotation((dir == DIR.DOWN) ? 9000f : 0f);
            }
            else if (dir == DIR.DOWN)
            {
                // dla pozostałych elementów występuje jedynie zmiana punktu odniesienia pozycji
                Vector2f origin = sprite.Origin;
                origin.Y += damageBox.Size.Y;
                sprite.Origin = new Vector2f(origin.X, origin.Y);
                damageBox.Origin = new Vector2f(origin.X, origin.Y);
            }
        }

        /// <summary>Metoda ustawiająca zezwolenie na użycie efektu skalowania.</summary>
        public void SetEffect()
        {
            effect = true;
        }
    }
}
