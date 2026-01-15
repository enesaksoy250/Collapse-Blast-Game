using UnityEngine;

namespace CollapseBlast.Input
{
    /// <summary>
    /// Handles user input for block selection.
    /// Uses raycasting to detect clicked blocks.
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        [SerializeField]
        private Camera mainCamera;

        [SerializeField]
        private LayerMask blockLayer;

        private bool inputEnabled = true;

        // Event fired when a valid position is clicked
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

            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                HandleClick();
            }
        }

        /// <summary>
        /// Handles mouse/touch click.
        /// </summary>
        private void HandleClick()
        {
            Vector3 mousePosition = UnityEngine.Input.mousePosition;
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null)
            {
                Block.BlockView blockView = hit.collider.GetComponent<Block.BlockView>();
                if (blockView != null && blockView.IsActive)
                {
                    Vector2Int position = new Vector2Int(blockView.Column, blockView.Row);
                    OnPositionClicked?.Invoke(position);
                }
            }
        }

        /// <summary>
        /// Enables or disables input handling.
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            inputEnabled = enabled;
        }
    }
}
