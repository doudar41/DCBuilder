using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadLevelOrder : MonoBehaviour
{
    [SerializeField]
    GameObject itemModelPrefab;

    void Start()
    {
        print("start loading ");
        GameInstance.initItems();
        GameInstance.party.LoadEquipment();
        foreach(KeyValuePair< string, SavedState > s in GameInstance.savedItemsState)
        {
            print(s.Key +" "+ s.Value );

            if (GameInstance.GetItemFromSaved(s.Key) != null) print(GameInstance.GetItemFromSaved(s.Key));
            if(s.Value == SavedState.Replaced)
            {
                if (!GameInstance.savedItemsReplaced.ContainsKey(s.Key)) continue ;
                if (GameInstance.savedItemsReplaced[s.Key].level == GameInstance.GetLevelName())
                {
                    print(" continue to execute placement " + GameInstance.savedItemsReplaced[s.Key].container);
                    HeroInventoryItem hII = GameInstance.savedItemsReplaced[s.Key];
                    GameObject item = Instantiate(itemModelPrefab);

                    IItem iItem = item.GetComponent<IItem>();

                    iItem.SetPrefab(hII.container);
                    iItem.InitializeItem(hII.positionReplaced);
                    iItem.SetItemsAmount(hII.stackAmount);

                    iItem.SetGUID(hII._GUID);
                    iItem.RemoveFromParent();
                }
            }
        }

    }


}
