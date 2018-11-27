using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using AudioSynthesis.Midi;
using AudioSynthesis.Midi.Event;
using AudioSynthesis.Sequencer;
using AudioSynthesis.Synthesis;
using UnityEngine;
using UnityMidi;

//using Kazedan.Graphics;
using Timer = System.Timers.Timer;

namespace Kazedan.Construct
{
    public class MIDISequencer : IDisposable
    {

        
        public int Delay { get; set; } = 1000;
        public bool ShowDebug { get; set; } = true;
        private int LoadingStatus { get; set; } = -1;

        public Bars MidiPlayer;

        private long LastFancyTick { get; set; }
        public bool Stopped { get; private set; } = true;
        public bool Initialized { get; private set; }
        private readonly Stopwatch Stopwatch;

        public string MIDIFile = @"null";

        ////private OutputDevice outDevice;
        ////private Sequence sequence;
        ////private Sequencer sequencer;

        private Timer eventTimer;

        public MIDIKeyboard Keyboard { get; set; }
        public NoteManager NoteManager { get; set; }

       

        public MIDISequencer()
        {
            Keyboard = new MIDIKeyboard();
            NoteManager = new NoteManager();
            Stopwatch = Stopwatch.StartNew();
            Stopwatch.Stop();

        }

        public void Init()
        {
            // Make sure we don't initialize twice and create a disaster
            if (Initialized)
                return;
            Initialized = true;

            // Create timer for event management
            eventTimer = new Timer(10);
            eventTimer.Elapsed += delegate
            {
                lock (NoteManager.Backlog)
                {
                    while (NoteManager.Backlog.Any() && NoteManager.Backlog.First().StartTime <= Stopwatch.ElapsedMilliseconds)
                    {
                        Event ev = NoteManager.Backlog.Dequeue();
                        ev.Method();
                    }
                }
            };

            LoadingStatus = 0;
            // Create handles to MIDI devices
            MidiPlayer.synthesizer.MidiEvent += PlayerHandler;



            //MidiPlayer.Awake();

            //var music = MidiMusic.Read(System.IO.File.OpenRead("mysong.mid"));
            //output.Send(new byte[] { 0xC0, GeneralMidi.Instruments.AcousticGrandPiano }, 0, 2, 0);
            //player = new MidiPlayer(new MidiMusic(), output);
            LoadingStatus = -1;

            //player.EventReceived += PlayerHandler;

            //outDevice = new OutputDevice(0);    // You might want to change this!
            //sequencer = new Sequencer();
            //sequence = new Sequence();
            //  RendererManager = new RendererManagerManager(NoteManager, Keyboard, sequence, sequencer);

            // Set custom event handlers for sequencer


            // BUG A bug in the Sanford.Multimedia.Midi library prevents certain SysEx messages from playing
            // Disabling SysEx messages completely solves this issue
            //sequencer.SysExMessagePlayed += delegate (object o, SysExMessageEventArgs args)
            //{
            //    lock (NoteManager.Backlog)
            //        NoteManager.Backlog.Enqueue(new Event(() => { try { outDevice.Send(args.Message); } catch { } }, Stopwatch.ElapsedMilliseconds, Delay));
            //};
            //sequencer.Chased += delegate (object o, ChasedEventArgs args)
            //{
            //    foreach (ChannelMessage message in args.Messages)
            //        lock (NoteManager.Backlog)
            //            NoteManager.Backlog.Enqueue(new Event(() => { try { outDevice.Send(message); } catch { } }, Stopwatch.ElapsedMilliseconds, Delay));
            //};
            //sequencer.Stopped += delegate (object o, StoppedEventArgs args)
            //{
            //    foreach (ChannelMessage message in args.Messages)
            //        lock (NoteManager.Backlog)
            //            NoteManager.Backlog.Enqueue(new Event(() => { try { outDevice.Send(message); } catch { } }, Stopwatch.ElapsedMilliseconds, Delay));
            //     Stop everything when the MIDI is done playing
            //    Stop();
            //};
            //sequence.LoadCompleted += delegate (object o, AsyncCompletedEventArgs args)
            //{
            //    LoadingStatus = -1;
            //    if (args.Cancelled)
            //    {
            //        MessageBox.Show("The operation was cancelled.", "MIDITrailer - Error", MessageBoxButtons.OK,
            //            MessageBoxIcon.Error);
            //        return;
            //    }
            //    sequencer.Sequence = sequence;
            //    if (LoadCompleted != null)
            //    {
            //        LoadCompleted.Invoke(this, args);
            //    }

            //};
            //sequence.LoadProgressChanged += delegate (object sender, ProgressChangedEventArgs args)
            //{
            //    LoadingStatus = args.ProgressPercentage;
            //};



        }
        

