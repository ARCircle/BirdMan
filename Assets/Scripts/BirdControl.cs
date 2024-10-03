using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // UIを使用するために必要
using System.Collections;
using UnityEngine.Animations.Rigging;  // Chain IK Constraintが含まれている名前空間

[System.Serializable]
public struct ObjectParameters
{
    public float forceMultiplierUp;
    public float forceMultiplierDown;
    public float forceMultiplierLeft;
    public float forceMultiplierFallDown;
    public float maxForwardSpeed;
    public float maxForwardFallSpeed;
    public float forceMultiplierForward;
    public float forceMultiplierFallForward;
    public float forceMultiplierForwardStop;
    public float forceMagnitudeThreshold;
    public float maxUpSpeed;
    public float maxDownSpeed;
    public float maxLift;
    public float liftMultiplier;
    public float resistance;
}
public class BirdControl : MonoBehaviour
{
    public GameObject Player;
    public GameObject WingL;
    public GameObject WingR;
    private float targetRotationZL = 0f;
    private float targetRotationZR = 0f;
    public float rotationSpeed = 5f; // 回転速度を調整する
    public Image ClickPositionImage; // 初期クリック地点を表示するイメージ
    bool isArmL;
    bool isArmR;
    Rigidbody rb;
    public float newRotationZL;
    public float newRotationZR;
    public float minVelocityY;

    private Vector2 initialClickPosition;
    private bool isDragging = false;
  
  

