using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] Transform alfaTransform;


    public void GetEnemyHealth(float amountNormilized)
    {
        alfaTransform.localScale = new Vector3(amountNormilized, alfaTransform.localScale.y, alfaTransform.localScale.z);
    }
}
