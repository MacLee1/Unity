
namespace ALF.CONDITION
{
    public enum E_CONDITION : byte 
    {
		NONE = 0,
        TIMEOUT,
        FORMTOMAX,
	}

    public interface ICondition
    {
        bool Execute(float dt);
        void Reset();
        
        E_CONDITION         ConditionEum { get;}
        // bool                FirstTick { get; set;}
        float               UpdateScale { get; set;}
        bool                Paused {get; set;}
    }

    class TimeOutCondition : ICondition
    {
        float m_fUpdateScale = 1.0f;
        bool m_bPaused = false;
        // bool m_bUpdateNextFrame = false;

        float m_elapsed = 0;
        float m_fDurationTime = 0;
        float m_fTimePercent = 0;      

        public E_CONDITION ConditionEum { get { return E_CONDITION.TIMEOUT; } }
        public float UpdateScale { get{ return m_fUpdateScale; } set{ m_fUpdateScale = value; } }
        public bool Paused { get{ return m_bPaused; } set{ m_bPaused = value; } }
        // public bool UpdateNextFrame { get{ return m_bUpdateNextFrame; } set{ m_bUpdateNextFrame = value; } }
        public TimeOutCondition(float fDurationTime)
        {
            m_fDurationTime = fDurationTime;
        }
        public bool Execute(float dt)
        {
            // if(m_bUpdateNextFrame)
            // {
            //     m_bUpdateNextFrame = false;
            //     return false;
            // }
            
            if(m_bPaused || m_fUpdateScale == 0)
                return false;
            m_elapsed += dt * m_fUpdateScale;
            
            if(m_fDurationTime == float.MaxValue) return false;

            m_fTimePercent = System.Math.Max(0, System.Math.Min(1, m_elapsed / m_fDurationTime));
            if ( m_fTimePercent == 1 || (m_fUpdateScale < 0 && m_fTimePercent == 0) )
            {
                m_elapsed = m_fDurationTime;
                return true;
            }
            else
                return false;
        }
        public void Reset()
        {
            UpdateScale = 1.0f;
            Paused = false;
            // UpdateNextFrame = false;
            m_elapsed = 0.0f;
            m_fTimePercent = 0.0f;
            m_fDurationTime = 0.0f;
        }
        public float GetElapsed()
        {
            return m_elapsed;
        }
        public float GetRemainTime()
        {
            return m_fDurationTime - m_elapsed;
        }
        public float GetTimePercent()
        {
            return m_fTimePercent;
        }

        public float GetDurationTime()
        {
            return m_fDurationTime;
        }
        public void SetRemainTime(float rt)
        {
            if(rt < 0)
            {
                m_fDurationTime = float.MaxValue;
                m_fTimePercent = 0;
            }
            else
            {
                // m_bUpdateNextFrame = rt > 0;
                m_fDurationTime = rt;
            }
        }
        public void AddRemainTime(float rt)
        {
            // m_bUpdateNextFrame = bNextFrame;
            m_fDurationTime += rt;

            if(m_bPaused || m_fUpdateScale == 0) return;

            m_fTimePercent = System.Math.Max(0, System.Math.Min(1, m_elapsed / m_fDurationTime));

            if ( m_fTimePercent == 1 || (m_fUpdateScale < 0 && m_fTimePercent == 0) )
            {
                m_elapsed = m_fDurationTime * m_fTimePercent;
            }
        }
        public void Reverse()
        {
            m_elapsed = m_fDurationTime * ( 1.0f - m_fTimePercent);
        }

        public void AddElapsed(float rt)
        {
            m_elapsed += rt;
            
            if(m_bPaused || m_fUpdateScale == 0) return;
            
            m_fTimePercent = System.Math.Max (0,System.Math.Min(1, m_elapsed / System.Math.Max(m_fDurationTime, System.Single.Epsilon)));
            
            if ( m_fTimePercent == 1 || (m_fUpdateScale < 0 && m_fTimePercent == 0) )
                m_elapsed = m_fDurationTime * m_fTimePercent;
        }
        public void CopyValue(TimeOutCondition pTimeOutCondition)
        {
            if(pTimeOutCondition != null)
            {
                m_elapsed = pTimeOutCondition.m_elapsed;
                m_fTimePercent = pTimeOutCondition.m_fTimePercent;
                m_fDurationTime = pTimeOutCondition.m_fDurationTime;
                // m_bFirstTick = pTimeOutCondition.m_bFirstTick;
                // m_bUpdateNextFrame = pTimeOutCondition.m_bUpdateNextFrame;
            }
        }
    }
}