using StudioForge.Engine.Core;
using System;
using System.IO;
using StudioForge.TotalMiner;
using Microsoft.Xna.Framework.Graphics;
using StudioForge.Engine;
using Microsoft.Xna.Framework;

namespace RandomBricksArcade
{
    class HighScore
    {

        string fileName = FileSystem.RootPath + BrickbreakerMod.Path + "HighScore.dat";


        public void SaveHighScore(int score)
        {

            using (StreamWriter outputFile = new StreamWriter(fileName))
            {
                
                    outputFile.WriteLine(score.ToString());
            }
        }

        public int LoadHighScore()
        {
            String score = "0";
            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(fileName))
                {
                    // Read the stream to a string
                    score = sr.ReadToEnd();
                    
                }
            }
            catch (Exception e)
            {

                string msg = "Random Brick: The file could not be read: " + e.Message;


            }

            return Int32.Parse(score);
        }


    }


}
