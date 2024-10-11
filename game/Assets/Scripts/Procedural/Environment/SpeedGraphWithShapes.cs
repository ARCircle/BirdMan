using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Shapes2D;

public class SpeedGraphWithShapes : MonoBehaviour
{
    [Header("Player Settings")]
    public Transform playerTransform; // プレイヤーのTransform
    public Camera mainCamera; // カメラ

    [Header("Sampling Settings")]
    public float sampleInterval = 0.1f; // サンプリング間隔
    public int maxDataPoints = 100; // グラフに表示する最大データポイント数

    [Header("UI Settings")]
    public Text distanceText;
    public Text speedText;

    [Header("Graph Settings")]
    public GameObject shapePrefab; // ShapeのPrefab
    public Transform graphContainer; // グラフを表示するための親オブジェクト
    public float graphWidth = 300f; // グラフの横幅
    public float graphHeight = 150f; // グラフの高さ

    [Header("Graph Mode")]
    public bool isDifferenceGraph = false; // Trueで速度変化グラフ、Falseで速度グラフ

    [Header("Opacity Settings")]
    public float fillOpacity = 0.5f; // 透明度を制御するための変数 (0.0 - 1.0)

    [Header("Camera Settings")]
    public float minFieldOfView = 60f; // 最小Field of View
    public float maxFieldOfView = 100f; // 最大Field of View
    public float fovChangeSpeedUp = 2f; // FOVの変化速度（時間あたりの増減量）
    public float fovChangeSpeedDown = 2f; // FOVの変化速度（時間あたりの増減量）

    private List<float> speedData = new List<float>(); // 速度データ
    private List<float> speedDifferenceData = new List<float>(); // 速度差分データ
    private List<GameObject> shapeInstances = new List<GameObject>(); // Shapeのインスタンス
    private float elapsedTime = 0f;
    private float previousZPosition = 0f;
    private float cumulativeDistance = 0f; // 累計距離
    private float previousSpeed = 0f; // 前回の速度を保持
    private float currentFieldOfView; // 現在のFOV

    void Start()
    {
        if (playerTransform == null || shapePrefab == null || mainCamera == null)
        {
            Debug.LogError("Player Transform, Shape Prefab, または Camera が設定されていません。");
            enabled = false;
            return;
        }

        previousZPosition = playerTransform.position.z;
        currentFieldOfView = mainCamera.fieldOfView;

        // ShapeのインスタンスをmaxDataPoints分作成
        for (int i = 0; i < maxDataPoints; i++)
        {
            GameObject newShape = Instantiate(shapePrefab, graphContainer);
            newShape.transform.localPosition = Vector3.zero;
            shapeInstances.Add(newShape);
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

            // 速度変化を計算し、差分データとして保持
            if (speedData.Count > 1)
            {
                float speedDifference = speed - previousSpeed;
                speedDifferenceData.Add(speedDifference);
            }
            else
            {
                speedDifferenceData.Add(0); // 初回は変化がないため0
            }

            // データポイント数を制限
            if (speedData.Count > maxDataPoints)
            {
                speedData.RemoveAt(0);
                speedDifferenceData.RemoveAt(0); // 速度変化データも同様に削除
            }

            previousZPosition = currentZPosition;
            previousSpeed = speed;

            // カメラのField of Viewを更新
            UpdateCameraFieldOfView();

            // グラフの更新
           // UpdateGraph();
        }

        // テキストの更新
        UpdateTexts();
    }

    // テキストの更新
    void UpdateTexts()
    {
        if (distanceText != null)
        {
            distanceText.text = $"{cumulativeDistance:F0} m";
        }

        if (speedText != null && speedData.Count > 0)
        {
            float currentSpeed = speedData[speedData.Count - 1];
            speedText.text = $"{currentSpeed:F0} m/s";
        }
    }

