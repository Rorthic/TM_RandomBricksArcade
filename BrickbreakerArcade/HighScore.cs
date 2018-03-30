using StudioForge.Engine.Core;
using System;
using System.IO;
using StudioForge.TotalMiner;
using Microsoft.Xna.Framework.Graphics;
using StudioForge.Engine;
using Microsoft.Xna.Framework;
using System.Linq;

namespace RandomBricksArcade
{
    class HighScore
    {

        string fileName = FileSystem.RootPath + BrickbreakerMod.Path + "HighScore.dat";
        int highScoreRandom = 0;
        int highScoreItem = 0;
        public string errorMsg = string.Empty;


        public int HighScoreRandom()
        {
            //load score from file then return it
            LoadHighScore();
            return highScoreRandom;
        }

        public int HighScoreItem()
        {
            //load score from file then return it
            LoadHighScore();
            return highScoreItem;
        }

        public void SaveHighScore(int scoreR, int scoreI)
        {

            using (StreamWriter outputFile = new StreamWriter(fileName))
            {
                outputFile.WriteLine("{0}{1}{2}", scoreR, ",", scoreI);
                //outputFile.WriteLine(scoreR.ToString() + "," + scoreI.ToString());
            }
        }

        void LoadHighScore()
        {
            String scores = "0";
            if (System.IO.File.Exists(fileName))
            {
                try
                {   // Open the text file using a stream reader.
                    using (StreamReader sr = new StreamReader(fileName))
                    {
                        // Read the stream to a string
                        scores = sr.ReadToEnd();

                        //need to parse the read lines into scoreI and scoreR as int32
                        string[] commands = scores.Split(',');

                        if (!Int32.TryParse(commands[0], out highScoreRandom) || !Int32.TryParse(commands[1], out highScoreItem))
                        {
                            errorMsg += "Random Brick: Error parsing high score";

                        }

                    }
                }
                catch (Exception e)
                {
                    errorMsg += "Random Brick: The file could not be read: " + e.Message;
                }
            }
            else
            {
                SaveHighScore(0, 0);//create file as it doesnt exist
            }
           // return Int32.Parse(score);
        }


    }


}
