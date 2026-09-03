using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Shared HUD: connection bar, per-player control hints, split buddy markers.
public class GameHud : MonoBehaviour
{
    [SerializeField] PlayerConnection connection;
    [SerializeField] PlatformerController player1;
    [SerializeField] PlatformerController player2;

    [Header("Connection bar")]
    [SerializeField] Image barImage;
    // [0]=Bar_4 empty, [1]=Bar_3, [2]=Bar_2 stripes, [3]=Bar_1 solid/full
    [SerializeField] Sprite[] connectionStages = new Sprite[4];

    [Header("Control hints (hide after that player uses move+jump)")]
    [SerializeField] GameObject p1Controls;
    [SerializeField] GameObject p2Controls;

    [Header("Split markers")]
    [SerializeField] Sprite p1Icon;
    [SerializeField] Sprite p2Icon;
    [SerializeField] float markerHeight = 2.35f;
    [SerializeField] float markerScale = 0.45f;

    bool p1Up, p1Down, p1Left, p1Right, p1Jump;
    bool p2Up, p2Down, p2Left, p2Right, p2Jump;
    int lastStage = -1;
    SpriteRenderer marker1;
    SpriteRenderer marker2;
    Text winBanner;

    void Start()
    {
        marker1 = CreateMarker("P1Marker", p1Icon);
        marker2 = CreateMarker("P2Marker", p2Icon);
        UpdateConnectionBar(force: true);
    }

    void Update()
    {
        UpdateConnectionBar(force: false);
        UpdateControlHints();
        PulseBarOnLinkEvents();
    }

    void LateUpdate() => UpdateMarkers();

    GameObject winPanel;

    public void ShowWin(string message = "YOU WIN!")
    {
        EnsureWinBanner();
        winBanner.text = message;
        winPanel.SetActive(true);
        winPanel.transform.SetAsLastSibling();
    }

