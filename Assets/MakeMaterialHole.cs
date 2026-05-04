using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeMaterialHole : MonoBehaviour
{
    Texture2D texture;
    public int res = 100;   

    void Start()
    {

        this.texture = new Texture2D(res, res, TextureFormat.RGBA32,false);
           GetComponent<Renderer>().material.mainTexture = texture;     

        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {


                     this.texture.SetPixel(x, y, Color.black);
            }
        }

        this.texture.Apply();
        StartCoroutine(delayHole());

    }

    Vector2 MapCoordinatesToTexture(Vector3 worldPos)
    {
        float x = worldPos.x + 250; // Assuming the world coordinates range from -250 to 250
        x /= 250; // Normalize to 0-1
        x *= res; // Scale to texture resolution
        x += (res / 2); // Center the texture
        float y = worldPos.z + 250; // Assuming the world coordinates range from -250 to 250
        y /= 250; // Normalize to 0-1
        y *= res; // Scale to texture resolution
        y += (res / 2); // Center the texture
        return new Vector2(x, y);
    }

    public void MakeHole(Vector2 pos, float radius)
    {
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                if (Vector2.Distance(pos, new Vector2(x, y)) < radius)
                {
                    this.texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        this.texture.Apply();
    }


    IEnumerator delayHole()
            {
        yield return new WaitForSeconds(5f);


            MakeHole(MapCoordinatesToTexture(GameInstance.playerController.transform.position), 5);
        
    }


}
