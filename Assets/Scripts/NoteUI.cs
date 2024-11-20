using UnityEngine;
using UnityEngine.UI;

public class NoteUI : MonoBehaviour
{
    public Image top;
    public Image glow;
    public Color[] fretColours;
    public Sprite starSprite;
    private int fret;

    public void MakeTap() {
        glow.color = new Color(0, 0, 0.1f, 1);
    }

    public void MakeStar() {
        top.sprite = starSprite;
    }

    public void SetCyanState(bool isOn) {
        if (isOn) {
            GetComponent<Image>().color = fretColours[5];
        }
        else {
            if (fret != 7) {
                GetComponent<Image>().color = fretColours[fret];
            } else {
                GetComponent<Image>().color = fretColours[6];
            }
        }

    }

    public void SetFret(int f) {
        fret = f;
        if (f != 7) {
            GetComponent<Image>().color = fretColours[fret];
        } else {
            GetComponent<Image>().color = fretColours[6];
        }
    }
}