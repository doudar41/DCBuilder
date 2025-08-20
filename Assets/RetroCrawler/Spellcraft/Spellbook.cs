using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Spellbook : MonoBehaviour
{

    [SerializeField] List<SpellPage> pages = new List<SpellPage>(); //Check page for spell availability
    [SerializeField] Toggle spellbookSwitch;
    PlayerController playerController;
    SpellContainer spellWaitToRelease;
    [SerializeField] List<GameObject> objectToClose = new List<GameObject>();

    public UnityEvent<GameObject> spellTargetEvent;
    public UnityEvent<List<string>, List<string>> battlelogEvent;

    [SerializeField] Texture2D cursorTargetGraphics, cursorNormal;

    int lastOpenedPage = 0;
    public bool SpellCharged = false;

    [SerializeField] Camera mapCamera, mapMini;

    public Dictionary<Spell, int> spellTimeActive = new Dictionary<Spell, int>();

    [SerializeField] GameObject stateIconPanel;
    List<Image> massSpellIcons = new List<Image>();

    private void OnEnable()
    {
        GameInstance.spellbook = this;
        foreach (SpellPage sp in pages)
        {
            sp.InitializeSpellPage();
        }

    }
    private void Awake()
    {
        GameInstance.progress += TimeLimitSpellCount;
        foreach (Image i in stateIconPanel.transform.GetComponentsInChildren<Image>())
        {
            massSpellIcons.Add(i);
        }
    }
    private void OnDestroy()
    {
        GameInstance.progress -= TimeLimitSpellCount;

    }
    public void CloseSpellbook()
    {
        foreach (SpellPage sp in pages) 
        {
            sp.OpenSpellPage(false);
        }
        foreach(GameObject g in objectToClose)
        {
            g.SetActive(false);
        }
        spellbookSwitch.isOn = false;
        GameInstance.playerController.ExitHover();
    }

    public void GetPagesReady()
    {
        foreach(SpellPage sp in pages)
        {
            sp.SetPageAvailableSpells(GameInstance.party.activeHero.GetActiveHeroSpellbook());
        }
    }

    public bool SpellWaiting()
    {
        return spellWaitToRelease != null;
    }

    public void CastSpell(SpellContainer spellToCast)
    {
        
        if (spellToCast.gameplaySpell)
        {
            foreach (Spell s in spellToCast.spells)
            {
                switch (s.spellEffect)
                {
                    case SpellEffects.Recall:
                        if (GameInstance.playerController.playerState != PlayerState.Battle) print("recall mark");
                        //Get list of marked coordinates from active hero trasfer to this coordinates 
                        break;
                    case SpellEffects.WizardEye:
                        // open special signs on map
                        if (spellTimeActive.ContainsKey(s)) break;
                        mapCamera.cullingMask |= 1 << 10;
                        mapMini.cullingMask |= 1 << 10;
                        spellTimeActive.Add(s,s.numberOfTurns);
                        massSpellIcons[1].color = Color.white;
                        break;
                    case SpellEffects.Waterwalk:
                        if (spellTimeActive.ContainsKey(s)) 
                        {
                            spellTimeActive[s] = s.numberOfTurns;
                            break; 
                        }
                        spellTimeActive.Add(s, s.numberOfTurns);
                        massSpellIcons[3].color = Color.white;
                        GameInstance.playerController.waterWalk = true;
                        //blocks returned water ground become walkable changes in player controller
                        break;
                    case SpellEffects.LavaWalk:
                        if (spellTimeActive.ContainsKey(s))
                        {
                            spellTimeActive[s] = s.numberOfTurns;
                            break;
                        }
                        spellTimeActive.Add(s, s.numberOfTurns);
                        massSpellIcons[4].color = Color.white;
                        GameInstance.playerController.lavaWalk = true;


                        break;
                    case SpellEffects.Restoration:
                        break;
                    case SpellEffects.Identify:
                        //close spell book and wait for an item to click
                        break;
                    case SpellEffects.LightARoom:
                        if (spellTimeActive.ContainsKey(s)) { spellTimeActive[s] = s.numberOfTurns; break; }
                        GameInstance.playerController.LightARoom(2);
                        spellTimeActive.Add(s, s.numberOfTurns);
                        massSpellIcons[2].color = Color.white;
                        break;
                }
                GameInstance.party.activeHero.ManaDecrease(s.manaCost); 
            }
            
            CloseSpellbook();

            return;
        }


        if (spellToCast.AOE)
        {
            if((!spellToCast.OnlyEnemies & !spellToCast.OnlyParty) || (spellToCast.OnlyEnemies & spellToCast.OnlyParty))
            {
                print("AOE spell everyone");
            }
            if (spellToCast.OnlyEnemies)
            {
                print("AOE spell enemy only");
            }
            if (spellToCast.OnlyParty)
            {
                List<Hero> heroList = GameInstance.party.GetHeroList();
                foreach(Hero h in heroList)
                {
                    h.ApplySpellToHero(spellToCast, GameInstance.party.activeHero.GetThisHero().gameObject );
                }
                CloseSpellbook();
                if (!spellToCast.gameplaySpell)
                {
                    battlelogEvent.Invoke(new List<string>() { GameInstance.party.activeHero.HeroName(), "Whole Party", spellToCast.spellName }, null);
                }
                if(GameInstance.playerController.playerState == PlayerState.Battle)
                {
                    GameInstance.battleManager.AttackEnding();
                }
            }
            foreach (Spell s in spellToCast.spells)
            {
                GameInstance.party.activeHero.ManaDecrease(s.manaCost);
            }
        }
        else
        {
            spellWaitToRelease = spellToCast;
            spellTargetEvent.AddListener(GetGameObjectTarget);
            SpellCharged = true;
            GameInstance.SetMouseCursor(cursorTargetGraphics);
            CloseSpellbook();
        }
        if (GameInstance.playerController.playerState == PlayerState.Battle)
        {
            foreach(Spell s in spellToCast.spells)
            {
                GameInstance.battleManager.BattleSpellAgro(s.agroPoints);
            }
        }
    }

    public void GetGameObjectTarget(GameObject target)
    {
        //print("weapon spell hero check" + target);
        if (target.GetComponent<IHero>() != null) 
        {
            //print("cast to target");
            IHero ihero =  target.GetComponent<IHero>();
            ihero.ApplySpellToHero(spellWaitToRelease, GameInstance.party.activeHero.GetThisHero().gameObject);
        }
        if (target.GetComponent<IEnemy>() != null)
        {
            IEnemy ienemy = target.GetComponent<IEnemy>();
            int sum = GameInstance.party.activeHero.GetRowIndex() + ienemy.GetEnemyRow();
            if (ienemy.GetEnemyRow() <= spellWaitToRelease.minDistanceToEnemy)
            {

                if (ienemy.GetEnemyRow() > 1)
                {
                    
                    Dictionary<int, List<int>> places = new  Dictionary<int, List<int>>();
                    places.Add(1, new List<int>());
                    places.Add(2, new List<int>());
                    List<IEnemy> enemies = GameInstance.battleManager.GetEnemies();
                    foreach (IEnemy e in enemies)
                    {
                        if (e.GetEnemyRow() < ienemy.GetEnemyRow())
                        {
                            if (e.CheckForPlaceMatch(ienemy.GetEnemyPlace()).Count>0)
                            {
                               foreach(int i in e.CheckForPlaceMatch(ienemy.GetEnemyPlace()))
                                {

                                    places[e.GetEnemyRow()].Add(i);
                                }
                            }
                        }
                    }

                    if (places[2].Count >= ienemy.GetEnemyPlace().Count && ienemy.GetEnemyRow() !=2)
                    {

                        foreach (IEnemy e in enemies)
                        {
                            foreach (int i in places[2])
                            {
                                if (e.GetEnemyRow() == 2 && e.GetEnemyPlace().Contains(i)) ienemy = e;
                            }
                        }
                    }
                    if (places[1].Count >= ienemy.GetEnemyPlace().Count)
                    {

                        foreach (IEnemy e in enemies)
                        {
                            foreach(int i in places[1])
                            {
                                if (e.GetEnemyRow() == 1 && e.GetEnemyPlace().Contains(i))
                                {
                                    print("check enemies " + places[1].Count);
                                    ienemy = e; 
                                }
                            }

                        }
                    }
                }
               List<string> results =  ienemy.ApplySpellToEnemy(spellWaitToRelease, GameInstance.party.activeHero.GetThisHero().gameObject);
               battlelogEvent.Invoke(new List<string>() { GameInstance.party.activeHero.HeroName(), target.name, spellWaitToRelease.spellName }, results);
            }
            else
            {
                StartCoroutine(AttackDelay());
                battlelogEvent.Invoke(new List<string>() { GameInstance.party.activeHero.HeroName(), target.name, "no spell casted" }, null);
            }

        }


        if (target.GetComponent<IInteractable>() != null)
        {
            IInteractable interactable = target.GetComponent<IInteractable>();
            interactable.ApplySpellToItem(spellWaitToRelease);
        }
        foreach (Spell s in spellWaitToRelease.spells)
        {
            GameInstance.party.activeHero.ManaDecrease(s.manaCost);
        }
        SpellCharged = false;
        spellTargetEvent.RemoveAllListeners();
        spellWaitToRelease = null;
        GameInstance.SetMouseCursor(cursorNormal);
    }


    public void ReleaseSpellWithoutCasting()
    {
        SpellCharged = false;
        spellTargetEvent.RemoveAllListeners();
        spellWaitToRelease = null;
        GameInstance.SetMouseCursor(cursorNormal);
    }

    IEnumerator AttackDelay()
    {
        yield return new WaitForSeconds(0.5f);
        GameInstance.battleManager.AttackEnding();
    }

    public void ReleaseSpellTargetSearch()
    {
        spellWaitToRelease = null;
        spellTargetEvent.RemoveAllListeners();
        GameInstance.SetMouseCursor(cursorNormal);
    }


    public List<SpellContainer> GetHeroSpellbook(List<SpellContainer> heroSpellbook)
    {
        //Run through a spellpages to activate and deactivate spells 

        return heroSpellbook;
    } 

    public void OpenSpellbook(bool active)
    {

        foreach (GameObject g in objectToClose)
        {
            g.SetActive(true);
            if (g.GetComponent<SpellPageButton>() != null) g.GetComponent<SpellPageButton>().OnUncheckPage();
        }
        //print("spellbook?" + active);
        if (active) pages[lastOpenedPage].OpenSpellPage(true);
        else CloseSpellbook();
        if (objectToClose[lastOpenedPage].GetComponent<SpellPageButton>() != null) objectToClose[lastOpenedPage].GetComponent<SpellPageButton>().CheckedPage();
        
    }


    public void SetSpellPage(int index)
    {
        lastOpenedPage = index;
        CloseSpellbook();
        spellbookSwitch.isOn = true;
        pages[lastOpenedPage].OpenSpellPage(true);
        foreach(GameObject g in objectToClose)
        {
            if (g.GetComponent<SpellPageButton>() != null) g.GetComponent<SpellPageButton>().OnUncheckPage();
        }
        if (objectToClose[lastOpenedPage].GetComponent<SpellPageButton>() != null) objectToClose[lastOpenedPage].GetComponent<SpellPageButton>().CheckedPage();
        foreach (GameObject g in objectToClose)
        {
            g.SetActive(true);
        }
    }


    void BattleTimeChanges()
    {
        if (GameInstance.playerController.playerState != PlayerState.Battle) return;
        TimeChanges();
    }

    void TimeLimitSpellCount(int count)
    {
        if (GameInstance.playerController.playerState == PlayerState.Battle) return;
        //print("spellbook time");
        if (spellTimeActive.Count <= 0) return;

        TimeChanges();

    }

    private void TimeChanges()
    {
        List<Spell> listToDelete = new List<Spell>();
        List<Spell> listToChange = new List<Spell>();
        foreach (KeyValuePair<Spell, int> s in spellTimeActive)
        {
            if (spellTimeActive[s.Key] > 0) { listToChange.Add(s.Key); }
            else listToDelete.Add(s.Key);
        }
        foreach (Spell s in listToChange)
        {
            int x = spellTimeActive[s];
            spellTimeActive[s] = x - 1;
            switch (s.spellEffect)
            {
                case SpellEffects.WizardEye:
                    massSpellIcons[1].color = new Color32(255, 255, 255, (byte)(((float)spellTimeActive[s] / (float)s.numberOfTurns) * 255));
                    break;
                case SpellEffects.LightARoom:
                    GameInstance.playerController.LightARoom(((float)spellTimeActive[s] / (float)s.numberOfTurns) * 2);
                    massSpellIcons[2].color = new Color32(255, 255, 255, (byte)(((float)spellTimeActive[s] / (float)s.numberOfTurns) * 255));
                    break;
                case SpellEffects.Waterwalk:
                    massSpellIcons[3].color = new Color32(255, 255, 255, (byte)(((float)spellTimeActive[s] / (float)s.numberOfTurns) * 255));
                    break;
                case SpellEffects.LavaWalk:
                    massSpellIcons[3].color = new Color32(255, 255, 255, (byte)(((float)spellTimeActive[s] / (float)s.numberOfTurns) * 255));
                    break;
            }
        }

        foreach (Spell s in listToDelete)
        {
            switch (s.spellEffect)
            {
                case SpellEffects.WizardEye:
                    mapCamera.cullingMask &= ~(1 << 10);
                    mapMini.cullingMask &= ~(1 << 10);
                    massSpellIcons[1].color = Color.clear;
                    break;
                case SpellEffects.LightARoom:
                    GameInstance.playerController.LightARoom(0);
                    massSpellIcons[2].color = Color.clear;
                    break;
                case SpellEffects.Waterwalk:
                    massSpellIcons[3].color = Color.clear;
                    GameInstance.playerController.waterWalk = false;
                    break;
                case SpellEffects.LavaWalk:
                    massSpellIcons[4].color = Color.clear;
                    GameInstance.playerController.waterWalk = false;
                    break;
            }

            spellTimeActive.Remove(s);
        }
    }
}
