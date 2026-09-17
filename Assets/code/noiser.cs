using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class noiser : MonoBehaviour
{


    mesh_manager mesh;

    public float time;
    float timer;

    public float volume;

    void Start(){
        mesh = FindObjectsByType<mesh_manager>(FindObjectsSortMode.None)[0];
    }

    void Update(){
        if(timer < Time.time){
            timer = Time.time + time;
            mesh.grid.editnodeval(mesh.grid.pos_to_ind(transform.position),volume);
        }
    }
}
