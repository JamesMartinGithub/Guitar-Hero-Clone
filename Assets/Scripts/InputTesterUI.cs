using UnityEngine;
using UnityEngine.UI;

public class InputTesterUI : MonoBehaviour
{
    public ControllerInput input;

    public GameObject[] frets;
    public GameObject[] slides;
    public GameObject[] strummer;
    public Text whammy;
    public Image select;
    private float selectFade = 0;

    private void OnEnable() {
        select.color = new Color(1, 1, 1, 0);
        input.SelectPressed += Select;
    }

    private void OnDisable() {
        input.SelectPressed -= Select;
    }

    void Update() {
        for (int i = 0; i < 5; i++) {
            if (input.frets[i]) {
                if (!frets[i].activeSelf) { frets[i].SetActive(true); }
            } else {
                if (frets[i].activeSelf) { frets[i].SetActive(false); }
            }
            if (input.slides[i]) {
                if (!slides[i].activeSelf) { slides[i].SetActive(true); }
            } else {
                if (slides[i].activeSelf) { slides[i].SetActive(false); }
            }
        }
        switch (input.strum) {
            case 0:
                if (!strummer[0].activeSelf) { strummer[0].SetActive(true); }
                if (strummer[1].activeSelf) { strummer[1].SetActive(false); }
                break;
            case 1:
                if (strummer[0].activeSelf) { strummer[0].SetActive(false); }
                if (strummer[1].activeSelf) { strummer[1].SetActive(false); }
                break;
            case 2:
                if (strummer[0].activeSelf) { strummer[0].SetActive(false); }
                if (!strummer[1].activeSelf) { strummer[1].SetActive(true); }
                break;
        }
        whammy.text = input.whammy.ToString();
        if (selectFade > 0) {
            selectFade -= Time.deltaTime * 3;
            if (selectFade <= 0) {
                select.color = new Color(1, 1, 1, 0);
            } else {
                select.color = new Color(1, 1, 1, selectFade);
            }
        }
    }

    private void Select() {
        select.color = new Color(1, 1, 1, 1);
        selectFade = 1;
    }
}