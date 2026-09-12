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
    NativeList<int> ActPos;
    mesh_manager mesh;

    public float width, height;

    public void Start(){
        mesh = FindObjectsByType<mesh_manager>(FindObjectsSortMode.None)[0];
        col = GetComponentInChildren<Collider2D>();
        shadow = new NativeList<float2>(Allocator.Persistent);
        createShadow();
        updateNodeVal();
    }

    public void StepNodeVal(){
        // used to update mediuns position.
        // resets prior call of updateNodeVal and then calls another
        resetNodeVal();
        updateNodeVal();
    }

    void updateNodeVal(){
        var stream = new NativeStream(shadow.Length, Allocator.TempJob);

        NativeList<int> SavPos = new NativeList<int>(Allocator.Persistent);

        var jobCheck = new MediumCheckJob
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
        JobHandle handleCheck = jobCheck.Schedule(shadow.Length, 128);

        var jobAssign = new MediumAssignJob
        {
            reader = stream.AsReader(),
            NodeArr = mesh.grid.Nodes,
            Sav = SavPos,
            foreachCount = shadow.Length,
        };
        JobHandle handleAssign = jobAssign.Schedule(handleCheck); 
        
        handleAssign.Complete();
        stream.Dispose();
        ActPos = SavPos;
    }

    void resetNodeVal(){
        if(ActPos.IsCreated){
            var stream = new NativeStream(ActPos.Length, Allocator.TempJob);
            var jobReset = new MediumResetJob
            {
                writer = stream.AsWriter(),
                NodeArr = mesh.grid.Nodes,
                listPos = ActPos,
                medium_val = medium_val,
                base_val = mesh.air,
            };
            JobHandle handleReset = jobReset.Schedule(ActPos.Length, 128);

            var jobAssign = new MediumAssignJob
            {
                reader = stream.AsReader(),
                NodeArr = mesh.grid.Nodes,
                Sav = ActPos,
                foreachCount = shadow.Length,
            };
            JobHandle handleAssign = jobAssign.Schedule(handleReset); 
            handleAssign.Complete();
            ActPos.Dispose();
            stream.Dispose();
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
                float2 tr = (float2)(Vector2)transform.position - mesh.grid.worldPositions[i];
                shadow.Add(tr);
            }
        }

        listPos.Dispose();
    }
}
