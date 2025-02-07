using UnityEngine;
using System;
using ALF;
using System.Runtime.CompilerServices;

namespace ALF
{
    public  class InfiniteLoopDetector : IBase
    {
        private  string prevPoint = "";
        private int detectionCount = 0;
        private const int DetectionThreshold = 100000;

        public InfiniteLoopDetector(){}

        public void Dispose(){}

        // [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void Run([CallerMemberName] string mn = "",[CallerFilePath] string fp = "",[CallerLineNumber] int ln = 0)
        {
            string currentPoint = $"{fp}{ln} : {mn}()";

            if (prevPoint == currentPoint)
                detectionCount++;
            else
                detectionCount = 0;

            if (detectionCount > DetectionThreshold)
                throw new Exception($"Infinite Loop Detected: \n{currentPoint}\n\n");

            prevPoint = currentPoint;
        }

    // #if UNITY_EDITOR
    //     [UnityEditor.InitializeOnLoadMethod]
    //     private static void Init()
    //     {
    //         UnityEditor.EditorApplication.update += () =>
    //         {
    //             detectionCount = 0;
    //         };
    //     }
    // #endif

        public void Reset()
        {
            detectionCount = 0;
        }
    }
}
