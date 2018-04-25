using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StudioForge.BlockWorld;
using StudioForge.Engine;
using StudioForge.Engine.Core;
using StudioForge.Engine.Integration;
using StudioForge.TotalMiner;
using StudioForge.TotalMiner.API;

namespace RandomBricksArcade
{
    class BrickbreakerGame : ArcadeMachine
    {
        #region Enum

        public enum GameState
        {
            Play,
            GameOverTransition,
            GameOver

        }

        public enum GameMenu
        {
            Main,
            Controls
        }

        #endregion

        #region Fields
        public int GlobalScale = 2;
        public GameState State;
        public Point ScreenSize;
        public static PcgRandom Random;
        public string ScoreText1;
        public string LivesText;
        public int GameScreenY; //height of play area
        public List<Brick> Bricks = new List<Brick>();
        Brick brickTemplate;
        public List<PowerUp> PowerUps = new List<PowerUp>();
        public List<Bullet> Bullets = new List<Bullet>();
        public List<PlayerBall> Balls = new List<PlayerBall>();
        public ITMTexturePack texturePack;

        public HighScore highScore;
        public string messageToPlayer = string.Empty;
        public GameMenu currentMenu;
        public int highScoreRandom = 0;
        public int highScoreItem = 0;

        public bool useRandomLayout = true;
        public bool pauseGame = false;
        public bool loadingNewLevel = false;
        public bool playerAlive = true;

        public Color ballColor = Color.White;

        //for power ups
        public PowerUpTracker powerUpTracker;
        public Rectangle iconRect;
        public bool stickToPaddle = false;
        public bool heavyBall = false;
        public bool slowBall = false;
        public bool resetBallVel = false;
        public bool InvertControls = false;
        public bool RandomBounce = false;
        public bool CurveBall = false;

        public int bulletsRemain = 0;
        Bullet bulletTemplate = new Bullet();
        float nextFire;

        float gameOverTransitionTimer;
        float deathTransitionTimer;
        float newLevelTransitionTimer;
        bool allowPlayerMovement = true;
        int lives;
        int score;

        public PlayerPaddle Paddle;

        //float mouseDampen;

        int currentLevel;
        int nextLifeUp;
        int livesAwarded;

        int messageToPlayerTimer = 0;
        public Item levelItem; //starting item
        List<Item> LowLevelItemList;
        List<Item> MidLevelItemList;
        List<Item> HighLevelItemList;

        float ballSpeedTimer;



        //for testing only
#if DEBUG
        bool autoPlay = false;

#endif
        bool testSlowBall = false;

        #endregion

        #region Properties

        public override bool CanDeactivate { get { return State == GameState.GameOver; } }

        #endregion

        #region Initialization

        public BrickbreakerGame(ITMGame game, ITMMap map, ITMPlayer player, GlobalPoint3D point, BlockFace face)
            : base(game, map, player, point, face)
        {


        }

