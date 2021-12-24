using SFML.Graphics;
using SFML.System;

namespace Game
{
    /// <summary>
    /// Klasa stanu animacji.
    /// </summary>
    public class AnimationState
    {
        /// <summary>
        /// Identyfikator aktualnie wyświetlanej ramki.
        /// </summary>
        public int currentFrameId;
        /// <summary>
        /// Czas wyświetlania aktualnej ramki.
        /// </summary>
        public float currentFrameTime;
        /// <summary>
        /// Ilość animowanych ramek.
        /// </summary>
        public int framesNr;
        /// <summary>
        /// maksymalny czas wyświetlania jednej ramki.
        /// </summary>
        public float maxFrameTime;
        /// <summary>
        /// Obiekt wskazujący aktualną ramkę w teksturze.
        /// </summary>
        public IntRect currentFrame;
        /// <summary>
        /// Pozycja wyświetlanej animacji.
        /// </summary>
        public Vector2f position;

        /// <summary>
        /// Konstruktor podstawowy - inicjalizacja podstawowych parametrów.
        /// </summary>
        public AnimationState() 
        {
            currentFrameId = 0;
            currentFrameTime = 0f;
        }

        /// <summary>
        /// Konstruktor animacji ze wskazaniem parametrów.
        /// </summary>
        /// <param name="framesNr">Ilość animowanych ramek.</param>
        /// <param name="maxFrameTime">Maksymalny czas wyświetlania jednej ramki.</param>
        /// <param name="position">Pozycja wyświetlania animacji.</param>
        public AnimationState(int framesNr, float maxFrameTime, Vector2f position)
        {
            this.framesNr = framesNr;
            this.maxFrameTime = maxFrameTime;
            this.position = position;

            currentFrameId = 0;
            currentFrameTime = 0f;
            currentFrame = new IntRect(0, 0, Animation.tSize, Animation.tSize);
        }
    }
}
