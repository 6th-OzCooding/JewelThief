using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.LookDev;

public class ObjectSpawnEditor : EditorWindow
{
    private enum ItemObjectType
    {
        Jewel,
        Tool,
        Interactable,
        Painting,
        Statue
    }

    private string _jewelObjectAddress = "Pool_Jewel";
    private string _toolObjectAddress = "Pool_Tool";
    private string _PaintingObjectAddress = "Pool_Painting";
    private string _StatueObjectAddress = "Pool_Statue";
    private string _interactableObjectAddress = "InteractableContainer_Prefab";
    // private string _interactableObjectAddress = "Door_Prefab";

    private static string[] _jewelItemIds =
    {
        "Item_Jewel_Diamond",
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
    private static string[] _interactableItemIds =
    {
        
        "Object_01",
        "Object_02",
        "Object_03",
        "Object_04",
        "Object_05",
        "Object_06"
        
        // "Door_01"
    };
    private static string[] _PaintingItemId =
    {
        "Item_Painting_01",
        "Item_Painting_02",
        "Item_Painting_03",
        "Item_Painting_04",
        "Item_Painting_05"
    };
    private static string[] _StatueItemId =
    {
        "Item_Statue_Stone",
        "Item_Statue_Copper",
        "Item_Statue_Metal",
        "Item_Statue_Marble"
    };
    private static StageRuntimeInterface[] _stageRuntimeInterfaces;

    private ItemObjectType _selectedType = ItemObjectType.Jewel;

    private bool _showJewelObject = true;
    private bool _showToolObject = false;
    private bool _showInteractableObject = false;
    private bool _showPaintingObject = false;
    private bool _showStatueObject = false;

    private int _selectedJewelIndex = 0;
    private int _selectedToolIndex = 0;
    private int _selectedInteractableIndex = 0;
    private int _selectedPaintingIndex = 0;
    private int _selectedStatueIndex = 0;

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
        EditorGUILayout.Space(4);
        DrawInteractableObjectSection();
        EditorGUILayout.Space(4);
        DrawPaintingObjectSection();
        EditorGUILayout.Space(4);
        DrawStatueObjectSection();

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

    private void DrawInteractableObjectSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        DrawSelectableFoldoutHeader(
            ItemObjectType.Interactable,
            ref _showInteractableObject,
            "InteractableObject"
        );

        if (_showInteractableObject)
        {
            EditorGUI.indentLevel++;
            using (new EditorGUI.DisabledScope(_selectedType != ItemObjectType.Interactable))
            {
                _selectedInteractableIndex = EditorGUILayout.Popup(
                    "Interactable Item",
                    _selectedInteractableIndex,
                    _interactableItemIds
                );
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
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

    private void DrawPaintingObjectSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        DrawSelectableFoldoutHeader(
            ItemObjectType.Painting,
            ref _showPaintingObject,
            "PaintingObject"
        );

        if (_showPaintingObject)
        {
            EditorGUI.indentLevel++;

            using (new EditorGUI.DisabledScope(_selectedType != ItemObjectType.Painting))
            {
                _selectedPaintingIndex = EditorGUILayout.Popup(
                    "Painting Item",
                    _selectedPaintingIndex,
                    _PaintingItemId
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

    //추가: 조각상 스폰 섹션
    private void DrawStatueObjectSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        DrawSelectableFoldoutHeader(
            ItemObjectType.Statue,
            ref _showStatueObject,
            "StatueObject"
        );

        if (_showStatueObject)
        {
            EditorGUI.indentLevel++;

            using (new EditorGUI.DisabledScope(_selectedType != ItemObjectType.Statue))
            {
                _selectedStatueIndex = EditorGUILayout.Popup(
                    "Statue Item",
                    _selectedStatueIndex,
                    _StatueItemId
                );
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
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

        GameObject spawnedObject = GameManager.Pool.SpawnFromPool(objectAddress, _spawnPosition, Quaternion.identity);

        switch (_selectedType)
        {
            case ItemObjectType.Jewel:
                spawnedObject.GetComponent<Jewel>().InitFromSpawner(itemId);
                break;
            case ItemObjectType.Tool:
                spawnedObject.GetComponent<Tool>().InitFromSpawner(itemId);
                break;
            case ItemObjectType.Interactable:
                spawnedObject.GetComponent<BaseDisarmableObejct>().InitFromSpawner(itemId);
                break;
            case ItemObjectType.Painting:
                spawnedObject.GetComponent<Painting>().InitFromSpawner(itemId);
                break;
            case ItemObjectType.Statue:
                spawnedObject.GetComponent<Statue>().InitFromSpawner(itemId);
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
            ItemObjectType.Interactable => _interactableObjectAddress,
            ItemObjectType.Painting => _PaintingObjectAddress,
            ItemObjectType.Statue => _StatueObjectAddress,
            _ => null
        };
    }

    private string GetSelectedItemId()
    {
        return _selectedType switch
        {
            ItemObjectType.Jewel => _jewelItemIds[_selectedJewelIndex],
            ItemObjectType.Tool => _toolItemIds[_selectedToolIndex],
            ItemObjectType.Interactable => _interactableItemIds[_selectedInteractableIndex],
            ItemObjectType.Painting => _PaintingItemId[_selectedPaintingIndex],
            ItemObjectType.Statue => _StatueItemId[_selectedStatueIndex],
            _ => null
        };
    }
}
