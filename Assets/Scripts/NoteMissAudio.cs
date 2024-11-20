using UnityEngine;

public class NoteMissAudio : MonoBehaviour
{
    public AudioSource aud;
    public AudioClip[] missSounds;
    private bool playable = true;

    public void NoteMiss() {
        if (playable && aud.enabled && missSounds.Length >= 1) {
            aud.clip = missSounds[Random.Range(0, 7)];
            aud.Play();
            playable = false;
            Invoke("ResetPlayable", 0.3f);
        }
    }

    public void ToggleAudio(bool state) {
        aud.enabled = state;
    }

    private void ResetPlayable() {
        playable = true;
    }
}