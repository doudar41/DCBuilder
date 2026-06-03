using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundRoom : MonoBehaviour
{
    [SerializeField] AudioListener mainListener, soundroomListener;
    [SerializeField] List<GameObject> barSources = new List<GameObject>();
    [SerializeField] List<GameObject> weaponShopSources = new List<GameObject>();
    [SerializeField] List<GameObject> accessoriesShopSources = new List<GameObject>();
    [SerializeField] List<GameObject> templeShopSources = new List<GameObject>();
    [SerializeField] List<GameObject> elementalShopSources = new List<GameObject>();
    [SerializeField] List<GameObject> lightShopSources = new List<GameObject>();
    [SerializeField] List<GameObject> darkShopSources = new List<GameObject>();
    [SerializeField] List<GameObject> caveEntranceSources = new List<GameObject>();

    void MainLocalListenerSwitch(bool onOff)
    {
        if (onOff) 
        { mainListener.enabled = false; soundroomListener.enabled = true; } 
        else 
        { mainListener.enabled = true; soundroomListener.enabled = false; }
    }


    public void SwitchSoundRoom(RoomSpaces roomSpaces, bool onoff)
    {
        MainLocalListenerSwitch(onoff);
        switch (roomSpaces)
        {
            case RoomSpaces.None:
                break;
            case RoomSpaces.Bar:
                transform.position = barSources[0].transform.position;
                foreach (GameObject go in barSources)
                {
                    go.SetActive(onoff);
                    if(go.GetComponent<AudioSource>() != null)
                    {
                       if(onoff) go.GetComponent<AudioSource>().Play();
                       else go.GetComponent<AudioSource>().Stop();
                    }
                }
                break;
            case RoomSpaces.WeaponShop:
                transform.position = weaponShopSources[0].transform.position;
                foreach (GameObject go in weaponShopSources)
                {
                    go.SetActive(onoff);
                    if (go.GetComponent<AudioSource>() != null)
                    {
                        if (onoff) go.GetComponent<AudioSource>().Play();
                        else go.GetComponent<AudioSource>().Stop();
                    }
                }
                break;
            case RoomSpaces.AccessoriesShop:
                transform.position = accessoriesShopSources[0].transform.position;
                foreach (GameObject go in accessoriesShopSources)
                    { go.SetActive(onoff);
                    if (go.GetComponent<AudioSource>() != null)
                    {
                        if (onoff) go.GetComponent<AudioSource>().Play();
                        else go.GetComponent<AudioSource>().Stop();
                    }
                }
                break;
            case RoomSpaces.TempleShop:
                transform.position = templeShopSources[0].transform.position;
                foreach (GameObject go in templeShopSources)
                    { go.SetActive(onoff);
                    if (go.GetComponent<AudioSource>() != null)
                    {
                        if (onoff) go.GetComponent<AudioSource>().Play();
                        else go.GetComponent<AudioSource>().Stop();
                    }
                }
                break;
            case RoomSpaces.ElementalShop:
                transform.position = elementalShopSources[0].transform.position;
                foreach (GameObject go in elementalShopSources)
                    { go.SetActive(onoff);
                    if (go.GetComponent<AudioSource>() != null)
                    {
                        if (onoff) go.GetComponent<AudioSource>().Play();
                        else go.GetComponent<AudioSource>().Stop();
                    }
                }
                break;
            case RoomSpaces.LightShop:
                transform.position = lightShopSources[0].transform.position;
                foreach (GameObject go in lightShopSources)
                    { go.SetActive(onoff);
                    if (go.GetComponent<AudioSource>() != null)
                    {
                        if (onoff) go.GetComponent<AudioSource>().Play();
                        else go.GetComponent<AudioSource>().Stop();
                    }
                }
                break;
            case RoomSpaces.DarkShop:
                transform.position = darkShopSources[0].transform.position;
                foreach (GameObject go in darkShopSources)
                    { go.SetActive(onoff);
                    if (go.GetComponent<AudioSource>() != null)
                    {
                        if (onoff) go.GetComponent<AudioSource>().Play();
                        else go.GetComponent<AudioSource>().Stop();
                    }
                }
                break;
            case RoomSpaces.CaveEntrance:
                transform.position = caveEntranceSources[0].transform.position;
                foreach( GameObject go in caveEntranceSources)
                    { go.SetActive(onoff);
                    if (go.GetComponent<AudioSource>() != null)
                    {
                        if (onoff) go.GetComponent<AudioSource>().Play();
                        else go.GetComponent<AudioSource>().Stop();
                    }
                }

                break;
        }
    }
}

public enum RoomSpaces
{
    None,
    Bar,
    WeaponShop,
    AccessoriesShop,
    TempleShop,
    ElementalShop,
    LightShop,
    DarkShop,
    KingsCastle,
    CaveEntrance,
    TrainingGrounds

}