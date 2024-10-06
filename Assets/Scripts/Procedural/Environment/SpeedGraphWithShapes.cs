using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Shapes2D;

public class SpeedGraphWithShapes : MonoBehaviour
{
    [Header("Player Settings")]
    public Transform playerTransform; // �v���C���[��Transform
    public Camera mainCamera; // �J����

    [Header("Sampling Settings")]
    public float sampleInterval = 0.1f; // �T���v�����O�Ԋu
    public int maxDataPoints = 100; // �O���t�ɕ\������ő�f�[�^�|�C���g��

    [Header("UI Settings")]
    public Text distanceText;
    public Text speedText;

    [Header("Graph Settings")]
    public GameObject shapePrefab; // Shape��Prefab
    public Transform graphContainer; // �O���t��\�����邽�߂̐e�I�u�W�F�N�g
    public float graphWidth = 300f; // �O���t�̉���
    public float graphHeight = 150f; // �O���t�̍���

    [Header("Graph Mode")]
    public bool isDifferenceGraph = false; // True�ő��x�ω��O���t�AFalse�ő��x�O���t

    [Header("Opacity Settings")]
    public float fillOpacity = 0.5f; // �����x�𐧌䂷�邽�߂̕ϐ� (0.0 - 1.0)

    [Header("Camera Settings")]
    public float minFieldOfView = 60f; // �ŏ�Field of View
    public float maxFieldOfView = 100f; // �ő�Field of View
    public float fovChangeSpeedUp = 2f; // FOV�̕ω����x�i���Ԃ�����̑����ʁj
    public float fovChangeSpeedDown = 2f; // FOV�̕ω����x�i���Ԃ�����̑����ʁj

    private List<float> speedData = new List<float>(); // ���x�f�[�^
    private List<float> speedDifferenceData = new List<float>(); // ���x�����f�[�^
    private List<GameObject> shapeInstances = new List<GameObject>(); // Shape�̃C���X�^���X
    private float elapsedTime = 0f;
    private float previousZPosition = 0f;
    private float cumulativeDistance = 0f; // �݌v����
    private float previousSpeed = 0f; // �O��̑��x��ێ�
    private float currentFieldOfView; // ���݂�FOV

    void Start()
    {
        if (playerTransform == null || shapePrefab == null || mainCamera == null)
        {
            Debug.LogError("Player Transform, Shape Prefab, �܂��� Camera ���ݒ肳��Ă��܂���B");
            enabled = false;
            return;
        }

        previousZPosition = playerTransform.position.z;
        currentFieldOfView = mainCamera.fieldOfView;

        // Shape�̃C���X�^���X��maxDataPoints���쐬
        for (int i = 0; i < maxDataPoints; i++)
        {
            GameObject newShape = Instantiate(shapePrefab, graphContainer);
            newShape.transform.localPosition = Vector3.zero;
            shapeInstances.Add(newShape);
        }
    }

    void Update()
    {
        // �f�[�^�̃T���v�����O
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= sampleInterval)
        {
            elapsedTime -= sampleInterval;

            // Z�������̈ړ��ʂƑ��x���v�Z
            float currentZPosition = playerTransform.position.z;
            float deltaZ = currentZPosition - previousZPosition;
            float speed = deltaZ / sampleInterval;

            // �݌v�������X�V
            cumulativeDistance += Mathf.Abs(deltaZ);

            // �f�[�^�����X�g�ɒǉ�
            speedData.Add(speed);

            // ���x�ω����v�Z���A�����f�[�^�Ƃ��ĕێ�
            if (speedData.Count > 1)
            {
                float speedDifference = speed - previousSpeed;
                speedDifferenceData.Add(speedDifference);
            }
            else
            {
                speedDifferenceData.Add(0); // ����͕ω����Ȃ�����0
            }

            // �f�[�^�|�C���g���𐧌�
            if (speedData.Count > maxDataPoints)
            {
                speedData.RemoveAt(0);
                speedDifferenceData.RemoveAt(0); // ���x�ω��f�[�^�����l�ɍ폜
            }

            previousZPosition = currentZPosition;
            previousSpeed = speed;

            // �J������Field of View���X�V
            UpdateCameraFieldOfView();

            // �O���t�̍X�V
           // UpdateGraph();
        }

