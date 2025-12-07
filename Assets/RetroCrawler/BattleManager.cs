using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Ami.BroAudio;



/// <summary>
/// Battle start by sorting quarry of opponents
/// Then it awaits for signal to attack or cast a spell
/// While waiting player can choose spell to cast target to cast
/// pressing attack uses weapon in hands to perform mellee or range attack
/// pressing press use spell use last spell(if no spell it opens a spellbook) or choose spell from spell book 
/// attack or casting are send a spell from hero to target 
/// </summary>



public class BattleManager : MonoBehaviour
{

    // These are lists of randomly spawned enemies in timebased or scripted encounters
    [SerializeField] List<EnemySized> level01Enemies = new List<EnemySized>();
    [SerializeField] List<GameObject> level01Bosses = new List<GameObject>();

    //Reference to GameObject where defeated enemies went after being killed.
    [SerializeField] defeatedList _defeatedList;

    // List of all enemies and heroes 
    List<GameObject> allOpponents = new List<GameObject>();
    // Dictionary is used to make an order of opponents depend on their initiative and state
    Dictionary<int, GameObject> quarrySorted = new Dictionary<int, GameObject>();
    //  integer which increment by 1 each turn till reach end of quarrySorted list then back to zero and start new round
    int quarrySortedKey = 0;
    // Counter of time turns
    int actionCounter = 0;

    // it is teleport destination in a gameworld for Player for battle 
    [SerializeField] Transform playerBattlePlace;

    // GameObjects which contains transforms for enemies to spawn
    [SerializeField] List<GameObject> spawnPointsRaw01, spawnPointsRaw02, SpawnPointsRaw03;

    //Event which makes battle text appear at battle start and during enemy turn
    public UnityEvent<string> enemyTurn;

    // battleStarts opens overlay of battle buttons and close map zooming buttons disable map functionality and also starts battle log
    // BattleEnd restores everything back mainly the map fuctionality and also close battle log
    // PlayerTurn open battle buttons close battle text objects (one which showws "Enemy Turn" text)
    public UnityEvent battleStarts, BattleEnd, PlayerTurn;

    //Index of a target opponent in allopponents list 
    int targetIndexInOpponents = 0; //index in allopponents list

    //This delegate is send command to opponents counter number instead of normal exploration gametime delegate
    public delegate void BattlePassTime(int count);
    public BattlePassTime battlePassTime;


    // Not used yet
    [SerializeField] List<ItemScriptableContainer> listRandomLoot = new List<ItemScriptableContainer>();

    //This class contains textures of several available bioms to choose depend on battle ground environment enum of the block
    [SerializeField] BattleGroundGraphics battleGroundGraphics;




    //if it's scripted battle
    bool customBattle = false;


    // Block responsible for calling a custom battle
    IBlock customBattleBlock;

    //List of defeated enemies
    List<int> listOfTheDead = new List<int>();



    private void Awake()
    {
        GameInstance.battleManager = this;

    }



    //This is start of battle after random time passed in explore state. Player Controller has NoEncounter bool which on/off time based encounter
    public void BattleStart()
    {

        if (!customBattle)
        {
            // This will notify player that Battle begun
            enemyTurn.Invoke("Battle");
            GameInstance.party.SetTimerForHeroes(true);
            //Spawn enemies and add them to allopponents list
            SpawnEnemies(level01Enemies, 2, spawnPointsRaw01, 1);
            SpawnEnemies(level01Enemies, 2, spawnPointsRaw02, 2);

        }


        //Add heroes to allopponents list
        foreach (Hero h in GameInstance.party.GetHeroList())
        {
            allOpponents.Add(h.gameObject);
        }

        SortingOpponents();

        targetIndexInOpponents = -1;
        quarrySortedKey = 0;
        //Teleport player to battle location
        StartCoroutine(RecheckPositionRotation());

        //Check if the first member of a quarry is enemy
        if (IfActiveOpponentIsEnemy())
        {
            EnemyAutoAttack();
        }
        else
        {
            GameInstance.playerController.attackAllowed = true;
        }
        //Open UI elements which need for battle 
        battleStarts.Invoke();


    }


    public void SetListOfEnemies(List<EnemySized> enemies)
    {
        level01Enemies.Clear();
        foreach(EnemySized es in enemies)
        {
            level01Enemies.Add(es);
        }

    }

