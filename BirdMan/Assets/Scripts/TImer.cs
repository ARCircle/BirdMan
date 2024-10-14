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
    public float clearTime = 10f; // 1��30�b = 90�b
    public BirdControl BirdControl;
    public GameObject LongHand;
    public GameObject ShortHand;
    public GameObject TimerBack;
   
    public GameObject TitleUI;
    public GameObject PracticeUI;
    public GameObject GameUI;
   
    public GameObject ClearUI;
    public Camera mainCamera;

    private float previousZPosition = 0f; // �O���Z�ʒu
    private float cumulativeDistance = 0f; // �݌v����
    private float elapsedTime = 0f; // �o�ߎ��Ԃ̃g���b�L���O
    private bool isClear = false; // �Q�[���N���A���ǂ����𔻒肷��t���O
    bool isGame;
    bool isTitle;
    bool isPractice;
    Rigidbody rb;

    public float minFieldOfView = 60f; // �ŏ�Field of View
    public float maxFieldOfView = 100f; // �ő�Field of View
    public float fovChangeSpeedUp = 2f; // FOV�̕ω����x�i���Ԃ�����̑����ʁj
    public float fovChangeSpeedDown = 2f; // FOV�̕ω����x�i���Ԃ�����̑����ʁj
    private float currentFieldOfView; // ���݂�FOV

    public AutoMouseControl autoMouseControl;

    void Awake()
    {
      

        // ��ʂ̕���1920�A������1080�A�E�B���h�E���[�h�Őݒ肷��
        Screen.SetResolution(640, 360, true);
        // �t���[�����[�g��60�ɐݒ�
        Application.targetFrameRate = 25;
        // �}�E�X�J�[�\�����\���ɂ��ă��b�N����
        Cursor.visible = false;

     
    }
    void Start()
    {
        previousZPosition = BirdControl.transform.position.z;
        rb = BirdControl.GetComponent<Rigidbody>();

      

        // �J�����̏���FOV��ݒ�
        currentFieldOfView = mainCamera.fieldOfView;
        ToTitle();
        //audioSourceWind1.PlayOneShot(soundWind1);
        audioSourceWind1.Play();
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


        // �����Ƒ��x���e�L�X�g�ɕ\��
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

        // C�L�[�ŃA�v���P�[�V�����I��
        if (keyboard.cKey.wasPressedThisFrame)
        {
            QuitApplication();
        }

        // R�L�[�ŃV�[�����ēǂݍ���
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
            UpdateCameraFieldOfView();

            // ���Ԃ�0�ɂȂ�����N���AUI��\��
            if (remainingTime <= 0f && !isClear)
            {
                ToClear();
              //  ClearUI.SetActive(true);
              //  isClear = true; // �N���A��ԂɕύX
            }

        }
        else
            UpdateCameraFieldOfView0();


        // ���N���b�N�������ꂽ��^�C�g���V�[���Ɉړ�
        if (isClear && Mouse.current.leftButton.wasPressedThisFrame)
        {
            //ToTitle();
            ToTitle();
        }
    }



    // �A�v���P�[�V�������I������֐�
    void QuitApplication()
    {
#if UNITY_EDITOR
        // Unity�G�f�B�^���œ��쒆�̏ꍇ�̓G�f�B�^���I�����Ȃ�
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // �r���h���ꂽ�A�v���P�[�V�����ł͏I������
            Application.Quit();
#endif
    }

    // ���݂̃V�[�����ēǂݍ��݂���֐�
    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // �����Ƒ��x�̍X�V
    float startPosZ;
    void UpdateDistanceAndSpeed()
    {
        elapsedTime += Time.deltaTime;

        // ���Ԋu���Ƃɋ����Ƒ��x���X�V
        if (elapsedTime >= 0.1f)
        {
            elapsedTime = 0f; // ���Z�b�g

            float currentZPosition = BirdControl.transform.position.z- startPosZ;

            // �����Ƒ��x���e�L�X�g�ɕ\��
            distanceText.text = $"{currentZPosition:F0} m";
            speedText.text = $"{rb.velocity.z:F0} m/s";
        }
    }

