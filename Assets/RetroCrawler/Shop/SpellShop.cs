using Ami.BroAudio;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SpellShop : MonoBehaviour
{
    [SerializeField] Image backGroundImage;
    [SerializeField] List<SpellShopSlot> spellSlots = new List<SpellShopSlot>();
    [SerializeField] List<MagicType> magicTypes = new List<MagicType>();
    [SerializeField] TextMeshProUGUI heroesGemsText;
    [SerializeField] float sellMultiplier = 1;
    [SerializeField] Vector2Int itemsLevel = new Vector2Int(0,1);
    [SerializeField] CameraOrder cam;
    [SerializeField] TextMeshProUGUI textOfShopState;
    [SerializeField] SoundID openDoor, closeDoor, openSpellShopVO, closeSpellShopVO;
    [SerializeField] GameObject openSwitch;
    [SerializeField] GameObject spellsShelf, exitButton, moneyPanel;

    int gemsSpent = 0;


    public UnityEvent closeShopPanel;


    public void OpenSpellShop()
    {
        backGroundImage.enabled = true;
        openSwitch.SetActive(true);

        var gemAmount = GameInstance.party.CheckGems(0);
        heroesGemsText.text = gemAmount.ToString();

        textOfShopState.text = "Spells";
        exitButton.SetActive(true);
        moneyPanel.SetActive(true);
        spellsShelf.SetActive(false);
        BroAudio.Play(openDoor);
    }


    public void OpenSpellShelf()
    {
        spellsShelf.SetActive(true);
    }


    public void PlayerGems(int coins)
    {
        gemsSpent = coins;
    }
    public void RefreshPlayersCoins()
    {
        var gemAmount = GameInstance.party.CheckGems(0);
        heroesGemsText.text = gemAmount.ToString();
    }

    public void RefreshSoldSpells()
    {
        //print("Refreshing sold spells");
        for (int i = 0; i < spellSlots.Count; i++)
        {
            spellSlots[i].SetSpellToSell(RandomSpellToSell(magicTypes[Random.Range(0, magicTypes.Count)]));
        }
    }

    public void CameraOut()
    {
        cam.BattleLogWithGameplay();
    }

    public SpellContainer RandomSpellToSell(MagicType magicType)
    { 
        List<SpellContainer> spellOfType = new List<SpellContainer>();

        foreach (SpellContainer spell in GameInstance.dataBase.GetAllSpells())
        {
            if (spell.spells[0].magicType == magicType)
            {
                if (spell.spellLevel>= itemsLevel.x && spell.spellLevel <= itemsLevel.y)
                {
                    spellOfType.Add(spell);
                }
            }
        }
        return spellOfType[Random.Range(0, spellOfType.Count)];
    }


    public void CloseShop()
    {
        if (spellsShelf.activeSelf) { spellsShelf.SetActive(false); return; }

        closeShopPanel.Invoke();
        CameraOut();
        GameInstance.playerController.shopIsOpened = false;
        gameObject.SetActive(false);

        BroAudio.Play(closeDoor);
        BroAudio.Play(closeSpellShopVO);

        BroAudio.Stop(openSpellShopVO);
        backGroundImage.enabled = false;
        openSwitch.SetActive(false);
    }

    public void ClearSlots()
    {
        for (int i = 0; i < spellSlots.Count; i++)
        {

            spellSlots[i].ClearSlot();
        }
    }


}
