using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SeedController : MonoBehaviour
{
    private int startIndex = 1;
    public Dictionary<int, ConstructureSeed> seeds = new();


    private async void Awake()
    {
        foreach(Transform obj in transform)
        {
            if(transform != obj)
            {
                var seed = obj.GetComponent<ConstructureSeed>();
                seeds[startIndex] = seed;
                seed.indexNumber = startIndex++;
                seed.GhostVisible();
            }
        }
        var ui = await UIManager.Instance.GetUI<StructureUpgradeUI>();
        ui.CloseUI();
    }

    private void Start()
    {
        StageManager.Instance.OnStageClear += OnStageEnd;
        StageManager.Instance.OnStageFail += OnStageEnd;
        StageManager.Instance.OnStageStart += OnStageStart;
    }

    public async void LoadStructure()
    {
        var structureCardDict = StageManager.data.structureCards;
        var structureLvDict = StageManager.data.structureLevels;

        foreach (var card in structureCardDict.Keys.ToList())
        {
            StructureBase targetStructure = null;

            if(card == 0)
            {
                targetStructure = StageManager.Instance.Basement;
            }
            else if (seeds[card] != null && seeds[card].gameObject.activeSelf)
            {
                targetStructure = await seeds[card].BuildStructure();
            }

            if (targetStructure != null)
            {
                targetStructure.SetDataSO(await DataManager.Instance.GetSOData<StructureSO>(structureCardDict[card]));
                targetStructure.SetLevelStatus(structureLvDict[card]);
            }
        }
    }

    public void OnStageStart()
    {
        foreach(var ghost in seeds.Keys.ToList())
        {
            seeds[ghost].GhostInvisible();
        }
    }

    public void OnStageEnd()
    {
        foreach (var ghost in seeds.Keys.ToList())
        {
            if (seeds[ghost] != null && seeds[ghost].gameObject.activeSelf)
                seeds[ghost].GhostVisible();
        }
    }
}