    public ObjectParameters bird;
    public ObjectParameters plane;
    Animator anime;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        ClickPositionImage.gameObject.SetActive(false); // 最初は非表示にしておく
   anime=Player.GetComponent<Animator>();
        // 現在のオブジェクトの初期X軸回転を保存
        initialRotationX = Tail.transform.localEulerAngles.x;
    }

    private float lastDragDistanceY;
    private float dragTimeCounter;
    public float dragTimeThreshold = 0.5f; // 一定時間の閾値（秒）
    public float oscillationSpeed = 1.0f; // 振動の速度
    public float oscillationAmplitude = 5.0f; // 振動の振幅
    private float currentRotationValue = 0f;
    private float startTime;
    Vector2 dragDistance;
    void Update()
    {
        var pointer = Pointer.current;
        if (pointer != null)
        {
            // ディスプレイの中心点を初期クリック位置として設定
            initialClickPosition = new Vector2(Screen.width / 2, Screen.height / 2);

            // マウスの現在の位置を常に取得
            Vector2 currentPosition = pointer.position.ReadValue();
            dragDistance = currentPosition - initialClickPosition;

            // 方向に応じた回転を計算
            float angle = Mathf.Atan2(dragDistance.y, dragDistance.x) * Mathf.Rad2Deg;
            ClickPositionImage.transform.parent.rotation = Quaternion.Euler(0, 0, angle);

            // 伸びるように幅を調整
            ClickPositionImage.rectTransform.sizeDelta = new Vector2(dragDistance.magnitude, ClickPositionImage.rectTransform.sizeDelta.y);

            // ドラッグ距離の変化をチェック
            if (Mathf.Abs(dragDistance.y - lastDragDistanceY) > 1)
            {
                dragTimeCounter = 0f;
                lastDragDistanceY = dragDistance.y;
                currentRotationValue = dragDistance.y * 0.1f; // 調整係数をかけて回転速度を調整

                startTime = 0f; // 振動をリセット
            }
            else
            {
                dragTimeCounter += Time.deltaTime;
            }

            if (dragTimeCounter >= dragTimeThreshold)
            {
                if (startTime == 0f)
                {
                    startTime = Time.time;
                }
                // 振動させる
                float oscillation = Mathf.Sin((Time.time - startTime) * oscillationSpeed) * oscillationAmplitude;
                isArmL = true;
                isArmR = true;
            }
            else
            {
                if (Mathf.Abs(dragDistance.y) > 1) // 上下方向へのドラッグ
                {
                    currentRotationValue = dragDistance.y * 0.1f; // 調整係数をかけて回転速度を調整
                    targetRotationZL = Mathf.Clamp(currentRotationValue, -20f, 20f);
                    isArmL = true;
                    targetRotationZR = Mathf.Clamp(currentRotationValue, -20f, 20f);
                    isArmR = true;
                }
                else
                {
                    targetRotationZL = 0f;
                    isArmL = false;
                    targetRotationZR = 0f;
                    isArmR = false;
                }
            }
        }
        else
        {
            targetRotationZL = 0f;
            isArmL = false;
            targetRotationZR = 0f;
            isArmR = false;
            isDragging = false;
            ClickPositionImage.gameObject.SetActive(false); // ドラッグ終了時に非表示にする
        }

        // 翼の回転処理
        newRotationZL = Mathf.MoveTowardsAngle(WingL.transform.eulerAngles.z, targetRotationZL, rotationSpeed * Time.deltaTime);
        WingL.transform.rotation = Quaternion.Euler(WingL.transform.eulerAngles.x, WingL.transform.eulerAngles.y, newRotationZL);

        newRotationZR = Mathf.MoveTowardsAngle(WingR.transform.eulerAngles.z, targetRotationZR, rotationSpeed * Time.deltaTime);
        WingR.transform.rotation = Quaternion.Euler(WingR.transform.eulerAngles.x, WingR.transform.eulerAngles.y, newRotationZR);

        if (Player.name == "Bird")
            Bird(newRotationZL, newRotationZR, dragDistance.x);
        if (Player.name == "Plane")
            Plane(dragDistance.x, dragDistance.y);

        AngleText(newRotationZL, newRotationZR);
        Anime(newRotationZL, newRotationZR, dragDistance.x, dragDistance.y);

        if (rb.velocity.y < minVelocityY)
            rb.velocity = new Vector3(rb.velocity.x, minVelocityY, rb.velocity.z);

        IKWeightOscillator();
        TailOscillator();
    }

    public ChainIKConstraint chainIKR;  // Chain IK Constraintをアタッチする
    public ChainIKConstraint chainIKL;  // Chain IK Constraintをアタッチする
    public float frequency = 1.0f;     // 振動の周波数
    public float amplitude = 0.5f;     // 振動の振幅
    public float baseWeight = 0.5f;    // Weightの基本値

    public GameObject Tail;
    public float frequencyTail = 1.0f;     // 振動の周波数
    public float amplitudeTail = 30.0f;    // 振動の振幅 (度数)
    private float initialRotationX;    // 初期のX軸回転を保存

    void IKWeightOscillator()
    {
       
            // Weightを振動させる（正弦波で0〜1の範囲に変化）
            float oscillation = amplitude * Mathf.Sin(Time.time * frequency * 2.0f * Mathf.PI);
            chainIKR.weight = Mathf.Clamp(baseWeight + oscillation, 0.0f, 1.0f);  // 0〜1の範囲に制限
            chainIKL.weight = Mathf.Clamp(baseWeight + oscillation, 0.0f, 1.0f);  // 0〜1の範囲に制限
        
    }

   
    public GameObject Hip;
    public float hipRotationInfluence = 1.0f; // Hipの回転がTailのX軸回転に与える影響度

    void TailOscillator()
    {
        // TailのX軸のみを振動させる
        float oscillation = amplitudeTail * Mathf.Sin(Time.time * frequencyTail * 2.0f * Mathf.PI);

        // HipのY軸の回転角度を取得（-180から180の範囲）
        float hipRotationY = Hip.transform.localEulerAngles.y;
        if (hipRotationY > 180) hipRotationY -= 360;  // -180〜180に正規化

        // HipのY軸回転に応じたTailのX軸の追加回転量を計算
        float hipRotationEffect = hipRotationY * hipRotationInfluence;

        // Tailの回転を更新
        Vector3 newRotation = Tail.transform.localEulerAngles;
        newRotation.x = initialRotationX + oscillation + hipRotationEffect;  // 初期回転 + 振動 + Hipの影響
        Tail.transform.localEulerAngles = newRotation;  // 新しい回転を適用
    }
    private float previousRotationZL = 0f;
    private float previousRotationZR = 0f;
    /*public float forceMultiplierUp = 1f;
    public float forceMultiplierDown = 1f;
    public float forceMultiplierLeft = 1f;
    public float forceMultiplierFallDown = 1f;
    public float maxForwardSpeed = 10f;
    public float maxForwardFallSpeed = 100f;
    public float forceMultiplierForward = 1f;
    public float forceMultiplierFallForward = 1f;
    public float forceMultiplierForwardStop = 1f;
    public float forceMagnitudethreshold = 0.1f;
    public float maxUpSpeed = 10f;
    public float maxDownSpeed = -10f;
    public float maxLift = 1f;
    public float liftMultipler = 1f;
    public float resistance = -1f;*/
    

    void Bird(float L, float R,float dragX)
    {
       
        float forwardSpeed = Vector3.Dot(rb.velocity, rb.transform.forward);
        if (R > 20)
            R = R - 360;
        if (L > 20)
            L = L - 360;
        //rb.AddForce(Vector3.right * (L - R) * forceMultiplierLeft);
        if(Mathf.Abs( dragX * 0.1f) >60f)
            dragX = 600*Mathf.Sign(dragX);
        rb.AddForce(Vector3.right * dragX*0.1f * bird.forceMultiplierLeft*Time.deltaTime);

        if (forwardSpeed < bird.maxForwardSpeed)
        {
            rb.AddForce(rb.transform.forward * bird.forceMultiplierForward * Time.deltaTime);
        }

        if (isArmL || isArmR)
        {
            rb.AddForce(rb.transform.up * bird.resistance * Time.deltaTime);
        }

        float upSpeed = Vector3.Dot(rb.velocity, rb.transform.up);
        float forceMagnitude = (previousRotationZL - L) + (previousRotationZR - R);

        if (L > 0 || R > 0)
        {
            rb.AddForce(Vector3.up * (L + R) * bird.forceMultiplierFallDown * Time.deltaTime);
            if (forwardSpeed < bird.maxForwardFallSpeed)
            {
                rb.AddForce(rb.transform.forward * bird.forceMultiplierFallForward);
            }
        }

        if (L < 0 || R < 0)
        {
            rb.AddForce(rb.transform.forward * bird.forceMultiplierForwardStop * Time.deltaTime);
        }

        if (forceMagnitude > bird.forceMagnitudeThreshold)
        {
            if (upSpeed < bird.maxUpSpeed)
            {
                rb.AddForce(Vector3.up * forceMagnitude * bird.forceMultiplierUp * Time.deltaTime);
            }
        }
        else if (forceMagnitude < -bird.forceMagnitudeThreshold)
        {
            if (upSpeed < bird.maxDownSpeed)
            {
                rb.AddForce(Vector3.up * forceMagnitude * bird.forceMultiplierDown * Time.deltaTime);
            }
        }

        previousRotationZL = L;
        previousRotationZR = R;
    }

    void Plane(float dragX ,float dragY)
    {
        //print(Time.deltaTime+"  "+1/ Time.deltaTime);
        //rb.AddForce(Vector3.right * (L - R) * forceMultiplierLeft);
        if (Mathf.Abs(dragX * 0.1f) > 60f)
            dragX = 60 * Mathf.Sign(dragX);
        rb.AddForce(Vector3.right * dragX * 0.1f * plane.forceMultiplierLeft * Time.deltaTime);

        if (Mathf.Abs(dragY * 0.1f) > 60f)
            dragY = 60 * Mathf.Sign(dragY);
        rb.AddForce(Vector3.up * dragY * 0.01f * plane.forceMultiplierUp * Time.deltaTime);

        float forwardSpeed = Vector3.Dot(rb.velocity, rb.transform.forward);
      
        if (forwardSpeed < plane.maxForwardSpeed)
        {
            rb.AddForce(rb.transform.forward * plane.forceMultiplierForward * Time.deltaTime);
        }
    }

    void AngleText(float L, float R)
    {
       // AngleL.text = "" + L.ToString("F0");
      //  if (L > 20)
        //    AngleL.text = "" + (L - 360).ToString("F0");
       // AngleR.text = "" + R.ToString("F0");
       // if (R > 20)
         //   AngleR.text = "" + (R - 360).ToString("F0");
    }

    
    void Anime(float L, float R,float dragX, float dragY)
    {
        if (R > 20)
            R = R - 360;
        if (L > 20)
            L = L - 360;

        if (Mathf.Abs(dragX * 0.1f) >60f)
            dragX = 600 * Mathf.Sign(dragX);
        anime.SetFloat("Left", L / 20f);
        anime.SetFloat("Right", R / 20f);
        anime.SetFloat("Turn", dragX * 0.1f / 40f / 3f);
        anime.SetFloat("Pitch", dragY * 0.1f / 40f / 3f);
        /*
        anime.SetFloat("Left", L / 20f);
        anime.SetFloat("Right", R / 20f);
        anime.SetFloat("Turn", (L - R) / 40f / 3f);*/
    }

    public GameObject Hearts;
    public GameObject GameOverUI;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            if (Hearts.transform.childCount > 0)
            {
                Destroy(Hearts.transform.GetChild(0).gameObject);
                Destroy(collision.gameObject);
            }
            if (Hearts.transform.childCount == 1)
            {
                GameOverUI.SetActive(true);
                StartCoroutine(ToTitle(3f));
            }
        }
    }

    private IEnumerator ToTitle(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("Title");
    }
}
