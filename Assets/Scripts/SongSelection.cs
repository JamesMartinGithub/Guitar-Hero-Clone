using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SongSelection : MonoBehaviour
{
    private class SongListItem {
        public GameObject obj;
        public Song song;
        public string folder;

        public SongListItem(GameObject obj, Song song, string folder) {
            this.obj = obj;
            this.song = song;
            this.folder = folder;
        }
    }

    public Controller controller;
    public ScoreController scoreController;
    public NoteMissAudio missAudio;
    public PreviewAudio previewAudio;
    public ControllerInput controllerInput;
    public UnitySerialPort portScript;
    public Slider noteSpeedSlider;
    public Slider audioOffsetSlider;
    public Slider volumeSlider;
    public Toggle missSoundToggle;
    public Toggle keyboardInputToggle;
    public GameObject songPrefab;
    public RectTransform scrollContentArea;
    // Song info
    public RawImage albumImg;
    public Text artistText;
    public Text nameText;
    public Text scoreText;
    public Text descriptionText;
    public GameObject[] difficultyTexts;
    public GameObject[] difficultyHighlights;
    //
    private List<SongListItem> songs = new List<SongListItem>();
    private int selected = 0;
    private int difficulty = 0;

    private void Start() {
        LoadSongs();
    }

    private void LoadSongs() {
        string folder;
#if UNITY_EDITOR
        folder = System.Environment.GetEnvironmentVariable("USERPROFILE") + "\\Desktop\\Songs";
#else
        folder = AppDomain.CurrentDomain.BaseDirectory + "\\Songs";
#endif
        string[] songFolders = Directory.GetDirectories(folder);
        scrollContentArea.sizeDelta = new Vector2 (0, 70 * songFolders.Length);
        for (int i = 0; i < songFolders.Length; i++) {
            GameObject songObj = Instantiate(songPrefab, scrollContentArea);
            songObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, (-70 * i) - 35);
            Song song = ChartParser.ParseIni(songFolders[i]);
            songObj.GetComponent<SongListItemUI>().SetInfo(i, song.artist, song.name, song.difficulties, scoreController.GetScore(songFolders[i]));
            songs.Add(new SongListItem(songObj, song, songFolders[i]));
        }
        if (songFolders.Length > 0) {
            SelectSong(0);
        }
    }

    public void SelectSong(int id) {
        selected = id;
        SongListItem songItem = songs[id];
        StartCoroutine(LoadAlbumImage(songItem.folder));
        artistText.text = songItem.song.artist;
        nameText.text = songItem.song.name;
        scoreText.text = scoreController.GetScore(songItem.folder).ToString();
        descriptionText.text = songItem.song.loadingPhrase;
        // Enable relevent difficulty texts
        if (songItem.song.difficulties.expert) {
            difficultyTexts[3].SetActive(true);
            difficulty = 3;
        } else {
            difficultyTexts[3].SetActive(false);
        }
        if (songItem.song.difficulties.hard) {
            difficultyTexts[2].SetActive(true);
            difficulty = 2;
        } else {
            difficultyTexts[2].SetActive(false);
        }
        if (songItem.song.difficulties.medium) {
            difficultyTexts[1].SetActive(true);
            difficulty = 1;
        } else {
            difficultyTexts[1].SetActive(false);
        }
        if (songItem.song.difficulties.easy) {
            difficultyTexts[0].SetActive(true);
            difficulty = 0;
        } else {
            difficultyTexts[0].SetActive(false);
        }
        // Enable relevent difficulty highlights
        SetDifficulty(difficulty);
        // Deselect other songs
        for (int i = 0; i < songs.Count; i++) {
            if (id != i) {
                songs[i].obj.GetComponent<SongListItemUI>().DeselectSong();
            }
        }
        //Play preview audio
        previewAudio.StartPreview(songItem.folder, songItem.song.previewStartTime);
    }

    public void SetDifficulty(int difficulty) {
        this.difficulty = difficulty;
        SetDifficultyHighlights(difficulty);
        controller.SetDifficulty(difficulty);
    }

    public void StartSong() {
        previewAudio.StopPreview();
        controller.StartSong(songs[selected].folder, songs[selected].song);
    }

    public void ChangeAudioOffset() {
        controller.SetAudioOffset(((audioOffsetSlider.maxValue * 2) - (audioOffsetSlider.value + audioOffsetSlider.maxValue)) - audioOffsetSlider.maxValue);
    }

    public void ChangeNoteSpeed() {
        controller.SetNoteSpeed((int)noteSpeedSlider.value);
    }

    public void ChangeVolume() {
        controller.ChangeVolume(Mathf.Log(Mathf.Log10(volumeSlider.value)));
    }

    public void ToggleMissSounds() {
        missAudio.ToggleAudio(missSoundToggle.isOn);
    }

    public void ToggleKeyboardInput() {
        controllerInput.SetKeyboard(keyboardInputToggle.isOn);
        portScript.enabled = !keyboardInputToggle.isOn;
    }

    public void UpdateScoreTexts() {
        scoreText.text = scoreController.GetScore(songs[selected].folder).ToString();
        foreach (SongListItem song in songs) {
            song.obj.GetComponent<SongListItemUI>().SetScore(scoreController.GetScore(song.folder));
        }
    }

    private IEnumerator LoadAlbumImage(string folder) {
        using (UnityWebRequest url = UnityWebRequestTexture.GetTexture("file://" + folder + "\\album.jpg", true)) {
            yield return url.SendWebRequest();
            albumImg.texture = DownloadHandlerTexture.GetContent(url);
        }
    }

    private void SetDifficultyHighlights(int selected) {
        for (int i = 0; i < 4; i++) {
            if (selected == i) {
                difficultyHighlights[i].SetActive(true);
            } else {
                difficultyHighlights[i].SetActive(false);
            }
        }
    }
}