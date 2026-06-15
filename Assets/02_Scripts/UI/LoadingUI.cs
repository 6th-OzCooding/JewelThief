using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using TMPro;

public class LoadingUI : MonoBehaviour
{
    [Header("컴포넌트 연결")]
    [SerializeField] private Image LoadingBar;
    [SerializeField] private TextMeshProUGUI LoadingText;

    private ResourceManager _resourceManager;

    private void Awake()
    {
        _resourceManager = new ResourceManager();
    }

    public async UniTask StartLoading()
    {
        LoadingBar.fillAmount = 0;
        LoadingText.text = "데이터 불러 오는 중 ... 0%";

        await _resourceManager.Init(OnResourceLoadProgress);

        LoadingText.text = "로딩 완료";

        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                break;
            }

            await UniTask.Yield();
        }
    }


    private void OnResourceLoadProgress(float progress)
    {
        LoadingBar.fillAmount = progress;

        LoadingText.text = string.Format("데이터 불러오는 중...{0}%", Mathf.RoundToInt(progress * 100));
    }
}