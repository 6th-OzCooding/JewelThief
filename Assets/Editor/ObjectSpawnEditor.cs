using UnityEditor;
using UnityEngine;

public class ObjectSpawnEditor : EditorWindow
{
    private enum ItemObjectType
    {
        Jewel,
        Tool
    }

    private string _jewelObjectAddress = "JewelObject";
    private string _toolObjectAddress = "ToolObject";

    private static string[] _jewelItemIds =
    {
        "Item_Jewel__Diamond",
        "Item_Jewel_Amethyst",
        "Item_Jewel_Aquamarine",
        "Item_Jewel_Emerald",
        "Item_Jewel_Ruby",
        "Item_Jewel_Sapphire",
        "Item_Jewel_Topaz"
    };
    private static string[] _toolItemIds =
    {
        "Item_Tool_MasterKey",
        "Item_Tool_Key"
    };

    private ItemObjectType _selectedType = ItemObjectType.Jewel;

    private bool _showJewelObject = true;
    private bool _showToolObject = false;

    private int _selectedJewelIndex = 0;
    private int _selectedToolIndex = 0;

    private bool _useGravity = false;

    private Vector3 _spawnPosition = new Vector3(0f, -99f, 0f);

    [MenuItem("Tools/Item Object Spawner")]
    private static void Open()
    {
        GetWindow<ObjectSpawnEditor>("Item Object Spawner");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Item Object Spawner", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "해당 에디터는 게임을 실행하고, 로딩이 끝난 후 사용해야 합니다.",
            MessageType.Info
        );

        DrawJewelObjectSection();
        EditorGUILayout.Space(4);
        DrawToolObjectSection();

        EditorGUILayout.Space(10);

        _spawnPosition = EditorGUILayout.Vector3Field("Spawn Position", _spawnPosition);

        _useGravity = EditorGUILayout.Toggle("중력 적용", _useGravity);

        EditorGUILayout.Space(10);

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (GUILayout.Button("Spawn Selected Item", GUILayout.Height(32)))
            {
                SpawnSelectedItem();
            }
        }

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "현재 게임 플레이 중이 아닙니다. 이 창은 현재 구조상 플레이 모드에서만 스폰할 수 있습니다.",
                MessageType.Warning
            );
        }
    }

    private void DrawJewelObjectSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        DrawSelectableFoldoutHeader(
            ItemObjectType.Jewel,
            ref _showJewelObject,
            "JewelObject"
        );

        if (_showJewelObject)
        {
            EditorGUI.indentLevel++;

            using (new EditorGUI.DisabledScope(_selectedType != ItemObjectType.Jewel))
            {
                _selectedJewelIndex = EditorGUILayout.Popup(
                    "Jewel Item",
                    _selectedJewelIndex,
                    _jewelItemIds
                );
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawToolObjectSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        DrawSelectableFoldoutHeader(
            ItemObjectType.Tool,
            ref _showToolObject,
            "ToolObject"
        );

        if (_showToolObject)
        {
            EditorGUI.indentLevel++;

            using (new EditorGUI.DisabledScope(_selectedType != ItemObjectType.Tool))
            {
                _selectedToolIndex = EditorGUILayout.Popup(
                    "Tool Item",
                    _selectedToolIndex,
                    _toolItemIds
                );
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawSelectableFoldoutHeader(
        ItemObjectType type,
        ref bool foldout,
        string label
    )
    {
        EditorGUILayout.BeginHorizontal();

        bool isSelected = _selectedType == type;

        bool selectedNow = GUILayout.Toggle(
            isSelected,
            GUIContent.none,
            GUILayout.Width(18)
        );

        if (selectedNow)
        {
            _selectedType = type;
        }

        foldout = EditorGUILayout.Foldout(foldout, label, true);

        EditorGUILayout.EndHorizontal();
    }

    private void SpawnSelectedItem()
    {
        string objectAddress = GetSelectedObjectAddress();
        string itemId = GetSelectedItemId();

        if (string.IsNullOrWhiteSpace(objectAddress))
        {
            Debug.LogError("선택된 오브젝트 Address가 비어 있습니다.");
            return;
        }

        if (string.IsNullOrWhiteSpace(itemId))
        {
            Debug.LogError("선택된 ItemId가 비어 있습니다.");
            return;
        }

        GameObject prefab = GameManager.Resource.GetLoadedAsset<GameObject>(objectAddress);

        if (prefab == null)
        {
            Debug.LogError(
                $"로드된 프리팹을 찾을 수 없습니다. Address: {objectAddress}\n" +
                $"ResourceManager.Init()에서 해당 Address가 미리 로드되었는지 확인하세요."
            );
            return;
        }

        GameObject spawnedObject = Instantiate(prefab, _spawnPosition, Quaternion.identity);

        switch(_selectedType)
        {
            case ItemObjectType.Jewel:
                spawnedObject.GetComponent<Jewel>().InitFromSpawner(itemId);
                break;
            case ItemObjectType.Tool:
                spawnedObject.GetComponent<Tool>().InitFromSpawner(itemId);
                break;
            default:
                Debug.LogError("선택된 오브젝트가 Jewel 또는 Tool이 아닙니다.");
                Destroy(spawnedObject);
                return;
        }

        if (spawnedObject.TryGetComponent<Rigidbody>(out Rigidbody rigidbody))
        {
            rigidbody.useGravity = _useGravity;
        }

        EditorUtility.SetDirty(spawnedObject);
        Selection.activeGameObject = spawnedObject;
    }

    private string GetSelectedObjectAddress()
    {
        return _selectedType switch
        {
            ItemObjectType.Jewel => _jewelObjectAddress,
            ItemObjectType.Tool => _toolObjectAddress,
            _ => null
        };
    }

    private string GetSelectedItemId()
    {
        return _selectedType switch
        {
            ItemObjectType.Jewel => _jewelItemIds[_selectedJewelIndex],
            ItemObjectType.Tool => _toolItemIds[_selectedToolIndex],
            _ => null
        };
    }
}
