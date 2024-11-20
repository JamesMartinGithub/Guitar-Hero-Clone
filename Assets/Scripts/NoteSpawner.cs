using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteSpawner : MonoBehaviour
{
    bool done = true;
    public Controller controller;
    public NoteStrummer noteStrummer;
    public ScoreUI scoreUI;
    public Image progressBar;
    private Song song;
    private bool spawning = false;
    //
    bool hopo = false;
    bool tap = false;
    bool star = false;
    Solo solo = Solo.NONE;
    // Frets
    public Transform fretboardOrigin;
    public GameObject fretPrefab;
    // Notes
    public Transform[] noteOrigins;
    public Transform lineParent;
    public GameObject[] notePrefabs;
    public GameObject linePrefab;
    // Timing
    private double startTime;
    private float staticStartTime;
    private double time;
    private float timeFromStaticStart;
    private int tickRate = 192; // 192
    private double bps = 128 / (double)60;
    private int timeSig = 4;
    private int tick = 0;
    private int lastTempoTick = 0;

    void Update()
    {
        if (spawning) {
            if (!done) {
                //systemStartTime = System.DateTime.Now.Ticks;
                startTime = Time.timeAsDouble;
                lastTempoTick = 0;
                done = true;
            }
            time = Time.timeAsDouble - startTime;
            timeFromStaticStart = Time.time - staticStartTime;
            // Check if song finished
            if (timeFromStaticStart > song.songLength) {
                controller.EndSong();
            } else {
                progressBar.fillAmount = Mathf.Clamp01(timeFromStaticStart / song.songLength);
                int latestTick = (int)(bps * tickRate * time) + lastTempoTick; // When changing tempo, set startTime to currenttime, and set lastTempoTick to tick
                while (tick < latestTick) {
                    // Individual tick action here
                    if (song.events.ContainsKey(tick)) {
                        List<Note> noteList = new List<Note>();
                        hopo = false;
                        tap = false;
                        solo = Solo.NONE;
                        foreach (Events.Event e in song.events[tick]) {
                            if (e is Events.NoteEvent) {
                                Events.NoteEvent n = (Events.NoteEvent)e;
                                if ((n.type >= 0 && n.type <= 4) || n.type == 7) {
                                    // Note
                                    Note note = new Note();
                                    if (n.type == 7) {
                                        GameObject noteObj = Instantiate(notePrefabs[1], noteOrigins[2].position, noteOrigins[2].rotation, noteOrigins[2]);
                                        note.obj = noteObj;
                                        note.transform = noteObj.transform.GetComponent<RectTransform>();
                                        noteList.Add(note);
                                    } else {
                                        GameObject noteObj = Instantiate(notePrefabs[0], noteOrigins[n.type].position, noteOrigins[n.type].rotation, noteOrigins[n.type]);
                                        note.obj = noteObj;
                                        note.transform = noteObj.transform.GetComponent<RectTransform>();
                                        noteList.Add(note);
                                    }
                                    note.fret = n.type;
                                    note.ui = note.obj.GetComponent<NoteUI>();
                                    note.ui.SetFret(n.type);
                                    note.tick = tick;
                                    if (n.length > 0) {
                                        // Sustain note, line required
                                        GameObject lineObj;
                                        if (n.type == 7) {
                                            lineObj = Instantiate(linePrefab, noteOrigins[2].position, noteOrigins[2].rotation, lineParent);
                                        } else {
                                            lineObj = Instantiate(linePrefab, noteOrigins[n.type].position, noteOrigins[n.type].rotation, lineParent);
                                        }
                                        Line line = new Line();
                                        line.fret = n.type;
                                        line.ui = lineObj.GetComponent<LineUI>();
                                        line.ui.SetFret(n.type);
                                        line.transform = lineObj.GetComponent<RectTransform>();
                                        line.lineRend = lineObj.GetComponent<LineRenderer>();
                                        line.obj = lineObj;
                                        noteStrummer.AddLine(line);
                                        note.line = line;
                                        // Add LineEnd event
                                        Events.LineEndEvent lineEvent = new Events.LineEndEvent();
                                        lineEvent.tick = tick + n.length;
                                        lineEvent.line = line;
                                        if (song.events.ContainsKey(lineEvent.tick)) {
                                            song.events[lineEvent.tick].Add(lineEvent);
                                        } else {
                                            List<Events.Event> events = new List<Events.Event> { lineEvent };
                                            song.events.Add(lineEvent.tick, events);
                                        }
                                    }
                                } else {
                                    // Note Modifier
                                    switch (n.type) {
                                        case 5:
                                            hopo = true;
                                            break;
                                        case 6:
                                            tap = true;
                                            break;
                                    }
                                }
                            } else if (e is Events.TempoEvent) {
                                Events.TempoEvent t = (Events.TempoEvent)e;
                                bps = t.tempo / (double)60;
                                startTime = Time.timeAsDouble;
                                lastTempoTick = tick;
                            } else if (e is Events.TimeSignatureEvent) {
                                Events.TimeSignatureEvent ts = (Events.TimeSignatureEvent)e;
                                timeSig = ts.numerator;
                            } else if (e is Events.LineEndEvent) {
                                Events.LineEndEvent l = (Events.LineEndEvent)e;
                                l.line.lineEndMoving = true;
                            } else if (e is Events.SpecialEvent) {
                                Events.SpecialEvent s = (Events.SpecialEvent)e;
                                if (s.type == 2) {
                                    // Star power start
                                    star = true;
                                    // Add starpower end event
                                    Events.SpecialEvent starEndEvent = new Events.SpecialEvent();
                                    starEndEvent.tick = tick + s.length;
                                    starEndEvent.type = 3; 
                                    if (song.events.ContainsKey(starEndEvent.tick)) {
                                        song.events[starEndEvent.tick].Add(starEndEvent);
                                    } else {
                                        List<Events.Event> events = new List<Events.Event> { starEndEvent };
                                        song.events.Add(starEndEvent.tick, events);
                                    }
                                } else if (s.type == 3) {
                                    // Star power end
                                    star = false;
                                }
                            } else if (e is Events.SoloEvent) {
                                Events.SoloEvent so = (Events.SoloEvent)e;
                                if (so.start) {
                                    solo = Solo.START;
                                } else {
                                    solo = Solo.END;
                                }
                            }
                        }
                        foreach (Note note in noteList) {
                            if (hopo) {
                                note.hopo = true;
                            }
                            if (tap) {
                                note.tap = true;
                                note.ui.MakeTap();
                            }
                            if (star && note.fret != 7) {
                                note.star = true;
                                note.ui.MakeStar();
                                if (note.line != null) {
                                    note.line.star = true;
                                }
                            }
                            if (solo == Solo.START) {
                                note.solo = Solo.START;
                            } else if (solo == Solo.END) {
                                note.solo = Solo.END;
                            }
                            noteStrummer.AddNote(note);
                        }
                    }
                    // Spawn beat fret
                    if (tick % (tickRate) == 0) {
                        noteStrummer.frets.Add(Instantiate(fretPrefab, fretboardOrigin.position, fretboardOrigin.rotation, fretboardOrigin).GetComponent<RectTransform>());
                    }
                    tick++;
                }
            }
        }
    }

    public void SetSong(Song s) {
        song = s;
        tickRate = s.resolution;
        bps = 128 / (double)60;
        tick = 0;
        progressBar.fillAmount = 0;
    }

    public void StartSpawning() {
        scoreUI.ResetScore(tickRate);
        star = false;
        done = false;
        spawning = true;
        staticStartTime = Time.time;
    }

    public void Stop() {
        spawning = false;
    }

    public int GetTick() {
        return tick;
    }

    // Returns length of bar (numerator * beats) in seconds
    public float GetBarLength() {
        return (float)bps * timeSig;
    }
}