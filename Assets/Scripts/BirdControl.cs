using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // UI���g�p���邽�߂ɕK�v
using System.Collections;

public class BirdControl : MonoBehaviour
{
    public GameObject Bird;
    public GameObject WingL;
    public GameObject WingR;
    private float targetRotationZL = 0f;
    private float targetRotationZR = 0f;
    public float rotationSpeed = 5f; // ��]���x�𒲐�����
    public Image ClickPositionImage; // �����N���b�N�n�_��\������C���[�W
    bool isArmL;
    bool isArmR;
    Rigidbody rb;
    public float newRotationZL;
    public float newRotationZR;
    public float minVelocityY;

    private Vector2 initialClickPosition;
    private bool isDragging = false;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        ClickPositionImage.gameObject.SetActive(false); // �ŏ��͔�\���ɂ��Ă���
    }

    private float lastDragDistanceY;
    private float dragTimeCounter;
    public float dragTimeThreshold = 0.5f; // ��莞�Ԃ�臒l�i�b�j
    public float oscillationSpeed = 1.0f; // �U���̑��x
    public float oscillationAmplitude = 5.0f; // �U���̐U��
    private float currentRotationValue = 0f;
    private float startTime;
    Vector2 dragDistance;
    void Update()
    {
        var pointer = Pointer.current;
        if (pointer != null)
        {
            if (pointer.press.isPressed)
            {
                if (!isDragging)
                {

                    // �h���b�O�J�n
                    initialClickPosition = pointer.position.ReadValue();
                    isDragging = true;
                    lastDragDistanceY = initialClickPosition.y;
                    dragTimeCounter = 0f;
                    currentRotationValue = 0f;
                    startTime = 0f; // ������
                    // �����N���b�N�n�_����ʂɕ\��
                    ClickPositionImage.transform.parent.position = initialClickPosition;
                    ClickPositionImage.gameObject.SetActive(true);


                }
                else
                {
                    // �h���b�O��
                    Vector2 currentPosition = pointer.position.ReadValue();
                    dragDistance = currentPosition - initialClickPosition;

                    // �����ɉ�������]���v�Z
                    float angle = Mathf.Atan2(dragDistance.y, dragDistance.x) * Mathf.Rad2Deg;
                    ClickPositionImage.transform.parent.rotation = Quaternion.Euler(0, 0, angle);

                    // �L�т�悤�ɕ��𒲐�
                    ClickPositionImage.rectTransform.sizeDelta = new Vector2(dragDistance.magnitude, ClickPositionImage.rectTransform.sizeDelta.y);

                    // �h���b�O�����̕ω����`�F�b�N
                    if (Mathf.Abs(dragDistance.y - lastDragDistanceY) > 1)
                    {
                        dragTimeCounter = 0f;
                        lastDragDistanceY = dragDistance.y;
                        currentRotationValue = dragDistance.y * 0.1f; // �����W���������ĉ�]���x�𒲐�

                        startTime = 0f; // �U�������Z�b�g
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
                        // �U��������
                        float oscillation = Mathf.Sin((Time.time - startTime) * oscillationSpeed) * oscillationAmplitude;
                       // targetRotationZL = Mathf.Clamp(currentRotationValue + oscillation, -20f, 20f);
                       // targetRotationZR = Mathf.Clamp(currentRotationValue + oscillation, -20f, 20f);
                        isArmL = true;
                        isArmR = true;
                    }
                    else
                    {
                        if (Mathf.Abs(dragDistance.y) > 1) // �㉺�����ւ̃h���b�O
                        {
                            currentRotationValue = dragDistance.y * 0.1f; // �����W���������ĉ�]���x�𒲐�
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
                    /* if (Mathf.Abs(dragDistance.x) > 1) // �㉺�����ւ̃h���b�O
                     {
                         if (Mathf.Abs(dragDistance.x * 0.1f) >= 10)
                             dragDistance.x = 10 * Mathf.Sign(dragDistance.x);
                         targetRotationZL += dragDistance.x * 0.1f; // �����W���������ĉ�]���x�𒲐�
                             isArmL = true;
                             targetRotationZR -= dragDistance.x * 0.1f;
                             isArmR = true;

                     }*/
                }
            }
            else
            {
                targetRotationZL = 0f;
                isArmL = false;
                targetRotationZR = 0f;
                isArmR = false;
                isDragging = false;
                ClickPositionImage.gameObject.SetActive(false); // �h���b�O�I�����ɔ�\���ɂ���
            }
            // ������Z���̉�]��ڕW�l�ɋ߂Â���
            newRotationZL = Mathf.MoveTowardsAngle(WingL.transform.eulerAngles.z, targetRotationZL, rotationSpeed * Time.deltaTime);
            WingL.transform.rotation = Quaternion.Euler(WingL.transform.eulerAngles.x, WingL.transform.eulerAngles.y, newRotationZL);

            // �E����Z���̉�]��ڕW�l�ɋ߂Â���
            newRotationZR = Mathf.MoveTowardsAngle(WingR.transform.eulerAngles.z, targetRotationZR, rotationSpeed * Time.deltaTime);
            WingR.transform.rotation = Quaternion.Euler(WingR.transform.eulerAngles.x, WingR.transform.eulerAngles.y, newRotationZR);
            Fly(newRotationZL, newRotationZR,dragDistance.x);
            AngleText(newRotationZL, newRotationZR);
            Anime(newRotationZL, newRotationZR, dragDistance.x);

            if (rb.velocity.y < minVelocityY)
                rb.velocity = new Vector3(rb.velocity.x, minVelocityY, rb.velocity.z);
        }
    }

    private float previousRotationZL = 0f;
    private float previousRotationZR = 0f;
    public float forceMultiplierUp = 1f;
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
    public float resistance = -1f;
    public Animator anime;

    void Fly(float L, float R,float dragX)
    {
        float forwardSpeed = Vector3.Dot(rb.velocity, rb.transform.forward);
        if (R > 20)
            R = R - 360;
        if (L > 20)
            L = L - 360;
        //rb.AddForce(Vector3.right * (L - R) * forceMultiplierLeft);
        if(Mathf.Abs( dragX * 0.1f) >60f)
            dragX = 60*Mathf.Sign(dragX);
        rb.AddForce(Vector3.right * dragX*0.1f * forceMultiplierLeft);

        if (forwardSpeed < maxForwardSpeed)
        {
            rb.AddForce(rb.transform.forward * forceMultiplierForward);
        }

        if (isArmL || isArmR)
        {
            rb.AddForce(rb.transform.up * resistance);
        }

        float upSpeed = Vector3.Dot(rb.velocity, rb.transform.up);
        float forceMagnitude = (previousRotationZL - L) + (previousRotationZR - R);

        if (L > 0 || R > 0)
        {
            rb.AddForce(Vector3.up * (L + R) * forceMultiplierFallDown);
            if (forwardSpeed < maxForwardFallSpeed)
            {
                rb.AddForce(rb.transform.forward * forceMultiplierFallForward);
            }
        }

        if (L < 0 || R < 0)
        {
            rb.AddForce(rb.transform.forward * forceMultiplierForwardStop);
        }

        if (forceMagnitude > forceMagnitudethreshold)
        {
            if (upSpeed < maxUpSpeed)
            {
                rb.AddForce(Vector3.up * forceMagnitude * forceMultiplierUp);
            }
        }
        else if (forceMagnitude < -forceMagnitudethreshold)
        {
            if (upSpeed < maxDownSpeed)
            {
                rb.AddForce(Vector3.up * forceMagnitude * forceMultiplierDown);
            }
        }

        previousRotationZL = L;
        previousRotationZR = R;
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

    void Anime(float L, float R,float dragX)
    {
        if (R > 20)
            R = R - 360;
        if (L > 20)
            L = L - 360;

        if (Mathf.Abs(dragX * 0.1f) >60f)
            dragX = 60 * Mathf.Sign(dragX);
        anime.SetFloat("Left", L / 20f);
        anime.SetFloat("Right", R / 20f);
        anime.SetFloat("Turn", dragX * 0.1f / 40f / 3f);
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
