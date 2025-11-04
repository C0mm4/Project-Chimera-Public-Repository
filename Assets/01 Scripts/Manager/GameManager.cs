using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public PlayerStatus Player;

    //public Dictionary<string, List<GameObject>> cardList = new Dictionary<string, List<GameObject>>();
    public Dictionary<string, List<ScriptableObject>> cardList { get; set; }
        = new ()
        {
            { "L", new () },
            { "E", new () },
            { "R", new () },
            { "C", new () }
        };

//    [Header("Inspector에서 보기용")]
//    public List<CardListEntry> cardListInspector = new();

    private void Awake()
    {
        //테스트용 더미 카드 생성
        TestData();
        

        // 인스펙터용 리스트로 복사
        SyncCardListToInspector();
    }

    public void SyncCardListToInspector()
    {
/*        cardListInspector = cardList
            .Select(kv => new CardListEntry { key = kv.Key, value = kv.Value })
            .ToList();*/
    }


    [Header("소지 카드 확인용")]
    public List<ScriptableObject> lCard = new();
    public List<ScriptableObject> eCard = new();
    public List<ScriptableObject> rCard = new();
    public List<ScriptableObject> cCard = new();

    [Header("모든카드 SO Data")]
    public List<ScriptableObject> cardDatas = new();


    public void GameStart()
    {
       
    }

    private async void Start()
    {
        UIManager.Instance.OpenUI<GameplayUI>();
        // 볼륨 설정값 반영위해서 열었다 닫기
        var ui = await UIManager.Instance.GetUI<SettingUI>();
        ui.OpenUI();
        ui.CloseUI();

        SoundManager.Instance.PlayBGM("MainBGM");
        TutorialManager test = TutorialManager.Instance;
        //so라벨화 로드
        StartCoroutine(SODataLoad("cardSO", true));

        //처음 가지고 있는 카드 세팅 해야함
        //StartCoroutine(SODataLoad("startCardSO", true));
    }

    private void StartCardSetting(int soNumber)
    {
        StageManager.data.cardInventory[soNumber] = new();
        StageManager.data.cardInventory[soNumber]++;
    }


    public void GameSave()
    {
        var data = JsonConvert.SerializeObject(StageManager.data);
      //  Debug.Log(data);

        CryptoManager crypt = new();
        var encrypted = crypt.EncryptString(data);

#if UNITY_WEBGL
        PlayerPrefs.SetString("SaveData", encrypted);
        PlayerPrefs.Save();
#else
        string path = string.Concat(Application.persistentDataPath, "/save.dat");

        File.WriteAllText(path, encrypted);

#endif
    }

    public void GameLoad()
    {

        string encryptedData = "";
        string json = "";
        CryptoManager crypt = new();

#if UNITY_WEBGL

        if (PlayerPrefs.HasKey("SaveData"))
        {
            encryptedData = PlayerPrefs.GetString("SaveData");
        }
        else
            return;
#else
        string path = string.Concat(Application.persistentDataPath, "/save.dat");

        if (!File.Exists(path))
        {
         //   Debug.Log("There is no save data");
            return;
        }

        encryptedData = File.ReadAllText(path);
#endif

        if (string.IsNullOrEmpty(encryptedData))
        {
           // Debug.LogWarning("No valid encrypted data found.");
            return;
        }

        json = crypt.DecryptString(encryptedData);
       // Debug.Log(json);

        try
        {
            var data = JsonConvert.DeserializeObject<GameData>(json);
           // Debug.Log(data.structureCards.Count);
            foreach (var d in data.structureCards.Keys)
            {
               // Debug.Log(d);
            }
            StageManager.data = data;
            StageManager.Instance.Stage.seedController.LoadStructure();
        }
        catch (Exception e)
        {
          //  Debug.LogError($"Load Game Data Fail. Error : {e.Message} exception!");
        }
    }

    private IEnumerator SODataLoad(string label,bool addData)
    {
        bool isDone = false;
        List<ScriptableObject> loadedList = null;

        DataManager.Instance.LoadSODataByLabel<ScriptableObject>(label, (list) =>
        {
            loadedList = new List<ScriptableObject>(list);
            isDone = true;
        });

        yield return new WaitUntil(() => isDone);

        
        foreach (ScriptableObject data in loadedList)
        {
            if (data is StructureSO structure)
                if (addData)
                    cardDatas.Add(structure);
                else
                    StartCardSetting(structure.soNumber);
            else if (data is WeaponDataSO weapon)
                if(addData)
                    cardDatas.Add(weapon);
                else
                    StartCardSetting(weapon.id);

            yield return null; 
        }

    }

    private void TestData()
    {
        StageManager.data.cardInventory[301000] = new();
        StageManager.data.cardInventory[302000] = new();
        StageManager.data.cardInventory[303000] = new();

        StageManager.data.cardInventory[330001] = new();
        StageManager.data.cardInventory[330000] = new();

        StageManager.data.cardInventory[301000] = 10;
        StageManager.data.cardInventory[302000] = 10;
        StageManager.data.cardInventory[303000] = 10;

        StageManager.data.cardInventory[330001] = 35;
        StageManager.data.cardInventory[330000] = 26;

        StageManager.Instance.GetGold(1000,true);
        StageManager.data.cardDust = 40;
    }
}
