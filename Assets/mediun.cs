using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class mediun : MonoBehaviour
{
    public mat_vals medium_val;

    Collider2D col;

    public void generate(noise_grid grid)
    {
        col = GetComponent<Collider2D>();
        //NativeArray<int> nodes = grid.get_nodes_grid(transform.position,transform.localScale.x,transform.localScale.y);

        for(int i = 0; i < grid.Nodes.Length; i++){
            // loops through all nodes as it is a tilemap
            if(i > 0 && col.OverlapPoint(grid.worldPositions[i])){
                mat_vals temp = grid.Nodes[i];
                float v = temp.decibels;
                temp = medium_val;
                temp.decibels = v;
                grid.Nodes[i] = temp;
            }
        }
    }
}
