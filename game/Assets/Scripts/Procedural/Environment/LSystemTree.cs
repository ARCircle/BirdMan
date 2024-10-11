using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UIElements;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]



public class LSystemTreeGenerator : MonoBehaviour
{
    public int iterations = 1; // L-System�̔�����
    public float angleX = 25.0f; // ��]�p�x
    public float angleZ = 25.0f; // ��]�p�x
    public float segmentLength = 1.0f; // �Z�O�����g�̒���
    public float segmentRadius = 0.1f; // �Z�O�����g�̔��a
    public int segmentsPerCurve = 10; // �e�Z�O�����g������̍ו�����
    public int vertexCountPerRing = 8; // �~����̒��_��

    private LSystem lSystem;
    List<(Vector3, int)> controlPoints = new List<(Vector3, int)>{};
    private List<float> radii = new List<float>();
    private int branchCount = 1; // ����̃J�E���g���J�n
    public string F = "FF+[+F-F-F+L]&[-F+F+F+L]^[-F+F+F+L]"; // ���Ǝ}�̐���
    public string R = "+[+R-R-R]&[-R+R+R]^[-R+R+R]";



    [Serializable]
    public class LeafParameter
    {

        public Vector2 piece = new Vector2(1, 2);

        public Vector2 posX = new Vector2(-0.5f, 0.5f);
        public Vector2 posY = new Vector2(-0.5f, 0.5f);
        public Vector2 posZ = new Vector2(-0.5f, 0.5f);
        public Vector2 rotX = new Vector2(-90f, 90f);
        public Vector2 rotY = new Vector2(0f, 360f);
        public Vector2 rotZ = new Vector2(0f, 90f);
        public Vector2 scale = new Vector2(0.5f, 1.5f);

    }

    [Serializable]
    public class FruitParameter
    {

        public Vector2 piece = new Vector2(1, 2);

        public Vector2 posX = new Vector2(-0.5f, 0.5f);
        public Vector2 posY = new Vector2(-0.5f, 0.5f);
        public Vector2 posZ = new Vector2(-0.5f, 0.5f);
        public Vector2 rotX = new Vector2(-90f, 90f);
        public Vector2 rotY = new Vector2(0f, 360f);
        public Vector2 rotZ = new Vector2(0f, 90f);
        public Vector2 scale = new Vector2(0.5f, 1.5f);

    }

