using UnityEngine;
using UnityEngine.UI;

public class SongListItemUI : MonoBehaviour
{
    public Text artistText;
    public Text nameText;
    public GameObject[] difficultiesVisualiser;
    public Text scoreText;
    public GameObject highlight;
    private int id;

    public void SetInfo(int id, string artist, string name, Difficulties difficulties, int score) {
        this.id = id;
        artistText.text = artist;
        nameText.text = name;
        if (difficulties.easy) {
            difficultiesVisualiser[0].SetActive(true);
        } else {
            difficultiesVisualiser[0].SetActive(false);
        }
        if (difficulties.medium) {
            difficultiesVisualiser[1].SetActive(true);
        } else {
            difficultiesVisualiser[1].SetActive(false);
        }
        if (difficulties.hard) {
            difficultiesVisualiser[2].SetActive(true);
        } else {
            difficultiesVisualiser[2].SetActive(false);
        }
        if (difficulties.expert) {
            difficultiesVisualiser[3].SetActive(true);
        } else {
            difficultiesVisualiser[3].SetActive(false);
        }
        scoreText.text = score.ToString();
        if (id == 0) {
            highlight.SetActive(true);
        }
    }

    public void SetScore(int score) {
        scoreText.text = score.ToString();
    }

    public void SelectSong() {
        GameObject.Find("Song Selector").GetComponent<SongSelection>().SelectSong(id);
        highlight.SetActive(true);
    }

    public void DeselectSong() {
        highlight.SetActive(false);
    }
}