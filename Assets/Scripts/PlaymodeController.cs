using UnityEngine;

public class PlaymodeController : MonoBehaviour
{
    public Controller controller;
    public ScoreUI scoreUI;
    public GameObject[] restartHighlights;
    public GameObject[] quitHighlights;
    public GameObject pauseMenu;
    private bool paused = false;

    private void OnEnable() {
        HighlightRestart();
        pauseMenu.SetActive(false);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (!paused) {
                controller.SetSongPauseState(true);
                paused = true;
                pauseMenu.SetActive(true);
            } else {
                controller.SetSongPauseState(false);
                paused = false;
                pauseMenu.SetActive(false);
            }
        }
    }

    public void RestartSong() {
        controller.SetSongPauseState(false);
        paused = false;
        pauseMenu.SetActive(false);
        HighlightRestart();
        controller.RestartSong();
    }

    public void Quit() {
        controller.SetSongPauseState(false);
        paused = false;
        pauseMenu.SetActive(false);
        HighlightRestart();
        scoreUI.SaveScore();
        controller.ExitSong();
    }

    public void SelectPauseButton(int id) {
        switch (id) {
            case 0:
                HighlightRestart();
                break;
            case 1:
                HighlightQuit();
                break;
        }
    }

    private void HighlightRestart() {
        restartHighlights[0].SetActive(false);
        restartHighlights[1].SetActive(true);
        quitHighlights[0].SetActive(true);
        quitHighlights[1].SetActive(false);
    }

    private void HighlightQuit() {
        restartHighlights[0].SetActive(true);
        restartHighlights[1].SetActive(false);
        quitHighlights[0].SetActive(false);
        quitHighlights[1].SetActive(true);
    }
}