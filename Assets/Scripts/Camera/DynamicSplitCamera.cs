using UnityEngine;

public class DynamicSplitCamera : MonoBehaviour
{
    [SerializeField] private PlatformerController player1;
    [SerializeField] private PlatformerController player2;
    [SerializeField] private PlayerConnection connection;
    [SerializeField] private Camera camera1;
    [SerializeField] private Camera camera2;
    [SerializeField] private float followDistance = 9f;
    [SerializeField] private float followHeight = 4.5f;
    [SerializeField] private float splitDuration = 0.6f;

    private float splitBlend;

    void Awake()
    {
        if (camera1 != null)
            camera1.depth = 0;
        if (camera2 != null)
            camera2.depth = 1;

        player1.SetMoveCamera(camera1.transform);
        player2.SetMoveCamera(camera1.transform);
    }

    void LateUpdate()
    {
        float targetSplit = connection.IsLinked ? 0f : 1f;
        float step = splitDuration > 0f ? Time.deltaTime / splitDuration : 1f;
        splitBlend = Mathf.MoveTowards(splitBlend, targetSplit, step);

        GetLinkedPose(out Vector3 linkedPos, out Quaternion linkedRot);
        GetPlayerPose(player1.transform, player2.transform, out Vector3 pos1, out Quaternion rot1);
        GetPlayerPose(player2.transform, player1.transform, out Vector3 pos2, out Quaternion rot2);

        camera1.transform.position = Vector3.Lerp(linkedPos, pos1, splitBlend);
        camera1.transform.rotation = Quaternion.Slerp(linkedRot, rot1, splitBlend);

        camera2.transform.position = Vector3.Lerp(linkedPos, pos2, splitBlend);
        camera2.transform.rotation = Quaternion.Slerp(linkedRot, rot2, splitBlend);

        ApplyViewports();

        player1.SetMoveCamera(camera1.transform);
        player2.SetMoveCamera(splitBlend > 0.8f ? camera2.transform : camera1.transform);
    }

    void GetLinkedPose(out Vector3 position, out Quaternion rotation)
    {
        Vector3 mid = (player1.transform.position + player2.transform.position) * 0.5f;
        Vector3 lookTarget = mid + Vector3.up * 1.2f;
        position = mid + Vector3.up * followHeight + Vector3.back * followDistance;
        rotation = Quaternion.LookRotation(lookTarget - position);
    }

    void GetPlayerPose(Transform player, Transform lookToward, out Vector3 position, out Quaternion rotation)
    {
        Vector3 focus = player.position + Vector3.up * 1.2f;
        Vector3 toOther = lookToward.position - player.position;
        toOther.y = 0f;
        if (toOther.sqrMagnitude < 0.1f)
            toOther = player.forward;

        Vector3 back = -toOther.normalized;
        position = player.position + Vector3.up * followHeight + back * followDistance;
        rotation = Quaternion.LookRotation(focus - position);
    }

    void ApplyViewports()
    {
        float leftWidth = Mathf.Lerp(1f, 0.5f, splitBlend);
        float rightWidth = Mathf.Lerp(0f, 0.5f, splitBlend);

        camera1.rect = new Rect(0f, 0f, leftWidth, 1f);
        camera2.rect = new Rect(leftWidth, 0f, rightWidth, 1f);
        camera1.enabled = true;
        camera2.enabled = splitBlend > 0.02f;
    }
}