    public LeafParameter leafP;
    public FruitParameter fruitP;
    void Start()
    {
        // L-System�̃��[�����`
        // L-System�̃��[�����`
        Dictionary<char, string> rules = new Dictionary<char, string>
    {
        { 'F',F }, // ���Ǝ}�̐���
        
        { 'R',R } // ���̐����p�^�[����ǉ�
    };

        lSystem = new LSystem("RF", rules, iterations);

        string lSystemString = lSystem.Generate();
        // ���b�V������
        GenerateTreeMesh(lSystemString);

      
    }
    Vector3 leafPoint1;
    void GenerateTreeMesh(string lSystemString)
    {
        Stack<Matrix4x4> transformStack = new Stack<Matrix4x4>();
        Matrix4x4 currentTransform = Matrix4x4.identity;
        int i = 1;
        char lastChar= 'R';
        foreach (char c in lSystemString)
        {
            float angleX2 = UnityEngine.Random.Range(-angleX, angleX); // X����]�p�x�������_���ɐݒ�
            float angleZ2 = UnityEngine.Random.Range(-angleZ, angleZ); // X����]�p�x�������_���ɐݒ�

            if (c == 'R') // ���̏������ŏ��ɍs��
            {
                Vector3 startPoint = currentTransform.MultiplyPoint(Vector3.zero);
                //print("startPoint"+startPoint);
                // ���̐���_��ǉ�
                if (controlPoints.Count == 0 || controlPoints[controlPoints.Count - 1].Item1 != startPoint)
                {
                    controlPoints.Add((startPoint, -1)); // ���������^�C�v���u-1�v�ɐݒ�
                    radii.Add(segmentRadius * Mathf.Pow(0.9f, i));
                    i++;
                }
                lastChar = c;
                //currentTransform *= Matrix4x4.Translate(Vector3.up * segmentLength);
                // �������ɐi�ނ��߂�Y���̕������Ɉړ�
                currentTransform *= Matrix4x4.Translate(Vector3.down * segmentLength);
            }
            else if (c == 'F' && lastChar == 'R') // ���̏�Ɋ��𐶐�() // ���̏�Ɋ��𐶐�
            {
                currentTransform = Matrix4x4.identity; // ���̐����O�Ƀg�����X�t�H�[�������Z�b�g
                Vector3 startPoint = currentTransform.MultiplyPoint(Vector3.zero);
                i = 1;
                // ���̐���_��ǉ�
                if (controlPoints.Count == 0 || controlPoints[controlPoints.Count - 1].Item1 != startPoint)
                {
                    controlPoints.Add((startPoint, 0));
                    radii.Add(segmentRadius * Mathf.Pow(0.9f, i));
                    i++;
                }
                lastChar = c;
                currentTransform *= Matrix4x4.Translate(Vector3.up * segmentLength);
            }
            else if (c == 'F' ) // ���̏�Ɋ��𐶐�(c == 'F' && lastChar == 'R') // ���̏�Ɋ��𐶐�
            {
                Vector3 startPoint = currentTransform.MultiplyPoint(Vector3.zero);

                // ���̐���_��ǉ�
                if (controlPoints.Count == 0 || controlPoints[controlPoints.Count - 1].Item1 != startPoint)
                {
                    controlPoints.Add((startPoint, 0));
                    radii.Add(segmentRadius * Mathf.Pow(0.9f, i));
                    i++;
                }
                lastChar = c;
                currentTransform *= Matrix4x4.Translate(Vector3.up * segmentLength);
            }
            else if (c == 'B') // �}�̏�����ǉ�
            {
                // �}�̃Z�O�����g�̏���
                Vector3 startPoint = currentTransform.MultiplyPoint(Vector3.zero);

                // �}�̐���_��ǉ�
                if (controlPoints.Count == 0 || controlPoints[controlPoints.Count - 1].Item1 != startPoint)
                {
                    controlPoints.Add((startPoint, 3)); // �}�������^�C�v���u3�v�ɐݒ�
                    radii.Add(segmentRadius * Mathf.Pow(0.7f, i)); // �}�̑����͊����ׂ�
                    i++;
                }

                currentTransform *= Matrix4x4.Translate(Vector3.up * segmentLength * 0.5f); // �}�̒����𒲐��i�����Z������j
            }
            else if (c == 'L') // �t�̐���������ǉ�
            {
                Vector3 leafPosition = currentTransform.MultiplyPoint(Vector3.zero);
                
                GenerateLeaves(leafPosition);
            }
            else if (c == 'O') // �����S�̐���������ǉ�
            {
                Vector3 currentPoint = currentTransform.MultiplyPoint(Vector3.zero);
                // �O��̓_�i�������݂���Ȃ�j
                if (controlPoints.Count > 0)
                {
                    // �Ō�̐���_���擾
                    Vector3 lastPoint = controlPoints[controlPoints.Count - 1].Item1;

                    // ����������߂�i��F1:2 �̔䗦�œ�������ꍇ�j
                    float ratio = 0f; // 1:2 �̔䗦���g�p���Ă���Ɖ���

                    // �����_�̌v�Z
                    Vector3 fruitPosition = Vector3.Lerp(lastPoint, currentPoint, ratio);

                    // �����_���g���ă����S��z�u
                    GenerateFruits(fruitPosition);
                }
                else
                {
                    // �ŏ��̃����S�̈ʒu�͌��݂̕ϊ��s��̈ʒu���g�p
                    GenerateFruits(currentPoint);
                }
            }
            else if (c == '+')
            {
                currentTransform *= Matrix4x4.Rotate(Quaternion.Euler(0, 0, angleX2));
            }
            else if (c == '-')
            {
                currentTransform *= Matrix4x4.Rotate(Quaternion.Euler(0, 0, -angleX2));
            }
            else if (c == '&')
            {
                currentTransform *= Matrix4x4.Rotate(Quaternion.Euler(angleZ2, 0, 0)); // y-z���ʂł̉�]�i�������j
            }
            else if (c == '^')
            {
                currentTransform *= Matrix4x4.Rotate(Quaternion.Euler(-angleZ2, 0, 0)); // y-z���ʂł̉�]�i������j
            }
            else if (c == '[')
            {
              // print("lastChar" + lastChar );
                leafPoint1 = currentTransform.MultiplyPoint(Vector3.zero);
                if(lastChar=='R')
                    controlPoints.Add((currentTransform.MultiplyPoint(Vector3.zero), -1));
                else
                controlPoints.Add((currentTransform.MultiplyPoint(Vector3.zero), 2));
                radii.Add(segmentRadius* Mathf.Pow(0.9f, i));
                transformStack.Push(currentTransform);
             
            }
            else if (c == ']')
            {



                Vector3 endPoint = currentTransform.MultiplyPoint(Vector3.zero);
//if(lastChar != 'R')
               // GenerateLeaves((4*endPoint+leafPoint1)/5);
                //GenerateLeaves((4*endPoint+leafPoint1)/5);
                currentTransform = transformStack.Pop();

                i--;  // ���򂩂�߂�Ƃ��̓C���f�b�N�X������������i�}�������Ȃ�j
            }
        }

        // ���b�V���̐����ƃA�T�C��
        GenerateMeshAndAssign();
    }
    public GameObject leafPrefab;
    // public Vector2 leafP.rotX = new Vector2(0f,90f); // �t�̉�]�͈�

