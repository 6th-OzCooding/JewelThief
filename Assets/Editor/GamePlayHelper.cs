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

    [MenuItem("Tools/GamePlayHelper/SpawnKey")]
    private static void SpawnKey()
    {
        //var key = new Key();
        //key.InitFromSpawner("Item_Tool_Key");

        var keyObject = GameManager.Resource.GetLoadedAsset<GameObject>("ToolObject ");
        var a = GameObject.Instantiate(keyObject, Vector3.zero, Quaternion.identity);
        a.GetComponent<Tool>().InitFromSpawner("Item_Tool_MasterKey");
        a.GetComponent<Rigidbody>().useGravity = false;
    }
}
