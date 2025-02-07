using UnityEngine;

namespace ALF.ACTOR
{
    public class BaseActor : MonoBase
    {
        // public SkeletonAnimation  m_AniChar = null;
        // public GameObject m_AniChar = null;
        protected Coroutine animationCoroutine = null;
        private string id = null;
        
        public string ID 
        {
            get{ 
                if(id == null || id == "")
                {
                    id = this.GetType().Name;
                }

                return id;
            }
            private set { id = value;}
        }

        public float Speed { get; set;}

        public void Flip(bool bFlip)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);

            if(bFlip)
            {
                scale.x *= -1;
            }
            
            transform.localScale = scale;
        }

        public bool DoMove(Vector3 dir,float dt)
        {
            if(dir == null)
            {
                return false;
            }
            
            Vector3 pos = transform.localPosition;
            transform.localPosition = pos + (dir * Speed * dt);

            return true;
        }

        public virtual void InitActor(string id,bool bSkip = false)
        {
            ID = id;
        }

        public virtual void ChangeAnimation(string animation,float delay = 0, byte dir = 0)
        {
        }

        public virtual bool IsPlaying(string animation)
        {
            ALFUtils.Assert(false,"BaseActor IsPlaying !!!");
            return false;
        }

        public virtual void StopAnimation()
        {
            if(animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
        }

        public virtual void Reset()
        {
        }

        public virtual void ChangeColor(Color color)
        {

        }

        public virtual float GetAnimationDuration(string _name)
        {
            ALFUtils.Assert(false,"BaseActor GetAnimationDuration !!!");
            return 0;
        }
    }

}
