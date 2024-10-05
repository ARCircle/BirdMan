using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Animations.Rigging;

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
    public float rotationSpeed = 5f;
    public Image ClickPositionImage;
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

    private float lastDragDistanceY;
    private float dragTimeCounter;
    public float dragTimeThreshold = 0.5f;
    public float oscillationSpeed = 1.0f;
    public float oscillationAmplitude = 5.0f;
    private float currentRotationValue = 0f;
    private float startTime;
    Vector2 dragDistance;

    // 追加: 入力情報を保持する変数
    private float inputDragX;
    private float inputDragY;
    private float inputRotationZL;
    private float inputRotationZR;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        ClickPositionImage.gameObject.SetActive(false);
        anime = Player.GetComponent<Animator>();
        initialRotationX = Tail.transform.localEulerAngles.x;
    }

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
                currentRotationValue = dragDistance.y * 0.1f;

                startTime = 0f;
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
                if (Mathf.Abs(dragDistance.y) > 1)
                {
                    currentRotationValue = dragDistance.y * 0.1f;
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

            // 入力情報を保持
            inputDragX = dragDistance.x;
            inputDragY = dragDistance.y;
            inputRotationZL = targetRotationZL;
            inputRotationZR = targetRotationZR;
        }
        else
        {
            targetRotationZL = 0f;
            isArmL = false;
            targetRotationZR = 0f;
            isArmR = false;
            isDragging = false;
            ClickPositionImage.gameObject.SetActive(false);

            // 入力情報をリセット
            inputDragX = 0f;
            inputDragY = 0f;
            inputRotationZL = 0f;
            inputRotationZR = 0f;
        }

        // 翼の回転処理
        newRotationZL = Mathf.MoveTowardsAngle(WingL.transform.eulerAngles.z, targetRotationZL, rotationSpeed * Time.deltaTime);
        WingL.transform.rotation = Quaternion.Euler(WingL.transform.eulerAngles.x, WingL.transform.eulerAngles.y, newRotationZL);

        newRotationZR = Mathf.MoveTowardsAngle(WingR.transform.eulerAngles.z, targetRotationZR, rotationSpeed * Time.deltaTime);
        WingR.transform.rotation = Quaternion.Euler(WingR.transform.eulerAngles.x, WingR.transform.eulerAngles.y, newRotationZR);

        // アニメーション処理
        AngleText(newRotationZL, newRotationZR);
        Anime(newRotationZL, newRotationZR, dragDistance.x, dragDistance.y);

        if (rb.velocity.y < minVelocityY)
            rb.velocity = new Vector3(rb.velocity.x, minVelocityY, rb.velocity.z);

        IKWeightOscillator();
        TailOscillator();
    }

    void FixedUpdate()
    {
        if (Player.name == "Bird")
            Bird(inputRotationZL, inputRotationZR, inputDragX);
        if (Player.name == "Plane")
            Plane(inputDragX, inputDragY);
    }

    public ChainIKConstraint chainIKR;
    public ChainIKConstraint chainIKL;
    public float frequency = 1.0f;
    public float amplitude = 0.5f;
    public float baseWeight = 0.5f;

    public GameObject Tail;
    public float frequencyTail = 1.0f;
    public float amplitudeTail = 30.0f;
    private float initialRotationX;

    void IKWeightOscillator()
    {
        // Weightを振動させる（正弦波で0〜1の範囲に変化）
        float oscillation = amplitude * Mathf.Sin(Time.time * frequency * 2.0f * Mathf.PI);
        chainIKR.weight = Mathf.Clamp(baseWeight + oscillation, 0.0f, 1.0f);
        chainIKL.weight = Mathf.Clamp(baseWeight + oscillation, 0.0f, 1.0f);
    }

    public GameObject Hip;
    public float hipRotationInfluence = 1.0f;

    void TailOscillator()
    {
        // TailのX軸のみを振動させる
        float oscillation = amplitudeTail * Mathf.Sin(Time.time * frequencyTail * 2.0f * Mathf.PI);

        // HipのY軸の回転角度を取得（-180から180の範囲）
        float hipRotationY = Hip.transform.localEulerAngles.y;
        if (hipRotationY > 180) hipRotationY -= 360;

        // HipのY軸回転に応じたTailのX軸の追加回転量を計算
        float hipRotationEffect = hipRotationY * hipRotationInfluence;

        // Tailの回転を更新
        Vector3 newRotation = Tail.transform.localEulerAngles;
        newRotation.x = initialRotationX + oscillation + hipRotationEffect;
        Tail.transform.localEulerAngles = newRotation;
    }

    private float previousRotationZL = 0f;
    private float previousRotationZR = 0f;

    void Bird(float L, float R, float dragX)
    {
        float forwardSpeed = Vector3.Dot(rb.velocity, rb.transform.forward);
        if (R > 20)
            R = R - 360;
        if (L > 20)
            L = L - 360;

        // 左右への力の適用
        if (Mathf.Abs(dragX * 0.1f) > 60f)
            dragX = 600 * Mathf.Sign(dragX);
        rb.AddForce(Vector3.right * dragX * 0.1f * bird.forceMultiplierLeft);

        if (forwardSpeed < bird.maxForwardSpeed)
        {
            rb.AddForce(rb.transform.forward * bird.forceMultiplierForward);
        }

        if (isArmL || isArmR)
        {
            rb.AddForce(rb.transform.up * bird.resistance);
        }

        float upSpeed = Vector3.Dot(rb.velocity, rb.transform.up);
        float forceMagnitude = (previousRotationZL - L) + (previousRotationZR - R);

        if (L > 0 || R > 0)
        {
            rb.AddForce(Vector3.up * (L + R) * bird.forceMultiplierFallDown);
            if (forwardSpeed < bird.maxForwardFallSpeed)
            {
                rb.AddForce(rb.transform.forward * bird.forceMultiplierFallForward);
            }
        }

        if (L < 0 || R < 0)
        {
            rb.AddForce(rb.transform.forward * bird.forceMultiplierForwardStop);
        }

        if (forceMagnitude > bird.forceMagnitudeThreshold)
        {
            if (upSpeed < bird.maxUpSpeed)
            {
                rb.AddForce(Vector3.up * forceMagnitude * bird.forceMultiplierUp);
            }
        }
        else if (forceMagnitude < -bird.forceMagnitudeThreshold)
        {
            if (upSpeed < bird.maxDownSpeed)
            {
                rb.AddForce(Vector3.up * forceMagnitude * bird.forceMultiplierDown);
            }
        }

        previousRotationZL = L;
        previousRotationZR = R;
    }

    void Plane(float dragX, float dragY)
    {
        if (Mathf.Abs(dragX * 0.1f) > 60f)
            dragX = 60 * Mathf.Sign(dragX);
        rb.AddForce(Vector3.right * dragX * 0.1f * plane.forceMultiplierLeft);

        if (Mathf.Abs(dragY * 0.1f) > 60f)
            dragY = 60 * Mathf.Sign(dragY);
        rb.AddForce(Vector3.up * dragY * 0.01f * plane.forceMultiplierUp);

        float forwardSpeed = Vector3.Dot(rb.velocity, rb.transform.forward);

        if (forwardSpeed < plane.maxForwardSpeed)
        {
            rb.AddForce(rb.transform.forward * plane.forceMultiplierForward);
        }
    }

    void AngleText(float L, float R)
    {
        // テキスト表示の処理（省略）
    }

    void Anime(float L, float R, float dragX, float dragY)
    {
        if (R > 20)
            R = R - 360;
        if (L > 20)
            L = L - 360;

        if (Mathf.Abs(dragX * 0.1f) > 60f)
            dragX = 600 * Mathf.Sign(dragX);
        anime.SetFloat("Left", L / 20f);
        anime.SetFloat("Right", R / 20f);
        anime.SetFloat("Turn", dragX * 0.1f / 40f / 3f);
        anime.SetFloat("Pitch", dragY * 0.1f / 40f / 3f);
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
