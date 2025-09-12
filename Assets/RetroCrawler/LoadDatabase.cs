using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadDatabase : MonoBehaviour
{
    [SerializeField] GameObject databasePrefab;

    void Awake()
    {
        Database database = FindAnyObjectByType<Database>();
        if(database == null)
        {
            GameObject data = Instantiate(databasePrefab, transform);
        }
    }


}
