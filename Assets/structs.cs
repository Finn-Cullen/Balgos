using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

// stores data structures

[System.Serializable]
public struct mat_vals{
    // values for assigning mediums
    public float decibels;
    // how loud the point is
    public float decay;
    // how fast the noise decays to zero
    public float spreadRate;
    // how fast it spreads
    public float spreadPerc;
    // how much of it spreads each time
    public float spreadExclusive;
    // how quickly does it spread to new mediums
}

[System.Serializable]
public struct col_rank{
    // used to handle colour display by values
    public Color32 col;
    public float max;
}

public struct NodeUpdate
{
    // used in jobs to write node values in bulk
    public int index;
    public float value;
}