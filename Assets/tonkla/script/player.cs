using UnityEngine;

public class FloatingCameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float verticalSpeed = 7f;
    public float zoomSpeed = 10f;
  

    public float minHeight = 5f;
    public float maxHeight = 100f;

    private bool isDraging = false;
    private Vector3 lastDragVelocity;
    public float dragSpeed = 5f;
    public float inertiaDamp = 3f; //sticky value

    private Vector3 dragOrigin;

    void Update()
    {
        HandleMovement();
        HandleZoom();
        HandleMouseDrag();
        AfterDragForce();
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float moveZ = Input.GetAxis("Vertical");   // W/S or Up/Down

        Vector3 direction = new Vector3(moveX, 0, moveZ);
        transform.Translate(direction * moveSpeed * Time.deltaTime, Space.Self);

        // Vertical movement (float up/down)
        if (Input.GetKey(KeyCode.Q)) transform.Translate(Vector3.down * verticalSpeed * Time.deltaTime, Space.World);
        if (Input.GetKey(KeyCode.E)) transform.Translate(Vector3.up * verticalSpeed * Time.deltaTime, Space.World);
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 zoomDirection = transform.forward * scroll * zoomSpeed;
        Vector3 targetPosition = transform.position + zoomDirection;

        // Clamp camera height
        if (targetPosition.y >= minHeight && targetPosition.y <= maxHeight)
        {
            transform.position = targetPosition;
        }
    }

    void HandleMouseDrag()
    {
        // Optional: Right-click drag to move
        if (Input.GetMouseButtonDown(1))
        {
            dragOrigin = Input.mousePosition;
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 difference = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
            Vector3 move = new Vector3(difference.x * -dragSpeed, 0, difference.y * -dragSpeed);

            lastDragVelocity = move / Time.deltaTime;

            transform.Translate(move, Space.Self);
            dragOrigin = Input.mousePosition;
        }
    }

    void AfterDragForce()
    {
        if (!isDraging && lastDragVelocity.magnitude > 0.01f)
        {
            transform.Translate(lastDragVelocity * Time.deltaTime, Space.Self);

            lastDragVelocity = Vector3.Lerp(lastDragVelocity, Vector3.zero, Time.deltaTime * inertiaDamp);
        }
    }
}
