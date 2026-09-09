using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class act_updater : MonoBehaviour
{

    public mat_vals medium_val;

    NativeList<int> act_nodes;
    Collider2D[] col;
    mesh_manager mesh;

    public float width,height = 1;

    int pos;

    void Start(){
        mesh = FindObjectsByType<mesh_manager>(FindObjectsSortMode.None)[0];
        col = GetComponentsInChildren<Collider2D>();
        act_nodes = mesh.grid.get_nodes_grid(transform.position,width,height);
        for(int I = 0; I < act_nodes.Length; I++){
            // loops through all nodes as it is a tilemap
            int i = act_nodes[I];
            if(i > 0 && colCHK(mesh.grid.worldPositions[i])){
                mat_vals temp = mesh.grid.Nodes[i];
                float v = temp.decibels;
                temp = medium_val;
                temp.decibels = v;
                mesh.grid.Nodes[i] = temp;
            }
            else{
                act_nodes.RemoveAt(I);
                // if it doesnt intersect with our collder we dont need to consider it
            }
        }
        pos = mesh.grid.pos_to_ind(transform.position);
    }

    void Update()
    {
        // maybe clone current node vals to new pos? so noise carries
        //if(pos != mesh.grid.pos_to_ind(transform.position)){
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
            
            NativeList<int> savnode = mesh.grid.get_nodes_grid(transform.position,width,height);
            for(int i = 0; i < savnode.Length; i++){
                int savpos = savnode[i];
                // loops through all nodes as it is a tilemap
                bool matCHK = mesh.grid.Nodes[savpos].decay == mesh.air.decay;
                matCHK = matCHK || mesh.grid.Nodes[savpos].decay == medium_val.decay;
                if(i > 0 && colCHK(mesh.grid.worldPositions[savpos]) && matCHK){
                    mat_vals temp = mesh.grid.Nodes[savpos];
                    float v = temp.decibels;
                    temp = medium_val;
                    temp.decibels = v;
                    mesh.grid.Nodes[savpos] = temp;
                }
                else{
                    savnode.RemoveAt(i);
                    // if it doesnt intersect with our collder we dont need to consider it
                }
            }
            act_nodes.Dispose();
            act_nodes = savnode;
        //}
    }

    bool colCHK(Vector2 tr){
        bool ch = false;
        foreach(Collider2D c in col){
            if(c.OverlapPoint(tr)){
                ch = true;
                break;
            }
        }
        return ch;
    }
}
