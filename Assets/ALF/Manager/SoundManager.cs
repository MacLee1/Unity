using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using ALF.MACHINE;
using ALF.STATE;
using ALF.CONDITION;
using System.Linq;

namespace ALF.SOUND
{
    public class SoundManager : IBase
    {
        const string C_BGM = "BGM";
        const string C_SFX = "SFX";
        internal class ChangeBGMStateData : IStateData
        {
            public AudioSource Out {get; set;}
            public AudioSource In {get; set;}
            
            public ChangeBGMStateData(){}
            public void Dispose()
            {
                Out = null;
                In = null;
            }
        }

        internal class SFXStateData : IStateData
        {
            public AudioSource Src {get; set;}
            public float Delay {get; set;}
            public bool IsPlay {get; set;}
            public int ID {get; set;}

            public bool Fade {get; set;}
            
            public SFXStateData(){}
            public void Dispose()
            {
                Src = null;
            }
        }

        internal class ChangeValumeStateData : IStateData
        {
            public AudioSource Src {get; set;}
            public float Volume {get; set;}

            public bool Mute {get; set;}
            public float EndVolume {get; set;}
            public ChangeValumeStateData(){}
            public void Dispose()
            {
                Src = null;
            }
        }
        
        const uint BGM_CHANGE_STATE_TYPE = 20000;
        const uint BGM_CHANGE_VOLUME_STATE_TYPE = 20001;
        // List<Vector2Int> m_baseRespawnList = new List<Vector2Int>();
        AudioSource BGM = null;
        Dictionary<int,AudioSource> SFX  = new Dictionary<int,AudioSource>(10);
        Stack<AudioSource> audioSourceList  = new Stack<AudioSource>();
        StateMachine soundStateMachine = null;

        // private AudioSource[] SFXsource;
        float volumeBGM = 1.0f;
        float volumeSFX = 1.0f;
        bool m_bBGMPause = false;
        bool m_bSFXPause = false;

        public bool IsBGMPause {get{return m_bBGMPause;}}
        public bool IsSFXPause {get{return m_bSFXPause;}}
        public bool IsAllPause {get{return (m_bBGMPause && m_bSFXPause);}}

        public float VolumeBGM
        {
            get{ return volumeBGM;}

            set{ 
                volumeBGM = value;
                // PlayerPrefs.SetFloat("BGMV", volumeBGM);
            }
        }
        
        public float VolumeSFX
        {
            get{ return volumeSFX;}

            set{ 
                volumeSFX = value;
                // PlayerPrefs.SetFloat("SFXV", volumeSFX);
            }
        }

        bool isPreload = true;

        static SoundManager instance = null;

        public static SoundManager Instance
        {
            get {return instance;}
        }

        public static bool InitInstance()
        {
            if (instance == null)
            {
                instance = new SoundManager();
                return true;
            }
            return false;
        }

        public static void PreloadFile()
        {
            Director.StartCoroutine(Instance.coPreloadFile());
        }

        public static bool IsPreloadFile()
        {
            return Instance.SFX.Count != 0;
        }

        IEnumerator coPreloadFile()
        {
            List<string> clipIDList = AFPool.GetAllItemName<AudioClip>(C_SFX);
            if(clipIDList.Count > 0)
            {
                float volume = VolumeSFX;
                for(int i =0; i < clipIDList.Count; ++i)
                {
                    PlaySFX(clipIDList[i]);
                    yield return null;
                }
            }
        }

        protected SoundManager()
        {
            volumeBGM = PlayerPrefs.GetFloat("BGMV", 1);
            volumeSFX = PlayerPrefs.GetFloat("SFXV", 1);
            soundStateMachine = StateMachine.Create();
            Director.Instance.AddSchedule(soundStateMachine);
        }
        
        public void Dispose()
        {
            soundStateMachine.Dispose();
            soundStateMachine = null;
        }

        public void StopBGM( bool isSmooth = false,float duration = 1.0f)
        {
            if(BGM != null)
            {
                if(isSmooth)
                {
                    BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(Director.Runner),duration, BGM_CHANGE_VOLUME_STATE_TYPE, null, this.executeChangeValumeCallback,this.exitChangeValumeCallback);
                    ChangeValumeStateData data = new ChangeValumeStateData();

                    data.Mute = false;
                    data.Src = BGM;
                    data.Volume = BGM.volume;
                    data.EndVolume = -(BGM.volume);
                    pBaseState.StateData = data;
                    soundStateMachine.AddState(pBaseState);
                }
                else
                {
                    BGM.Stop();
                    AddEmptyAudioSource(BGM);
                    BGM = null;
                }
            }
        }

        public void FadeOutFX(int id )
        {
            if(SFX.ContainsKey(id))
            {
                List<BaseState> list = soundStateMachine.GetCurrentTargetStates<BaseState>(SFX[id],BGM_CHANGE_STATE_TYPE);
                for(int i = 0; i < list.Count; ++i)
                {
                    if(list[i].StateData is SFXStateData data)
                    {
                        data.Fade = true;
                    }
                }
            }
        }

