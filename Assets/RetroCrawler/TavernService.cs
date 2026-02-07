using Ami.BroAudio;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TavernService : MonoBehaviour
{
    [SerializeField] GameObject backGroundImage;
    [SerializeField] GameObject tavernButtons, heroesMoney;
    [SerializeField] List<TextMeshProUGUI> heroesCoinsText;
    [SerializeField] SoundID closeDoor, closeTavernVO, openTavernVO, background, backgroundMusic;
    [SerializeField] int coinsForDrink = 5, rentForRoom = 10, buyFood = 10;
    [SerializeField] CameraOrder cameraOrder;
    [SerializeField] GameObject restAnimation;
    public UnityEvent exitTavern;


    public void OpenTavern()
    {
        backGroundImage.SetActive(true);
        tavernButtons.SetActive(true);
        //GameInstance.soundManagerInGame.DuckingCurrentMusic(background);
        GameInstance.soundManagerInGame.DuckExploreMusicSwitchToAmbience(RoomSpaces.Bar);
        GameInstance.soundManagerInGame.ProtectedPlay(closeDoor);
        heroesMoney.SetActive  (true);
        GetPlayersCoins();
    }

    public void BuyADrink()
    {
       if(GameInstance.party.SellBuyMoneyCheck(coinsForDrink) >= 0)
        {
            // add gossip tip to journal or buff
        }
    }

    public void RentARoom()
    {
        if (restAnimation.activeSelf) return;
        
        if (GameInstance.party.SellBuyMoneyCheck(rentForRoom) >= 0)
        {
            restAnimation.SetActive(true);
            restAnimation.GetComponent<animateUIImage>().StartAnimation();
            GameInstance.dayNightChange.ChangeTimeFlow(0.001f);
            GameInstance.party.AddSomeFood(0);
            GameInstance.party.MoneyGoes(rentForRoom);
            GetPlayersCoins();
        }
    }

    private void Update()
    {
        if (restAnimation.activeSelf)
        {
            int hour = GameInstance.GetNormalTime()[1]%24;
            int minute = GameInstance.GetNormalTime()[0]%60;
            print("checking sleep" + hour+" "+ minute);
            if (hour == 5 && (minute >0 && minute <5))
            {
                restAnimation.GetComponent<animateUIImage>().StopAnimation();
                restAnimation.SetActive(false);
                foreach(Hero h in GameInstance.party.GetHeroList())
                {
                    h.HealthDecrease(-h.GetMaxDependedStat(DependedStat.maxHealth));
                }
                GameInstance.dayNightChange.ChangeTimeFlow(0.5f);
            }
        }
    }


    public void BuyFood()
    {
        if (GameInstance.party.SellBuyMoneyCheck(buyFood) >= 0)
        {
            GameInstance.party.AddSomeFood(100);
            GameInstance.party.MoneyGoes(buyFood);
            GetPlayersCoins();
        }
    }

    public void GetPlayersCoins()
    {
        var money = GameInstance.party.GetCoinsForUI();
        for (int i = 0; i < money.Count; i++)
        {
            heroesCoinsText[i].text = money[i].ToString();
        }
    }


    public void CloseTavern()
    {
        backGroundImage.SetActive(false);
        GameInstance.soundManagerInGame.ProtectedPlay(closeDoor);
        GameInstance.soundManagerInGame.ProtectedPlay(closeTavernVO);
        BroAudio.Stop(openTavernVO);
        tavernButtons.SetActive(false);
        exitTavern.Invoke();
        GameInstance.playerController.shopIsOpened = false;
        GameInstance.soundManagerInGame.UnduckingCurrentMusic(background);
        cameraOrder.BattleLogWithGameplay();
        heroesMoney.SetActive(false);
        GameInstance.soundManagerInGame.UnDuckExploreMusicSwitchToAmbience();
    }

}
