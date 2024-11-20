using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnitySerialPort;

public class ControllerInput : MonoBehaviour
{
    private UnitySerialPort portScript;
    public NoteStrummer noteStrummer;
    private readonly char[] hex = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
    // For Input Debugger
    public bool[] frets = { false, false, false, false, false };
    public bool[] slides = { false, false, false, false, false };
    // For playmode inputs
    private bool[] constFalse = { false, false, false, false, false };
    public bool[] fretsslides = { false, false, false, false , false };
    public int strum = 1; // 0|1|2
    public int whammy = 100; // 0-100
    public delegate void SelectPressedDelegate();
    public event SelectPressedDelegate SelectPressed;
    private bool selected = false;
    private bool[] lastFretsslides = { false, false, false, false, false };
    private int lastStrum = 1;
    private Dictionary<int, int> recentFretslides = new Dictionary<int, int>();
    private const int recencyTime = 5; // 60 would be 1 second
    private bool keyboardEnabled = false;

    private void Start() {
        portScript = UnitySerialPort.Instance;
        UnitySerialPort.SerialDataParseEvent += new SerialDataParseEventHandler(DataRecieved);
    }

    private void Update() {
        if (keyboardEnabled) {
            // Update recent fretslides
            for (int i = 0; i < 5; i++) {
                if (recentFretslides.ContainsKey(i)) {
                    if (recentFretslides[i]-- <= 0) {
                        recentFretslides.Remove(i);
                    }
                }
            }
            constFalse.CopyTo(fretsslides, 0);
            fretsslides[0] = Input.GetKey(KeyCode.Q) ? true : false;
            fretsslides[1] = Input.GetKey(KeyCode.W) ? true : false;
            fretsslides[2] = Input.GetKey(KeyCode.E) ? true : false;
            fretsslides[3] = Input.GetKey(KeyCode.R) ? true : false;
            fretsslides[4] = Input.GetKey(KeyCode.T) ? true : false;
            if (Input.GetKeyDown(KeyCode.Space) && SelectPressed != null) {
                SelectPressed();
            }
            // Check for differences and call relevent events
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow)) {
                noteStrummer.StrumEvent(fretsslides, false);
            } else if (!fretsslides.SequenceEqual(lastFretsslides)) {
                CallTapEvent();
            }
            fretsslides.CopyTo(lastFretsslides, 0);
        }
    }

    private void DataRecieved(string[] Data, string RawData) {
        try {
            if ((RawData[0] == '0' || RawData[0] == '1') && RawData.Length == 15) {
                //print(RawData);
                // Update recent fretslides
                for (int i = 0; i < 5; i++) {
                    if (recentFretslides.ContainsKey(i)) {
                        if (recentFretslides[i]-- <= 0) {
                            recentFretslides.Remove(i);
                        }
                    }
                }
                constFalse.CopyTo(fretsslides, 0);
                // Frets
                for (int i = 0; i < 5; i++) {
                    // Frets
                    if (RawData[i] == '1') {
                        frets[i] = true;
                        fretsslides[i] = true;
                    } else {
                        frets[i] = false;
                    }
                    // Slides
                    if (RawData[i + 5] == '1') {
                        slides[i] = true;
                        fretsslides[i] = true;
                    } else {
                        slides[i] = false;
                    }
                }
                // Strum
                switch (RawData[10]) {
                    case '0':
                        strum = 0;
                        break;
                    case '1':
                        strum = 1;
                        break;
                    case '2':
                        strum = 2;
                        break;
                }
                // Whammy
                whammy = 100 - Mathf.Clamp((int.Parse(RawData.Substring(11, 3), System.Globalization.NumberStyles.HexNumber) / 9) - 10, 0, 100);
                // Select Button
                if (RawData[14] == 'S') {
                    if (!selected) {
                        selected = true;
                        if (SelectPressed != null) {
                            SelectPressed();
                        }
                    }
                } else {
                    if (selected) {
                        selected = false;
                    }
                }
                // Check for differences and call relevent events
                if ((strum == 0 || strum == 2) && (strum != lastStrum)) {
                    noteStrummer.StrumEvent(fretsslides, false);
                } else if (!fretsslides.SequenceEqual(lastFretsslides)) {
                    CallTapEvent();
                }
                fretsslides.CopyTo(lastFretsslides, 0);
                lastStrum = strum;
            }
        }
        catch (System.Exception e) {
            print("PARSE ERROR: " + RawData);
            Debug.LogException(e);
        }
    }

    public void SetKeyboard(bool allowed) {
        keyboardEnabled = allowed;
    }

    private void CallTapEvent() {
        bool[] tapOns = { false, false, false, false, false };
        bool[] letOffs = { false, false, false, false, false };
        bool onsAllFalse = true;
        bool offsAllFalse = true;
        for (int i = 0; i < 5; i++) {
            // XOR inputs and lastinputs to get changes, then AND with inputs to get values just changed to true
            tapOns[i] = (fretsslides[i] ^ lastFretsslides[i]) && fretsslides[i];
            letOffs[i] = (fretsslides[i] ^ lastFretsslides[i]) && !fretsslides[i];
            if (tapOns[i]) {
                onsAllFalse = false;
                if (!recentFretslides.ContainsKey(i)) {
                    // Add to recent dictionary
                    recentFretslides[i] = recencyTime;
                }
            } else {
                if (recentFretslides.ContainsKey(i)) {
                    // Add recent fretslide to taps
                    tapOns[i] = true;
                }
            }
            if (letOffs[i]) {
                offsAllFalse = false;
            }
        }
        if (!onsAllFalse) {
            noteStrummer.StrumEvent(tapOns, true);
        }
        if (!offsAllFalse) {
            noteStrummer.ReleaseEvent(letOffs);
        }
    }
}