using TeamConvention.Interfaces;
using UnityEngine;

public class InteractionHoverDetector : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private Camera _targetCamera;
    [SerializeField] private float _detectDistance = 3f;
    [SerializeField] private LayerMask _targetLayerMask = ~0;
    [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore;

    [Header("Player Context")]
    [SerializeField] private PlayerController _playerController;

    private IInteractable _currentTarget;
    private PopupType _currentPopupType = PopupType.None;

    public IInteractable CurrentTarget => _currentTarget;

    private void Awake()
    {
        if (_targetCamera == null)
            _targetCamera = Camera.main;

        if (_playerController == null)
            _playerController = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        IInteractable detectedTarget = DetectInteractableTarget();
        if (detectedTarget == null)
        {
            ClearCurrentPopup();
            return;
        }

        PopupInfoTarget popupInfoTarget = FindPopupInfoTarget(detectedTarget);
        if (!PopupViewDataBuilder.TryBuild(detectedTarget, popupInfoTarget, _playerController, out PopupDisplayData displayData))
        {
            ClearCurrentPopup();
            return;
        }

        bool shouldRestartAnimation = detectedTarget != _currentTarget || displayData.PopupType != _currentPopupType;
        if (displayData.PopupType != _currentPopupType)
            GameManager.UI.CloseHoverPopupUI();

        OpenPopup(displayData, shouldRestartAnimation);

        _currentTarget = detectedTarget;
        _currentPopupType = displayData.PopupType;
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

    private PopupInfoTarget FindPopupInfoTarget(IInteractable interactable)
    {
        if (interactable is not Component component)
            return null;

        if (component.TryGetComponent(out PopupInfoTarget popupInfoTarget))
            return popupInfoTarget;

        return component.GetComponentInParent<PopupInfoTarget>();
    }

    private void OpenPopup(PopupDisplayData displayData, bool shouldRestartAnimation)
    {
        if (GameManager.UI == null || displayData == null)
            return;

        switch (displayData.PopupType)
        {
            case PopupType.Simple:
                OpenSimplePopup(displayData, shouldRestartAnimation);
                break;

            case PopupType.ItemInfo:
                OpenItemInfoPopup(displayData, shouldRestartAnimation);
                break;

            case PopupType.ShopInfo:
                OpenShopInfoPopup(displayData, shouldRestartAnimation);
                break;

            default:
                ClearCurrentPopup();
                break;
        }
    }

    private void OpenSimplePopup(PopupDisplayData displayData, bool shouldRestartAnimation)
    {
        UIBase uiBase = GameManager.UI.OpenPopupUI(UIType.SimplePopupUI);
        if (uiBase == null)
            return;

        if (!uiBase.TryGetComponent(out SimplePopupUI simplePopupUI))
            return;

        simplePopupUI.SetInfo(displayData);
        if (shouldRestartAnimation)
            simplePopupUI.RestartOpenAnimation();
    }

    private void OpenItemInfoPopup(PopupDisplayData displayData, bool shouldRestartAnimation)
    {
        UIBase uiBase = GameManager.UI.OpenPopupUI(UIType.ItemInfoPopupUI);
        if (uiBase == null)
            return;

        if (!uiBase.TryGetComponent(out ItemInfoPopupUI itemInfoPopupUI))
            return;

        itemInfoPopupUI.SetInfo(displayData);
        if (shouldRestartAnimation)
            itemInfoPopupUI.RestartOpenAnimation();
    }

    private void OpenShopInfoPopup(PopupDisplayData displayData, bool shouldRestartAnimation)
    {
        UIBase uiBase = GameManager.UI.OpenPopupUI(UIType.ShopInfoPopupUI);
        if (uiBase == null)
            return;

        if (!uiBase.TryGetComponent(out ShopInfoPopupUI shopInfoPopupUI))
            return;

        shopInfoPopupUI.SetInfo(displayData);
        if (shouldRestartAnimation)
            shopInfoPopupUI.RestartOpenAnimation();
    }

    private void ClearCurrentPopup()
    {
        if (_currentTarget == null && _currentPopupType == PopupType.None)
            return;

        if (GameManager.UI != null)
            GameManager.UI.CloseHoverPopupUI();

        _currentTarget = null;
        _currentPopupType = PopupType.None;
    }
}
