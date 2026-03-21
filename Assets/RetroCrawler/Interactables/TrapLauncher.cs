using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapLauncher : MonoBehaviour
{

    [SerializeField] GameObject missilePrefab;

    public void LaunchMissile()
    {
       var mis = Instantiate(missilePrefab, transform);
    }

}
