using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using TMPro;

public class LoadingUI : UIBase
{
    [Header("컴포넌트 연결")]
    [SerializeField] private Image LoadingBar;
    [SerializeField] private TextMeshProUGUI LoadingText;

    public async UniTask StartLoading()
    {
        LoadingBar.fillAmount = 0;
        LoadingText.text = "데이터 불러 오는 중 ... 0%";

        await GameManager.Resource.Init(OnResourceLoadProgress);

        LoadingText.text = "로딩 완료. [Enter]";

        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                break;
            }

            await UniTask.Yield();
        }

        UIManager.Instance.CloseLoadingUI();
        UIManager.Instance.ExitGameplayCursorMode();
        UIManager.Instance.OpenUI(UIRootType.MainUI, UIType.TitleUI);
    }


    private void OnResourceLoadProgress(float progress)
    {
        LoadingBar.fillAmount = progress;

        LoadingText.text = string.Format("데이터 불러오는 중...{0}%", Mathf.RoundToInt(progress * 100));
    }
}