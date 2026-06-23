using UnityEditor;
using UnityEngine;

public class GamePlayHelper : Editor
{
    [MenuItem("Tools/GamePlayHelper/End Stage")]
    private static void EndStage()
    {
        GameManager.Instance.ExitStage();
    }
}
