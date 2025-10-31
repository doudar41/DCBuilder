using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

// Summary
// This script handles  
// 1. Spellbook UI, reading saved spells on active party member and show them in a spellbook 
// 2. Casting non-battle time based spells with time limit like Water or Lava walk
// 3. battle log UI event
// 4. Casting delay while waiting for target and wait for spell animation effect is finished

public class Spellbook : MonoBehaviour
{

    [SerializeField] List<SpellPage> spellSchoolScripts = new List<SpellPage>(); //Spell page is a script which contains spell buttons 
    [SerializeField] Toggle spellbookSwitch; //Switch between main magic schools Elemental, Light and Dark
    [SerializeField] List<GameObject> objectToClose = new List<GameObject>(); // List of menus to meant to be closed when spellbook opened or closed
    [SerializeField] Texture2D cursorTargetGraphics, cursorNormal;
    [SerializeField] Camera mapCamera, mapMini;
    [SerializeField] GameObject stateIconPanel; //time based spells icons panel 

    SpellContainer spellWaitToRelease; //Saved Spell to use after clicking on target 
    int lastOpenedPage = 0; // School of magic last opened 
    List<Image> massSpellIcons = new List<Image>(); // Spell icons list to fade in and out on have time based spell active
    bool SpellCharged = false; // Indicates if saved spell is ready to cast 
    Dictionary<Spell, int> spellTimeActive = new Dictionary<Spell, int>(); //Saved time of time based spell is lasting
    
    public UnityEvent<GameObject> spellTargetEvent;
    public UnityEvent<List<string>, List<string>> battlelogEvent;
    public UnityEvent<SpellContainer> hitTargetEffect; 


    private void Awake() 
    {
        GameInstance.spellbook = this; // Make reference in GameInstance 
        GameInstance.progress += TimeLimitSpellCount; // Using GameInstance time delegate to manage countdown of time based spells 
        foreach (Image i in stateIconPanel.transform.GetComponentsInChildren<Image>()) 
        {
            massSpellIcons.Add(i); // Get image components from time based spell panel gameobject, probably will be changed to new class 
        }
        foreach (SpellPage sp in spellSchoolScripts)
        {
            sp.InitializeSpellSchoolScript(); // Each spell page script check for availability of a spell and save it into dictionary
        }
    }

    private void OnDestroy()
    {
        GameInstance.progress -= TimeLimitSpellCount; // sign out from Gameinstance delegate
    }

    public void OpenSpellbook(bool active) // Method is switching spellbook on and off by using UI toggle on right side menu (GameMenu)
        // it also switch to last magic school opened
    {
        GetPagesReady(); //Check spells available for a specific player and on and off them on spell pages
        foreach (GameObject g in objectToClose)
        {
            g.SetActive(true);
            if (g.GetComponent<SpellPageButton>() != null) g.GetComponent<SpellPageButton>().OnUncheckPage(); 
        }
        if (active) spellSchoolScripts[lastOpenedPage].OpenSpellPage(true);
        else CloseSpellbook();
        if (objectToClose[lastOpenedPage].GetComponent<SpellPageButton>() != null) objectToClose[lastOpenedPage].GetComponent<SpellPageButton>().CheckedPage();
    }


