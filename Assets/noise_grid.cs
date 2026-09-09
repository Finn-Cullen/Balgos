using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

// noise grid gets its own file bc its too big

[System.Serializable]
public struct noise_grid{

    // each array val is a node, each node represents the noise at a point in space

    // public NativeArray<float> decibels; // how loud the node is      
    // public NativeArray<float> decayRate; // rate at which nodes lose decibels per frame
    // public NativeArray<float> spreadRate; // how fast the noise spreads to nearby nodes
    // public NativeArray<float> spreadPerc; // how much of the decibels spread per spread
    // public NativeArray<float> spreadExclusive; // used for intermedium transmission

    public NativeArray<mat_vals> Nodes; // holds all the node values
    public NativeArray<float2> worldPositions; // position of the node
        
    public NativeList<int> activeNodes; // nodes that currently have noise
 
    public int width, height;
    public float spacing;
    
    public void construct_grid(mat_vals air, float res){
        // produces new grid
        
        spacing = (1/res)/100;
        int nodeCount = (int)((width/spacing)*(height/spacing));
        Nodes = new NativeArray<mat_vals>(nodeCount, Allocator.Persistent);
        worldPositions = new NativeArray<float2>(nodeCount, Allocator.Persistent);
        // sets array lengths

        activeNodes = new NativeList<int>(Allocator.Persistent);

        // threading stops abominable lag
        var job = new assign_pos_job
        {
            pos = worldPositions,
            NodeArr = Nodes,
            width = Mathf.RoundToInt(width/spacing),
            spacing = spacing,
            mv = air
        };

        JobHandle handle = job.Schedule(worldPositions.Length, 512);
        handle.Complete();

        Debug.Log("graph complete!");
        Debug.Log(worldPositions.Length);
    }

    public void remove_grid(){
        // clears array
        if (Nodes.IsCreated) Nodes.Dispose();
        if (worldPositions.IsCreated) worldPositions.Dispose();
    } 

    public void updateMap(int frame){
        var stream = new NativeStream(activeNodes.Length, Allocator.TempJob);
        // need to do decay and have newly spread nodes become active + cull low nodes

        var jobspread = new spreadJob
        {
            activeNodes = activeNodes.AsArray(),
            NodeArr = Nodes,
            writer = stream.AsWriter(),
            width = Mathf.RoundToInt(width/spacing),
            frame = frame,
        };
        JobHandle handlespread = jobspread.Schedule(activeNodes.Length, 512);
        // correctly spreads

        NativeList<int> setNodes = new NativeList<int>(Allocator.Persistent);
        // makes new list to have new nodes written to
        NativeArray<bool> a = new NativeArray<bool>(Nodes.Length,Allocator.TempJob);
        var jobmerge = new MergeSpreadJob
        {
            reader = stream.AsReader(),
            // decibels = decibels,
            NodeArr = Nodes,
            foreachCount = activeNodes.Length,
            activeNodes = setNodes,
            isActive = a,
        };
        JobHandle handlemerge = jobmerge.Schedule(handlespread); 
        
        handlemerge.Complete();
        // correctly applies spread/decay

        activeNodes.Dispose();
        activeNodes = setNodes;
        a.Dispose();
        stream.Dispose();

    }

    public void editnodeval(int pos, float val){
        //decibels[pos] = val;
        mat_vals temp = Nodes[pos];
        temp.decibels = val;
        Nodes[pos] = temp;
        activeNodes.Add(pos);
    }

    public int pos_to_ind(Vector2 pos){
        // takes a world position and returns the closest node
        float remx = pos.x % spacing;
        float addx = 0;
        if(remx >= spacing/2){
            addx = spacing;
        }
        float remy = pos.y % spacing;
        float addy = 0;
        if(remy >= spacing/2){
            addy = spacing;
        }
        // rounds pos values to grid
        remx = pos.x - (pos.x%spacing) + addx;
        remy = pos.y - (pos.y%spacing) + addy;
        return Mathf.RoundToInt(((remx/spacing)+((remy/spacing)*(width/spacing))));
    }

    public NativeList<int> get_nodes_grid(Vector2 worldpos, float wide, float tall){
        // takes a world position and returns the nodes within a distance of that position

        worldpos = new Vector2(worldpos.x - wide/2, worldpos.y - tall/2);
        // puts us in bottom left corner

        // turns world units into grid units
        int widthloc = Mathf.RoundToInt(wide/spacing);
        int heightloc = Mathf.RoundToInt(tall/spacing);
        // how wide / tall the search space is 

        float addx = 0;
        float x = worldpos.x % spacing;
        if(x > spacing/2){
            addx = spacing;
        }
        float addy = 0;
        float y = worldpos.y % spacing;
        if(y > spacing/2){
            addy = spacing;
        }
        float2 startpos = new float2((worldpos.x-x) + addx, (worldpos.y-y) + addy);
        int startindex = pos_to_ind(startpos);
        // rounds position to grid

        NativeList<int> gridpositions = new NativeList<int>(Allocator.Persistent);

        for(int i = 0; i < widthloc*heightloc; i++){
            int pos = Mathf.RoundToInt(i % widthloc + ((width/spacing) * Mathf.RoundToInt(i / widthloc)));
            pos += startindex;
            gridpositions.Add(pos);
        }

        // returns a list of grid index positions that fall within the distance
        return gridpositions;
        // works, might need to turn into a job bc of how expensive this is
    }
}