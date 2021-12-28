using SFML.Graphics;

namespace Game
{
    /// <summary>Statyczna klasa przechowująca stałe parametry gry tj. ścieżki do tekstur, fontów itp.</summary>
    public static class C
    {
        /// <summary>Ścieżka do dokumentu XML.</summary>
        public static string XML_DOCUMENT_PATH = "..\\..\\Config.xml";
        /// <summary>Ścieżka do pliku z fontami.</summary>
        public static string FONT_PATH = "..\\..\\resource\\fonts\\MPLUSCodeLatin-Bold.ttf";
        /// <summary>Ścieżka do katalogu z dźwiękami.</summary>
        public static string SOUNDS_DIRECTORY_PATH = "..\\..\\resource\\sounds";
        /// <summary>Ścieżka do tekstury tła.</summary>
        public static string BACKGROUND_TEXTURE_PATH = "..\\..\\resource\\images\\background.png";
        /// <summary>Ścieżka do tekstury monet.</summary>
        public static string COINS_TEXTURE_PATH = "..\\..\\resource\\images\\coins.png";
        /// <summary>Ścieżka do tekstury cyfr.</summary>
        public static string NUMBERS_TEXTURE_PATH = "..\\..\\resource\\images\\numbers.png";
        /// <summary>Ścieżka do tekstury samochodów.</summary>
        public static string CARS_TEXTURE_PATH = "..\\..\\resource\\images\\cars.png";
        /// <summary>Ścieżka do tekstury filtru.</summary>
        public static string FILTER_TEXTURE_PATH = "..\\..\\resource\\images\\vignette_filter.png";
        /// <summary>Ścieżka do tekstury animacji serca.</summary>
        public static string HEART_ANIMATION_PATH = "..\\..\\resource\\images\\heart_animated.png";
        /// <summary>Ścieżka do tekstury animacji monety.</summary>
        public static string COIN_ANIMATION_PATH = "..\\..\\resource\\images\\coin_animated.png";
        /// <summary>Ścieżka do tekstury animacji kapsla.</summary>
        public static string BOTTLE_CAP_ANIMATION_PATH = "..\\..\\resource\\images\\bottle_cap_animated.png";
        /// <summary>Ścieżka do tekstury animacji eksplozji.</summary>
        public static string EXPLOSION_ANIMATION_PATH = "..\\..\\resource\\images\\explosion_animated.png";
        /// <summary>Procentowe szanse na utworzenie kapsla.</summary>
        public static double BOTTLE_CAP_CHANCE_PERCENT = .1d;
        /// <summary>Procentowe szanse na utworzenie serca.</summary>
        public static double HEART_CHANCE_PERCENT = .05d;
        /// <summary>Procentowe szanse na utworzenie monety.</summary>
        public static double COIN_CHANCE_PERCENT = .9d;
        /// <summary>Procentowe szanse na utworzenie samochodu.</summary>
        public static double CAR_CHANCE_PERCENT = .1d;
        /// <summary>Kolor hitboxa elementu serca.</summary>
        public static Color ENTITY_HEART_HITBOX_COLOR = new Color(0, 255, 0, 128);
        /// <summary>Kolor hitboxa elementu monety.</summary>
        public static Color ENTITY_COIN_HITBOX_COLOR = new Color(128, 255, 0, 128);
        /// <summary>Kolor hitboxa elementu kapsla.</summary>
        public static Color ENTITY_CAP_HITBOX_COLOR = new Color(255, 128, 255, 128);
        /// <summary>Kolor hitboxa elementu samochodu na drodze.</summary>
        public static Color ENTITY_CAR_HITBOX_COLOR = new Color(255, 128, 64, 128);
        /// <summary>Kolor hitboxa elementu pojazdu gracza.</summary>
        public static Color ENTITY_MAIN_CAR_HITBOX_COLOR = new Color(0, 255, 255, 128);
        /// <summary>Kolor hitboxa pobocza jezdni.</summary>
        public static Color ENTITY_LR_ROAD_HITBOX_COLOR = new Color(255, 0, 0, 128);
    }
}
