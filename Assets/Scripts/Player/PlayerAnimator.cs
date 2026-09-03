using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    static readonly int SpeedHash = Animator.StringToHash("Speed");

    [SerializeField] PlatformerController player;
    [SerializeField] Animator animator;
    [SerializeField] Color tint = Color.white;

    void Awake()
    {
        if (player == null)
            player = GetComponent<PlatformerController>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if (animator != null)
            animator.applyRootMotion = false;

        MeshRenderer capsule = GetComponent<MeshRenderer>();
        if (capsule != null)
            capsule.enabled = false;

        ApplyTint();
    }

    // 0 = idle, moving = walk
    void Update()
    {
        if (player != null && animator != null)
            animator.SetFloat(SpeedHash, player.PlanarSpeed);
    }

    // p1 cyan / p2 orange
    void ApplyTint()
    {
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            if (renderer.gameObject == gameObject)
                continue;

            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null && materials[i].HasProperty("_BaseColor"))
                    materials[i].SetColor("_BaseColor", Color.Lerp(materials[i].GetColor("_BaseColor"), tint, 0.75f));
            }
            renderer.materials = materials;
        }
    }
}
