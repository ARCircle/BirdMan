using UnityEngine;
using UnityEngine.InputSystem;

public class AutoMouseControl : MonoBehaviour
{
    public BirdControl birdControl; // BirdControl�X�N���v�g�ւ̎Q��
    public float minFrequencyX = 0.5f; // �U���̍ŏ����g���iX���j
    public float maxFrequencyX = 2.0f; // �U���̍ő���g���iX���j
    public float minFrequencyY = 0.5f; // �U���̍ŏ����g���iY���j
    public float maxFrequencyY = 2.0f; // �U���̍ő���g���iY���j
    public float amplitudeX = 100.0f; // �U���̐U���iX���j
    public float amplitudeY = 100.0f; // �U���̐U���iY���j

    private float currentFrequencyX; // ���݂̐U���̎��g���iX���j
    private float currentFrequencyY; // ���݂̐U���̎��g���iY���j
    private float timeCounterX = 0f; // ���Ԃ̃J�E���^�[�iX���j
    private float timeCounterY = 0f; // ���Ԃ̃J�E���^�[�iY���j
    public Vector2 simulatedMousePosition; // ���z�}�E�X�̈ʒu
    private Vector2 initialClickPosition; // ��ʂ̒��S�ʒu

    // �ʑ��`�F�b�N�p�t���O
    private bool hasReachedPiOverTwoX = false;
    private bool hasReachedPiOverTwoY = false;
    private float phaseTolerance = 0.1f; // �ʑ��̋��e�͈�

    // Sinusoidal�����L���ɂ���t���O
    public bool isSinusoidalControlEnabled = false;

    // ���g�������̒l�ɂȂ肷����̂�h�����߂̃t���O�ƃJ�E���^�[
    private int negativeFrequencyCounter = 0;
    public int maxNegativeFrequencyCount = 3; // ���̎��g���������ő��

    void Start()
    {
        // �����̎��g���������_���ɐݒ�
        currentFrequencyX = Random.Range(minFrequencyX, maxFrequencyX);
        currentFrequencyY = Random.Range(minFrequencyY, maxFrequencyY);

        // �f�B�X�v���C�̒��S�_�������N���b�N�ʒu�Ƃ��Đݒ�
        initialClickPosition = new Vector2(Screen.width / 2, Screen.height / 2);
    }

    void Update()
    {
        if (isSinusoidalControlEnabled)
        {
            // Y���̐U���v�Z
            timeCounterY += Time.deltaTime * currentFrequencyY;
            float oscillationY = Mathf.Sin(timeCounterY) * amplitudeY;

            // X���̐U���v�Z
            timeCounterX += Time.deltaTime * currentFrequencyX;
            float oscillationX = Mathf.Sin(timeCounterX) * amplitudeX;

            // ���z�I�Ƀ}�E�X�̈ʒu�𒆐S�_����ɐ��䂷��
            simulatedMousePosition = new Vector2(initialClickPosition.x + oscillationX, initialClickPosition.y + oscillationY);

            // Y���̈ʑ�����/2�ɓ��B�������ǂ������m�F
            float phaseY = timeCounterY % (2 * Mathf.PI);
            if (Mathf.Abs(phaseY - Mathf.PI / 2) < phaseTolerance && !hasReachedPiOverTwoY)
            {
                // �ʑ�����/2�ɋ߂Â����̂ŁAY���̎��g���������_���ɕύX
                currentFrequencyY = Random.Range(minFrequencyY, maxFrequencyY);
                hasReachedPiOverTwoY = true; // �t���O�𗧂Ă�
            }
            else if (phaseY < Mathf.PI / 2)
            {
                // �ʑ����Ă�0�ɖ߂�܂Ńt���O�����Z�b�g
                hasReachedPiOverTwoY = false;
            }

            // X���̈ʑ����}��/2�ɓ��B�������ǂ������m�F
            float phaseX = timeCounterX % (2 * Mathf.PI);
            if ((Mathf.Abs(phaseX - Mathf.PI / 2) < phaseTolerance || Mathf.Abs(phaseX + Mathf.PI / 2) < phaseTolerance) && !hasReachedPiOverTwoX)
            {
                // �������̎��g���������������ꍇ�͐��̒l�ɕύX
                if (negativeFrequencyCounter >= maxNegativeFrequencyCount)
                {
                    currentFrequencyX = Random.Range(minFrequencyX, maxFrequencyX);
                    negativeFrequencyCounter = 0; // �J�E���^�[�����Z�b�g
                }
                else
                {
                    // �ʑ����}��/2�ɋ߂Â����̂ŁAX���̎��g���������_���ɕύX
                    currentFrequencyX = Random.Range(minFrequencyX, maxFrequencyX);
                }

                // ���g�������ł��邩�ǂ������m�F���A�J�E���^�[�𑝂₷
                if (currentFrequencyX < 0)
                {
                    negativeFrequencyCounter++;
                }
                else
                {
                    negativeFrequencyCounter = 0; // ���̎��g�����o����J�E���^�[�����Z�b�g
                }

                hasReachedPiOverTwoX = true; // �t���O�𗧂Ă�
            }
            else if (phaseX < Mathf.PI / 2 && phaseX > -Mathf.PI / 2)
            {
                // �ʑ����Ă�0�ɖ߂�܂Ńt���O�����Z�b�g
                hasReachedPiOverTwoX = false;
            }

            // BirdControl�ɉ��z�}�E�X�̈ʒu��ݒ�
            // birdControl.SetSimulatedMousePosition(simulatedMousePosition);
        }
    }
}
