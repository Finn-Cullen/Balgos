using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cursorCont : MonoBehaviour
{

    Camera cam;
    mesh_manager mesh;
    Vector3 savPos;

    public float clickNoise;
    public float idleNoise;
    public float moveNoise;
    public float interval;
    float T;

    void Start()
    {
        cam = FindObjectsByType<Camera>(FindObjectsSortMode.None)[0];
        mesh = FindObjectsByType<mesh_manager>(FindObjectsSortMode.None)[0];
        Vector3 mousePosition = Input.mousePosition;
        mousePosition = cam.ScreenToWorldPoint(mousePosition);
        savPos = new Vector3(mousePosition.x,mousePosition.y,0);
    }

    void FixedUpdate()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition = cam.ScreenToWorldPoint(mousePosition);
        mousePosition = new Vector3(mousePosition.x,mousePosition.y,0);
        if(mousePosition.x > 0 && mousePosition.y > 0){
            if(Input.GetMouseButtonDown(0)){
                mesh.grid.editnodeval(mesh.grid.pos_to_ind(mousePosition),clickNoise);
            }
            else if(Vector3.Distance(mousePosition,savPos) < 0.1f && T < Time.time){
                T = Time.time + interval;
                mesh.grid.editnodeval(mesh.grid.pos_to_ind(mousePosition),idleNoise);
            }
            else{
                float Noise = Vector3.Distance(mousePosition,savPos)/2.4f;
                Noise = Noise*moveNoise;
                mesh.grid.editnodeval(mesh.grid.pos_to_ind(mousePosition),Noise);
            }
        }
        savPos = mousePosition;
    }
}