        private void PlayerHandler(object sender, MidiMessage e)
        {


            var cmd = e.command;
            int data1 = 0;
            int data2 = 0;
            var channel = 1;

            if (cmd == (int)MidiEventTypeEnum.NoteOff || (cmd == (int)MidiEventTypeEnum.NoteOn))
            {
                data1 = e.data1;
                data2 = e.data2;
            }

            if (cmd == (int)MidiEventTypeEnum.NoteOff || (cmd == (int)MidiEventTypeEnum.NoteOn && data2 == 0))
            {
                if (NoteManager.LastPlayed[channel, data1] != null)
                {
                    Note n = NoteManager.LastPlayed[channel, data1];
                    n.Playing = false;
                }
            }
            else if (cmd == (int)MidiEventTypeEnum.NoteOn)
            {
                Note n = new Note
                {
                    Key = data1,
                    Length = 0,
                    Playing = true,
                    Position = 0,
                    Time = Stopwatch.ElapsedMilliseconds,
                    Channel = channel,
                    Velocity = data2
                };
                lock (NoteManager.Notes)
                    NoteManager.Notes.Add(n);
                if (NoteManager.LastPlayed[channel, data1] != null)
                    NoteManager.LastPlayed[channel, data1].Playing = false;
                NoteManager.LastPlayed[channel, data1] = n;
            }

            lock (NoteManager.Backlog)
            {
                NoteManager.Backlog.Enqueue(new Event(delegate
                {
                    MidiPlayer.ProcessMidiMessage(e.channel,e.command,e.data1,e.data2);
                    //output.Send(e.Data, 0, 100, 0);
                    //outDevice.Send(args.Message);
                    if (cmd == (int)MidiEventTypeEnum.NoteOff || (cmd == (int)MidiEventTypeEnum.NoteOn && data2 == 0))
                    {
                        if (Keyboard.KeyPressed[data1] > 0)
                            Keyboard.KeyPressed[data1]--;
                    }
                    else if (cmd == (int)MidiEventTypeEnum.NoteOn)
                    {
                        Keyboard.KeyPressed[data1]++;
                    }
                    else if (cmd == (int)MidiEventTypeEnum.Controller)
                    {
                        if (data1 == 0x07)
                            Keyboard.ChannelVolume[channel] = data2;
                    }
                    else if (cmd == (int)MidiEventTypeEnum.PitchBend)
                    {
                        int pitchValue = Get14BitValue(data1, data2);
                        Keyboard.Pitchwheel[channel] = pitchValue;
                    }
                }, Stopwatch.ElapsedMilliseconds, Delay));
            }

        }

        //public void Load(string file)
        //{
        //    MIDIFile = file;
        //    music = MidiMusic.Read(System.IO.File.OpenRead(file));
        //    player = new MidiPlayer(music, output);
        //    player.EventReceived += PlayerHandler;
        //}

        public void Reset()
        {
            Keyboard.Reset();
            NoteManager.Reset();
            //outDevice?.Reset();
        }

        public void Dispose()
        {

            //player.Dispose();
        }

        public void Stop()
        {
            if (Stopped)
                return;
            eventTimer.Stop();
            //sequencer.Stop();
            Stopwatch.Stop();
            Stopped = true;
        }

        public void Start()
        {
            Stopped = false;
            eventTimer.Start();
            Stopwatch.Start();
           // player.PlayAsync();
            
        }

        public void Jump(int tick)
        {
            Reset();
            //sequencer.Position = tick;
            //if (Stopped)
            //    sequencer.Stop();
        }



        public void UpdateNotePositions()
        {
            int keyboardY = 100;
            long now = Stopwatch.ElapsedMilliseconds;
            float speed = 1.0f * keyboardY / Delay;
            // Update all note positions
            lock (NoteManager.Notes)
            {
                for (int i = 0; i < NoteManager.Notes.Count; i++)
                {
                    Note n = NoteManager.Notes[i];
                    if (n.Position > keyboardY)
                    {
                        NoteManager.Notes.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        if (n.Playing)
                            n.Length = (now - n.Time) * speed;
                        else
                            n.Position = (now - n.Time) * speed - n.Length;
                    }

                }
            }
        }

        public void UpdateRenderer()
        {
            // Update forced-fast mode
            if (NoteManager.Notes.Count > NoteManager.ForcedFastThreshold)
            {
                LastFancyTick = Stopwatch.ElapsedMilliseconds;
                NoteManager.RenderFancy = false;
            }
            else
            {
                if (NoteManager.UserEnabledFancy)
                    if (Stopwatch.ElapsedMilliseconds - LastFancyTick > NoteManager.ReturnToFancyDelay)
                        NoteManager.RenderFancy = true;
            }
        }

        public static int Get14BitValue(int nLowerPart, int nHigherPart)
        {
            return (nLowerPart & 0x7F) | ((nHigherPart & 0x7F) << 7);
        }

    }
}
