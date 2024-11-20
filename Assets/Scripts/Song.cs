using System.Collections.Generic;

public class Song
{
    public string name;
    public string artist;
    public string album;
    public string year;
    public float offset;
    public string genre;
    public string charter;
    public string loadingPhrase;
    public string musicFile;
    public int resolution;
    public float songLength;
    public float previewStartTime;
    public float videoStartTime;
    public Difficulties difficulties = new Difficulties();
    public Dictionary<int, List<Events.Event>> events = new Dictionary<int, List<Events.Event>>();
}