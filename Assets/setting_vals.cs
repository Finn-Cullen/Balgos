using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setting_vals : MonoBehaviour
{
    public float width; // how many pixels wide do we want it
    public float height; // how many pixels talll do we want it
    public float resolution; // multiplies pixel amounts

    public col_rank[] coloursequence;
    // defines sound colors

    void Awake(){
        Application.targetFrameRate = 60;
    }
}