    public void CloseSpellbook() // Close spellbook interface, switch off toggle and allows player to move
    {
        foreach (SpellPage sp in spellSchoolScripts) 
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

    public void GetPagesReady() //Check spells available for a specific player and on and off them on spell pages
    {
        foreach(SpellPage sp in spellSchoolScripts)
        {
            sp.SetPageAvailableSpells(GameInstance.party.activeHero.GetActiveHeroSpellbook());
        }
    }


    public void SetSpellPage(int index) //Reopen spell page script with specific index 
    {
        lastOpenedPage = index;
        CloseSpellbook();
        spellbookSwitch.isOn = true;
        spellSchoolScripts[lastOpenedPage].OpenSpellPage(true);
        foreach (GameObject g in objectToClose)
        {
            if (g.GetComponent<SpellPageButton>() != null) g.GetComponent<SpellPageButton>().OnUncheckPage();
        }
        if (objectToClose[lastOpenedPage].GetComponent<SpellPageButton>() != null) objectToClose[lastOpenedPage].GetComponent<SpellPageButton>().CheckedPage();
        foreach (GameObject g in objectToClose)
        {
            g.SetActive(true);
        }
    }



    public bool SpellWaiting() //Check if there is a spell waiting for release by clicking target or pressing cast button in battle menu
    {
        return spellWaitToRelease != null;
    }

    public void CastSpell(SpellContainer spellToCast)  
    {
        // Checking is a spell is explore based Light, Waterwalk, Lavawalk etc. and immediately start it as a time based spell.
        // Add it to spellTimeActive dictionary for a timepassed check in TimeLimitSpellCount(int count)

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
                        GameInstance.playerController.LightARoom(1);
                        spellTimeActive.Add(s, s.numberOfTurns);
                        massSpellIcons[2].color = Color.white;
                        break;
                }
                GameInstance.party.activeHero.ManaDecrease(s.manaCost); 
            }
            
            CloseSpellbook();