    void EnsureWinBanner()
    {
        if (winBanner != null)
            return;

        winPanel = new GameObject("WinPanel");
        winPanel.transform.SetParent(transform, false);
        var panelRt = winPanel.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;
        var panelBg = winPanel.AddComponent<Image>();
        panelBg.color = new Color(0f, 0f, 0f, 0.7f);
        panelBg.raycastTarget = false;

        var go = new GameObject("WinBanner");
        go.transform.SetParent(winPanel.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(900f, 140f);
        winBanner = go.AddComponent<Text>();
        winBanner.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        winBanner.fontSize = 72;
        winBanner.fontStyle = FontStyle.Bold;
        winBanner.alignment = TextAnchor.MiddleCenter;
        winBanner.color = Color.white;
        winBanner.raycastTarget = false;
        var outline = go.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(3f, -3f);
        winPanel.SetActive(false);
    }

    void PulseBarOnLinkEvents()
    {
        if (connection == null || barImage == null)
            return;
        if (connection.JustSnapped)
            barImage.color = new Color(1f, 0.35f, 0.3f, 1f);
        else if (connection.JustReconnected)
            barImage.color = new Color(0.4f, 1f, 0.55f, 1f);
        else
            barImage.color = Color.Lerp(barImage.color, Color.white, 8f * Time.deltaTime);
    }

    SpriteRenderer CreateMarker(string name, Sprite sprite)
    {
        var go = new GameObject(name);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = PunchBlackTransparent(sprite);
        sr.sortingOrder = 50;
        go.AddComponent<CameraBillboard>();
        go.transform.localScale = Vector3.one * markerScale;
        sr.enabled = false;
        return sr;
    }

    static Sprite PunchBlackTransparent(Sprite source)
    {
        if (source == null)
            return null;
        try
        {
            Texture2D src = source.texture;
            Rect r = source.textureRect;
            int x = Mathf.FloorToInt(r.x);
            int y = Mathf.FloorToInt(r.y);
            int w = Mathf.FloorToInt(r.width);
            int h = Mathf.FloorToInt(r.height);
            Color[] pixels = src.GetPixels(x, y, w, h);
            for (int i = 0; i < pixels.Length; i++)
                if (pixels[i].r < 0.08f && pixels[i].g < 0.08f && pixels[i].b < 0.08f)
                    pixels[i].a = 0f;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.SetPixels(pixels);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        }
        catch
        {
            return source;
        }
    }

    void UpdateMarkers()
    {
        bool split = connection != null && !connection.IsLinked;
        PlaceMarker(marker1, player1, split);
        PlaceMarker(marker2, player2, split);
    }

    void PlaceMarker(SpriteRenderer marker, PlatformerController player, bool show)
    {
        if (marker == null || player == null)
            return;
        marker.enabled = show && marker.sprite != null;
        if (marker.enabled)
            marker.transform.position = player.transform.position + Vector3.up * markerHeight;
    }

    void UpdateConnectionBar(bool force)
    {
        if (connection == null || barImage == null || connectionStages == null || connectionStages.Length < 4)
            return;

        // Linked → always Bar_1 (full). Split → Bar_4/3/2 by how close you are to reconnect.
        int stage;
        if (connection.IsLinked)
        {
            stage = 3; // Bar_1 solid
        }
        else
        {
            float p = Mathf.Clamp01(connection.ReconnectProgress);
            // far → empty(0), mid → Bar_3(1), close → Bar_2(2)
            stage = Mathf.Clamp(Mathf.FloorToInt(p * 3f), 0, 2);
            if (p >= 0.95f)
                stage = 2;
        }

        if (!force && stage == lastStage)
            return;
        if (connectionStages[stage] == null)
            return;

        lastStage = stage;
        barImage.sprite = connectionStages[stage];
        if (!connection.JustSnapped && !connection.JustReconnected)
            barImage.color = Color.white;
        barImage.enabled = true;
        barImage.preserveAspect = true;
        barImage.type = Image.Type.Simple;
    }

    void UpdateControlHints()
    {
        if (p1Controls != null && p1Controls.activeSelf)
        {
            TrackPlayer1();
            if (p1Up && p1Down && p1Left && p1Right && p1Jump)
                p1Controls.SetActive(false);
        }
        if (p2Controls != null && p2Controls.activeSelf)
        {
            TrackPlayer2();
            if (p2Up && p2Down && p2Left && p2Right && p2Jump)
                p2Controls.SetActive(false);
        }
    }

    void TrackPlayer1()
    {
        if (player1 != null)
            TrackMove(player1.MoveInput, player1.JumpPressedThisFrame || player1.JumpHeld,
                ref p1Up, ref p1Down, ref p1Left, ref p1Right, ref p1Jump);
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.wKey.isPressed || kb.wKey.wasPressedThisFrame) p1Up = true;
        if (kb.sKey.isPressed || kb.sKey.wasPressedThisFrame) p1Down = true;
        if (kb.aKey.isPressed || kb.aKey.wasPressedThisFrame) p1Left = true;
        if (kb.dKey.isPressed || kb.dKey.wasPressedThisFrame) p1Right = true;
        if (kb.spaceKey.isPressed || kb.spaceKey.wasPressedThisFrame) p1Jump = true;
    }

    void TrackPlayer2()
    {
        if (player2 != null)
            TrackMove(player2.MoveInput, player2.JumpPressedThisFrame || player2.JumpHeld,
                ref p2Up, ref p2Down, ref p2Left, ref p2Right, ref p2Jump);
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.upArrowKey.isPressed || kb.upArrowKey.wasPressedThisFrame) p2Up = true;
        if (kb.downArrowKey.isPressed || kb.downArrowKey.wasPressedThisFrame) p2Down = true;
        if (kb.leftArrowKey.isPressed || kb.leftArrowKey.wasPressedThisFrame) p2Left = true;
        if (kb.rightArrowKey.isPressed || kb.rightArrowKey.wasPressedThisFrame) p2Right = true;
        if (kb.enterKey.isPressed || kb.enterKey.wasPressedThisFrame
            || kb.numpadEnterKey.isPressed || kb.numpadEnterKey.wasPressedThisFrame)
            p2Jump = true;
    }

    static void TrackMove(Vector2 move, bool jumped, ref bool up, ref bool down, ref bool left, ref bool right, ref bool jump)
    {
        const float dead = 0.35f;
        if (move.y > dead) up = true;
        if (move.y < -dead) down = true;
        if (move.x < -dead) left = true;
        if (move.x > dead) right = true;
        if (jumped) jump = true;
    }
}
