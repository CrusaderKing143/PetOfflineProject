using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class SweepingRobotSortingOrderController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Vector2 areaMin = new Vector2(-2.06f, -1.24f);
    [SerializeField] private Vector2 areaMax = new Vector2(1f, 0.2f);
    [SerializeField] private int insideSortingOrder = -2;
    [SerializeField] private int normalSortingOrder = 2;

    private void Awake()
    {
        ResolveRenderer();
        UpdateSortingOrder();
    }

    private void LateUpdate()
    {
        UpdateSortingOrder();
    }

    private void UpdateSortingOrder()
    {
        Vector2 position = transform.position;
        bool isInside = position.x >= areaMin.x && position.x <= areaMax.x
            && position.y >= areaMin.y && position.y <= areaMax.y;
        int targetOrder = isInside ? insideSortingOrder : normalSortingOrder;

        if (spriteRenderer.sortingOrder != targetOrder)
        {
            spriteRenderer.sortingOrder = targetOrder;
        }
    }

    private void ResolveRenderer()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void Reset()
    {
        ResolveRenderer();
    }
}
