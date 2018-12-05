using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using AudioSynthesis.Bank;
using AudioSynthesis.Midi;
using AudioSynthesis.Sequencer;
using AudioSynthesis.Synthesis;
using Kazedan.Construct;
using UnityEngine;
using UnityMidi;

[RequireComponent(typeof(AudioSource))]
public class Bars : MonoBehaviour {
    
    [SerializeField] StreamingAssetResouce bankSource;
    [SerializeField] StreamingAssetResouce midiSource;
    [SerializeField] bool loadOnAwake = true;
    [SerializeField] bool playOnAwake = true;
    [SerializeField] int channel = 1;
    [SerializeField] int sampleRate = 44100;
    [SerializeField] int bufferSize = 1024;
    PatchBank bank;
    MidiFile midi;
    public Synthesizer synthesizer;
    AudioSource audioSource;
    MidiFileSequencer sequencer2;
    int bufferHead;
    float[] currentBuffer;

    //private MidiPlayer player;

    private Kazedan.Construct.MIDISequencer sequencer;
    //SpriteRenderer sr;
    //private Sprite[] sprites;

    // Use this for initialization
    void Start () {
	    
	    //sequencer.ShowDebug = true;
	    
	    //sequencer.Load("C:/repo/Piano/Assets/bach.mid");
     //   sequencer.Start();


        //var synthesizer = new Synthesizer(sampleRate, channel, bufferSize, 1);
        //var sequencer2 = new MidiFileSequencer(synthesizer);
        //var audioSource = GetComponent<AudioSource>();

        
        
        
        //player.Play();

         sequencer = new MIDISequencer();
        sequencer.MidiPlayer = this;
        //player.Awake(synthesizer, sequencer2, audioSource, bankSource, midiSource);
        sequencer.Init();
        sequencer.Start();

    }

    public void Awake()
    {
        synthesizer = new Synthesizer(sampleRate, channel, bufferSize, 1);
        sequencer2 = new MidiFileSequencer(synthesizer);
        audioSource = GetComponent<AudioSource>();

        if (loadOnAwake)
        {
            LoadBank(new PatchBank(bankSource));
            LoadMidi(new MidiFile(midiSource));
        }

        if (playOnAwake)
        {
            Play();
        }
    }

    // Update is called once per frame
    void Update () {
        sequencer.UpdateNotePositions();
        sequencer.UpdateRenderer();
        //sr.size.Set(sr.size.x + count, sr.size.y + count);
        int noteIndex = 0;

        foreach (var bar in BarPool.bars)
        {
            bar.SetActive(false);
        }

        var notesArray = sequencer.NoteManager.Notes.ToArray();

        
        foreach (var n in notesArray)
        {
            var shuntNotes = 40;
            var noteOffset = 0;
            var noteCount = 88;
            float keywidth = 1f;
            float xscale = 0.0005f;
            float yscale = 0.005f;
            float yoffset = 5f;
            float xspacing = 30f;
            if (n.Key >= noteOffset && n.Key < noteOffset + noteCount && n.Length > 0 && n.Velocity > 0)
            {
                float left = (n.Key - shuntNotes) * keywidth + (xspacing * (n.Key - shuntNotes));
                float x = left;
                float y = 100f - n.Position;
                float width = keywidth;
                float height = n.Length * 1f;
                var bar = BarPool.bars[noteIndex];
                var renderer = bar.GetComponentInChildren<SpriteRenderer>();
                renderer.transform.position = new Vector3(this.transform.position.x + (x * xscale), this.transform.position.y + ((y - (height / 2)) * yscale), this.transform.position.z);
                //renderer.transform.rotation = this.transform.rotation;
                //renderer.transform.Rotate(180,0,0);
                renderer.size = new Vector3(width * 0.01f, height * yscale);
                bar.SetActive(true);

            }
            noteIndex++;
        }

    }

    public void LoadBank(PatchBank bank)
    {
        this.bank = bank;
        synthesizer.UnloadBank();
        synthesizer.LoadBank(bank);
    }

    public void LoadMidi(MidiFile midi)
    {
        this.midi = midi;
        sequencer2.Stop();
        sequencer2.UnloadMidi();
        sequencer2.LoadMidi(midi);
    }

    public void ProcessMidiMessage(int channel, int command, int data1, int data2)
    {
        synthesizer.ProcessMidiMessage(channel, command, data1, data2);
    }

    public void Play()
    {
        sequencer2.Play();
        audioSource.Play();
    }

    void OnAudioFilterRead(float[] data, int channel)
    {
        //Debug.Assert(this.channel == channel);
        int count = 0;
        while (count < data.Length)
        {
            if (currentBuffer == null || bufferHead >= currentBuffer.Length)
            {
                sequencer2.FillMidiEventQueue();
                synthesizer.GetNext();
                currentBuffer = synthesizer.WorkingBuffer;
                bufferHead = 0;
            }
            var length = Mathf.Min(currentBuffer.Length - bufferHead, data.Length - count);
            System.Array.Copy(currentBuffer, bufferHead, data, count, length);
            bufferHead += length;
            count += length;
        }
    }
}
