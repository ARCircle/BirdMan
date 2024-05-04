using UnityEngine;
using UnityEngine.InputSystem;
using TMPro; // TextMeshPro���g�p���邽�߂ɕK�v
using UnityEngine.SceneManagement;
using System.Collections;

public class BirdControl : MonoBehaviour
{
    public GameObject Bird;
    public GameObject WingL;
    public GameObject WingR;
    private float targetRotationZL = 0f;
    private float targetRotationZR = 0f;
    public float rotationSpeed = 5f; // ��]���x�𒲐�����
    public TextMeshProUGUI AngleL; // �����̉�]��\������e�L�X�g
    public TextMeshProUGUI AngleR; // �E���̉�]��\������e�L�X�g
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
        // �L�[�{�[�h�̏�Ԃ��擾
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            // F�L�[��D�L�[�ō�����Z����]�𐧌�
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
            // J�L�[��K�L�[�ŉE����Z����]�𐧌�
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

            // ������Z���̉�]��ڕW�l�ɋ߂Â���
           newRotationZL = Mathf.MoveTowardsAngle(WingL.transform.eulerAngles.z, targetRotationZL, rotationSpeed * Time.deltaTime);
            WingL.transform.rotation = Quaternion.Euler(WingL.transform.eulerAngles.x, WingL.transform.eulerAngles.y, newRotationZL);

            // �E����Z���̉�]��ڕW�l�ɋ߂Â���
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
    // forceMultiplier�͗͂̑傫���𒲐����邽�߂̕ϐ�
    public float forceMultiplierUp = 1f;
    public float forceMultiplierDown = 1f;
    public float forceMultiplierLeft = 1f;
    public float forceMultiplierFallDown = 1f;
    public float maxForwardSpeed = 10f; // �O�����̑��x�̏��
    public float maxForwardFallSpeed = 100f; // �O�����̑��x�̏��
    public float forceMultiplierForward = 1f;
    public float forceMultiplierFallForward = 1f;
    public float forceMultiplierForwardStop= 1f;
    public float forceMagnitudethreshold = 0.1f;
    public float maxUpSpeed = 10f; // ������̑��x�̏��
    public float maxDownSpeed = -10f; // �������̑��x�̉���
    public float maxLift = 1f;
    public float liftMultipler = 1f;
    public float resistance = -1f;
    public Animator anime;
    void Fly(float L, float R)
    {
        //�i�s�����ւ�
        // Rigidbody�̑��x�x�N�g������O�����̐����𒊏o
        float forwardSpeed = Vector3.Dot(rb.velocity, rb.transform.forward);
        if (R > 20)
            R = R - 360;
        if (L > 20)
            L = L - 360;
        rb.AddForce((Vector3.right * (L - R) * forceMultiplierLeft));

        // �O�����̑��x��臒l�ȉ��ł��邩�m�F
        if (forwardSpeed < maxForwardSpeed)
        {
            // �O�����̑��x��臒l�ȉ��̏ꍇ�A�O���ɗ͂�������
            rb.AddForce(rb.transform.forward * forceMultiplierForward);
        }

        if (isArmL || isArmR)//�r�����������Ă��鎞�͉�����
        {
            rb.AddForce(rb.transform.up * resistance);

        }
        float upSpeed = Vector3.Dot(rb.velocity, rb.transform.up);
    
        // ��]�̕ω��ʂɊ�Â��ď�����̗͂��v�Z
        float forceMagnitude = (previousRotationZL - L) + (previousRotationZR - R);

        if (L > 0 || R > 0)
        {
            rb.AddForce(Vector3.up * (L+R) * forceMultiplierFallDown); // forceMultiplier�͗͂̑傫���𒲐����邽�߂̕ϐ�
            if (forwardSpeed < maxForwardFallSpeed)
            {
                rb.AddForce(rb.transform.forward * forceMultiplierFallForward);
            }
        }
        if (L < 0 || R < 0)
        {
          // forceMultiplier�͗͂̑傫���𒲐����邽�߂̕ϐ�
        
               rb.AddForce(rb.transform.forward * forceMultiplierForwardStop);
            
        }

        print(forceMagnitude);
        if (forceMagnitude > forceMagnitudethreshold)
        {
            if (upSpeed < maxUpSpeed)
            {
                rb.AddForce(Vector3.up * forceMagnitude * forceMultiplierUp); // forceMultiplier�͗͂̑傫���𒲐����邽�߂̕ϐ�
            }
        }
        else if (forceMagnitude < -forceMagnitudethreshold)
        {
            if (upSpeed < maxDownSpeed)
            {
                rb.AddForce(Vector3.up * forceMagnitude * forceMultiplierDown); // forceMultiplier�͗͂̑傫���𒲐����邽�߂̕ϐ�
            }
        }
      //  Bird.transform.rotation = Quaternion.Euler(Bird.transform.eulerAngles.x, (L - R) , Bird.transform.eulerAngles.z);
        // ���݂̉�]��ۑ�
        previousRotationZL = L;
        previousRotationZR = R;
    
    }
    void AngleText(float L, float R)
    {
        // �����ƉE���̉�]���e�L�X�g�ɕ\��
        AngleL.text = "" + L.ToString("F0"); // �����_�ȉ�2���܂ŕ\��
        if (L > 20)
            AngleL.text = "" + (L - 360).ToString("F0"); // �����_�ȉ�2���܂ŕ\��
        AngleR.text = "" + R.ToString("F0"); // �����_�ȉ�2���܂ŕ\��
        if (R > 20)
            AngleR.text = "" + (R - 360).ToString("F0"); // �����_�ȉ�2���܂ŕ\��
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
                // Hearts�̍ŏ��̎q�I�u�W�F�N�g���폜
                Destroy(Hearts.transform.GetChild(0).gameObject);
                Destroy(collision.gameObject);
            }
            if (Hearts.transform.childCount == 1)
            {
                // �Q�[���I�[�o�[�I�u�W�F�N�g��\��

                GameOverUI.SetActive(true);

                StartCoroutine(ToTitle(3f));
                // �Q�[�����~
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
