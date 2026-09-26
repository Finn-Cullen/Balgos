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

    void Update(){
        if(nav){
            nav = !nav;
            mesh = FindObjectsByType<mesh_manager>(FindObjectsSortMode.None)[0];
            int2 desDir = new int2(-1,-1);
            if(transform.position.x < tarPos.position.x){
                desDir.x = 1;
            }
            if(transform.position.y < tarPos.position.y){
                desDir.y = 1;
            }
            int a = searchProng(desDir);
            Debug.Log(desDir);
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
            Debug.Log(mesh.grid.worldPositions[searchPos]);
            if(mesh.grid.Nodes[searchPos].spreadRate > 3){
                // breaks loop and adds position of colision to list
                searchConclude = false;
                return searchPos;
            }
        }
        return -1;
    }
}
