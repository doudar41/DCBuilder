using System.Collections;
using System.Collections.Generic;
using UnityEngine;using UnityEngine.UI;
using TMPro;

public class GetCardinalDirection : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI cardinalDir;
    [SerializeField]
    List<Sprite> compasSprites = new List<Sprite>();
    [SerializeField]
    Image compassImage;


    public void GetDir(CardinalDirections dir)
    {
        StartCoroutine(CompassAnim(dir));

    }

    IEnumerator CompassAnim(CardinalDirections dir)
    {
        foreach(Sprite s in compasSprites)
        {
            compassImage.sprite = s;
            yield return new WaitForSeconds(0.2f);
        }
        switch (dir)
        {
            case CardinalDirections.NORTH:
                cardinalDir.text = "N";
                break;
            case CardinalDirections.EAST:
                cardinalDir.text = "E";
                break;
            case CardinalDirections.SOUTH:
                cardinalDir.text = "S";
                break;
            case CardinalDirections.WEST:
                cardinalDir.text = "W";
                break;
        }
        yield return null;
    }
}
