using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "setting_vals", menuName = "Data/setting_vals")]
public class setting_vals : ScriptableObject
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
