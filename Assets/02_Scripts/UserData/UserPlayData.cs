using System;
using System.IO;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public int Gold;
    public int ClearStage;
}

public class UserPlayData : Security, IUserData
{
    private PlayerData _playerData;

    private const string KEY = "TESTKEY_1234";
    private string PATH = Path.Combine(Application.dataPath, "Data/PlayerData.json");
    //private string PATH = Path.Combine(Application.persistentDataPath, "Data/PlayerData.json");

    public void SetDefaultData()
    {
        _playerData = new PlayerData
        {
            Gold = 0,
            ClearStage = 0
        };
    }


    public bool SaveData()
    {
        bool result = false;

        try
        {
            string jsonData = JsonUtility.ToJson(_playerData);
            File.WriteAllText(PATH, jsonData);
            //File.WriteAllText(PATH, Encrypt(jsonData, KEY));

            result = true;
        }
        catch (Exception e)
        {
            Debug.Log($"Save Failed: {e.Message}");
        }

        return result;
    }

    public bool LoadData()
    {
        bool result = false;

        try
        {
            if (!File.Exists(PATH))
            {
                SetDefaultData();
            }
            else
            {
                string loadJson = File.ReadAllText(PATH);
                _playerData = JsonUtility.FromJson<PlayerData>(loadJson);
                //_playerData = JsonUtility.FromJson<PlayerData>(Decrypt(loadJson, KEY));
            }

            result = true;
        }
        catch (Exception e)
        {
            Debug.Log($"Load Failed: {e.Message}");
        }

        return result;
    }
}
