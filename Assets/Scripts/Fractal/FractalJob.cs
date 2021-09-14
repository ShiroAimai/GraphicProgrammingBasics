using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;

using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;
using Random = UnityEngine.Random;

namespace Fractal
{
    public class FractalJob : MonoBehaviour
    {
        private static readonly int
            colorBId = Shader.PropertyToID("_ColorA"),
            colorAId = Shader.PropertyToID("_ColorB"),
            matricesId = Shader.PropertyToID("_Matrices"),
            sequenceNumbersId = Shader.PropertyToID("_SequenceNumbers")
            ;

        private Vector4[] sequenceNumbers;
        private static MaterialPropertyBlock _propertyBlock;

        [SerializeField] private Gradient _gradientA,_gradientB;
        [SerializeField] private Color leafColorA,leafColorB;
        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        struct UpdateFractalLevelJob : IJobFor
        {
            public float spinAngleDelta;
            public float scale;

            [ReadOnly]
            public NativeArray<FractalPart> parents;
            public NativeArray<FractalPart> parts;
            [WriteOnly]
            public NativeArray<float3x4> matrices;
            
            public void Execute(int index)
            {
                FractalPart parent = parents[index / 5];
                FractalPart part = parts[index];
                part.spinAngle += spinAngleDelta;
                float3 upAxis = mul(mul(parent.worldRot, part.rot), up());
                float3 sagAxis = cross(up(), upAxis);
                float sagMagnitude = length(sagAxis);
                quaternion baseRot;
                if (sagMagnitude > 0f)
                {
                    sagAxis /= sagMagnitude;
                    quaternion sagRotation = quaternion.AxisAngle(sagAxis, PI * 0.25f);
                    baseRot = mul(sagRotation, parent.worldRot);
                }
                else
                {
                    baseRot = parent.worldRot;
                }

                part.worldRot = mul(baseRot, mul(part.rot, quaternion.RotateY(part.spinAngle)));
                part.worldPos =
                    parent.worldPos + mul(part.worldRot, float3(0f,1.5f * scale,0f));

                parts[index] = part;
                float3x3 r = float3x3(part.worldRot) * scale;
                matrices[index] = float3x4(r.c0, r.c1, r.c2, part.worldPos);
            }
        }
        struct FractalPart
        {
            public float3 worldPos;
            public quaternion rot, worldRot;
            public float spinAngle;
        }
        
        [SerializeField, Range(3, 8)] private int depth = 4;
        
        [SerializeField] private Mesh mesh, leafMesh;

        [SerializeField] private Material material;

        private NativeArray<FractalPart>[] parts;
        private NativeArray<float3x4>[] matrices;

        private ComputeBuffer[] matricesBuffers;

        private static quaternion[] rots =
        {
            quaternion.identity, 
            quaternion.RotateZ(-0.5f * PI), 
            quaternion.RotateZ(0.5f* PI), 
            quaternion.RotateX(0.5f * PI),
            quaternion.RotateX(-0.5f * PI) 
        };

        private void OnEnable()
        {
            parts = new NativeArray<FractalPart>[depth];
            matrices = new NativeArray<float3x4>[depth];
            matricesBuffers = new ComputeBuffer[depth];
            sequenceNumbers = new Vector4[depth];
            int stride = 12 * sizeof(float);
            for (int i = 0, length = 1; i < parts.Length; ++i, length *= 5)
            {
                parts[i] = new NativeArray<FractalPart>(length, Allocator.Persistent);
                matrices[i] = new NativeArray<float3x4>(length, Allocator.Persistent);
                matricesBuffers[i] = new ComputeBuffer(length, stride);
                sequenceNumbers[i] = new Vector4(Random.value, Random.value,Random.value, Random.value);
            }
            
            parts[0][0] = CreatePart(0);
            for (int i = 1; i < parts.Length; ++i)
            {
                NativeArray<FractalPart> levelParts = parts[i];
                for (int j = 0; j < levelParts.Length; j += 5)
                {
                    for (int w = 0; w < 5; ++w)
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
            for (int i = 0; i < matricesBuffers.Length; ++i)
            {
                matricesBuffers[i].Release();
                parts[i].Dispose();
                matrices[i].Dispose();
            }

            parts = null;
            matrices = null;
            matricesBuffers = null;
            sequenceNumbers = null;
        }

        private void Update()
        {
            float spinAngleDelta = 0.125f * PI * Time.deltaTime;

            FractalPart root = parts[0][0];
            root.spinAngle += spinAngleDelta;
            root.worldRot = mul(transform.rotation, 
                mul(root.rot, quaternion.RotateY(root.spinAngle)));
            root.worldPos = transform.position;
            parts[0][0] = root;
            float objectScale = transform.lossyScale.x;
            float3x3 r = float3x3(root.worldRot) * objectScale;
            matrices[0][0] = float3x4(r.c0, r.c1, r.c2, root.worldPos);

            float scale = objectScale;
            JobHandle handle = default;
            for (int li = 1; li < parts.Length; li++)
            {
                scale *= 0.5f;
                var Job = new UpdateFractalLevelJob
                {
                    spinAngleDelta = spinAngleDelta,
                    scale = scale,
                    parents = parts[li - 1],
                    parts = parts[li],
                    matrices = matrices[li]
                };
                handle = Job.ScheduleParallel(parts[li].Length, 1, handle);
            }
            handle.Complete();
            
            var bounds = new Bounds(root.worldPos, 3f * float3(objectScale));
            int leafIndex = matricesBuffers.Length - 1;
            for (int i = 0; i < matricesBuffers.Length; ++i)
            {
                ComputeBuffer buffer = matricesBuffers[i];
                Mesh MeshInstance;
                Color colorA, colorB;
                if (i == leafIndex) {
                    colorA = leafColorA;
                    colorB = leafColorB;
                    MeshInstance = leafMesh;
                }
                else
                {
                    MeshInstance = mesh;
                    float gradientInterpolator = i / (matricesBuffers.Length - 2f);
                    colorA = _gradientA.Evaluate(gradientInterpolator);
                    colorB = _gradientB.Evaluate(gradientInterpolator);
                }

                _propertyBlock.SetColor(colorAId, colorA);
                _propertyBlock.SetColor(colorBId, colorB);
                buffer.SetData(matrices[i]);
                _propertyBlock.SetBuffer(matricesId, buffer);
                _propertyBlock.SetVector(sequenceNumbersId, sequenceNumbers[i]);
                Graphics.DrawMeshInstancedProcedural(MeshInstance, 0, material, bounds, buffer.count, _propertyBlock);
            }
        }

        private FractalPart CreatePart(int childIndex)
        {
            return new FractalPart
            {
                rot = rots[childIndex]
            };
        }
        
    }
   
}
