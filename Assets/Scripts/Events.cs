public class Events
{
    public class Event
    {
        public int tick;
    }

    public class NoteEvent : Event
    {
        public int type;
        public int length;
    }

    public class TimeSignatureEvent : Event
    {
        public int numerator = 4;
    }

    public class TempoEvent : Event
    {
        public double tempo;
    }

    public class SpecialEvent : Event
    {
        public int type;
        public int length;
    }

    public class LineEndEvent : Event 
    {
        public Line line;
    }

    public class SoloEvent : Event
    {
        public bool start;
    }
}