        public void ChangeBGM(string id, bool isSmooth = false, float duration = 1.0f)
        {
            if(IsPlayBGM(id))
            {
                return;
            }

            AudioClip clip = AFPool.GetItem<AudioClip>(C_BGM,id);
            if(clip == null) return;
            // ALFUtils.Assert(clip, "ChangeBGM: not find AudioClip!");

            if (isSmooth)
            {
                BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(Director.Runner),duration, BGM_CHANGE_STATE_TYPE, this.enterFadeCallback, this.executeFadeCallback,this.exitFadeCallback);
                ChangeBGMStateData data = new ChangeBGMStateData();

                AudioSource audioSource = GetEmptyAudioSource(clip);
                audioSource.volume = 0;
                audioSource.playOnAwake = false;
                audioSource.loop = true;

                data.In = audioSource;
                data.Out = BGM;
                pBaseState.StateData = data;
                soundStateMachine.AddState(pBaseState);
            }
            else
            {
                BGM = GetEmptyAudioSource(clip);
                BGM.volume = VolumeBGM;
                BGM.playOnAwake = false;
                BGM.loop = true;
                BGM.Play();
                if(m_bBGMPause)
                {
                    BGM.Pause();
                }
            }
        }
        public void ChangeBGMVolume(float fValume, float duration = 1.0f, bool bMute = false)
        {
            if(BGM == null || BGM.volume == fValume || m_bBGMPause)
            {
                return;
            }

            List<BaseState> list = soundStateMachine.GetCurrentTargetStates<BaseState>(Director.Runner,BGM_CHANGE_VOLUME_STATE_TYPE);
            if(list.Count > 0)
            {
                for(int i = 0; i < list.Count; ++i)
                {
                    list[i].Exit(true);
                }
                list.Clear();
            }

            BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(Director.Runner),duration, BGM_CHANGE_VOLUME_STATE_TYPE, null, this.executeChangeValumeCallback);
            ChangeValumeStateData data = new ChangeValumeStateData();

            data.Mute = bMute;
            data.Src = BGM;
            data.Volume = BGM.volume;
            // VolumeBGM = fValume;
            if(BGM.volume > fValume)
            {
                data.EndVolume = -(BGM.volume - fValume);
            }
            else
            {
                data.EndVolume = fValume > VolumeBGM ? VolumeBGM : fValume;
            }
            
            pBaseState.StateData = data;
            soundStateMachine.AddState(pBaseState);
        }

        AudioSource GetEmptyAudioSource(AudioClip clip)
        {
            AudioSource audioSource = null;
            
            if(audioSourceList.Any())
            {
                audioSource = audioSourceList.Pop();
            }
            else
            {
                audioSource = Director.gameObject.AddComponent<AudioSource>();
            }
            
            audioSource.enabled = true;
            audioSource.clip = clip;
            
            return audioSource;
        }
        void AddEmptyAudioSource(AudioSource audioSource)
        {
            if(audioSource != null)
            {
                audioSource.Stop();
                audioSource.clip = null;
                audioSource.volume = 0;
                audioSource.playOnAwake = false;
                audioSource.loop = false;
                audioSource.enabled = false;
                audioSourceList.Push(audioSource);
            }
        }
        void enterFadeCallback(IState state)
        {
            if (state.StateData is ChangeBGMStateData data)
            {
                if(data.In != null)
                {
                    data.In.volume = 0;
                    data.In.playOnAwake = false;
                    data.In.loop = true;
                    data.In.Play();
                    if(m_bBGMPause)
                    {
                        data.In.Pause();
                    }
                }
            }
        }
        bool executeFadeCallback(IState state,float dt,bool bEnd)
        {
            if (state.StateData is ChangeBGMStateData data)
            {
                TimeOutCondition condition = state.GetCondition<TimeOutCondition>();
                float v = VolumeBGM * condition.GetTimePercent();
                if(data.In != null)
                {
                    data.In.volume = v;
                }

                if(data.Out != null)
                {
                    data.Out.volume = VolumeBGM - v;
                }
            }

            return bEnd;
        }

        bool executeChangeValumeCallback(IState state,float dt,bool bEnd)
        {
            if (state.StateData is ChangeValumeStateData data)
            {
                TimeOutCondition condition = state.GetCondition<TimeOutCondition>();
                float v = data.EndVolume * condition.GetTimePercent() + data.Volume;
                
                if(VolumeBGM < v)
                {
                    v = VolumeBGM;
                }
                else if(0 > v)
                {
                    v = 0;
                }
                data.Src.volume = v;
                if(bEnd && data.Mute && BGM != null)
                {
                    if(data.EndVolume <= 0)
                    {
                        BGM.Pause();
                    }
                    else
                    {
                        BGM.UnPause();
                    }
                }
            }
            return bEnd;
        }

        IState exitFadeCallback(IState state)
        {
            if (state.StateData is ChangeBGMStateData data)
            {
                AddEmptyAudioSource(BGM);
                BGM = data.In;
            }
            return null;
        }

        IState exitChangeValumeCallback(IState state)
        {
            if (state.StateData is ChangeValumeStateData data)
            {
                AddEmptyAudioSource(BGM);
                BGM = null;
            }
            return null;
        }
        
        bool executeSFXCallback(IState state,float dt,bool bEnd)
        {
            if (state.StateData is SFXStateData data)
            {
                if(data.IsPlay == false )
                {
                    TimeOutCondition condition = state.GetCondition<TimeOutCondition>();
                    if ( condition.GetElapsed() >= data.Delay)
                    {
                        data.Src.volume = VolumeSFX;
                        data.Src.playOnAwake = false;
                        // data.Src.loop = false;
                        data.Src.Play();
                        if(m_bSFXPause)
                        {
                            data.Src.Pause();
                        }
                        data.IsPlay = true;
                    }
                }
                else
                {
                    if(data.Fade)
                    {
                       data.Src.volume = data.Src.volume - 0.02f;
                    }
                }
            }

            return bEnd;
        }

        IState exitSFXCallback(IState state)
        {
            if (state.StateData is SFXStateData data)
            {
                SFX.Remove(data.ID);
                AddEmptyAudioSource(data.Src);
            }
            return null;
        }

        public void StopSFX(int id)
        {
            if(SFX.ContainsKey(id))
            {
                List<BaseState> list = soundStateMachine.GetCurrentTargetStates<BaseState>(SFX[id],BGM_CHANGE_STATE_TYPE);
                for(int i = 0; i < list.Count; ++i)
                {
                    list[i].Exit(true);
                }
                list.Clear();
            }
        }

        public int PlaySFX(string id, float delay = 0.0f,bool bLoop = false)
        {
            AudioClip clip = AFPool.GetItem<AudioClip>(C_SFX,id);
            if(clip == null) return -1;
            // ALFUtils.Assert(clip, "PlaySFX: not find AudioClip!");
            AudioSource audioSource = GetEmptyAudioSource(clip);
            audioSource.volume = VolumeSFX;
            audioSource.playOnAwake = false;
            audioSource.loop = bLoop;
            float duration = bLoop ? -1 : delay + clip.length;
            BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(audioSource),duration, BGM_CHANGE_STATE_TYPE, null, this.executeSFXCallback,this.exitSFXCallback);
            SFXStateData data = new SFXStateData();
            data.Fade = false;
            data.Src = audioSource;
            data.Delay = delay;
            data.IsPlay = false;
            data.ID = audioSource.GetInstanceID();
            SFX[data.ID] = audioSource;
            pBaseState.StateData = data;
            soundStateMachine.AddState(pBaseState);

            return data.ID;
        }

        public bool IsPlaySFX(int id)
        {
            if(SFX.ContainsKey(id))
            {
                return SFX[id].isPlaying;
            }
            
            return false;
        }

        public bool IsPlayBGM(string id = null)
        {
            if (BGM != null)
            {
                if(string.IsNullOrEmpty(id))
                {
                    return BGM.isPlaying;
                }
                
                if(BGM.clip.name == id)
                {
                    return BGM.isPlaying;
                }
            }

            return false;
        }

        public void AllPauseSFX()
        {
            if(!m_bSFXPause)
            {
                var it = SFX.GetEnumerator();
                while(it.MoveNext())
                {
                    it.Current.Value.Pause();
                }
                m_bSFXPause = true;
            }
        }

        public void AllUnPauseSFX()
        {
            if(m_bSFXPause)
            {
                m_bSFXPause = false;
                var it = SFX.GetEnumerator();
                while(it.MoveNext())
                {
                    it.Current.Value.UnPause();
                }
            }
        }

        public void AllPauseBGM(bool bFade = true)
        {
            if(!m_bBGMPause)
            {
                if (BGM != null)
                {
                    if(bFade)
                    {
                        SoundManager.Instance.ChangeBGMVolume(0, 0.5f,true);
                    }
                    else
                    {
                        BGM.volume = 0;
                        BGM.Pause();
                    }
                }
                m_bBGMPause = true;
            }
        }

        public void AllUnPauseBGM(bool bFade = true)
        {
            if(m_bBGMPause)
            {
                m_bBGMPause = false;
                if (BGM != null)
                {
                    if(bFade)
                    {
                        BGM.volume = 0;
                        SoundManager.Instance.ChangeBGMVolume(1, 0.5f,true);
                    }
                    else
                    {
                        BGM.volume = VolumeBGM;
                        BGM.UnPause();
                    }
                }
            }
        }

        public void AllPause(bool bFade = true)
        {
            AllPauseSFX();
            AllPauseBGM(bFade);
        }

        public void AllUnPause(bool bFade = true)
        {
            AllUnPauseSFX();
            AllUnPauseBGM(bFade);
        }

        public void Resume()
        {
            soundStateMachine.Resume();
        }
        public void Pause()
        {
            soundStateMachine.Pause();
        }
    }
}
