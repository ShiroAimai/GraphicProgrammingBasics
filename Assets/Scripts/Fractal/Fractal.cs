using System;
using Unity.Mathematics;
using UnityEngine;

namespace Fractal
{
    public class Fractal : MonoBehaviour
    {
        static readonly int matricesId = Shader.PropertyToID("_Matrices");
        private static MaterialPropertyBlock _propertyBlock;
        struct FractalPart
        {
            public Vector3 dir, worldPos;
            public Quaternion rot, worldRot;
            public float spinAngle;
        }
        
        [SerializeField, Range(1, 8)] private int depth = 4;
        
        [SerializeField] private Mesh mesh;

        [SerializeField] private Material material;

        private FractalPart[][] parts;
        private Matrix4x4[][] matrices;

        private ComputeBuffer[] matricesBuffers;
        
        private static Vector3[] dirs =
        {
            Vector3.up, Vector3.right, Vector3.left, Vector3.forward, Vector3.back
        };

        private static Quaternion[] rots =
        {
            Quaternion.identity, 
            Quaternion.Euler(0f,0f, -90f), 
            Quaternion.Euler(0f,0f,90f), 
            Quaternion.Euler(90,0f,0f),
            Quaternion.Euler(-90f, 0f, 0f), 
        };

        private void OnEnable()
        {
            parts = new FractalPart[depth][];
            matrices = new Matrix4x4[depth][];
            matricesBuffers = new ComputeBuffer[depth];

            int stride = 16 * sizeof(float);
            for (int i = 0, length = 1; i < parts.Length; ++i, length *= dirs.Length)
            {
                parts[i] = new FractalPart[length];
                matrices[i] = new Matrix4x4[length];
                matricesBuffers[i] = new ComputeBuffer(length, stride);
            }
            
            parts[0][0] = CreatePart(0);
            for (int i = 1; i < parts.Length; ++i)
            {
                FractalPart[] levelParts = parts[i];
                for (int j = 0; j < levelParts.Length; j += 5)
                {
                    for (int w = 0; w < dirs.Length; ++w)
                    {
                        levelParts[j + w] = CreatePart(w);
                    }
                }
            }

            _propertyBlock ??= new MaterialPropertyBlock();
        }

        private void OnValidate()
        {
            if (parts != null && enabled)
            {
                OnDisable();
                OnEnable();
            }
        }

        private void OnDisable()
        {
            foreach (var c in matricesBuffers)
            {
                c.Release();
            }

            parts = null;
            matrices = null;
            matricesBuffers = null;
        }

        private void Update()
        {
            float spinAngleDelta = 2.5f * Time.deltaTime;

            FractalPart root = parts[0][0];
            root.spinAngle += spinAngleDelta;
            root.worldRot = transform.rotation * 
                (root.rot * Quaternion.Euler(0f, root.spinAngle, 0f));
            root.worldPos = transform.position;
            parts[0][0] = root;
            float objectScale = transform.lossyScale.x;
            matrices[0][0] = Matrix4x4.TRS(root.worldPos, root.worldRot, objectScale * Vector3.one);

            float scale = objectScale;
            for (int li = 1; li < parts.Length; li++)
            {
                scale *= 0.5f;
                FractalPart[] parentParts = parts[li - 1];
                FractalPart[] levelParts = parts[li];
                Matrix4x4[] levelMatrix = matrices[li];
                for (int fpi = 0; fpi < levelParts.Length; fpi++)
                {
                    FractalPart parent = parentParts[fpi / dirs.Length];
                    FractalPart part = levelParts[fpi];
                    part.spinAngle += spinAngleDelta;
                    part.worldRot = parent.worldRot * (part.rot * Quaternion.Euler(0f, spinAngleDelta, 0f));
                    part.worldPos =
                        parent.worldPos +
                        parent.worldRot *
                        (1.5f * scale * part.dir);

                    levelParts[fpi] = part;
                    levelMatrix[fpi] = Matrix4x4.TRS(part.worldPos, part.worldRot, scale * Vector3.one);
                }
            }

            var bounds = new Bounds(root.worldPos, 3f * objectScale * Vector3.one);
            for (int i = 0; i < matricesBuffers.Length; ++i)
            {
                ComputeBuffer buffer = matricesBuffers[i];
                buffer.SetData(matrices[i]);
                _propertyBlock.SetBuffer(matricesId, buffer);
                Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, buffer.count, _propertyBlock);
            }
        }

        private FractalPart CreatePart(int childIndex)
        {
            return new FractalPart
            {
                dir =  dirs[childIndex],
                rot = rots[childIndex]
            };
        }
        
    }
   
}
