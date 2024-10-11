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
    public float clearTime = 10f; // 1��30�b = 90�b
    public BirdControl BirdControl;
    public GameObject LongHand;
    public GameObject ShortHand;
    public GameObject TimerBack;
    public GameObject ClearUI;

    private float previousZPosition = 0f; // �O���Z�ʒu
    private float cumulativeDistance = 0f; // �݌v����
    private float previousSpeed = 0f; // �O��̑��x
    private float elapsedTime = 0f; // �o�ߎ��Ԃ̃g���b�L���O
    private bool isClear = false; // �Q�[���N���A���ǂ����𔻒肷��t���O

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
            remainingTime = clearTime - timer; // �c�莞�Ԃ̌v�Z

            // �c�莞�Ԃ�0�����ɂȂ�Ȃ��悤�ɐ���
            remainingTime = Mathf.Max(remainingTime, 0f);

            // ���ƕb�ɕϊ�
            float minutes = Mathf.FloorToInt(remainingTime / 60f);
            float seconds = Mathf.FloorToInt(remainingTime % 60f);

            TimerBack.GetComponent<Image>().fillAmount = remainingTime / clearTime;

            // �^�C�}�[���e�L�X�g�ɕ\���i�c�莞�ԁj
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            // �����Ƒ��x�̌v�Z�ƕ\��
            UpdateDistanceAndSpeed();
        }
        // ���Ԃ�0�ɂȂ�����N���AUI��\��
        if (remainingTime <= 0f && !isClear)
        {
            ClearUI.SetActive(true);
            isClear = true; // �N���A��ԂɕύX
        }

       

        // ���j�ƒZ�j�̉�]���X�V
       // Hand();

        // ���N���b�N�������ꂽ��^�C�g���V�[���Ɉړ�
        if (isClear && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ToTitle();
        }
    }

    // �����Ƒ��x�̍X�V
    void UpdateDistanceAndSpeed()
    {
        elapsedTime += Time.deltaTime;

        // ���Ԋu���Ƃɋ����Ƒ��x���X�V
        if (elapsedTime >= 0.1f)
        {
            elapsedTime = 0f; // ���Z�b�g

            float currentZPosition = BirdControl.transform.position.z;
            float deltaZ = currentZPosition - previousZPosition; // Z�������̈ړ���
            float speed = deltaZ / 0.1f; // ���x = �ړ��� / ����

            // �݌v�������X�V
            cumulativeDistance += Mathf.Abs(deltaZ);

            // �����Ƒ��x���e�L�X�g�ɕ\��
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
