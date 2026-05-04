using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWarTexture : MonoBehaviour
{

    [SerializeField] Material brushMaterial;
    [SerializeField] Material fogMaterial, accumulateMaterial;

    [SerializeField] Texture2D brushTexture;

    RenderTexture visibilityRT;
    RenderTexture explorationRT;


    [SerializeField] Vector2 worldOrigin;
    [SerializeField] Vector2 worldSize;

    [SerializeField] private float fogHeight = 0.1f;
    [SerializeField] public float revealRadiusWorld = 3f;
    [SerializeField] int resolution = 512;

    void Awake()
    {


        worldOrigin.x -= (worldSize.x * 0.5f);
        worldOrigin.y -= (worldSize.y * 0.5f);
        SetupQuad();
        SetupTextures();
        fogMaterial.SetTexture("_VisibilityTex", visibilityRT);
        fogMaterial.SetTexture("_ExplorationTex", explorationRT);

        brushMaterial.SetTexture("_BrushTex", brushTexture);
        fogMaterial.SetVector("_WorldOrigin", new Vector4(worldOrigin.x, 0, worldOrigin.y, 0));
        fogMaterial.SetVector("_WorldSize", new Vector4(worldSize.x, 0, worldSize.y, 0));
    }

    void SetupTextures()
    {
        visibilityRT = CreateRT();
        explorationRT = CreateRT();
    }

    RenderTexture CreateRT()
    {
        var rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32);
        rt.Create();

        RenderTexture.active = rt;
        GL.Clear(false, true, Color.black);
        RenderTexture.active = null;

        return rt;
    }

    void SetupQuad()
    {
        Vector3 center = new Vector3(
            worldOrigin.x + worldSize.x * 0.5f,
            fogHeight,
            worldOrigin.y + worldSize.y * 0.5f
        );

        transform.position = center;

        transform.rotation = Quaternion.Euler(90f, 0f, 0f); // flat on XZ

        transform.localScale = new Vector3(worldSize.x, worldSize.y, 1f);
    }

    public void UpdateVisibility(Vector3 position)
    {
        RenderTexture.active = visibilityRT;
        GL.Clear(false, true, Color.black);
        Paint(position);
        RenderTexture.active = null;
        AccumulateExploration();
    }

    void Paint(Vector3 worldPos)
    {
        Vector2 uv = WorldToUV(worldPos);
        float uvRadius = revealRadiusWorld / worldSize.x;

        brushMaterial.SetVector("_Center", uv);
        brushMaterial.SetVector("_Size", new Vector2(uvRadius, uvRadius));

        Graphics.Blit(null, visibilityRT, brushMaterial);
    }

    public void RevealArea(Vector3 pos, Vector3 size)
    {
        RenderTexture.active = visibilityRT;
        GL.Clear(false, true, Color.black);
        
        RenderTexture.active = null;

        Vector2 uv = WorldToUV(pos);
        float uvRadiusX = size.x / worldSize.x;
        float uvRadiusY = size.z / worldSize.y;
        brushMaterial.SetVector("_Center", uv);
        brushMaterial.SetVector("_Size", new Vector2(uvRadiusX, uvRadiusY));
        Graphics.Blit(null, visibilityRT, brushMaterial);
        AccumulateExploration();
    }


    void AccumulateExploration()
    {
        RenderTexture temp = RenderTexture.GetTemporary(explorationRT.descriptor);

        accumulateMaterial.SetTexture("_MainTex", visibilityRT);
        accumulateMaterial.SetTexture("_PrevTex", explorationRT);

        Graphics.Blit(null, temp, accumulateMaterial);
        Graphics.Blit(temp, explorationRT);

        RenderTexture.ReleaseTemporary(temp);
    }

    Vector2 WorldToUV(Vector3 w)
    {
        return new Vector2(
            (w.x - worldOrigin.x) / worldSize.x,
            (w.z - worldOrigin.y) / worldSize.y
        );
    }

}
