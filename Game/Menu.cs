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

            public Player(string name, int score, float time)
            {
                this.name = name;
                this.score = score;
                this.time = time;
            }
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

        public void Update(ref RenderWindow window)
        {
            try
            {
                foreach (var component in components)
                    component.Update(ref window);
            }
            catch (Exception) { }
        }

        public void Render(ref RenderWindow window)
        {
            foreach (var component in components)
                component.Render(ref window);
        }

        public void InitMainMenu()
        {
            components.Clear();
            Vector2f size = new Vector2f(1000f, 100f);
            float posX = resources.options.winWidth / 2f;

            GUI.Button ContinueBtn   = new GUI.Button(size, new Vector2f(posX, 100f), 78, "continue", resources.font, idle, hover, active);
            GUI.Button StartAgainBtn = new GUI.Button(size, new Vector2f(posX, 206f), 78, "start again", resources.font, idle, hover, active);
            GUI.Button SelectCarBtn  = new GUI.Button(size, new Vector2f(posX, 312f), 78, "select car", resources.font, idle, hover, active);
            GUI.Button ResultsBtn    = new GUI.Button(size, new Vector2f(posX, 418f), 78, "other players results", resources.font, idle, hover, active);
            GUI.Button ControlBnt    = new GUI.Button(size, new Vector2f(posX, 524f), 78, "show control", resources.font, idle, hover, active);
            GUI.Button ExitBtn       = new GUI.Button(size, new Vector2f(posX, 630f), 78, "exit game", resources.font, idle, hover, active);

            ContinueBtn.onClick   = new GUI.Component.Function(engine.OnContinue);
            StartAgainBtn.onClick = new GUI.Component.Function(engine.OnStartAgain);
            SelectCarBtn.onClick  = new GUI.Component.Function(InitSelectCars);
            ResultsBtn.onClick    = new GUI.Component.Function(InitPlayersResults);
            ControlBnt.onClick    = new GUI.Component.Function(InitControl);
            ExitBtn.onClick       = new GUI.Component.Function(engine.OnExit);

            components.Add(ContinueBtn);
            components.Add(StartAgainBtn);
            components.Add(SelectCarBtn);
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

        private void InitPlayersResults()
        {
            int page = 1;
            ShowPlayersResults(page);
        }

        private void InitSelectCars()
        {
            components.Clear();

            List<Vector2f> shapeSizes = new List<Vector2f>
            {
                new Vector2f(130f, 200f), new Vector2f(130f, 200f),
                new Vector2f(130f, 200f), new Vector2f(130f, 200f),
                new Vector2f(130f, 200f), new Vector2f(130f, 200f),
                new Vector2f(130f, 200f), new Vector2f(130f, 230f),
                new Vector2f(115f, 300f), new Vector2f(120f, 250f),
                new Vector2f(140f, 380f), new Vector2f(140f, 350f),
                new Vector2f(140f, 390f), new Vector2f(130f, 370f),
            };

            List<Vector2f> textureSize = new List<Vector2f>
            {
                new Vector2f(100f, 190f), new Vector2f(100f, 190f),
                new Vector2f(100f, 190f), new Vector2f(100f, 190f),
                new Vector2f(100f, 190f), new Vector2f(100f, 190f),
                new Vector2f(100f, 190f), new Vector2f(100f, 220f),
                new Vector2f(105f, 290f), new Vector2f(110f, 240f),
                new Vector2f(130f, 370f), new Vector2f(130f, 340f),
                new Vector2f(130f, 380f), new Vector2f(120f, 360f),
            };

            List<Vector2f> pos = new List<Vector2f>
            {
                new Vector2f(80f, 150f), new Vector2f(253f, 150f),
                new Vector2f(426f, 150f),new Vector2f(599f, 150f),
                new Vector2f(772f, 150f),new Vector2f(945f, 150f),
                new Vector2f(80f, 375f), new Vector2f(80f, 620f),
                new Vector2f(339f, 702f),new Vector2f(253f, 400f),
                new Vector2f(772f, 465f),new Vector2f(426f, 448f),
                new Vector2f(945f, 475f),new Vector2f(599f, 465f),
            };

            for (int id = 0; id < 14; id++)
            {
                int carID = id;
                float rotation = (id == 8) ? 90f : 0f;
                components.Add(new GUI.Texture(id, resources.carCollection, shapeSizes[id], textureSize[id], pos[id], new Color(Color.Transparent), new Color(Color.Green), new Color(Color.Blue), rotation)
                    {
                        onClick = new GUI.Component.Function(delegate() { engine.OnSelectCar(carID); })
                    }
                );
            }

            components.Add(new GUI.Button(new Vector2f(200f, 80f), new Vector2f(900f, 720f), 70, "back", resources.font, idle, hover, active)
                {
                    onClick = new GUI.Component.Function(InitMainMenu)
                }
            );
        }

        public void InitGameResult()
        {
            components.Clear();

            int score = engine.score;
            double gameTime = engine.gameTime.GetCurrentTime();
            double alcoTime = engine.alcoTime.GetCurrentTime();

            components.Add(new GUI.Text(new Vector2f(512f, 150f), 90, "YOU LOSE!", resources.font, hover));
            components.Add(new GUI.Text(new Vector2f(200f, 300f), 60, "User name:", resources.font, hover));
            components.Add(new GUI.Text(new Vector2f(512f, 450f), 50, $"Your score {score}, game time: {(int)gameTime} sec", resources.font, hover));
            components.Add(new GUI.Text(new Vector2f(512f, 520f), 50, $"drinking time: {(int)alcoTime} sec", resources.font, hover));

            GUI.Input input = new GUI.Input(new Vector2f(370f, 260f), 60, resources.font, idle, resources.keys);
            input.onEnter = new GUI.Component.Function(delegate () { Save(input.text, score, gameTime); });
            components.Add(input);

            components.Add(new GUI.Button(new Vector2f(700f, 95f), new Vector2f(512f, 720f), 70, "save and start again", resources.font, idle, hover, active)
                {
                    onClick = new GUI.Component.Function(delegate() { Save(input.text, score, gameTime); })
                }
            );
        }

        private void ShowPlayersResults(int page)
        {
            // ilość graczy na stronie
            int itemsPerPage = 6;
            // czyszczenie listy componentów
            components.Clear();
            // tworzenie listy z zapisanymi graczami
            CreatePlayerList(out List<Player> playerList);
            // paginacja
            CreatePagination(playerList, itemsPerPage);
            // dodawanie elementów stacyjnych
            LoadStationElements(page);
            // dodanie elementów graczy
            LoadPlayersResults(playerList, page, itemsPerPage);
        }

        private void CreatePlayerList(out List<Player> list)
        {
            list = new List<Player>();
            XmlNodeList playerNodes = resources.document.GetElementsByTagName("player");
            // utworzenie wewnętrznej listy z graczami
            foreach (XmlNode node in playerNodes)
            {
                string name = node.Attributes["name"].Value;
                int score = Convert.ToInt32(node.Attributes["score"].Value);
                float time = float.Parse(node.Attributes["time"].Value);
                list.Add(new Player(name, score, time));
            }
            // sortowanie listy
            list.Sort(
                delegate (Player A, Player B)
                {
                    if (A.score > B.score) return -1;
                    else if (A.score < B.score) return 1;
                    return 0;
                }
            );
        }

        private void CreatePagination(List<Player> list, int itemsPerPage)
        {
            // obliczenie ilości stron
            int pages = (int)Math.Ceiling((float)list.Count / (float)itemsPerPage);
            // dodanie elementów stron do listy componentów
            for (int i = 1; i <= pages; i++)
            {
                int pagePtr = i;
                components.Add(new GUI.Button(new Vector2f(80f, 50f), new Vector2f(200f + i * 80f, 100f), 40, $"{i}", resources.font, idle, hover, active)
                {
                    onClick = new GUI.Component.Function(delegate () { ShowPlayersResults(pagePtr); })
                }
                );
            }
        }

        private void LoadStationElements(int page)
        {
            // dodanie elementów stacyjnych
            components.Add(new GUI.Text(new Vector2f(60f, 150f), 40, "Id:", resources.font, hover));
            components.Add(new GUI.Text(new Vector2f(250f, 150f), 40, "Name:", resources.font, hover));
            components.Add(new GUI.Text(new Vector2f(500f, 150f), 40, "Score:", resources.font, hover));
            components.Add(new GUI.Text(new Vector2f(830f, 150f), 40, "Total time [sec]:", resources.font, hover));
            components.Add(new GUI.Text(new Vector2f(140f, 100f), 40, $"Pages ({page}):", resources.font, hover));

            components.Add(new GUI.Button(new Vector2f(200f, 80f), new Vector2f(900f, 720f), 70, "back", resources.font, idle, hover, active)
            {
                onClick = new GUI.Component.Function(InitMainMenu)
            }
            );
        }

        private void LoadPlayersResults(List<Player> list, int page, int itemsPerPage)
        {
            int maxNameLen = 12;
            // dodanie elementów graczy
            for (int i = 0; i < itemsPerPage; i++)
            {
                int id = i + itemsPerPage * (page - 1);

                string name = list[id].name.Trim();

                if (name.Length > maxNameLen)
                {
                    string newName = "";
                    for (int j = 0; j < maxNameLen - 3; j++)
                        newName += name[j];
                    
                    name = newName.Trim() + "...";
                }

                components.Add(new GUI.Text(new Vector2f(250f, 230f + i * 80f), 40, $"{name}", resources.font, idle));
                components.Add(new GUI.Text(new Vector2f(500f, 230f + i * 80f), 40, $"{list[id].score}", resources.font, idle));
                components.Add(new GUI.Text(new Vector2f(830f, 230f + i * 80f), 40, $"{list[id].time}", resources.font, idle));
                components.Add(new GUI.Text(new Vector2f(60f, 230f + i * 80f), 40, $"{id + 1}.", resources.font, idle));
            }
        }

        private void Save(Text text, int score, double time)
        {
            XmlNode playersNode = resources.document.GetElementsByTagName("players")[0];
            XmlElement newPlayer = resources.document.CreateElement("player");

            newPlayer.SetAttribute("name", text.DisplayedString);
            newPlayer.SetAttribute("score", score.ToString());
            newPlayer.SetAttribute("time", ((int)time).ToString());

            playersNode.AppendChild(newPlayer);
            resources.document.Save("..\\..\\..\\Config.xml");

            engine.OnStartAgain();
        }
    }
}
