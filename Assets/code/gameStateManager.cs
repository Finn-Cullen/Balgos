using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using UnityEngine.SceneManagement;

public class gameStateManager : MonoBehaviour
{

    mesh_manager mesh;

    public bool transition;
    public string NEWSCENE;

    void Awake(){
        mesh = FindObjectsByType<mesh_manager>(FindObjectsSortMode.None)[0];
    }

    void Update(){
        if(transition){
            // we be transitioning
            if(mesh.grid.activeNodes.Length == 0){
                // no noise in the world
                SceneManager.LoadScene(NEWSCENE);
            }
        }
    }

    void StopAllSources(){
        act_updater[] a = FindObjectsByType<act_updater>(FindObjectsSortMode.None);
        noiser[] n = FindObjectsByType<noiser>(FindObjectsSortMode.None);
        SoundSwitch[] s = FindObjectsByType<SoundSwitch>(FindObjectsSortMode.None);
        BALGOS[] b = FindObjectsByType<BALGOS>(FindObjectsSortMode.None);

        foreach(act_updater A in a){A.enabled = false;}
        foreach(noiser N in n){N.enabled = false;}
        foreach(SoundSwitch S in s){S.enabled = false;}
        foreach(BALGOS B in b){B.enabled = false;}

        mat_vals MV = new mat_vals();
        MV.decay = 2;
        MV.maxDecibelVal = 200;
        MV.spreadRate = 1;
        MV.spreadPerc = 1f;

        var jobInit = new InitGridJob
        {
            NodeArr = mesh.grid.Nodes,
            PosArr = mesh.grid.worldPositions,
            width = Mathf.RoundToInt(mesh.grid.width/mesh.grid.spacing),
            spacing = mesh.grid.spacing,
            medium_val = MV,
            SPWidth = mesh.grid.spacingWidth,
            GRWidth = mesh.grid.gradientWidth,
            GRHeight = mesh.grid.gradientHeight,
        };
        JobHandle handleInit = jobInit.Schedule(mesh.grid.Nodes.Length, 512);
        handleInit.Complete();
        // sets every medium to a fast decay so we can load in a timely manner

        transition = true;
    }
}
