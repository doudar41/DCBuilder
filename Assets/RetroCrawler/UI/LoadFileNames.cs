using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class LoadFileNames : MonoBehaviour
{
    [SerializeField] TMP_InputField fileNameInput;
    [SerializeField] ToggleGroup toggleGroup;
    [SerializeField] GameObject saveLoadTransform;
    [SerializeField] GameObject saveFilePrefab;
    List<SaveFileToggleContainer> listOfFiles = new List<SaveFileToggleContainer>();
    [SerializeField] GameObject SaveGamePanel;

    public void EnableSavePanel()
    {
        GameInstance.playerController.EnterHover(HoverUIElementEnum.INVENTORY, SaveGamePanel);
        SaveGamePanel.SetActive(true);
        RefreshFileToggles();
    }


    public void DisableSavePanel()
    {
        SaveGamePanel.SetActive(false);
        GameInstance.playerController.ExitHover();
    }

    private void Start()
    {
       if(fileNameInput!=null) fileNameInput.onEndEdit.AddListener(AddFileNameToList);
    }

    private void AddFileNameToList(string fileName)
    {
        GameInstance.AddNewFileName(fileName);
        GameInstance.SaveFile(fileName);        
        fileNameInput.gameObject.SetActive(false);
        RefreshFileToggles();
    }

    public void RemoveFileFromList()
    {
        string filename = toggleGroup.GetFirstActiveToggle().gameObject.GetComponent<SaveFileToggleContainer>().fileNameToggle;
        print("Remove file "+ filename);
        GameInstance.RemoveFileName(filename);
        //toggleGroup.GetFirstActiveToggle().gameObject.GetComponent<SaveFileToggleContainer>().fileNameToggle = "Empty slot";
        RefreshFileToggles();
    }

    public void RemoveAllFiles()
    {
        GameInstance.ClearAllSaves();
        RefreshFileToggles();
    }


    public void RefreshFileToggles()
    {
        List<string> list  = GameInstance.GetFileNameList();
        foreach (string fname in list)
        {
            Debug.Log(fname + "file loaded");
        }
        print(" list of files " + list.Count);

        for (int i = 0; i< list.Count;i++)
        {
            if (i < (listOfFiles.Count))
            {
                listOfFiles[i].Visibility(true);
                listOfFiles[i].SetFileName(list[i]);
            }
            else
            {
                GameObject newToggle = Instantiate(saveFilePrefab, saveLoadTransform.transform);
                newToggle.GetComponent<SaveFileToggleContainer>().SetFileName(list[i]);
                newToggle.GetComponent<Toggle>().group = toggleGroup;
                listOfFiles.Add(newToggle.GetComponent<SaveFileToggleContainer>());
                newToggle.GetComponent<SaveFileToggleContainer>().Visibility(true);
            }
        }

        if(list.Count< listOfFiles.Count)
        {
            print(" less toggles than names "+ (listOfFiles.Count - list.Count));
            for (int i = list.Count; i < listOfFiles.Count; i++)
            {
                listOfFiles[i].SetFileName("Empty slot");
                listOfFiles[i].Visibility(false);
            }
        }
        if(list.Count> listOfFiles.Count)
        {
            for (int i = listOfFiles.Count; i < list.Count; i++)
            {
                listOfFiles[i].SetFileName("Empty slot");
                listOfFiles[i].Visibility(false);
            }
        }
    }

    public void LoadFile()
    {
        if(toggleGroup.GetFirstActiveToggle() == null) { print("no files to load"); return; }
        GameInstance.LoadFile(toggleGroup.GetFirstActiveToggle().gameObject.GetComponent<SaveFileToggleContainer>().fileNameToggle);
    }


    public void OverrideSaveFile()
    {
        string fileName = toggleGroup.GetFirstActiveToggle().gameObject.GetComponent<SaveFileToggleContainer>().fileNameToggle;
        GameInstance.SaveFile(fileName);
        RefreshFileToggles();
    }

    private void OnDestroy()
    {
        fileNameInput.onEndEdit.RemoveAllListeners();
    }
}
