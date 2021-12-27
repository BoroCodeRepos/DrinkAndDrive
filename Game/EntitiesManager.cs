using SFML.Graphics;
using SFML.System;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Game
{
    using Entities = List<Entity>;
    using EntitiesAnimation = Dictionary<TYPE, AnimationState>;
    using Timers = Dictionary<TYPE, TimeCounter>;

    /// <summary>
    /// Klasa zarządzająca elementami gry (pojazdy, serca, monety i kapsle).
    /// </summary>
    class EntitiesManager
    {
        /// <summary>Referencja do obiektu zasobów gry.</summary>
        public Resources resources;
        /// <summary>Lista wszystkich elementów gry.</summary>
        public Entities entities;
        /// <summary>Obiekt głównego pojazdu gracza.</summary>
        public Entity mainCar;
        /// <summary>Słownik animacji obiektów gry.</summary>
        public EntitiesAnimation animation;
        /// <summary>Czasy blokad tworzenia elementów gry.</summary>
        public Timers timers;                        

        /// <summary>Delegata do funkcji wywołujących kolizje.</summary>
        /// <param name="e">Wskazanie elementu, z którym zaszła kolizja</param>
        public delegate void OnCollision(Entity e);

        /// <summary>Delegata do kolizji z elementem serca.</summary>
        public OnCollision onHeartCollision;
        /// <summary>Delegata do kolizji z monetą.</summary>
        public OnCollision onCoinCollision;
        /// <summary>Delegata do kolizji z kapslem.</summary>
        public OnCollision onBeerCollision;
        /// <summary>Delegata do kolizji z pojazdem.</summary>
        public OnCollision onCarCollision;
        /// <summary>Delegata do kolizji z niedozwolonym obszarem mapy.</summary>
        public OnCollision onMapCollision;

        /// <summary>Zmienna procentowej szansy na utworzenie elementu serca.</summary>
        public double heartPercent = C.HEART_CHANCE_PERCENT;
        /// <summary>Zmienna procentowej szansy na utworzenie elementu monety.</summary>
        public double coinPercent = C.COIN_CHANCE_PERCENT;
        /// <summary>Zmienna procentowej szansy na utworzenie elementu kapsla.</summary>
        public double beerPercent = C.BOTTLE_CAP_CHANCE_PERCENT;
        /// <summary>Zmienna procentowej szansy na utworzenie elementu pojazdu.</summary>
        public double carPercent = C.CAR_CHANCE_PERCENT;

        //------------------------------------------------------------------------------------
        //                          Constructor
        //------------------------------------------------------------------------------------
        /// <summary>
        /// Konstruktor managera elementów - inicjalizacja obiektów.
        /// </summary>
        /// <param name="resources">Referencja do obiektu zasobów gry.</param>
        public EntitiesManager(Resources resources)
        {
            // wskaźnik do zasobów
            this.resources = resources;
            heartPercent = 0d;
            // utworzenie elementów animacji monet, kapsli i serc
            entities = new Entities();
            animation = new EntitiesAnimation
            {
                [TYPE.COIN]  = new AnimationState(),
                [TYPE.BEER]  = new AnimationState(),
                [TYPE.HEART] = new AnimationState()
            };
            // inicjalizacja animacji z dokumentu XML
            InitAnimation("coins",  animation, TYPE.COIN);
            InitAnimation("beers",  animation, TYPE.BEER);
            InitAnimation("hearts", animation, TYPE.HEART);
            // utworzenie timerów do opóźnienia losowanie elementów gry
            timers = new Timers
            {
                [TYPE.COIN] = new TimeCounter(.2d),
                [TYPE.BEER] = new TimeCounter(.2d),
                [TYPE.CAR] = new TimeCounter(.2d)
            };
        }

        //------------------------------------------------------------------------------------
        //                          Initialization methods
        //------------------------------------------------------------------------------------
        /// <summary>
        /// Metoda inicjalizująca animacje.
        /// </summary>
        /// <param name="attrName">Nazwa atrybutu animacji w dok. XML.</param>
        /// <param name="Tanimation">Lista animowanych elementów.</param>
        /// <param name="type">Typ inicjalizowanej animacji.</param>
        private void InitAnimation(string attrName, EntitiesAnimation Tanimation, TYPE type)
        {
            // inicjalizacja wartości animation state pobrana z dokumentu XML
            AnimationState animation = Tanimation[type];
            XmlElement root = resources.document.DocumentElement;
            XmlElement element = root["game"]["animation"][attrName];
            animation.framesNr = Convert.ToInt16(element.Attributes["frame_nr"].Value);
            animation.maxFrameTime = float.Parse(element.Attributes["max_frame_time"].Value);
            int width = Convert.ToInt16(element.Attributes["width"].Value);
            int height = Convert.ToInt16(element.Attributes["height"].Value);
            animation.currentFrame = new IntRect(0, 0, width, height);
            Tanimation[type] = animation;
        }

        //------------------------------------------------------------------------------------
        //                          Update game methods
        //------------------------------------------------------------------------------------
        /// <summary>
        /// Główna metoda aktualizująca elementy gry.
        /// </summary>
        /// <param name="dt">Czas od poprzedniego wywołania.</param>
        /// <param name="speed">Szybkość poruszania się jezdni.</param>
        /// <param name="offset">Przesunięcie ze względu na poziom alkoholu.</param>
        public void Update(float dt, float speed, float offset)
        {
            // aktualizacja timerów
            UpdateTimers(dt);
            // próba utworzenia nowych elementów gry
            TryCreateEntities();
            // usunięcie elementów poza mapą
            DeleteEntities();
            // aktualizacja ruchu głównego pojazdu
            mainCar.UpdateMove(dt, speed, 0f);
            // aktualizacja ruchu elementów gry
            UpdateEntitiesMove(dt, speed, offset);
            // aktualizacja położenia na mapie głownego samochodu
            UpdateMapBounds();
            // sprawdzenie wyjazdu gracza poza jezdnię
            UpdateMapCollision();
            // aktualizacja kolizji obiektów
            UpdateCollisions();                                 
        }

        /// <summary>
        /// Metoda aktualizująca animacje elementów gry.
        /// </summary>
        /// <param name="dt">Czas od poprzedniego wywołania.</param>
        public void UpdateAnimation(float dt)
        {
            List<TYPE> types = new List<TYPE>
            {
                TYPE.HEART,
                TYPE.COIN,
                TYPE.BEER
            };

            foreach (var type in types)
            {
                // przejscie przez wszystkie animowane elementy (taka sama procedura)
                var animation = this.animation[type];
                // zwiększenie czasu trawnia ramki
                animation.currentFrameTime += dt;
                // sprawdzenie maksymalnego czasu trwania ramki
                if (animation.currentFrameTime >= animation.maxFrameTime)
                {
                    // czas dobiegł końca - kolejna ramka
                    animation.currentFrameTime = 0f;
                    animation.currentFrameId += 1;
                    // sprawdzenie warunku końca ramek
                    if (animation.currentFrameId >= animation.framesNr)
                        animation.currentFrameId = 0;
                }
                // ustalenie parametrów pod kątem nowo obliczonych wartości animacji
                animation.currentFrame.Left = animation.currentFrameId * animation.currentFrame.Width;
                this.animation[type] = animation;
                // pobranie z listy wszystkich elementów tylko te o danym typie
                var animatedEntities = entities.Where(entity => entity.type == type);
                // i zamiana w tych elementach tekstury
                foreach (var item in animatedEntities)
                    item.sprite.TextureRect = animation.currentFrame;
            }
        }

        /// <summary>
        /// Metoda aktualizująca ruch elementów gry.
        /// </summary>
        /// <param name="dt">Czas od poprzedniego wywołania.</param>
        /// <param name="speed">Szybkość poruszania się jezdni.</param>
        /// <param name="offset">Przesunięcie ze względu na poziom alkoholu.</param>
        private void UpdateEntitiesMove(float dt, float speed, float offset)
        {
            Entities entitiesToDelete = new Entities();
            foreach (var item in entities)
            {
                // aktualizacja ruchu każdego elementu gry
                item.UpdateMove(dt, speed, offset);
                // sprawdzenie czy element zgłosił możliwość usunięcia 
                if (item.toDelete)
                    entitiesToDelete.Add(item);
            }
            // usunięcie elementów z listy
            if (entitiesToDelete.Count > 0)
                foreach (var item in entitiesToDelete)
                    entities.Remove(item);
        }

        /// <summary>
        /// Metoda aktualizująca położenie głównego pojazdu w obszarze wyświetlanej mapy.
        /// </summary>
        private void UpdateMapBounds()
        {
            // sprawdzenie wyjechania poza mapę w osi Y
            Vector2f pos = mainCar.damageBox.Position;
            // warunek na górną krawędź
            if (pos.Y < 0f)
            {
                // ustalamy że nie może wyjechać poza górną krawędź
                mainCar.SetPosition(pos.X, 0f);
                mainCar.movement.velocity.Y = 0f;
            }
            pos.Y += mainCar.damageBox.Size.Y;
            if (pos.Y > resources.options.winHeight)
            {
                // ustalamy że nie może wyjechać poza dolną krawędź
                pos.Y = resources.options.winHeight - mainCar.damageBox.Size.Y;
                mainCar.SetPosition(pos.X, pos.Y);
                mainCar.movement.velocity.Y = 0f;
            }
        }

        /// <summary>
        /// Metoda aktualizująca liczniki czasu blokad tworzenia elementów gry.
        /// </summary>
        /// <param name="dt">Czas od poprzedniego wywołania.</param>
        private void UpdateTimers(float dt)
        {
            timers[TYPE.COIN].Update(dt);
            timers[TYPE.BEER].Update(dt);
            timers[TYPE.CAR].Update(dt);
        }

        /// <summary>
        /// Metoda sprawdzająca kolizje głównego pojazdu z elementami gry.
        /// </summary>
        private void UpdateCollisions()
        {
            // sprawdzenie kolizji wszystkich elementów z głównym pojazdem
            foreach (var entity in entities)
            {
                // warunek kolizji
                if (CollisionsCheck(mainCar, entity) && !entity.effect)
                {
                    // zaszła kolizja - sprawdzenie typu elementu i wywołanie odpowiedniej funkcji
                    if (entity.type == TYPE.HEART)
                        onHeartCollision(entity);

                    if (entity.type == TYPE.COIN)
                        onCoinCollision(entity);

                    if (entity.type == TYPE.BEER)
                        onBeerCollision(entity);

                    if (entity.type == TYPE.CAR)
                        onCarCollision(entity);
                }
            }
        }

        /// <summary>
        /// Metoda sprawdzająca kolizje głównego pojazdu z niedozwolonym obszarem jezdni.
        /// </summary>
        private void UpdateMapCollision()
        {
            // sprawdzenie kolizji z poboczem jezdni
            FloatRect carRect = mainCar.damageBox.GetGlobalBounds();
            FloatRect bgRectL = resources.dmgBoxL.GetGlobalBounds();
            FloatRect bgRectR = resources.dmgBoxR.GetGlobalBounds();
            // warunek zderzenia
            if (carRect.Intersects(bgRectL) || carRect.Intersects(bgRectR))
                // wywołanie funkcji zderzenia
                onMapCollision(null);
        }

        //------------------------------------------------------------------------------------
        //                          Render elements on screen
        //------------------------------------------------------------------------------------
        /// <summary>
        /// Metoda renderująca elementy gry we wskazanym oknie.
        /// </summary>
        /// <param name="window">Referencja do głównego okna gry.</param>
        public void RenderEntities(ref RenderWindow window)
        {
            // wyświetlenie monet, butelek, serc i  samochodów
            foreach (var entity in entities)
                window.Draw(entity.sprite);
            // wyświetlenie samochodu gracza
            window.Draw(mainCar.sprite);
        }

        /// <summary>
        /// Metoda renderująca modele uszkodzeń.
        /// </summary>
        /// <param name="window">Referencja do głównego okna gry.</param>
        /// <param name="showDamageBoxes">Wskaźnik wyświetlenia modelu uszkodzeń.</param>
        public void RenderDamageBoxes(ref RenderWindow window, bool showDamageBoxes)
        {
            if (showDamageBoxes)
            {
                // jeżeli zezwolono na wyświetlenie hitboxów
                // zostają one wyświetlone, dla elementów gry i pobocza jezdni
                // oraz pojazdu gracza
                window.Draw(resources.dmgBoxL);
                window.Draw(resources.dmgBoxR);
                window.Draw(mainCar.damageBox);

                foreach (Entity entity in entities)
                    window.Draw(entity.damageBox);
            }
        }

        //------------------------------------------------------------------------------------
        //                          Supporting methods
        //------------------------------------------------------------------------------------
        /// <summary>
        /// Metoda usuwająca wszystkie elementy gry.
        /// </summary>
        public  void ClearAll()
        {
            entities.Clear();
        }

        /// <summary>
        /// Metoda tworząca odpowiedni pojazd gracza.
        /// </summary>
        /// <param name="id">Identyfikator tworzonego pojazdu.</param>
        public  void SetPlayerCarById(int id)
        {
            // ograniczenie identyfikatora od 0 do 13
            // tyle mamy dostępnych pojazdów w grze
            id %= 14;
            // utworzenie pojazdu gracza
            mainCar = new Entity(id, resources.carCollection);
            // ustalenie koloru hitboxa
            mainCar.damageBox.FillColor = C.ENTITY_MAIN_CAR_HITBOX_COLOR;
            // ustalenie pozycji początkowej gracza
            mainCar.SetPosition(
                resources.background.Texture.Size.X / 2f - resources.background.Origin.X,
                resources.background.Texture.Size.Y / 2f
            );
        }

        /// <summary>
        /// Metoda przeprowadzająca próbę utworzenia nowego elementu gry.
        /// </summary>
        private void TryCreateEntities()
        {
            // określenie offsetu w osi X
            float offset = resources.background.Position.X;
            offset -= resources.background.Origin.X;
            // utworzenie obiektów pasów
            FloatRect[] lanes = new FloatRect[4];
            lanes[0] = new FloatRect(319f + offset, -1f, 138f, 10f);
            lanes[1] = new FloatRect(466f + offset, -1f, 153f, 10f);
            lanes[2] = new FloatRect(627f + offset, -1f, 158f, 10f);
            lanes[3] = new FloatRect(792f + offset, -1f, 144f, 10f);

            Random rnd = new Random();
            // losowanie pasa
            int lane = rnd.Next(4);
            // losowanie szansy na utworzenie elementu
            double percent = rnd.NextDouble();
            // sprawdzenie zajetości pasa
            if (CheckBusyLane(lanes[lane]))
            {
                // pas wolny - losowanie typu nowo tworzonego elementu
                TYPE type = (TYPE)rnd.Next((int)TYPE.COUNT);
                // kierunek elementu zależny od wylosowanego pasa
                DIRECTION dir = (lane < 2) ? DIRECTION.DOWN : DIRECTION.UP;
                // aktualizacja pozycji pasa na domyślny (bez przesuniecia)
                lanes[lane].Left -= (offset + resources.background.Origin.X);
                // określenie pozycji początkowej elementu (bez przesunięcia)
                Vector2f position = new Vector2f(lanes[lane].Left + lanes[lane].Width / 2f, 0f);
                // sprawdzenie procentowych szans na utworzenie elementu i utworzenie go
                CheckPercentageChances(type, dir, percent, position);
            }
        }

        /// <summary>
        /// Metoda sprawdzająca zajętość pasa ruchu na jezdni.
        /// </summary>
        /// <param name="lane">Obiekt pasa ruchu.</param>
        /// <returns>Pas zajęty => false, pas wolny => true.</returns>
        private bool CheckBusyLane(FloatRect lane)
        {
            // sprawdzenie zajętosci pasa ze wszystkimi elementami gry
            foreach (var item in entities)
                if (lane.Intersects(item.damageBox.GetGlobalBounds()))
                    return false;
            // przejście przez wszystkie elementy - brak kolizji
            return true;
        }

        /// <summary>
        /// Metoda sprawdzająca procentowe szanse na utworzenie elementu gry.
        /// </summary>
        /// <param name="type">Typ elementu gry.</param>
        /// <param name="dir">Kierunek jazdy / poruszania się elementu.</param>
        /// <param name="percent">Wyznaczone procentowe szanse na utworzenie.</param>
        /// <param name="position">Pozycja inicjalizacyjna w przypadku utworzenia.</param>
        private void CheckPercentageChances(TYPE type, DIRECTION dir, double percent, Vector2f position)
        {
            // utworzenie obiektu danego typu jeśli zostanie poprawnie określony procentowo
            bool create = false;
            // pobranie szansy procentowej w zależności od typu elementu
            double chances = 0d;
            if (type == TYPE.HEART)
                chances = heartPercent;
            if (type == TYPE.COIN)
                chances = coinPercent;
            if (type == TYPE.BEER)
                chances = beerPercent;
            if (type == TYPE.CAR)
                chances = carPercent;
            // obsługa dla serc nie jest objęta timerem
            if (type != TYPE.HEART)
            {
                // pobranie timera danego typu
                var timer = timers[type];
                // jeżeli timer jest w użytku sprawdzamy czy doszło do przekroczenia czasu zdarzenia
                if (timer.IsRun())
                {
                    // sprawdzenie czy doszło do zdarzenia
                    if (timer.GetEventStatus())
                    {
                        // jeżeli doszło to tworzymy element i zerujemy status zdarzenia.
                        create = true;
                        timer.ClearEventStatus();
                    }
                }
                else
                {
                    // timer nie został jeszcze załączony - pierwsze tworzenie elementu
                    create = true;
                    timer.Start();
                }
            }
            else
            {
                // w przypadku serca bezpośrednio następuje tworzenie elementu
                create = true;
            }
            // jeżeli jest zezwolenie na tworzenie i zgadza się warunek na szanse to tworzymy element
            if (create && chances > percent)
                CreateElement(position, type, dir);
        }

        /// <summary>
        /// Metoda tworząca odpowiedni (wskazany typem) element gry.
        /// </summary>
        /// <param name="position">Pozycja inicjalizacyjna tworzonego elementu.</param>
        /// <param name="type">Typ tworzonego elementu gry.</param>
        /// <param name="dir">Kierunek poruszania się elementu gry.</param>
        private void CreateElement(Vector2f position, TYPE type, DIRECTION dir)
        {
            if (type == TYPE.CAR)
            {
                // tworzenie elementu pojazdu
                Random rnd = new Random();
                // losowanie pojazdu z puli dostepnych i tworzenie encji
                Entity car = new Entity(rnd.Next(14), resources.carCollection, false, dir);
                // korekcja pozycji pod kątem kierunku jazdy
                position.Y -= (dir == DIRECTION.UP) ? car.damageBox.Size.Y : 0f;
                // pozycja inicjalizująca w osi X
                car.primaryPosX = position.X;
                // ustalenie pozycji początkowej
                car.SetPosition(position.X, position.Y);
                // dodatnie do listy elementów
                entities.Add(car);
            }
            else
            {
                // tworzenie pozostałych elementów (monety, serca lub kapsla)
                IntRect damageRect = new IntRect()
                {
                    // wielkości left i top zostaną ustawione podczas aktualizacji
                    Left = 0, Top = 0,
                    // wielkość ramek określona na podstawie obiektu animacji
                    Width = animation[type].currentFrame.Width,
                    Height = animation[type].currentFrame.Height
                };
                // tworzenie elementu
                Entity entity = new Entity(
                    resources.GetTexture(type),
                    animation[type].currentFrame,
                    damageRect,
                    type,
                    dir );
                // aktualizacja pozycji pod kątem kieruku
                position.Y -= (dir == DIRECTION.UP) ? entity.damageBox.Size.Y : 0f;
                // ustalenie pozyci początkowej w osi X
                entity.primaryPosX = position.X;
                // ustalenie pozycji poczatkowej
                entity.SetPosition(position.X, position.Y);
                // dodanie elementu do listy
                entities.Add(entity);
            }
        }

        /// <summary>
        /// Metoda usuwająca elementy gry, które opuściły obszar mapy.
        /// </summary>
        private void DeleteEntities()
        {
            Vector2f position;
            Entities toDelete = new Entities();
            foreach (Entity entity in entities)
            {
                // przegląd przez wszystkie elementy gry 
                // pobranie pozycji 
                position = entity.damageBox.Position;
                switch (entity.dir)
                {
                // obsługa dla kierunku DOWN
                case DIRECTION.DOWN:
                    position.Y -= entity.damageBox.Size.Y;
                    if (position.Y > resources.options.winHeight)
                        toDelete.Add(entity);
                    break;
                // obsługa dla kierunku UP
                case DIRECTION.UP:
                    if (position.Y > resources.options.winHeight)
                        toDelete.Add(entity);
                    break;
                }
            }
            // usunięcie wszystkich elementów
            foreach (Entity entity in toDelete)
                entities.Remove(entity);      
        }

        /// <summary>
        /// Metoda sprawdzająca kolizje pomiędzy dwoma elementami gry.
        /// </summary>
        /// <param name="A">Pierwszy element gry.</param>
        /// <param name="B">Drugi element gry.</param>
        /// <returns>Doszło do kolizji => true, nie doszło do kolizji => false.</returns>
        private bool CollisionsCheck(Entity A, Entity B)
        {
            // sprawdzenie kolizji pomiędzy encjami
            // pobranie FloatRect elementów
            FloatRect rectA = A.damageBox.GetGlobalBounds();
            FloatRect rectB = B.damageBox.GetGlobalBounds();
            // sprawdzenie intersekcji
            if (rectA.Intersects(rectB))
                // doszlo do kolizji
                return true;
            // nie doszło do kolizji
            return false;
        }
    }
}
