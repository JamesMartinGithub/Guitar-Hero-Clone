using UnityEngine;

public class Note
{
    public int fret;
    public GameObject obj;
    public NoteUI ui;
    public RectTransform transform;
    public bool hopo = false;
    public bool tap = false;
    public bool star = false;
    public Line line;
    public int tick;
    public Solo solo = Solo.NONE;
    public bool missed = false;
}