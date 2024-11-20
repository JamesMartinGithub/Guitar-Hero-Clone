using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ChartParser
{
    public static Song ParseIni(string folder) {
        Song song = new Song();
        StreamReader sr = new StreamReader(folder + "\\song.ini");
        String line;
        sr.ReadLine();
        line = sr.ReadLine();
        while (line != null) {
            (string field, string data) = GetFieldData(line);
            switch (field) {
                case "name":
                    song.name = data;
                    break;
                case "artist":
                    song.artist = data;
                    break;
                case "genre":
                    song.genre = data;
                    break;
                case "year":
                    song.year = data;
                    break;
                case "charter":
                    song.charter = data;
                    break;
                case "album":
                    song.genre = data;
                    break;
                case "song_length":
                    song.songLength  = float.Parse(data) / 1000;
                    break;
                case "preview_start_time":
                    song.previewStartTime = float.Parse(data) / 1000;
                    if (song.previewStartTime < 0) {
                        song.previewStartTime = 0;
                    } 
                    break;
                case "loading_phrase":
                    song.loadingPhrase = data;
                    break;
                case "video_start_time":
                    song.videoStartTime = -(float.Parse(data) / 1000);
                    break;
                default:
                    break;
            }
            line = sr.ReadLine();
        }
        sr.Close();
        // Get difficulties
        sr = new StreamReader(folder + "\\notes.chart");
        line = sr.ReadLine();
        while (line != null) {
            switch (line) {
                case "[EasySingle]":
                    song.difficulties.easy = true;
                    break;
                case "[MediumSingle]":
                    song.difficulties.medium = true;
                    break;
                case "[HardSingle]":
                    song.difficulties.hard = true;
                    break;
                case "[ExpertSingle]":
                    song.difficulties.expert = true;
                    break;
                default: 
                    break;
            }
            line = sr.ReadLine();
        }
        return song;
    }

    public static Song ParseChart(Song song, string folder, int difficulty) {
        //folder = "C:\\Users\\Fierce\\Downloads\\Laszlo - Supernova(Wagsii)\\Laszlo - Supernova(Wagsii)";
        StreamReader sr = new StreamReader(folder + "\\notes.chart");
        String line;
        line = sr.ReadLine();
        while (line != null) {
            string category = line.Replace(" ", "");
            // Get category data
            List<string> lines = new List<string>();
            sr.ReadLine();
            line = sr.ReadLine();
            while (line != "}") {
                lines.Add(line.Substring(2));
                line = sr.ReadLine();
            }
            // Send data to category parser
            string s = "test";
            switch (category) {
                case "[Song]":
                    song = ParseSong(song, lines);
                    break;
                case "[SyncTrack]":
                    song = ParseEvents(song, lines);
                    break;
                case "[EasySingle]":
                    if (difficulty == 0) {
                        song = ParseEvents(song, lines);
                    }
                    break;
                case "[MediumSingle]":
                    if (difficulty == 1) {
                        song = ParseEvents(song, lines);
                    }
                    break;
                case "[HardSingle]":
                    if (difficulty == 2) {
                        song = ParseEvents(song, lines);
                    }
                    break;
                case "[ExpertSingle]":
                    if (difficulty == 3) {
                        song = ParseEvents(song, lines);
                    }
                    break;
                default:
                    break;
            }
            line = sr.ReadLine();
        }
        sr.Close();
        return song;
    }

    private static Song ParseSong(Song song, List<string> lines) {
        foreach (string line in lines) {
            (string field, string data) = GetFieldData(line);
            switch (field) {
                /*
                case "Name":
                    song.name = data.Replace("\"", "");
                    break;
                case "Artist":
                    song.artist = data.Replace("\"", "");
                    break;
                case "Charter":
                    song.charter = data.Replace("\"", "");
                    break;
                case "Album":
                    song.album = data.Replace("\"", "");
                    break;
                case "Year":
                    song.year = data.Replace("\"", "").Substring(2);
                    break;
                case "Genre":
                    song.genre = data.Replace("\"", "");
                    break;
                */
                case "Offset":
                    song.offset = float.Parse(data);
                    break;
                case "MusicStream":
                    song.musicFile = data.Replace("\"", "");
                    break;
                case "Resolution":
                    song.resolution = int.Parse(data);
                    break;
                case "undefined":
                    Debug.LogError("Invalid [Song] field: " + line);
                    break;
                default:
                    // Unused field
                    break;
            }
        }
        return song;
    }

    private static Song ParseEvents(Song song, List<string> lines) {
        foreach (string line in lines) {
            (string field, string data) = GetFieldData(line);
            int tick = int.Parse(field);
            if (song.events.ContainsKey(tick)) {
                song.events[tick].Add(ParseEvent(tick, data));
            } else {
                List<Events.Event> events = new List<Events.Event> {
                    ParseEvent(tick, data)
                };
                song.events.Add(tick, events);
            }
        }
        return song;
    }

    private static Events.Event ParseEvent(int tick, string line) {
        string[] data = line.Split(" ");
        switch (data[0]) {
            case "TS":
                Events.TimeSignatureEvent ts = new Events.TimeSignatureEvent();
                ts.tick = tick;
                ts.numerator = int.Parse(data[1]);
                return ts;
            case "B":
                Events.TempoEvent b = new Events.TempoEvent();
                b.tick = tick;
                b.tempo = double.Parse(data[1]) / 1000;
                return b;
            case "N":
                Events.NoteEvent n = new Events.NoteEvent();
                n.tick = tick;
                n.type = int.Parse(data[1]);
                n.length = int.Parse(data[2]);
                return n;
            case "S":
                Events.SpecialEvent s = new Events.SpecialEvent();
                s.tick = tick;
                s.type = int.Parse(data[1]);
                s.length = int.Parse(data[2]);
                return s;
            case "E":
                    Events.SoloEvent so = new Events.SoloEvent();
                    if (data[1] == "solo") {
                        so.start = true;
                    } else if (data[1] == "soloend") {
                        so.start = false;
                    } else {
                        goto default;
                    }
                    return so;
            default:
                Debug.LogError("Invalid event: " + line);
                return null;
        }
    }

    private static (string, string) GetFieldData(string line) {
        string field = "undefined";
        string data = "undefined";
        for (int i = 0; i < line.Length - 1; i++) {
            if (char.IsWhiteSpace(line[i])) {
                field = line.Substring(0, i);
                try {
                    data = line.Substring(i + 3, line.Length - (i + 3));
                    if (data == null) {
                        data = " ";
                    }
                }
                catch {
                    data = " ";
                }
                break;
            }
        }
        return (field, data);
    }
}