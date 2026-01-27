
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DayNightChange : MonoBehaviour
{
    [Range(0.01f, 0.5f)]
    [SerializeField] float timeframe = 0.1f;
    [SerializeField] Material dayMaterial;
    [SerializeField] Material nightMaterial;
    bool isDay = true;
    [SerializeField] Texture dayTexture;
    [SerializeField] List<Texture> dayNightTransitionTextures = new List<Texture>();
    int transitionCount = 0, transitionTintCount = 60, transitionTintCountToNight = 0;

    [SerializeField] AnimationCurve skyTintGraph, ambientColorShift, fogDensityGraph;
    [SerializeField] Color testColor;
    bool onceReset = false, corouting = false;
    public bool isDungeon { get; set; }
    public UnityEvent<int, int, int> gameClock;

    public bool isNight { get; set; }

    private void Awake()
    {
        GameInstance.dayNightChange = this;
        GameInstance.progress += OnProgressChanged;
        transitionCount = dayNightTransitionTextures.Count-1;
    }

    void ChangeTimeFlow()
    {
        GameInstance.ChangeTimeFlow(timeframe);
    }
    
    public void ChangeTimeFlow(float _newTime)
    {
        timeframe = _newTime;
    }

    public void InitDayNightShift()
    {
        print("check day or night");
        if (isDungeon)
        {
            isDay = false;
            RenderSettings.fog = true;
            RenderSettings.skybox.SetVector("_Tint", new Vector4(0.4f, 0.4f, 0.4f, 1));
            RenderSettings.skybox.SetTexture("_MainTex", dayNightTransitionTextures[dayNightTransitionTextures.Count - 1]);
            RenderSettings.ambientLight = Color.black;
            RenderSettings.fogColor = RenderSettings.ambientLight;
            RenderSettings.fogDensity = 0.18f; return; 
        }

        if (GameInstance.playerController.GetBattleGroundEnvironment() == BattleGroundEnvironment.CITY
            || GameInstance.playerController.GetBattleGroundEnvironment() == BattleGroundEnvironment.STONE
            || GameInstance.playerController.GetBattleGroundEnvironment() == BattleGroundEnvironment.WOOD)
        {

            if (GameInstance.GetNormalTime()[1] % 24 >= 6 && GameInstance.GetNormalTime()[1] % 24 < 20 && RenderSettings.fogDensity !=0)
            {
                if (corouting) return;
                isDay = false;
                RenderSettings.fog = false;
                RenderSettings.skybox.SetVector("_Tint", new Vector4(1, 1, 1, 1));
                RenderSettings.skybox.SetTexture("_MainTex", dayNightTransitionTextures[0]);
                RenderSettings.ambientLight = Color.white;
                RenderSettings.fogColor = RenderSettings.ambientLight;
                RenderSettings.fogDensity = 0;
            }
            else
            {
                if (corouting) return;
                isDay = false;
                RenderSettings.fog = true;
                RenderSettings.skybox.SetVector("_Tint", new Vector4(0.5f, 0.5f, 0.5f, 1));
                RenderSettings.skybox.SetTexture("_MainTex", dayNightTransitionTextures[dayNightTransitionTextures.Count - 1]);
                RenderSettings.ambientLight = Color.black;
                RenderSettings.fogColor = RenderSettings.ambientLight;
                RenderSettings.fogDensity = 0.16f;
            }
        }
        else
        {
            if (corouting) return;
            isDay = false;
            RenderSettings.fog = true;
            RenderSettings.skybox.SetVector("_Tint", new Vector4(0.5f, 0.5f, 0.5f, 1));
            RenderSettings.skybox.SetTexture("_MainTex", dayNightTransitionTextures[dayNightTransitionTextures.Count - 1]);
            RenderSettings.ambientLight = Color.black;
            RenderSettings.fogColor = RenderSettings.ambientLight;
            RenderSettings.fogDensity = 0.16f;
        }
    }


    private void OnProgressChanged(int countdown)
    {
        gameClock.Invoke(GameInstance.GetNormalTime()[2], GameInstance.GetNormalTime()[1] % 24, GameInstance.GetNormalTime()[0] % 60);
        ChangeTimeFlow();
        isNight = NightClosed(countdown);
        //print(GameInstance.GetNormalTime()[1].ToString() + ":" + GameInstance.GetNormalTime()[2].ToString()+":"+GameInstance.GetNormalTime()[3].ToString());

        if (GameInstance.playerController.GetBattleGroundEnvironment() == BattleGroundEnvironment.CITY
            || GameInstance.playerController.GetBattleGroundEnvironment() == BattleGroundEnvironment.STONE
            || GameInstance.playerController.GetBattleGroundEnvironment() == BattleGroundEnvironment.WOOD)
        {
            DayChange(countdown);
            if (GameInstance.GetNormalTime()[1] % 24 >= 6 && GameInstance.GetNormalTime()[1] % 24 < 19)
            {
                isDay = true;
                //RenderSettings.fog = false;
            }
            else
            {
                isDay = false;
                //RenderSettings.fog = true;
            }
        }
        else
        {
            //In caves and dungeons, always night
            //isDay = false;
            //RenderSettings.fog = true;
            RenderSettings.fogColor = Color.black;
            RenderSettings.fogDensity = 0.16f;
        }



    }


    void DayChange(int _count)
    {
        /*    FLOAT VARS IN SKYBOX MATERIAL    
        _Exposure
        _Rotation
        _Mapping
        _ImageType
        _MirrorOnBack
        _Layout

        */


/*        Material skyMat = RenderSettings.skybox;
        var propSky = skyMat.GetPropertyNames(MaterialPropertyType.Float);*/
        /*        foreach (string propName in propSky)
                {
                    print(propName);
                }*/
        RenderSettings.skybox.SetFloat("_Rotation", _count%360);
        StartCoroutine(SmoothSkyRotation(GameInstance.GetTimeFlow(), 10));

        //print("time in hours "+GameInstance.GetNormalTime()[0]%60 +" "+ GameInstance.GetNormalTime()[1]%24);


        if (GameInstance.GetNormalTime()[1]%24 == 5)
        {

            float dayShift = skyTintGraph.Evaluate(transitionTintCount);
            //print(transitionTintCount + "/"+ dayShift);
            RenderSettings.skybox.SetVector("_Tint", new Vector4(dayShift, dayShift, dayShift, 1));
            RenderSettings.ambientLight = new Color(ambientColorShift.Evaluate(transitionTintCount), ambientColorShift.Evaluate(transitionTintCount), ambientColorShift.Evaluate(transitionTintCount), 1);
            RenderSettings.fogColor = RenderSettings.ambientLight;
            RenderSettings.fogDensity = fogDensityGraph.Evaluate(transitionTintCount);
            testColor = RenderSettings.skybox.GetVector("_Tint");
            transitionTintCount--;
            if (transitionTintCount < 0) transitionTintCount = 0;

            if (transitionCount > 0 && transitionTintCount%5==0)
            {
                transitionCount--;
                RenderSettings.skybox.SetTexture("_MainTex", dayNightTransitionTextures[transitionCount]);
            }


        }


        if (GameInstance.GetNormalTime()[1] % 24 == 19)
        {

            float dayShift = skyTintGraph.Evaluate(transitionTintCount);
            RenderSettings.skybox.SetVector("_Tint", new Vector4(dayShift, dayShift, dayShift, 1));
            RenderSettings.ambientLight = new Color(ambientColorShift.Evaluate(transitionTintCount), ambientColorShift.Evaluate(transitionTintCount), ambientColorShift.Evaluate(transitionTintCount), 1);
            RenderSettings.fogDensity = fogDensityGraph.Evaluate(transitionTintCount);
            RenderSettings.fogColor = RenderSettings.ambientLight;
            testColor = RenderSettings.skybox.GetVector("_Tint");
            transitionTintCount++;
            if (transitionTintCount > 60) transitionTintCount = 60;



            if (transitionCount < dayNightTransitionTextures.Count && transitionTintCount % 5 == 0)
            {
                RenderSettings.skybox.SetTexture("_MainTex", dayNightTransitionTextures[transitionCount]);
                transitionCount++;
                if (transitionCount >= dayNightTransitionTextures.Count)
                {
                    transitionCount = dayNightTransitionTextures.Count - 1;
                }
            }


        }

            


    }



    public void ChangeFogDensity(float den)
    {
        RenderSettings.fogDensity = den;
    }

    void OnDestroy()
    {
        GameInstance.progress -= OnProgressChanged;
    }

    IEnumerator SmoothSkyRotation(float timeFlow, int division)
    {
        corouting = true;
        float minTime = timeFlow / (float)division;

        for(int i = 0; i < division; i++)
        {
            //print("rotation "+ RenderSettings.skybox.GetFloat("_Rotation"));
            float targetRotation = RenderSettings.skybox.GetFloat("_Rotation");
            RenderSettings.skybox.SetFloat("_Rotation", targetRotation + 1.0f/(float)division);
            yield return new WaitForSeconds(minTime);
        }
        corouting = false;
        yield return null;
    }

    public bool NightClosed(int count)
    {
        //print(GameInstance.GetNormalTime()[1].ToString() + ":" + GameInstance.GetNormalTime()[2].ToString()+":"+GameInstance.GetNormalTime()[3].ToString());
        if (GameInstance.GetNormalTime()[1] % 24 >= 6 && GameInstance.GetNormalTime()[1] % 24 < 20)
        {
            return  false;
        }
        else
        {
            return true;

        }
    }

}
