using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Timer : MonoBehaviour
{
    public Text timerText;
    public Text distanceText;
    public Text lastDistanceText;
    public Text speedText;
    private float timer = 0f;
    public float clearTime = 10f; // 1分30秒 = 90秒
    public BirdControl BirdControl;
    public GameObject LongHand;
    public GameObject ShortHand;
    public GameObject TimerBack;
   
    public GameObject TitleUI;
    public GameObject PracticeUI;
    public GameObject GameUI;
   
    public GameObject ClearUI;
    public Camera mainCamera;

    private float previousZPosition = 0f; // 前回のZ位置
    private float cumulativeDistance = 0f; // 累計距離
    private float elapsedTime = 0f; // 経過時間のトラッキング
    private bool isClear = false; // ゲームクリアかどうかを判定するフラグ
    bool isGame;
    bool isTitle;
    bool isPractice;
    Rigidbody rb;

    public float minFieldOfView = 60f; // 最小Field of View
    public float maxFieldOfView = 100f; // 最大Field of View
    public float fovChangeSpeedUp = 2f; // FOVの変化速度（時間あたりの増減量）
    public float fovChangeSpeedDown = 2f; // FOVの変化速度（時間あたりの増減量）
    private float currentFieldOfView; // 現在のFOV

    public AutoMouseControl autoMouseControl;

    void Awake()
    {
      

        // 画面の幅を1920、高さを1080、ウィンドウモードで設定する
        Screen.SetResolution(640, 360, true);
        // フレームレートを60に設定
        Application.targetFrameRate = 25;
        // マウスカーソルを非表示にしてロックする
        Cursor.visible = false;

     
    }
    void Start()
    {
        previousZPosition = BirdControl.transform.position.z;
        rb = BirdControl.GetComponent<Rigidbody>();

      

        // カメラの初期FOVを設定
        currentFieldOfView = mainCamera.fieldOfView;
        ToTitle();
    }

    void ToTitle()
    {

        TitleUI.SetActive(true);
        PracticeUI.SetActive(false);
        GameUI.SetActive(false);
        ClearUI.SetActive(false);

        isTitle = true;
        isPractice = false;
        isGame = false;
        isClear = false;


        // 距離と速度をテキストに表示
        lastDistanceText.text = distanceText.text;
        //= $"{currentZPosition:F0} m";
        autoMouseControl.isSinusoidalControlEnabled = true;
        


     }

    void ToPractice()
    {

        TitleUI.SetActive(false);
        PracticeUI.SetActive(true);
        GameUI.SetActive(false);
        ClearUI.SetActive(false);

        isTitle = false;
        isPractice = true;
        isGame = false;
        isClear = false;

        autoMouseControl.isSinusoidalControlEnabled = false;



    }

    void ToGame()
    {


        TitleUI.SetActive(false);
        PracticeUI.SetActive(false);
        GameUI.SetActive(true);
        ClearUI.SetActive(false);

        timer = 0;
        startPosZ = BirdControl.transform.position.z;

        isTitle = false;
        isPractice = false;
        isGame = true;
        isClear = false;

        autoMouseControl.isSinusoidalControlEnabled = false;
        


    }

    void ToClear()
    {

        TitleUI.SetActive(false);
        PracticeUI.SetActive(false);
        GameUI.SetActive(true);
        ClearUI.SetActive(true);

        isTitle = false;
        isPractice = false;
        isGame = false;
        isClear = true;

        autoMouseControl.isSinusoidalControlEnabled = true;

    }
    void KeyInput()
    {
        var keyboard = Keyboard.current;

        // Cキーでアプリケーション終了
        if (keyboard.cKey.wasPressedThisFrame)
        {
            QuitApplication();
        }

        // Rキーでシーンを再読み込み
        if (keyboard.rKey.wasPressedThisFrame)
        {
            ReloadScene();
        }


        if (keyboard.tKey.wasPressedThisFrame)
        {
            ToTitle();
        }



        if (keyboard.pKey.wasPressedThisFrame)
        {
            ToPractice();
        }

       
        if (keyboard.gKey.wasPressedThisFrame)
        {
            ToGame();
        }

        

    }

    

    void Update()
    {


        KeyInput();


      
        float remainingTime=0;
        //if (!isClear&!isTitle)
        if (isGame)
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
            UpdateCameraFieldOfView();

            // 時間が0になったらクリアUIを表示
            if (remainingTime <= 0f && !isClear)
            {
                ToClear();
              //  ClearUI.SetActive(true);
              //  isClear = true; // クリア状態に変更
            }

        }
        else
            UpdateCameraFieldOfView0();


        // 左クリックが押されたらタイトルシーンに移動
        if (isClear && Mouse.current.leftButton.wasPressedThisFrame)
        {
            //ToTitle();
            ToTitle();
        }
    }



    // アプリケーションを終了する関数
    void QuitApplication()
    {
#if UNITY_EDITOR
        // Unityエディタ内で動作中の場合はエディタを終了しない
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // ビルドされたアプリケーションでは終了する
            Application.Quit();
#endif
    }

    // 現在のシーンを再読み込みする関数
    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 距離と速度の更新
    float startPosZ;
    void UpdateDistanceAndSpeed()
    {
        elapsedTime += Time.deltaTime;

        // 一定間隔ごとに距離と速度を更新
        if (elapsedTime >= 0.1f)
        {
            elapsedTime = 0f; // リセット

            float currentZPosition = BirdControl.transform.position.z- startPosZ;

            // 距離と速度をテキストに表示
            distanceText.text = $"{currentZPosition:F0} m";
            speedText.text = $"{rb.velocity.z:F0} m/s";
        }
    }

    // FOVを速度に応じて滑らかに変更
    void UpdateCameraFieldOfView()
    {
        float currentSpeed = rb.velocity.z;

        // 加速している場合、FOVを増加
        if (currentSpeed >= previousZPosition)
        {
            currentFieldOfView += fovChangeSpeedUp * Time.deltaTime;
        }
        else if (currentSpeed < previousZPosition)
        {
            // 減速している場合、FOVを減少
            currentFieldOfView -= fovChangeSpeedDown * Time.deltaTime;
        }

        // FOVの範囲を制限
        currentFieldOfView = Mathf.Clamp(currentFieldOfView, minFieldOfView, maxFieldOfView);

        // カメラのFOVに適用
        mainCamera.fieldOfView = currentFieldOfView;

        // 前回の速度を更新
        previousZPosition = currentSpeed;
    }

    void UpdateCameraFieldOfView0()
    {
       if (100 < currentFieldOfView)
        {
            // 減速している場合、FOVを減少
            currentFieldOfView -= fovChangeSpeedDown * Time.deltaTime;
            mainCamera.fieldOfView = currentFieldOfView;
        }

        // FOVの範囲を制限
       // currentFieldOfView = Mathf.Clamp(currentFieldOfView, minFieldOfView, maxFieldOfView);

        // カメラのFOVに適用
       

        // 前回の速度を更新
        //previousZPosition = currentSpeed;
    }


}
