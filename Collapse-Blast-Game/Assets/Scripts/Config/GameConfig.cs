using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "CollapseBlast/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Grid Settings")]
    [Tooltip("Number of rows (M)")]
    [Range(2, 10)]
    public int rowCount = 10;

    [Tooltip("Number of columns (N)")]
    [Range(2, 10)]
    public int columnCount = 12;

    [Tooltip("Number of colors (K)")]
    [Range(1, 6)]
    public int colorCount = 6;

    [Header("Group Size Thresholds")]
    [Tooltip("First threshold (A) - groups larger than this show IconA")]
    [Min(2)]
    public int thresholdA = 4;

    [Tooltip("Second threshold (B) - groups larger than this show IconB")]
    [Min(2)]
    public int thresholdB = 7;

    [Tooltip("Third threshold (C) - groups larger than this show IconC")]
    [Min(2)]
    public int thresholdC = 9;

    [Header("Block Settings")]
    [Tooltip("Scale factor for blocks (1 = original sprite size)")]
    public float blockSize = 1f;

    [Tooltip("Spacing between blocks in world units")]
    public float blockSpacing = 0.05f;

    [Header("Animation Settings")]
    [Tooltip("Duration for block fall animation")]
    public float fallDuration = 0.3f;

    [Tooltip("Duration for block spawn animation")]
    public float spawnDuration = 0.2f;

    public IconState GetIconStateForGroupSize(int groupSize)
    {
        if (groupSize > thresholdC)
            return IconState.IconC;
        if (groupSize > thresholdB)
            return IconState.IconB;
        if (groupSize > thresholdA)
            return IconState.IconA;
        return IconState.Default;
    }

    private void OnValidate()
    {
        if (thresholdB <= thresholdA)
            thresholdB = thresholdA + 1;
        if (thresholdC <= thresholdB)
            thresholdC = thresholdB + 1;
    }
}

public enum IconState
{
    Default,
    IconA,
    IconB,
    IconC
}
