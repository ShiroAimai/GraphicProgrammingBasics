using UnityEngine;

public class Graph : MonoBehaviour
{
     [SerializeField]
     private Transform prefabPoint;

     [SerializeField, Range(10, 100)] private int resolution = 10;
     [SerializeField] private FunctionLibrary.Functions function;
     
     private Transform[] points;
     private void Awake()
     {
          points = new Transform[resolution * resolution];
          float step = 2f / resolution;
          Vector3 scale = Vector3.one * step;
          for (int i = 0; i < points.Length; ++i)
          {
               points[i] = Instantiate(prefabPoint, transform, false);
               points[i].localScale = scale;
          }
     }

     private void Update()
     {
          FunctionLibrary.Function f = FunctionLibrary.GetFunc(function);
          float elapsed = Time.time;
          float step = 2f / resolution;
          float v = 0.5f * step - 1f;
          for (int i = 0, x = 0, z = 0; i < points.Length; ++i, ++x)
          {
               if (x == resolution)
               {
                    x = 0;
                    ++z;
                    v = (z + 0.5f) * step - 1f;
               }
               Transform point = points[i];
               float u = (x + 0.5f) * step - 1f;
               point.localPosition = f(u, v, elapsed);
          }
     }
}
