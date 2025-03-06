using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ScoreController : MonoBehaviour
{
    public Controller controller;
    private Dictionary<string, int> highscores = new Dictionary<string, int>();
#if UNITY_EDITOR
    private string scoreFilePath = System.Environment.GetEnvironmentVariable("USERPROFILE") + "\\Desktop\\Songs\\Highscores.txt";
    
#else
    private string scoreFilePath = AppDomain.CurrentDomain.BaseDirectory + "\\Highscores.txt";
#endif

    private void Start() {
        // Load highscores from scoreFile
        if (File.Exists(scoreFilePath)) {
            StreamReader reader = new StreamReader(scoreFilePath);
            string line = reader.ReadLine();
            while (line != null) {
                string[] data = line.Split('*');
                highscores[data[0]] = int.Parse(data[1]);
                line = reader.ReadLine();
            }
            reader.Close();
            controller.ShowInitialScores();
        } else {
            FileStream newFile = File.Create(scoreFilePath);
            newFile.Close();
        }
    }

    public void UpdateScore(int newScore) {
        string song = controller.GetCurrentSongFolder();
        if (highscores.ContainsKey(song)) {
            // Song has highscore
            if (highscores[song] < newScore) {
                // New highscore is greater
                highscores[song] = newScore;
                // Update score in file
                string[] allLines = File.ReadAllLines(scoreFilePath);
                for (int i = 0; i < allLines.Length; i++) {
                    string[] data = allLines[i].Split('*');
                    if (data[0] == song) {
                        allLines[i] = song + "*" + newScore.ToString();
                    }
                }
                File.WriteAllLines(scoreFilePath, allLines);
            }
        } else {
            // Song has no highscore
            highscores[song] = newScore;
            // Save score to scoreFile
            StreamWriter writer = new StreamWriter(scoreFilePath, true);
            writer.WriteLine(song + "*" + newScore.ToString());
            writer.Close();
        }
    }

    public int GetScore(string song) {
        if (highscores.ContainsKey(song)) {
            return highscores[song];
        } else {
            return 0;
        }
    }

    public int GetHighscore() {
        return GetScore(controller.GetCurrentSongFolder());
    }
}