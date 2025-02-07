using System;
// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using ALF;
using DATA;
using MATCHTEAMDATA;
using STATISTICSDATA;
using FlatBuffers;
using Newtonsoft.Json.Linq;

//using System.Text.RegularExpressions;
// using System.Linq;
namespace ALF
{
    public class MatchEngine : IBase
    { 


#if !UNITY_EDITOR && UNITY_IPHONE
        const string DLL = "__Internal";
#else
        const string DLL = "MatchEngine";
#endif

        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        static private extern IntPtr CreateMatchEngine(int width,int height,int x1,int y1,int x2,int y2);

        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        static private extern void DisposeMatchEngine(IntPtr pMatchEngine);

        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        static private extern void Init( IntPtr gameObjectNew,IntPtr transformSetPosition,IntPtr debugMessage);

        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        static private extern void UpdateTimer(IntPtr pMatchEngine,float dt);

        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        static private extern bool SetupMatchTeamData(IntPtr pMatchEngine, byte[] pMatchTeamData);
        
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        static private extern bool SetTeamData(IntPtr pMatchEngine,int index, byte[] pTeampTeamData);

        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        static private extern void NewGameMatchEngine(IntPtr pMatchEngine,int iMatchType);
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        static private extern void ResumeMatchEngine(IntPtr pMatchEngine);

        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        static private extern void FinishMatchEngine(IntPtr pMatchEngine);

        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        static private extern IntPtr GetStatisticsData(IntPtr pMatchEngine, ref int iSize);

        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        static private extern void FreeMem(IntPtr pPtr);

        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        static private extern void DebugMode(IntPtr pPtr,bool bEnable);

        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        static private extern void OnKeyDown(IntPtr pPtr,ushort usKey);
        
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static private extern bool AddBroadCastText(IntPtr pPtr,byte eEvent,int index, string strBroadCastTextData);

        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        static private extern void ChangeTacticsData(IntPtr pMatchEngine,int index, byte[] pTeamData);

        ////////////////////////////////////////////////////////////////
        // C# functions for C++ to call
        ////////////////////////////////////////////////////////////////
        [AOT.MonoPInvokeCallback(typeof(SendCommandDelegate))]
        static void SendCommand(int iCommand,string data)
        {
            MatchView pMatchView = Director.Instance.GetActiveBaseScene<MainScene>().GetInstance<MatchView>();
            switch ((E_COMMAND)iCommand)
            {
                case E_COMMAND.COMMAND_SOUND_SHOT:
                {
                    pMatchView.PlaySFX("sfx_match_ball_shoot");
                }
                break;
                case E_COMMAND.COMMAND_SOUND_PASS:
                {
                    pMatchView.PlaySFX("sfx_match_ball_pass");
                }
                break;
                case E_COMMAND.COMMAND_SOUND_HEADING:
                {
                    pMatchView.PlaySFX("sfx_match_ball_heading");
                }
                break;
                case E_COMMAND.COMMAND_GAME_BALL_CHANGE:
                {
                    pMatchView.BallChangePlayer(data);
                }
                break;
                case E_COMMAND.COMMAND_BROAD_CAST_TEXT:
                {
                    pMatchView.PlayBroadCastText(data);
                }
                break;
                case E_COMMAND.COMMAND_GAME_POSSESSION_TOTAL:
                {
                    pMatchView.PossessionTotal(data);
                }
                break;
                case E_COMMAND.COMMAND_GAME_GAMEOVER:
                {
                    pMatchView.PlayGameOver();
                }
                break;
                case E_COMMAND.COMMAND_GAME_KICKOFF:
                {
                    pMatchView.KickOff();
                }
                break;
                case E_COMMAND.COMMAND_GAME_HALFTIME:
                {
                    pMatchView.HalfTime();
                }
                    break;
                case E_COMMAND.COMMAND_GAME_TIME:
                {
                    pMatchView.SetPlayTime(data);
                }
                    break;
                case E_COMMAND.COMMAND_SUBSTITUTION_WAIT:
                case E_COMMAND.COMMAND_TATICS_WAIT:
                {
                    pMatchView.PlayWaitView(true);
                }
                    break;
                case E_COMMAND.COMMAND_TIMELINE_GOAL:
                case E_COMMAND.COMMAND_TIMELINE_SUBSTITUTION:
                case E_COMMAND.COMMAND_TIMELINE_YELLOWCARD:
                case E_COMMAND.COMMAND_TIMELINE_REDCARD:
                case E_COMMAND.COMMAND_TIMELINE_INJURY:
                {
                    if((E_COMMAND)iCommand == E_COMMAND.COMMAND_TIMELINE_SUBSTITUTION)
                    {
                        pMatchView.PlayWaitView(false);
                    }
                    pMatchView.PlayNoticePopup((E_COMMAND)iCommand,data);
                }
                    break;
                case E_COMMAND.COMMAND_GAME_SCORE:
                {
                    Director.Instance.GetActiveBaseScene<MainScene>().GetInstance<MatchView>().SetScore(data);
                }
                    break;
                default:
                    break;
            }
        }
    
       
        [AOT.MonoPInvokeCallback(typeof(TransformSetPositionDelegate))]
        static void TransformSetPosition(int iTeamIndex, int thisHandle, Vector3 position)
        {
            MatchView pMatchView = Director.Instance.GetActiveBaseScene<MainScene>().GetInstance<MatchView>();
            pMatchView.TransformSetPosition(iTeamIndex,thisHandle, position);
        }

