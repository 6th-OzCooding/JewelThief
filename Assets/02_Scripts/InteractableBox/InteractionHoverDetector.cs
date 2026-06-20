using TeamConvention.Interfaces;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 카메라 화면 중앙에서 Raycast를 수행해 Hover 정보 대상 감지와 팝업 열기/닫기를 처리합니다.
/// </summary>
public class InteractionHoverDetector : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private Camera _targetCamera;
    [SerializeField] private float _detectDistance = 3f;
    [SerializeField] private LayerMask _targetLayerMask = ~0;
    [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore;

    private IInteractable _currentTarget;

    private void Awake()
    {
        if (_targetCamera == null)
            _targetCamera = Camera.main;
    }

    private void Update()
    {
        IInteractable detectedTarget = DetectInteractableTarget();
        if (detectedTarget == _currentTarget)
            return;

        _currentTarget = detectedTarget;

        if (_currentTarget == null)
        {
            CloseItemInfoPopupUI();
            return;
        }


        // OpenItemInfoPopupUI();

        // _currentTarget(IInteractable이 아이템인지 Object인지 함정인지 등을 판단하는 메서드 필요
        SoltingInfoPopUpUIAndOpenPopUp(_currentTarget);
    }

    private HoverInfoTarget DetectHoverTarget()
    {
        if (_targetCamera == null)
            return null;

        Ray ray = _targetCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit, _detectDistance, _targetLayerMask, _triggerInteraction))
            return null;

        if (hit.collider.TryGetComponent(out HoverInfoTarget target))
            return target;

        return hit.collider.GetComponentInParent<HoverInfoTarget>();
    }

    private IInteractable DetectInteractableTarget()
    {
        if (_targetCamera == null)
            return null;

        Ray ray = _targetCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit, _detectDistance, _targetLayerMask, _triggerInteraction))
            return null;

        if (hit.collider.TryGetComponent(out IInteractable target))
            return target;

        return hit.collider.GetComponentInParent<IInteractable>();
    }

    private void OpenItemInfoPopupUI()
    {
        if (UIManager.Instance == null)
            return;

        UIManager.Instance.OpenItemInfoPopupUI();
    }

    private void CloseItemInfoPopupUI()
    {
        if (UIManager.Instance == null)
            return;

        UIManager.Instance.CloseItemInfoPopupUI();
    }

    private void SoltingInfoPopUpUIAndOpenPopUp(IInteractable interactObj)
    {
        if (UIManager.Instance == null || interactObj == null)
            return;

        string dataId = interactObj.InteractPrompt;

        if(dataId.Contains("Object"))
        {
            OpenInfoPopUpUI(UIType.ObjectInfoPopupUI, dataId);
        }
    }

    private void OpenInfoPopUpUI(UIType uiType, string dataId)
    {
        switch(uiType)
        {
            case UIType.ObjectInfoPopupUI:
                UIBase uiObj = UIManager.Instance.OpenObjectInfoPopupUI();
                if (uiObj == null)
                {
                    return;
                }
                
                if(uiObj.TryGetComponent<ObjectInfoPopupUI>(out ObjectInfoPopupUI infoPopUpUI))
                {
                    infoPopUpUI.SetObjectComentText
                        (GameManager.DataTable.GetPoolingInteractableObjectData(dataId).ObjName);
                    infoPopUpUI.SetObjectComentText
                        (GameManager.DataTable.GetPoolingInteractableObjectData(dataId).ObjectComment);
                }
                break;
        }
    }
}
