using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUGraph : MonoBehaviour
{
    private const int max_res = 1000;
    
    [SerializeField] private ComputeShader computeShader;
    static readonly int positionsId = Shader.PropertyToID("_Positions"),
    resolutionId = Shader.PropertyToID("_Resolution"),
    stepId = Shader.PropertyToID("_Step"),
    timeId = Shader.PropertyToID("_Time"),
    transitionProgressId = Shader.PropertyToID("_TransitionProgress");
    
    [SerializeField]
    Material material;

    [SerializeField]
    Mesh mesh;
    
    [SerializeField, Range(10, max_res)]
    int resolution = 10;

    [SerializeField]
    FunctionLibrary.Functions function;

    public enum TransitionMode { Cycle, Random }

    [SerializeField]
    TransitionMode transitionMode = TransitionMode.Cycle;

    [SerializeField, Min(0f)]
    float functionDuration = 1f, transitionDuration = 1f;
    
    float duration;

    bool transitioning;

    FunctionLibrary.Functions transitionFunction;

    private ComputeBuffer positionsBuffer;

    private void OnEnable()
    {
        positionsBuffer = new ComputeBuffer(max_res * max_res, 4 * 3);
    }

    private void OnDisable()
    {
        positionsBuffer.Release();
        positionsBuffer = null;
    }

    void Update()
    {
        duration += Time.deltaTime;
        if (transitioning) {
            if (duration >= transitionDuration) {
                duration -= transitionDuration;
                transitioning = false;
            }
        }
        else if (duration >= functionDuration) {
            duration -= functionDuration;
            transitioning = true;
            transitionFunction = function;
            PickNextFunction();
        }

        UpdateFunctionOnGPU();
    }

    void PickNextFunction () {
        function = transitionMode == TransitionMode.Cycle ?
            FunctionLibrary.GetNextFunctionName(function) :
            FunctionLibrary.GetRandomFunctionNameOtherThan(function);
    }
    
    void UpdateFunctionOnGPU () {
        float step = 2f / resolution;

        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetFloat(stepId, step);
        computeShader.SetFloat(timeId, Time.time);
        if (transitioning)
        {
            computeShader.SetFloat(transitionProgressId,Mathf.SmoothStep(0f, 1f, duration / transitionDuration));
        }
        var index = (int) function + (int) (transitioning ? transitionFunction : function) * FunctionLibrary.FunctionCount;
        computeShader.SetBuffer(index, positionsId, positionsBuffer);

        
        int groups = Mathf.CeilToInt(resolution / 8f);
        computeShader.Dispatch(index, groups, groups, 1);
        
        material.SetBuffer(positionsId, positionsBuffer);
        material.SetFloat(stepId, step);
        
        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, resolution * resolution);
    }
}
