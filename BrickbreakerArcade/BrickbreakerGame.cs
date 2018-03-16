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

        #endregion

        #region Fields

        public GameState State;
        public Point ScreenSize;
        public PcgRandom Random;
        public string ScoreText1;
        public string LivesText;
        public int GameScreenY; //height of play area
        public List<Brick> Bricks = new List<Brick>();

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
       // float pauseTime;
       // float pauseTimer;
        int paddleIndent;
        float velInc;
        bool autoPlay;
        int currentLevel;
        int nextLifeUp;
        int livesAwarded;
        bool stickyBall;
        public HighScore highScore;
        public bool useRandomBlocks = false;

        #endregion

        #region Properties

        public override bool CanDeactivate { get { return State == GameState.GameOver; } }
        public Rectangle PlayerPaddleRect { get { return new Rectangle((int)(playerPaddlePos - paddleSize2.X ), ScreenSize.Y  - paddleIndent - paddleSize.Y, paddleSize.X, paddleSize.Y); } }
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
            //Notify("Random Brick: Load Content");
            ScreenSize = new Point(renderTarget.Width, renderTarget.Height);
            State = GameState.GameOver;
            Random = new PcgRandom(new Random().Next());
            ballSize = 6;
            ballSize2 = ballSize * 0.5f;
            paddleSize = new Point(32, 8);
            paddleSize2 = new Vector2(paddleSize.X * 0.5f, paddleSize.Y * 0.5f);
            paddleSpeed = 3;
            paddleIndent = 4;
            GameScreenY = 16; //top of play area 
            velInc = 0.2f;
            highScore = new HighScore();
            NewGame();


            autoPlay = true;

        }
        private void NewGame()
        {
           // Notify("Random Brick: Bricks " + Bricks.Count);
            Bricks.Clear();
            Bricks.TrimExcess();
            stickyBall = true;
            ballPos = new Vector2(ScreenSize.X / 2, ScreenSize.Y / 2);
            //pauseTime = 3;
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



            //Vector2 startPoint = new Vector2(Brick.width* 2, Brick.height* 2);
            // int brickPerRow = 16;
            //int numOfRows = 16;
            //int maxBlocks = 265;

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
                    if (maxBlocks > 0)//(nextValue > 50 && maxBlocks > 0)
                    {
                        nextValue = r.Next(0, 100);
                        int h = 1;
                        bool b = true;

                        if (currentLevel >= 5)
                        {
                            if (nextValue >= 50 && nextValue <= 90) { h = 2; }
                        }
                        if (currentLevel >= 10)
                        {
                            if (nextValue > 90 && nextValue <= 98) { h = 3; }
                            if (nextValue == 99) { h = 10; b = false; }
                        }
                        if (currentLevel >= 15)
                        {
                            if (nextValue > 75 && nextValue <= 90) { h = 3; }
                            if (nextValue > 90) { h = 10; b = false; }
                        }

                        //////////TESTING only new round loading
                        //if (nextValue > 50) { h = 10; b = false; }
                        //if (nextValue <= 50) { h = 1; }
                        ///////////////////////////
                        Bricks.Add(new Brick(curX, curY, h, b));
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
            bool result = false;

            if (State == GameState.Play)
            {


                var right = InputManager.GetGamepadRightStick(tmPlayer.PlayerIndex);
                if (right.Y != 0 || right.X != 0)
                {
                    playerPaddlePos -= right.X * paddleSpeed;
                    result = true;
                }

                if (InputManager.IsKeyPressed(tmPlayer.PlayerIndex, Keys.Right))
                {
                    playerPaddlePos += paddleSpeed;
                    result = true;
                }
                if (InputManager.IsKeyPressed(tmPlayer.PlayerIndex, Keys.Left))
                {
                    playerPaddlePos -= paddleSpeed;
                    result = true;
                }
                if (InputManager.IsKeyPressed(tmPlayer.PlayerIndex, Keys.Up) || InputManager.IsButtonPressed(tmPlayer.PlayerIndex, Buttons.A))
                {
                    stickyBall = false;
                    result = true;
                }

                if (InputManager1.IsInputReleasedNew(tmPlayer.PlayerIndex, GuiInput.ExitScreen))
                {
                    GameOver(false);
                    result = true;
                }
            }

            return result ||
                InputManager.IsButtonPressed(tmPlayer.PlayerIndex, Buttons.A) ||
                InputManager.IsButtonPressed(tmPlayer.PlayerIndex, Buttons.X) ||
                InputManager.IsButtonPressed(tmPlayer.PlayerIndex, Buttons.Y);
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
                        UpdateGameOverTransitionState();
                        break;
                }
            }
            catch (Exception e)
            {
                Services.ExceptionReporter.ReportExceptionCaught(1, e);
            }

           
        }

        void UpdatePlayState()
        {
            //if (pauseTime > 0)
            //{
            //    pauseTimer += Services.ElapsedTime;
            //    if (pauseTimer < pauseTime)
            //    {
            //        ClampPaddles();
            //        return;
            //    }
            //    pauseTime = pauseTimer = 0;
            //}

            if (stickyBall)
            {
                ResetBall();
            }
            else
            {
                ballPos += ballVel;
            }

            if (autoPlay)
            {
                var diff = ballPos.X - playerPaddlePos;
                if (diff > 0) playerPaddlePos += Math.Min(diff, paddleSpeed);
                else if (diff < 0) playerPaddlePos -= Math.Min(-diff, paddleSpeed);
            }

            ClampPaddles();


            if (ballPos.Y > ScreenSize.Y)
            {
                //ball fell down below screen
                lives--;
                LivesText = "Lives: " + lives;
                if (lives < 0)
                {
                    lives = 0;
                    LivesText = "Lives: "+ lives;
                    GameOver(true);
                    if(score > highScore.LoadHighScore())
                    {
                        highScore.SaveHighScore(score);
                    }
                    return;
                }
                else
                {
                    //reset for next round
                    playerPaddlePos = ScreenSize.X / 2;
                    stickyBall = true;
                    ResetBall();
                    

                   // pauseTime = 3;
                }

            }

            else
            {
                var f = float.MaxValue;
                if (ballPos.Y > ScreenSize.Y - paddleIndent - paddleSize.Y && ballPos.X > playerPaddlePos - paddleSize2.X && ballPos.X < playerPaddlePos + paddleSize2.X)
                {
                    //ball hit paddle
                    f = (ballPos.X - playerPaddlePos) * 0.1f;
                }
                else if (ballPos.X <= 0 + ballSize2 || ballPos.X >= ScreenSize.X - ballSize2)
                {
                    //Left or Right wall bounce 

                    ballVel.X = -ballVel.X;
                }
                if (ballPos.Y <= GameScreenY + ballSize2)
                {
                    //ball hit top of screen
                    ballVel.Y = -ballVel.Y;
                }

                CheckBrickCollision();

                if (f != float.MaxValue)
                {
                    //ball hit something reverse direction 
                    ballVel.Y = -ballVel.Y;
                    ballVel.X += f;
                }

            }
        }

        private void ResetBall()
        {
            
            //ballPos = new Vector2(ScreenSize.X / 2, ScreenSize.Y / 2);
            ballVel = new Vector2(1 + (currentLevel * velInc), 1 + (currentLevel * velInc));
            ballPos.X = playerPaddlePos;
            ballPos.Y = ScreenSize.Y - ballSize2 -ballSize2 - paddleSize2.Y - paddleIndent ; //think i need paddle indent here

        }

        private void CheckBrickCollision()
        {

            for (int i = 0; i < Bricks.Count; i++)
            {
                float x = Bricks[i].pos.X;
                float y = Bricks[i].pos.Y;
                if (ballPos.X > Bricks[i].pos.X && ballPos.Y > Bricks[i].pos.Y && ballPos.Y < Bricks[i].pos.Y + Brick.height && ballPos.X < Bricks[i].pos.X + Brick.width)//(brick top left corner)
                {

                    if (ballPos.X - ballSize2 <= Bricks[i].pos.X && ballPos.X + ballSize2 >= Bricks[i].pos.X && ballPos.Y > Bricks[i].pos.Y && ballPos.Y < Bricks[i].pos.Y + Brick.height)
                    {
                        //ball hit on left side of brick 
                        ballVel.X = -ballVel.X;
                    }
                    else if (ballPos.X - ballSize2 <= Bricks[i].pos.X + Brick.width && ballPos.X + ballSize2 >= Bricks[i].pos.X + Brick.width && ballPos.Y > Bricks[i].pos.Y && ballPos.Y < Bricks[i].pos.Y + Brick.height)
                    {//hit right side 
                        ballVel.X = -ballVel.X;
                    }

                    if (ballPos.Y - ballSize2 <= Bricks[i].pos.Y && ballPos.Y  + ballSize2 >= Bricks[i].pos.Y && ballPos.X > Bricks[i].pos.X && ballPos.X < Bricks[i].pos.X + Brick.width)
                    { //ball hit top 
                        ballVel.Y = -ballVel.Y;
                    }
                    else if (ballPos.Y - ballSize2 <= Bricks[i].pos.Y + Brick.height  && ballPos.Y + ballSize2 >= Bricks[i].pos.Y + Brick.height && ballPos.X > Bricks[i].pos.X && ballPos.X < Bricks[i].pos.X + Brick.width)
                    { //ball hit bottom 
                          ballVel.Y = -ballVel.Y;
                    }

                    score++;
                    ScoreText1 = "Score: " + score;

                    if (score >= nextLifeUp)
                    {
                        lives++;
                        livesAwarded++;
                        CalculateNextLifeScore();
                        LivesText = "Lives: " + lives;
                    }


                    if (Bricks[i].breakable)
                    {
                        Bricks[i].health -= 1;
                    }
                    if (Bricks[i].health <= 0)
                    {
                        Bricks.RemoveAt(i);
                    }

                    if (!BreakableBricksRemain())
                    {
                        //start next round
                        currentLevel += 1;
                        stickyBall = true;
                        ResetBall();
                        GenerateLevel(currentLevel);
                    }
                    break;

                }
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

       public void Notify(string msg)
        {
            game.AddNotification(msg, NotifyRecipient.Local);

           // PopUp popup = new PopUp(msg, Color.Red, 1.2f, 3);
        }


        #endregion
    }
}
