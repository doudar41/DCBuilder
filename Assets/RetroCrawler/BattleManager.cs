using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



/// <summary>
/// Battle start by sorting quarry of ooponents
/// Then it awaits for signal to attack or cast a spell
/// While waiting player can choose spell to cast target to cast
/// pressing attack uses weapon in hands to perform mellee or range attack
/// pressing press use spell use last spell(if no spell it opens a spellbook) or choose spell from spell book 
/// attack or casting are send a spell from hero to target 
/// </summary>



public class BattleManager : MonoBehaviour
{
    [SerializeField] List<EnemySized> level01Enemies = new List<EnemySized>();
    [SerializeField] List<GameObject> level01Bosses = new List<GameObject>();


    List<GameObject> allOpponents = new List<GameObject>();
    Dictionary<int, GameObject> quarrySorted = new Dictionary<int, GameObject>();
    int quarrySortedKey = 0;

    int actionCounter = 0;
    //Turn event 
    //Quarry of heroes and enemies
    [SerializeField] Transform playerBattlePlace;

    [SerializeField] List<GameObject> spawnPointsRaw01, spawnPointsRaw02, SpawnPointsRaw03;


    public UnityEvent<string> enemyTurn;
    public UnityEvent battleStarts, BattleEnd, PlayerTurn;

    int targetIndexInOpponents = 0; //index in allopponents list
    public bool BattleEffect = false;

    public delegate void BattlePassTime(int count);
    public BattlePassTime battlePassTime;

    private void Awake()
    {
        GameInstance.battleManager = this;
    }

    public void BattleStart()
    {
        enemyTurn.Invoke("Battle");
        StopCoroutine(GameInstance.TimeStep());
        GameInstance.party.SetTimerForHeroes(true);
        //Add enemies
        SpawnEnemies(level01Enemies, 2, spawnPointsRaw01, 1);
        SpawnEnemies(level01Enemies, 2, spawnPointsRaw02, 2);

        foreach (Hero h in GameInstance.party.GetHeroList())
        {
            allOpponents.Add(h.gameObject);
        }

        SortingOpponents();

        targetIndexInOpponents = -1;
        quarrySortedKey = 0;
        StartCoroutine(RecheckPositionRotation());

        if (IfActiveOpponentIsEnemy())
        {
            EnemyAutoAttack();
        }
        battleStarts.Invoke();
    }


