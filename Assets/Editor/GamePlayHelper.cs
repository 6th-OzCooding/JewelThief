using UnityEditor;
using UnityEngine;

public class GamePlayHelper : Editor
{
    [MenuItem("Tools/GamePlayHelper/End Stage")]
    private static void EndStage()
    {
        GameManager.Instance.ExitStage();
    }

    [MenuItem("Tools/GamePlayHelper/Save Data")]
    private static void SaveData()
    {
        GameManager.UserData.SaveUserData();
    }

    [MenuItem("Tools/GamePlayHelper/Load Data")]
    private static void LoadData()
    {
        GameManager.UserData.LoadUserData();
    }
}
