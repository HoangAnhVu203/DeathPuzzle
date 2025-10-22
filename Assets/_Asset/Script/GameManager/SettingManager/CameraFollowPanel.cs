using UnityEngine;

public class CameraFollowPanel : MonoBehaviour
{
   
    public RectTransform panelWin;        // Panel Win (RectTransform)
    public Canvas canvas;                 // Canvas chứa panel Win (Screen Space - Camera)

    public float moveSpeed = 3f;
    public Vector3 offset = new Vector3(0, 0, -10f);

    private bool followPanel = false;
    private Vector3 originalPos;

    void Start()
    {
        originalPos = transform.position;
    }

    void Update()
    {
        if (followPanel && panelWin)
        {
            // Lấy vị trí world của panel Win (Canvas phải ở chế độ Screen Space - Camera)
            Vector3 worldPos = canvas.worldCamera != null
                ? canvas.worldCamera.ScreenToWorldPoint(panelWin.position)
                : panelWin.position;

            worldPos.z = offset.z; // Giữ Z cố định cho camera
            transform.position = Vector3.Lerp(transform.position, worldPos + offset, Time.deltaTime * moveSpeed);
        }
    }

    // Gọi khi bật panel Win
    public void FocusOnPanel()
    {
        followPanel = true;
    }

    // Gọi khi tắt panel Win
    public void ResetCamera()
    {
        followPanel = false;
        StartCoroutine(BackToOriginal());
    }

    System.Collections.IEnumerator BackToOriginal()
    {
        float t = 0;
        Vector3 start = transform.position;
        while (t < 1)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(start, originalPos, t);
            yield return null;
        }
    }
}