    void SpawnEnemies(List<EnemySized> enemies, int randomRange, List<GameObject> spawnRow, int row)
    {
        foreach (GameObject spawnPoint in spawnRow)
        {
            for (int i = 0; i < spawnPoint.transform.childCount; i++)
            {
                DestroyImmediate(spawnPoint.transform.GetChild(i));
            }
        }

        List<int> empty = new List<int>();

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

        if (!empty.Contains(0) && !empty.Contains(1)&& !empty.Contains(2))
        {
            if (Random.Range(0, randomRange) == 0)
            {
                    GameObject enemy = Instantiate(enemies[1].enemies[Random.Range(0, enemies[1].enemies.Count)], spawnRow[1].transform);
                    allOpponents.Add(enemy);
                    empty.Add(1);
                    enemy.GetComponent<IEnemy>().SetEnemyPlaceSpace(row, new List<int> { 0,1,2});
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

        if (empty.Count == 0)
        {
            if (Random.Range(0, 1) == 0)
            {
                GameObject enemy = Instantiate(enemies[2].enemies[Random.Range(0, enemies[2].enemies.Count)], spawnRow[2].transform);
                allOpponents.Add(enemy);
                empty.Add(2);
                enemy.GetComponent<IEnemy>().SetEnemyPlaceSpace(row, new List<int> { 0, 1, 2,3,4 });
            }
        }

        if (empty.Count == 0)
        {
            GameObject enemy = Instantiate(enemies[0].enemies[Random.Range(0, enemies[0].enemies.Count)], spawnRow[2].transform);
            allOpponents.Add(enemy);
            enemy.GetComponent<IEnemy>().SetEnemyPlaceSpace(row, new List<int> { 2 });
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
        IEnemy attacker = quarrySorted[quarrySortedKey].GetComponent<IEnemy>();
        if (attacker.GetEnemyStatus().Contains(GameplayStatus.Petrified)) AttackEnding();
        GameInstance.battleManager.BattleEffect = true;
        // Choosing hero to attack
        int biggestAgro = -1;
        if(attacker.GetEnemyHealth() <= 0)
        {
            AttackEnding();
        }
        enemyTurn.Invoke("Enemy turn");
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

        if (targetIndexInOpponents > -1) 
        {
            IHero targetHero = allOpponents[targetIndexInOpponents].GetComponent<IHero>();
            targetHero.ApplySpellToHero(attacker.enemyAttack(), allOpponents[targetIndexInOpponents]);
        }

    }

    bool IfActiveOpponentIsEnemy()
    {
        if (quarrySorted.Count <= 0 || quarrySorted[quarrySortedKey]== null) return false;
        if (quarrySorted[quarrySortedKey].GetComponent<IEnemy>() != null) return true;
        else return false;
    }


    public List<IEnemy> GetEnemies()
    {
        List<IEnemy> enemiesList = new List<IEnemy>();

        foreach(GameObject g in allOpponents)
        {
            if(g.GetComponent<IEnemy>() != null)
            {
                enemiesList.Add(g.GetComponent<IEnemy>());
            }
        }

        return enemiesList;
    }


    public void EndOfTheTurn()
    {
        actionCounter++;
        battlePassTime(actionCounter);
        quarrySorted.Remove(quarrySortedKey);
        quarrySortedKey++;
        targetIndexInOpponents = -1;
        enemyTurn.Invoke("");
        PlayerTurn.Invoke();
        if (IfActiveOpponentIsEnemy())
        {

            EnemyAutoAttack();

        }
        if (quarrySorted.ContainsKey(quarrySortedKey))
        {
            if(quarrySorted.TryGetValue(quarrySortedKey, out GameObject g))
            {
                if (quarrySorted.Count != 0 && g !=null)
                {

                    if (g.GetComponent<IHero>() != null)
                    { 
                        IHero newactivehero = g.GetComponent<IHero>();
                        GameInstance.party.BattleHeroSwitch(newactivehero.GetThisHero());
                        if (newactivehero.GetHeroHealth() <= 0 || 
                            newactivehero.GetHeroStatus().Contains(GameplayStatus.Petrified) ||
                            newactivehero.GetHeroStatus().Contains(GameplayStatus.Stunned) ) AttackEnding();
                    }
                }
            }
        }



        if (quarrySorted.Count == 0) EndOfRound();

        GetRidOfDeadEnemies();
        //refresh initiative of all 
        // apply damage and 
    }
    public void EndOfRound()
    {
       // print("end of the round");

        quarrySortedKey = 0;
        //GetRidOfDeadEnemies();
        SortingOpponents();
        CheckForEmptyRow();

        if (IfActiveOpponentIsEnemy())
        {
            EnemyAutoAttack();
        }
    }

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



    int WhoWon()
    {
        int enemyHealth = 0;
        int heroesHealth = 0;
        int petrifiedEnemies = 0, petrifiedHeroes = 0;
        int enemyCount = 0;
        foreach (GameObject g in allOpponents)
        { 
            if (g.GetComponent<IHero>() != null)
            {
                heroesHealth += g.GetComponent<IHero>().GetHeroHealth();
                if (g.GetComponent<IHero>().GetHeroStatus().Contains(GameplayStatus.Petrified)) petrifiedHeroes++;
            }
            else if(g.GetComponent<IEnemy>() != null)
            {
                enemyHealth += g.GetComponent<IEnemy>().GetEnemyHealth();
                if (g.GetComponent<IEnemy>().GetEnemyStatus().Contains(GameplayStatus.Petrified)) petrifiedEnemies++;
                enemyCount++;
            }
        }

        if (enemyHealth <= 0) return 1;
        if (heroesHealth <= 0) return 2;
        if(petrifiedHeroes == 4) return 2;
        if(petrifiedEnemies == enemyCount) return 1;


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
                }
            }
            if (g.GetComponent<IHero>() != null)
            {
                if (g.GetComponent<IHero>().GetHeroHealth() > 0)
                {
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
                    if (!quarrySorted.ContainsValue(g)) quarrySorted.Add(quarrySorted.Count, g);
                }
            }
        }

        foreach (KeyValuePair<int, GameObject> k in quarrySorted)
        {
            //print(k.Key + " " + k.Value.name + " " + k.Value.GetComponent<IBattle>().GetInitiativeInBattle());
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
                    toErase.Add(i);
                }
            }
            foreach(int i in toErase)
            {
                DestroyImmediate(allOpponents[i]);
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

        StartCoroutine(GameInstance.TimeStep());
        GameInstance.party.SetTimerForHeroes(false);
    }
    
    public void ReceiveAttackInput()
    {
        if (quarrySorted[quarrySortedKey] == null)
        {
            AttackEnding();
            return;
        }
        if (quarrySorted[quarrySortedKey].GetComponent<IHero>() == null) 
        {
            AttackEnding();
            return; 
        }

        IHero attacker = quarrySorted[quarrySortedKey].GetComponent<IHero>();
        GameInstance.spellbook.CastSpell(attacker.GetWeaponSpell());
    }

    private void FindEnemyOpponent()
    {
        if (targetIndexInOpponents < 0)
        {
            for (int i = 0; i < allOpponents.Count; i++)
            {
                if (allOpponents[i].GetComponent<IEnemy>() != null) 
                {
                    if (allOpponents[i].GetComponent<IEnemy>().GetEnemyRow() == 1)
                    { 
                        targetIndexInOpponents = i;
                        break; 
                    }
                }
            }
        }
    }

    public void ReceiveLastSpellInput()
    {
        if (quarrySorted[quarrySortedKey].GetComponent<IHero>() == null) return;


        // if spell AOE roll through all with current spell of a hero
        //if spell no AOE wait for cursor input
    }


    public void BattleSpellAgro(int amount)
    {
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
        GameInstance.battleManager.BattleEffect = false;
        if (WhoWon() == 0) EndOfTheTurn();
    }


    public void ChooseTargetForSpell(GameObject opponentGameObject, SpellContainer spellContainer)
    {
        if(allOpponents.Contains(opponentGameObject))
        {
            //print("target found applying spell ");
            //GetComponent<IHero>()
            targetIndexInOpponents = allOpponents.IndexOf(opponentGameObject);
        }

        if (opponentGameObject.GetComponent<IEnemy>() != null)
        {
           // allOpponents[targetIndexInOpponents].GetComponent<IEnemy>().CastSpellOnEnemy(spellContainer, opponentGameObject);
            EndOfTheTurn();
        }
        if (opponentGameObject.GetComponent<IHero>() != null)
        {

            EndOfTheTurn();
        }
        //apply spell to chosen opponent
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