using UnityEngine;

// Faces the camera that is currently rendering (works with split screen).
public class CameraBillboard : MonoBehaviour
{
    void OnWillRenderObject()
    {
        Camera cam = Camera.current;
        if (cam == null)
            return;
        transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position, Vector3.up);
    }
}
