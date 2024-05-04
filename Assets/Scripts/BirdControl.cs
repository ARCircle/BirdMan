using UnityEngine;
using UnityEngine.InputSystem;
using TMPro; // TextMeshProを使用するために必要
using UnityEngine.SceneManagement;
using System.Collections;

public class BirdControl : MonoBehaviour
{
    public GameObject Bird;
    public GameObject WingL;
    public GameObject WingR;
    private float targetRotationZL = 0f;
    private float targetRotationZR = 0f;
    public float rotationSpeed = 5f; // 回転速度を調整する
    public TextMeshProUGUI AngleL; // 左翼の回転を表示するテキスト
    public TextMeshProUGUI AngleR; // 右翼の回転を表示するテキスト
    bool isArmL;
    bool isArmR;
    Rigidbody rb;
    public float newRotationZL;
    public float newRotationZR;
    public float minVelocityY;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        // キーボードの状態を取得
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            // FキーとDキーで左翼のZ軸回転を制御
            if (keyboard.fKey.isPressed)
            {
                targetRotationZL = -20f;
                isArmL = true;
            }
            else if (keyboard.dKey.isPressed)
            {
                targetRotationZL = 20f;
                isArmL = true;
            }
          
            else
            {
                targetRotationZL = 0f;
                isArmL = false;
            }

               if (keyboard.jKey.isPressed)
            {
                targetRotationZR = -20f;
                isArmR = true;
            }
            // JキーとKキーで右翼のZ軸回転を制御
            else if (keyboard.kKey.isPressed) //semicolonKey
            {
                targetRotationZR = 20f;
                isArmR = true;
            }
          
            else
            {
                targetRotationZR = 0f;
                isArmR = false;
            }

            // 左翼のZ軸の回転を目標値に近づける
           newRotationZL = Mathf.MoveTowardsAngle(WingL.transform.eulerAngles.z, targetRotationZL, rotationSpeed * Time.deltaTime);
            WingL.transform.rotation = Quaternion.Euler(WingL.transform.eulerAngles.x, WingL.transform.eulerAngles.y, newRotationZL);

            // 右翼のZ軸の回転を目標値に近づける
            newRotationZR = Mathf.MoveTowardsAngle(WingR.transform.eulerAngles.z, targetRotationZR, rotationSpeed * Time.deltaTime);
            WingR.transform.rotation = Quaternion.Euler(WingR.transform.eulerAngles.x, WingR.transform.eulerAngles.y, newRotationZR);
            Fly(newRotationZL, newRotationZR);
            AngleText(newRotationZL, newRotationZR);
            Anime(newRotationZL, newRotationZR);

            if(rb.velocity.y<minVelocityY)
                rb.velocity= new Vector3(rb.velocity.x, minVelocityY,rb.velocity.z) ;
        }
    }
    private float previousRotationZL = 0f;
    private float previousRotationZR = 0f;
    // forceMultiplierは力の大きさを調整するための変数
    public float forceMultiplierUp = 1f;
    public float forceMultiplierDown = 1f;
    public float forceMultiplierLeft = 1f;
    public float forceMultiplierFallDown = 1f;
    public float maxForwardSpeed = 10f; // 前方向の速度の上限
    public float maxForwardFallSpeed = 100f; // 前方向の速度の上限
    public float forceMultiplierForward = 1f;
    public float forceMultiplierFallForward = 1f;
    public float forceMultiplierForwardStop= 1f;
    public float forceMagnitudethreshold = 0.1f;
    public float maxUpSpeed = 10f; // 上方向の速度の上限
    public float maxDownSpeed = -10f; // 下方向の速度の下限
    public float maxLift = 1f;
    public float liftMultipler = 1f;
    public float resistance = -1f;
    public Animator anime;
    void Fly(float L, float R)
    {
        //進行方向への
        // Rigidbodyの速度ベクトルから前方向の成分を抽出
        float forwardSpeed = Vector3.Dot(rb.velocity, rb.transform.forward);
        if (R > 20)
            R = R - 360;
        if (L > 20)
            L = L - 360;
        rb.AddForce((Vector3.right * (L - R) * forceMultiplierLeft));

        // 前方向の速度が閾値以下であるか確認
        if (forwardSpeed < maxForwardSpeed)
        {
            // 前方向の速度が閾値以下の場合、前方に力を加える
            rb.AddForce(rb.transform.forward * forceMultiplierForward);
        }

        if (isArmL || isArmR)//腕をうごかしている時は下がる
        {
            rb.AddForce(rb.transform.up * resistance);

        }
        float upSpeed = Vector3.Dot(rb.velocity, rb.transform.up);
    
        // 回転の変化量に基づいて上向きの力を計算
        float forceMagnitude = (previousRotationZL - L) + (previousRotationZR - R);

        if (L > 0 || R > 0)
        {
            rb.AddForce(Vector3.up * (L+R) * forceMultiplierFallDown); // forceMultiplierは力の大きさを調整するための変数
            if (forwardSpeed < maxForwardFallSpeed)
            {
                rb.AddForce(rb.transform.forward * forceMultiplierFallForward);
            }
        }
        if (L < 0 || R < 0)
        {
          // forceMultiplierは力の大きさを調整するための変数
        
               rb.AddForce(rb.transform.forward * forceMultiplierForwardStop);
            
        }

        print(forceMagnitude);
        if (forceMagnitude > forceMagnitudethreshold)
        {
            if (upSpeed < maxUpSpeed)
            {
                rb.AddForce(Vector3.up * forceMagnitude * forceMultiplierUp); // forceMultiplierは力の大きさを調整するための変数
            }
        }
        else if (forceMagnitude < -forceMagnitudethreshold)
        {
            if (upSpeed < maxDownSpeed)
            {
                rb.AddForce(Vector3.up * forceMagnitude * forceMultiplierDown); // forceMultiplierは力の大きさを調整するための変数
            }
        }
      //  Bird.transform.rotation = Quaternion.Euler(Bird.transform.eulerAngles.x, (L - R) , Bird.transform.eulerAngles.z);
        // 現在の回転を保存
        previousRotationZL = L;
        previousRotationZR = R;
    
    }
    void AngleText(float L, float R)
    {
        // 左翼と右翼の回転をテキストに表示
        AngleL.text = "" + L.ToString("F0"); // 小数点以下2桁まで表示
        if (L > 20)
            AngleL.text = "" + (L - 360).ToString("F0"); // 小数点以下2桁まで表示
        AngleR.text = "" + R.ToString("F0"); // 小数点以下2桁まで表示
        if (R > 20)
            AngleR.text = "" + (R - 360).ToString("F0"); // 小数点以下2桁まで表示
    }
    void Anime(float L, float R)
    {
        if (R > 20)
            R = R - 360;
        if (L > 20)
            L = L - 360;
                anime.SetFloat("Left",L / 20f);
        anime.SetFloat("Right", R / 20f);
        anime.SetFloat("Turn", (L-R)/40f/3f );

    }
    public GameObject Hearts;
    public GameObject GameOverUI;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            
          
            if (Hearts.transform.childCount > 0)
            {
                // Heartsの最初の子オブジェクトを削除
                Destroy(Hearts.transform.GetChild(0).gameObject);
                Destroy(collision.gameObject);
            }
            if (Hearts.transform.childCount == 1)
            {
                // ゲームオーバーオブジェクトを表示

                GameOverUI.SetActive(true);

                StartCoroutine(ToTitle(3f));
                // ゲームを停止
                // Time.timeScale = 0f;

            }
        }
    }
    private IEnumerator ToTitle(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("Title");
    }

}
