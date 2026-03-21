using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebuffIcon : MonoBehaviour
{
    Image image;
    [SerializeField] GameplayStates debuffType;

    private void Awake()
    {
        image = GetComponent<Image>();
    }


    public GameplayStates GetDebuffType()
    {
        return debuffType;
    }


    public void OnOffState(bool on)
    {
        gameObject.SetActive(on);
    }
}
