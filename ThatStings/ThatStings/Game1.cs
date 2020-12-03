using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace ThatStings
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private InputHelper inputHelper;
        private GraphicsHelper graphicsHelper;
        int facing; //0 1 2 3 --> N E S W
        Vector2 playerPos, beesPos;
        bool[,] level;
        SoundEffect thud, walk;
        Song bees;
        int beeTimer, beeSpeed = 240, fadeTimer;
        int gamestate = 0; //0 = menu, 1 = playing, 2 = lost, 3 = won.
        public Astar astar;

        public Game1()
        {
            graphicsHelper = new GraphicsHelper(this);
            inputHelper = new InputHelper();
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            thud = Content.Load<SoundEffect>("thud");
            walk = Content.Load<SoundEffect>("walk");
            bees = Content.Load<Song>("bees");
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            Screen one = new Screen(Content.Load<Texture2D>("0"), 1f);
            Objects.List.Add(one);
        }

        void Start()
        {
            Objects.List.Clear();
            Screen one = new Screen(Content.Load<Texture2D>("playing"), 1f);
            Objects.List.Add(one);

            facing = 0;
            beeSpeed = 240;
            beeTimer = 0;

            List<string> levelStrings = new List<string>();
            StreamReader reader = new StreamReader(Content.RootDirectory + "/1.txt"); //Loads the file that contains the level
            string nextLine = reader.ReadLine();
            while (nextLine != null) //Reads the line until it ends
            {
                levelStrings.Add(nextLine);
                nextLine = reader.ReadLine();
            }
            string[] levelArray = levelStrings.ToArray();
            BuildLevel(levelArray);
            astar = new Astar(AStarGrid());
            MediaPlayer.Play(bees);
        }

        void BuildLevel(string[] levelArray)
        {
            level = new bool[levelArray[0].Length, levelArray.Length];
            for (int y = 0; y < levelArray.Length; y++)
            {
                for (int x = 0; x < levelArray[0].Length; x++)
                {
                    switch (levelArray[y][x])
                    {
                        case '#':
                            level[x,y] = true;
                            break;
                        case '.':
                            break;
                        case 'p':
                            playerPos = new Vector2(x, y);
                            break;
                        case 'b':
                            beesPos = new Vector2(x, y);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {

        }

        void Move()
        {
            Vector2 movement = Vector2.Zero;
            switch (facing)
            {
                case 0:
                    movement.Y -= 1;
                    break;
                case 1:
                    movement.X += 1;
                    break;
                case 2:
                    movement.Y += 1;
                    break;
                case 3:
                    movement.X -= 1;
                    break;
            }
            Vector2 newPos = playerPos + movement;

            if (newPos.Y < 0)
            {
                Win();
                return;
            }

            if (level[(int)newPos.X, (int)newPos.Y] == true)
            {
                thud.Play();
                return;
            }


            walk.Play();
            playerPos = newPos;
        }

        void MoveBees()
        {
            Stack<Node> path = astar.FindPath(beesPos, playerPos);
            Node newPos = path.Pop();
            beesPos = newPos.Position;
            beeSpeed -= 10;
        }

        void Lose()
        {
            MediaPlayer.Stop();
            Objects.List.Clear();
            Screen one = new Screen(Content.Load<Texture2D>("lost"), 0f);
            Objects.List.Add(one);
            gamestate = 2;
        }

        void Win()
        {
            MediaPlayer.Stop();
            Objects.List.Clear();
            Screen one = new Screen(Content.Load<Texture2D>("0"), 1f);
            Objects.List.Add(one);
            Screen two = new Screen(Content.Load<Texture2D>("won"), 0f);
            Objects.List.Add(two);
            gamestate = 2;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            inputHelper.Update(gameTime);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            switch (gamestate)
            {
                case 0:
                    fadeTimer++;
                    int a = 200;
                    if (fadeTimer == a)
                    {
                        Screen one = new Screen(Content.Load<Texture2D>("1"), 0f);
                        Objects.List.Add(one);
                    }
                    if (fadeTimer == a * 2)
                    {
                        Screen one = new Screen(Content.Load<Texture2D>("2"), 0f);
                        Objects.List.Add(one);
                    }
                    if (fadeTimer == a * 3)
                    {
                        Screen one = new Screen(Content.Load<Texture2D>("3"), 0f);
                        Objects.List.Add(one);
                    }
                    if (fadeTimer == a * 4)
                    {
                        gamestate++;
                        Start();
                    }
                    break;

                case 1:
                    if (inputHelper.KeyPressed(Keys.W) || inputHelper.KeyPressed(Keys.Up)) Move();
                    if (inputHelper.KeyPressed(Keys.A) || inputHelper.KeyPressed(Keys.Left)) facing--;
                    if (inputHelper.KeyPressed(Keys.D) || inputHelper.KeyPressed(Keys.Right)) facing++;

                    if (facing == -1) facing = 3;
                    if (facing == 4) facing = 0;

                    beeTimer++;
                    if (beeTimer == beeSpeed)
                    {
                        MoveBees();
                        beeTimer = 0;
                    }

                    float volume = 1 / Vector2.Distance(playerPos, beesPos);
                    MediaPlayer.Volume = volume * volume;

                    if (playerPos == beesPos) Lose();
                    break;
                case 2:
                    if (inputHelper.KeyPressed(Keys.Space))
                    {
                        gamestate = 1;
                        Start();
                    }
                    break;
            }

            // TODO: Add your update logic here

            base.Update(gameTime);
            graphicsHelper.Update(gameTime);
            graphicsHelper.HandleInput(inputHelper);
            foreach (GameObject obj in Objects.List)
                obj.HandleInput(inputHelper);
            foreach (GameObject obj in Objects.List)
                obj.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            GraphicsDevice.SetRenderTarget(null);
            graphicsHelper.Draw(gameTime);
        }

        List<List<Node>> AStarGrid()
        {
            List<List<Node>> grid = new List<List<Node>>();
            for (int x = 0; x < level.GetLength(0); x++)
            {
                List<Node> loopList = new List<Node>();
                for (int y = 0; y < level.GetLength(1); y++)
                {
                    Node node;
                    if (level[x,y] == true)
                        node = new Node(new Vector2(x, y), false);
                    else
                        node = new Node(new Vector2(x, y), true);
                    loopList.Add(node);
                }
                grid.Add(loopList);
            }
            return grid;
        }
    }

    public class Node
    {
        // Change this depending on what the desired size is for each element in the grid
        public static int NODE_SIZE = 1;
        public Node Parent;
        public Vector2 Position;
        public Vector2 Center
        {
            get
            {
                return new Vector2(Position.X + NODE_SIZE / 2, Position.Y + NODE_SIZE / 2);
            }
        }
        public float DistanceToTarget;
        public float Cost;
        public float Weight;
        public float F
        {
            get
            {
                if (DistanceToTarget != -1 && Cost != -1)
                    return DistanceToTarget + Cost;
                else
                    return -1;
            }
        }
        public bool Walkable;

        public Node(Vector2 pos, bool walkable, float weight = 1)
        {
            Parent = null;
            Position = pos;
            DistanceToTarget = -1;
            Cost = 1;
            Weight = weight;
            Walkable = walkable;
        }
    }

    public class Astar
    {
        List<List<Node>> Grid;
        int GridRows
        {
            get
            {
                return Grid[0].Count;
            }
        }
        int GridCols
        {
            get
            {
                return Grid.Count;
            }
        }

        public Astar(List<List<Node>> grid)
        {
            Grid = grid;
        }

        public Stack<Node> FindPath(Vector2 Start, Vector2 End)
        {
            Node start = new Node(new Vector2((int)(Start.X / Node.NODE_SIZE), (int)(Start.Y / Node.NODE_SIZE)), true);
            Node end = new Node(new Vector2((int)(End.X / Node.NODE_SIZE), (int)(End.Y / Node.NODE_SIZE)), true);

            Stack<Node> Path = new Stack<Node>();
            List<Node> OpenList = new List<Node>();
            List<Node> ClosedList = new List<Node>();
            List<Node> adjacencies;
            Node current = start;

            // add start node to Open List
            OpenList.Add(start);

            while (OpenList.Count != 0 && !ClosedList.Exists(x => x.Position == end.Position))
            {
                current = OpenList[0];
                OpenList.Remove(current);
                ClosedList.Add(current);
                adjacencies = GetAdjacentNodes(current);


                foreach (Node n in adjacencies)
                {
                    if (!ClosedList.Contains(n) && n.Walkable)
                    {
                        if (!OpenList.Contains(n))
                        {
                            n.Parent = current;
                            n.DistanceToTarget = Math.Abs(n.Position.X - end.Position.X) + Math.Abs(n.Position.Y - end.Position.Y);
                            n.Cost = n.Weight + n.Parent.Cost;
                            OpenList.Add(n);
                            OpenList = OpenList.OrderBy(node => node.F).ToList<Node>();
                        }
                    }
                }
            }

            // construct path, if end was not closed return null
            if (!ClosedList.Exists(x => x.Position == end.Position))
            {
                return null;
            }

            // if all good, return path
            Node temp = ClosedList[ClosedList.IndexOf(current)];
            if (temp == null) return null;
            do
            {
                Path.Push(temp);
                temp = temp.Parent;
            } while (temp != start && temp != null);
            return Path;
        }

        private List<Node> GetAdjacentNodes(Node n)
        {
            List<Node> temp = new List<Node>();

            int row = (int)n.Position.Y;
            int col = (int)n.Position.X;

            if (row + 1 < GridRows)
            {
                temp.Add(Grid[col][row + 1]);
            }
            if (row - 1 >= 0)
            {
                temp.Add(Grid[col][row - 1]);
            }
            if (col - 1 >= 0)
            {
                temp.Add(Grid[col - 1][row]);
            }
            if (col + 1 < GridCols)
            {
                temp.Add(Grid[col + 1][row]);
            }

            return temp;
        }
    }
}
