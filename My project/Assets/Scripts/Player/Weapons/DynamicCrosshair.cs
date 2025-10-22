using UnityEngine;
using UnityEngine.UI;

public class DynamicCrosshair : MonoBehaviour
{
    public RectTransform top;
    public RectTransform bottom;
    public RectTransform left;
    public RectTransform right;
    public GameObject crosshairObj;

    [Header("Crosshair Settings")]
    public float size = 10f;
    public float gap = 5f;
    public float dynamicGap;
    public float thickness = 2f;
    public Color color = Color.green;
    public int maxSpread;
    public float recoverySpeed;
    public static DynamicCrosshair instance;

    private Vector2 originalTopPos, originalBottomPos, originalLeftPos, originalRightPos;


    void Start()
    {
        instance = this;
        dynamicGap = gap;
        foreach (var arm in new[] { top, bottom, left, right })
        {
            var img = arm.GetComponent<Image>();
            if (img) img.color = color;
        }

        UpdateCrosshairLayout();
    }

    void Update()
    {
        if (dynamicGap > gap)
        {
            dynamicGap=Mathf.MoveTowards(dynamicGap, gap, recoverySpeed * Time.deltaTime);
            SetSpread();
        }
    }

    void UpdateCrosshairLayout()
    {
        top.sizeDelta = new Vector2(thickness, size);
        bottom.sizeDelta = new Vector2(thickness, size);
        left.sizeDelta = new Vector2(size, thickness);
        right.sizeDelta = new Vector2(size, thickness);

        top.anchoredPosition = new Vector2(0, gap + size / 2);
        bottom.anchoredPosition = new Vector2(0, -(gap + size / 2));
        left.anchoredPosition = new Vector2(-(gap + size / 2), 0);
        right.anchoredPosition = new Vector2(gap + size / 2, 0);
    }

    public void SetSpread(float spread = 0f)
    {
        dynamicGap += spread;
        dynamicGap = Mathf.Clamp(dynamicGap, gap, maxSpread);

        top.anchoredPosition = new Vector2(0, dynamicGap + size / 2);
        bottom.anchoredPosition = new Vector2(0, -(dynamicGap + size / 2));
        left.anchoredPosition = new Vector2(-(dynamicGap + size / 2), 0);
        right.anchoredPosition = new Vector2(dynamicGap + size / 2, 0);
    }

    public void Disable()
    {
        crosshairObj.SetActive(false);
    }
    
    public void Enable()
    {
        crosshairObj.SetActive(true);
    }
}
