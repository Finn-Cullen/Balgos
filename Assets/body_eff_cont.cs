using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class body_eff_cont : MonoBehaviour
{

    Camera cam;

    public void setCAM(Camera Cam){
        cam = Cam;
    }

    public float rotSPEED;
    public float max_rot_angle;

    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition = cam.ScreenToWorldPoint(mousePosition);
        mousePosition = new Vector3(mousePosition.x,mousePosition.y,0);
        mousePosition = mousePosition - transform.position;
        if(Vector3.Angle(transform.parent.up,mousePosition) <= max_rot_angle){
            transform.up = Vector3.Lerp(transform.up,mousePosition,Time.deltaTime*rotSPEED);
        }
    }
}