        // �e�L�X�g�̍X�V
        UpdateTexts();
    }

    // �e�L�X�g�̍X�V
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

    // �J������Field of View�������x�ɉ����Ċ��炩�ɍX�V
    void UpdateCameraFieldOfView()
    {
        if (speedDifferenceData.Count > 0)
        {
            float lastSpeedDifference = speedDifferenceData[speedDifferenceData.Count - 1];
            print(lastSpeedDifference);
            if (lastSpeedDifference >= 0)
            {

                // �������Ă���ꍇ�AFOV�𑝉�
                //currentFieldOfView += fovChangeSpeedUp * Time.deltaTime;
                currentFieldOfView += fovChangeSpeedUp;
            }
            else if (lastSpeedDifference < 0)
            {
                // �������Ă���ꍇ�AFOV������
                //  currentFieldOfView -= fovChangeSpeedDown * Time.deltaTime;
                currentFieldOfView -= fovChangeSpeedDown;
            }

            // FOV�͈̔͂𐧌�
            currentFieldOfView = Mathf.Clamp(currentFieldOfView, minFieldOfView, maxFieldOfView);

            // �J������FOV�ɓK�p
            mainCamera.fieldOfView = currentFieldOfView;
        }
    }

    public float maxDataValue;
    public float minDataValue;

    void UpdateGraph()
    {
        if (speedData.Count == 0)
            return;

        // �O���t�̕\�����[�h�ɂ���Ďg���f�[�^������
        List<float> dataToUse = isDifferenceGraph ? speedDifferenceData : speedData;

        // �f�[�^�̍ő�l�ƍŏ��l���擾
        if (Mathf.Approximately(maxDataValue, minDataValue))
        {
            maxDataValue += 0.1f; // �ق�̏����̍���݂���0���Z��h��
        }

        // Shape�C���X�^���X�̈ʒu�ƃT�C�Y�A�F���X�V
        for (int i = 0; i < dataToUse.Count; i++)
        {
            float value = dataToUse[i];

            // �_�̍������f�[�^�ɔ�Ⴕ�Đݒ�B���̏ꍇ�ɂ͉������֐L�т�悤��
            float normalizedValue = Mathf.Abs((value - minDataValue) / (maxDataValue - minDataValue));

            // �w���I��height���X�P�[�����O
            float exponent = 2.0f; // �w���̒l�B�傫���قǕω������������
            float height = Mathf.Pow(normalizedValue, exponent) * graphHeight;

            if (float.IsNaN(height))
            {
                height = 0; // ������ NaN �ɂȂ����ꍇ�̃t�H�[���o�b�N
            }

            float posX = i * (graphWidth / maxDataPoints);
            float posY = value >= 0 ? height / 2 : -height / 2; // ���������̏ꍇ�͏�����A���̏ꍇ�͉�����

            GameObject shapeInstance = shapeInstances[i];
            RectTransform shapeRect = shapeInstance.GetComponent<RectTransform>();

            // �����ƈʒu���X�V
            shapeRect.sizeDelta = new Vector2(shapeRect.sizeDelta.x, height);
            shapeRect.localPosition = new Vector3(posX, posY, 0);

            // �f�[�^�Ɋ�Â��ĐF��ݒ�i�Ⴂ�l�F�A�����l�F�ԁj
            Color fillColor = GetColorForValue(value, minDataValue, maxDataValue);
            Shape shapeComponent = shapeInstance.GetComponent<Shape>();
            shapeComponent.settings.fillColor = fillColor;

            shapeComponent.settings.dirty = true; // �F�̕ύX�𔽉f���邽�߂�dirty�t���O�𗧂Ă�
        }
    }

    // �l�ɉ������F���擾����֐�
    Color GetColorForValue(float value, float minValue, float maxValue)
    {
        // 0�����E�ɂ��邽�߂�t���v�Z
        float t = Mathf.InverseLerp(minValue, maxValue, Mathf.Abs(value));

        // �X�P�[�����O��ǉ����Ĕ����ω�������
        float scalingFactor = 2.0f; // �l�𒲐����ĐF�ω��̋��x�𑝕�
        t = Mathf.Clamp01(t * scalingFactor); // �X�P�[�����O���t���N�����v

        Color baseColor;

        if (value >= 0)
        {
            // 0�����ɍs���ꍇ�͎������
            baseColor = Color.Lerp(Color.magenta, Color.red, t);
        }
        else
        {
            // 0���牺�ɍs���ꍇ�͎������
            baseColor = Color.Lerp(Color.magenta, Color.blue, t);
        }

        // �A���t�@�l��fillOpacity�œK�p
        baseColor.a = fillOpacity;

        return baseColor;
    }

}
