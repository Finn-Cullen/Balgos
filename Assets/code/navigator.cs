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

    public float targetRad;

    void Start(){
        mesh = FindObjectsByType<mesh_manager>(FindObjectsSortMode.None)[0];
    }

    void Update(){
        if(nav){
            nav = !nav;
            Navigate();
        }
    }

    public List<float2> posList;

    public void Navigate(){
        bool goal = false;
        while(!goal){
            // checks list so far, removes redundant moves, checks if goal is reached
            goal = checkList();
        }
    }

    bool checkList(){
        bool ch = false;
        // checks list, identifies if we are at goal or not
        if(posList.Count > 1){
            // does the check
            for(int i = 1; i < posList.Count; i++){
                float2 PosA = posList[i];
                float2 PosB = posList[i-1];
                bool checkPath = PosA.x < tarPos.position.x && PosB.x > tarPos.position.x;
                checkPath = checkPath || (PosA.x > tarPos.position.x && PosB.x < tarPos.position.x);
                float chVar = Mathf.Abs(PosA.y-tarPos.position.y);
                if(Mathf.Abs(PosA.x-PosB.x) < 0.5f){
                    checkPath = PosA.y < tarPos.position.y && PosB.y > tarPos.position.y;
                    checkPath = checkPath || (PosA.y > tarPos.position.y && PosB.y < tarPos.position.y);
                    chVar = Mathf.Abs(PosA.x-tarPos.position.x);
                }
                // checkPath checks if the target position lies between the nodes
                // chVar is the distance from the travel directio of the nodes and the target pos
                if(chVar < targetRad && checkPath){
                    Debug.Log("correct");
                    ch = true;
                }
                if(i > 1){
                    // from the third node onward we check for double backs
                    float2 PosC = posList[i-2];
                    string AtoB = "across";
                    string BtoC = "across";
                    if(Mathf.Abs(PosA.x-PosB.x) < 0.5f){AtoB = "above";}
                    if(Mathf.Abs(PosB.x-PosC.x) < 0.5f){BtoC = "above";}
                    if(AtoB == BtoC){
                        // checks if the nodes double back (I.E same direction is used twice in succession) and handles it
                        Debug.Log("removed double back");
                        posList.RemoveAt(i-1);
                        i--;
                    }
                }
            }
        }
        else{
            posList.Add((Vector2)transform.position);
        }
        return ch;
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
