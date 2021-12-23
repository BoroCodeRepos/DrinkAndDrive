using SFML.System;

namespace Game
{
    public class MovementComponent
    {
        public Vector2f acceleration;   // przyspieszanie
        public Vector2f deceleration;   // hamowanie
        public Vector2f velocity;       // prędkość
        public Vector2f maxVelocity;    // prędkość maksymalna
        public Vector2f move;           // aktualny współczynnik ruchu pojazdu

        public MovementComponent(Vector2f acceleration, Vector2f deceleration, Vector2f maxVelocity)
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

        public Vector2f Update(float dt)
        {
            velocity.X += acceleration.X * dt * move.X;
            velocity.Y += acceleration.Y * dt * move.Y;

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

            return velocity;
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
    }
}
