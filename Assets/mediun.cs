using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

public class mediun : MonoBehaviour
{
    public mat_vals medium_val;

    Collider2D[] cols;
    NativeList<int> act_nodes;
    mesh_manager mesh;

    public float width, height;

    public void Start(){
        mesh = FindObjectsByType<mesh_manager>(FindObjectsSortMode.None)[0];
        cols = GetComponentsInChildren<Collider2D>();

        updateNodeVal();
    }

    public void updateNodeVal(){
        // updates noise map with medium values
        NativeList<int> savnode = mesh.grid.get_nodes_grid(transform.position,width,height);

        var jobAssign = new MediumAssignmentJob
        {
            NodeArr = mesh.grid.Nodes,
            PosArr = savnode,
            col = cols,
            width = mesh.grid.width,
            spacing = mesh.grid.spacing,
            medium_val = medium_val,
            base_val = mesh.air,
        };
        JobHandle handleAssign = jobAssign.Schedule(savnode.Length, 128);

        handleAssign.Complete();

        act_nodes.Dispose();
        act_nodes = savnode;
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
}
