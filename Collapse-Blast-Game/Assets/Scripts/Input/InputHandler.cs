using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private LayerMask blockLayer;

    private bool inputEnabled = true;

    public event System.Action<Vector2Int> OnPositionClicked;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (!inputEnabled)
            return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleClick(Mouse.current.position.ReadValue());
        }
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            HandleClick(Touchscreen.current.primaryTouch.position.ReadValue());
        }
    }

    private void HandleClick(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null)
        {
            BlockView blockView = hit.collider.GetComponent<BlockView>();
            if (blockView != null && blockView.IsActive)
            {
                Vector2Int position = new Vector2Int(blockView.Column, blockView.Row);
                OnPositionClicked?.Invoke(position);
            }
        }
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
    }
}