        protected override void CreateRenderTarget()
        {
            //default render is 320,240
            renderTarget = new RenderTarget2D(CoreGlobals.GraphicsDevice, 640, 480, false, SurfaceFormat.Bgra5551, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
        }

        public override void LoadContent(InitState state)
        {
            base.LoadContent(state);

            //brickTemplate = new Brick(GlobalScale, 1);

            texturePack = game.TexturePack;
            powerUpTracker = new PowerUpTracker(game, this);
            ScreenSize = new Point(renderTarget.Width, renderTarget.Height);

            iconRect = new Rectangle(ScreenSize.X - 25, ScreenSize.Y - 10, 2, 3);

            State = GameState.GameOver;
            Random = new PcgRandom(new Random().Next());

            Paddle = new PlayerPaddle(ScreenSize, GlobalScale);

            Balls = new List<PlayerBall>();

            //mouseDampen = 4;
            GameScreenY = 16; //top of play area 

            highScore = new HighScore();
            highScoreItem = highScore.HighScoreForItemLayout();
            highScoreRandom = highScore.HighScoreForRandomLayout();
            currentMenu = GameMenu.Main;

            CreateRandomItemLists();

            SetStartStats();
        }

        #endregion


        #region HandleInput

        public override bool HandleInput()
        {
            if (State == GameState.Play)
            {
                if (allowPlayerMovement)
                {
                    var right = InputManager.GetGamepadRightStick(tmPlayer.PlayerIndex);
                    if (right.Y != 0 || right.X != 0)
                    {

                        if (!InvertControls)
                        {
                            Paddle.CenterPosX += right.X * Paddle.Speed;
                        }
                        else
                        {
                            Paddle.CenterPosX -= right.X * Paddle.Speed;
                        }
                    }

                    var left = InputManager.GetGamepadLeftStick(tmPlayer.PlayerIndex);
                    if (left.Y != 0 || left.X != 0)
                    {
                        
                        if (!InvertControls)
                        {
                            Paddle.CenterPosX += left.X * Paddle.Speed;
                        }
                        else
                        {
                            Paddle.CenterPosX -= left.X * Paddle.Speed;
                        }
                    }

                    var mouse = InputManager.GetMousePosDelta(tmPlayer.PlayerIndex);
                    if (mouse.X != 0)
                    {
                        if (!InvertControls)
                        {
                            Paddle.CenterPosX += mouse.X;// / mouseDampen;

                        }
                        else
                        {
                            Paddle.CenterPosX -= mouse.X;// / mouseDampen;

                        }

                    }

                    if (InputManager.IsKeyPressed(tmPlayer.PlayerIndex, Keys.D))
                    {
                        if (!InvertControls)
                        {
                            Paddle.CenterPosX += Paddle.Speed;
                        }
                        else
                        {
                            Paddle.CenterPosX -= Paddle.Speed;
                        }

                    }

                    if (InputManager.IsKeyPressed(tmPlayer.PlayerIndex, Keys.A))
                    {
                        if (!InvertControls)
                        {
                            Paddle.CenterPosX -= Paddle.Speed;
                        }
                        else
                        {
                            Paddle.CenterPosX += Paddle.Speed;
                        }
                    }

                    nextFire -= Services.ElapsedTime;
                    if (InputManager.IsKeyPressed(tmPlayer.PlayerIndex, Keys.W)
                        || InputManager.IsButtonPressed(tmPlayer.PlayerIndex, Buttons.A)
                        || InputManager.IsMouseButtonPressedNew(tmPlayer.PlayerIndex, MouseButtons.LeftButton))
                    {

                        for (int i = 0; i < Balls.Count; i++)
                        {
                            Balls[i].StuckToPaddle = false;
                        }



                        if (bulletsRemain > 0 && nextFire < 0)
                        {
                            Bullets.Add(new Bullet(new Vector2(Paddle.CenterPos.X, Paddle.CenterPos.Y), GlobalScale));
                            bulletsRemain--;
                            nextFire = bulletTemplate.RoF;
                        }

                    }
                }


                if (InputManager.IsKeyPressedNew(TMPlayer.PlayerIndex, Keys.P) || InputManager.IsButtonPressedNew(TMPlayer.PlayerIndex, Buttons.Back))
                {
                    pauseGame = !pauseGame;
                    if (pauseGame)
                    {
                        allowPlayerMovement = false;
                    }
                    else
                    {
                        allowPlayerMovement = true;
                    }

                }


                if (InputManager1.IsInputReleasedNew(TMPlayer.PlayerIndex, GuiInput.ExitScreen))
                {
                    GameOver(false);
                }
#if DEBUG
                if (InputManager.IsKeyReleasedNew(tmPlayer.PlayerIndex, Keys.N))
                {
                    currentLevel += 1;
                    //stickyBall = true;

                    ResetAllBalls();


                    GenerateLevel(currentLevel);
                    for (int i = 0; i < Balls.Count; i++)
                    {
                        Balls[i].CurrentVelocity = new Vector2(1, 1);
                    }

                    return true;
                }

                if (InputManager.IsKeyPressed(tmPlayer.PlayerIndex, Keys.S))
                {
                    testSlowBall = true;
                    return true;
                }
                else
                    testSlowBall = false;

                if (InputManager.IsKeyReleasedNew(tmPlayer.PlayerIndex, Keys.B))
                {
                    bulletsRemain += 10;
                    SpawnNewBalls(2);
                    // Paddle.WidthMultiplier += 1;
                    for (int i = 0; i < Balls.Count; i++)
                    {
                        //Balls[i].SizeMultiplier += 1;
                    }
                }
#endif
                return true;
            }

            if (State == GameState.GameOver)
            {
                if (InputManager.IsButtonReleasedNew(TMPlayer.PlayerIndex, Buttons.A) || InputManager.IsKeyReleasedNew(TMPlayer.PlayerIndex, Keys.Enter))
                {
                    if (currentMenu == GameMenu.Main)
                        currentMenu = GameMenu.Controls;
                    else
                        currentMenu = GameMenu.Main;
                    return true;
                }
                if (InputManager1.IsInputReleasedNew(TMPlayer.PlayerIndex, GuiInput.MsgBoxY))
                {
                    useRandomLayout = !useRandomLayout;
                    return true;
                }

                return InputManager.IsButtonPressed(TMPlayer.PlayerIndex, Buttons.A) ||
                        InputManager.IsKeyPressed(TMPlayer.PlayerIndex, Keys.Enter) ||
                        InputManager1.IsInputPressed(TMPlayer.PlayerIndex, GuiInput.MsgBoxY);

            }

            if(State == GameState.GameOverTransition)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region LevelCreation
        private void CreateRandomItemLists()
        {
            CreateItemLevelLists();
            List.Shuffle<Item>(LowLevelItemList);
            List.Shuffle<Item>(MidLevelItemList);
            List.Shuffle<Item>(HighLevelItemList);
        }

        private void GenerateLevel(int level)
        {
            ballSpeedTimer = 0;
            Bricks = new List<Brick>();

            for (int i = 0; i < Balls.Count; i++)
            {
                Balls[i].UpdateVelocity(level);
                Balls[i].StuckToPaddle = true;

            }


            if (useRandomLayout)
            {
                GenerateRandomLayout(level);
            }
            else
            {
                GenerateSpriteLayout(level);
            }

        }

        private void GenerateSpriteLayout(int level)
        {


            if (level <= LowLevelItemList.Count)
            {
                levelItem = LowLevelItemList[level - 1];

            }
            else if (level >= LowLevelItemList.Count && level <= MidLevelItemList.Count + LowLevelItemList.Count)
            {
                levelItem = MidLevelItemList[level - LowLevelItemList.Count - 1];

            }
            else if (level >= MidLevelItemList.Count + LowLevelItemList.Count && level <= HighLevelItemList.Count + MidLevelItemList.Count + LowLevelItemList.Count)
            {
                levelItem = HighLevelItemList[level - LowLevelItemList.Count - MidLevelItemList.Count - 1];

            }
            else
            {
                //find a better way to do this when items run out, maybe use the TM logo?
                levelItem = Item.NaturalWorld;
                //ERROR NO ITEM player has passed all the levels WTF?
            }

            //for testing only
            //levelItem = Item.Blueberries;

            Color[] pixelColor = game.TexturePack.GetItemColorData(levelItem);
            int curIndex = 0;

            List<Color> colorList = new List<Color>();


            var size = game.TexturePack.ItemTextureSize();

            int brickPerRow = 16;
            int numOfRows = 16;
            int modifier = 1;

            if (size == 32)
            {
                brickPerRow = 32;
                numOfRows = 32;
                modifier = 2;
            }

            brickTemplate = new Brick(GlobalScale, modifier);

            //MessagePlayer(90, levelItem.ToString());

            Vector2 startPoint = new Vector2(brickTemplate.Width * 2, brickTemplate.Height * 4);


            float curX = startPoint.X;
            float curY = startPoint.Y + GameScreenY;



            for (int x = 0; x < pixelColor.Length; x++)
            {
                Color color = pixelColor[x];
                if (color.R < 10 && color.G < 10 && color.B < 10 && color != Color.Transparent)
                {
                    //color is black and cant see it
                    color = new Color(10, 10, 10);
                }
                colorList.Add(color);
            }



            for (int i = 0; i < numOfRows; i++)
            {
                for (int j = 0; j < brickPerRow; j++)
                {
                    //block here?
                    if (colorList[curIndex] != Color.Transparent)
                    {
                        int hp = ColorToHealth(colorList[curIndex]);
                        bool bo = hp < 10 ? true : false;
                        Bricks.Add(new Brick(curX, curY, hp, bo, colorList[curIndex], GlobalScale, modifier));


                    }
                    curIndex++;
                    curX += brickTemplate.Width;

                }
                curX = startPoint.X;
                curY += brickTemplate.Height;
            }

            //  game.AddNotification("Before Count " + Bricks.Count);
            CombineBricks();
            // game.AddNotification("After Count " + Bricks.Count);
        }

        void CombineBricks()
        {

            for (int i = 0; i + 1 < Bricks.Count; i++)
            {

                if (ColorsAreClose(Bricks[i].color, Bricks[i + 1].color) && Bricks[i].pos.Y == Bricks[i + 1].pos.Y && Bricks[i].pos.X + Bricks[i].Width == Bricks[i + 1].pos.X)
                {
                    Bricks[i].ResizeBrick(Bricks[i].Width + Bricks[i + 1].Width);
                    Bricks.RemoveAt(i + 1);
                }
            }

        }

        bool ColorsAreClose(Color a, Color z, int threshold = 75)
        {
            int r = (int)a.R - z.R,
                g = (int)a.G - z.G,
                b = (int)a.B - z.B;

            return (r * r + g * g + b * b) <= threshold * threshold;
        }

        private void GenerateRandomLayout(int level)
        {
            brickTemplate = new Brick(GlobalScale, 1);

            Vector2 startPoint;
            if (currentLevel < 30)
            {
                startPoint = new Vector2(brickTemplate.Width, brickTemplate.Height * (5 - (currentLevel / 10))); //indent
            }
            else
            {
                startPoint = new Vector2(brickTemplate.Width * 2, brickTemplate.Height * 2);
            }

            int brickPerRow = 16;
            int numOfRows = 16;

            float curX = startPoint.X;
            float curY = startPoint.Y + GameScreenY;
            Random r = new Random();

            int maxBlocks = 8 + (level * 2);

            for (int i = 0; i < numOfRows; i++)
            {
                for (int j = 0; j < brickPerRow; j++)
                {

                    int nextValue = r.Next(0, 100); // Returns a random number from 0-99

                    //block here?
                    if (nextValue > 50 && maxBlocks > 0)
                    {
                        nextValue = r.Next(0, 100);
                        int hp = 1;//block health
                        bool b = true;//is breakable

                        if (currentLevel >= 5)
                        {
                            if (nextValue >= 50 && nextValue <= 90) { hp = 2; }
                        }
                        if (currentLevel >= 10)
                        {
                            if (nextValue > 90 && nextValue <= 98) { hp = 3; }
                            if (nextValue == 99) { hp = 10; b = false; }
                        }
                        if (currentLevel >= 15)
                        {
                            if (nextValue > 75 && nextValue <= 90) { hp = 3; }
                            if (nextValue > 90) { hp = 10; b = false; }
                        }


                        Bricks.Add(new Brick(curX, curY, hp, b, i, GlobalScale));
                        maxBlocks--;
                    }
                    curX += brickTemplate.Width;

                }
                curX = startPoint.X;
                curY += brickTemplate.Height;
            }
        }

        void CreateItemLevelLists()
        {
            LowLevelItemList = new List<Item>();
            MidLevelItemList = new List<Item>();
            HighLevelItemList = new List<Item>();

            #region LowLevel
            LowLevelItemList.Add(Item.Bullet);
            //LowLevelItemList.Add(Item.Splinter1);
            LowLevelItemList.Add(Item.SteelSword);
            LowLevelItemList.Add(Item.TomatoSeed);
            LowLevelItemList.Add(Item.Blueberries);
            LowLevelItemList.Add(Item.Wand);
            //LowLevelItemList.Add(Item.Splinter2);
            LowLevelItemList.Add(Item.DiamondSword);
            LowLevelItemList.Add(Item.Stick);
            LowLevelItemList.Add(Item.FlintArrow);
            LowLevelItemList.Add(Item.Revolver);
            LowLevelItemList.Add(Item.TitaniumArrow);
            LowLevelItemList.Add(Item.IronArrow);
            LowLevelItemList.Add(Item.BronzeArrow);
            LowLevelItemList.Add(Item.NatureStaff);
            LowLevelItemList.Add(Item.Plum);
            LowLevelItemList.Add(Item.IronSword);
            LowLevelItemList.Add(Item.DiamondArrow);
            LowLevelItemList.Add(Item.RubyArrow);
            LowLevelItemList.Add(Item.BoomArrow);
            LowLevelItemList.Add(Item.SteelArrow);
            LowLevelItemList.Add(Item.IceArrow);
            LowLevelItemList.Add(Item.BronzeSword);
            LowLevelItemList.Add(Item.SugarCaneSeed);
            LowLevelItemList.Add(Item.SteelKatana);
            LowLevelItemList.Add(Item.DarkStaff);
            LowLevelItemList.Add(Item.RubySword);
            LowLevelItemList.Add(Item.TitaniumSword);
            // LowLevelItemList.Add(Item.Splinter3);
            LowLevelItemList.Add(Item.WoodBow);
            LowLevelItemList.Add(Item.ElvenBow);
            LowLevelItemList.Add(Item.SniperRifle);
            LowLevelItemList.Add(Item.Shotgun);
            LowLevelItemList.Add(Item.WheatSeed);
            LowLevelItemList.Add(Item.TitaniumKatana);
            LowLevelItemList.Add(Item.SteelScimitar);
            LowLevelItemList.Add(Item.WoodSword);
            LowLevelItemList.Add(Item.Chisel);
            LowLevelItemList.Add(Item.Cherries);
            LowLevelItemList.Add(Item.LightStaff);
            LowLevelItemList.Add(Item.GreenstoneGoldSword);
            LowLevelItemList.Add(Item.AssaultRifle);
            LowLevelItemList.Add(Item.Rosemary);
            LowLevelItemList.Add(Item.SteelBattleAxe);
            LowLevelItemList.Add(Item.DiamondSpear);
            LowLevelItemList.Add(Item.SkillRanged);
            LowLevelItemList.Add(Item.WoodSpear);
            LowLevelItemList.Add(Item.FireArrow);
            LowLevelItemList.Add(Item.IronSpear);
            LowLevelItemList.Add(Item.SteelSpear);
            LowLevelItemList.Add(Item.Gooseberries);
            LowLevelItemList.Add(Item.DwarfKey);
            LowLevelItemList.Add(Item.GoldNecklace);
            LowLevelItemList.Add(Item.TrollBow);
            LowLevelItemList.Add(Item.Lemon);
            LowLevelItemList.Add(Item.SemiAutoHandGun);
            LowLevelItemList.Add(Item.Lime);
            LowLevelItemList.Add(Item.IronPickaxe);
            LowLevelItemList.Add(Item.BattleAxe);
            LowLevelItemList.Add(Item.IronHoe);
            LowLevelItemList.Add(Item.SteelHoe);
            LowLevelItemList.Add(Item.DiamondHoe);
            LowLevelItemList.Add(Item.BronzeHoe);

            #endregion


            #region MidLevel
            MidLevelItemList.Add(Item.GoldKey);
            MidLevelItemList.Add(Item.SteelClaymore);
            MidLevelItemList.Add(Item.Strawberries);
            MidLevelItemList.Add(Item.TitaniumKey);
            MidLevelItemList.Add(Item.PlatinumSword);
            MidLevelItemList.Add(Item.BronzeSpear);
            MidLevelItemList.Add(Item.WoodPickaxe);
            MidLevelItemList.Add(Item.NecklaceOfKnowledge);
            MidLevelItemList.Add(Item.NecklaceOfHypocrisy);
            MidLevelItemList.Add(Item.NecklaceOfFarsight);
            MidLevelItemList.Add(Item.Raspberries);
            MidLevelItemList.Add(Item.DiamondPickaxe);
            MidLevelItemList.Add(Item.RubyPickaxe);
            MidLevelItemList.Add(Item.Feather);
            MidLevelItemList.Add(Item.UnknownNecklace);
            MidLevelItemList.Add(Item.RubyWarHammer);
            MidLevelItemList.Add(Item.Olives);
            MidLevelItemList.Add(Item.Lock);
            MidLevelItemList.Add(Item.GoldRing);
            MidLevelItemList.Add(Item.HeroKey);
            MidLevelItemList.Add(Item.SteelPike);
            MidLevelItemList.Add(Item.RingOfBob);
            MidLevelItemList.Add(Item.SpiderRing);
            MidLevelItemList.Add(Item.RingOfExemption);
            //MidLevelItemList.Add(Item.Splinter4);
            MidLevelItemList.Add(Item.SteelPickaxe);
            MidLevelItemList.Add(Item.Grapes);
            MidLevelItemList.Add(Item.WoodHatchet);
            MidLevelItemList.Add(Item.SkullKey);
            MidLevelItemList.Add(Item.GhostKey);
            MidLevelItemList.Add(Item.FishKey);
            MidLevelItemList.Add(Item.IronScythe);
            MidLevelItemList.Add(Item.SteelScythe);
            MidLevelItemList.Add(Item.DiamondScythe);
            MidLevelItemList.Add(Item.TitaniumBattleAxe);
            MidLevelItemList.Add(Item.BoneMeal);
            MidLevelItemList.Add(Item.TitaniumBow);
            MidLevelItemList.Add(Item.BronzeScythe);
            MidLevelItemList.Add(Item.Blackberries);
            MidLevelItemList.Add(Item.Thyme);
            MidLevelItemList.Add(Item.TitaniumPickaxe);
            MidLevelItemList.Add(Item.TitaniumWarHammer);
            MidLevelItemList.Add(Item.CopperIngot);
            MidLevelItemList.Add(Item.TinIngot);
            MidLevelItemList.Add(Item.BronzeIngot);
            MidLevelItemList.Add(Item.EldarPistol);
            MidLevelItemList.Add(Item.DiamondShovel);
            MidLevelItemList.Add(Item.NatureKey);
            MidLevelItemList.Add(Item.NauticalKey);
            MidLevelItemList.Add(Item.WoodShovel);
            MidLevelItemList.Add(Item.IronBattleAxe);
            MidLevelItemList.Add(Item.GreenstoneGoldPickaxe);
            MidLevelItemList.Add(Item.SlimeKey);
            MidLevelItemList.Add(Item.OceanStaff);
            MidLevelItemList.Add(Item.SkeletonKey);
            MidLevelItemList.Add(Item.DarkKey);
            MidLevelItemList.Add(Item.GreenstoneGoldSledgeHammer);
            MidLevelItemList.Add(Item.MiniGun);
            MidLevelItemList.Add(Item.HeavyAssaultRifle);
            MidLevelItemList.Add(Item.RubyKey);
            MidLevelItemList.Add(Item.DiamondBattleAxe);
            MidLevelItemList.Add(Item.Orange);
            MidLevelItemList.Add(Item.DiamondKey);
            MidLevelItemList.Add(Item.LightningKey);
            MidLevelItemList.Add(Item.SpiderBow);
            MidLevelItemList.Add(Item.RawDuckMeat);
            MidLevelItemList.Add(Item.BobKey);
            MidLevelItemList.Add(Item.GoldenBow);
            MidLevelItemList.Add(Item.PlasmaRifle);
            MidLevelItemList.Add(Item.GrenadeLauncher);
            MidLevelItemList.Add(Item.SpiderStaff);
            MidLevelItemList.Add(Item.Oregano);
            MidLevelItemList.Add(Item.CookedDuckMeat);
            MidLevelItemList.Add(Item.SpiderKey);
            MidLevelItemList.Add(Item.NecromancerStaff);
            MidLevelItemList.Add(Item.KeyOfLight);
            MidLevelItemList.Add(Item.SteelHatchet);
            MidLevelItemList.Add(Item.ShieldBadge);
            MidLevelItemList.Add(Item.RubyBattleAxe);
            MidLevelItemList.Add(Item.RingOfIce);
            MidLevelItemList.Add(Item.SkillAttack);
            MidLevelItemList.Add(Item.Bed);
            MidLevelItemList.Add(Item.StoneKey);
            MidLevelItemList.Add(Item.Wheat);
            MidLevelItemList.Add(Item.SteelShovel);
            MidLevelItemList.Add(Item.Grenade);
            //MidLevelItemList.Add(Item.RopeIcon);
            MidLevelItemList.Add(Item.GreenstoneGoldShovel);
            MidLevelItemList.Add(Item.SapphireGemStone);
            MidLevelItemList.Add(Item.Grapefruit);
            MidLevelItemList.Add(Item.IronHatchet);
            MidLevelItemList.Add(Item.IronShovel);
            MidLevelItemList.Add(Item.GoldPieces);
            MidLevelItemList.Add(Item.CelestialKey);
            MidLevelItemList.Add(Item.GoldAmulet);
            MidLevelItemList.Add(Item.FlintFlake);
            MidLevelItemList.Add(Item.SilkKey);
            MidLevelItemList.Add(Item.TeleportKey);
            MidLevelItemList.Add(Item.AmuletOfFlight);
            MidLevelItemList.Add(Item.GreenstoneGoldBattleAxe);
            MidLevelItemList.Add(Item.DecalApplicator);
            MidLevelItemList.Add(Item.DebugTool);
            MidLevelItemList.Add(Item.DiamantiumSword);
            MidLevelItemList.Add(Item.Sage);
            MidLevelItemList.Add(Item.ComboAssaultRifle);
            // MidLevelItemList.Add(Item.TableIcon);
            // MidLevelItemList.Add(Item.Splinter5);
            MidLevelItemList.Add(Item.MagicPotion);
            MidLevelItemList.Add(Item.DiamondHatchet);
            MidLevelItemList.Add(Item.SolarKey);
            MidLevelItemList.Add(Item.Tarragon);
            MidLevelItemList.Add(Item.TitaniumIngot);
            MidLevelItemList.Add(Item.AmuletOfFury);
            MidLevelItemList.Add(Item.DiamantiumIngot);
            MidLevelItemList.Add(Item.Lavender);
            MidLevelItemList.Add(Item.SledgeHammer);
            MidLevelItemList.Add(Item.ShadowKey);
            MidLevelItemList.Add(Item.UndeadKey);
            MidLevelItemList.Add(Item.SkillHealth);
            MidLevelItemList.Add(Item.LunaKey);
            MidLevelItemList.Add(Item.SkillDigging);
            MidLevelItemList.Add(Item.RubyGemStone);
            MidLevelItemList.Add(Item.DiamondGemStone);
            MidLevelItemList.Add(Item.IronIngot);
            MidLevelItemList.Add(Item.SteelIngot);
            MidLevelItemList.Add(Item.GoldBar);
            //MidLevelItemList.Add(Item.SkillChopping);
            MidLevelItemList.Add(Item.RawFish);
            MidLevelItemList.Add(Item.Apple);
            MidLevelItemList.Add(Item.CookedFish);
            // MidLevelItemList.Add(Item.SwitchIcon);
            MidLevelItemList.Add(Item.ButtonIcon);
            MidLevelItemList.Add(Item.SkillMining);




            #endregion

            #region HighLevel
            HighLevelItemList.Add(Item.GreenstoneGoldHatchet);
            HighLevelItemList.Add(Item.RawBeef);
            HighLevelItemList.Add(Item.TitanKey);
            HighLevelItemList.Add(Item.Banana);
            HighLevelItemList.Add(Item.BayLeaves);
            HighLevelItemList.Add(Item.RawLambChops);
            //HighLevelItemList.Add(Item.SpiderSMG);
            HighLevelItemList.Add(Item.Tomato);
            HighLevelItemList.Add(Item.LaserBlaster);
            HighLevelItemList.Add(Item.Bottle);
            HighLevelItemList.Add(Item.BottleOfWater);
            HighLevelItemList.Add(Item.BottleOfMilk);
            HighLevelItemList.Add(Item.CookedBeef);
            HighLevelItemList.Add(Item.PredatorAmulet);
            HighLevelItemList.Add(Item.SkillSmithing);
            HighLevelItemList.Add(Item.UnknownAmulet);
            HighLevelItemList.Add(Item.GoldenSMG);
            HighLevelItemList.Add(Item.AmuletOfStarlight);
            HighLevelItemList.Add(Item.Bucket);
            HighLevelItemList.Add(Item.BucketOfWater);
            HighLevelItemList.Add(Item.BucketOfLava);
            HighLevelItemList.Add(Item.BucketOfMilk);
            HighLevelItemList.Add(Item.EssenciaPotion);
            HighLevelItemList.Add(Item.KarmicPotion);
            HighLevelItemList.Add(Item.CookedLambChops);
            HighLevelItemList.Add(Item.FenceIcon);
            //HighLevelItemList.Add(Item.Splinter6);
            HighLevelItemList.Add(Item.VaporPotion);
            HighLevelItemList.Add(Item.Potato);
            HighLevelItemList.Add(Item.SkillStrength);
            HighLevelItemList.Add(Item.IchorPotion);
            HighLevelItemList.Add(Item.IronHelmet);
            HighLevelItemList.Add(Item.AstroPotion);
            HighLevelItemList.Add(Item.WoodDoor);
            HighLevelItemList.Add(Item.SteelDoor);
            //HighLevelItemList.Add(Item.RampIcon);
            HighLevelItemList.Add(Item.Egg);
            HighLevelItemList.Add(Item.SteelHelmet);
            HighLevelItemList.Add(Item.Parsley);
            HighLevelItemList.Add(Item.Salt);
            HighLevelItemList.Add(Item.SkillDefence);
            HighLevelItemList.Add(Item.Cheese);
            HighLevelItemList.Add(Item.LeatherBoots);
            HighLevelItemList.Add(Item.Coriander);
            HighLevelItemList.Add(Item.DwarvenPotion);
            HighLevelItemList.Add(Item.IronGauntlets);
            HighLevelItemList.Add(Item.Lighter);
            HighLevelItemList.Add(Item.ObsidianGemStone);
            HighLevelItemList.Add(Item.SkillSmelting);
            HighLevelItemList.Add(Item.TrollHideBody);
            HighLevelItemList.Add(Item.LeatherLeggings);
            HighLevelItemList.Add(Item.EctoplasmFlask);
            HighLevelItemList.Add(Item.Dill);
            //HighLevelItemList.Add(Item.LockedDoor);
            //HighLevelItemList.Add(Item.HalfBlockIcon);
            HighLevelItemList.Add(Item.SteelGauntlets);
            HighLevelItemList.Add(Item.DiamantiumGauntlets);
            HighLevelItemList.Add(Item.SkillCombat);
            //HighLevelItemList.Add(Item.HalfBlock2Icon);
            HighLevelItemList.Add(Item.Chives);
            HighLevelItemList.Add(Item.DiamantiumBoots);
            HighLevelItemList.Add(Item.YliasterPotion);
            HighLevelItemList.Add(Item.SkoomaFlask);
            HighLevelItemList.Add(Item.IronBoots);
            HighLevelItemList.Add(Item.SteelBoots);
            HighLevelItemList.Add(Item.SteelLeggings);
            HighLevelItemList.Add(Item.DiamantiumBody);
            HighLevelItemList.Add(Item.TitaniumHelmet);
            HighLevelItemList.Add(Item.TitaniumGauntlets);
            //HighLevelItemList.Add(Item.SkillBuilding);
            //HighLevelItemList.Add(Item.SkillCrafting);
            // HighLevelItemList.Add(Item.Splinter7);
            //HighLevelItemList.Add(Item.Ramp2Icon);
            HighLevelItemList.Add(Item.DiamantiumLeggings);
            HighLevelItemList.Add(Item.ResplendentMixture);
            HighLevelItemList.Add(Item.LeatherHelmet);
            HighLevelItemList.Add(Item.SteelBody);
            HighLevelItemList.Add(Item.TitaniumBody);
            HighLevelItemList.Add(Item.LeatherBody);
            HighLevelItemList.Add(Item.IronBody);
            HighLevelItemList.Add(Item.IronLeggings);
            HighLevelItemList.Add(Item.TitaniumLeggings);
            HighLevelItemList.Add(Item.EitrPotion);
            HighLevelItemList.Add(Item.AestusFlask);
            HighLevelItemList.Add(Item.Fennel);
            HighLevelItemList.Add(Item.WaterTalisman);
            HighLevelItemList.Add(Item.Dough);
            HighLevelItemList.Add(Item.TitaniumBoots);
            HighLevelItemList.Add(Item.Corn);
            HighLevelItemList.Add(Item.Basil);
            HighLevelItemList.Add(Item.EarthPotion);
            HighLevelItemList.Add(Item.VolatileConcoction);
            HighLevelItemList.Add(Item.Bread);
            HighLevelItemList.Add(Item.SkillFarming);
            HighLevelItemList.Add(Item.PotatoPie);
            HighLevelItemList.Add(Item.Butter);
            HighLevelItemList.Add(Item.TrollHideHelmet);
            HighLevelItemList.Add(Item.Mint);
            HighLevelItemList.Add(Item.LeatherGauntlets);
            //HighLevelItemList.Add(Item.ChatIcon);
            HighLevelItemList.Add(Item.Coif);
            //HighLevelItemList.Add(Item.Splinter8);
            HighLevelItemList.Add(Item.Camera);
            HighLevelItemList.Add(Item.DiamantiumHelmet);
            HighLevelItemList.Add(Item.TrollHideLeggings);
            HighLevelItemList.Add(Item.TenLeagueBoots);
            HighLevelItemList.Add(Item.TrollHideBoots);
            HighLevelItemList.Add(Item.TrollHideGauntlets);
            HighLevelItemList.Add(Item.Majoram);
            HighLevelItemList.Add(Item.Clipboard);
            //HighLevelItemList.Add(Item.Sugar);
            HighLevelItemList.Add(Item.TitaniumShield);
            HighLevelItemList.Add(Item.SignIcon);
            HighLevelItemList.Add(Item.Binoculars);
            HighLevelItemList.Add(Item.SkillCooking);
            HighLevelItemList.Add(Item.Cornbread);
            HighLevelItemList.Add(Item.Leather);
            HighLevelItemList.Add(Item.WoodShield);
            //HighLevelItemList.Add(Item.StairsIcon);
            HighLevelItemList.Add(Item.GreenstoneGoldShield);
            //HighLevelItemList.Add(Item.Stairs2Icon);
            //HighLevelItemList.Add(Item.FolderIcon);
            HighLevelItemList.Add(Item.DiamondShield);
            HighLevelItemList.Add(Item.IronShield);
            HighLevelItemList.Add(Item.SteelShield);
            //HighLevelItemList.Add(Item.SkillLooting);
            HighLevelItemList.Add(Item.Cake);
            HighLevelItemList.Add(Item.DiamantiumShield);
            HighLevelItemList.Add(Item.Pizza);
            HighLevelItemList.Add(Item.CowHide);
            HighLevelItemList.Add(Item.Flour);
            HighLevelItemList.Add(Item.TrollHide);
            //HighLevelItemList.Add(Item.CylinderIcon);
            HighLevelItemList.Add(Item.RingMould);
            HighLevelItemList.Add(Item.AmuletMould);
            HighLevelItemList.Add(Item.NecklaceMould);
            HighLevelItemList.Add(Item.Chance);
            //HighLevelItemList.Add(Item.SkyWorld);
            //HighLevelItemList.Add(Item.SpaceWorld);

            //HighLevelItemList.Add(Item.GrenadeTex);

            #endregion
        }

        #endregion

        #region Update

        public override void Update()
        {
            if (!tmPlayer.IsInputEnabled) return;


            try
            {
                switch (State)
                {
                    case GameState.Play:
                        if (!pauseGame)
                        {
                            UpdatePlayState();
                            if (!playerAlive)
                            {
                                UpdateDeathTransition();
                            }
                            if (loadingNewLevel)
                            {
                                UpdateNewLevelTransition();
                            }
                        }
                        break;

                    case GameState.GameOverTransition:
                        messageToPlayerTimer = 0; //turn off player message
                        UpdateGameOverTransitionState();
                        break;
                }
            }
            catch (Exception e)
            {
                Services.ExceptionReporter.ReportExceptionCaught(1, e);
            }
            if (messageToPlayerTimer > 0)
            {
                messageToPlayerTimer--;
            }
            else
            {
                messageToPlayer = string.Empty;
            }

        }

        void UpdatePlayState()
        {
#if DEBUG
            if (autoPlay)
            {
                if (Balls.Count > 0)
                {
                    var diff = Balls[0].CenterPos.X - Paddle.CenterPos.X;
                    if (diff > 0) Paddle.CenterPosX += Math.Min(diff, Paddle.Speed);
                    else if (diff < 0) Paddle.CenterPosX -= Math.Min(-diff, Paddle.Speed);
                }

            }

#endif
            //BALL Movement
            UpdateBalls();

            UpdatePowerUps();

            UpdateBullets();

            ClampPaddles();

            CheckAndEndLevel();
        }

        private void UpdateBullets()
        {
            //loop through all bullets
            for (int i = 0; i < Bullets.Count; i++)
            {
                Bullets[i].Update();
                if (CheckBulletCollision(i))
                {
                    //bulleted collided it needs destroying
                    Bullets.RemoveAt(i);
                    i--;
                }
            }

        }

        private void UpdatePowerUps()
        {
            //loop through all powerups and run their Update
            for (int i = 0; i < PowerUps.Count; i++)
            {
                PowerUps[i].Update();
                if (CheckPowerUpCollision(i))
                {
                    PowerUps.RemoveAt(i);
                    i--;
                }
            }
            powerUpTracker.DecreaseTimers(Services.ElapsedTime);
        }

        private void UpdateBalls()
        {
            ballSpeedTimer += Services.ElapsedTime;
            if (ballSpeedTimer > 15)
            {
                for (int i = 0; i < Balls.Count; i++)
                {
                    Balls[i].IncreaseVelocity();
                }
                ballSpeedTimer = 0;
            }

            //loop though all balls
            for (int i = 0; i < Balls.Count; i++)
            {

                Balls[i].Update(testSlowBall, slowBall, CurveBall);

                if (resetBallVel)
                {
                    Balls[i].ResetVelocityToDefault();
                    if (i == Balls.Count - 1)
                    {
                        //last ball
                        resetBallVel = false;
                    }
                }

                //check ball in play
                if (Balls[i].CenterPos.Y > ScreenSize.Y )
                {
                    //ball fell down below screen
                    Balls.RemoveAt(i);
                    i--;
                    if (Balls.Count <= 0)
                    {
                        lives--;
                        LivesText = "Lives: " + lives;
                        ResetPowerUps();

                        if (lives < 0)
                        {
                            lives = 0;
                            LivesText = "Lives: " + lives;
                            GameOver(true);
                            CheckAndSaveHighScore();
                            return;
                        }
                        else
                        {
                            LifeOverScreen(true);
                            //reset for new ball/round
                            Paddle.ResetPositionToDefault();
                            ballSpeedTimer = 0;
                            //create a new ball
                            SpawnNewBalls(1);
                            ResetAllBalls();

                        }
                    }

                }
                else //ball is in play still check other collisions
                {
                    ClampBall(Balls[i]);
                    CheckPaddleCollision(Balls[i]);
                    CheckBrickCollision(Balls[i]);
                }
            }

        }

        private void CheckAndSaveHighScore()
        {
            if (score > highScoreRandom && useRandomLayout)
            {
                //only change the random score
                highScoreRandom = score;
                highScore.SaveHighScore(highScoreRandom, highScoreItem);
            }
            else if (score > highScore.HighScoreForItemLayout() && !useRandomLayout)
            {
                //only change the item score
                highScoreItem = score;
                highScore.SaveHighScore(highScoreRandom, highScoreItem);
            }

            if (highScore.errorMsg != string.Empty)
            {
                MessagePlayer(120, highScore.errorMsg);
            }
        }

        private void SetBallToDefaultVelocity(PlayerBall Ball)
        {

            if (Ball.CurrentVelocityX > 0)
            {
                Ball.CurrentVelocityX = Ball.DefaultLevelVelocity.X;
            }
            else
            {
                Ball.CurrentVelocityX = -Ball.DefaultLevelVelocity.X;
            }

            if (Ball.CurrentVelocityY > 0)
            {
                Ball.CurrentVelocityY = Ball.DefaultLevelVelocity.Y;
            }
            else
            {
                Ball.CurrentVelocityY = -Ball.DefaultLevelVelocity.Y;
            }


            resetBallVel = false;
        }

        private void NewGame()
        {
            SetStartStats();

            Balls.Clear();
            SpawnNewBalls(1);
            ResetAllBalls();

            Paddle.ResetPositionToDefault();
            ResetPowerUps();
            GenerateLevel(currentLevel);
        }

        private void SetStartStats()
        {
            ballSpeedTimer = 0;
            lives = 2;
            score = 0;
            ScoreText1 = "Score: 0";
            LivesText = "Lives: " + lives;
            currentLevel = 1;

            livesAwarded = 1;
            CalculateNextLifeScore();

        }

        public override void StartGame()
        {
            if (State != GameState.Play)
            {
                if (Credits > 0)
                {
                    NewGame();
                    ChangeCredits(-1);
                    State = GameState.Play;
                }
            }
            else
            {
                GameOver(false);
            }
        }

        void GameOver(bool transition)
        {
            if (transition)
            {
                State = GameState.GameOverTransition;
                gameOverTransitionTimer = 3;
            }
            else
            {
                State = GameState.GameOver;

            }
        }

        void LifeOverScreen(bool transition)
        {
            if (transition)
            {
                allowPlayerMovement = false;
                playerAlive = false;
                deathTransitionTimer = 1.5f;
            }
            else
            {
                allowPlayerMovement = true;
                playerAlive = true;
            }
        }

        void NewLevelLoadingScreen(bool transition)
        {
            if (transition)
            {
                loadingNewLevel = true;
                allowPlayerMovement = false;
                newLevelTransitionTimer = 1.5f;
            }
            else
            {
                allowPlayerMovement = true;
                loadingNewLevel = false;
            }
        }

        void UpdateGameOverTransitionState()
        {
            gameOverTransitionTimer -= Services.ElapsedTime;
            if (gameOverTransitionTimer > 0)
            {
            }
            else
            {
                GameOver(false);
            }
        }

        void UpdateDeathTransition()
        {
            deathTransitionTimer -= Services.ElapsedTime;
            if (deathTransitionTimer > 0)
            {
            }
            else
            {
                LifeOverScreen(false);
            }
        }

        void UpdateNewLevelTransition()
        {
            newLevelTransitionTimer -= Services.ElapsedTime;
            if (newLevelTransitionTimer > 0)
            {
            }
            else
            {
                NewLevelLoadingScreen(false);
            }
        }

        public void SpawnNewBalls(int amount)
        {
            for (int i = amount; i > 0; i--)
            {
                PlayerBall newBall = new PlayerBall(ScreenSize, GlobalScale, Paddle);
                newBall.UpdateVelocity(currentLevel);
                newBall.CurrentVelocityX = newBall.CurrentVelocityX + (i * (float)Random.NextDouble());
                newBall.CurrentVelocityY = newBall.CurrentVelocityY + (i * (float)Random.NextDouble());
                newBall.CenterPos.X = Paddle.CenterPos.X;
                newBall.CenterPos.Y = Paddle.CenterPos.Y - newBall.HalfSize - Paddle.HalfHeight;

                Balls.Add(newBall);
            }
        }

        private void ResetAllBalls()
        {
            for (int i = 0; i < Balls.Count; i++)
            {
                Balls[i].ResetVelocityToDefault();
                Balls[i].CenterPos.X = Paddle.CenterPos.X;
                Balls[i].CenterPos.Y = Paddle.CenterPos.Y - Balls[i].HalfSize - Paddle.HalfHeight;
                Balls[i].StuckToPaddle = true;
            }
        }

        bool CheckAndEndLevel()
        {

            if (!BreakableBricksRemain())
            {
                //start next round
                currentLevel += 1;
                GenerateLevel(currentLevel);
                NewLevelLoadingScreen(true);
                Balls.Clear();
                SpawnNewBalls(1);
                ResetAllBalls();
                ResetPowerUps();

                return true;
            }
            return false;
        }

        private bool DamageBrick_IsDestroyed(int i)
        {

            bool destroyed = false;
            if (Bricks[i].breakable)
            {
                AddToScore(1);
                Bricks[i].health -= 1;
                //alter brick color
                Bricks[i].color = SetColorBasedOnHealth(Bricks[i].health);
                //insure the sound is updated
                Bricks[i].SetDefaultItemMimic();

                if (SummonPowerUp())
                {
                    PowerUps.Add(new PowerUp(Bricks[i].pos, GlobalScale));
                }
            }

            if (Bricks[i].health <= 0)
            {
                Bricks.RemoveAt(i);

                destroyed = true;
            }
            return destroyed;
        }

        private void AddToScore(int amount)
        {
            score += amount;
            ScoreText1 = "Score: " + score;
        }

        public bool BeatHighScore()
        {
            if (score >= highScoreItem && !useRandomLayout)
            {
                return true;
            }

            if (score >= highScoreRandom && useRandomLayout)
            {
                return true;
            }

            return false;
        }

        public void AddExtraLife()
        {
            MessagePlayer(90, "Extra Life!");
            lives++;
            LivesText = "Lives: " + lives;
        }

        void ClampPaddles()
        {

            if (Paddle.CenterPos.X - Paddle.HalfWidth <= 0)
            {
                Paddle.CenterPosX = Paddle.HalfWidth;
            }

            if (Paddle.CenterPos.X + Paddle.Rectangle.Width - Paddle.HalfWidth > ScreenSize.X + 2)
            {
                Paddle.CenterPosX = ScreenSize.X + Paddle.HalfWidth - Paddle.Rectangle.Width;

            }


        }

        private void ClampBall(PlayerBall Ball)
        {
            float randomAmount = Random.Next(0, 100) / 100 * Random.Next(-1, 1) == 0 ? 1 : -1; ; //random float between -1 and 1;
            if (Ball.CurrentVelocityX < 0)
            {

                if (Ball.CenterPos.X < Ball.Size) //left side going left
                {
                    Ball.CenterPos.X = Ball.HalfSize;
                    Ball.CenterPos.Y += randomAmount;
                    Ball.CurrentVelocityX = -Ball.CurrentVelocityX;
   
                }
            }
            else if (Ball.CurrentVelocityX > 0)
            {

                if (Ball.CenterPos.X + Ball.Rectangle.Width / 2 > ScreenSize.X)
                {
                    Ball.CenterPos.X = ScreenSize.X - Ball.Size + Paddle.Indent;
                    Ball.CenterPos.Y += randomAmount;
                    Ball.CurrentVelocityX = -Ball.CurrentVelocityX;

                }

            }

            if (Ball.CurrentVelocityY < 0 && Ball.CenterPos.Y < GameScreenY + Ball.HalfSize)
            {
                Ball.CenterPos.Y = GameScreenY + Ball.Size;
                Ball.CenterPos.X += randomAmount;
                Ball.CurrentVelocityY = -Ball.CurrentVelocityY;
            }
        }

       
        #endregion

        #region Visuals
        int ColorToHealth(Color color)
        {
            int health = 1;
            // 10 is unbreakable
            if (color.R > color.G && color.R > color.B)
            {
                health = 1;
            }
            else if (color.R == color.G)
            {
                health = 1;
            }
            else if (color.B > color.R && color.B > color.G)
            {
                health = 2;
            }
            else if (color.G > color.R && color.G > color.B)
            {
                health = 3;

            }
            else if (color.B == color.G)
            {
                health = 3;
            }
            else if (color.R == color.B)
            {
                health = 10;
            }
            else
            {
                //should never happen only here incase it does
                health = 10;
            }
            return health;
        }

        public void MessagePlayer(int frameCount, string msg)
        {
            messageToPlayerTimer = frameCount;
            messageToPlayer = msg;
        }

        Color SetColorBasedOnHealth(int health)
        {


            Color color = Color.White;
            switch (health)
            {
                case 1:
                    color = Color.Red;
                    break;
                case 2:
                    color = Color.Blue;
                    break;
                case 3:
                    color = Color.Green;
                    break;
                case 10:
                    color = Color.SlateGray;
                    break;
                default:
                    color = Color.White;
                    break;
            }


            return color;
        }

        #endregion

        #region Collision

        void CheckPaddleCollision(PlayerBall Ball)
        {

            if (Ball.CenterPos.Y > Paddle.CenterPosY - Paddle.HalfHeight / 2)
            {
                //game.AddNotification("Ball is below paddle");
                Ball.StuckToPaddle = false;
            }

            if (Ball.StuckToPaddle)
            {
                //dont do paddle collision
                return;
            }


            var paddleRect = Paddle.Rectangle;
            var collideRect = TestBallCollision(Ball, paddleRect, false);

            if (collideRect.Width > collideRect.Height)
            {
                if (Ball.CenterPos.X < paddleRect.X + paddleRect.Width * 0.2f)
                {
                    if (Ball.CurrentVelocityX > 0) Ball.CurrentVelocityX = -Ball.CurrentVelocityX;
                    Ball.CurrentVelocityX *= 1.5f;

                    if (stickToPaddle)
                    {
                        Ball.StuckToPaddle = true;
                        Ball.OffSet = Paddle.CenterPos - Ball.CenterPos;
                    }
                }
                else if (Ball.CenterPos.X < paddleRect.X + paddleRect.Width * 0.4f)
                {
                    if (Ball.CurrentVelocityX > 0)
                        Ball.CurrentVelocityX *= 0.8f;
                    else
                        Ball.CurrentVelocityX *= 1.25f;

                    if (stickToPaddle)
                    {
                        Ball.StuckToPaddle = true;
                        Ball.OffSet = Paddle.CenterPos - Ball.CenterPos;
                    }

                }
                else if (Ball.CenterPos.X < paddleRect.X + paddleRect.Width * 0.6f)
                {
                    if (stickToPaddle)
                    {
                        Ball.StuckToPaddle = true;
                        Ball.OffSet = Paddle.CenterPos - Ball.CenterPos;
                    }
                }
                else if (Ball.CenterPos.X < paddleRect.X + paddleRect.Width * 0.8f)
                {
                    if (Ball.CurrentVelocityX < 0)
                        Ball.CurrentVelocityX *= 0.8f;
                    else
                        Ball.CurrentVelocityX *= 1.25f;

                    if (stickToPaddle)
                    {
                        Ball.StuckToPaddle = true;
                        Ball.OffSet = Paddle.CenterPos - Ball.CenterPos;
                    }
                }
                else
                {
                    if (Ball.CurrentVelocityX < 0) Ball.CurrentVelocityX = -Ball.CurrentVelocityX;
                    Ball.CurrentVelocityX *= 1.5f;

                    if (stickToPaddle)
                    {
                        Ball.StuckToPaddle = true;
                        Ball.OffSet = Paddle.CenterPos - Ball.CenterPos;
                    }
                }
                if (RandomBounce)
                {
                    float change = Random.Next(0, 200) / 100;
                    Ball.CurrentVelocityX += Random.Next(-1, 1) == 0 ? change : -change;

                }
            }

        }

        private void CheckBrickCollision(PlayerBall Ball)
        {
            Rectangle ballRect = Ball.Rectangle;

            for (int i = 0; i < Bricks.Count; i++)
            {
                var brick = Bricks[i];

                if (TestBallCollision(Ball, brick.rectangle, true).Width > 0)
                {
                    PlayCollideSound(Bricks[i].ItemToMimic);
                    if (DamageBrick_IsDestroyed(i))
                    {
                        i--;
                    }
                    if (!Bricks[i].breakable && Ball.HeavyBallPass > 0)
                    {
                        //heavy ball break non breakable bricks
                        Bricks[i].breakable = true;
                        Bricks[i].health = 3;
                        Ball.HeavyBallPass = 0;
                        Bricks[i].SetDefaultItemMimic();
                    }

                    if (score >= nextLifeUp)
                    {
                        AddExtraLife();
                        livesAwarded++;
                        CalculateNextLifeScore();
                    }
                    if (CheckAndEndLevel())
                    {
                        break;
                    }
                }
            }

        }

        Rectangle TestBallCollision(PlayerBall Ball, Rectangle targetRect, bool CanBypassCollision)
        {
            Rectangle collideRect = Rectangle.Intersect(Ball.Rectangle, targetRect);
            if (collideRect.Width > 0)
            {

                if (Ball.HeavyBallPass <= 0 || !CanBypassCollision)
                {

                    if (collideRect.Width >= collideRect.Height)
                    {
                        if (collideRect.Y == targetRect.Y)
                        {

                            if (Ball.CurrentVelocityY > 0)
                                Ball.CurrentVelocityY = -(Ball.CurrentVelocityY);
                        }
                        else if (collideRect.Y + collideRect.Height == targetRect.Y + targetRect.Height)
                        {
                            if (Ball.CurrentVelocityY < 0)
                                Ball.CurrentVelocityY = -(Ball.CurrentVelocityY);
                        }
                    }
                    if (collideRect.Width <= collideRect.Height)
                    {
                        if (collideRect.X == targetRect.X)
                        {
                            if (Ball.CurrentVelocityX > 0)
                                Ball.CurrentVelocityX = -(Ball.CurrentVelocityX);
                        }
                        else if (collideRect.X + collideRect.Width == targetRect.X + targetRect.Width)
                        {
                            if (Ball.CurrentVelocityX < 0)
                                Ball.CurrentVelocityX = -(Ball.CurrentVelocityX);
                        }
                    }
                    if (RandomBounce)
                    {
                        float change = Random.Next(0, 200) / 100;
                        Ball.CurrentVelocityX += Random.Next(-1, 1) == 0 ? change : -change;
                        change = Random.Next(0, 200) / 100;
                        Ball.CurrentVelocityY += Random.Next(-1, 1) == 0 ? change : -change;
                    }
                }
                else
                {
                    Ball.HeavyBallPass--;
                    //game.AddNotification("collision bypassed");

                }
            }
            return collideRect;
        }

        bool TestGenericCollision(Rectangle rectOne, Rectangle rectTwo)
        {
            Rectangle collideRect = Rectangle.Intersect(rectOne, rectTwo);
            if (collideRect.Width > 0)
            {
                return true;
            }
            return false;
        }

        private void PlayCollideSound(int mimic)
        {
            Sounds.PlaySound((Item)mimic, ItemSoundType.Hit);
        }

        private bool BreakableBricksRemain()
        {
            bool rtn = true;
            if (Bricks.Count <= 0)
            {
                rtn = false;
            }
            else
            {
                for (int i = 0; i < Bricks.Count; i++)
                {
                    if (Bricks[i].breakable == true)
                    {
                        rtn = true;
                        break;
                    }
                    else
                    {
                        rtn = false;
                    }
                }
            }


            return rtn;
        }

        private void CalculateNextLifeScore()
        {

            //if (useRandomLayout)
            //{
            //    //1st = 50, 2nd = 150, 3rd = 300, 4th = 500.....

            //    nextLifeUp = (int)((Math.Pow(livesAwarded, 2) + livesAwarded) * .5 * 50);
            //}
            //else
            //{
                //500 per life
                nextLifeUp = livesAwarded * 500;
           // }
        }

        #endregion

        #region powerups

        private bool SummonPowerUp()
        {
  
            if (Random.Next(1, 100) > 90)
            {
                return true;
            }
            return false;
        }

        private void ResetPowerUps()
        {
            powerUpTracker.ResetPowerUpTimers();
            PowerUps = new List<PowerUp>();
            PowerUps.Clear();

            bulletsRemain = 0;
            Bullets.Clear();

            ResetHeavyBalCountl();
            //InvertControls = false;
            //RandomBounce = false;
            //CurveBall = false;
        }

        public void IncreaseAllBallSpeed(float amount)
        {
            for (int i = 0; i < Balls.Count; i++)
            {
                Balls[i].CurrentVelocity += new Vector2(Balls[i].CurrentVelocity.X + amount, Balls[i].CurrentVelocity.Y + amount);
            }
        }

        public void AddToHeavyBallCount(int amount)
        {
            for (int i = 0; i < Balls.Count; i++)
            {
                Balls[i].HeavyBallPass += amount;
            }
        }

        void ResetHeavyBalCountl()
        {
            for (int i = 0; i < Balls.Count; i++)
            {
                Balls[i].HeavyBallPass = 0;
            }

        }

        bool CheckPowerUpCollision(int i)
        {
            bool value = false;
            //loop through all powerups
            //for (int i = 0; i < PowerUps.Count; i++)
            //{
            if (TestGenericCollision(Paddle.Rectangle, PowerUps[i].Rectangle) && !PowerUps[i].DestroyMe)
            {
                //TODO might be to much accessing things
                powerUpTracker.PowerUpCollected(PowerUps[i].PowerUpCollected());
                AddToScore(powerUpTracker.ScoreValue(PowerUps[i].Type));
                value = true;
                // MessagePlayer(60, "Collected " + PowerUps[i].PowerUpMessage());
            }

            if (PowerUps[i].Location().Y > ScreenSize.Y)
            {
                //power up fell off screen
                PowerUps[i].SetToDestroy();
                value = true;

            }
            return value;
            // }
        }

        bool CheckBulletCollision(int i)
        {
            bool value = false;
            for (int x = 0; x < Bricks.Count; x++)
            {
                if (TestGenericCollision(Bullets[i].rectangle, Bricks[x].rectangle))
                {
                    //bullet hit a brick
                    if (DamageBrick_IsDestroyed(x))
                    {
                        x--;
                    }
                    value = true;
                }
            }

            if (Bullets[i].pos.Y < 0)
            {//off screen
                value = true;
            }

            CheckAndEndLevel();
            return value;

        }

        #endregion
    }
}