        [AOT.MonoPInvokeCallback(typeof(DebugMessageDelegate))]
        
        static void DebugMessage( string msg)
        {
            #if UNITY_EDITOR
            if(string.IsNullOrEmpty(msg)) return;

            // // #if UNITY_EDITOR_WIN
            // //string pattern = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
            // //var match = Regex.Match(msg, pattern, "");
        
            // //bool isUnicode= UnicodeEncoding.GetEncoding(0).GetString(UnicodeEncoding.GetEncoding(0).GetBytes(msg)) != msg;
            // if(msg.Any(c => c > 255))
            // {
            //     byte[] bytesForEncoding = Encoding.Unicode.GetBytes(msg);
            //     msg = Convert.ToBase64String (bytesForEncoding );
            // }
            // else
            // {
            //    byte[] bytesForEncoding = Encoding.UTF8.GetBytes ( msg );
            //     msg = Convert.ToBase64String (bytesForEncoding );
            // }
            
            // // utf-8 디코딩
            // byte[] decodedBytes = Convert.FromBase64String (msg );

            // msg = Encoding.UTF8.GetString (decodedBytes );
            // // #endif
            // msg = ALFUtils.Utf8Convert(msg);    
            Debug.Log(msg);
            #endif
        }

        private IntPtr m_pNativeObject;
        // Variable to hold the C++ class's this pointer

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void SendCommandDelegate(int iCommand,string data);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void TransformSetPositionDelegate(int iTeamIndex,int thisHandle, Vector3 position);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void DebugMessageDelegate( string msg);

        public MatchEngine(int width,int height,int x1,int y1,int x2,int y2)
        {
		    Init(
                Marshal.GetFunctionPointerForDelegate(new SendCommandDelegate(SendCommand)),
                Marshal.GetFunctionPointerForDelegate(new TransformSetPositionDelegate(TransformSetPosition)),
                Marshal.GetFunctionPointerForDelegate(new DebugMessageDelegate(DebugMessage))
                );

            m_pNativeObject = CreateMatchEngine(width,height,x1,y1,x2,y2);
        }

        // public static MatchTeamData FromFileStream( System.IO.FileStream fs)
        // {
        //     //Create Buffer
        //     int ii = Marshal.SizeOf<MatchTeamData>();
        //     byte[] buff = new byte[Marshal.SizeOf<MatchTeamData>()]; 
        //     int amt = 0; 
        //     //Loop until we've read enough bytes (usually once) 
        //     while(amt < buff.Length)
        //         amt += fs.Read(buff, amt, buff.Length-amt); //Read bytes 
        //     //Make sure that the Garbage Collector doesn't move our buffer 
        //     GCHandle handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
        //     //Marshal the bytes
        //     // MatchEngine.MatchTeamData s = Marshal.PtrToStructure<MatchEngine.MatchTeamData>(handle.AddrOfPinnedObject()); 
        //     MatchTeamData result = (MatchTeamData)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(MatchTeamData));
       
        //     handle.Free();//Give control of the buffer back to the GC 
        //     return result;
        // }

        // void ByteToData(byte[] buffer)
        // {
        //     // //Create Buffer
        //     // int iRawSize = Marshal.SizeOf(typeof(MatchTeamData));
        //     // if(iRawSize > buffer.Length )
        //     // {
        //     //     throw new Exception();
        //     // }
            
        //     // IntPtr iptr = Marshal.AllocHGlobal(buffer.Length);
        //     // Marshal.Copy(buffer,0,iptr,buffer.Length);
            
        //     // teamDatas = (MatchTeamData)Marshal.PtrToStructure(iptr, typeof(MatchTeamData));
        //     // Marshal.FreeHGlobal(iptr);
        // }

