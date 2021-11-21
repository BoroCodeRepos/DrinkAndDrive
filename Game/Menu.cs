using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Game
{
    class Menu
    {
        private struct Player
        {
            public string name;
            public int score;
            public float time;
        }

        Resources resources;
        Engine engine;

        Color idle = new Color(0xE0, 0xFF, 0xFF);
        Color hover = new Color(0xF5FFFA);
        Color active = new Color(0xFF, 0xFF, 0xFF);

        List<GUI.Component> components;

        public Menu(Engine engine, Resources resources)
        {
            this.engine = engine;
            this.resources = resources;
            components = new List<GUI.Component>();
            InitMainMenu();
        }

        private void InitMainMenu()
        {
            components.Clear();
            Vector2f size = new Vector2f(1000f, 100f);
            float posX = resources.options.winWidth / 2f;

            GUI.Button ContinueBtn = new GUI.Button(size, new Vector2f(posX, 150f), 78, "continue", resources.font, idle, hover, active);
            GUI.Button StartAgainBtn = new GUI.Button(size, new Vector2f(posX, 270f), 78, "start again", resources.font, idle, hover, active);
            GUI.Button ResultsBtn = new GUI.Button(size, new Vector2f(posX, 390f), 78, "other players results", resources.font, idle, hover, active);
            GUI.Button ControlBnt = new GUI.Button(size, new Vector2f(posX, 510f), 78, "show control", resources.font, idle, hover, active);
            GUI.Button ExitBtn = new GUI.Button(size, new Vector2f(posX, 630f), 78, "exit game", resources.font, idle, hover, active);

            ContinueBtn.onClick = new GUI.Component.Function(engine.OnContinue);
            StartAgainBtn.onClick = new GUI.Component.Function(engine.OnStartAgain);
            ResultsBtn.onClick = new GUI.Component.Function(InitResults);
            ControlBnt.onClick = new GUI.Component.Function(InitControl);
            ExitBtn.onClick = new GUI.Component.Function(engine.OnExit);

            components.Add(ContinueBtn);
            components.Add(StartAgainBtn);
            components.Add(ResultsBtn);
            components.Add(ControlBnt);
            components.Add(ExitBtn);
        }

        private void InitControl()
        {
            components.Clear();
            string control = "Control: \n" +
                " ^ / w - move top \n" +
                " < / a - move left \n" +
                " v / s - move bottom \n" +
                " > / d - move right \n" +
                " P / Esc - open / close menu \n" +
                " B - show / hide hitboxes";
            GUI.Button ctrl = new GUI.Button(new Vector2f(1000f, 550f), new Vector2f(512f, 384f), 60, control, resources.font, hover, hover, hover);
            GUI.Button back = new GUI.Button(new Vector2f(200f, 80f), new Vector2f(900f, 720f), 70, "back", resources.font, idle, hover, active)
            {
                onClick = new GUI.Component.Function(InitMainMenu)
            };

            components.Add(ctrl);
            components.Add(back);
        }

        private void InitResults()
        {
            ShowResults(1);
        }

        private void ShowResults(int page)
        {
            components.Clear();
            int itemsPerPage = 6;

            List<Player> playerList = new List<Player>();
            XmlNodeList players = resources.document.GetElementsByTagName("player");
            // utworzenie wewnętrznej listy z graczami
            foreach (XmlNode player in players)
            {
                string name = player.Attributes["name"].Value;
                int score = Convert.ToInt32(player.Attributes["score"].Value);
                float time = float.Parse(player.Attributes["time"].Value);
                playerList.Add(new Player() { name = name, score = score, time = time });
            }
            // sortowanie listy
            playerList.Sort(
                delegate (Player A, Player B) 
                {
                    if (A.score > B.score) return -1;
                    else if (A.score < B.score) return 1;
                    return 0;
                }
            );

            // paginacja
            // obliczenie ilości stron
            int pages = (int)Math.Ceiling((float)playerList.Count / (float)itemsPerPage);
            // dodanie elementów stron do listy componentów
            for (int i = 1; i <= pages; i++)
            {
                int p = i;
                GUI.Button pageBtn = new GUI.Button(new Vector2f(80f, 50f), new Vector2f(200f + i * 80f, 100f), 40, $"{i}", resources.font, idle, hover, active)
                {
                    onClick = new GUI.Component.Function(delegate () { ShowResults(p); })
                };
                components.Add(pageBtn);
            }
            // dodanie elementów stacyjnych
            GUI.Button idText = new GUI.Button(new Vector2f(100f, 50f), new Vector2f(60f, 150f), 40, "Id:", resources.font, hover, hover, hover);
            GUI.Button nameText = new GUI.Button(new Vector2f(200f, 50f), new Vector2f(250f, 150f), 40, "Name:", resources.font, hover, hover, hover);
            GUI.Button scoreText = new GUI.Button(new Vector2f(200f, 50f), new Vector2f(500f, 150f), 40, "Score:", resources.font, hover, hover, hover);
            GUI.Button timeText = new GUI.Button(new Vector2f(400f, 50f), new Vector2f(830f, 150f), 40, "Total time [sec]:", resources.font, hover, hover, hover);
            GUI.Button pagesText = new GUI.Button(new Vector2f(240f, 50f), new Vector2f(140f, 100f), 40, "Pages:", resources.font, hover, hover, hover);
            GUI.Button backBtn = new GUI.Button(new Vector2f(200f, 80f), new Vector2f(900f, 720f), 70, "back", resources.font, idle, hover, active)
            {
                onClick = new GUI.Component.Function(InitMainMenu)
            };

            components.Add(idText);
            components.Add(nameText);
            components.Add(scoreText);
            components.Add(timeText);
            components.Add(pagesText);
            components.Add(backBtn);

            // dodanie elementów graczy
            for (int i = 0; i < itemsPerPage; i++)
            {
                int id = i + itemsPerPage * (page - 1);
                components.Add(new GUI.Button(new Vector2f(200f, 50f), new Vector2f(250f, 230f + i * 80f), 40, $"{playerList[id].name}", resources.font, idle, idle, idle));
                components.Add(new GUI.Button(new Vector2f(200f, 50f), new Vector2f(500f, 230f + i * 80f), 40, $"{playerList[id].score}", resources.font, idle, idle, idle));
                components.Add(new GUI.Button(new Vector2f(400f, 50f), new Vector2f(830f, 230f + i * 80f), 40, $"{playerList[id].time}", resources.font, idle, idle, idle));
                components.Add(new GUI.Button(new Vector2f(100f, 50f), new Vector2f(60f, 230f + i * 80f), 40, $"{id + 1}.", resources.font, idle, idle, idle));
            }
        }

        public void Update(ref RenderWindow window)
        {
            try
            {
                foreach (var component in components)
                    component.Update(ref window);
            }
            catch(Exception exc) { }
        }

        public void Render(ref RenderWindow window)
        {
            foreach (var component in components)
                component.Render(ref window);
        }
    }
}
