using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Timer : MonoBehaviour
{
    public Text timerText;
    public Text distanceText;
    public Text speedText;
    private float timer = 0f;
    public float clearTime = 10f; // 1分30秒 = 90秒
    public BirdControl BirdControl;
    public GameObject LongHand;
    public GameObject ShortHand;
    public GameObject TimerBack;
    public GameObject ClearUI;

    private float previousZPosition = 0f; // 前回のZ位置
    private float cumulativeDistance = 0f; // 累計距離
    private float previousSpeed = 0f; // 前回の速度
    private float elapsedTime = 0f; // 経過時間のトラッキング
    private bool isClear = false; // ゲームクリアかどうかを判定するフラグ

    void Start()
    {
        previousZPosition = BirdControl.transform.position.z;
    }
    float remainingTime;

    void Update()
    {
        if (!isClear)
        {
            timer += Time.deltaTime;
            remainingTime = clearTime - timer; // 残り時間の計算

            // 残り時間が0未満にならないように制限
            remainingTime = Mathf.Max(remainingTime, 0f);

            // 分と秒に変換
            float minutes = Mathf.FloorToInt(remainingTime / 60f);
            float seconds = Mathf.FloorToInt(remainingTime % 60f);

            TimerBack.GetComponent<Image>().fillAmount = remainingTime / clearTime;

            // タイマーをテキストに表示（残り時間）
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            // 距離と速度の計算と表示
            UpdateDistanceAndSpeed();
        }
        // 時間が0になったらクリアUIを表示
        if (remainingTime <= 0f && !isClear)
        {
            ClearUI.SetActive(true);
            isClear = true; // クリア状態に変更
        }

       

        // 長針と短針の回転を更新
       // Hand();

        // 左クリックが押されたらタイトルシーンに移動
        if (isClear && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ToTitle();
        }
    }

    // 距離と速度の更新
    void UpdateDistanceAndSpeed()
    {
        elapsedTime += Time.deltaTime;

        // 一定間隔ごとに距離と速度を更新
        if (elapsedTime >= 0.1f)
        {
            elapsedTime = 0f; // リセット

            float currentZPosition = BirdControl.transform.position.z;
            float deltaZ = currentZPosition - previousZPosition; // Z軸方向の移動量
            float speed = deltaZ / 0.1f; // 速度 = 移動量 / 時間

            // 累計距離を更新
            cumulativeDistance += Mathf.Abs(deltaZ);

            // 距離と速度をテキストに表示
            distanceText.text = $"{cumulativeDistance:F0} m";
            speedText.text = $"{speed:F0} m/s";

            previousZPosition = currentZPosition;
            previousSpeed = speed;
        }
    }

    private void Hand()
    {
        LongHand.transform.rotation = Quaternion.Euler(LongHand.transform.eulerAngles.x, LongHand.transform.eulerAngles.y, BirdControl.newRotationZR);
        ShortHand.transform.rotation = Quaternion.Euler(ShortHand.transform.eulerAngles.x, ShortHand.transform.eulerAngles.y, -BirdControl.newRotationZL + 180);
    }

    private void ToTitle()
    {
        SceneManager.LoadScene("Title");
    }
}
