using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSwitch : MonoBehaviour
{
    public string ACTSTR;
    // used to call function in another script
    public MonoBehaviour Conn;
    // calls function from here

    public float interval;
    float T;
    public float threshold;
    public bool activated;

    mesh_manager m;

    void Awake(){
        m = FindObjectsByType<mesh_manager>(FindObjectsSortMode.None)[0];
    }

    void Update()
    {
        if(T < Time.time && !activated){
            T = Time.time + interval;
            float thresh = m.grid.Nodes[m.grid.pos_to_ind((Vector2)transform.position)].decibels;
            // grabs decibel val at objects node position
            if(thresh > threshold){
                activated = true;
                Conn.Invoke(ACTSTR,0);
            }
        }
    }
}
