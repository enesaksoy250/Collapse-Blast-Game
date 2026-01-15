using UnityEngine;
using CollapseBlast.Config;

namespace CollapseBlast.Block
{
    /// <summary>
    /// Visual representation of a block in the game.
    /// Handles sprite rendering and position updates.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class BlockView : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private int colorIndex;
        private int row;
        private int column;
        private BlockColorData colorData;
        private IconState currentIconState;
        private bool isActive;

        public int ColorIndex => colorIndex;
        public int Row => row;
        public int Column => column;
        public bool IsActive => isActive;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }

        /// <summary>
        /// Initializes the block with color and position data.
        /// </summary>
        public void Initialize(int colorIndex, int row, int column, BlockColorData colorData)
        {
            this.colorIndex = colorIndex;
            this.row = row;
            this.column = column;
            this.colorData = colorData;
            this.currentIconState = IconState.Default;
            this.isActive = true;

            gameObject.SetActive(true);
            UpdateSprite();
        }

        /// <summary>
        /// Updates the block's grid position.
        /// </summary>
        public void SetGridPosition(int row, int column)
        {
            this.row = row;
            this.column = column;
        }

        /// <summary>
        /// Updates the block's icon based on the group size state.
        /// </summary>
        public void UpdateIconState(IconState state)
        {
            if (currentIconState == state)
                return;

            currentIconState = state;
            UpdateSprite();
        }

        /// <summary>
        /// Updates the sprite based on current color and icon state.
        /// </summary>
        private void UpdateSprite()
        {
            if (colorData == null || spriteRenderer == null)
                return;

            spriteRenderer.sprite = colorData.GetSprite(currentIconState);
        }

        /// <summary>
        /// Sets the world position of the block.
        /// </summary>
        public void SetWorldPosition(Vector3 position)
        {
            transform.position = position;
        }

        /// <summary>
        /// Moves the block to target position (for animations).
        /// </summary>
        public void MoveTo(Vector3 targetPosition, float duration, System.Action onComplete = null)
        {
            // Simple lerp movement - can be replaced with DOTween for smoother animation
            StartCoroutine(MoveCoroutine(targetPosition, duration, onComplete));
        }

        private System.Collections.IEnumerator MoveCoroutine(Vector3 targetPosition, float duration, System.Action onComplete)
        {
            Vector3 startPosition = transform.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Ease out quad for smooth landing
                t = 1f - (1f - t) * (1f - t);
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            transform.position = targetPosition;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Resets the block for pooling reuse.
        /// </summary>
        public void Reset()
        {
            colorIndex = -1;
            row = -1;
            column = -1;
            colorData = null;
            currentIconState = IconState.Default;
            isActive = false;
            gameObject.SetActive(false);
            StopAllCoroutines();
        }

        /// <summary>
        /// Called when block is clicked.
        /// </summary>
        private void OnMouseDown()
        {
            if (!isActive)
                return;

            // Notify the game controller about the click
            BlockClickHandler.OnBlockClicked?.Invoke(this);
        }
    }

    /// <summary>
    /// Static class to handle block click events.
    /// Avoids coupling BlockView directly to GameController.
    /// </summary>
    public static class BlockClickHandler
    {
        public static System.Action<BlockView> OnBlockClicked;
    }
}