    //This one starts scripted battle 
    public void CustomBattleStart(List<EnemySized> _level01Enemies, IBlock iblock, BattleGroundEnvironment battleGroundEnvironment )
    {
        //With this bool == true the InteractablesEnum "CUSTOMBATTLE" in blockInteractables list will be removed after winning battle so the battle will not be repeated
        customBattle = true;
        customBattleBlock = iblock;


        battleGroundGraphics.SetBattleGround(battleGroundEnvironment);
        GameInstance.soundManager.LaunchBattleMusic(battleGroundEnvironment);
        GameInstance.playerController.StartCustomBattle();
        enemyTurn.Invoke("Battle");
        //StopCoroutine(GameInstance.TimeStep());
        GameInstance.party.SetTimerForHeroes(true);
        //Add enemies
        if(_level01Enemies != null)
        {
            SpawnEnemies(_level01Enemies, 2, spawnPointsRaw01, 1);
            SpawnEnemies(_level01Enemies, 2, spawnPointsRaw02, 2);
        }
        else
        {
            SpawnEnemies(level01Enemies, 2, spawnPointsRaw01, 1);
            SpawnEnemies(level01Enemies, 2, spawnPointsRaw02, 2);
        }

        BattleStart();
    }


    //Spawn enemies logic 
    void SpawnEnemies(List<EnemySized> enemies, int randomRange, List<GameObject> spawnRow, int row)
    {

        // Destroys any left object on spawn line 
        foreach (GameObject spawnPoint in spawnRow)
        {
            for (int i = 0; i < spawnPoint.transform.childCount; i++)
            {
                DestroyImmediate(spawnPoint.transform.GetChild(i));
            }
        }

        List<int> empty = new List<int>();

        //Try to fill each spawn point with 1 level enemies

        for (int i =0;i< spawnRow.Count;i++)
        {
            if(Random.Range(0, randomRange) == 0)
            {
                GameObject enemy = Instantiate(enemies[0].enemies[Random.Range(0, enemies[0].enemies.Count)], spawnRow[i].transform);
                empty.Add(i);
                allOpponents.Add(enemy);
                enemy.GetComponent<IEnemy>().SetEnemyPlaceSpace(row, new List<int> { i });
            }
        }

        // if there are no enemies spawned and enemy list have only level 1 enemies 
        // spawn one level 1 enemy at the center of spawn line

        if (empty.Count == 0 && enemies.Count == 1)
        {
            GameObject enemy = Instantiate(enemies[0].enemies[Random.Range(0, enemies[0].enemies.Count)], spawnRow[2].transform);
            enemy.GetComponent<IEnemy>().SetEnemyPlaceSpace(row, new List<int> { 2 });
        }

        if (enemies.Count == 1) return;

        if (!empty.Contains(0) && !empty.Contains(1) && !empty.Contains(2))
        {
            if (Random.Range(0, randomRange) == 0)
            {
                GameObject enemy = Instantiate(enemies[1].enemies[Random.Range(0, enemies[1].enemies.Count)], spawnRow[1].transform);
                allOpponents.Add(enemy);
                empty.Add(1);
                enemy.GetComponent<IEnemy>().SetEnemyPlaceSpace(row, new List<int> { 0, 1, 2 });
            }
        }

        if (!empty.Contains(2) && !empty.Contains(3) && !empty.Contains(4))
        {
            if (Random.Range(0, randomRange) == 0)
            {
                GameObject enemy = Instantiate(enemies[1].enemies[Random.Range(0, enemies[1].enemies.Count)], spawnRow[3].transform);
                allOpponents.Add(enemy);
                empty.Add(3);
                enemy.GetComponent<IEnemy>().SetEnemyPlaceSpace(row, new List<int> { 2, 3, 4 });
            }
        }

        if (empty.Count == 0 && enemies.Count == 2)
        {
            GameObject enemy = Instantiate(enemies[1].enemies[Random.Range(0, enemies[1].enemies.Count)], spawnRow[2].transform);
            allOpponents.Add(enemy);
            empty.Add(3);
            enemy.GetComponent<IEnemy>().SetEnemyPlaceSpace(row, new List<int> { 1, 2, 3 });
        }

        if (enemies.Count == 2) return;

        if (empty.Count == 0 && enemies.Count == 3)
        {
            GameObject enemy = Instantiate(enemies[2].enemies[Random.Range(0, enemies[2].enemies.Count)], spawnRow[2].transform);
            allOpponents.Add(enemy);
            empty.Add(5);
            enemy.GetComponent<IEnemy>().SetEnemyPlaceSpace(row, new List<int> {0, 1, 2, 3, 4 });
            return;
        }

        if (empty.Count == 0)
        {
            GameObject enemy = Instantiate(enemies[0].enemies[Random.Range(0, enemies[0].enemies.Count)], spawnRow[2].transform);
            enemy.GetComponent<IEnemy>().SetEnemyPlaceSpace(row, new List<int> { 2 });
            return;
        }
    }