public AudioSource audioSourceWind1;
public AudioSource audioSourceWind2;
public AudioClip soundWind1;
public AudioClip soundWind2;
float pans;
    // FOV�𑬓x�ɉ����Ċ��炩�ɕύX
    void UpdateCameraFieldOfView()
    {
        float currentSpeed = rb.velocity.z;


        // �������Ă���ꍇ�AFOV�𑝉�
        if (currentSpeed >= previousZPosition)
        {
            currentFieldOfView += fovChangeSpeedUp * Time.deltaTime;
        }
        else if (currentSpeed < previousZPosition)
        {
            // �������Ă���ꍇ�AFOV������
            currentFieldOfView -= fovChangeSpeedDown * Time.deltaTime;
        }

         /*
    float middleFieldOfView= (minFieldOfView+maxFieldOfView)/2;
    if( currentFieldOfView< middleFieldOfView ){
         SEVolume(audioSourceWind1,soundWind1,true);
         SEVolume(audioSourceWind2,soundWind2,false);
 
    
    }
    else{
         SEVolume(audioSourceWind1,soundWind1,false);
         SEVolume(audioSourceWind2,soundWind2,true);
       
    }*/


        // FOV�͈̔͂𐧌�
        currentFieldOfView = Mathf.Clamp(currentFieldOfView, minFieldOfView, maxFieldOfView);
 float FieldOfViewMul= currentFieldOfView/minFieldOfView;
     audioSourceWind1.pitch = FieldOfViewMul*FieldOfViewMul*FieldOfViewMul+1f;
     audioSourceWind1.volume= FieldOfViewMul*FieldOfViewMul-0.9f;
     float panSpeed = 10f*FieldOfViewMul*FieldOfViewMul;
    // pans+=panSpeed;
      float panValue = Mathf.Sin(Time.time*panSpeed); // panSpeed は動きの速さ
      //audioSourceWind1.panStereo = panValue;
        // �J������FOV�ɓK�p
        mainCamera.fieldOfView = currentFieldOfView;

        // �O��̑��x���X�V
        previousZPosition = currentSpeed;
    }
void SEVolume(AudioSource audioSource,AudioClip sound, bool up)
{
    if (!up)
    {
        // 音量を下げる処理
        if (audioSource.volume > 0)
        {
            audioSource.volume -= Time.deltaTime * 1f; // 徐々に音量を減らす

            if (audioSource.volume < 0.1f)
            {
                audioSource.volume = 0; // マイナスにはならないようにする
                audioSource.Stop(); // 完全に音量が下がったら停止
            }
        }
    }
    else
    {
        // 音量を上げる処理
        if (!audioSource.isPlaying)
        {
            audioSource.Play(); // 再生が停止している場合のみ再生開始
            audioSource.PlayOneShot(sound);
            audioSource.volume = 1f; // 音量の上限を 1 に制限
          
        }

        audioSource.volume += Time.deltaTime * 1f; // 徐々に音量を上げる

        if (audioSource.volume > 1f)
        {
            audioSource.volume = 1f; // 音量の上限を 1 に制限
        }
    }
}

    void UpdateCameraFieldOfView0()
    {
       if (100 < currentFieldOfView)
        {
            // �������Ă���ꍇ�AFOV������
            currentFieldOfView -= fovChangeSpeedDown * Time.deltaTime;
            mainCamera.fieldOfView = currentFieldOfView;
        }

        // FOV�͈̔͂𐧌�
       // currentFieldOfView = Mathf.Clamp(currentFieldOfView, minFieldOfView, maxFieldOfView);

        // �J������FOV�ɓK�p
       

        // �O��̑��x���X�V
        //previousZPosition = currentSpeed;
    }


}
