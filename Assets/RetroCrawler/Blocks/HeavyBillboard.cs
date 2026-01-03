using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyBillboard : MonoBehaviour
{
    [SerializeField] GameObject picture;



    void Update()
    {
        if (Vector3.Distance(this.transform.position, GameInstance.playerController.gameObject.transform.position) > 20) return;
        picture.transform.LookAt(new Vector3(GameInstance.playerController.gameObject.transform.position.x, picture.transform.position.y, GameInstance.playerController.gameObject.transform.position.z));
        picture.transform.rotation = Quaternion.Euler(picture.transform.rotation.eulerAngles.x, 45*((int)(picture.transform.rotation.eulerAngles.y/45)), picture.transform.rotation.eulerAngles.z);
        //choose sprite based on player angle relative to billboard

        /*        Quaternion rotAngle = Quaternion.FromToRotation(picture.transform.position, GameInstance.playerController.gameObject.transform.position );   //Angle(GameInstance.playerController.gameObject.transform.rotation, picture.transform.rotation);

                print( (int)(rotAngle.eulerAngles.y));*/

        //picture.transform.eulerAngles = new Vector3();
    }
}
