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
        public uint id;                         // ID
        public string name;                     // nazwa 
        public TYPE type;                       // typ elementu
        public DIRECTION dir;                   // kierunek ruchu
        public Sprite sprite;                   // element wyświetlany na ekranie
        public RectangleShape damageBox;        // model uszkodzeń
        
        public MovementComponent movement;      // zapis aktualnych wartości określających ruch

        public Entity(
            Texture texture,
            IntRect textureRect,
            IntRect damageRect,
            Color damageRectColor,
            TYPE type,
            DIRECTION dir = DIRECTION.UP
        )
        {
            this.type = type;
            this.dir = dir;
            sprite = new Sprite()
            {
                Texture = texture,
                TextureRect = textureRect,
                Origin = new Vector2f((float)(textureRect.Width) / 2f, 0f)
            };

            IntRect offset = new IntRect
            {
                Left = textureRect.Left - damageRect.Left,
                Top = textureRect.Top - damageRect.Top
            };

            damageBox = new RectangleShape()
            {
                Size = new Vector2f(
                    (float)damageRect.Width,
                    (float)damageRect.Height
                ),
                Origin = new Vector2f(
                    (float)damageRect.Width / 2f, 0f
                ),
                FillColor = damageRectColor,
                OutlineThickness = 1f,
                OutlineColor = Color.Black
            };

            if (type == TYPE.CAR)
            {
                SetRotation((dir == DIRECTION.DOWN) ? 9000f : 0f);
            }
            else if (dir == DIRECTION.DOWN)
            {
                Vector2f origin = sprite.Origin;
                origin.Y += damageBox.Size.Y;
                sprite.Origin = new Vector2f(origin.X, origin.Y);
                damageBox.Origin = new Vector2f(origin.X, origin.Y);
            }
           
        }

        public Entity(int carNr, List<Entity> carCollection, DIRECTION dir = DIRECTION.UP)
        {
            try
            {
                Entity car = carCollection[carNr];
                sprite = new Sprite(car.sprite);
                damageBox = new RectangleShape(car.damageBox);
                type = TYPE.CAR;
                this.dir = dir;
                SetRotation((dir == DIRECTION.DOWN) ? 9000f : 0f);

                if (car.movement != null)
                    movement = new MovementComponent(car.movement);
            }
            catch(Exception exception)
            {
                Program.ShowError(exception);
            }
        }

        public void UpdateMovementComponent(float dt)
        {
            if (movement != null)
            {
                dt = dt / 1000f;
                movement.velocity.X += movement.acceleration.X * dt * movement.move.X;                
                movement.velocity.Y += movement.acceleration.Y * dt * movement.move.Y;

                UpdateVelocity(
                    dt, 
                    ref movement.velocity.X, 
                    ref movement.maxVelocity.X, 
                    ref movement.deceleration.X, 
                    movement.move.X );
                UpdateVelocity(
                    dt,
                    ref movement.velocity.Y,
                    ref movement.maxVelocity.Y,
                    ref movement.deceleration.Y,
                    1f );
                    //movement.move.Y );

                Move(movement.velocity.X * dt, movement.velocity.Y * dt);
                SetRotation(movement.velocity.X);
            }
        }

        public void UpdateMove(float dt, float speed)
        {
            speed = ((dir == DIRECTION.UP) ? 1f : 2f) * Math.Abs(speed) * dt;
            Move(0f, speed);
        }

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
                // max velocity check
                if (velocity > maxVelocity)
                    velocity = maxVelocity;

                // deceleration
                velocity -= deceleration * dt * ((dir == 0f) ? 8f : 1f);
                if (velocity < 0f)
                    velocity = 0f;
            }
            else if (velocity < 0f)
            {
                // max velocity check
                if (velocity < -maxVelocity)
                    velocity = -maxVelocity;

                // deceleration
                velocity += deceleration * dt * ((dir == 0f) ? 8f : 1f);
                if (velocity > 0f)
                    velocity = 0f;
            }
        }

        public void CreateMovementCompontent(
            Vector2f acceleration,
            Vector2f deceleration,
            Vector2f maxVelocity
        )
        {
            movement = new MovementComponent(
                acceleration,
                deceleration,
                maxVelocity
            );
        }

        public void SetPosition(float X, float Y)
        {
            sprite.Position = new Vector2f(X, Y);
            damageBox.Position = new Vector2f(X, Y);
        }

        public void SetRotation(float angle)
        {
            sprite.Rotation = angle / 50f;
            damageBox.Rotation = angle / 50f;
        }

        public void Move(float dx, float dy)
        {
            Vector2f position = damageBox.Position;
            position.X += dx;
            position.Y += dy;
            SetPosition(position.X, position.Y);
        }

        public void MoveDir(int X, int Y)
        {
            if (movement != null)
            {
                movement.move.X += X;
                movement.move.Y += Y;

                if (movement.move.X >  1f) movement.move.X =  1f;
                if (movement.move.X < -1f) movement.move.X = -1f;
                if (movement.move.Y >  1f) movement.move.Y =  1f;
                if (movement.move.Y < -1f) movement.move.Y = -1f;
            }
        }
    }

    public class MovementComponent
    {
        public Vector2f acceleration;   // przyspieszanie
        public Vector2f deceleration;   // hamowanie
        public Vector2f velocity;       // prędkość
        public Vector2f maxVelocity;    // prędkość maksymalna
        public Vector2f move;           // aktualny współczynnik ruchu pojazdu

        public MovementComponent(
            Vector2f acceleration,
            Vector2f deceleration,
            Vector2f maxVelocity
        )
        {
            this.acceleration = new Vector2f(acceleration.X, acceleration.Y);
            this.deceleration = new Vector2f(deceleration.X, deceleration.Y);
            this.maxVelocity = new Vector2f(maxVelocity.X, maxVelocity.Y);
            this.velocity = new Vector2f(0f, 0f);
            this.move = new Vector2f(0f, 0f);
        }

        public MovementComponent(MovementComponent component)
        {
            acceleration = new Vector2f(component.acceleration.X, component.acceleration.Y);
            deceleration = new Vector2f(component.deceleration.X, component.deceleration.Y);
            maxVelocity = new Vector2f(component.maxVelocity.X, component.maxVelocity.Y);
            velocity = new Vector2f(0f, 0f);
            move = new Vector2f(0f, 0f);
        }
    }
}
