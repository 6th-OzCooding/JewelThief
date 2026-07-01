using UnityEditor;
using UnityEngine;

public class GamePlayHelper : Editor
{
    [MenuItem("Tools/GamePlayHelper/Add Gold : 500")]
    private static void AddGold()
    {
        GameManager.Instance.AddGold(500);
    }

    [MenuItem("Tools/GamePlayHelper/End Stage")]
    private static void EndStage()
    {
        GameManager.Instance.ReturnToLobby();
    }

    [MenuItem("Tools/GamePlayHelper/Save Data")]
    private static void SaveData()
    {
        GameManager.UserData.SaveAllUserData();
    }

    [MenuItem("Tools/GamePlayHelper/Load Data")]
    private static void LoadData()
    {
        GameManager.UserData.LoadAllUserData();
    }
}