    void GenerateLeaves(Vector3 endPoint)
    {
        if (leafPrefab != null)
        {
            float randomPiece = Mathf.FloorToInt(UnityEngine.Random.Range(leafP.piece.x, leafP.piece.y));

            // X�̐��������̃��[�v
            for (int i = 0; i < randomPiece; i++)
            {
              
          

           
            // �����_���ȃI�t�Z�b�g��ݒ�
            Vector3 randomOffset = new Vector3(
                UnityEngine.Random.Range(leafP.posX.x, leafP.posX.y),
                UnityEngine.Random.Range(leafP.posY.x, leafP.posY.y),
                UnityEngine.Random.Range(leafP.posZ.x, leafP.posZ.y)
            );

                // �����_���ȉ�]��ݒ�
                Quaternion randomRotation = Quaternion.Euler(
                    UnityEngine.Random.Range(leafP.rotX.x, leafP.rotX.y), // X���̉�]�͈�
                    UnityEngine.Random.Range(leafP.rotY.x, leafP.rotY.y), // Y���̉�]�͈�
                    UnityEngine.Random.Range(leafP.rotZ.x, leafP.rotZ.y)  // Z���̉�]�͈�
                );
                // �����_���ȃX�P�[����ݒ�
                float randomScale = UnityEngine.Random.Range(leafP.scale.x, leafP.scale.y);


                // �t��z�u
                //   GameObject leaf = Instantiate(leafPrefab, endPoint+transform.position + randomOffset, Quaternion.identity, transform);
                // �t��z�u
                GameObject leaf = Instantiate(leafPrefab, endPoint + transform.position + randomOffset, randomRotation, transform);

               // GenerateFruits(endPoint,leaf);
            // �X�P�[����K�p
            leaf.transform.localScale = leaf.transform.localScale*randomScale;

            }
        }
    }
    public GameObject fruitPrefab; // �����S�p�̃v���n�u���w�肷�邽�߂�public�t�B�[���h
    public Camera mainCamera; // �g�p����J�������w��iOrthographic�J�����j
    void GenerateFruits(Vector3 fruitPosition)
    {
        if (fruitPrefab != null)
        {
            // �����_���ȃX�P�[����ݒ�
            float randomScale = UnityEngine.Random.Range(leafP.scale.x, fruitP.scale.y);
            // �����_���ȉ�]��ݒ�
            Quaternion randomRotation = Quaternion.Euler(
                UnityEngine.Random.Range(fruitP.rotX.x, fruitP.rotX.y), // X���̉�]�͈�
                UnityEngine.Random.Range(fruitP.rotY.x, fruitP.rotY.y), // Y���̉�]�͈�
                UnityEngine.Random.Range(fruitP.rotZ.x, fruitP.rotZ.y)  // Z���̉�]�͈�
            );

            // �����S��z�u
            GameObject fruit = Instantiate(fruitPrefab, fruitPosition + transform.position, randomRotation, transform);

            // �X�P�[����K�p
            fruit.transform.localScale = fruit.transform.localScale * randomScale;
        }
    }

