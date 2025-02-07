using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALF
{
    public class AssetData : ALF.MonoBase 
    {
        public AFPool afpool;

        public override void OnDispose()
        {
            // afpool의 포인터를 넘기고 사라진다.
            // afpool != null인 경우만 삭제 한다..
            if(afpool != null)
            {
                afpool.Dispose();
            }
            afpool = null;
        }
    }
}