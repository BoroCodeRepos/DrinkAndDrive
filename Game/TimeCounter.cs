namespace Game
{
    /// <summary>
    /// Klasa zliczająca czas i informująca o wystąpieniu zdarzenia.
    /// </summary>
    class TimeCounter
    {
        /// <summary>Aktualnie zliczony czas.</summary>
        private double currentTime;
        /// <summary>Czas wystąpienia zdarzenia.</summary>
        private double eventTime;
        /// <summary>Zmienna stanu wystąpienia zdarzenia.</summary>
        private bool isEvent;
        /// <summary>Zmienna stanu pracy licznika.</summary>
        private bool run;

        /// <summary>
        /// Konstruktor - inicjalizacja podstawowych parametrów licznika.
        /// </summary>
        public TimeCounter()
        {
            currentTime = 0d;
            eventTime = 0d;
            run = false;
            isEvent = false;
        }

        /// <summary>
        /// Konstruktor - inicjalizacja parametrów wraz z czasem zdarzenia.
        /// </summary>
        /// <param name="evTime">Czas do zgłoszenia zdarzenia.</param>
        public TimeCounter(double evTime)
        {
            currentTime = 0d;
            eventTime = evTime;
            run = false;
            isEvent = false;
        }

        /// <summary>
        /// Metoda ustawiająca czas zdarzenia.
        /// </summary>
        /// <param name="eventTime">Czas do zgłoszenia zdarzenia.</param>
        public void SetEventTime(double eventTime)
        {
            this.eventTime = eventTime;
        }

        /// <summary>
        /// Start licznika.
        /// </summary>
        public void Start()
        {
            run = true;
        }

        /// <summary>
        /// Stop licznika.
        /// </summary>
        public void Stop()
        {
            run = false;
        }

        /// <summary>
        /// Aktualizacja pracy licznika.
        /// </summary>
        /// <param name="dt">Czas od poprzedniego wywołania.</param>
        public void Update(float dt)
        {
            if (run)
                currentTime += (double)(dt / 1000f);

            if (eventTime > 0d && currentTime >= eventTime)
            {
                currentTime -= eventTime;
                isEvent = true;
            }
        }

        /// <summary>
        /// Metoda zwracająca wystąpienie zdarzenia.
        /// </summary>
        /// <returns>Wystapienie zdarzenia.</returns>
        public bool GetEventStatus()
        {
            return isEvent;
        }

        /// <summary>
        /// Metoda zerująca status zdarzenia.
        /// </summary>
        public void ClearEventStatus()
        {
            isEvent = false;
        }

        /// <summary>
        /// Metoda zerująca zliczony czas.
        /// </summary>
        public void ClearTime()
        {
            currentTime = 0d;
        }

        /// <summary>
        /// Metoda zwracająca aktualnie zliczony czas.
        /// </summary>
        /// <returns>Aktualny czas.</returns>
        public double GetCurrentTime()
        {
            return currentTime;
        }

        /// <summary>
        /// Metoda zwracająca stan pracy licznika.
        /// </summary>
        /// <returns>Stan pracy licznika.</returns>
        public bool IsRun()
        {
            return run;
        }
    }
}
