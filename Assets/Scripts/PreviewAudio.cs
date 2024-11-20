using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class PreviewAudio : MonoBehaviour
{
    public AudioSource aud;
    public float volume;
    private Coroutine coroutine;

    public void StartPreview(string path, float startTime) {
        if (coroutine != null) StopCoroutine(coroutine);
        aud.Stop();
        coroutine = StartCoroutine(PlayAudio(path, startTime));
    }

    public void StopPreview() {
        aud.Stop();
        if (coroutine != null) StopCoroutine(coroutine);
    }

    private IEnumerator PlayAudio(string folder, float startTime) {
        using (UnityWebRequest url = UnityWebRequestMultimedia.GetAudioClip("file://" + folder + "\\song.wav", AudioType.WAV)) {
            yield return url.SendWebRequest();
            aud.clip = DownloadHandlerAudioClip.GetContent(url);
            aud.time = startTime;
            aud.Play();
            for (float f = 0; f < volume; f += 0.05f) {
                aud.volume = f;
                yield return new WaitForSeconds(0.05f);
            }
            aud.volume = volume;
        }
    }
}