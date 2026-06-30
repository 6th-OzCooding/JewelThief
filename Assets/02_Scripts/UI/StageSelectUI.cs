using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StageSelectUI : UIBase
{
    [Header("UI Elements")]
    [SerializeField] private StageButton[] _stageButtons;

    private StageSelectController _stageSelectController;

    private void Start()
    {
        InitializeStageData();
    }

    public void SetController(StageSelectController controller)
    {
        _stageSelectController = controller;
    }

    private void InitializeStageData()
    {
        Dictionary<string, StageData> stageDataTable = GameManager.DataTable.GetStageDataTable();

        if (stageDataTable == null || stageDataTable.Count == 0) return;

        List<StageData> stageDataList = stageDataTable.Values.ToList();

        for (int i = 0; i < _stageButtons.Length; i++)
        {
            if (i >= stageDataList.Count)
            {
                _stageButtons[i].gameObject.SetActive(false);
                continue;
            }

            _stageButtons[i].Init(stageDataList[i], this);
        }
    }

    public void OnStageButtonClicked(StageData stageData)
    {
        Debug.Log($"선택한 스테이지: {stageData.Name} / Id: {stageData.Id}");

        GameManager.Instance.SelectedStageId = stageData.Id;
        GameManager.UI.CloseStageSelectUI();

        if (_stageSelectController != null)
            _stageSelectController.ExitStageSelect();

        GameManager.Instance.EnterInGame(stageData.Id);

    }
}