using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public float sensitivity = 3f;
    public float heightOffset = 0.2f;

    [Header("Zoom Settings")]
    public float distance = 3f;
    public float minDistance = 1f;
    public float maxDistance = 6f;
    public float zoomSpeed = 2f;

    private float currentX = 0f;
    private float currentY = 15f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        currentX += Input.GetAxis("Mouse X") * sensitivity;
        currentY -= Input.GetAxis("Mouse Y") * sensitivity;
        currentY = Mathf.Clamp(currentY, -20f, 60f);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        Vector3 direction = new Vector3(0, 0, -distance);
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

        Vector3 targetPos = target.position + (Vector3.up * heightOffset);
        transform.position = targetPos + rotation * direction;
        transform.LookAt(targetPos);
    }
}