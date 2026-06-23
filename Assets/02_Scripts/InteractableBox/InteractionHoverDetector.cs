using TeamConvention.Interfaces;
using UnityEngine;

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
            CloseObjectInfoPopupUI();
            return;
        }

        OpenObjectInfoPopupUI(_currentTarget);
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

    private void CloseObjectInfoPopupUI()
    {
        GameManager.UI.CloseItemInfoPopupUI();
    }

    private void OpenObjectInfoPopupUI(IInteractable interactObj)
    {
        if (interactObj == null)
            return;

        UIBase uiObj = GameManager.UI.OpenObjectInfoPopupUI();
        if (uiObj == null)
            return;

        if (uiObj.TryGetComponent<ObjectInfoPopupUI>(out ObjectInfoPopupUI infoPopUpUI))
        {
            infoPopUpUI.SetObjectNameText(interactObj.GetName);
        }
    }
}