    /* void GenerateFruits(Vector3 endPoint, GameObject leaf)
     {
         if (leafPrefab != null && fruitPrefab != null)
         {
             Mesh leafMesh = leaf.GetComponent<MeshFilter>().sharedMesh; // leafPrefab�̃��b�V�����擾
             if(leafMesh == null)
                 leafMesh = leaf.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh; // leafPrefab�̃��b�V�����擾

             Vector3[] vertices = leafMesh.vertices; // ���b�V���̒��_���擾
             Vector3[] normals = leafMesh.normals; // ���b�V���̖@�����擾

             int randomPiece = Mathf.FloorToInt(UnityEngine.Random.Range(leafPiece.x, leafPiece.y));

             for (int i = 0; i < randomPiece; i++)
             {
                 // �����_���ȃX�P�[����ݒ�
                 float randomScale = UnityEngine.Random.Range(leafScaleRange.x, leafScaleRange.y);

                 // ���_�������_���ɑI��
                 int randomIndex = UnityEngine.Random.Range(0, vertices.Length);
                 Vector3 randomVertex = vertices[randomIndex];
                 Vector3 normal = normals[randomIndex];

                 // �@����Y�����ɕ��ł��邩���m�F
                 if (normal.y < 0.5f)
                 {
                     // ���[���h���W�n�ɕϊ�
                     Vector3 leafPosition = leaf.transform.TransformPoint(randomVertex);

                     // �����_���ȃI�t�Z�b�g��ݒ�
                     Vector3 randomOffset = new Vector3(
                         UnityEngine.Random.Range(-leafPositionOffsetRange.x, leafPositionOffsetRange.x),
                         UnityEngine.Random.Range(-leafPositionOffsetRange.y, leafPositionOffsetRange.y),
                         UnityEngine.Random.Range(-leafPositionOffsetRange.z, leafPositionOffsetRange.z)
                     );

                     // �J��������I�u�W�F�N�g�܂ł̃��C�𐶐�
                     // print("leafPosition" + leafPosition);
                     //Vector3 leafPositionOffset = new Vector3(0, 0.01f, 0);
                    // leafPosition = leafPosition + normal/8f;
                     Vector3 leafPositionOffset = normal*10;
                     Vector3 directionToLeaf = leafPosition - mainCamera.transform.position;
                     //directionToLeaf -= directionToLeaf.normalized/100;
                     // ���C�L���X�g���s���A�J������fruitPrefab�̊Ԃɑ��̃I�u�W�F�N�g�����邩�m�F
                     if (Physics.Raycast(mainCamera.transform.position, directionToLeaf, out RaycastHit hitInfo, directionToLeaf.magnitude))
                     {
                         // �J��������̃��C���Ղ��Ă��Ȃ��ꍇ�A�����S��z�u
                        //print("hitInfo.transform.name" +hitInfo.transform.name);

                     }
                     else
                     {
                         // �J�����̐��ʕ������擾
                         Vector3 cameraForward = mainCamera.transform.forward;

                         // �@���ƃJ�������ʂ̓��ς��v�Z
                         float dotProduct = Vector3.Dot(normal, cameraForward);
                        // print(dotProduct);
                         // ���ς����̏ꍇ�A�I�u�W�F�N�g�̓J�����̕����������Ă���
                         if (dotProduct < 0)
                         {
                             // �@���̋t�����Ɍ����ĉ�]��ݒ�
                             Quaternion rotation = Quaternion.LookRotation(mainCamera.transform.forward,-normal);
                             //Quaternion rotation = Quaternion.LookRotation(mainCamera.transform.forward);//billboard���ɂȂ�


                             // �����S��z�u
                            // GameObject fruit = Instantiate(fruitPrefab, leafPosition, rotation, transform);
                             GameObject fruit = Instantiate(fruitPrefab, leafPosition, Quaternion.identity, transform);
                             // �����S�p�̃}�e���A�����擾���ARender Queue��ݒ肷��


                             // �X�P�[����K�p
                            // fruit.transform.localScale = Vector3.one * randomScale;
                         }
                     }
                 }
                 else
                 {
                   //  i--; // �@�������łȂ��ꍇ�A�J�E���^�[�����炵�čĎ��s
                 }
             }
         }
     }*/



