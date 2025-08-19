using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPack : MonoBehaviour
{
    GameObject[,] enemyformation = new GameObject[3,3];

    [SerializeField] List<GameObject> enemies = new List<GameObject>();
}
    