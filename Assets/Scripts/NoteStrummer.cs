using System;
using System.Collections.Generic;
using UnityEngine;

public class NoteStrummer : MonoBehaviour
{
    public ControllerInput controllerInput;
    public NoteSpawner noteSpawner;
    public ScoreUI scoreUI;
    public NoteTargetsUI targetsUI;
    public NoteMissAudio missAudio;
    [NonSerialized]
    public List<RectTransform> frets = new List<RectTransform>();
    private List<Note> notes = new List<Note>();
    private List<Line> lines = new List<Line>();
    private float noteSpeed = 10f;
    private const int hitboxTop = -750;
    private const int hitboxBottom =-850;
    private bool starPower = false;
    private int lastWhammy = 0;

    // Solo detection first thing when note detected in score area
    private void FixedUpdate() {
        if (frets.Count > 0) {
            for (int i = frets.Count - 1; i >= 0; i--) {
                frets[i].anchoredPosition = new Vector2(0, frets[i].anchoredPosition.y - noteSpeed);
                if (frets[i].anchoredPosition.y < -950) {
                    Destroy(frets[i].gameObject);
                    frets.RemoveAt(i);
                }
            }
        }
        if (notes.Count > 0) {
            for (int i = notes.Count - 1; i >= 0; i--) {
                notes[i].transform.anchoredPosition = new Vector2(0, notes[i].transform.anchoredPosition.y - noteSpeed);
                if (!notes[i].missed && notes[i].transform.anchoredPosition.y < hitboxBottom) {
                    notes[i].missed = true;
                    scoreUI.ResetCombo();
                    missAudio.NoteMiss();
                }
                if (notes[i].transform.anchoredPosition.y < -950) {
                    // Missed note
                    if (notes[i].star) {
                        scoreUI.StarPowerMiss();
                    }
                    Destroy(notes[i].obj);
                    notes.RemoveAt(i);
                }
            }
        }
        if (lines.Count > 0) {
            for (int i = lines.Count - 1; i >= 0; i--) {
                if (lines[i].holding) {
                    lines[i].lineRend.SetPosition(0, new Vector3(0, -800, 0));
                    if (lines[i].star && Mathf.Abs(controllerInput.whammy - lastWhammy) > 40) {
                        lastWhammy = controllerInput.whammy;
                        scoreUI.WhammyStarSustain();
                    }
                } else {
                    if (lines[i].lineRend.GetPosition(0).y > -950) {
                        lines[i].lineRend.SetPosition(0, new Vector3(0, lines[i].lineRend.GetPosition(0).y - noteSpeed, 0));
                    }
                }
                if (lines[i].lineEndMoving) {
                    lines[i].lineRend.SetPosition(1, new Vector3(0, lines[i].lineRend.GetPosition(1).y - noteSpeed, 0));
                    if (lines[i].holding && lines[i].lineRend.GetPosition(1).y < -800) {
                        // Calculate line score here (completed)
                        scoreUI.ScoreSustain(lines[i].holdTick, noteSpawner.GetTick());
                        targetsUI.LowerTarget(lines[i].fret);
                        Destroy(lines[i].obj);
                        lines.RemoveAt(i);
                    } else if (lines[i].lineRend.GetPosition(1).y < -950) {
                        Destroy(lines[i].obj);
                        lines.RemoveAt(i);
                    }
                }
            }
        }
    }

    public void StrumEvent(bool[] heldFrets, bool tapOnly) {
        // Chords can't be strummed individually, and other held freys are allowed (clone hero disallows this if lower frets held though)
        Dictionary<int, List<int>> tickPointers = new Dictionary<int, List<int>>();
        for (int i = 0; i < notes.Count; i++) {
            if ((notes[i].transform.anchoredPosition.y <= hitboxTop && notes[i].transform.anchoredPosition.y >= hitboxBottom) && (!tapOnly || notes[i].tap)) {
                // Note within hitbox
                if (tickPointers.ContainsKey(notes[i].tick)) {
                    tickPointers[notes[i].tick].Add(i);
                } else {
                    tickPointers[notes[i].tick] = new List<int> { i };
                }
            }
        }
        foreach (KeyValuePair<int, List<int>> entry in tickPointers) {
            bool allHeld = true;
            for (int e = 0; e < entry.Value.Count; e++) {
                if (notes[entry.Value[e]].fret == 7) {
                    allHeld = !(heldFrets[0] && heldFrets[1] && heldFrets[2] && heldFrets[3] && heldFrets[4]);
                } else {
                    if (!heldFrets[notes[entry.Value[e]].fret]) {
                        allHeld = false;
                    }
                }
            }
            if (allHeld) {
                // Notes strummed successfully
                for (int e = 0; e < entry.Value.Count; e++) {
                    // Start line hold if needed
                    if (notes[entry.Value[e]].line != null) {
                        notes[entry.Value[e]].line.holding = true;
                        notes[entry.Value[e]].line.holdTick = noteSpawner.GetTick();
                        notes[entry.Value[e]].line.ui.SetHolding(true);
                        targetsUI.LiftTarget(notes[entry.Value[e]].fret, false);
                    } else {
                        targetsUI.LiftTarget(notes[entry.Value[e]].fret, true);
                    }
                    // Calculate note score here
                    scoreUI.ScoreNote(notes[entry.Value[e]].star);
                    if (starPower) {
                        lastWhammy = controllerInput.whammy;
                    }
                    // Remove note
                    Destroy(notes[entry.Value[e]].obj);
                    notes[entry.Value[e]] = null;
                }
            }
        }
        for (int i = 0; i < tickPointers.Count; i++) {
            
        }
        // Remove null notes from notes list
        notes.RemoveAll(x => x == null);
    }

    public void ReleaseEvent(bool[] releasedFrets) {
        // Go through all lines to see any being held
        foreach (Line line in lines) {
            if (line.fret != 7 && line.holding && releasedFrets[line.fret]) {
                line.holding = false;
                line.ui.SetHolding(false);
                // Calculate line score here (semi-completed)
                scoreUI.ScoreSustain(line.holdTick, noteSpawner.GetTick());
                targetsUI.LowerTarget(line.fret);
            }
        }
    }

    public void SetNoteSpeed(float newValue) {
        noteSpeed = newValue;
    }

    public void ClearAll() {
        for (int i = frets.Count - 1; i >= 0; i--) {
            Destroy(frets[i].gameObject);
            frets.RemoveAt(i);
        }
        for (int i = notes.Count - 1; i >= 0; i--) {
            Destroy(notes[i].obj);
            notes.RemoveAt(i);
        }
        for (int i = lines.Count - 1; i >= 0; i--) {
            Destroy(lines[i].obj);
            lines.RemoveAt(i);
        }
        starPower = false;
    }

    public void AddNote(Note note) {
        note.ui.SetCyanState(starPower);
        notes.Add(note);
    }

    public void AddLine(Line line) {
        line.ui.SetCyanState(starPower);
        lines.Add(line);
    }

    public void SetStarState(bool isOn) {
        starPower = isOn;
        // Set all to star or not
        for (int i = notes.Count - 1; i >= 0; i--) {
            notes[i].ui.SetCyanState(isOn);
        }
        for (int i = lines.Count - 1; i >= 0; i--) {
            lines[i].ui.SetCyanState(isOn);
        }
    }
}