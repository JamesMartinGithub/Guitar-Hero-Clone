using UnityEngine;
using UnityEngine.UI;

public class NoteTargetsUI : MonoBehaviour
{
    public ControllerInput controllerInput;
    public Image[] noteTargetGlows;
    public RectTransform[] noteTargetTops;
    public Color[] glowColours;
    private bool[] willFalls = { false, false, false, false, false };

    private void Update() {
        for (int i = 0; i < 5; i++) {
            if (controllerInput.fretsslides[i]) {
                noteTargetGlows[i].color = glowColours[0];
            } else {
                noteTargetGlows[i].color = glowColours[1];
            }
            if (willFalls[i] && noteTargetTops[i].anchoredPosition.y > -61.1f) {
                noteTargetTops[i].anchoredPosition = new Vector2 (0, noteTargetTops[i].anchoredPosition.y - 2f);
                if (noteTargetTops[i].anchoredPosition.y <= -61.1f) {
                    noteTargetTops[i].anchoredPosition = new Vector2(0, -61.1f);
                    willFalls[i] = false;
                }
            }
        }
    }

    public void LiftTarget(int fret, bool willFall) {
        willFalls[fret] = willFall;
        noteTargetTops[fret].anchoredPosition = new Vector2(0, -35f);
    }

    public void LowerTarget(int fret) {
        willFalls[fret] = true;
    }
}