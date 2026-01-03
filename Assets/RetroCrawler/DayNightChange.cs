
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightChange : MonoBehaviour
{
    [SerializeField] Material dayMaterial;
    [SerializeField] Material nightMaterial;

    private void Awake()
    {
        GameInstance.progress += OnProgressChanged;
    }

    private void OnProgressChanged(int countdown)
    {
        //print(GameInstance.GetNormalTime()[1].ToString() + ":" + GameInstance.GetNormalTime()[2].ToString()+":"+GameInstance.GetNormalTime()[3].ToString());
        if (GameInstance.GetNormalTime()[1] >= 6 && GameInstance.GetNormalTime()[1] < 20  
            && (GameInstance.playerController.GetBattleGroundEnvironment() == BattleGroundEnvironment.CITY 
            || GameInstance.playerController.GetBattleGroundEnvironment() == BattleGroundEnvironment.STONE
            || GameInstance.playerController.GetBattleGroundEnvironment() == BattleGroundEnvironment.WOOD))
        {
            if (RenderSettings.skybox != dayMaterial)
            {
                RenderSettings.skybox = dayMaterial;
                RenderSettings.fog = false;
            }
        }
        else
        {
            if(RenderSettings.skybox != nightMaterial)
            {
                RenderSettings.skybox = nightMaterial;
                RenderSettings.fog = true;
            }
/*            if (GameInstance.playerController.IsTorchIsOn())
            {
                RenderSettings.fogEndDistance = 30f;
                RenderSettings.fogDensity = 0.16f;
            }
            else
            {
                RenderSettings.fogEndDistance = 15f;
                RenderSettings.fogDensity = 0.55f;
            }*/

        }
    }

    public void ChangeFogDensity(float den)
    {
        print("fog density " + den);
        RenderSettings.fogDensity = den;
    }

    void OnDestroy()
    {
        GameInstance.progress -= OnProgressChanged;
    }


}
