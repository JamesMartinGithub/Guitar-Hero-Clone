using UnityEngine;

public class LineUI : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Color[] fretColours;
    public int fret;
    private bool cyan = false;

    public void SetFret(int f) {
        fret = f;
        lineRenderer.startColor = ChangeColourBrightness(fretColours[fret], -0.3f);
        lineRenderer.endColor = ChangeColourBrightness(fretColours[fret], -0.3f);
        if (fret == 7) {
            GetComponent<RectTransform>().localScale = new Vector3(-414.8f, 1f, -0.7999644f);
        }
    }

    public void SetHolding(bool isHolding) {
        if (isHolding) {
            lineRenderer.startColor =  ChangeColourBrightness(fretColours[cyan ? 6 : fret], +0.3f);
            lineRenderer.endColor = ChangeColourBrightness(fretColours[cyan ? 6 : fret], +0.3f);
        } else {
            lineRenderer.startColor = fretColours[5];
            lineRenderer.endColor = fretColours[5];
        }
    }

    public void SetCyanState(bool isOn) {
        cyan = isOn;
        lineRenderer.startColor = ChangeColourBrightness(fretColours[cyan ? 6 : fret], -0.3f);
        lineRenderer.endColor = ChangeColourBrightness(fretColours[cyan ? 6 : fret], -0.3f);
        
    }

    private Color ChangeColourBrightness(Color col, float change) {
        return new Color(Mathf.Clamp(col.r + change, 0, 1), Mathf.Clamp(col.g + change, 0, 1), Mathf.Clamp(col.b + change, 0, 1), 1);
    }
}