    IEnumerator RecheckPositionRotation()
    {
        yield return new WaitForSeconds(0.3f);
        GameInstance.playerController.beforeBattleTransformRot = GameInstance.playerController.gameObject.transform.rotation;
        GameInstance.playerController.transform.position = playerBattlePlace.position;
        GameInstance.playerController.transform.rotation = playerBattlePlace.rotation;
    }

    void EnemyAutoAttack()
    {
        if (quarrySorted[quarrySortedKey].GetComponent<IEnemy>() == null) AttackEnding();

        //print("attacker name " + quarrySorted[quarrySortedKey].GetComponent<IEnemy>().GetEnemyName() + "/"+ quarrySortedKey);
        
        IEnemy attacker = quarrySorted[quarrySortedKey].GetComponent<IEnemy>();

        if (!CheckEnemyState(attacker)) { AttackEnding(); return; }

        if(attacker.GetCurrentStatValue(EnemyStat.INITIATIVE) <= 0) { AttackEnding(); return; }



        // Notify player that it's a enemy turn
        enemyTurn.Invoke("Enemy turn");

        // Choosing hero to attack by finding hero with biggest agro
        int biggestAgro = -1;

        // Check if enemy has a health to attack 
        if (attacker.GetEnemyHealth() <= 0)
        {
            AttackEnding();
            return;
        }

        int maxspelldistance =0;
        foreach (SpellContainer spell in attacker.GetEnemyAttackSpell())
        {
            if(spell.minDistanceToEnemy > maxspelldistance)
            {
                maxspelldistance = spell.minDistanceToEnemy;
            }
        }

        if(maxspelldistance < attacker.GetEnemyRow())
        {
            // Enemy can't reach any hero to attack
            //enemyTurn.Invoke(attacker.GetEnemyName() + " can't reach any hero to attack!");
            AttackEnding();
            return;
        }
        //Play animation of moving forward
        attacker.MoveAheadOnAttack();

        //Ruun through all opponents, getting IHero interface, if it on gameobject checking for agro and save biggest one to biggestAgro to compare 
        for (int i =0; i<allOpponents.Count;i++)
        {

            if (allOpponents[i].GetComponent<IHero>() != null)
            {

                int a = allOpponents[i].GetComponent<IHero>().GetHeroAgro();
                if (biggestAgro < a && allOpponents[i].GetComponent<IHero>().GetHeroHealth() > 0)
                {
                    //print("opponents " + i + " agro " + biggestAgro);
                    biggestAgro = a;
                    targetIndexInOpponents = i;
                }
            }
        }
        //If targetIndexInOpponents -1 there is no one to attack
        if (targetIndexInOpponents > -1) 
        {
            IHero targetHero = allOpponents[targetIndexInOpponents].GetComponent<IHero>();
            //Apply attack spell from enemy to chosen hero
            List<ResultMsg> resultMsgs = targetHero.ApplySpellToHero(attacker.enemyAttack(attacker.GetEnemyRow()), quarrySorted[quarrySortedKey]);
            GameInstance.spellbook.ResultsToBattleLog(new List<string> { "enemy "} ,resultMsgs);

        }
    }

