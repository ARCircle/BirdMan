using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SpeedGraphController : MonoBehaviour
{
    [Header("Player Settings")]
    public Transform playerTransform; // �v���C���[��Transform

    [Header("Sampling Settings")]
    public float sampleInterval = 0.1f; // �T���v�����O�Ԋu
    public int maxDataPoints = 100; // �O���t�ɕ\������ő�f�[�^�|�C���g��

    [Header("UI Settings")]
    public RawImage graphImage;
    public Text distanceText;
    public Text speedText;

    [Header("Graph Settings")]
    public int graphWidth = 300;
    public int graphHeight = 150;
    public Color graphLineColor = Color.green;
    public Color graphFillColor = new Color(0f, 1f, 0f, 0.3f); // �������̗�

    private List<float> speedData = new List<float>(); // ���x�f�[�^
    private float cumulativeDistance = 0f; // �݌v����

    private float elapsedTime = 0f;
    private float previousZPosition = 0f;

    private Texture2D graphTexture;

    void Start()
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player Transform���ݒ肳��Ă��܂���B");
            enabled = false;
            return;
        }

        previousZPosition = playerTransform.position.z;

        // �O���t�p�̃e�N�X�`�����쐬
        graphTexture = new Texture2D(graphWidth, graphHeight);
        graphTexture.wrapMode = TextureWrapMode.Clamp;
        graphTexture.filterMode = FilterMode.Bilinear;

        if (graphImage != null)
        {
            graphImage.texture = graphTexture;
        }
        else
        {
            Debug.LogError("Graph Image���ݒ肳��Ă��܂���B");
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

            // �f�[�^�|�C���g���𐧌�
            if (speedData.Count > maxDataPoints)
            {
                speedData.RemoveAt(0);
            }

            previousZPosition = currentZPosition;
        }

        // �O���t�ƃe�L�X�g�̍X�V
        UpdateGraph();
        UpdateTexts();
    }

    void UpdateTexts()
    {
        if (distanceText != null)
        {
            distanceText.text = $"{cumulativeDistance:F0} m";
        }

        if (speedText != null && speedData.Count > 0)
        {
            float currentSpeed = speedData[speedData.Count - 1];
            speedText.text = $" {currentSpeed:F0} m/s";
        }
    }
    void UpdateGraph()
    {
        if (graphTexture == null || speedData.Count < 2)
            return;

        // ���x�f�[�^����ő呬�x�ƍŏ����x���擾
        float maxSpeed, minSpeed;
        GetSpeedRange(out maxSpeed, out minSpeed);

        // �㉺��10%�̗]�T����������
        float padding = (maxSpeed - minSpeed) * 0.1f;
        maxSpeed += padding;
        minSpeed -= padding;

        float speedRange = Mathf.Max(maxSpeed - minSpeed, 0.1f); // 0.1�Ŋ���Z�����

        // �N���A������ύX�F�O���t����x�ɃN���A
        ClearTextureFast();

        Color[] pixels = graphTexture.GetPixels();

        // �O���t�̕`��
        for (int i = 0; i < speedData.Count - 1; i++)
        {
            float speed = speedData[i];
            float nextSpeed = speedData[i + 1];

            // ���W�v�Z�i�ŏ����x����ɁA�c�����̃X�P�[���𒲐��j
            float x0 = (float)i / (speedData.Count - 1) * graphWidth;
            float y0 = ((speed - minSpeed) / speedRange) * graphHeight;

            float x1 = (float)(i + 1) / (speedData.Count - 1) * graphWidth;
            float y1 = ((nextSpeed - minSpeed) / speedRange) * graphHeight;

            // ����`��
            DrawLineFast(pixels, (int)x0, (int)y0, (int)x1, (int)y1, graphLineColor);

            // �h��Ԃ�
            FillAreaUnderLineFast(pixels, (int)x0, (int)y0, (int)x1, (int)y1, graphFillColor);
        }

        // �Ō�Ɉ�x�����e�N�X�`�����X�V
        graphTexture.SetPixels(pixels);
        graphTexture.Apply();
    }

    // �ő呬�x�ƍŏ����x���擾����֐�
    void GetSpeedRange(out float maxSpeed, out float minSpeed)
    {
        maxSpeed = float.MinValue;
        // minSpeed = float.MaxValue;
        minSpeed = 0;

        foreach (float speed in speedData)
        {
            if (speed > maxSpeed)
            {
                maxSpeed = speed;
            }
            if (speed < minSpeed)
            {
                //minSpeed = speed;
                minSpeed = 0;
            }
        }
    }



    // �s�N�Z�����܂Ƃ߂ăN���A
    void ClearTextureFast()
    {
        Color clearColor = new Color(0, 0, 0, 0); // ����
        Color[] clearPixels = new Color[graphWidth * graphHeight];
        for (int i = 0; i < clearPixels.Length; i++)
        {
            clearPixels[i] = clearColor;
        }
        graphTexture.SetPixels(clearPixels);
    }

    // �s�N�Z�����܂Ƃ߂ĕ`��
    void DrawLineFast(Color[] pixels, int x0, int y0, int x1, int y1, Color color)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (IsInsideTexture(x0, y0))
            {
                pixels[y0 * graphWidth + x0] = color;
            }

            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    // �h��Ԃ��������œK��
    void FillAreaUnderLineFast(Color[] pixels, int x0, int y0, int x1, int y1, Color color)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            // X���܂œh��Ԃ�
            for (int fillY = 0; fillY <= y0; fillY++)
            {
                if (IsInsideTexture(x0, fillY))
                {
                    pixels[fillY * graphWidth + x0] = color;
                }
            }

            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    float GetMaxSpeed()
    {
        float maxSpeed = 0f;
        foreach (float speed in speedData)
        {
            if (Mathf.Abs(speed) > maxSpeed)
            {
                maxSpeed = Mathf.Abs(speed);
            }
        }
        return maxSpeed;
    }

    bool IsInsideTexture(int x, int y)
    {
        return x >= 0 && x < graphWidth && y >= 0 && y < graphHeight;
    }
}
