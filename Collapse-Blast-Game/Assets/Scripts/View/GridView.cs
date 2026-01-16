using UnityEngine;

public class GridView : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private Transform gridParent;

    [Header("Settings")]
    [SerializeField]
    private float padding = 1f;

    public void FitGridToCamera(int rowCount, int columnCount, float blockSize, float spacing)
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        float totalWidth = columnCount * (blockSize + spacing);
        float totalHeight = rowCount * (blockSize + spacing);

        float cameraAspect = mainCamera.aspect;
        float requiredHeight = totalHeight / 2f + padding;
        float requiredWidth = (totalWidth / 2f + padding) / cameraAspect;

        mainCamera.orthographicSize = Mathf.Max(requiredHeight, requiredWidth);
    }

    public void CenterGrid()
    {
        if (gridParent != null)
        {
            gridParent.position = Vector3.zero;
        }
    }
}
