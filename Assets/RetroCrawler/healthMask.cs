using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


public class healthMask : MonoBehaviour
{
    [SerializeField] RectMask2D mask2D;
    [SerializeField] Rect rect;

    [SerializeField] float topValue, maxValue;
    float rectHeight;


    void Start()
    {

        rectHeight = rect.height;
        
    }

    private void Update()
    {
        
    }

    public void ChangeProgressValue(float x)
    {
        float value = Mathf.Clamp(x, 0, maxValue);
        topValue = rectHeight * value/maxValue;
        mask2D.padding = new Vector4(0, 0, 0, topValue);
    }


}
