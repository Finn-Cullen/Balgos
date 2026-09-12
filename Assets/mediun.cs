using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

public class mediun : MonoBehaviour
{
    public mat_vals medium_val;

    Collider2D col;
    NativeList<float2> shadow;
    NativeList<int> act_nodes;
    mesh_manager mesh;

    public float width, height;

    public void Start(){
        mesh = FindObjectsByType<mesh_manager>(FindObjectsSortMode.None)[0];
        col = GetComponentInChildren<Collider2D>();
        shadow = new NativeList<float2>(Allocator.Persistent);
        createShadow();
        updateNodeVal();
        
    }

    public void updateNodeVal(){
        var stream = new NativeStream(shadow.Length, Allocator.TempJob);

        var jobAssign = new MediumCheckJob
        {
            NodeArr = mesh.grid.Nodes,
            writer = stream.AsWriter(),
            PosArr = shadow,
            pos = (float2)(Vector2)transform.position,
            medium_val = medium_val,
            base_val = mesh.air,
            width = mesh.grid.width,
            spacing = mesh.grid.spacing,
        };
        JobHandle handleCheck = jobAssign.Schedule(shadow.Length, 128);

        var jobmerge = new MediumAssignJob
        {
            reader = stream.AsReader(),
            NodeArr = mesh.grid.Nodes,
            foreachCount = shadow.Length,
        };
        JobHandle handleAssign = jobmerge.Schedule(handleCheck); 
        handleAssign.Complete();
    }

    public void resetNodeVal(){
        // undoes prior work on Noise Map
        foreach(int i in act_nodes){
            // loops through all nodes as it is a tilemap
            bool matCHK = mesh.grid.Nodes[i].decay == mesh.air.decay;
            matCHK = matCHK || mesh.grid.Nodes[i].decay == medium_val.decay;
            if(i > 0 && matCHK){
                mat_vals temp = mesh.grid.Nodes[i];
                float v = temp.decibels;
                temp = mesh.air;
                temp.decibels = v;
                mesh.grid.Nodes[i] = temp;
            }
        }
    }

    public void createShadow(){
        NativeList<int> listPos;
        if(width > 0){
            listPos = mesh.grid.get_nodes_grid(transform.position,width,height);
        }
        else{
            listPos = mesh.grid.get_nodes_grid(mesh.transform.position,mesh.grid.width,mesh.grid.height);
        }
        foreach(int i in listPos){
            if(col.OverlapPoint(mesh.grid.worldPositions[i])){
                float2 tr = mesh.grid.worldPositions[i]; // (float2)(Vector2)transform.position
                shadow.Add(tr);
            }
        }

        listPos.Dispose();
    }
}
