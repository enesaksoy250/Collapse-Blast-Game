using UnityEngine;

namespace CollapseBlast.Config
{
    /// <summary>
    /// ScriptableObject that holds sprite data for a single block color.
    /// Contains default icon and icons for different group size thresholds.
    /// </summary>
    [CreateAssetMenu(fileName = "BlockColorData", menuName = "CollapseBlast/Block Color Data")]
    public class BlockColorData : ScriptableObject
    {
        [Tooltip("Name of this color (for debugging)")]
        public string colorName;

        [Header("Icons")]
        [Tooltip("Default icon shown for small groups")]
        public Sprite defaultIcon;

        [Tooltip("Icon shown when group size > A")]
        public Sprite iconA;

        [Tooltip("Icon shown when group size > B")]
        public Sprite iconB;

        [Tooltip("Icon shown when group size > C")]
        public Sprite iconC;

        /// <summary>
        /// Gets the appropriate sprite based on the icon state.
        /// </summary>
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
}
