using System.Collections.Generic;

namespace Kazedan.Construct
{
    public class NoteManager
    {
        public readonly Queue<Event> Backlog = new Queue<Event>();
        public readonly List<Note> Notes = new List<Note>();
        public readonly Note[,] LastPlayed = new Note[16, 128];

        public const int ReturnToFancyDelay = 3000;
        public const int ForcedFastThreshold = 3000;

        public bool UserEnabledFancy = false;
        public bool RenderFancy = false;

        public void Reset()
        {
            lock (Notes)
            {
                Notes.Clear();
            }
            Backlog.Clear();
        }
    }
}
