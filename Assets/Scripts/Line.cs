using UnityEngine;

public class Line
{
    public int fret;
    public GameObject obj;
    public LineUI ui;
    public RectTransform transform;
    public bool lineEndMoving = false;
    public LineRenderer lineRend;
    public bool holding = false;
    public int holdTick = 0;
    public bool star = false;
}