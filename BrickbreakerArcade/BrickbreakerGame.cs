using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
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

        public GameState State;
        public Point ScreenSize;
        public static PcgRandom Random;
        public string ScoreText1;
        public string LivesText;
        public int GameScreenY; //height of play area
        public List<Brick> Bricks = new List<Brick>();
        
        public HighScore highScore;
        public bool useRandomLayout = true;
        public string messageToPlayer = string.Empty;
        public GameMenu currentMenu;
        public int highScoreRandom = 0;
        public int highScoreItem = 0;

        //AudioManager audioManager;
        float gameOverTransitionTimer;
        int lives;
        int score;
        float playerPaddlePos;
        Vector2 ballPos;
        int ballSize;
        float ballSize2;
        Point paddleSize;
        Vector2 paddleSize2;
        Vector2 ballVel;
        float paddleSpeed;
        float mouseDampen;
        int paddleIndent;
        float velInc;
        int currentLevel;
        int nextLifeUp;
        int livesAwarded;
        bool stickyBall;
        int messageToPlayerTimer = 0;
        Item item; //starting item
        List<Item> LowLevelItemList;
        List<Item> MidLevelItemList;
        List<Item> HighLevelItemList;

       float ballSpeedTimer;
       //IAudioManager audioManager;


        //for testing only
#if DEBUG
        bool autoPlay = false; //TODO set to false for release
#endif
        bool slowBall = false;

#endregion

#region Properties

        public override bool CanDeactivate { get { return State == GameState.GameOver; } }
        public Rectangle PlayerPaddleRect { get { return new Rectangle((int)(playerPaddlePos - paddleSize2.X), ScreenSize.Y - paddleIndent - paddleSize.Y, paddleSize.X, paddleSize.Y); } }
        public Rectangle BallRect { get { return new Rectangle((int)(ballPos.X - ballSize2), (int)(ballPos.Y - ballSize2), ballSize, ballSize); } }

#endregion

