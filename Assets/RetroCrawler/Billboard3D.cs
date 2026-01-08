using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard3D : MonoBehaviour
{
    [SerializeField] SpriteRenderer picture;
    [SerializeField] GameObject rotationHead;
    [SerializeField] Sprite[] angleSprites;
    int currentAngle = -1;

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(this.transform.position, GameInstance.playerController.gameObject.transform.position) > 20) return;
        rotationHead.transform.LookAt(new Vector3(GameInstance.playerController.gameObject.transform.position.x, rotationHead.transform.position.y, GameInstance.playerController.gameObject.transform.position.z));

        print("rotation " + ((int)(rotationHead.transform.rotation.eulerAngles.y / 45)));
        if(currentAngle != (int)(rotationHead.transform.rotation.eulerAngles.y / 45))
        {
            currentAngle = (int)(rotationHead.transform.rotation.eulerAngles.y / 45);
            picture.sprite = angleSprites[currentAngle];
        }

        rotationHead.transform.rotation = Quaternion.Euler(rotationHead.transform.rotation.eulerAngles.x, 45 * ((int)(rotationHead.transform.rotation.eulerAngles.y / 45)), rotationHead.transform.rotation.eulerAngles.z);


    }
}
