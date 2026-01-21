using UnityEngine;
using System;
using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public sealed class Block : MonoBehaviour
{

    private GameConfig gameConfig;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private int row;
    private int column;
    private BlockColorData colorData;
    private IconState currentIconState;
    private bool isActive;
    private Tween currentTween;

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

    public void Initialize(int row, int column, BlockColorData colorData, GameConfig gameConfig)
    {
        this.row = row;
        this.column = column;
        this.colorData = colorData;
        this.gameConfig = gameConfig;
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

    public void MoveTo(Vector3 targetPosition, Action onComplete = null)
    {

        currentTween?.Kill();

        float distance = Mathf.Abs(transform.position.y - targetPosition.y);
        float duration = CalculateFallDuration(distance);


        currentTween = transform
            .DOMoveY(targetPosition.y, duration)
            .SetEase(Ease.InQuad)
            .OnComplete(() => onComplete?.Invoke());
    }

    private float CalculateFallDuration(float distance)
    {
        if (distance <= 0) return 0f;

        float gravity = gameConfig.gravity;
        float duration = Mathf.Sqrt(2f * distance / gravity);

        return Mathf.Clamp(duration, 0.05f, 1f);
    }

    public void Reset()
    {
        currentTween?.Kill();
        currentTween = null;

        row = -1;
        column = -1;
        colorData = null;
        currentIconState = IconState.Default;
        isActive = false;
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        currentTween?.Kill();
    }
}
