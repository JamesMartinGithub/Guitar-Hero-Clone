using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

public class Controller : MonoBehaviour
{
    public NoteSpawner noteSpawner;
    public NoteStrummer noteStrummer;
    public SongSelection songSelection;
    public ScoreUI scoreUI;
    public GameObject[] scenes; // 0=InputTest 1=SongSelector 2=Playmode
    public RawImage backgroundImage;
    public VideoPlayer videoPlayer;
    public RenderTexture videoTexture;
    public AudioMixer audioMixer;
    private float timescale = 1;
    private AudioSource aud;
    private float audioStartDelay = 1.6f;
    private float audioOffset = 0;
    private (string, Song) currentSong;
    private bool songPaused = false;
    private int difficulty = 0;

    void Start() {
        aud = GetComponent<AudioSource>();
    }

    public void StartSong(string folder, Song song) {
        currentSong = (folder, song);
        SetScene(2);
        StartCoroutine(SetBackground(folder));
        StartCoroutine(PlaySongAndNotes(folder, song));
    }

    private void Update() {
        if (!songPaused) {
            if (!Application.isFocused) {
                if (timescale != 0) {
                    aud.Pause();
                    videoPlayer.Pause();
                    Time.timeScale = 0;
                    timescale = 0;
                }
            } else {
                if (timescale != 1) {
                    aud.Play();
                    if (videoPlayer.url != null) {
                        videoPlayer.Play();
                    }
                    Time.timeScale = 1;
                    timescale = 1;
                }
            }
        }
    }

    private IEnumerator PlaySongAndNotes(string folder, Song song) {
        using (UnityWebRequest url = UnityWebRequestMultimedia.GetAudioClip("file://" + folder + "\\song.wav", AudioType.WAV)) {
            yield return url.SendWebRequest();
            aud.clip = DownloadHandlerAudioClip.GetContent(url);
            noteSpawner.SetSong(ChartParser.ParseChart(song, folder, difficulty));
            noteSpawner.StartSpawning();
            yield return new WaitForSeconds(audioStartDelay + song.offset + audioOffset);
            aud.Play();
        }
    }

    private IEnumerator SetBackground(string folder) {
        if (File.Exists(folder + "\\video.mp4")) {
            backgroundImage.enabled = true;
            videoPlayer.enabled = true;
            backgroundImage.texture = videoTexture;
            videoPlayer.url = folder + "\\video.mp4";
            yield return new WaitForSeconds(audioStartDelay + currentSong.Item2.videoStartTime - 0.15f + audioOffset); //currentSong.Item2.offset
            videoPlayer.Play();
        } else {
            videoPlayer.enabled = false;
            using (UnityWebRequest url = UnityWebRequestTexture.GetTexture("file://" + folder + "\\background.jpg", true)) {
                yield return url.SendWebRequest();
                try {
                    backgroundImage.texture = DownloadHandlerTexture.GetContent(url);
                    backgroundImage.enabled = true;
                }
                catch {
                    // No background image
                    backgroundImage.texture = null;
                    backgroundImage.enabled = false;
                }
            }
        }
    }

    public void SetNoteSpeed(int newValue) {
        switch (newValue) {
            case 0:
                audioStartDelay = 3.2f;
                noteStrummer.SetNoteSpeed(5f);
                break;
            case 1:
                audioStartDelay = 1.6f;
                noteStrummer.SetNoteSpeed(10f);
                break;
            case 2:
                audioStartDelay = 0.8f;
                noteStrummer.SetNoteSpeed(20f);
                break;
        }
    }

    public void SetAudioOffset(float newValue) {
        audioOffset = newValue;
    }

    public void RestartSong() {
        aud.Stop();
        noteSpawner.Stop();
        noteStrummer.ClearAll();
        scoreUI.SaveScore();
        if (videoPlayer.url != null) {
            videoPlayer.Stop();
        }
        // TODO: set current points to 0
        Invoke("RestartSongDelayed", 1f);
    }

    private void RestartSongDelayed() {
        StartSong(currentSong.Item1, currentSong.Item2);
    }

    public void ExitSong() {
        aud.Stop();
        if (videoPlayer.url != null) {
            videoPlayer.Stop();
        }
        aud.clip = null;
        StopAllCoroutines();
        noteSpawner.Stop();
        scoreUI.SaveScore();
        noteStrummer.ClearAll();
        songSelection.UpdateScoreTexts();
        SetScene(1);
        backgroundImage.texture = null;
        backgroundImage.enabled = false;
    }

    public void EndSong() {
        ExitSong();
    }

    public void InputTester() {
        SetScene(0);
    }

    public void ExitInputTester() {
        SetScene(1);
    }

    public void SetSongPauseState(bool pause) {
        if (pause) {
            aud.Pause();
            videoPlayer.Pause();
            Time.timeScale = 0;
            timescale = 0;
            songPaused = true;
        } else {
            aud.Play();
            if (videoPlayer.url != null) {
                videoPlayer.Play();
            }
            Time.timeScale = 1;
            timescale = 1;
            songPaused = false;
        }
    }

    public void Quit() {
        Application.Quit();
    }

    private void SetScene(int index) {
        for (int i  = 0; i < scenes.Length; i++) {
            if (index == i) {
                scenes[i].SetActive(true);
            } else {
                scenes[i].SetActive(false);
            }
        }
    }

    public void SetDifficulty(int difficulty) {
        this.difficulty = difficulty;
    }

    public void ChangeVolume(float volume) {
        audioMixer.SetFloat("Volume", (volume * 90) - 80); //10 -> -80
    }

    public string GetCurrentSongFolder() {
        return currentSong.Item1;
    }

    public void ShowInitialScores() {
        songSelection.UpdateScoreTexts();
    }
}