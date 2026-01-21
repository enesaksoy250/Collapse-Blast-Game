using UnityEngine;

public enum BlockColorType
{
    Red,
    Green,
    Blue,
    Yellow,
    Purple,
    Pink
}

[CreateAssetMenu(fileName = "BlockColorData", menuName = "CollapseBlast/Block Color Data")]
public sealed class BlockColorData : ScriptableObject
{
    [Tooltip("Color type of this block")]
    public BlockColorType colorType;

    [Header("Icons")]
    [Tooltip("Default icon shown for small groups")]
    public Sprite defaultIcon;

    [Tooltip("Icon shown when group size > A")]
    public Sprite iconA;

    [Tooltip("Icon shown when group size > B")]
    public Sprite iconB;

    [Tooltip("Icon shown when group size > C")]
    public Sprite iconC;

    public Sprite GetSprite(IconState state)
    {
        switch (state)
        {
            case IconState.IconC:
                return iconC != null ? iconC : defaultIcon;
            case IconState.IconB:
                return iconB != null ? iconB : defaultIcon;
            case IconState.IconA:
                return iconA != null ? iconA : defaultIcon;
            default:
                return defaultIcon;
        }
    }
}