#region Initialization

        public BrickbreakerGame(ITMGame game, ITMMap map, ITMPlayer player, GlobalPoint3D point, BlockFace face)
            : base(game, map, player, point, face)
        {
        }

        public override void LoadContent(InitState state)
        {
            base.LoadContent(state);
            //audioManager = new AudioManager(GetService);
            //Notify("Random Brick: Load Content");
            ScreenSize = new Point(renderTarget.Width, renderTarget.Height);
            State = GameState.GameOver;
            Random = new PcgRandom(new Random().Next());
            ballSize = 6;
            ballSize2 = ballSize * 0.5f;
            paddleSize = new Point(32, 8);
            paddleSize2 = new Vector2(paddleSize.X * 0.5f, paddleSize.Y * 0.5f);
            paddleSpeed = 4;
            mouseDampen = 4;
            paddleIndent = 4;
            GameScreenY = 16; //top of play area 
            velInc = 0.2f;
            highScore = new HighScore();
            highScoreItem = highScore.HighScoreItem();
            highScoreRandom = highScore.HighScoreRandom();
            currentMenu = GameMenu.Main;

            CreateRandomItemLists();

            NewGame();

        }

        private void CreateRandomItemLists()
        {
            CreateItemLevelLists();
            RandomizeList.Shuffle<Item>(LowLevelItemList);
            RandomizeList.Shuffle<Item>(MidLevelItemList);
            RandomizeList.Shuffle<Item>(HighLevelItemList);
        }

        private void NewGame()
        {
            // Notify("Random Brick: Bricks " + Bricks.Count);
            Bricks.Clear();
            Bricks.TrimExcess();
            stickyBall = true;
            ballPos = new Vector2(ScreenSize.X / 2, ScreenSize.Y / 2);
            lives = 2;
            score = 0;
            ScoreText1 = "Score: 0";
            LivesText = "Lives: " + lives;
            currentLevel = 1;
            GenerateLevel(currentLevel);
            livesAwarded = 1;
            nextLifeUp = 10;

        }
        private void GenerateLevel(int level)
        {
            ballVel = new Vector2(1 + (currentLevel * velInc), 1 + (currentLevel * velInc));

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
            Bricks = new List<Brick>(); //clear out any left over bricks
            Bricks.TrimExcess();


            if (level < LowLevelItemList.Count)
            {
                item = LowLevelItemList[level];
            }
            else if (level > LowLevelItemList.Count && level < MidLevelItemList.Count)
            {
                //mid level item 
                item = MidLevelItemList[level - LowLevelItemList.Count];
            }
            else if (level > MidLevelItemList.Count && level < HighLevelItemList.Count)
            {
                item = HighLevelItemList[level - LowLevelItemList.Count - MidLevelItemList.Count];
                //high level
            }
            else
            {
                //TODO find a better way to do this when items run out, maybe use the TM logo?
                item = Item.Furnace;
                //ERROR NO ITEM player has passed all the levels WTF?
            }

            //for testing
            //item = Item.Rosemary;
            // MessagePlayer(1800, item.ToString());
            MessagePlayer(90, item.ToString());

            Vector2 startPoint = new Vector2(Brick.width * 2, Brick.height * 2);
            int brickPerRow = 16;
            int numOfRows = 16;

            float curX = startPoint.X;
            float curY = startPoint.Y + GameScreenY;

            Color[] pixelColor = game.TexturePack.GetItemColorData(item);
            int curIndex = 0;

            List<Color> colorList = new List<Color>();

            var size = game.TexturePack.ItemTextureSize();
            var d = size == 32 ? 2 : 1;
            for (int y = 0; y < 16; ++y)
            {
                for (int x = 0; x < 16; ++x)
                {
                    //average the color then add to this point?
                    Color color = pixelColor[x * d + y * d * size];
                    if (color.R < 10 && color.G < 10 && color.B < 10 && color != Color.Transparent)
                    {
                        //color is black and cant see it
                        color = new Color(10, 10, 10);
                    }
                    colorList.Add(color);

                }
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
                        Bricks.Add(new Brick(curX, curY, hp, bo, colorList[curIndex]));
                        //TODO check to make sure it doesnt go out of bounds?

                    }
                    curIndex++;
                    curX += Brick.width;

                }
                curX = startPoint.X;
                curY += Brick.height;
            }



        }

        private void GenerateRandomLayout(int level)
        {
            //origianl code
            Vector2 startPoint = new Vector2(Brick.width, Brick.height * (5 - (currentLevel / 10))); //indent 
            int brickPerRow = (ScreenSize.X - (Brick.width * 2)) / Brick.width;
            int numOfRows = ((ScreenSize.Y / 2) - GameScreenY * 2 - (Brick.height * 2)) / Brick.height;

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

                        Color color = SetColorBasedOnHealth(hp);
                        


                        //////////TESTING only new round loading
                        //if (nextValue > 50) { hp = 10; b = false; }
                        //if (nextValue <= 50) { hp = 1; }
                        ///////////////////////////
                        Bricks.Add(new Brick(curX, curY, hp, b, color));
                        maxBlocks--;
                    }
                    curX += Brick.width;

                }
                curX = startPoint.X;
                curY += Brick.height;
            }
        }

        public override void StartGame()
        {
            if (State != GameState.Play)
            {
                if (Credits > 0)
                {
                    NewGame();
                    // Notify("Random Brick: Start Game");
                    ChangeCredits(-1);
                    playerPaddlePos = ScreenSize.X / 2;
                    ballPos = new Vector2(ScreenSize.X / 2, ScreenSize.Y / 2);
                    ballVel = new Vector2(1 + (currentLevel * velInc), 1 + (currentLevel * velInc)); ;
                    //pauseTime = 5;
                    CalculateNextLifeScore();
                    State = GameState.Play;
                }
            }
            else
            {
                GameOver(false);
            }
        }

        public void GameOver(bool transition)
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

#endregion

