using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class metronome : MonoBehaviour
{

    public float ang;
    public float period;

    void Update()
    {
        float timer = Mathf.Sin(Time.time*period);
        transform.localEulerAngles = new Vector3(0,0,timer*ang);
    }
}
