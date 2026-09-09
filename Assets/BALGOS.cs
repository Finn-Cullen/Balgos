using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class BALGOS : MonoBehaviour
{

    // player script

    Rigidbody2D rb;
    public mesh_manager mesh;
    public LayerMask mask;

    public float speed;
    public float rotSPEED;

    float step_timer;
    public float trail_noise;

    void Start(){
        foreach(body_eff_cont b in GetComponentsInChildren<body_eff_cont>()){
            b.setCAM(GetComponentInChildren<Camera>());
        }
        rb = GetComponent<Rigidbody2D>();
        mesh = FindObjectsByType<mesh_manager>(FindObjectsSortMode.None)[0];
    }

    void Update()
    {
        movement();
        load_manager();
        fireManager();
        noiseManager();
        steps();
        load_vis();
    }

    void steps(){
        if(Vector3.Distance(rb.velocity,Vector3.zero) > 0.2f){
            if(Time.time > step_timer){
                float r = UnityEngine.Random.Range(50,70);
                step_timer = Time.time + r/100;
                FOOT.NOISE();
            }
        }
    }

    float echoT;

    void fireManager(){
        if(Input.GetMouseButtonDown(0)){
            if(!isbreachopen && isroundload){
                isroundload = false;
                FIRE();
                barrel_fire_point.NOISE(); // fire noise
                bullet_EJECT.NOISE(); // bullet casing ejecting to floor
                breach_point.NOISE(); // breach opening
            }
        }
        if(Input.GetMouseButtonDown(1) && echoT < Time.time){
            // echolocate
            echoT = Time.time + 1.5f;
            echolocate_point.NOISE();
        }
    }

    public void FIRE(){
        RaycastHit2D hit = Physics2D.Raycast(barrel_fire_point.transform.position, barrel_fire_point.transform.up, 100, mask);
        limb l = hit.collider.GetComponent<limb>();
        if(l != null && l.canInjure){
            l.isInjure = true;
        }
        Vector3 diff = (Vector3)hit.point-barrel_fire_point.transform.position;
        for(float i = 0.01f; i < 1.01f; i += 0.01f){
            int grPoint = mesh.grid.pos_to_ind(barrel_fire_point.transform.position + (diff * i));
            mesh.grid.editnodeval(grPoint,trail_noise);
        }
    }

    bool isbreachopen;

    bool isroundload = true; 
    
    float loadtimer;
    string queued_action;

    public limb bullet_LOAD;
    // point where bullet is loaded/ejected
    public limb bullet_EJECT;
    // fumbles bullet
    public limb bullet_PICKUP;
    // point where bullet is picked up
    public limb barrel_fire_point;
    // where bullet is fired
    public limb echolocate_point;
    // where we echolocate from
    public limb lever_point;
    // where the breach is opened
    public limb breach_point;
    // where the breach jams
    public limb FOOT;
    // foot noises?

    public void load_manager(){
        // handles loading
        if(Input.GetKeyDown(KeyCode.Q)){
            // open/close breach
            if(loadtimer < Time.time){
                loadtimer = Time.time + 0.2f;
                queued_action = "breach_success";
                // queues action
            }
        }
        if(Input.GetKeyDown(KeyCode.E)){
            // load/unload round
            if(isbreachopen && !isroundload){
                if(loadtimer < Time.time){
                    loadtimer = Time.time + 0.5f;
                    queued_action = "load_success";
                    // queues action
                    bullet_PICKUP.NOISE();
                }
            }
        }
        if(loadtimer < Time.time && queued_action != ""){
            if(queued_action == "breach_success"){
                isbreachopen = !isbreachopen;
                lever_point.NOISE();
                queued_action = "";
            }
            if(queued_action == "load_success"){
                isroundload = true;
                //bullet_LOAD.NOISE();
                // unfortunately isnt audible enough
                queued_action = "";
                breach_point.NOISE();
            }
        }
    }

    public void load_vis(){
        float breachANG = 0;
        if(isbreachopen){
            breachANG = 90;
        }
        Vector2 bulletPOS = new Vector2(0.006f,0.08f);
        if(!isroundload){
            bulletPOS = new Vector2(0.006f,-0.02f);
        }
        lever_point.transform.localEulerAngles = new Vector3(0,0,Mathf.LerpAngle(lever_point.transform.localEulerAngles.z,breachANG,Time.deltaTime*5));
        bullet_LOAD.transform.localPosition = Vector2.Lerp(bullet_LOAD.transform.localPosition,bulletPOS,Time.deltaTime*5);
    }

    void movement(){
        float2 move = new float2(0,0);
        if(Input.GetKey(KeyCode.W)){
            move.y = speed;
        }
        if(Input.GetKey(KeyCode.S)){
            move.y = -speed;
        }
        if(Input.GetKey(KeyCode.A)){
            move.x = -speed;
        }
        if(Input.GetKey(KeyCode.D)){
            move.x = speed;
        }
        rb.AddForce(move);
        // make inertial later
    }

    void noiseManager(){
        if(Input.GetKeyDown(KeyCode.Escape)){
            Application.Quit();
        }
    }
}
