using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SpeedGraphController : MonoBehaviour
{
    [Header("Player Settings")]
    public Transform playerTransform; // プレイヤーのTransform

    [Header("Sampling Settings")]
    public float sampleInterval = 0.1f; // サンプリング間隔
    public int maxDataPoints = 100; // グラフに表示する最大データポイント数

    [Header("UI Settings")]
    public RawImage graphImage;
    public Text distanceText;
    public Text speedText;

    [Header("Graph Settings")]
    public int graphWidth = 300;
    public int graphHeight = 150;
    public Color graphLineColor = Color.green;
    public Color graphFillColor = new Color(0f, 1f, 0f, 0.3f); // 半透明の緑

    private List<float> speedData = new List<float>(); // 速度データ
    private float cumulativeDistance = 0f; // 累計距離

    private float elapsedTime = 0f;
    private float previousZPosition = 0f;

    private Texture2D graphTexture;

    void Start()
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player Transformが設定されていません。");
            enabled = false;
            return;
        }

        previousZPosition = playerTransform.position.z;

        // グラフ用のテクスチャを作成
        graphTexture = new Texture2D(graphWidth, graphHeight);
        graphTexture.wrapMode = TextureWrapMode.Clamp;
        graphTexture.filterMode = FilterMode.Bilinear;

        if (graphImage != null)
        {
            graphImage.texture = graphTexture;
        }
        else
        {
            Debug.LogError("Graph Imageが設定されていません。");
        }
    }

    void Update()
    {
        // データのサンプリング
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= sampleInterval)
        {
            elapsedTime -= sampleInterval;

            // Z軸方向の移動量と速度を計算
            float currentZPosition = playerTransform.position.z;
            float deltaZ = currentZPosition - previousZPosition;
            float speed = deltaZ / sampleInterval;

            // 累計距離を更新
            cumulativeDistance += Mathf.Abs(deltaZ);

            // データをリストに追加
            speedData.Add(speed);

            // データポイント数を制限
            if (speedData.Count > maxDataPoints)
            {
                speedData.RemoveAt(0);
            }

            previousZPosition = currentZPosition;
        }

        // グラフとテキストの更新
        UpdateGraph();
        UpdateTexts();
    }

    void UpdateTexts()
    {
        if (distanceText != null)
        {
            distanceText.text = $"{cumulativeDistance:F0} m";
        }

        if (speedText != null && speedData.Count > 0)
        {
            float currentSpeed = speedData[speedData.Count - 1];
            speedText.text = $" {currentSpeed:F0} m/s";
        }
    }
    void UpdateGraph()
    {
        if (graphTexture == null || speedData.Count < 2)
            return;

        // 速度データから最大速度と最小速度を取得
        float maxSpeed, minSpeed;
        GetSpeedRange(out maxSpeed, out minSpeed);

        // 上下に10%の余裕を持たせる
        float padding = (maxSpeed - minSpeed) * 0.1f;
        maxSpeed += padding;
        minSpeed -= padding;

        float speedRange = Mathf.Max(maxSpeed - minSpeed, 0.1f); // 0.1で割り算を回避

        // クリア処理を変更：グラフを一度にクリア
        ClearTextureFast();

        Color[] pixels = graphTexture.GetPixels();

        // グラフの描画
        for (int i = 0; i < speedData.Count - 1; i++)
        {
            float speed = speedData[i];
            float nextSpeed = speedData[i + 1];

            // 座標計算（最小速度を基準に、縦方向のスケールを調整）
            float x0 = (float)i / (speedData.Count - 1) * graphWidth;
            float y0 = ((speed - minSpeed) / speedRange) * graphHeight;

            float x1 = (float)(i + 1) / (speedData.Count - 1) * graphWidth;
            float y1 = ((nextSpeed - minSpeed) / speedRange) * graphHeight;

            // 線を描画
            DrawLineFast(pixels, (int)x0, (int)y0, (int)x1, (int)y1, graphLineColor);

            // 塗りつぶし
            FillAreaUnderLineFast(pixels, (int)x0, (int)y0, (int)x1, (int)y1, graphFillColor);
        }

        // 最後に一度だけテクスチャを更新
        graphTexture.SetPixels(pixels);
        graphTexture.Apply();
    }

    // 最大速度と最小速度を取得する関数
    void GetSpeedRange(out float maxSpeed, out float minSpeed)
    {
        maxSpeed = float.MinValue;
        // minSpeed = float.MaxValue;
        minSpeed = 0;

        foreach (float speed in speedData)
        {
            if (speed > maxSpeed)
            {
                maxSpeed = speed;
            }
            if (speed < minSpeed)
            {
                //minSpeed = speed;
                minSpeed = 0;
            }
        }
    }



    // ピクセルをまとめてクリア
    void ClearTextureFast()
    {
        Color clearColor = new Color(0, 0, 0, 0); // 透明
        Color[] clearPixels = new Color[graphWidth * graphHeight];
        for (int i = 0; i < clearPixels.Length; i++)
        {
            clearPixels[i] = clearColor;
        }
        graphTexture.SetPixels(clearPixels);
    }

    // ピクセルをまとめて描画
    void DrawLineFast(Color[] pixels, int x0, int y0, int x1, int y1, Color color)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (IsInsideTexture(x0, y0))
            {
                pixels[y0 * graphWidth + x0] = color;
            }

            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    // 塗りつぶし処理を最適化
    void FillAreaUnderLineFast(Color[] pixels, int x0, int y0, int x1, int y1, Color color)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            // X軸まで塗りつぶす
            for (int fillY = 0; fillY <= y0; fillY++)
            {
                if (IsInsideTexture(x0, fillY))
                {
                    pixels[fillY * graphWidth + x0] = color;
                }
            }

            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    float GetMaxSpeed()
    {
        float maxSpeed = 0f;
        foreach (float speed in speedData)
        {
            if (Mathf.Abs(speed) > maxSpeed)
            {
                maxSpeed = Mathf.Abs(speed);
            }
        }
        return maxSpeed;
    }

    bool IsInsideTexture(int x, int y)
    {
        return x >= 0 && x < graphWidth && y >= 0 && y < graphHeight;
    }
}
