using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Kazedan.Construct;
using UnityEngine;

public class Bars : MonoBehaviour {
    private MIDISequencer sequencer;

    //SpriteRenderer sr;
    //private Sprite[] sprites;

    // Use this for initialization
    void Start () {
	    sequencer = new MIDISequencer();
	    sequencer.LoadCompleted += Sequencer_LoadCompleted;
	    Thread t = new Thread(() => { sequencer.Init(); });
	    t.Start();
	    t.Join();
        
	    sequencer.ShowDebug = true;
	    
	    sequencer.Load("C:/repo/Piano/Assets/bach.mid");
    }

    private void Sequencer_LoadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        sequencer.Start();
    
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
            var noteOffset = 0;
            var noteCount = 88;
            float keywidth = 3f;
            float scale = 0.001f;
            float yoffset = 5f;
            float xspacing = 30f;
            if (n.Key >= noteOffset && n.Key < noteOffset + noteCount && n.Length > 0 && n.Velocity > 0)
            {
                float left = n.Key * keywidth + (xspacing * n.Key);
                float x = left;
                float y = 100f - n.Position;
                float width = keywidth;
                float height = n.Length * 1f ;
                var bar = BarPool.bars[noteIndex];
                var renderer = bar.GetComponentInChildren<SpriteRenderer>();
                renderer.transform.position = new Vector3(x * scale, (y - (height / 2))* scale);
                renderer.size = new Vector3(width * 0.01f, height * scale);
                bar.SetActive(true);
                
            }
            noteIndex++;
        }
        
        
        //renderer.sprite.bounds.size.Set(count, count, 1);



        //      var sprite = GetComponentInChildren<Sprite>();
        //var obj = GetComponentInParent<GameObject>();

        //        var newSprite = Component.Instantiate(sprite);
        //SpriteRenderer rend = obj.AddComponent<Sprite>();
        //rend.sprite = sprites[0];
    }
}
