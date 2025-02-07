// using UnityEngine;
// using UnityEngine.UI;
// using System.Collections.Generic;
 
// [ExecuteInEditMode]
// [RequireComponent(typeof(CanvasRenderer))]
// [RequireComponent(typeof(ParticleSystem))]
// public class UIParticleSystem : MaskableGraphic {
 
//     public Texture particleTexture;
//     public Sprite particleSprite;
 
//     private Transform _transform;
//     private ParticleSystem _particleSystem;
//     private ParticleSystem.Particle[] _particles;
//     private UIVertex[] _quad = new UIVertex[4];
//     private Vector4 _uv = Vector4.zero;
//     private ParticleSystem.TextureSheetAnimationModule _textureSheetAnimation;
//     private int _textureSheetAnimationFrames;
//     private Vector2 _textureSheedAnimationFrameSize;
 
//     public override Texture mainTexture {
//         get {
//             if (particleTexture) {
//                 return particleTexture;
//             }
 
//             if (particleSprite) {
//                 return particleSprite.texture;
//             }
               
//             return null;
//         }
//     }
 
//     protected bool Initialize() {
//         // initialize members
//         if (_transform == null) {
//             _transform = transform;
//         }
 
//         // prepare particle system
//         ParticleSystemRenderer renderer = GetComponent<ParticleSystemRenderer>();
//         bool setParticleSystemMaterial = false;
 
//         if (_particleSystem == null) {
//             _particleSystem = GetComponent<ParticleSystem>();
 
//             if (_particleSystem == null) {
//                 return false;
//             }
 
//             // get current particle texture
//             if (renderer == null) {
//                 renderer = _particleSystem.gameObject.AddComponent<ParticleSystemRenderer>();
//             }
//             Material currentMaterial = renderer.sharedMaterial;
//             if (currentMaterial && currentMaterial.HasProperty("_MainTex")) {
//                 particleTexture = currentMaterial.mainTexture;
//             }
 
//             // automatically set scaling
//             _particleSystem.scalingMode = ParticleSystemScalingMode.Local;
 
//             _particles = null;
//             setParticleSystemMaterial = true;
//         } else {
//             if (Application.isPlaying) {
//                 setParticleSystemMaterial = (renderer.material == null);
//             }
//             #if UNITY_EDITOR
//             else {
//                 setParticleSystemMaterial = (renderer.sharedMaterial == null);
//             }
//             #endif
//         }
 
//         // automatically set material to UI/Particles/Hidden shader, and get previous texture
//         if (setParticleSystemMaterial) {
//             Material material = new Material(Shader.Find("UI/Particles/Hidden"));
//             if (Application.isPlaying) {
//                 renderer.material = material;
//             }
//             #if UNITY_EDITOR
//             else {
//                 material.hideFlags = HideFlags.DontSave;
//                 renderer.sharedMaterial = material;
//             }
//             #endif
//         }
 
//         // prepare particles array
//         if (_particles == null) {
//             _particles = new ParticleSystem.Particle[_particleSystem.maxParticles];
//         }
 
//         // prepare uvs
//         if (particleTexture) {
//             _uv = new Vector4(0, 0, 1, 1);
//         } else if (particleSprite) {
//             _uv = UnityEngine.Sprites.DataUtility.GetOuterUV(particleSprite);
//         }
 
//         // prepare texture sheet animation
//         _textureSheetAnimation = _particleSystem.textureSheetAnimation;
//         _textureSheetAnimationFrames = 0;
//         _textureSheedAnimationFrameSize = Vector2.zero;
//         if (_textureSheetAnimation.enabled) {
//             _textureSheetAnimationFrames = _textureSheetAnimation.numTilesX * _textureSheetAnimation.numTilesY;
//             _textureSheedAnimationFrameSize = new Vector2(1f / _textureSheetAnimation.numTilesX, 1f / _textureSheetAnimation.numTilesY);
//         }
 
//         return true;
//     }
 
//     protected override void Awake() {
//         base.Awake();
 
//         if (!Initialize()) {
//             enabled = false;
//         }
//     }
 
//     protected override void OnPopulateMesh(VertexHelper vh) {
//         #if UNITY_EDITOR
//         if (!Application.isPlaying) {
//             if (!Initialize()) {
//                 return;
//             }
//         }
//         #endif
 
//         // prepare vertices
//         vh.Clear();
 
//         if (!gameObject.activeInHierarchy) {
//             return;
//         }
 
//         // iterate through current particles
//         int count = _particleSystem.GetParticles(_particles);
 
