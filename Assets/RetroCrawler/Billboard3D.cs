using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard3D : MonoBehaviour
{
    [SerializeField] SpriteRenderer picture;
    [SerializeField] GameObject rotationHead;
    [SerializeField] List<Sprite> angleSprites;
    int angleMod = 0;
    int currentAngle = -1;
    [SerializeField] CardinalDirections direction;
    [SerializeField] BoxCollider col;
    bool animationPlaying = false;
    private void Start()
    {
        switch (direction)
        {
            case CardinalDirections.NORTH:
                angleMod = 0;
                break;
            case CardinalDirections.SOUTH:
                angleMod = 6;
                break;
            case CardinalDirections.EAST:
                if (col != null)
                {
                    var bo = col.size;
                    float z = col.size.z;
                    bo.z = bo.x;
                    bo.x = z;
                    col.size = bo;
                }
                angleMod = 7;
                break;
            case CardinalDirections.WEST:
                if (col != null)
                {
                    var _bo = col.size;
                    float _z = col.size.z;
                    _bo.z = _bo.x;
                    _bo.x = _z;
                    col.size = _bo;
                }
                angleMod = 5;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (animationPlaying) return;
        if (Vector3.Distance(this.transform.position, GameInstance.playerController.gameObject.transform.position) > 15) return;
        rotationHead.transform.LookAt(new Vector3(GameInstance.playerController.gameObject.transform.position.x, rotationHead.transform.position.y, GameInstance.playerController.gameObject.transform.position.z));
        float realangle = rotationHead.transform.rotation.eulerAngles.y;
        //rotationHead.transform.rotation = Quaternion.Euler(rotationHead.transform.rotation.eulerAngles.x, 90 * ((int)(realangle / 90)), rotationHead.transform.rotation.eulerAngles.z);

        //print(name + " picture index " + currentAngle + " real angle " + realangle);
        if (realangle % 360 > 315 && realangle % 360 <= 360 || realangle % 360 >= 0 && realangle % 360 < 45)
        { picture.gameObject.transform.rotation = Quaternion.Euler(picture.gameObject.transform.rotation.x, 0 ,picture.gameObject.transform.rotation.z);  currentAngle = 0; }
        if (realangle % 360 >= 45 && realangle % 360 < 135) 
        { picture.gameObject.transform.rotation = Quaternion.Euler(picture.gameObject.transform.rotation.x, 90, picture.gameObject.transform.rotation.z); currentAngle = 1; }
        if (realangle % 360 >= 135 && realangle % 360 <= 225) 
        { picture.gameObject.transform.rotation = Quaternion.Euler(picture.gameObject.transform.rotation.x, 180, picture.gameObject.transform.rotation.z); currentAngle = 2; }
        if (realangle % 360 > 225 && realangle % 360 <= 315) 
        { picture.gameObject.transform.rotation = Quaternion.Euler(picture.gameObject.transform.rotation.x, 270, picture.gameObject.transform.rotation.z); currentAngle = 3; }
        int angResult=currentAngle;
        if (angleMod>1) angResult = (currentAngle + angleMod) % 4 ;
        picture.sprite = angleSprites[angResult];

    }

    public void AnimationPlaying(bool onOff)
    {
        animationPlaying = onOff;
    }
    public void ReplaceSprite( List<Sprite> _sprites)
    {
        //print("change sprites");
        angleSprites.Clear();
        angleSprites.AddRange(_sprites);

    }
}
