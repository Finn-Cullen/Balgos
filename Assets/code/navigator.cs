using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

public class navigator : MonoBehaviour
{
    
    public bool nav;
    public Transform tarPos;
    mesh_manager mesh;

    void Start(){
        mesh = FindObjectsByType<mesh_manager>(FindObjectsSortMode.None)[0];
    }

    void Update(){
        if(nav){
            nav = !nav;
            Navigate();
        }
    }

    public void Navigate(){
        // order of conduct
        // establish init perameters for search
        // generate positions
        //      generate prongs at position
        //      discover most legitimate prong
        //      edit prong pos to be better (in line with center of room / tarpos)
        generatePositions();
    }

    public void generatePositions(){
        // returns 1 new position every time called
        int2 desDir = new int2(-1,-1);
        if(transform.position.x < tarPos.position.x){
            desDir.x = 1;
        }
        if(transform.position.y < tarPos.position.y){
            desDir.y = 1;
        }
        int2[] dirs = {new int2(1,0),new int2(-1,0),new int2(0,1),new int2(0,-1),desDir};
        foreach(int2 d in dirs){
            int pos = searchProng(d);
            Debug.Log(mesh.grid.worldPositions[pos]);
        }
    }

    public int searchProng(int2 dir){
        int tot = Mathf.RoundToInt(mesh.grid.width/mesh.grid.spacing);
        int searchPos = mesh.grid.pos_to_ind(transform.position);
        bool searchConclude = true;
        int breakOUT = 0;
        while(searchConclude && breakOUT < tot){
            // loop until hit wall (medium denoted by low spreadrate)
            breakOUT++;
            searchPos += 5*(dir.x * 1);
            searchPos += 5*(dir.y * tot);
            if(mesh.grid.Nodes[searchPos].spreadRate > 3){
                // breaks loop and adds position of colision to list
                searchConclude = false;
                return searchPos;
            }
        }
        return -1;
    }
}
