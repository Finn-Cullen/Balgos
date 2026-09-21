using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameStateManager : MonoBehaviour
{


    void Update(){
        // DEBUG
        if(Input.GetKeyDown(KeyCode.E)){
            StopAllSources();
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

        //mediun medium = gameObject.AddComponent<mediun>();
        //medium.width = -1;
        //medium.medium_val.decay = 2;
        //medium.medium_val.minDecibelVal = 0;
    }
}