    public bool CheckEnemyState(IEnemy _enemy)
    { 

        var enemystatus = _enemy.GetEnemyPermanentStatus();
        if (enemystatus.ContainsKey(SpellEffects.Stone))
        {
            enemyTurn.Invoke(_enemy.GetEnemyName() + " is turned to stone and can't move!");

            return false;
        }
        if (enemystatus.ContainsKey(SpellEffects.Paralize))
        {
            enemyTurn.Invoke(_enemy.GetEnemyName() + " is paralyzed and can't move!");

            return false;
        }
        if (enemystatus.ContainsKey(SpellEffects.Freeze))
        {
            enemyTurn.Invoke(_enemy.GetEnemyName() + " is frozen and can't move!");

            return false;
        }

        return true; 
    }


    /// Checking if gameobject has IEnemy interface
    bool IfActiveOpponentIsEnemy()
    {
        if (quarrySorted.Count <= 0 || quarrySorted[quarrySortedKey]== null) return false;
        if (quarrySorted[quarrySortedKey].GetComponent<IEnemy>() != null) return true;
        else return false;
    }


    //Make an enemiy list
    public List<IEnemy> GetEnemies()
    {
        List<IEnemy> enemiesList = new List<IEnemy>();

        foreach(GameObject g in allOpponents)
        {
            if(g.GetComponent<IEnemy>() != null)
            {
                if(g.GetComponent<IEnemy>().GetEnemyHealth()>0 && !g.GetComponent<IEnemy>().GetEnemyPermanentStatus().ContainsKey( SpellEffects.Stone))

                enemiesList.Add(g.GetComponent<IEnemy>());
            }
        }

        return enemiesList;
    }


    //This method close a current turn and start next

    public void EndOfTheTurn()
    {
        //Advace battle time 
        actionCounter++;
        battlePassTime(actionCounter);
        //Next element in quarrySorted objects key
        quarrySortedKey++;


        //Check if current key exist 
        if (quarrySorted.ContainsKey(quarrySortedKey))
        {
            //print("end of turn if it stops press skip " + quarrySorted[quarrySortedKey]);
        }
        else
        {
            if(quarrySortedKey >= quarrySorted.Count)
            {
                //Switch to next round if key larger than quarry size
                targetIndexInOpponents = -1;
                enemyTurn.Invoke("");
                quarrySorted.Clear();

                EndOfRound();
                return;
            }
        }

        //Make reference of dead enemies
        if (listOfTheDead.Contains(quarrySortedKey))
        {
            for (int i = quarrySortedKey; i < quarrySorted.Count; i++)
            {
                if (!listOfTheDead.Contains(i))
                {
                    quarrySortedKey = i; break;
                }
            }

        }

        // Choose between enemy or player turn
        if (IfActiveOpponentIsEnemy())
        {
            EnemyAutoAttack();
            return;
        }
        else
        {
            GameInstance.playerController.attackAllowed = true;
        }
        targetIndexInOpponents = -1;
        enemyTurn.Invoke("");
        PlayerTurn.Invoke();
        SetActiveHero();
    }


    //This will check Hero status and make it active hero 
    private void SetActiveHero()
    {
        if (quarrySorted.ContainsKey(quarrySortedKey))
        {
            if (quarrySorted.TryGetValue(quarrySortedKey, out GameObject g))
            {
                if (quarrySorted.Count != 0 && g != null)
                {

                    if (g.GetComponent<IHero>() != null)
                    {
                        IHero newactivehero = g.GetComponent<IHero>();
                        GameInstance.party.BattleHeroSwitch(newactivehero.GetThisHero());
                        if (newactivehero.GetHeroHealth() <= 0 ||
                            newactivehero.GetHeroStatus().Contains(GameplayStatus.Petrified) ||
                            newactivehero.GetHeroStatus().Contains(GameplayStatus.Stunned)) AttackEnding();
                    }
                }
            }
        }
    }

    //End round nullifies quarry key resorting opponents check for empty row in enemy formation, if yes move enemies closer to player

    public void EndOfRound()
    {
        quarrySortedKey = 0;
        SortingOpponents();
        CheckForEmptyRow();
        SetActiveHero();

        if (IfActiveOpponentIsEnemy())
        {
            EnemyAutoAttack();
        }
        else
        {
            GameInstance.playerController.attackAllowed = true;
        }
    }