//         for (int i = 0; i < count; ++i) {
//             ParticleSystem.Particle particle = _particles[i];
 
//             // get particle properties
//             Vector2 position = (_particleSystem.main.simulationSpace == ParticleSystemSimulationSpace.Local ? particle.position : _transform.InverseTransformPoint(particle.position));
//             float rotation = -particle.rotation * Mathf.Deg2Rad;
//             float rotation90 = rotation + Mathf.PI / 2;
//             Color32 color = particle.GetCurrentColor(_particleSystem);
//             float size = particle.GetCurrentSize(_particleSystem) * 0.5f;
 
//             // apply scale
//             if (_particleSystem.main.scalingMode == ParticleSystemScalingMode.Shape) {
//                 position /= canvas.scaleFactor;
//             }
 
//             // apply texture sheet animation
//             Vector4 particleUV = _uv;
//             if (_textureSheetAnimation.enabled) {
//                 float frameProgress = 1 - (particle.remainingLifetime / particle.startLifetime);
// //                float frameProgress = textureSheetAnimation.frameOverTime.curveMin.Evaluate(1 - (particle.lifetime / particle.startLifetime)); // TODO - once Unity allows MinMaxCurve reading
//                 frameProgress = Mathf.Repeat(frameProgress * _textureSheetAnimation.cycleCount, 1);
//                 int frame = 0;
 
//                 switch (_textureSheetAnimation.animation) {
 
//                 case ParticleSystemAnimationType.WholeSheet:
//                     frame = Mathf.FloorToInt(frameProgress * _textureSheetAnimationFrames);
//                     break;
 
//                 case ParticleSystemAnimationType.SingleRow:
//                     frame = Mathf.FloorToInt(frameProgress * _textureSheetAnimation.numTilesX);
 
//                     int row = _textureSheetAnimation.rowIndex;
// //                    if (textureSheetAnimation.useRandomRow) { // FIXME - is this handled internally by rowIndex?
// //                        row = Random.Range(0, textureSheetAnimation.numTilesY, using: particle.randomSeed);
// //                    }
//                     frame += row * _textureSheetAnimation.numTilesX;
//                     break;
 
//                 }
 
//                 frame %= _textureSheetAnimationFrames;
 
//                 particleUV.x = (frame % _textureSheetAnimation.numTilesX) * _textureSheedAnimationFrameSize.x;
//                 particleUV.y = Mathf.FloorToInt(frame / _textureSheetAnimation.numTilesX) * _textureSheedAnimationFrameSize.y;
//                 particleUV.z = particleUV.x + _textureSheedAnimationFrameSize.x;
//                 particleUV.w = particleUV.y + _textureSheedAnimationFrameSize.y;
//             }
 
//             _quad[0] = UIVertex.simpleVert;
//             _quad[0].color = color;
//             _quad[0].uv0 = new Vector2(particleUV.x, particleUV.y);
 
//             _quad[1] = UIVertex.simpleVert;
//             _quad[1].color = color;
//             _quad[1].uv0 = new Vector2(particleUV.x, particleUV.w);
 
//             _quad[2] = UIVertex.simpleVert;
//             _quad[2].color = color;
//             _quad[2].uv0 = new Vector2(particleUV.z, particleUV.w);
 
//             _quad[3] = UIVertex.simpleVert;
//             _quad[3].color = color;
//             _quad[3].uv0 = new Vector2(particleUV.z, particleUV.y);
 
//             if (rotation == 0) {
//                 // no rotation
//                 Vector2 corner1 = new Vector2(position.x - size, position.y - size);
//                 Vector2 corner2 = new Vector2(position.x + size, position.y + size);
 
//                 _quad[0].position = new Vector2(corner1.x, corner1.y);
//                 _quad[1].position = new Vector2(corner1.x, corner2.y);
//                 _quad[2].position = new Vector2(corner2.x, corner2.y);
//                 _quad[3].position = new Vector2(corner2.x, corner1.y);
//             } else {
//                 // apply rotation
//                 Vector2 right = new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation)) * size;
//                 Vector2 up = new Vector2(Mathf.Cos(rotation90), Mathf.Sin(rotation90)) * size;
 
//                 _quad[0].position = position - right - up;
//                 _quad[1].position = position - right + up;
//                 _quad[2].position = position + right + up;
//                 _quad[3].position = position + right - up;
//             }
 
//             vh.AddUIVertexQuad(_quad);
//         }
//     }
 
//     void Update() {
//         if (Application.isPlaying) {
//             // unscaled animation within UI
//             _particleSystem.Simulate(Time.unscaledDeltaTime, false, false);
 
