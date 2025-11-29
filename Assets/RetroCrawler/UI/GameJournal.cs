using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class GameJournal : MonoBehaviour
{
    [SerializeField] GameObject entryTextPrefab;
    [SerializeField] GameObject pagePrefab;
    [SerializeField] GameObject dividerSprite;
    [SerializeField] GameObject[] pagePlaces = new GameObject[2];
    [SerializeField] GameObject journalPanel;
    [SerializeField] Camera cam;
    List<GameObject> entries = new List<GameObject>();
    List<GameObject> pages = new List<GameObject>();
    int pageOpenedIndex = 0;

    //Dialogue archive
    //Quest incomplete
    //Quests complete

    //load journal entries place entries to pages on load
    //add journal entry
    //add dialogue entry
    private void Awake()
    {
        GameInstance.gameJournal = this;
        GameInstance.initItems += JournalInit;
    }
    private void OnDestroy()
    {
        GameInstance.initItems -= JournalInit;
    }
    private void Start()
    {
/*        AddEntryToJournal("Let adventure begins!");

        AddEntryToJournal("Finally reached the infamous ChaosKeep.There are stories aplenty about this place and the variety of heroes and villains that come here in search of power or treasure. I should review the royal missive in my bag to ensure I don’t stumble into un-warranted troubles");
        AddEntryToJournal("Hear ye, Hear ye! His Royal crownship and keeper of our realm has hereby called all ye heroes and villains, all ye Death Crusaders, for one glorious quest! Unbound treasure and secrets await you in ChaosKeep! Make yer way to the Unwavering Hag. There, enquire for the royal advisor to the crown, the very own Duke Hitherin, for your first trial.");
*/
    }

    public void OpenJournal(bool open)
    {
        if(!open)
        {
            cam.depth = -1;
            journalPanel.SetActive(false) ; return;
        }
        if (pages.Count == 0) JournalInit();
        cam.depth = 2;
        journalPanel.SetActive(true);
        foreach(GameObject g in pages)
        {
            g.SetActive(false);
        }
        if (pages.Count == 0) return;
        pages[pageOpenedIndex].SetActive(true);
        if(pageOpenedIndex%2 == 0 && pages.Count > pageOpenedIndex+1)
        {
            pages[pageOpenedIndex + 1].SetActive(true);
        }
        if(pageOpenedIndex % 2 == 1)
        {
            pages[pageOpenedIndex - 1].SetActive(true);
        }
    }

    void JournalInit()
    {
        //print("gameinstance entries count "+ GameInstance.journalEntries.Count);
        if (GameInstance.journalEntries.Count == 0) return;
        foreach(string entry in GameInstance.journalEntries)
        {
            AddEntryToJournal(entry);
        }

    }



    public void AddEntryToJournal(string entryText)
    {
        int charCount = entryText.Length;
        int charCountMax = 700;


        if(pages.Count == 0)
        {
            GameObject pageBox = Instantiate(pagePrefab, pagePlaces[0].transform);
            pages.Add(pageBox);
        }
        pageOpenedIndex = pages.Count - 1;

        if (pages[pageOpenedIndex].transform.childCount > 0)
        {
            for (int i=0;i< pages[pageOpenedIndex].transform.childCount;i++)
            {

                if (pages[pageOpenedIndex].transform.GetChild(i).GetComponent<TextMeshProUGUI>() !=null) charCount += pages[pageOpenedIndex].transform.GetChild(i).GetComponent<TextMeshProUGUI>().text.Length;
                if (charCount > charCountMax)
                {

                    pageOpenedIndex++;
                    GameObject pageBox1 = Instantiate(pagePrefab, pagePlaces[pageOpenedIndex % 2].transform);
                    pages.Add(pageBox1);
                    charCount = 0;
                }
                //print(charCount);
            }
            GameObject entryBox = Instantiate(entryTextPrefab, pages[pageOpenedIndex % 2].transform);
            entryBox.GetComponent<TextMeshProUGUI>().text = entryText;
            entries.Add(entryBox);
        }
        if(pages[pageOpenedIndex].transform.childCount == 0)
        {

            GameObject entryBox = Instantiate(entryTextPrefab, pages[pageOpenedIndex].transform);
            entryBox.GetComponent<TextMeshProUGUI>().text = entryText;
            entries.Add(entryBox);
        }

       GameObject textdivider = Instantiate(dividerSprite, pages[pageOpenedIndex].transform);

       if(!GameInstance.journalEntries.Contains(entryText)) GameInstance.journalEntries.Add(entryText);
    }
}

