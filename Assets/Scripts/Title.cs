using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
public class Title : MonoBehaviour
{
    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            // F�L�[��D�L�[�ō�����Z����]�𐧌�
            if (keyboard.spaceKey.isPressed)
            {
               
                    // "Game"�Ƃ������O�̃V�[���Ɉړ�
                    SceneManager.LoadScene("Game");
                
            }
        }
        // �^�b�`�܂��̓}�E�X�N���b�N�ŃV�[����ύX
        var pointer = Pointer.current;
        if (pointer != null)
        {
            if (pointer.press.wasPressedThisFrame)
            {
                // "Game"�Ƃ������O�̃V�[���Ɉړ�
                SceneManager.LoadScene("Game");
            }
        }
    }
}