//             SetAllDirty();
//         }
//     }
 
//     #if UNITY_EDITOR
//     void LateUpdate() {
//         if (!Application.isPlaying) {
//             SetAllDirty();
//         }
//     }
//     #endif
 
// }

/// Credit glennpow, Zarlang
/// Sourced from - http://forum.unity3d.com/threads/free-script-particle-systems-in-ui-screen-space-overlay.406862/
/// Updated by Zarlang with a more robust implementation, including TextureSheet animation support

namespace UnityEngine.UI.Extensions
{
#if UNITY_5_3_OR_NEWER
    [ExecuteInEditMode]
    [RequireComponent(typeof(CanvasRenderer), typeof(ParticleSystem))]
    [AddComponentMenu("UI/Effects/Extensions/UIParticleSystem")]
    public class UIParticleSystem : MaskableGraphic
    {
        [Tooltip("Having this enabled run the system in LateUpdate rather than in Update making it faster but less precise (more clunky)")]
        public bool fixedTime = true;

        [Tooltip("Enables 3d rotation for the particles")]
        public bool use3dRotation = false;

        private Transform _transform;
        private ParticleSystem pSystem;
        private ParticleSystem.Particle[] particles;
        private UIVertex[] _quad = new UIVertex[4];
        private Vector4 imageUV = Vector4.zero;
        private ParticleSystem.TextureSheetAnimationModule textureSheetAnimation;
        private int textureSheetAnimationFrames;
        private Vector2 textureSheetAnimationFrameSize;
        private ParticleSystemRenderer pRenderer;
        private bool isInitialised = false;

        private Material currentMaterial;

        private Texture currentTexture;

#if UNITY_5_5_OR_NEWER
        private ParticleSystem.MainModule mainModule;
#endif

        public override Texture mainTexture
        {
            get
            {
                return currentTexture;
            }
        }

        protected bool Initialize()
        {
            // initialize members
            if (_transform == null)
            {
                _transform = transform;
            }
            if (pSystem == null)
            {
                pSystem = GetComponent<ParticleSystem>();

                if (pSystem == null)
                {
                    return false;
                }

#if UNITY_5_5_OR_NEWER
                mainModule = pSystem.main;
                if (pSystem.main.maxParticles > 14000)
                {
                    mainModule.maxParticles = 14000;
                }
#else
                if (pSystem.maxParticles > 14000)
                    pSystem.maxParticles = 14000;
#endif

                pRenderer = pSystem.GetComponent<ParticleSystemRenderer>();
                if (pRenderer != null)
                    pRenderer.enabled = false;

                if (material == null)
                {
                    // var foundShader = ShaderLibrary.GetShaderInstance("UI Extensions/Particles/Additive");
                   var foundShader = Shader.Find("UI/Additive");
                    if (foundShader)
                    {
                        material = new Material(foundShader);
                    }
                }

                currentMaterial = material;
                if (currentMaterial && currentMaterial.HasProperty("_MainTex"))
                {
                    currentTexture = currentMaterial.mainTexture;
                    if (currentTexture == null)
                        currentTexture = Texture2D.whiteTexture;
                }
                material = currentMaterial;
                // automatically set scaling
#if UNITY_5_5_OR_NEWER
                mainModule.scalingMode = ParticleSystemScalingMode.Hierarchy;
#else
                pSystem.scalingMode = ParticleSystemScalingMode.Hierarchy;
#endif

                particles = null;
            }
#if UNITY_5_5_OR_NEWER
            if (particles == null)
                particles = new ParticleSystem.Particle[pSystem.main.maxParticles];
#else
            if (particles == null)
                particles = new ParticleSystem.Particle[pSystem.maxParticles];
#endif

            imageUV = new Vector4(0, 0, 1, 1);

            // prepare texture sheet animation
            textureSheetAnimation = pSystem.textureSheetAnimation;
            textureSheetAnimationFrames = 0;
            textureSheetAnimationFrameSize = Vector2.zero;
            if (textureSheetAnimation.enabled)
            {
                textureSheetAnimationFrames = textureSheetAnimation.numTilesX * textureSheetAnimation.numTilesY;
                textureSheetAnimationFrameSize = new Vector2(1f / textureSheetAnimation.numTilesX, 1f / textureSheetAnimation.numTilesY);
            }

            return true;
        }

        protected override void Awake()
        {
            base.Awake();
            if (!Initialize())
                enabled = false;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (!Initialize())
                {
                    return;
                }
            }
#endif
            // prepare vertices
            vh.Clear();

            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            if (!isInitialised && !pSystem.main.playOnAwake)
            {
                pSystem.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                isInitialised = true;
            }