#region Input

        public override bool HandleInput()
        {
            if (State == GameState.Play)
            {
                var right = InputManager.GetGamepadRightStick(tmPlayer.PlayerIndex);
                if (right.Y != 0 || right.X != 0)
                {
                    playerPaddlePos += right.X * paddleSpeed;
                }

                var left = InputManager.GetGamepadLeftStick(tmPlayer.PlayerIndex);
                if (left.Y != 0 || left.X != 0)
                {
                    playerPaddlePos += left.X * paddleSpeed;
                }

                var mouse = InputManager.GetMousePosDeltaSmoothed(tmPlayer.PlayerIndex);
                if (mouse.X != 0)
                {
                    playerPaddlePos += mouse.X / mouseDampen;
                }

                if (InputManager.IsKeyPressed(tmPlayer.PlayerIndex, Keys.D))
                {
                    playerPaddlePos += paddleSpeed;
                }
                if (InputManager.IsKeyPressed(tmPlayer.PlayerIndex, Keys.A))
                {
                    playerPaddlePos -= paddleSpeed;
                }
                if (InputManager.IsKeyPressed(tmPlayer.PlayerIndex, Keys.W)
                    || InputManager.IsButtonPressed(tmPlayer.PlayerIndex, Buttons.A)
                    || InputManager.IsMouseButtonPressedNew(tmPlayer.PlayerIndex, MouseButtons.LeftButton))
                {
                    stickyBall = false;
                }

                if (InputManager1.IsInputReleasedNew(tmPlayer.PlayerIndex, GuiInput.ExitScreen))
                {
                    GameOver(false);
                }
#if DEBUG
                if (InputManager.IsKeyReleasedNew(tmPlayer.PlayerIndex, Keys.N))
                {
                    currentLevel += 1;
                    stickyBall = true;
                    ResetBall();
                    GenerateLevel(currentLevel);
                    ballVel = new Vector2(1, 1);
                    return true;
                }

                if (InputManager.IsKeyPressed(tmPlayer.PlayerIndex, Keys.S))
                {
                    slowBall = true;
                    return true;
                }
                else
                    slowBall = false;
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

            return false;
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
                        UpdatePlayState();
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

            if (stickyBall)
            {
                ResetBall();
            }
            else
            {
                ballSpeedTimer += Services.ElapsedTime;
                if (ballSpeedTimer > 25)
                {
                    //game.AddNotification("Ball speed increase", NotifyRecipient.Local);
                    ballVel += new Vector2(velInc, velInc);
                    ballSpeedTimer = 0;
                }

                if (!slowBall)
                {
                    ballPos += ballVel;
                }
                else
                {
                    ballPos += ballVel * 0.05f;
                }
            }

#if DEBUG
            if (autoPlay)
            {
                var diff = ballPos.X - playerPaddlePos;
                if (diff > 0) playerPaddlePos += Math.Min(diff, paddleSpeed);
                else if (diff < 0) playerPaddlePos -= Math.Min(-diff, paddleSpeed);
            }
#endif
            ClampPaddles();


            if (ballPos.Y > ScreenSize.Y)
            {
                //ball fell down below screen
                lives--;
                LivesText = "Lives: " + lives;
                if (lives < 0)
                {
                    lives = 0;
                    LivesText = "Lives: " + lives;
                    GameOver(true);
                    if (score > highScoreRandom && useRandomLayout)
                    {
                        //only change the random score
                        highScoreRandom = score;
                        highScore.SaveHighScore(highScoreRandom, highScoreItem);
                    }
                    else if (score > highScore.HighScoreItem() && !useRandomLayout)
                    {
                        //only change the item score
                        highScoreItem = score;
                        highScore.SaveHighScore(highScoreRandom, highScoreItem);
                    }

                    if (highScore.errorMsg != string.Empty)
                    {
                        MessagePlayer(120, highScore.errorMsg);
                    }

                    return;
                }
                else
                {
                    //reset for next round
                    playerPaddlePos = ScreenSize.X / 2;
                    stickyBall = true;
                    ResetBall();

                }

            }

            else
            {
                //var f = float.MaxValue;
                //if (ballPos.Y > ScreenSize.Y - paddleIndent - paddleSize.Y && ballPos.X > playerPaddlePos - paddleSize2.X && ballPos.X < playerPaddlePos + paddleSize2.X)
                //{
                //    //ball hit paddle
                //    f = (ballPos.X - playerPaddlePos) * 0.1f;
                //}

                CheckPaddleCollision();
                CheckBrickCollision();

                if (ballVel.X < 0)
                {
                    // Left Wall
                    if (ballPos.X < ballSize2)
                    {
                        ballPos.X = ballSize2;
                        ballVel.X = -ballVel.X;
                    }
                }
                else if (ballVel.X > 0)
                {
                    // Right Wall
                    if (ballPos.X > ScreenSize.X - ballSize2)
                    {
                        ballPos.X = ScreenSize.X - ballSize2;
                        ballVel.X = -ballVel.X;
                    }
                }
                if (ballVel.Y < 0 && ballPos.Y < GameScreenY + ballSize2)
                {
                    ballPos.Y = GameScreenY + ballSize2;
                    ballVel.Y = -ballVel.Y;
                }

                //if (f != float.MaxValue)
                //{
                //    //ball hit something reverse direction 
                //    ballVel.Y = -ballVel.Y;
                //    ballVel.X += f;
                //}

            }
        }

        private void ResetBall()
        {
            ballVel = new Vector2(1 + (currentLevel * velInc), 1 + (currentLevel * velInc));
            ballPos.X = playerPaddlePos;
            ballPos.Y = ScreenSize.Y - ballSize2 - ballSize2 - paddleSize2.Y - paddleIndent -1;
            
        }

        void CheckPaddleCollision()
        {
            var paddleRect = PlayerPaddleRect;
            var collideRect = TestBallCollision(paddleRect);

            if (collideRect.Width > collideRect.Height)
            {
                if (ballPos.X < paddleRect.X + paddleRect.Width * 0.1f)
                {
                    if (ballVel.X > 0) ballVel.X = -ballVel.X;
                    ballVel.X *= 1.5f;
                }
                else if (ballPos.X < paddleRect.X + paddleRect.Width * 0.3f)
                {
                    if (ballVel.X > 0)
                        ballVel.X *= 0.8f;
                    else
                        ballVel.X *= 1.25f;
                }
                else if (ballPos.X < paddleRect.X + paddleRect.Width * 0.7f)
                {
                }
                else if (ballPos.X < paddleRect.X + paddleRect.Width * 0.9f)
                {
                    if (ballVel.X < 0)
                        ballVel.X *= 0.8f;
                    else
                        ballVel.X *= 1.25f;
                }
                else
                {
                    if (ballVel.X < 0) ballVel.X = -ballVel.X;
                    ballVel.X *= 1.5f;
                }
            }            
        }

        private void CheckBrickCollision()
        {
            var ballRect = BallRect;

            for (int i = 0; i < Bricks.Count; i++)
            {
                var brick = Bricks[i];

                if (TestBallCollision(brick.rectangle).Width > 0)
                {

                    PlayCollideSound(Bricks[i].health);
                    if (Bricks[i].breakable)
                    {
                        score++;
                        ScoreText1 = "Score: " + score;
                        Bricks[i].health -= 1;
                        //alter brick color
                        Bricks[i].color = SetColorBasedOnHealth(Bricks[i].health);
                    }

                    if (Bricks[i].health <= 0)
                    {
                        //TODO remove after were done with list?
                        Bricks.RemoveAt(i);
                    }

                    if (score >= nextLifeUp)
                    {
                        MessagePlayer(90, "Extra Life!");
                        lives++;
                        livesAwarded++;
                        CalculateNextLifeScore();
                        LivesText = "Lives: " + lives;
                    }

                    if (!BreakableBricksRemain())
                    {
                        //start next round
                        currentLevel += 1;
                        stickyBall = true;
                        ResetBall();
                        GenerateLevel(currentLevel);
                        break;
                    }
                }
            }

        }

        Rectangle TestBallCollision(Rectangle targetRect)
        {
            Rectangle collideRect = Rectangle.Intersect(BallRect, targetRect);
            if (collideRect.Width > 0)
            {
                
                if (collideRect.Width >= collideRect.Height)
                {
                    if (collideRect.Y == targetRect.Y)
                    {
                        if (ballVel.Y > 0)
                            ballVel.Y = -ballVel.Y;
                    }
                    else if (collideRect.Y + collideRect.Height == targetRect.Y + targetRect.Height)
                    {
                        if (ballVel.Y < 0)
                            ballVel.Y = -ballVel.Y;
                    }
                }
                if (collideRect.Width <= collideRect.Height)
                {
                    if (collideRect.X == targetRect.X)
                    {
                        if (ballVel.X > 0)
                            ballVel.X = -ballVel.X;
                    }
                    else if (collideRect.X + collideRect.Width == targetRect.X + targetRect.Width)
                    {
                        if (ballVel.X < 0)
                            ballVel.X = -ballVel.X;
                    }
                }
            }
            return collideRect;
        }

        private void PlayCollideSound(int hp)
        {
            //TODO add sound for each health 1,2,3,10
            switch (hp)
            {
                
                default: Sounds.PlaySound(Item.DiamondPickaxe, ItemSoundType.Hit);break;
            }


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
            //1st = 10, 2nd = 30, 3rd = 60, 4th = 100.....

            nextLifeUp = (int)((Math.Pow(livesAwarded, 2) + livesAwarded) * .5 * 10);
        }

        void ClampPaddles()
        {

            if (playerPaddlePos - paddleSize2.X <= 0)
            {
                playerPaddlePos = paddleSize2.X;
            }

            if (playerPaddlePos + paddleSize2.X >= ScreenSize.X - 2)
            {
                playerPaddlePos = ScreenSize.X - paddleSize2.X - 2;
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
            MidLevelItemList.Add(Item.Splinter4);
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
            MidLevelItemList.Add(Item.RopeIcon);
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
            MidLevelItemList.Add(Item.SkillChopping);
            MidLevelItemList.Add(Item.RawFish);
            MidLevelItemList.Add(Item.Apple);
            MidLevelItemList.Add(Item.CookedFish);
            MidLevelItemList.Add(Item.SwitchIcon);
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
            HighLevelItemList.Add(Item.SpiderSMG);
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
            HighLevelItemList.Add(Item.Splinter6);
            HighLevelItemList.Add(Item.VaporPotion);
            HighLevelItemList.Add(Item.Potato);
            HighLevelItemList.Add(Item.SkillStrength);
            HighLevelItemList.Add(Item.IchorPotion);
            HighLevelItemList.Add(Item.IronHelmet);
            HighLevelItemList.Add(Item.AstroPotion);
            HighLevelItemList.Add(Item.WoodDoor);
            HighLevelItemList.Add(Item.SteelDoor);
            HighLevelItemList.Add(Item.RampIcon);
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
            HighLevelItemList.Add(Item.LockedDoor);
            HighLevelItemList.Add(Item.HalfBlockIcon);
            HighLevelItemList.Add(Item.SteelGauntlets);
            HighLevelItemList.Add(Item.DiamantiumGauntlets);
            HighLevelItemList.Add(Item.SkillCombat);
            HighLevelItemList.Add(Item.HalfBlock2Icon);
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
            HighLevelItemList.Add(Item.SkillBuilding);
            HighLevelItemList.Add(Item.SkillCrafting);
           // HighLevelItemList.Add(Item.Splinter7);
            HighLevelItemList.Add(Item.Ramp2Icon);
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
            HighLevelItemList.Add(Item.ChatIcon);
            HighLevelItemList.Add(Item.Coif);
            HighLevelItemList.Add(Item.Splinter8);
            HighLevelItemList.Add(Item.Camera);
            HighLevelItemList.Add(Item.DiamantiumHelmet);
            HighLevelItemList.Add(Item.TrollHideLeggings);
            HighLevelItemList.Add(Item.TenLeagueBoots);
            HighLevelItemList.Add(Item.TrollHideBoots);
            HighLevelItemList.Add(Item.TrollHideGauntlets);
            HighLevelItemList.Add(Item.Majoram);
            HighLevelItemList.Add(Item.Clipboard);
            HighLevelItemList.Add(Item.Sugar);
            HighLevelItemList.Add(Item.TitaniumShield);
            HighLevelItemList.Add(Item.SignIcon);
            HighLevelItemList.Add(Item.Binoculars);
            HighLevelItemList.Add(Item.SkillCooking);
            HighLevelItemList.Add(Item.Cornbread);
            HighLevelItemList.Add(Item.Leather);
            HighLevelItemList.Add(Item.WoodShield);
            HighLevelItemList.Add(Item.StairsIcon);
            HighLevelItemList.Add(Item.GreenstoneGoldShield);
            HighLevelItemList.Add(Item.Stairs2Icon);
            HighLevelItemList.Add(Item.FolderIcon);
            HighLevelItemList.Add(Item.DiamondShield);
            HighLevelItemList.Add(Item.IronShield);
            HighLevelItemList.Add(Item.SteelShield);
            HighLevelItemList.Add(Item.SkillLooting);
            HighLevelItemList.Add(Item.Cake);
            HighLevelItemList.Add(Item.DiamantiumShield);
            HighLevelItemList.Add(Item.Pizza);
            HighLevelItemList.Add(Item.CowHide);
            HighLevelItemList.Add(Item.Flour);
            HighLevelItemList.Add(Item.TrollHide);
            HighLevelItemList.Add(Item.CylinderIcon);
            HighLevelItemList.Add(Item.RingMould);
            HighLevelItemList.Add(Item.AmuletMould);
            HighLevelItemList.Add(Item.NecklaceMould);
            HighLevelItemList.Add(Item.Chance);
            HighLevelItemList.Add(Item.SkyWorld);
            HighLevelItemList.Add(Item.SpaceWorld);
            HighLevelItemList.Add(Item.NaturalWorld);
            HighLevelItemList.Add(Item.GrenadeTex);

#endregion
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
                    color = Color.Silver;
                    break;
                default:
                    color = Color.White;
                    break;
            }


            return color;
        }


#endregion


#region experminatal





        //int AverageAround(int[][] g, int row, int col)
        //{
        //    //can use this to average the colors around the one were using
        //    int value = ValueIn(g, row - 1, col) + //up 1
        //        ValueIn(g, row, col - 1) + //left 1
        //        ValueIn(g, row + 1, col) + //down 1
        //        ValueIn(g, row, col + 1); //right one
        //    value = value / 4;

        //    return value;
        //}



        //int SumAround(int[][] g, int row, int col)
        //{
        //    //can use this to average the colors around the one were using
        //    return ValueIn(g, row - 1, col) + //up 1
        //        ValueIn(g, row, col - 1) + //left 1
        //        ValueIn(g, row + 1, col) + //down 1
        //        ValueIn(g, row, col + 1); //right one
        //}

        //bool isValid(int[][] g, int row, int col)
        //{
        //    return row >= 0 && row < g.Length && col >= 0 && col < g[0].Length;

        //}

        //int ValueIn(int[][] g, int row, int col)
        //{
        //    if (isValid(g, row, col))
        //        return g[row][col];
        //    else
        //        return 0;
        //}



        //Color LightenColor(Color color)
        //{
        //    float correctionFactor = 1;
        //    float red = (float)color.R;
        //    float green = (float)color.G;
        //    float blue = (float)color.B;


        //    red = (255 - red) * correctionFactor + red;
        //    green = (255 - green) * correctionFactor + green;
        //    blue = (255 - blue) * correctionFactor + blue;



        //    return new Color(color.A, (int)red, (int)green, (int)blue);
        //}

        //Color DarkenColor(Color color)
        //{
        //    float correctionFactor = -1;
        //    float red = (float)color.R;
        //    float green = (float)color.G;
        //    float blue = (float)color.B;


        //    correctionFactor = 1 + correctionFactor;
        //    red *= correctionFactor;
        //    green *= correctionFactor;
        //    blue *= correctionFactor;


        //    return new Color(color.A, (int)red, (int)green, (int)blue);
        //}

#endregion
    }
}
