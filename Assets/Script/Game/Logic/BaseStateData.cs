using ALF;
using ALF.STATE;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DATA;

namespace STATEDATA
{
    internal class ChangeStarStateData : IStateData
    {
        public List<Animation> AnimationList = new List<Animation>();
        public Animation CurrentAnimation {get; set;}
        public ChangeStarStateData(){}
        public void Dispose(){AnimationList.Clear(); AnimationList = null; CurrentAnimation = null;}
    }
    internal class DilogMoveStateData : IStateData
    {
        public float FadeDelta  {get; set;}
        public bool Out {get; set;}
        public float Distance {get; set;}
        public Vector3 Original {get; set;}
        public Vector3 Direction {get; set;}

        public DilogMoveStateData(){}
        public void Dispose(){}
    }

    internal class ScrollMoveStateData : DilogMoveStateData
    {
        public Transform ScrollSelectNode {get; set;} 
        public int ScrollSelectIndex {get; set;} 
        public ScrollMoveStateData(){}

        public new void Dispose()
        {
            ScrollSelectNode = null;
        }
    }

    internal class DailogStateData : IStateData
    {
        public string AnimationName {get; }
        public Animation Animation {get; private set;}

        public System.Action EndCallback {get; private set;}
        public DailogStateData(string animationName,Animation animation,System.Action endCallback)
        {
            Animation = animation;
            AnimationName = animationName;
            EndCallback = endCallback;
        }

        public void Dispose()
        {
            Animation = null;
            EndCallback = null;
        }
    }
    internal class LoopStateData : IStateData
    {
        public Vector3 Pos {get; set;}
        public LoopStateData(){}

        public void Dispose(){}
    }
    internal class FadeInOutStateData : IStateData
    {
        public float Delta {get; set;}
        public FadeInOutStateData(float fDeta)
        {
            Delta = fDeta;
        }

        public void Dispose(){}
    }
    internal class ShakeStateData : IStateData
    {
        public Vector3 cameraPos = Camera.main.transform.position;
        public float shakeTime = 0f;
        public ShakeStateData(){}

        public void Dispose(){}
    }

    internal class CurtainStateData : IStateData
    {
        public byte Step = 0;
        public bool Complete = false;
        public float Delay = -1;
        public CurtainStateData(){}

        public void Dispose(){}
    }
    
    internal class AnimationStateData : IStateData
    {
        public float Delay = -1;
        public string AnimationName = null;
        public AnimationStateData(string aniName, float delay =-1)
        {
            Delay = delay;
            AnimationName = aniName;
        }

        public void Dispose()
        {
            AnimationName = null;
        }
    }

    internal class AbilityGaugeStateData : IStateData
    {
        public byte Origin {get; set;}
        public byte Value {get; set;}
        public byte Add {get; set;}
        public float Width {get; set;}

        public AbilityGaugeStateData(byte origin,byte value,byte add)
        {
            Origin = origin;
            Value = value;
            Add = add;
        }
        public void Dispose(){}
    }

    internal class RollingNumberStateData : IStateData
    {
        public double Origin {get; set;}
        public double Add {get; set;}
        public double Current {get; set;}

        public RollingNumberStateData(double origin,double current,double add)
        {
            Origin = origin;
            Current = current;
            Add = add;
        }
        public void Dispose(){}
    }

    internal class LongTouchStateData : IStateData
    {
        public ulong Id {get; set;}
        public LongTouchStateData(ulong id)
        {
            Id = id;
        }
        public void Dispose(){}
    }

    internal class ObjectMoveStateData : IStateData
    {
        public float Time  {get; set;}
        public byte Dir  {get; set;}
        public bool Idle {get; set;}
        // public float Delay {get; set;}
        public List<Transform> Positons {get; set;}
        public Vector3 Next {get; set;}
        public Vector3 Direction {get; set;}
        public float Distance {get; set;}
        public RectTransform FrontMove {get; set;}
        public int Key {get; set;}
        
        public ObjectMoveStateData(){}
        public void Dispose()
        {
            Positons.Clear();
            Positons = null;
            FrontMove = null;
        }
    }
    internal enum E_STATE_TYPE : byte 
    {
        Slpash = 0,
        SceneLoaging,
        Loop, 
        Timer,
        Shake,
        FadeInOutCurtain,
        ShowDailog, 
        HideDailog,
        PlayAndHide,
        PlayAndEnable,
        AbilityGauge,
        MatchUpdate,
        ShowNoticePopup, 

        ScrollMove,
        ObjectMove,
        ShowStartPopup,
        AD,
    };
    

}