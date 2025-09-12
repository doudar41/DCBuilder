using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellShop : MonoBehaviour
{
    [SerializeField] Image backGroundImage;
    [SerializeField] List<ItemShopSlot> spellSlots = new List<ItemShopSlot>();
    [SerializeField] List<MagicType> magicTypes = new List<MagicType>();
    [SerializeField] int shopIndex = 1;
    [SerializeField] Camera cam;

    private void OnEnable()
    {
       // NewItems();
        cam.depth = 1;

    }
    private void Start()
    {
        //NewItems();
    }

    public void NewItems()
    {
        for (int i = 0; i < spellSlots.Count; i++)
        {
            spellSlots[i].SetSpellToSell(RandomItemsToSell(magicTypes[Random.Range(0, magicTypes.Count)]));
        }
    }

    public void CameraOut()
    {
        cam.depth = -2;
    }

    public SpellContainer RandomItemsToSell(MagicType magicType)
    { 
        List<SpellContainer> spellOfType = new List<SpellContainer>();

        foreach (SpellContainer spell in GameInstance.dataBase.GetAllSpells())
        {
            if (spell.spells[0].magicType == magicType)
            {
                spellOfType.Add(spell);
            }
        }

        return spellOfType[Random.Range(0, spellOfType.Count)];
    }

    public void CloseShop()
    {
        CameraOut();
        GameInstance.playerController.shopIsOpened = false;
        gameObject.SetActive(false);

    }



}
