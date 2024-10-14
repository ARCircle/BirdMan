using UnityEngine;
using System.Collections;

public class WaterSplashRaycast : MonoBehaviour
{
    public GameObject waterSplashEffectPrefab; // ���̃p�[�e�B�N���V�X�e���̃v���n�u
    public GameObject mapParticlePrefab; // Map�̃p�[�e�B�N���V�X�e���̃v���n�u
    public GameObject mapParticleGroundPrefab; // Map�̃p�[�e�B�N���V�X�e���̃v���n�u
    public Transform birdTransform; // ����Transform
    public LayerMask waterLayerMask; // ���̃��C���[�}�X�N
    public LayerMask mapLayerMask; // Map�̃��C���[�}�X�N
    public float raycastDistance = 10.0f; // �������Raycast�̋���
    public float splashInterval = 0.2f; // �p�[�e�B�N���𐶐�����Ԋu
    public GameObject FocusParticleObject;
    ParticleSystem FocusParticle;
    Rigidbody rb;
    public BirdControl birdControl;
    public GameObject InWaterPlane;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        FocusParticle = FocusParticleObject.GetComponent<ParticleSystem>();
  
    }
    bool firstDrop;
    void FixedUpdate()
    {

        if (FocusParticle.isPlaying)
        {
            // パーティクルが再生中なら停止
            FocusParticle.Stop();
        }

        if (birdControl.isSpeedUp)
        {
           
            FocusParticle.Play();
        }
        //if (FocusParticleObject.activeSelf == true)
        //   FocusParticleObject.SetActive(false);
        if (Player.transform.position.y < ground)
        {
//if(firstDrop){
   if (!audioSourceDrop.isPlaying){
audioSourceDrop.Play();
 audioSourceDrop.volume = 0.5f; // マイナスにはならないようにする
   }
   if (audioSourceWind.isPlaying)
audioSourceWind.Stop();
//firstDrop=false;
//}
            if (rb.velocity.z > 0)
                rb.AddForce(rb.transform.forward * speedDownMultiplier);
            if(!InWaterPlane.activeSelf)
                InWaterPlane.SetActive(true);

        }
        else
        {
              if (!audioSourceWind.isPlaying)
            audioSourceWind.Play();
            if (InWaterPlane.activeSelf)
                InWaterPlane.SetActive(false);
 if (audioSourceDrop.volume > 0)
    {
        audioSourceDrop.volume -= Time.deltaTime * 1f; // 徐々に音量を減らす
        
        if (audioSourceDrop.volume < 0.1)
        {
            audioSourceDrop.volume = 0; // マイナスにはならないようにする
            audioSourceDrop.Stop();
        }
    }
              //  if(!firstDrop){

//firstDrop=true;
//}
        }

        // ���̈ʒu���牺������Ray�𓊂���
        Ray ray = new Ray(birdTransform.position + Vector3.up * 0f, Vector3.down);
        RaycastHit hit;
      
        // �����C���[��Ray���q�b�g���������`�F�b�N
        if (Physics.Raycast(ray, out hit, raycastDistance, waterLayerMask))
        {

        
            // �q�b�g�����ʒu�ɐ��̃p�[�e�B�N���𐶐�
            SpawnWaterSplash(hit.point);
            speedUp();
           
            if (!audioSourceWater.isPlaying)
{
    audioSourceWater.PlayOneShot(soundWater);
     audioSourceWater.volume = 0.5f; // 音量を最大に設定
}

        }
        else
{

    // 音量を徐々に減らす処理
    if (audioSourceWater.volume > 0)
    {
        audioSourceWater.volume -= Time.deltaTime * 1f; // 徐々に音量を減らす
        
        if (audioSourceWater.volume < 0.1)
        {
            audioSourceWater.volume = 0; // マイナスにはならないようにする
            audioSourceWater.Stop();
        }
    }
}
        // Map���C���[��Ray���q�b�g���������`�F�b�N
        if (Physics.Raycast(ray, out hit, raycastDistance, mapLayerMask))
        {
            // �q�b�g�����ʒu��Map�̃p�[�e�B�N���𐶐�
            if (hit.point.y > ground)
            {
                SpawnMapParticle(hit.point);
                speedUp();
                  if (!audioSourceGrass.isPlaying)
{
    audioSourceGrass.PlayOneShot(soundGrass);
     audioSourceGrass.volume = 0.5f; // 音量を最大に設定
}

            }
            else
            {
                SpawnMapGroundParticle(hit.point);
            }
        }
        else
{

    // 音量を徐々に減らす処理
    if (audioSourceGrass.volume > 0)
    {
        audioSourceGrass.volume -= Time.deltaTime * 1f; // 徐々に音量を減らす
        
        if (audioSourceGrass.volume < 0.1)
        {
            audioSourceGrass.volume = 0; // マイナスにはならないようにする
            audioSourceGrass.Stop();
        }
    }
}
      

    }
    

    // �����Ԃ��̃p�[�e�B�N���𐶐�
    void SpawnWaterSplash(Vector3 hitPosition)
    {
        for (int i = 0; i < 3; i++)
        {
            Vector3 randomOffset = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
           Vector3 hitToBird= new Vector3(0,0, birdTransform.position.y-hitPosition.y);
            Vector3 spawnPosition = hitPosition + randomOffset+hitToBird ;

            // X����-90�x��]���ăp�[�e�B�N�����q�b�g�ʒu�ɐ���
            GameObject splash = Instantiate(waterSplashEffectPrefab, spawnPosition, Quaternion.Euler(-90, 0, 0));

            // �p�[�e�B�N����0.2�b��ɍ폜
            Destroy(splash, 0.5f); // �p�[�e�B�N���̎����𒲐�
        }
    }

    public float ground = 0f;
    // Map�p�̃p�[�e�B�N���𐶐�
    void SpawnMapParticle(Vector3 hitPosition)
    {
       
          for (int i = 0; i < 3; i++)
            {
                Vector3 randomOffset = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
            Vector3 hitToBird = new Vector3(0, 0, birdTransform.position.y - hitPosition.y);

       Vector3 spawnPosition = hitPosition + randomOffset + hitToBird ;



            // X����-90�x��]���ăp�[�e�B�N�����q�b�g�ʒu�ɐ���
            GameObject mapParticle = Instantiate(mapParticlePrefab, spawnPosition, Quaternion.Euler(-90, 0, 0));
              
                // �p�[�e�B�N����0.5�b��ɍ폜
                Destroy(mapParticle, 0.5f);
            }

       
    }

    void SpawnMapGroundParticle(Vector3 hitPosition)
    {

        for (int i = 0; i < 3; i++)
        {
            Vector3 randomOffset = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
            Vector3 hitToBird = new Vector3(0, 0, birdTransform.position.y - hitPosition.y);

            Vector3 spawnPosition = hitPosition + randomOffset+ hitToBird ;




            GameObject mapParticle = Instantiate(mapParticleGroundPrefab, spawnPosition, Quaternion.Euler(-90, 0, 0));

            // �p�[�e�B�N����0.5�b��ɍ폜
            Destroy(mapParticle, 0.5f);
        }


    }
    public GameObject Player;
    public float forceMultiplierForward;
    public float speedDownMultiplier;

public AudioClip soundWater;

public AudioClip soundGrass;
public AudioClip soundDrop;

public AudioSource audioSourceWater;
public AudioSource audioSourceGrass;
public AudioSource audioSourceDrop;
public AudioSource audioSourceWind;


    public void speedUp()
    {
     rb.AddForce(rb.transform.forward * forceMultiplierForward);

       // if (!FocusParticle.isPlaying)
      //  {
       //     // パーティクルが再生中なら停止
            FocusParticle.Play();
        //}
        // if (FocusParticleObject.activeSelf == false)
        //{
        // FocusParticleObject.SetActive(true);

       // FocusParticle.Stop();  // ��x��~���Ă���Đ�
          //  FocusParticle.Play();
       // }
    }
    public void speedDown()
    {
       
       
    }
}
