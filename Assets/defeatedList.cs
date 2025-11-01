using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class defeatedList : MonoBehaviour
{

    [SerializeField] List<Transform> listDefeated = new List<Transform>();

    private void Start()
    {
        ClearList();
    }


    public void ClearList()
    {
        foreach (Transform t in listDefeated)
        {
            if (t.childCount != 0)
            {
                foreach(GameObject g in t)
                {
                    DestroyImmediate(g);
                }
            }
        }
    }

    public void AddToList(GameObject enemy)
    {
        foreach(Transform t in listDefeated)
        {
            if (t.childCount == 0)
            {
                enemy.transform.parent = t;
                enemy.transform.localPosition = Vector3.zero;
            }
        }
    }

}
