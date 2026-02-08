using Ami.BroAudio;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SpellShop : MonoBehaviour
{
    [SerializeField] Image backGroundImage;
    [SerializeField] List<SpellShopSlot> spellSlots = new List<SpellShopSlot>();
    [SerializeField] SkillsStat magicType = SkillsStat.ElementalMagic;
    [SerializeField] TextMeshProUGUI heroesGemsText;
    [SerializeField] float sellMultiplier = 1;
    [SerializeField] Vector2Int itemsLevel = new Vector2Int(0,1);
    [SerializeField] CameraOrder cam;
    [SerializeField] TextMeshProUGUI textOfShopState;
    [SerializeField] SoundID openDoor, closeDoor, openSpellShopVO, closeSpellShopVO;
    [SerializeField] GameObject spellsShelf, exitButton, moneyPanel;

    int gemsSpent = 0;


    public UnityEvent closeShopPanel;


    public void OpenSpellShop()
    {
        backGroundImage.enabled = true;
        var gemAmount = GameInstance.party.CheckGems(0);
        heroesGemsText.text = gemAmount.ToString();

        textOfShopState.text = "Spells";
        exitButton.SetActive(true);
        moneyPanel.SetActive(true);
        spellsShelf.SetActive(false);
        GameInstance.soundManagerInGame.ProtectedPlay(openDoor);
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
        itemsLevel.y = (GameInstance.party.GetPartyLevel()/5) + 1;
        List<SpellContainer> spellOfType = new List<SpellContainer>();

        foreach (SpellContainer spell in GameInstance.dataBase.GetAllSpells())
        {
            if (spell.spells[0].skillToCheckInCalculations == magicType)
            {
                if (spell.spellLevel >= itemsLevel.x && spell.spellLevel <= itemsLevel.y)
                {
                    //print("found spell of type " + spell.spellLevel);
                    spellOfType.Add(spell);
                }
            }
        }
        print("found spells" + spellOfType.Count);
        if (spellOfType.Count == 0) return;
        for (int i = 0; i < spellSlots.Count; i++)
        {
            spellSlots[i].SetSpellToSell(spellOfType[Random.Range(0, spellOfType.Count - 1)]);
        }
    }

    public void CameraOut()
    {
        cam.BattleLogWithGameplay();
    }

    public SpellContainer RandomSpellToSell( List<SpellContainer> spellOfType)
    { 

        print("item level " + itemsLevel.y + " spell of type count " + spellOfType.Count);
        if (spellOfType.Count <= 0) { return null; }
        return spellOfType[Random.Range(0, spellOfType.Count-1)];
    }


    public void CloseShop()
    {
        if (spellsShelf.activeSelf) { spellsShelf.SetActive(false); return; }

        closeShopPanel.Invoke();
        CameraOut();
        GameInstance.playerController.shopIsOpened = false;
        gameObject.SetActive(false);

        GameInstance.soundManagerInGame.ProtectedPlay(closeDoor);
        GameInstance.soundManagerInGame.ProtectedPlay(closeSpellShopVO);

        BroAudio.Stop(openSpellShopVO);
        backGroundImage.enabled = false;
    }

    public void ClearSlots()
    {
        for (int i = 0; i < spellSlots.Count; i++)
        {

            spellSlots[i].ClearSlot();
        }
    }


}