    public void GenerateMeshAndAssign()
    {
        Mesh mesh = GenerateMesh();
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    public Mesh GenerateMesh()
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        if (controlPoints.Count < 2 || radii.Count < 2)
        {
            Debug.LogWarning("Not enough control points or radii to generate mesh.");
            return mesh;
        }

        int totalSegments = (controlPoints.Count - 1) * segmentsPerCurve;
        // �����̒��_�����݂��邩�m�F
        int existingVertexIndex = -1;
        for (int i = 0; i < controlPoints.Count - 1; i++)
        {

            //print("i"+i+"controlPoints[i].Item1_" + controlPoints[i].Item1 + "controlPoints[i].Item2_" + controlPoints[i].Item2);
        }
            for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            float Item2 = controlPoints[i].Item2;
            Vector3 p0 = controlPoints[i].Item1;
            Vector3 p1 = controlPoints[i + 1].Item1;
            //print("controlPoints[i].Item1_" + controlPoints[i].Item1+"controlPoints[i].Item2_" + controlPoints[i].Item2);
            existingVertexIndex = -1;
          //  if (controlPoints[i+1].Item2 >= 2)
           // {
               //  print( "i" + controlPoints[i+1].Item1);

                for (int m = 0; m < i+1; m++)
                {
                    //print("m"+controlPoints[m].Item1+"i"+ controlPoints[i+1].Item1);
                    

                    if (controlPoints[m].Item1 == controlPoints[i+1].Item1)
                    {
                       //  print("-----------m" + controlPoints[m].Item1 + "i" + controlPoints[i+1].Item1);

                        existingVertexIndex = i+1-m;
                       // print(existingVertexIndex);
                        
                    }
                }
            //}

          
              
            float r0 = radii[i];
            float r1 = radii[i + 1];

            for (int j = 0; j <= segmentsPerCurve; j++)
            {
                if (existingVertexIndex != -1)
                {
                    break;
                }
                float t = (float)j / segmentsPerCurve;
                Vector3 position = Vector3.Lerp(p0, p1, t);
                float radius = Mathf.Lerp(r0, r1, t);

                for (int k = 0; k < vertexCountPerRing; k++)
                {
                    float angle = k * Mathf.PI * 2 / (vertexCountPerRing - 1);
                    Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                    Vector3 newPosition = position + offset;


                    /* for (int m = 0; m < controlPoints.Count; m++)
                     {
                         if (controlPoints[m].Item1 == newPosition)
                         {
                             existingVertexIndex = m;
                             break;
                         }
                     }*/

                    // if (existingVertexIndex == -1)
                    // {
                   
                        vertices.Add(newPosition);
                    

                        if (j > 0 && k > 0)
                        {
                            int current = vertices.Count - 1;
                            int previous = current - 1;
                            int below = current - vertexCountPerRing;
                            int belowPrevious = previous - vertexCountPerRing;

                        if (existingVertexIndex  != -1)
                        {
                            // below = current - vertexCountPerRing*(existingVertexIndex-1);
                            // belowPrevious = previous - vertexCountPerRing * (existingVertexIndex - 1);
                            
                        }
                      
                       if (existingVertexIndex == -1)
                        {
                            if (Item2 == -1)
                            {
                                triangles.Add(below);
                               
                                triangles.Add(current);
                                triangles.Add(previous);

                                triangles.Add(belowPrevious);
                               
                                triangles.Add(below);
                                triangles.Add(previous);
                            }
                            else
                            {
                                triangles.Add(below);
                                triangles.Add(previous);
                                triangles.Add(current);

                                triangles.Add(belowPrevious);
                                triangles.Add(previous);
                                triangles.Add(below);
                            }
                        }
                    }
                  //  }
                  //  else
                  //  {
                        // �����̒��_�𗘗p
                      //  newPosition = controlPoints[existingVertexIndex].Item1;
                  //  }
                
                }

            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}

public class LSystem
{
    public string axiom;
    private Dictionary<char, string> rules;
    public int iterations;

    public LSystem(string axiom, Dictionary<char, string> rules, int iterations)
    {
        this.axiom = axiom;
        this.rules = rules;
        this.iterations = iterations;
    }

    public string Generate()
    {
        string currentString = axiom;

        for (int i = 0; i < iterations; i++)
        {
            string nextString = "";

            foreach (char c in currentString)
            {
                if (rules.ContainsKey(c))
                {
                    nextString += rules[c];
                }
                else
                {
                    nextString += c;
                }
            }

            currentString = nextString;
        }

        return currentString;
    }
}
