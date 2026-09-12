using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

public class mesh_manager : MonoBehaviour
{
    public noise_grid grid;

    public mat_vals air;
    public setting_vals s;

    public Vector2 debug;
    public float debugvala;

    public float refreshrate;
    float refrate;
    int frame = 0;

    public SpriteRenderer soundrenderer;

    Texture2D soundmap;
    NativeArray<Color32> soundColors;

    public void Awake(){

        soundColors = new NativeArray<Color32>(Mathf.RoundToInt((s.width*s.resolution)*(s.height*s.resolution)), Allocator.Persistent);
        soundmap = new Texture2D(Mathf.RoundToInt(s.width*s.resolution),Mathf.RoundToInt(s.height*s.resolution), TextureFormat.RGBA32, false);
        soundmap.wrapMode = TextureWrapMode.Clamp;
        soundmap.filterMode = FilterMode.Point;
        
        grid.construct_grid(air,s.resolution);
        // initialises grid

        Debug.Log("starts");
    }

    public void Update(){
        if(Time.time > refrate){
            // refreshes the screen
            refrate = Time.time + refreshrate;
            grid.updateMap(frame);
            UpdatesoundmapTexture();
            ApplysoundmapToRenderer();
            frame ++ ;
        }
    }

    void UpdatesoundmapTexture()
    {
        NativeArray<col_rank> colseq = new NativeArray<col_rank>(s.coloursequence.Length, Allocator.TempJob);
        for(int i = 0; i < s.coloursequence.Length; i++){
            colseq[i] = s.coloursequence[i];
        }
        var job = new DecibelsToColorJob
        {
            NodeArr = grid.Nodes,
            basecol = new Color32(0, 0, 0, 0), // background color
            colranks = colseq,

            colors = soundColors,

            gridwidth = Mathf.RoundToInt(grid.width/grid.spacing),
            colourwidth = Mathf.RoundToInt(s.width*s.resolution),
            spacing = grid.spacing,
            initpos = soundrenderer.transform.position,
        };

        job.Schedule(soundColors.Length, 128).Complete();
        soundmap.SetPixelData(soundColors, 0);
        soundmap.Apply(false, false);
        colseq.Dispose();
    }

    void ApplysoundmapToRenderer()
    {
        Sprite sprite = Sprite.Create(
            soundmap,
            new Rect(0, 0, (s.width*s.resolution), (s.height*s.resolution)), // size in pixels
            new Vector2(0f, 0f), // pivot
            pixelsPerUnit: 100
        );
        soundrenderer.transform.localScale = new Vector2(1/s.resolution,1/s.resolution);
        soundrenderer.sprite = sprite;
    }
}