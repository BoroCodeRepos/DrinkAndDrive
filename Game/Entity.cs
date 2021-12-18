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
    using DIR = DIRECTION;

    public class Entity
    {
        public TYPE type;                       // typ elementu
        public DIR dir;                         // kierunek ruchu
        public Sprite sprite;                   // element wyświetlany na ekranie
        public RectangleShape damageBox;        // model uszkodzeń
        public MovementComponent movement;      // zapis aktualnych wartości określających ruch
        public float primaryPosX;               // pozycja inicjalizacyjna

        public Entity(Texture texture, IntRect textureRect, IntRect damageRect, TYPE type, DIR dir = DIR.UP)
        {
            this.type = type;
            this.dir = dir;
            sprite = new Sprite()
            {
                Texture = texture,
                TextureRect = textureRect,
                Origin = new Vector2f((float)(textureRect.Width) / 2f, 0f)
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
                OutlineThickness = 1f,
                OutlineColor = Color.Black
            };
            SetColor();
            SetDirection();
        }

        public Entity(int carNr, List<Entity> carCollection, bool createMovementComponent = true, DIR dir = DIR.UP)
        {
            try
            {
                Entity car = carCollection[carNr];
                sprite = new Sprite(car.sprite);
                damageBox = new RectangleShape(car.damageBox);
                type = TYPE.CAR;
                this.dir = dir;
                SetRotation((dir == DIR.DOWN) ? 9000f : 0f);

                if (car.movement != null && createMovementComponent)
                    movement = new MovementComponent(car.movement);
            }
            catch(Exception exception)
            {
                Program.ShowError(exception);
            }
        }

        public void CreateMovementCompontent(Vector2f acceleration, Vector2f deceleration, Vector2f maxVelocity)
        {
            movement = new MovementComponent(
                acceleration,
                deceleration,
                maxVelocity
            );
        }

        public void UpdateMove(float dt, float speed, float offset)
        {
            if (movement != null)
            {
                dt = dt / 1000f;
                Vector2f velocity = movement.Update(dt);
                Move(velocity.X * dt, velocity.Y * dt);
                SetRotation(velocity.X);
            }
            else
            {
                Vector2f position = damageBox.Position;
                speed = ((dir == DIR.UP) ? 0.6f : 1.5f) * Math.Abs(speed) * dt;
                position.X = primaryPosX + offset;
                position.Y += speed;
                SetPosition(position.X, position.Y);
            }
        }

        public void Move(float dx, float dy)
        {
            Vector2f position = damageBox.Position;
            position.X += dx;
            position.Y += dy;
            SetPosition(position.X, position.Y);
        }

        public void SetPosition(float X, float Y)
        {
            sprite.Position = new Vector2f(X, Y);
            damageBox.Position = new Vector2f(X, Y);
        }

        public Vector2f GetPosition()
        {
            return damageBox.Position;
        }

        public Vector2f GetSize()
        {
            return damageBox.Size;
        }

        public void SetRotation(float angle)
        {
            sprite.Rotation = angle / 50f;
            damageBox.Rotation = angle / 50f;
        }

        public void DirectionMove(float X, float Y)
        {
            if (movement != null)
            {
                movement.move.X += X;
                movement.move.Y += Y;

                if (movement.move.X > 1f)  movement.move.X =  1f;
                if (movement.move.X < -1f) movement.move.X = -1f;
                if (movement.move.Y > 1f)  movement.move.Y =  1f;
                if (movement.move.Y < -1f) movement.move.Y = -1f;
            }
        }

        private void SetColor()
        {
            if (type == TYPE.HEART)
                damageBox.FillColor = new Color(0, 255, 0, 128);
            else if (type == TYPE.COIN)
                damageBox.FillColor = new Color(128, 255, 0, 128);
            else if (type == TYPE.BEER)
                damageBox.FillColor = new Color(255, 128, 255, 128);
            else if (type == TYPE.CAR)
                damageBox.FillColor = new Color(255, 128, 64, 128);
        }

        private void SetDirection()
        {
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
    }
}