        //구조체를 배열로 바꾸는 부분
        // byte[] Serialize()
        // {
        //     int iRawSize = Marshal.SizeOf(typeof(MatchTeamData));
        //     byte[] buffer = new byte[iRawSize];

        //     IntPtr iptr = Marshal.AllocHGlobal(iRawSize);
        //     // var gch = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        //     // var pBuffer = gch.AddrOfPinnedObject();

        //     //구조체를 바꾸는 부분
        //     Marshal.StructureToPtr(teamDatas, iptr, false);
        //     Marshal.Copy(iptr,buffer,0,iRawSize);
        //     Marshal.FreeHGlobal(iptr);
        //     // gch.Free();
        //     return buffer;
        // }

        // //배열을 구조체로
        // public object RawDeSerialize(byte[] data, Type dataType)
        // {
        //     int RawSize = Marshal.SizeOf(dataType);
        //     //만약에 데이터가 없으면 리턴값은 null값에 취한다.
        //     if (RawSize > data.Length)
        //     {
        //         return null;
        //     }
        //     IntPtr buffer = Marshal.AllocHGlobal(RawSize);
        //     Marshal.Copy(data, 0, buffer, RawSize);
        //     datapacket datapacket = new datapacket();
        //     object objData = Marshal.PtrToStructure(buffer, dataType);
        //     Marshal.FreeHGlobal(buffer);
        //     return objData;
        // }

        public void SetChangeTacticsData(int index, byte[] pTeamData)
        {
            ChangeTacticsData(m_pNativeObject,index, pTeamData);
        }

        public void SetMatchTeamData(byte[] pMatchTeamData)
        {
            // System.IO.File.WriteAllBytes(@"/Users/mac/Documents/TeamData.bytes",pMatchTeamData);
            // SetupTeamData(m_pNativeObject, byteData, byteData.Length);

            SetupMatchTeamData(m_pNativeObject, pMatchTeamData);
        }

        public void SetTeamDataByIndex(int index, byte[] pMatchTeamData)
        {
            MatchEngine.SetTeamData(m_pNativeObject,index, pMatchTeamData);
        }

        public void KeyDown(KeyCode ekey)
        {
            #if !UNITY_EDITOR 
            switch(ekey)
            {
                case KeyCode.Q:
                OnKeyDown(m_pNativeObject,'q');
                break;
            }            
            #endif
        }
        public void StartMatch()
        {
            NewGameMatchEngine(m_pNativeObject,(int)E_APP_MATCH_TYPE.APP_MATCH_TYPE_GIVEN);
            ResumeMatchEngine(m_pNativeObject);
        }

        public void FinishMatch()
        {
            FinishMatchEngine(m_pNativeObject);
        }
        public void SetDebugMode(bool bEnable)
        {
            DebugMode(m_pNativeObject,bEnable);
        }

        public void AddBroadCastTextData(E_BROADCAST_TEXT_EVENT_TYPE eEvent,uint index, string strBroadCastTextData)
        {
            AddBroadCastText(m_pNativeObject,(byte)eEvent,(int)index,strBroadCastTextData);
        }

        public MatchStatistics GetMatchStatisticsData()
        {
            int iSize = 0;
            IntPtr buffer = GetStatisticsData(m_pNativeObject,ref iSize);

            byte[] returnedResult = new byte[iSize];

            //Copy from result pointer to the C# variable
            if (buffer != IntPtr.Zero)
            {
                Marshal.Copy(buffer, returnedResult, 0, iSize);
                //Free native memory
                FreeMem(buffer);
            }
            
            return MatchStatistics.GetRootAsMatchStatistics(new ByteBuffer(returnedResult));
        } 

        public void Dispose()
        {
            if(m_pNativeObject != IntPtr.Zero)
            {
                // Call the DLL Export to dispose this class
                DisposeMatchEngine(m_pNativeObject);
                m_pNativeObject = IntPtr.Zero;
            }

            // if(bDisposing)
            // {
            //     // No need to call the finalizer since we've now cleaned
            //     // up the unmanaged memory
            //     GC.SuppressFinalize(this);
            // }
        }

        public void Update(float dt) 
        {
            UpdateTimer(m_pNativeObject,dt);
        }

        
        //static private extern IntPtr GetInternalData(IntPtr pMatchEngine);
        //static private extern IntPtr GetMatchStateMachine(IntPtr pMatchEngine);
        
        //static private extern int GetExternalState(IntPtr pMatchEngine);

    }
}