    // カメラのField of Viewを加速度に応じて滑らかに更新
    void UpdateCameraFieldOfView()
    {
        if (speedDifferenceData.Count > 0)
        {
            float lastSpeedDifference = speedDifferenceData[speedDifferenceData.Count - 1];
            print(lastSpeedDifference);
            if (lastSpeedDifference >= 0)
            {

                // 加速している場合、FOVを増加
                //currentFieldOfView += fovChangeSpeedUp * Time.deltaTime;
                currentFieldOfView += fovChangeSpeedUp;
            }
            else if (lastSpeedDifference < 0)
            {
                // 減速している場合、FOVを減少
                //  currentFieldOfView -= fovChangeSpeedDown * Time.deltaTime;
                currentFieldOfView -= fovChangeSpeedDown;
            }

            // FOVの範囲を制限
            currentFieldOfView = Mathf.Clamp(currentFieldOfView, minFieldOfView, maxFieldOfView);

            // カメラのFOVに適用
            mainCamera.fieldOfView = currentFieldOfView;
        }
    }

    public float maxDataValue;
    public float minDataValue;

    void UpdateGraph()
    {
        if (speedData.Count == 0)
            return;

        // グラフの表示モードによって使うデータを決定
        List<float> dataToUse = isDifferenceGraph ? speedDifferenceData : speedData;

        // データの最大値と最小値を取得
        if (Mathf.Approximately(maxDataValue, minDataValue))
        {
            maxDataValue += 0.1f; // ほんの少しの差を設けて0除算を防ぐ
        }

        // Shapeインスタンスの位置とサイズ、色を更新
        for (int i = 0; i < dataToUse.Count; i++)
        {
            float value = dataToUse[i];

            // 棒の高さをデータに比例して設定。負の場合には下方向へ伸びるように
            float normalizedValue = Mathf.Abs((value - minDataValue) / (maxDataValue - minDataValue));

            // 指数的にheightをスケーリング
            float exponent = 2.0f; // 指数の値。大きいほど変化が強調される
            float height = Mathf.Pow(normalizedValue, exponent) * graphHeight;

            if (float.IsNaN(height))
            {
                height = 0; // 万が一 NaN になった場合のフォールバック
            }

            float posX = i * (graphWidth / maxDataPoints);
            float posY = value >= 0 ? height / 2 : -height / 2; // 高さが正の場合は上方向、負の場合は下方向

            GameObject shapeInstance = shapeInstances[i];
            RectTransform shapeRect = shapeInstance.GetComponent<RectTransform>();

            // 高さと位置を更新
            shapeRect.sizeDelta = new Vector2(shapeRect.sizeDelta.x, height);
            shapeRect.localPosition = new Vector3(posX, posY, 0);

            // データに基づいて色を設定（低い値：青、高い値：赤）
            Color fillColor = GetColorForValue(value, minDataValue, maxDataValue);
            Shape shapeComponent = shapeInstance.GetComponent<Shape>();
            shapeComponent.settings.fillColor = fillColor;

            shapeComponent.settings.dirty = true; // 色の変更を反映するためにdirtyフラグを立てる
        }
    }

    // 値に応じた色を取得する関数
    Color GetColorForValue(float value, float minValue, float maxValue)
    {
        // 0を境界にするためのtを計算
        float t = Mathf.InverseLerp(minValue, maxValue, Mathf.Abs(value));

        // スケーリングを追加して微小変化を強調
        float scalingFactor = 2.0f; // 値を調整して色変化の強度を増幅
        t = Mathf.Clamp01(t * scalingFactor); // スケーリング後のtをクランプ

        Color baseColor;

        if (value >= 0)
        {
            // 0から上に行く場合は紫から赤
            baseColor = Color.Lerp(Color.magenta, Color.red, t);
        }
        else
        {
            // 0から下に行く場合は紫から青
            baseColor = Color.Lerp(Color.magenta, Color.blue, t);
        }

        // アルファ値をfillOpacityで適用
        baseColor.a = fillOpacity;

        return baseColor;
    }

}
