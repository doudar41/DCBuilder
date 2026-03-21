
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Events;

public class torchesVisibility : MonoBehaviour
{
    public UnityEvent<float> maxDistanceToTorch;
    float distanceMax = 0f;
    [SerializeField] CapsuleCollider triggerCollider;
    [SerializeField] float maxFogDensity = 0.33f, minFogDensity = 0.16f;
    List<Light> lights = new List<Light>();
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.GetComponentInChildren<Light>() != null) 
        {
            if (!lights.Contains(other.gameObject.GetComponentInChildren<Light>())) 
            { lights.Add(other.gameObject.GetComponentInChildren<Light>()); }

            other.gameObject.GetComponentInChildren<Light>().enabled = true;
        }
    }

    public  void OnStep()
    {
/*        float minDistance = 0f;
        foreach (Light light in lights)
        {
            float distance = Vector3.Distance(light.transform.position, GameInstance.playerController.transform.position);
            if (distance > distanceMax) distanceMax = distance;
            if(minDistance == 0f) minDistance = distance;
        }
        print("find light " + distanceMax);
        if (lights.Count == 0) minDistance = triggerCollider.radius;
        maxDistanceToTorch.Invoke(0.16f);*/
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponentInChildren<Light>() != null) 
        { 
            other.gameObject.GetComponentInChildren<Light>().enabled = false;
            if (lights.Contains(other.gameObject.GetComponentInChildren<Light>())) lights.Remove(other.gameObject.GetComponentInChildren<Light>());
            //print("lose light "+ lights.Count);
            if (lights.Count == 0) distanceMax = 0.55f;
        }
    }


    float ClampDistance(float distance)
    {
        float maxRadius = triggerCollider.radius;
        float _delta = (distance / maxRadius)*maxFogDensity;

        _delta -= lights.Count*0.1f;
        print("delta " + _delta);
        if (1-_delta < minFogDensity) return minFogDensity;
        if (1-_delta > maxFogDensity) return maxFogDensity;
        return _delta;
    }

}
