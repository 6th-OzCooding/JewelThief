using System;
using System.IO;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public int Gold;
    public int ClearStage;
    public float MouseSensitivity;
    public float MasterVolume;
    public float DisplayMode;
}

public class UserPlayData : Security, IUserData
{
    private PlayerData _playerData;

    private const string KEY = "TESTKEY_1234";
    private string PATH = Path.Combine(Application.dataPath, "Data/PlayerData.json");
    //private string PATH = Path.Combine(Application.persistentDataPath, "Data/PlayerData.json");

    #region Setters

    public void SetGold(int gold)
    {
        _playerData.Gold = gold;
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        _playerData.MouseSensitivity = sensitivity;
    }

    public void SetMasterVolume(float volume)
    {
        _playerData.MasterVolume = volume;
    }

    public void SetDisplayMode(float mode)
    {
        _playerData.DisplayMode = mode;
    }

    #endregion

    #region Getters

    public int GetGold() => _playerData.Gold;
    public float GetMouseSensitivity() => _playerData.MouseSensitivity;
    public float GetMasterVolume() => _playerData.MasterVolume;
    public float GetDisplayMode() => _playerData.DisplayMode;

    #endregion

    public void SetDefaultData()
    {
        _playerData = new PlayerData
        {
            Gold = 500,
            ClearStage = 0
        };
    }


    public bool SaveData()
    {
        bool result = false;

        try
        {
            string jsonData = JsonUtility.ToJson(_playerData);
            File.WriteAllText(PATH, Encrypt(jsonData, KEY));

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
                _playerData = JsonUtility.FromJson<PlayerData>(Decrypt(loadJson, KEY));
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