            Vector2 temp = Vector2.zero;
            Vector2 corner1 = Vector2.zero;
            Vector2 corner2 = Vector2.zero;
            // iterate through current particles
            int count = pSystem.GetParticles(particles);

            for (int i = 0; i < count; ++i)
            {
                ParticleSystem.Particle particle = particles[i];

                // get particle properties
#if UNITY_5_5_OR_NEWER
                Vector2 position = (mainModule.simulationSpace == ParticleSystemSimulationSpace.Local ? particle.position : _transform.InverseTransformPoint(particle.position));
#else
                Vector2 position = (pSystem.simulationSpace == ParticleSystemSimulationSpace.Local ? particle.position : _transform.InverseTransformPoint(particle.position));
#endif
                float rotation = -particle.rotation * Mathf.Deg2Rad;
                float rotation90 = rotation + Mathf.PI / 2;
                Color32 color = particle.GetCurrentColor(pSystem);
                float size = particle.GetCurrentSize(pSystem) * 0.5f;

                // apply scale
#if UNITY_5_5_OR_NEWER
                if (mainModule.scalingMode == ParticleSystemScalingMode.Shape)
                    position /= canvas.scaleFactor;
#else
                if (pSystem.scalingMode == ParticleSystemScalingMode.Shape)
                    position /= canvas.scaleFactor;
#endif

                // apply texture sheet animation
                Vector4 particleUV = imageUV;
                if (textureSheetAnimation.enabled)
                {
#if UNITY_5_5_OR_NEWER
                    float frameProgress = 1 - (particle.remainingLifetime / particle.startLifetime);

                    if (textureSheetAnimation.frameOverTime.curveMin != null)
                    {
                        frameProgress = textureSheetAnimation.frameOverTime.curveMin.Evaluate(1 - (particle.remainingLifetime / particle.startLifetime));
                    }
                    else if (textureSheetAnimation.frameOverTime.curve != null)
                    {
                        frameProgress = textureSheetAnimation.frameOverTime.curve.Evaluate(1 - (particle.remainingLifetime / particle.startLifetime));
                    }
                    else if (textureSheetAnimation.frameOverTime.constant > 0)
                    {
                        frameProgress = textureSheetAnimation.frameOverTime.constant - (particle.remainingLifetime / particle.startLifetime);
                    }
#else
                    float frameProgress = 1 - (particle.lifetime / particle.startLifetime);
#endif

                    frameProgress = Mathf.Repeat(frameProgress * textureSheetAnimation.cycleCount, 1);
                    int frame = 0;

                    switch (textureSheetAnimation.animation)
                    {

                        case ParticleSystemAnimationType.WholeSheet:
                            frame = Mathf.FloorToInt(frameProgress * textureSheetAnimationFrames);
                            break;

                        case ParticleSystemAnimationType.SingleRow:
                            frame = Mathf.FloorToInt(frameProgress * textureSheetAnimation.numTilesX);

                            int row = textureSheetAnimation.rowIndex;
#if UNITY_2020 || UNITY_2019
                            if (textureSheetAnimation.rowMode == ParticleSystemAnimationRowMode.Random)
#else
                            if (textureSheetAnimation.rowMode == ParticleSystemAnimationRowMode.Random)
#endif
                            { // FIXME - is this handled internally by rowIndex?
                                row = Mathf.Abs((int)particle.randomSeed % textureSheetAnimation.numTilesY);
                            }
                            frame += row * textureSheetAnimation.numTilesX;
                            break;

                    }

                    frame %= textureSheetAnimationFrames;

                    particleUV.x = (frame % textureSheetAnimation.numTilesX) * textureSheetAnimationFrameSize.x;
                    particleUV.y = 1.0f - ((frame / textureSheetAnimation.numTilesX) + 1) * textureSheetAnimationFrameSize.y;
                    particleUV.z = particleUV.x + textureSheetAnimationFrameSize.x;
                    particleUV.w = particleUV.y + textureSheetAnimationFrameSize.y;
                }

                temp.x = particleUV.x;
                temp.y = particleUV.y;

                _quad[0] = UIVertex.simpleVert;
                _quad[0].color = color;
                _quad[0].uv0 = temp;

                temp.x = particleUV.x;
                temp.y = particleUV.w;
                _quad[1] = UIVertex.simpleVert;
                _quad[1].color = color;
                _quad[1].uv0 = temp;

                temp.x = particleUV.z;
                temp.y = particleUV.w;
                _quad[2] = UIVertex.simpleVert;
                _quad[2].color = color;
                _quad[2].uv0 = temp;

                temp.x = particleUV.z;
                temp.y = particleUV.y;
                _quad[3] = UIVertex.simpleVert;
                _quad[3].color = color;
                _quad[3].uv0 = temp;

                if (rotation == 0)
                {
                    // no rotation
                    corner1.x = position.x - size;
                    corner1.y = position.y - size;
                    corner2.x = position.x + size;
                    corner2.y = position.y + size;

                    temp.x = corner1.x;
                    temp.y = corner1.y;
                    _quad[0].position = temp;
                    temp.x = corner1.x;
                    temp.y = corner2.y;
                    _quad[1].position = temp;
                    temp.x = corner2.x;
                    temp.y = corner2.y;
                    _quad[2].position = temp;
                    temp.x = corner2.x;
                    temp.y = corner1.y;
                    _quad[3].position = temp;
                }
                else
                {
                    if (use3dRotation)
                    {
                        // get particle properties
#if UNITY_5_5_OR_NEWER
                        Vector3 pos3d = (mainModule.simulationSpace == ParticleSystemSimulationSpace.Local ? particle.position : _transform.InverseTransformPoint(particle.position));
#else
                        Vector3 pos3d = (pSystem.simulationSpace == ParticleSystemSimulationSpace.Local ? particle.position : _transform.InverseTransformPoint(particle.position));
#endif

                        // apply scale
#if UNITY_5_5_OR_NEWER
                        if (mainModule.scalingMode == ParticleSystemScalingMode.Shape)
                            position /= canvas.scaleFactor;
#else
                        if (pSystem.scalingMode == ParticleSystemScalingMode.Shape)
                            position /= canvas.scaleFactor;
#endif

                        Vector3[] verts = new Vector3[4]
                        {
                            new Vector3(-size, -size, 0),
                            new Vector3(-size, size, 0),
                            new Vector3(size, size, 0),
                            new Vector3(size, -size, 0)
                        };

                        Quaternion particleRotation = Quaternion.Euler(particle.rotation3D);

                        _quad[0].position = pos3d + particleRotation * verts[0];
                        _quad[1].position = pos3d + particleRotation * verts[1];
                        _quad[2].position = pos3d + particleRotation * verts[2];
                        _quad[3].position = pos3d + particleRotation * verts[3];
                    }
                    else
                    {
                        // apply rotation
                        Vector2 right = new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation)) * size;
                        Vector2 up = new Vector2(Mathf.Cos(rotation90), Mathf.Sin(rotation90)) * size;

                        _quad[0].position = position - right - up;
                        _quad[1].position = position - right + up;
                        _quad[2].position = position + right + up;
                        _quad[3].position = position + right - up;
                    }
                }

#if UNITY_5_5_OR_NEWER
                Vector3 size3D = particle.GetCurrentSize3D(pSystem);
                if(!size3D.Equals(Vector3.zero))
                {
                    for(int t =0; t < 4; ++t)
                    {
                        _quad[t].position.x *= size3D.x;
                        _quad[t].position.y *= size3D.y;
                        _quad[t].position.z *= size3D.z;
                    }
                }
#endif
                vh.AddUIVertexQuad(_quad);
            }
        }

        private void Update()
        {
            if (!fixedTime && Application.isPlaying)
            {
                pSystem.Simulate(Time.unscaledDeltaTime, false, false, true);
                SetAllDirty();

                if ((currentMaterial != null && currentTexture != currentMaterial.mainTexture) ||
                    (material != null && currentMaterial != null && material.shader != currentMaterial.shader))
                {
                    pSystem = null;
                    Initialize();
                }
            }
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                SetAllDirty();
            }
            else
            {
                if (fixedTime)
                {
                    pSystem.Simulate(Time.unscaledDeltaTime, false, false, true);
                    SetAllDirty();
                    if ((currentMaterial != null && currentTexture != currentMaterial.mainTexture) ||
                        (material != null && currentMaterial != null && material.shader != currentMaterial.shader))
                    {
                        pSystem = null;
                        Initialize();
                    }
                }
            }
            if (material == currentMaterial)
                return;
            pSystem = null;
            Initialize();
        }

        protected override void OnDestroy()
        {
            currentMaterial = null;
            currentTexture = null;
        }

        public void StartParticleEmission()
        {
            pSystem.time = 0;
            pSystem.Play();
        }

        public void StopParticleEmission()
        {
            pSystem.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public void PauseParticleEmission()
        {
            pSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        }
    }
#endif
                    }