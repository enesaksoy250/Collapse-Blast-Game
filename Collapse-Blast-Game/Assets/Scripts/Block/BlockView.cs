using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class BlockView : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
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

        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }
    }

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
        UpdateCollider();
    }

    private void UpdateCollider()
    {
        if (spriteRenderer != null && spriteRenderer.sprite != null && boxCollider != null)
        {
            boxCollider.size = spriteRenderer.sprite.bounds.size;
        }
    }

    public void SetGridPosition(int row, int column)
    {
        this.row = row;
        this.column = column;
    }

    public void UpdateIconState(IconState state)
    {
        if (currentIconState == state)
            return;

        currentIconState = state;
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (colorData == null || spriteRenderer == null)
            return;

        spriteRenderer.sprite = colorData.GetSprite(currentIconState);
    }

    public void SetWorldPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void MoveTo(Vector3 targetPosition, float duration, System.Action onComplete = null)
    {
        StopAllCoroutines();
        StartCoroutine(GravityFallCoroutine(targetPosition, onComplete));
    }

    private System.Collections.IEnumerator GravityFallCoroutine(Vector3 targetPosition, System.Action onComplete)
    {
        float velocity = 0f;
        float gravity = 80f;
        float maxSpeed = 50f;

        while (transform.position.y > targetPosition.y + 0.01f)
        {
            velocity += gravity * Time.deltaTime;
            velocity = Mathf.Min(velocity, maxSpeed);

            float moveDistance = velocity * Time.deltaTime;
            float remainingDistance = transform.position.y - targetPosition.y;

            if (moveDistance >= remainingDistance)
            {
                break;
            }

            transform.position += Vector3.down * moveDistance;

            yield return null;
        }

        transform.position = targetPosition;
        onComplete?.Invoke();
    }

    public void MoveInstant(Vector3 targetPosition)
    {
        StopAllCoroutines();
        transform.position = targetPosition;
    }

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

    private void OnMouseDown()
    {
        if (!isActive)
            return;

        BlockClickHandler.OnBlockClicked?.Invoke(this);
    }
}

public static class BlockClickHandler
{
    public static System.Action<BlockView> OnBlockClicked;
}