    //Checking for empty row in enemy formation
    public void CheckForEmptyRow()
    {
        List<IEnemy> enemies = GetEnemies();
        List<IEnemy> row1 = new List<IEnemy>();
        foreach(IEnemy e in enemies)
        {
            if (e.GetEnemyRow() == 1)
            {
                row1.Add(e);
            }
        }
        if (row1.Count == 0)
        {
            foreach (IEnemy e in enemies)
            {
                if (e.GetEnemyRow() == 2)
                {
                    e.SetEnemyPlaceSpace(1, e.GetEnemyPlace());
                    if (e.GetEnemyPlace().Count == 1) e.SetTransform(spawnPointsRaw01[e.GetEnemyPlace()[0]]);
                    else
                    {
                        if (e.GetEnemySize()==3) e.SetTransform(spawnPointsRaw01[e.GetEnemyPlace()[1]]);
                        if (e.GetEnemySize() == 5) e.SetTransform(spawnPointsRaw01[2]);
                    }
                }
            }
        }


    }


    
    int WhoWon()// 1-enemies 2-heroes 0-no one
    {
        int enemyHealth = 0;
        int heroesHealth = 0;
        int unabledEnemies = 0, unabledHeroes = 0;
        int enemyCount = 0;
        foreach (GameObject g in allOpponents)
        { 
            if(g == null)
            {
                continue;
            }
            if (g.GetComponent<IHero>() != null)
            {
                heroesHealth += g.GetComponent<IHero>().GetHeroHealth();
                if (g.GetComponent<IHero>().GetHeroStatus().Contains(GameplayStatus.Stoned)) unabledHeroes++;
            }
            else if(g.GetComponent<IEnemy>() != null)
            {
                enemyHealth += g.GetComponent<IEnemy>().GetEnemyHealth();
                if (g.GetComponent<IEnemy>().GetEnemyPermanentStatus().ContainsKey(SpellEffects.Stone)) unabledEnemies++;
                if (g.GetComponent<IEnemy>().GetEnemyPermanentStatus().ContainsKey(SpellEffects.Paralize)) unabledEnemies++;
                if (g.GetComponent<IEnemy>().GetEnemyPermanentStatus().ContainsKey(SpellEffects.Freeze)) unabledEnemies++;
                if (g.GetComponent<IEnemy>().GetCurrentStatValue(EnemyStat.INITIATIVE) < 0) unabledEnemies++;
                enemyCount++;
            }
        }

        if (enemyHealth <= 0) return 1;
        if (heroesHealth <= 0) return 2;
        if(unabledHeroes == 4) return 2;
        if(unabledEnemies == enemyCount) return 1;


        return 0;
    }


    void SortingOpponents()
    {

        List<int> sortList = new List<int>();

        foreach(GameObject g in allOpponents)
        {
            if (g.GetComponent<IEnemy>() != null)
            {
                if (g.GetComponent<IEnemy>().GetEnemyHealth() > 0)
                {
                    sortList.Add(g.GetComponent<IBattle>().GetInitiativeInBattle());
                    g.GetComponent<IEnemy>().MoveBackAfterAttack();


                }
            }
            if (g.GetComponent<IHero>() != null)
            {

                if (g.GetComponent<IHero>().GetHeroHealth() > 0)
                {
                    //print("hero " + g.name);
                    sortList.Add(g.GetComponent<IBattle>().GetInitiativeInBattle());
                }
            }

        }
        sortList.Sort(); sortList.Reverse();
        foreach (int i in sortList)
        {
            foreach (GameObject g in allOpponents)
            {

                if (i == g.GetComponent<IBattle>().GetInitiativeInBattle())
                {
                    if (!quarrySorted.ContainsValue(g)) 
                    { 
                       if( !quarrySorted.TryAdd(quarrySorted.Count, g))
                        {
                            print(g);
                        } 
                    }
                }
            }
        }

        foreach (KeyValuePair<int, GameObject> k in quarrySorted)
        {
           // print(k.Key + " " + k.Value.name + " " + k.Value.GetComponent<IBattle>().GetInitiativeInBattle());
        }

    }


    public void GetOpponnentsFromPlayer(List<GameObject> opponentsFromPlayer)
    {
        allOpponents = opponentsFromPlayer;
    }


