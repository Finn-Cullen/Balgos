using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class act_updater : MonoBehaviour
{

    mediun[] meds;

    public float refresh_timer;
    float T;

    void Start(){
        meds = gameObject.GetComponentsInChildren<mediun>();
    }

    void Update()
    {
        if(T < Time.time){
            T = Time.time + refresh_timer;
            foreach(mediun m in meds){
                m.StepNodeVal();
            }
        }
    }
}