            return;
        }
        // If it spell is not one of the exploration spells (gameplay spell) check for AOE spell 

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
            //print(spellToCast.name);
            // Non AEO Single spell loaded to be released
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

    public void SaveContinousSpells() //This method saves exploration time based spells to GameInstance to be loaded on different level or from the save file
    {
        SavedSpellsAttached savedSpellsAttached = new SavedSpellsAttached();
        foreach (KeyValuePair< Spell,int> s in spellTimeActive)
        {
            savedSpellsAttached.spell.Add(s.Key);
            savedSpellsAttached.timesToFinish.Add(s.Value);        
            //print("save continous spells " + s.Key.spellEffect+"/"+s.Value);
        }
        GameInstance.spellsFromSpellbook.Add(savedSpellsAttached);
    }

    public void RestoreContinousSpells() // Restoring time based spells taken from GameInstance on game loding
    {
        //print("save continous spells ");
        foreach (SavedSpellsAttached savedspell in GameInstance.spellsFromSpellbook)
        {


            for (int i=0;i< savedspell.spell.Count; i++)
            {
                //print("save continous spells " + savedspell.spell[i].spellEffect + "/" + savedspell.timesToFinish[i]);
                switch (savedspell.spell[i].spellEffect)
                {
                    case SpellEffects.Recall:
                        if (GameInstance.playerController.playerState != PlayerState.Battle) print("recall mark");
                        //Get list of marked coordinates from active hero trasfer to this coordinates 
                        break;
                    case SpellEffects.WizardEye:
                        // open special signs on map
                        if (spellTimeActive.ContainsKey(savedspell.spell[i])) break;
                        mapCamera.cullingMask |= 1 << 10;
                        mapMini.cullingMask |= 1 << 10;
                        spellTimeActive.Add(savedspell.spell[i], savedspell.timesToFinish[i]);
                        massSpellIcons[1].color = Color.white;
                        break;
                    case SpellEffects.Waterwalk:
                        if (spellTimeActive.ContainsKey(savedspell.spell[i]))
                        {
                            spellTimeActive[savedspell.spell[i]] = savedspell.timesToFinish[i];
                            break;
                        }
                        spellTimeActive.Add(savedspell.spell[i], savedspell.timesToFinish[i]);
                        massSpellIcons[3].color = Color.white;
                        GameInstance.playerController.waterWalk = true;
                        //blocks returned water ground become walkable changes in player controller
                        break;
                    case SpellEffects.LavaWalk:
                        if (spellTimeActive.ContainsKey(savedspell.spell[i]))
                        {
                            spellTimeActive[savedspell.spell[i]] = savedspell.timesToFinish[i];
                            break;
                        }
                        spellTimeActive.Add(savedspell.spell[i], savedspell.timesToFinish[i]);
                        massSpellIcons[4].color = Color.white;
                        GameInstance.playerController.lavaWalk = true;


                        break;
                    case SpellEffects.Restoration:
                        break;
                    case SpellEffects.Identify:
                        //close spell book and wait for an item to click
                        break;
                    case SpellEffects.LightARoom:
                        if (spellTimeActive.ContainsKey(savedspell.spell[i]))
                        {
                            spellTimeActive[savedspell.spell[i]] = savedspell.timesToFinish[i];
                            break;
                        }
                        spellTimeActive.Add(savedspell.spell[i], savedspell.timesToFinish[i]);
                        GameInstance.playerController.LightARoom(1);
                        massSpellIcons[2].color = Color.white;
                        break;
                }
            }

        }

    }



    public void GetGameObjectTarget(GameObject target)
    {
        //print("weapon spell hero check" + target);
        if (target.GetComponent<IHero>() != null) 
        {
            IHero ihero =  target.GetComponent<IHero>();
            ihero.ApplySpellToHero(spellWaitToRelease, GameInstance.party.activeHero.GetThisHero().gameObject);
            foreach(Spell s in spellWaitToRelease.spells)
            {
                hitTargetEffect.Invoke(spellWaitToRelease);
            }
        }
        if (target.GetComponent<IEnemy>() != null)
        {
            IEnemy ienemy = target.GetComponent<IEnemy>();
            int sum = GameInstance.party.activeHero.GetRowIndex() + ienemy.GetEnemyRow();
            if (ienemy.GetEnemyRow() <= spellWaitToRelease.minDistanceToEnemy)
            {

               List<string> results =  ienemy.ApplySpellToEnemy(spellWaitToRelease, GameInstance.party.activeHero.GetThisHero().gameObject);
               battlelogEvent.Invoke(new List<string>() { GameInstance.party.activeHero.HeroName(), target.name, spellWaitToRelease.spellName }, results);
            }
            else
            {
                StartCoroutine(AttackDelay());
                battlelogEvent.Invoke(new List<string>() { GameInstance.party.activeHero.HeroName(), target.name, "no spell casted" }, null);
            }

        }

        /*        if (target.GetComponent<IInteractable>() != null)
            {
                IInteractable interactable = target.GetComponent<IInteractable>();
                interactable.ApplySpellToItem(spellWaitToRelease);
            }*/
        foreach (Spell s in spellWaitToRelease.spells)
        {
            GameInstance.party.activeHero.ManaDecrease(s.manaCost);
        }
        SpellCharged = false;
        spellTargetEvent.RemoveAllListeners();
        spellWaitToRelease = null;
        GameInstance.SetMouseCursor(cursorNormal);
    }


    public void ReleaseSpellWithoutCasting() // On Escape key spell releassed without casting
    {
        SpellCharged = false;
        spellTargetEvent.RemoveAllListeners();
        spellWaitToRelease = null;
        GameInstance.SetMouseCursor(cursorNormal);
    }

    IEnumerator AttackDelay() // Delay for spell animation 
    {
        yield return new WaitForSeconds(0.5f);
        GameInstance.battleManager.AttackEnding();
    }

    public void ReleaseSpellTargetSearch() // On Escape key spell releassed without casting
    {
        spellWaitToRelease = null;
        spellTargetEvent.RemoveAllListeners();
        GameInstance.SetMouseCursor(cursorNormal);
    }


    void TimeLimitSpellCount(int count) // Time based spells checked in intervals taken from GameInstance
    {
        if (GameInstance.playerController.playerState == PlayerState.Battle) return;
        if (spellTimeActive.Count <= 0) return;

        TimeChanges();
    }

    private void TimeChanges() // Checking time based spells state
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
                    GameInstance.playerController.LightARoom(((float)spellTimeActive[s] / (float)s.numberOfTurns) * 1);
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
