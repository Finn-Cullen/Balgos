using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

// stores jobs

[BurstCompile]
public struct assign_pos_job : IJobParallelFor
{
    public NativeArray<mat_vals> NodeArr;
    public NativeArray<float2> pos;
    [ReadOnly] public int width;
    [ReadOnly] public float spacing; 
    [ReadOnly] public mat_vals mv;

    public void Execute(int index)
    {
        float widthloc = width/spacing;
        
        float posx = (index%width)*spacing;
        float posy = (float)((int)index/(int)width)*spacing;

        pos[index] = new float2(posx,posy);

        // assigns position
        NodeArr[index] = mv;
        // assigns base air vals

    }
}

[BurstCompile]
public struct spreadJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<int> activeNodes;
    [ReadOnly] public NativeArray<mat_vals> NodeArr;

    [ReadOnly] public int width;
    [ReadOnly] public int frame;

    public NativeStream.Writer writer;
    //public NativeList<int> newSpread;

    public void Execute(int index){
        writer.BeginForEachIndex(index);
        int nodeIND = activeNodes[index];

        if(frame % (NodeArr[nodeIND].spreadRate - (NodeArr[nodeIND].spreadRate%1)) == 0){
            // writes to adgacent neighbours
            checkNeighbours(nodeIND + 1,nodeIND);
            checkNeighbours(nodeIND - 1,nodeIND);
            checkNeighbours(nodeIND + width,nodeIND);

            checkNeighbours(nodeIND + (width*2) + 2,nodeIND);
            checkNeighbours(nodeIND + (width*2) - 2,nodeIND);

            checkNeighbours(nodeIND - width,nodeIND);
            checkNeighbours(nodeIND - (width*2) + 2,nodeIND);
            checkNeighbours(nodeIND - (width*2) - 2,nodeIND);
        }

        // decays current node
        float decVAL = NodeArr[nodeIND].decibels - NodeArr[nodeIND].decay;
        decVAL = Mathf.Clamp(decVAL,0,200);
        writer.Write(new NodeUpdate {index = nodeIND,value = decVAL});

        writer.EndForEachIndex();
    }
 
    void checkNeighbours(int neighborIndex, int nodeIND){
        bool indCHK = neighborIndex > 0 && neighborIndex < NodeArr.Length;
        bool valCHK = false;
        if(indCHK){

            valCHK = NodeArr[neighborIndex].decibels < NodeArr[nodeIND].decibels;
            valCHK = valCHK && (NodeArr[nodeIND].decibels - NodeArr[neighborIndex].decibels > 50);
            valCHK = valCHK || NodeArr[neighborIndex].decibels <= 0;

            if(valCHK){
                // writes value to STREAM

                float decVAL = NodeArr[nodeIND].decibels-(NodeArr[nodeIND].decay*NodeArr[nodeIND].spreadPerc);
                // standard spread value, each spread decays by the decay amount multiplied by spread percentage

                if(NodeArr[nodeIND].spreadExclusive != NodeArr[neighborIndex].spreadExclusive){
                    decVAL = NodeArr[nodeIND].decibels*NodeArr[nodeIND].spreadExclusive; 
                    // when going between mediums we use the exclusive spread value
                    // its exclusive because only a few mediums will make use of it
                }                

                writer.Write(new NodeUpdate {index = neighborIndex, value = decVAL});
            }
        }
    }
}

[BurstCompile]
public struct MergeSpreadJob : IJob
{
    public NativeStream.Reader reader;
    public int foreachCount;
    public NativeArray<mat_vals> NodeArr;
    public NativeList<int> activeNodes;
    public NativeArray<bool> isActive;

    public void Execute()
    {
        for (int i = 0; i < foreachCount; i++)
        {
            int count = reader.BeginForEachIndex(i);
            for (int j = 0; j < count; j++)
            {
                NodeUpdate update = reader.Read<NodeUpdate>();
                mat_vals t = NodeArr[update.index];
                if(update.index < NodeArr.Length){
                    t.decibels = update.value;
                }
                // updates decibels with new value
                
                if(update.value > 0f && !isActive[update.index]){ //  && !activeNodes.Contains(update.index)
                    isActive[update.index] = true;
                    activeNodes.Add(update.index);
                    // outList holds all the valid index positions
                }
                else if(update.value < 0f && update.index < NodeArr.Length){
                    t.decibels = 0;
                    // removes node
                }
                NodeArr[update.index] = t;
            }
            reader.EndForEachIndex();
        }
    }
}

[BurstCompile]
public struct DecibelsToColorJob : IJobParallelFor
{
    
    [ReadOnly] public NativeArray<col_rank> colranks;
    [ReadOnly] public Color32 basecol;
    public NativeArray<Color32> colors; // to be written to
    
    [ReadOnly] public NativeArray<mat_vals> NodeArr; // used to calculate colour

    [ReadOnly] public int gridwidth; // how many nodes wide the grid is
    [ReadOnly] public int colourwidth; // how many nodes wide the screen is
    [ReadOnly] public float spacing; // worldspace distance between nodes
    [ReadOnly] public Vector2 initpos;  // position of soundmap

    public void Execute(int index)
    {
        float addx = 0;
        float x = initpos.x % spacing;
        if(x > spacing/2){
            addx = spacing;
        }
        float addy = 0;
        float y = initpos.y % spacing;
        if(y > spacing/2){
            addy = spacing;
        }
        float2 startindex = new float2((initpos.x-x) + addx, (initpos.y-y) + addy);
        // rounds initpos to be on the grid

        float indwidth = index % colourwidth; // how far along the screen we are
        float indheight = index / colourwidth;

        float2 indexpos = new float2(indwidth, indheight*gridwidth);

        int ind = job_pti(startindex);
        ind += Mathf.RoundToInt(indexpos.x);
        ind += Mathf.RoundToInt(indexpos.y);

        float mincolval = 0;
        float maxcolval = colranks[0].max;
        Color32 tarcol = colranks[0].col;
        Color32 orgcol = basecol;
        int i = 0;
        while(NodeArr[ind].decibels >= maxcolval && i+1 < colranks.Length){
            mincolval = colranks[i].max;
            orgcol = colranks[i].col;
            i++;
            maxcolval = colranks[i].max;
            tarcol = colranks[i].col;
        }

        float t = math.saturate((NodeArr[ind].decibels-mincolval) / (maxcolval-mincolval));
        colors[index] = Color32.Lerp(orgcol, tarcol, t);
        // handles colours
    }

    public int job_pti(Vector2 pos){
        // local version of pos to ind function
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

        return Mathf.RoundToInt(((remx/spacing)+((remy/spacing)*(gridwidth))));
    }
}