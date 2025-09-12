using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyBillboard : MonoBehaviour
{
    [SerializeField] GameObject picture;
    void Update()
    {
        picture.transform.LookAt(GameInstance.playerController.gameObject.transform.position);
    }
}
