using System.Collections.Generic;
using UnityEngine;

public class PaintCache : MonoBehaviour
{
    public static PaintCache Instance { get; private set; }

    // Danh sách pixel cần lau
    public readonly List<Vector2Int> dishes = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Lọc pixel có alpha > threshold
    public void CachePaintWithAlpha(Texture2D tex, float alphaThreshold, List<Vector2Int> outList)
    {
        outList.Clear();
        if (tex == null) return;

        var colors = tex.GetPixels32();
        int w = tex.width, h = tex.height;

        for (int y = 0; y < h; y++)
        {
            int row = y * w;
            for (int x = 0; x < w; x++)
            {
                Color32 c = colors[row + x];
                float a = c.a / 255f;
                if (a > alphaThreshold)
                    outList.Add(new Vector2Int(x, y));
            }
        }
    }
}