    public void BattleIsOver(bool win)
    {
        enemyTurn.Invoke("");
        PlayerTurn.Invoke();

        if (win)
        {

            for (int i = 0; i < allOpponents.Count; i++)
            {
                if (allOpponents[i].GetComponent<IHero>() != null)
                {
                    allOpponents[i].GetComponent<IHero>().ChangeArgo(-allOpponents[i].GetComponent<IHero>().GetHeroAgro());
                }
            }

            List<int> toErase = new List<int>();
            for(int i =0;i<allOpponents.Count;i++)
            {
                if(allOpponents[i].GetComponent<IEnemy>() != null)
                {
                    GameInstance.party.addExperiencePoints(allOpponents[i].GetComponent<IEnemy>().ExperienceReward());
                    toErase.Add(i);
                }
            }
            foreach(int i in toErase)
            {
                Destroy(allOpponents[i]);
            }
            if (customBattle)
            {
                customBattleBlock.FinishTheBattle();
            }
        }
        else
        {
            GameInstance.LoadGameMainMenu();
        }
        BattleEnd.Invoke();
        quarrySorted.Clear();
        allOpponents.Clear();
        GameInstance.playerController.SetPlayerState(PlayerState.Explore);
        GameInstance.playerController.ReturnToPreBattlePosition();
        GameInstance.soundManager.BackToCurrentExploreMusic();
        StartCoroutine(GameInstance.TimeStep());
        GameInstance.party.SetTimerForHeroes(false);
        //_defeatedList.ClearList();
    }
    
    public void ReceiveAttackInput()
    {

        //battleInputDelay = true;
        if (quarrySorted[quarrySortedKey] == null)
        {
            return;
        }
        if (quarrySorted[quarrySortedKey].GetComponent<IHero>() == null) 
        {
            return; 
        }
        //print("switch hero" + quarrySorted[quarrySortedKey].GetComponent<IHero>().HeroName());
        IHero attacker = quarrySorted[quarrySortedKey].GetComponent<IHero>();
        GameInstance.spellbook.CastSpell(attacker.GetWeaponSpell());
    }

    public void RemoveDeadEnemy(GameObject g)
    {
        foreach(KeyValuePair<int, GameObject> enemy in quarrySorted)
        {
            if (enemy.Value == g)
            {
                listOfTheDead.Add(enemy.Key);
            }
        }
        RemoveDefeated(g);
    }

/*    public void ReceiveLastSpellInput()
    {
        if (quarrySorted[quarrySortedKey].GetComponent<IHero>() == null) return;


        // if spell AOE roll through all with current spell of a hero
        //if spell no AOE wait for cursor input
    }*/


    public void BattleSpellAgro(int amount)
    {
        if (!quarrySorted.ContainsKey(quarrySortedKey)) return;
        if (quarrySorted[quarrySortedKey].GetComponent<IHero>() != null)
        {
            quarrySorted[quarrySortedKey].GetComponent<IHero>().ChangeArgo(amount);
        }
    }

    public void AttackEnding()
    {
            //Check for heroes health
            //if ok end of the turn
            
        if (WhoWon() == 2) BattleIsOver(false);
        if (WhoWon() == 1)
        {
            BattleIsOver(true);
        }
        //BattleEffect = false;
        if (WhoWon() == 0) EndOfTheTurn();
    }

    public void RemoveOpponent(GameObject opponent)
    {
        allOpponents.Remove(opponent);
    }

    public void GetRidOfDeadEnemies()
    {
        List<GameObject> deadenemies = new List<GameObject>();
        foreach (GameObject g in allOpponents)
        {
            if (g.GetComponent<IEnemy>() == null) continue;
            if (g.GetComponent<IEnemy>().GetEnemyHealth() <= 0)
            {
                deadenemies.Add(g);
            }
        }
        foreach(GameObject g in deadenemies)
        {
            allOpponents.Remove(g);
            DestroyImmediate(g);
        }

    }

    public int GetActionCounter()
    {
        return actionCounter;
    }

    public void RemoveDefeated(GameObject enemyObject)
    {
        _defeatedList.AddToList(enemyObject);
    }


}


public interface IBattle
{
    public int GetInitiativeInBattle();
    public List<GameObject> GetOpponents();
}


[System.Serializable]
public struct EnemySized
{
    public int size;
    public List<GameObject> enemies; 
}