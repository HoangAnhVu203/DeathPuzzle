using UnityEngine;

public class WashTheDishes : MonoBehaviour
{
    public static WashTheDishes Instance { get; private set; }

    private int dishesCleaned = 0;   // số đĩa đã lau xong
    public int totalDishes = 1;      // tổng số đĩa (có thể chỉnh trong Inspector)

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

    /// <summary>
    /// Gọi khi 1 đĩa hoàn thành > 90%
    /// </summary>
    public void CompleteDishes()
    {
        dishesCleaned++;

        Debug.Log($"Đã rửa xong {dishesCleaned}/{totalDishes} đĩa");

        if (dishesCleaned >= totalDishes)
        {
            OnAllDishesCleaned();
        }
    }

    /// <summary>
    /// Hành động khi tất cả đĩa đã rửa xong
    /// </summary>
    private void OnAllDishesCleaned()
    {
        Debug.Log("🎉 Tất cả đĩa đã sạch sẽ!");
        // TODO: mở UI thắng, phát hiệu ứng, load màn kế tiếp...
    }
}
