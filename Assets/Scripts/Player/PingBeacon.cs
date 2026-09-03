using UnityEngine;

public class PingBeacon : MonoBehaviour
{
    private Color color;
    private float age;
    private float life = 1.8f;
    private Transform beam;
    private Transform ring;
    private Light pulseLight;

    public static void Spawn(Vector3 position, Color color)
    {
        GameObject root = new GameObject("PingBeacon");
        root.transform.position = position;
        PingBeacon ping = root.AddComponent<PingBeacon>();
        ping.color = color;
        ping.Build();
    }

    void Build()
    {
        beam = CreatePrimitive(PrimitiveType.Cylinder, new Vector3(0f, 6f, 0f), new Vector3(0.18f, 6f, 0.18f));
        ring = CreatePrimitive(PrimitiveType.Cylinder, new Vector3(0f, 0.08f, 0f), new Vector3(1.2f, 0.04f, 1.2f));

        GameObject lightObj = new GameObject("PingLight");
        lightObj.transform.SetParent(transform, false);
        lightObj.transform.localPosition = Vector3.up * 2.5f;
        pulseLight = lightObj.AddComponent<Light>();
        pulseLight.type = LightType.Point;
        pulseLight.range = 12f;
        pulseLight.intensity = 4f;
        pulseLight.color = color;
    }

    Transform CreatePrimitive(PrimitiveType type, Vector3 localPos, Vector3 localScale)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        DestroyImmediate(go.GetComponent<Collider>());
        go.transform.SetParent(transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;
        Renderer renderer = go.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.SetColor("_BaseColor", color);
        mat.color = color;
        renderer.material = mat;
        return go.transform;
    }

    void Update()
    {
        age += Time.deltaTime;
        float t = Mathf.Clamp01(age / life);
        float pulse = 1f + Mathf.Sin(age * 14f) * 0.12f;
        if (beam != null)
            beam.localScale = new Vector3(0.18f * pulse, 6f, 0.18f * pulse);
        if (ring != null)
            ring.localScale = Vector3.Lerp(new Vector3(0.4f, 0.04f, 0.4f), new Vector3(4.5f, 0.04f, 4.5f), t);
        if (pulseLight != null)
            pulseLight.intensity = Mathf.Lerp(5f, 0f, t);

        if (age >= life)
            Destroy(gameObject);
    }
}
