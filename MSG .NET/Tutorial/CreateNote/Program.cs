using System;
using Independentsoft.Msg;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Message note = new Message();

            note.MessageClass = "IPM.StickyNote";
            note.Subject = "Test";
            note.Body = "Test";
            note.NoteColor = NoteColor.Green;
            note.NoteTop = 200;
            note.NoteLeft = 300;
            note.NoteHeight = 200;
            note.NoteWidth = 250;

            note.Save("c:\\temp\\note.msg", true);
        }
    }
}
