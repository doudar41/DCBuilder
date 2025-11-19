using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyBillboard : MonoBehaviour
{
    [SerializeField] GameObject picture;
    void Update()
    {
        picture.transform.LookAt(new Vector3(GameInstance.playerController.gameObject.transform.position.x, picture.transform.position.y, GameInstance.playerController.gameObject.transform.position.z));


    }
}
