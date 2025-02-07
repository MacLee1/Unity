
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Security.Cryptography;
using System.IO;
using UnityEngine.Networking;
// using System.Net;
// using System.Net.Sockets;
using System.Linq;


#if UNITY_IOS || UNITY_IPHONE
#if !USE_HIVE
using Unity.Notifications.iOS;
#endif
using System.Runtime.InteropServices;
#endif

namespace ALF
{
    class FwordNode
    {
        Hashtable m_Hash = new Hashtable();
        bool m_IsLeafNode;

        // -------------------------------------------------------------------------------------------------
        public bool IsLeafNode
        {
            get { return m_IsLeafNode; }
            set { m_IsLeafNode = value; }
        }

        // -------------------------------------------------------------------------------------------------
        public FwordNode AddChild(char ch)
        {
            FwordNode resultNode = FindChild(ch);

            if (resultNode == null)
            {
                resultNode = new FwordNode();
                m_Hash.Add(ch, resultNode);
            }
            return resultNode;
        }

        // -------------------------------------------------------------------------------------------------
        public FwordNode FindChild(char ch)
        {
            foreach (var key in m_Hash.Keys)
            {
                if (key.Equals(ch))
                    return m_Hash[key] as FwordNode;
            }
            return null;
        }

        public void Clear()
        {
            FwordNode pFwordNode = null;
            foreach (var key in m_Hash.Keys)
            {
                pFwordNode = m_Hash[key] as FwordNode;
                pFwordNode.Clear();
            }

            m_Hash.Clear();
        }
    }

    class FwordFilter
    {
        FwordNode m_Root = new FwordNode();

        // -------------------------------------------------------------------------------------------------
        public string Filter(string original)
        {
            string result = original;
            string convert = ConversionString(original);

            for (int start = 0; start < convert.Length; )
            {
                int count = Match(convert.Substring(start, convert.Length - start));
                if (count > 0)
                {
                    result = Replace(result.ToCharArray(), start, count);
                    start += count;
                }
                else start++;
            }
            return result;
        }

        public bool IsFilter(string original)
        {
            string result = original;
            string convert = ConversionString(original);

            for (int start = 0; start < convert.Length; )
            {
                int count = Match(convert.Substring(start, convert.Length - start));
                if (count > 0)
                {
                    return true;
                }
                else start++;
            }
            return false;
        }

        // -------------------------------------------------------------------------------------------------

        public void ClearAllFwordList()
        {
            m_Root.Clear();
        }


        public void AddFword(string slang)
        {
            if (string.IsNullOrEmpty(slang)) return;
            string convert = ConversionString(slang);

            FwordNode currentNode = m_Root;
            for (int i = 0; i < convert.Length; i++)
            {
                if (currentNode != null)
                    currentNode = currentNode.AddChild(convert[i]);
            }

            if (currentNode != null)
                currentNode.IsLeafNode = true;
        }

        int Match(string text)
        {
            FwordNode currentNode = m_Root;

            int matchCount = 0;
            for (int i = 0; i < text.Length; i++)
            {
                currentNode = currentNode.FindChild(text[i]);
                if (currentNode == null)
                    return matchCount;

                if (currentNode.IsLeafNode)
                    matchCount = i + 1;
            }
            return matchCount;
        }

        // -------------------------------------------------------------------------------------------------
        string Replace(char[] charArray, int start, int count)
        {
            for (int i = start; i < start + count; i++)
                charArray[i] = '*';

            return new string(charArray);
        }

        // -------------------------------------------------------------------------------------------------
        string ConversionString(string text)
        {
            return text.ToUpper();
        }
    }

    public static class ALFUtils
    {
        static FwordFilter fwordFilter = new FwordFilter();
        private static System.Random rng = new System.Random();
        public static CultureInfo CI = null;

#if UNITY_EDITOR
    #if UNITY_ANDROID
        static string versionCode = UnityEditor.PlayerSettings.Android.bundleVersionCode.ToString();
    #elif UNITY_IOS
        static string versionCode = UnityEditor.PlayerSettings.iOS.buildNumber;
    #endif
#else
        static string versionCode = null;
#endif
        

        static  DateTime googleTime = DateTime.Now;
        static  DateTime luncherTime = DateTime.Now;

        public static string Utf8Convert(string msg)
        {
            if(string.IsNullOrEmpty(msg)) return null;

            // #if UNITY_EDITOR_WIN
            //string pattern = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
            //var match = Regex.Match(msg, pattern, "");
        
            //bool isUnicode= UnicodeEncoding.GetEncoding(0).GetString(UnicodeEncoding.GetEncoding(0).GetBytes(msg)) != msg;
            if(msg.Any(c => c > 255))
            {
                byte[] bytesForEncoding = Encoding.Unicode.GetBytes(msg);
                msg = Convert.ToBase64String (bytesForEncoding );
            }
            else
            {
               byte[] bytesForEncoding = Encoding.UTF8.GetBytes ( msg );
                msg = Convert.ToBase64String (bytesForEncoding );
            }
            
            // utf-8 디코딩
            byte[] decodedBytes = Convert.FromBase64String (msg );
            msg = Encoding.UTF8.GetString (decodedBytes ); 
            return msg;
        }
        public static string Utf16ToUtf8(string utf16String)
        {
            /**************************************************************
            * Every .NET string will store text with the UTF16 encoding, *
            * known as Encoding.Unicode. Other encodings may exist as    *
            * Byte-Array or incorrectly stored with the UTF16 encoding.  *
            *                                                            *
            * UTF8 = 1 bytes per char                                    *
            *    ["100" for the ansi 'd']                                *
            *    ["206" and "186" for the russian '?']                   *
            *                                                            *
            * UTF16 = 2 bytes per char                                   *
            *    ["100, 0" for the ansi 'd']                             *
            *    ["186, 3" for the russian '?']                          *
            *                                                            *
            * UTF8 inside UTF16                                          *
            *    ["100, 0" for the ansi 'd']                             *
            *    ["206, 0" and "186, 0" for the russian '?']             *
            *                                                            *
            * We can use the convert encoding function to convert an     *
            * UTF16 Byte-Array to an UTF8 Byte-Array. When we use UTF8   *
            * encoding to string method now, we will get a UTF16 string. *
            *                                                            *
            * So we imitate UTF16 by filling the second byte of a char   *
            * with a 0 byte (binary 0) while creating the string.        *
            **************************************************************/

            // Get UTF16 bytes and convert UTF16 bytes to UTF8 bytes
            byte[] utf16Bytes = Encoding.Unicode.GetBytes(utf16String);
            byte[] utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, utf16Bytes);
            char[] chars = (char[])Array.CreateInstance(typeof(char), utf8Bytes.Length);

            for (int i = 0; i < utf8Bytes.Length; i++)
            {
                chars[i] = BitConverter.ToChar(new byte[2] { utf8Bytes[i], 0 }, 0);
            }

            // Return UTF8
            return new string(chars);
        }

        /// <summary>
        /// 이진 탐색을 수행한다.
        /// </summary>
        /// <param name="datas">배열</param>
        /// <param name="data">검색할 데이터</param>
        /// <returns></returns>
        public static T BinarySearch<T>(List<T> datas,System.Func<T,int> pIComparer,ref int index)
        {
            if(datas.Count > 1)
            {
                int right = datas.Count - 1;
                int left = 0;
                int middle = 0;
                int comparer = 0;
                for (left = 0; left <= right; )
                {
                    middle = (left + right) / 2;
                    comparer = pIComparer(datas[middle]);
                    index = middle;
                    if(comparer < 0)//if (data > datas[middle])
                    {
                        left = middle + 1; 
                    }
                    else if(comparer > 0)//else if (data < datas[middle])
                    {
                        right = middle - 1; 
                    } 
                    else
                    {
                        return datas[middle];
                    }
                }
            }
            else if(datas.Count == 1)
            {
                if(pIComparer(datas[0]) == 0)
                {
                    return datas[0];
                }
            }
            
            // if(bNear)
            // {
            //     return datas[left] > datas[right] ? right: left;
            // }
            //발견되지 않음
            return default(T);
        }
        public static int BinarySearch(uint[] datas,int data ,bool bNear = false)
        {
            int right = datas.Length - 1;
            int left = 0;
            int middle = 0;
            for (left = 0; left <= right; )
            {
                middle = (left + right) / 2;
                if (data > datas[middle])
                {
                    left = middle + 1; 
                }
                else if (data < datas[middle])
                {
                    right = middle - 1; 
                } 
                else
                {
                    return middle; //x==datas[middle]
                }
            }
            
            if(bNear)
            {
                return datas[left] > datas[right] ? right: left;
            }
            //발견되지 않음
            return -1;
        }

        public static void RandomSelect( List<int> list, Action<int> doComplete)
        {
            int i = -1;
            if(list.Count > 0)
            {
                int rate = UnityEngine.Random.Range(1,list[list.Count-1]+1);
                for(i =0; i < list.Count; ++i)
                {
                    if(rate <= list[i])
                    {
                        break;
                    }
                }
            }

            if(doComplete != null)
            {
                doComplete(i);
            }
        }

        public static string EncryptString(string InputText, string Password)
        {
            // Rihndael class를 선언하고, 초기화
            RijndaelManaged RijndaelCipher = new RijndaelManaged();

            // 입력받은 문자열을 바이트 배열로 변환
            byte[] PlainText = System.Text.Encoding.Unicode.GetBytes(InputText);

            // 딕셔너리 공격을 대비해서 키를 더 풀기 어렵게 만들기 위해서 
            // Salt를 사용한다.
            byte[] Salt = Encoding.ASCII.GetBytes(Password.Length.ToString());

            // PasswordDeriveBytes 클래스를 사용해서 SecretKey를 얻는다.
            PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(Password, Salt);

            // Create a encryptor from the existing SecretKey bytes.
            // encryptor 객체를 SecretKey로부터 만든다.
            // Secret Key에는 32바이트
            // (Rijndael의 디폴트인 256bit가 바로 32바이트입니다)를 사용하고, 
            // Initialization Vector로 16바이트
            // (역시 디폴트인 128비트가 바로 16바이트입니다)를 사용한다.
            ICryptoTransform Encryptor = RijndaelCipher.CreateEncryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));

            // 메모리스트림 객체를 선언,초기화 
            MemoryStream memoryStream = new MemoryStream(); 

            // CryptoStream객체를 암호화된 데이터를 쓰기 위한 용도로 선언
            CryptoStream cryptoStream = new CryptoStream(memoryStream, Encryptor, CryptoStreamMode.Write);

            // 암호화 프로세스가 진행된다.
            cryptoStream.Write(PlainText, 0, PlainText.Length);

            // 암호화 종료
            cryptoStream.FlushFinalBlock();

            // 암호화된 데이터를 바이트 배열로 담는다.
            byte[] CipherBytes = memoryStream.ToArray();

            // 스트림 해제
            memoryStream.Close();
            cryptoStream.Close();       

            // 암호화된 데이터를 Base64 인코딩된 문자열로 변환한다.
            string EncryptedData = Convert.ToBase64String(CipherBytes);

            // 최종 결과를 리턴
            return EncryptedData;
        }

        public static string DecryptString(string InputText, string Password)
        {
            RijndaelManaged  RijndaelCipher = new RijndaelManaged();

            byte[] EncryptedData = Convert.FromBase64String(InputText);
            byte[] Salt = Encoding.ASCII.GetBytes(Password.Length.ToString());       

            PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(Password, Salt);

            // Decryptor 객체를 만든다.
            ICryptoTransform Decryptor = RijndaelCipher.CreateDecryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));

            MemoryStream  memoryStream = new MemoryStream(EncryptedData);           

            // 데이터 읽기(복호화이므로) 용도로 cryptoStream객체를 선언, 초기화
            CryptoStream  cryptoStream = new CryptoStream(memoryStream, Decryptor, CryptoStreamMode.Read);

            // 복호화된 데이터를 담을 바이트 배열을 선언한다.
            // 길이는 알 수 없지만, 일단 복호화되기 전의 데이터의 길이보다는
            // 길지 않을 것이기 때문에 그 길이로 선언한다.
            byte[] PlainText = new byte[EncryptedData.Length];       

            // 복호화 시작
            int DecryptedCount = cryptoStream.Read(PlainText, 0, PlainText.Length);               

            memoryStream.Close();
            cryptoStream.Close();

            // 복호화된 데이터를 문자열로 바꾼다.
            string DecryptedData = Encoding.Unicode.GetString(PlainText, 0, DecryptedCount);

            // 최종 결과 리턴
            return DecryptedData;
        }

        public static string FormatCurrency(float amount)
        {
            if (CI != null)
            {
                // Debug.Log("CI != null:" + CI.NumberFormat.CurrencySymbol);
                return string.Format(CI, "{0:C}", amount);
            }
            else
            {
                // Debug.Log("CI == null");
                return amount.ToString();
            }
        }
            
        public static void FindSymbolCurrency(string symbol)
        {
            if (CI == null)
            {
                if (symbol.Contains("US"))
                {
                    symbol = "$";
                }

                foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
                {
                    if( ci.NumberFormat.CurrencySymbol == symbol)
                    {
                        CI = ci;
                        break;
                    }
                }
            }
        }

        #if !UNITY_IOS
        public static string ConvertMoney (string money,string currency)
        {
            if (CI == null)
            {
                
                foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
                {
                    RegionInfo r = new RegionInfo(ci.LCID);

                    if(r != null && r.CurrencySymbol.ToUpper() == currency.ToUpper())
                    {
                        CI = ci;
                        break;
                    }
                }   
                    
                if (CI == null)
                    return null;
            }

            if (string.IsNullOrEmpty(money))
                return null;

            int numValue;
            int indexStart = 0;
            while (Int32.TryParse(money[indexStart].ToString(), out numValue) == false)
            {
                ++indexStart;
                if(indexStart == money.Length)
                    return null;
            }

            int indexEnd = money.Length;
            while (Int32.TryParse(money[indexEnd -1].ToString(), out numValue) == false)
            {
                --indexEnd;
                if(indexEnd < 2)
                    return null;
            }

            money = money.Substring(indexStart, indexEnd - indexStart);

            string[] list = money.Split(CI.NumberFormat.NumberDecimalSeparator[0]);
            if(list.Length == 0 || list.Length > 2 )
                return null;

            string[] list1 = list[0].Split(CI.NumberFormat.NumberGroupSeparator[0]);
            StringBuilder sb = new StringBuilder();
            numValue = 0;
            for (int i = 0; i < list1.Length; ++i)
            {
                if (Int32.TryParse(list1[i].ToString(), out numValue))
                    sb.Append(list1[i]);
            }

            if (sb.Length <= 0)
                return null;

            string temp = sb.ToString();
            numValue = 0;

            for (int i = 0; i < temp.Length; ++i)
            {
                if (Int32.TryParse(temp[i].ToString(), out numValue) == false)
                    return null;
            }

            if (list.Length > 1)
            {
                sb.Append(".");
                sb.Append(list[1]);
            }

            return sb.ToString();
        }
        #endif
        
        public static Bounds GetBoundsRecursive ( GameObject go, bool keepCenter = false, bool bChildHide = true)
        {
            Bounds bounds = default(Bounds);
            Renderer[] rendererList = go.GetComponentsInChildren<Renderer>( true );

            for(int i  =0; i <rendererList.Length; ++i)
            {
                if(bChildHide)
                {
                    if ( !rendererList[i].enabled || rendererList[i].gameObject.activeSelf == false || rendererList[i].GetComponent<ParticleSystem>() != null) 
                        continue;
                }

                if (bounds == default(Bounds)) bounds = new Bounds(rendererList[i].bounds.center, Vector3.zero);
                bounds.Encapsulate(rendererList[i].bounds.max);
                bounds.Encapsulate(rendererList[i].bounds.min);
            }

            if (keepCenter) bounds.center = go.transform.position - (bounds.center - go.transform.position); 

            return bounds;
        }

        static readonly Vector3[] boxExtents = new Vector3[] 
        {
            new Vector3(-1, -1, -1),
            new Vector3( 1, -1, -1),
            new Vector3(-1,  1, -1),
            new Vector3( 1,  1, -1),
            new Vector3(-1, -1,  1),
            new Vector3( 1, -1,  1),
            new Vector3(-1,  1,  1),
            new Vector3( 1,  1,  1),
        };

        static void GetRendererBoundsInChildren( Matrix4x4 rootWorldToLocal, Vector3[] minMax, Transform t, bool includeAllChildren)
        {
            MeshFilter mf = t.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null) 
            {
                Bounds b = mf.sharedMesh.bounds;
                Matrix4x4 relativeMatrix = rootWorldToLocal * t.localToWorldMatrix;
                for (int j = 0; j < 8; ++j) 
                {
                    Vector3 localPoint = b.center + Vector3.Scale(b.extents, boxExtents[j]);
                    Vector3 pointRelativeToRoot = relativeMatrix.MultiplyPoint(localPoint);
                    minMax[0] = Vector3.Min(minMax[0], pointRelativeToRoot);
                    minMax[1] = Vector3.Max(minMax[1], pointRelativeToRoot);
                }
            }

            for (int i = 0; i < t.childCount; ++i) 
            {
                Transform child = t.GetChild(i);
                GetRendererBoundsInChildren(rootWorldToLocal, minMax,  child, includeAllChildren);
            }
        }

        // look for an object bounds
        public static Bounds findObjectBounds(GameObject obj)
        {
            // includes all mesh types (filter; renderer; skinnedRenderer)
            Renderer ren = obj.GetComponent<Renderer>();
            if(ren == null)
                ren = obj.GetComponentInChildren<Renderer>();
            
            if(ren != null)
                return ren.bounds;//ren.bounds	{Center: (394.1, 3.4, -30.0), Extents: (0.4, 0.4, 0.1)}	UnityEngine.Bounds
            
    //		Debug.LogError("Your prefab" + obj.ToString() + "needs a mesh to scale!!!");
            return new Bounds(Vector3.zero,Vector3.zero); // fail safe
        }
        
        public static float ToColor(string hexString, int colorIndex)
        {
            if (hexString.Length != 8)
                throw new ArgumentException("Color hexidecimal length must be 8, recieved: " + hexString, "hexString");
            return Convert.ToInt32(hexString.Substring(colorIndex * 2, 2), 16) / (float)255;
        }

        public static float RADIANS_TO_DEGREES(float angle)
        {
            return angle * Mathf.Rad2Deg;
        }

        public static float DEGREES_TO_RADIANS(float angle)
        {
            return angle * Mathf.Deg2Rad;
        }


        // listAddTo list에 listAddTargets item들을 중복 없이 add 한다.
        public static void AddItemWithoutConflict<T>(ref List<T> listAddTo, List<T> listAddTargets)
        {
            if(null==listAddTo || null==listAddTargets)
                return;

            for(int g = 0; g < listAddTargets.Count; ++g)
            {
                if(false == listAddTo.Contains( listAddTargets[g] ))
                    listAddTo.Add( listAddTargets[g] );
            }
        }

        public static T GetPropValue<T>(object target, string name)
        {
            FieldInfo fi = target.GetType().GetField(name);
            if(fi != null)
                return (T)fi.GetValue(target);
            PropertyInfo pi = target.GetType().GetProperty(name);
            if(pi != null)
                return (T)pi.GetValue(target, null);
            return default(T);
        }

        public static void SetPropValue<T>(object target, string name, T val)
        {
            FieldInfo fi = target.GetType().GetField(name);
            //Debugger.Assert(fi != null, "SetPropValue Field ["+name+"] not found");
            if(fi != null)
                fi.SetValue(target, val);
            PropertyInfo pi = target.GetType().GetProperty(name);
            if(pi != null)
                pi.SetValue(target, val, null);
        }

        public static void NormalizeDirection( ref Vector3 src)
        {
            if(src.x > 0)
            {
                src.x = 1;
            }
            else if (src.x < 0)
            {
                src.x = -1;
            }

            if(src.y > 0)
            {
                src.y = 1;
            }
            else if (src.y < 0)
            {
                src.y = -1;
            }

            if(src.z > 0)
            {
                src.z = 1;
            }
            else if (src.z < 0)
            {
                src.z = -1;
            }
        }
        /**
        *  게임 오브젝트 페이드 처리 (객체의 자식까지 일괄처리)
        */
        public static void FadeObject(Transform target,float alpha)
        {
            if(target == null) return;

            CanvasRenderer[] renderers = target.GetComponentsInChildren<CanvasRenderer>();
            float a = 0;
            for(int i = 0; i < renderers.Length; ++i)
            {
                a = renderers[i].GetAlpha() + alpha;
                if(a > 1)
                {
                    a = 1;
                }
                else if (a < 0)
                {
                    a = 0;
                }
                renderers[i].SetAlpha(a);
            }        
        }

        public static void Assert(bool condition,string msg)
        {
            UnityEngine.Assertions.Assert.IsTrue(condition,msg);
        }

        public static void CreateIsometricTile(int w, int h, MeshFilter meshFilter,Color color,Vector2 uvOffset,float sizeW,float sizeH)
        {
            if(meshFilter == null)
            {
                return;
            }

            if(meshFilter.sharedMesh != null)
            {
                meshFilter.sharedMesh.Clear();
                meshFilter.sharedMesh = null;
            }

            Mesh mesh = new Mesh();
            
            float u = 0.5f * w;
            float v = 0.5f * h;
            int W = w -1;
            int H = h -1;

            mesh.vertices = new [] { new Vector3(sizeW+(W* sizeW),(W* sizeH) + sizeH,0),new Vector3((W - H)*sizeW,sizeW+(W + H)*sizeH,0),new Vector3(-sizeW-(H*sizeW),(H*sizeH) + sizeH,0),Vector3.zero };
            mesh.tangents = new [] { new Vector4(0.9f,0.5f,0,1),new Vector4(0.9f,0.5f,0,1),new Vector4(0.9f,0.5f,0,1),new Vector4(0.9f,0.5f,0,1)};
            mesh.normals = new [] { new Vector3(0,0,1),new Vector3(0,0,1),new Vector3(0,0,1),new Vector3(0,0,1)};
            mesh.uv = new [] {new Vector2(u,0) + uvOffset,new Vector2(u,v) + uvOffset,new Vector2(0,v) + uvOffset,uvOffset};
            mesh.colors = new [] { color,color,color,color };
            mesh.triangles = new []{0,1,2,0,2,3};

            meshFilter.sharedMesh = mesh;
        }

        public static void CreateTile(int w, int h, MeshFilter meshFilter,Color color,Vector2 uvOffset,bool bZ = true)
        {
            if(meshFilter == null)
            {
                return;
            }

            if(meshFilter.sharedMesh != null)
            {
                meshFilter.sharedMesh.Clear();
                meshFilter.sharedMesh = null;
            }

            Mesh mesh = new Mesh();
            
            float u = 0.5f * w;
            float v = 0.5f * h;
            int W = (int)((w -1) * 2);
            int H = (int)((h -1) * 2);
            if(bZ)
            {
                mesh.vertices = new [] { new Vector3(1+W,-0.1f,-1),new Vector3(-1,-0.1f,-1),new Vector3(-1,-0.1f,1+H),new Vector3(1+W,-0.1f,1+H) };
            }
            else
            {
                // mesh.vertices = new [] { new Vector3(2+W,0,0.1f),new Vector3(0,0,0.1f),new Vector3(0,2+H,0.1f),new Vector3(2+W,2+H,0.1f) };
                mesh.vertices = new [] { new Vector3(1+W,-1,0.1f),new Vector3(-1,-1,0.1f),new Vector3(-1,1+H,0.1f),new Vector3(1+W,1+H,0.1f) };
            }

            mesh.tangents = new [] { new Vector4(-0.5f,0,-0.9f,1),new Vector4(-0.5f,0,-0.9f,1),new Vector4(-0.5f,0,-0.9f,1),new Vector4(-0.5f,0,-0.9f,1)};
            mesh.normals = new [] { new Vector3(0,1,0),new Vector3(0,1,0),new Vector3(0,1,0),new Vector3(0,1,0)};
            mesh.uv = new [] {new Vector2(u,0) + uvOffset,new Vector2(u,v) + uvOffset,new Vector2(0,v) + uvOffset,uvOffset};
            
            mesh.colors = new [] { color,color,color,color };
            mesh.triangles = new []{0,1,2,0,2,3};

            meshFilter.sharedMesh = mesh;
        }

        public static void CreateCircle(int iCircleSegmentCount, float fScale, MeshFilter meshFilter,Color color,Vector2 uvOffset,bool bZ = true)
        {
            if(meshFilter == null)
            {
                return;
            }

            if(meshFilter.sharedMesh != null)
            {
                meshFilter.sharedMesh.Clear();
                meshFilter.sharedMesh = null;
            }

            int iCircleVertexCount = iCircleSegmentCount + 2;
            int iCircleIndexCount = iCircleSegmentCount * 3;

            Mesh mesh = new Mesh();
            List<Vector3> vertices = new List<Vector3>(iCircleVertexCount);
            List<Color> _color = new List<Color>(iCircleVertexCount);
            int[] indices = new int[iCircleIndexCount];
            //int[] indices = new int[iCircleIndexCount];
            float segmentWidth = Mathf.PI * 2f / iCircleSegmentCount;
            float angle = 0f;
            vertices.Add(Vector3.zero);
            _color.Add(color);
            int j = 0;
            for (int i = 1; i < iCircleVertexCount; ++i)
            {
                if(bZ)
                {
                    vertices.Add(new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * fScale);
                }
                else
                {
                    vertices.Add(new Vector3(Mathf.Cos(angle),  Mathf.Sin(angle), 0f) * fScale);
                }
                _color.Add(color);
                angle -= segmentWidth;
                if (i > 1)
                {
                    j = (i - 2) * 3;
                    indices[j + 0] = 0;
                    indices[j + 1] = i - 1;
                    indices[j + 2] = i;
                }
            }
            mesh.SetVertices(vertices);
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            mesh.SetColors(_color);
            mesh.RecalculateBounds();
            
            // float u = 0.5f * w;
            // float v = 0.5f * h;
            // int W = (int)((w -1) * 2);
            // int H = (int)((h -1) * 2);
            // if(bZ)
            // {
            //     mesh.vertices = new [] { new Vector3(1+W,-0.1f,-1),new Vector3(-1,-0.1f,-1),new Vector3(-1,-0.1f,1+H),new Vector3(1+W,-0.1f,1+H) };
            // }
            // else
            // {
            //     mesh.vertices = new [] { new Vector3(2+W,0,0.1f),new Vector3(0,0,0.1f),new Vector3(0,2+H,0.1f),new Vector3(2+W,2+H,0.1f) };
            // }

            // mesh.tangents = new [] { new Vector4(-0.5f,0,-0.9f,1),new Vector4(-0.5f,0,-0.9f,1),new Vector4(-0.5f,0,-0.9f,1),new Vector4(-0.5f,0,-0.9f,1)};
            // mesh.normals = new [] { new Vector3(0,1,0),new Vector3(0,1,0),new Vector3(0,1,0),new Vector3(0,1,0)};
            // mesh.uv = new [] {new Vector2(u,0) + uvOffset,new Vector2(u,v) + uvOffset,new Vector2(0,v) + uvOffset,uvOffset};
            
            // mesh.triangles = new []{0,1,2,0,2,3};
            meshFilter.sharedMesh = mesh;

        //     float deltaTheta = (float) (2.0 * Mathf.PI) / numSegments;
        //     float theta = 0f;

        //     for (int i = 0 ; i < numSegments + 1 ; i++) {
        //         float x = radius * Mathf.Cos(theta);
        //         float z = radius * Mathf.Sin(theta);
        //     Vector3 pos = new Vector3(x, 0, z);
        //         lineRenderer.SetPosition(i, pos);
        //         theta += deltaTheta;
        // }
        }

        public static bool IsInside(Vector3 p,Vector3 a, Vector3 b, Vector3 c)
        {
            return IsSameSide(a,b,c,p) && IsSameSide(b,c,a,p) && IsSameSide(c,a,b,p);
        }

        public static bool IsSameSide(Vector3 a, Vector3 b, Vector3 c,Vector3 p)
        {
            Vector3 c1 = Vector3.Cross(b - a, c -a );
            Vector3 c2 = Vector3.Cross(b - a, p-a );
            return Vector3.Dot(c1,c2) >= 0;
        }

        public static string TimeToString(DateTime dateTime)
        {
            return dateTime.ToString("hh:mm tt", CultureInfo.InvariantCulture);
        }

        public static string TimeToString(long dateTime, bool bS)
        {
            DateTime date = new DateTime(dateTime);
            return date.ToString(bS ? "hh:mm:ss" : "hh:mm", CultureInfo.InvariantCulture);
        }

        public static string SecondToString(int second, bool bS,bool bM,bool bH)
        {
            int d = (second / 86400);
            string time = "";
            
            if( d > 0)
            {
                time = string.Format("{0}", d);
            }
            
            int h = (second / 3600) % 24;
            
            if(h > 0 || d > 0)
            {
                time = string.Format( string.IsNullOrEmpty(time) ? "{0:00}": "{1}:{0:00}", h,time);
            }
            else
            {
                if(bH)
                {
                    time = string.Format( string.IsNullOrEmpty(time) ? "00": "{0:00}:00", time);
                }
            }
            
            int m = (second / 60) % 60;
            if(m > 0 || h > 0 || d > 0)
            {
                time = string.Format( string.IsNullOrEmpty(time) ? "{0:00}": "{1}:{0:00}", m,time);
            }
            else
            {
                if(bM)
                {
                    if(bH)
                    {
                        time = string.Format( "{0:00}:00", time);
                    }
                    else
                    {
                        time = string.Format( string.IsNullOrEmpty(time) ? "00": "{0}:00:{1:00}", time,m);
                    }
                }
            }
            
            if(bS)
            {
                int s = second % 60;
                time = string.Format( string.IsNullOrEmpty(time) ? "{0:00}": "{1}:{0:00}", s,time);
            }
            
            return time;
        }

        public static string TimeToString(int second,bool bEtc,bool bH = true,bool bM = true,bool bS = true,bool bMark = false)
        {
            int d = (second / 86400);
            string time = "";
            int index =0;
            
            if( d > 0)
            {
                time = string.Format("{0}{1}", d,bMark ? "d ":":");
                if(bMark)
                {
                    ++index;
                }

                if(bEtc) return time;
            }
            
            int h = (second / 3600) % 24;
            
            if(bH && (h > 0 || d > 0))
            {
                time = string.Format("{2}{0:00}{1}", h, bMark ? "h ":":",time);
                if(bMark)
                {
                    ++index;
                }

                if(bEtc) return time;
            }
            
            int m = (second / 60) % 60;
            if(bM && (m > 0 || h > 0 || d > 0))
            {
                time = string.Format("{2}{0:00}{1}", m,bMark ? "m ":":",time);
                if(bMark)
                {
                    ++index;
                }

                if(bEtc) return time;
            }
            
            if(bS)
            {
                int s = second % 60;
                time = string.Format("{2}{0:00}{1}",s,bMark?"s ":"",time);
                if(bMark)
                {
                    ++index;
                }
            }
            
            return time;
        }

        public static List<int> Time2String(int second,ref int index)
        {
            List<int> list = new List<int>();

            int d = (second / 86400);
            
            index =-1;
            
            if( d > 0)
            {
                index =1;
                list.Add(d);
            }
            
            int h = (second / 3600) % 24;

            if(h > 0 || d > 0)
            {
                list.Add(h);
                index =2;
                if(list.Count == 2) return list;
            }

            int m = (second / 60) % 60;

            if(m > 0 || h > 0 || d > 0)
            {
                list.Add(m);
                index =3;
                if(list.Count == 2) return list;
            }
            
            int s = second % 60;
            if(s > 0 || m > 0 || h > 0 || d > 0)
            {
                list.Add(s);
                index =4;
            }
            
            return list;
        }

        // static IEnumerator coGoogleTimeServerTime()
        // {
        //     UnityWebRequest request = new UnityWebRequest();
        //     using( request  = UnityWebRequest.Get("www.time.google.com"))
        //     {
        //         yield return request.SendWebRequest();

        //         if(request.result == UnityWebRequest.Result.ConnectionError)
        //         {
        //             Debug.Log("-------------------request.isNetworkError:!!!");
        //         }
        //         else
        //         {
        //             // string data = request.GetResponseHeader("data");
        //             Debug.Log("------------------------:"+ request.downloadHandler.text);
        //         }
        //     }
        // }

        #if UNITY_IOS || UNITY_IPHONE
            
        public static IEnumerator InitNotification()
        {
#if USE_HIVE
            yield return null;
#else
            var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge;
            using (var req = new AuthorizationRequest(authorizationOption, true))
            {
                while (!req.IsFinished)
                {
                    yield return null;
                };

                // string res = "\n RequestAuthorization:";
                // res += "\n finished: " + req.IsFinished;
                // res += "\n granted :  " + req.Granted;
                // res += "\n error:  " + req.Error;
                // res += "\n deviceToken:  " + req.DeviceToken;
                // Debug.Log(res);
            }
#endif
        }
        #endif

        // public static DateTime GoogleTimeServerTime()
        // {
        //     luncherTime = DateTime.Now;
        //     googleTime = DateTime.Now;
        //     // try
        //     // {
        //     //     const string ntpServer = "time.google.com";
        //     //     var ntpData = new byte[48];
        //     //     ntpData[0] = 0x1B; //LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)

        //     //     var addresses = Dns.GetHostEntry(ntpServer).AddressList;
        //     //     var ipEndPoint = new IPEndPoint(addresses[0], 123);
        //     //     var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        //     //     socket.Connect(ipEndPoint);
        //     //     socket.Send(ntpData);
        //     //     socket.Receive(ntpData);
        //     //     socket.Close();

        //     //     ulong intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | (ulong)ntpData[43];
        //     //     ulong fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | (ulong)ntpData[47];

        //     //     var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
        //     //     googleTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);

        //     //     return googleTime;
        //     // }
        //     // catch
        //     // {
        //     //     return googleTime;
        //     // }
        //     return googleTime;
        // }

        public static DateTime GetGoogleTimeServerTime()
        {
            return googleTime;
        }

        public static DateTime GetLuncherTime()
        {
            return luncherTime;
        }

        
        // public static void GoogleTimeServerTime()
        // {
        //     Director.Runner.StartCoroutine(coGoogleTimeServerTime());
            
        //     // DateTime date = new DateTime(dateTime);
        //     // return date.ToString("hh:mm tt", CultureInfo.InvariantCulture);

        //     // SNTPClient client;

        //     // try
        //     // {
        //     //     client = new SNTPClient("time.google.com"); // Google NTP 서버 주소 추가
        //     //     //client = new SNTPClient("time.nuri.net");    // 참조할 NTP 서버 주소
        //     //     client.Connect(false);
        //     // }
        //     // catch (Exception e)
        //     // {
        //     //     Console.WriteLine("ERROR: {0}", e.Message);

        //     // }


        //     // Console.Write(client.ToString());
        // }

        public static string NumberToString(ulong number)
        {
            string strNum = string.Format("{0}",number);
            
            int numPos = strNum.Length;
            if(numPos <= 3)
            {
                return strNum;
            }
            
            string token = "";
            int numCount = 0;

            if(numPos < 7)
            {
                numCount = 3;
                token = "K";
            }
            else if(numPos < 10)
            {
                numCount = 6;
                token = "M";
            }
            else if(numPos < 13)
            {
                numCount = 9;
                token = "B";
            }
            else
            {
                numCount = 12;
                token = "T";
            }
            
            int count = numPos - numCount;
            int tempNum = int.Parse(strNum.Substring(0,count));
            // string temp1 = "." + strNum.Substring(count,2);
            
            // if(temp1[2] == '0')
            // {
            //     temp1 = temp1.Substring(0,2);
            // }
            string temp1 = "." + strNum.Substring(count,1);
            
            if(temp1[1] == '0')
            {
                temp1 = "";
            }

            return string.Format("{0:#,###}{1}{2}",tempNum,temp1,token);
        }

        public static string NumberToString(long number)
        {
            string strNum = string.Format("{0}",number);
            
            int numPos = strNum.Length;
            if(numPos < 4 || (numPos == 4 && strNum[0] == '-'))
            {
                return strNum;
            }

            if(strNum[0] == '-')
            {
                --numPos;
                strNum = strNum.Substring(1);
            }
            
            string token = "";
            int numCount = 0;

            if(numPos < 7)
            {
                numCount = 3;
                token = "K";
            }
            else if(numPos < 10)
            {
                numCount = 6;
                token = "M";
            }
            else if(numPos < 13)
            {
                numCount = 9;
                token = "B";
            }
            else
            {
                numCount = 12;
                token = "T";
            }
            
            int count = numPos - numCount;
            int tempNum = int.Parse(strNum.Substring(0,count));
            // string temp1 = "." + strNum.Substring(count,2);
            
            // if(temp1[2] == '0')
            // {
            //     temp1 = temp1.Substring(0,2);
            // }
            string temp1 = "." + strNum.Substring(count,1);
            
            if(temp1[1] == '0')
            {
                temp1 = "";
            }

            if(number < 0)
            {
                return string.Format("-{0:#,###}{1}{2}",tempNum,temp1,token);
            }
            
            return string.Format("{0:#,###}{1}{2}",tempNum,temp1,token);
        }

        public static bool IsNotchDevice()
        {
            return Screen.height != (int)Screen.safeArea.height && !SystemInfo.deviceModel.Contains("iPad");
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> list) 
        {
            var source = list.ToList();
            int n = source.Count;
            var shuffled = new List<T>(n);
            shuffled.AddRange(source);
            while (n > 1) 
            {
                n--;
                int k = rng.Next(n + 1);
                T value = shuffled[k];
                shuffled[k] = shuffled[n];
                shuffled[n] = value;
            }
            return shuffled;
        }

        public static float EaseIn(float time, float rate)
        {
            return Mathf.Pow(time, rate);   
        }

        public static float EaseOut(float time, float rate)
        {
            return Mathf.Pow(time, 1 /rate);
        }

        public static string EscapeURL(string url)
        {
            return UnityWebRequest.EscapeURL(url).Replace("+", "%20");
        }


        public static void SendStore(string id)
        {
#if UNITY_IOS
            Application.OpenURL(string.Format("http://itunes.apple.com/app/id{0}?mt=8", id));//storeKey));
#elif UNITY_ANDROID
    #if UNITY_EDITOR
            Application.OpenURL("http://play.google.com/store/apps/details?id="+id);// bundleIdentifier);
    #else
            Application.OpenURL("market://details?id="+id);//+bundleIdentifier);
    #endif
#endif
        }

#if UNITY_IPHONE
        [DllImport("__Internal")]
        private static extern string GetBundleVersion();
#endif
        public static string GetVersionCode()
        {
            if(string.IsNullOrEmpty(versionCode))
            {
    #if !UNITY_EDITOR
        #if UNITY_IOS
            versionCode = GetBundleVersion();
        #elif UNITY_ANDROID
            AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var ca = up.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject packageManager = ca.Call<AndroidJavaObject>("getPackageManager");
            var pInfo = packageManager.Call<AndroidJavaObject>("getPackageInfo", Application.identifier, 0);
            versionCode = pInfo.Get<int>("versionCode").ToString();
        #endif    
    #endif
            }
    
            return versionCode;
        }

        
        public static IEnumerator ShowWebview(string url)
        {
            WebViewObject webViewObject;
            webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
            webViewObject.Init(
                cb: (msg) =>
                {
                    // Debug.Log(string.Format("CallFromJS[{0}]", msg));
                },
                err: (msg) =>
                {
                    // Debug.Log(string.Format("CallOnError[{0}]", msg));
                },
                httpErr: (msg) =>
                {
                    // Debug.Log(string.Format("CallOnHttpError[{0}]", msg));
                },
                started: (msg) =>
                {
                    // Debug.Log(string.Format("CallOnStarted[{0}]", msg));
                },
                hooked: (msg) =>
                {
                    // Debug.Log(string.Format("CallOnHooked[{0}]", msg));
                },
                ld: (msg) =>
                {
                    // Debug.Log(string.Format("CallOnLoaded[{0}]", msg));
                    webViewObject.EvaluateJS(@"Unity.call('ua=' + navigator.userAgent)");
                }
                //transparent: false,
                //zoom: true,
                //ua: "custom user agent string",
                //// android
                //androidForceDarkMode: 0,  // 0: follow system setting, 1: force dark off, 2: force dark on
                //// ios
                //enableWKWebView: true,
                //wkContentMode: 0,  // 0: recommended, 1: mobile, 2: desktop
                //wkAllowsLinkPreview: true,
                //// editor
                //separated: false
                );
    #if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            webViewObject.bitmapRefreshCycle = 1;
    #endif
            // cf. https://github.com/gree/unity-webview/pull/512
            // Added alertDialogEnabled flag to enable/disable alert/confirm/prompt dialogs. by KojiNakamaru · Pull Request #512 · gree/unity-webview
            //webViewObject.SetAlertDialogEnabled(false);

            // cf. https://github.com/gree/unity-webview/pull/728
            //webViewObject.SetCameraAccess(true);
            //webViewObject.SetMicrophoneAccess(true);

            // cf. https://github.com/gree/unity-webview/pull/550
            // introduced SetURLPattern(..., hookPattern). by KojiNakamaru · Pull Request #550 · gree/unity-webview
            //webViewObject.SetURLPattern("", "^https://.*youtube.com", "^https://.*google.com");

            // cf. https://github.com/gree/unity-webview/pull/570
            // Add BASIC authentication feature (Android and iOS with WKWebView only) by takeh1k0 · Pull Request #570 · gree/unity-webview
            //webViewObject.SetBasicAuthInfo("id", "password");

            //webViewObject.SetScrollbarsVisibility(true);

            webViewObject.SetMargins(5, 100, 5, Screen.height / 4);
            webViewObject.SetTextZoom(100);  // android only. cf. https://stackoverflow.com/questions/21647641/android-webview-set-font-size-system-default/47017410#47017410
            webViewObject.SetVisibility(true);

    #if !UNITY_WEBPLAYER && !UNITY_WEBGL
            if (url.StartsWith("http")) {
                webViewObject.LoadURL(url.Replace(" ", "%20"));
            } else {
                var exts = new string[]{
                    ".jpg",
                    ".js",
                    ".htm",  // should be last
                    ".html"  // should be last
                };
                foreach (var ext in exts) {
                    var _url = url.Replace(".html", ext);
                    var src = System.IO.Path.Combine(Application.streamingAssetsPath, _url);
                    var dst = System.IO.Path.Combine(Application.persistentDataPath, _url);
                    byte[] result = null;
                    if (src.Contains("://")) {  // for Android
    #if UNITY_2018_4_OR_NEWER
                        // NOTE: a more complete code that utilizes UnityWebRequest can be found in https://github.com/gree/unity-webview/commit/2a07e82f760a8495aa3a77a23453f384869caba7#diff-4379160fa4c2a287f414c07eb10ee36d
                        var unityWebRequest = UnityWebRequest.Get(src);
                        yield return unityWebRequest.SendWebRequest();
                        result = unityWebRequest.downloadHandler.data;
    #else
                        var www = new WWW(src);
                        yield return www;
                        result = www.bytes;
    #endif
                    } else {
                        result = System.IO.File.ReadAllBytes(src);
                    }
                    System.IO.File.WriteAllBytes(dst, result);
                    if (ext == ".html" || ext == ".htm") {
                        webViewObject.LoadURL("file://" + dst.Replace(" ", "%20"));
                        break;
                    }
                }
            }
    #else
            if (Url.StartsWith("http")) {
                webViewObject.LoadURL(url.Replace(" ", "%20"));
            } else {
                webViewObject.LoadURL("StreamingAssets/" + url.Replace(" ", "%20"));
            }
    #endif
            yield break;
        }

        public static Color32 ConvertColorFormInt(int aCol)
        {
            return new Color32((byte)((aCol>>24) & 0xFF), (byte)((aCol >> 16) & 0xFF), (byte)((aCol >> 8) & 0xFF),(byte)((aCol) & 0xFF));
        }

        public static string ConvertThreeLetterNameToTwoLetterName(string twoLetterCountryCode)
        {
            ALFUtils.Assert(!string.IsNullOrEmpty(twoLetterCountryCode), "ConvertThreeLetterNameToTwoLetterName !!");

            if (twoLetterCountryCode.Length != 2)
            {
                return twoLetterCountryCode;
            }

            // if(twoLetterCountryCode == "BI")
            // {
            //     return "BDI";
            // }

            // if(twoLetterCountryCode == "CP")
            // {
            //     return "CPT";
            // }

            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

            foreach (CultureInfo culture in cultures)
            {
                // RegionInfo region = new RegionInfo(culture.LCID);
                RegionInfo region = new RegionInfo(culture.Name);
                if (region.TwoLetterISORegionName.ToUpper() == twoLetterCountryCode.ToUpper())
                {
                    return region.ThreeLetterISORegionName;
                }
            }            

            // Debug.Log(twoLetterCountryCode);
            throw new ArgumentException("Could not get country code");
        }
// public static string ToHexString(this Color c) => System.Drawing.ColorTranslator.ToHtml(System.Drawing.Color.FromArgb(int.Parse(ColorUtility.ToHtmlStringRGBA(c),System.Globalization.NumberStyles.HexNumber)));
        public static string ToHexString(this Color c) => $"#{ColorUtility.ToHtmlStringRGB(c)}";
    

        #region 압축하기 - Compress(source)

        /// <summary>
        /// 압축하기
        /// </summary>
        /// <param name="source">소스 문자열</param>
        /// <returns>압축 문자열</returns>
        public static string Compress(string source)
        {
            byte[] sourceArray = Encoding.UTF8.GetBytes(source);

            MemoryStream memoryStream = new MemoryStream();

            using(System.IO.Compression.GZipStream gZipStream = new System.IO.Compression.GZipStream(memoryStream, System.IO.Compression.CompressionMode.Compress, true))
            {
                gZipStream.Write(sourceArray, 0, sourceArray.Length);
            }

            memoryStream.Position = 0;

            byte[] temporaryArray = new byte[memoryStream.Length];

            memoryStream.Read(temporaryArray, 0, temporaryArray.Length);

            byte[] targetArray = new byte[temporaryArray.Length + 4];

            Buffer.BlockCopy(temporaryArray, 0, targetArray, 4, temporaryArray.Length);

            Buffer.BlockCopy(BitConverter.GetBytes(sourceArray.Length), 0, targetArray, 0, 4);

            return Convert.ToBase64String(targetArray);
        }

        #endregion
        #region 압축 해제하기 - Decompress(source)

        /// <summary>
        /// 압축 해제하기
        /// </summary>
        /// <param name="source">소스 문자열</param>
        /// <returns>압축 해제 문자열</returns>
        public static string Decompress(string source)
        {
            byte[] sourceArray = Convert.FromBase64String(source);

            using(MemoryStream memoryStream = new MemoryStream())
            {
                int dataLength = BitConverter.ToInt32(sourceArray, 0);

                memoryStream.Write(sourceArray, 4, sourceArray.Length - 4);

                byte[] targetArray = new byte[dataLength];

                memoryStream.Position = 0;

                using(System.IO.Compression.GZipStream gZipStream = new System.IO.Compression.GZipStream(memoryStream, System.IO.Compression.CompressionMode.Decompress))
                {
                    gZipStream.Read(targetArray, 0, targetArray.Length);
                }

                return Encoding.UTF8.GetString(targetArray);
            }
        }

        #endregion    

        public static void ClearAllFwordList()
        {
            fwordFilter.ClearAllFwordList();
        }
        public static void AddFwordList(string pFword)
        {
            fwordFilter.AddFword(pFword);
        }

        public static string FwordFilter(string src)
        {
            return fwordFilter.Filter(src);
        }

        public static bool IsFwordFilter(string src)
        {
            return fwordFilter.IsFilter(src);
        }

        // public static void Create2DTileMap(int x, int y, MeshFilter meshFilter, Material mat = null)
        // {
        //     Vector3 pos = Vector3.zero;
        //     pos.y = y * 0.5f;
        //     int num = 0;
        //     int randomValue = (x * y) > 20 ? UnityEngine.Random.Range(3, (int)(x * y * 0.3f) +1) : 0;

        //     CombineInstance[] combine = new CombineInstance[(x*y)];
            
        //     for(int n = 0; n < y; ++n)
        //     {
        //         List<TileData> list = new List<TileData>();
        //         for(int i = 0; i < x; ++i)
        //         {
        //             TileData baseTile = TileData.Create(null,i,n);

        //             if(randomValue > 0)
        //             {
        //                 if(Random.Range(0, 100) > 90)
        //                 {
        //                     --randomValue;
        //                     pos.x +=1;
        //                     pos.y -= 0.5f;
        //                     // baseTile.Empty = true;
        //                     list.Add(baseTile);
        //                     continue;        
        //                 }
        //             }
                    
        //             if(Random.Range(0, 100) > 90)
        //             {
        //                 // baseTile.Respawn = true;
        //                 m_baseRespawnList.Add( (x * n)+i);
        //             }
                    
        //             combine[num].mesh = CreateBase2DTileMesh(Vector3.zero);
        //             combine[num].transform = Matrix4x4.Translate(pos);
        //             // baseTile.Pos = pos;
        //             list.Add(baseTile);
        //             pos.x +=1;
        //             pos.y -= 0.5f;
        //             ++num;
        //         }
        //         m_baseTileList.Add(list);
        //         pos.y = (n + y)* 0.5f;
        //         pos.x = n;
        //     }
            
        //     if(meshFilter.mesh != null)
        //     {
        //         meshFilter.mesh.Clear();
        //     }
            
        //     meshFilter.mesh.CombineMeshes( combine );
        // }

        // public static void Create3DTileMap(int x, int z, MeshFilter meshFilter, Material mat = null)
        // {
        //     Vector3 pos = Vector3.zero;
        //     // pos.y = y * 0.5f;
        //     int num = 0;
        //     int randomValue = (x * z) > 20 ? Random.Range(3, (int)(x * z * 0.3f) +1) : 0;

        //     CombineInstance[] combine = new CombineInstance[(x*z)];
        //     for(int n = 0; n < z; ++n)
        //     {
        //         List<TileData> list = new List<TileData>();
        //         for(int i = 0; i < x; ++i)
        //         {
        //             TileData baseTile = TileData.Create(null,i,n);

        //             if(randomValue > 0)
        //             {
        //                 if(Random.Range(0, 100) > 90)
        //                 {
        //                     --randomValue;
        //                     pos.x += 2;
        //                     // baseTile.Empty = true;
        //                     list.Add(baseTile);
        //                     continue;        
        //                 }
        //             }
                    
        //             if(Random.Range(0, 100) > 90)
        //             {
        //                 // baseTile.Respawn = true;
        //                 m_baseRespawnList.Add( (x * n)+i);
        //             }
                    
        //             combine[num].mesh = CreateBase3DTileMesh(Vector3.zero);
        //             combine[num].transform = Matrix4x4.Translate(pos);
        //             // baseTile.Pos = pos;
        //             list.Add(baseTile);
        //             pos.x += 2;
        //             ++num;
        //         }
        //         m_baseTileList.Add(list);
        //         pos.z = n * 2;
        //         pos.x = 0;
        //     }
        //     if(meshFilter.mesh != null)
        //     {
        //         meshFilter.mesh.Clear();
        //     }
            
        //     meshFilter.mesh.CombineMeshes( combine );
        // }

        // static Mesh CreateBase3DTileMesh(Vector3 pos)
        // {
        //     Mesh mesh = new Mesh();
            
        //     Vector3[] vertices = new Vector3[base3DTileMashVertices.Length];
        //     for(int i =0; i < vertices.Length; ++i)
        //     {
        //         vertices[i] = base3DTileMashVertices[i] + pos;
        //     }
        //     mesh.vertices = vertices;
        //     mesh.uv = base3DTileMashUV;
        //     mesh.triangles = baseTileMashTriangles;
        //     mesh.tangents = base3DTileMashTangents;
        //     mesh.normals = base3DTileMashNormals;
        //     return mesh;
        // }
        // static Mesh CreateBase2DTileMesh(Vector3 pos)
        // {
        //     Mesh mesh = new Mesh();
            
        //     Vector3[] vertices = new Vector3[base2DTileMashVertices.Length];
        //     for(int i =0; i < vertices.Length; ++i)
        //     {
        //         vertices[i] = base2DTileMashVertices[i] + pos;
        //     }
        //     mesh.vertices = vertices;
        //     mesh.uv = base2DTileMashUV;
        //     mesh.triangles = baseTileMashTriangles;
        //     mesh.tangents = base2DTileMashTangents;
        //     mesh.normals = base2DTileMashNormals;
        //     return mesh;
        // }

        // static void CombineMeshes()
        // {
        //     // MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>();
        //     // Material material = meshFilters[0].GetComponent<MeshRenderer>().sharedMaterial;
            
        //     // CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        //     // int i = 0;
            
        //     // while ( i < meshFilters.Length )
        //     // {
        //     //     combine[i].mesh = meshFilters[i].sharedMesh;
        //     //     combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                
        //     //     meshFilters[i].gameObject.SetActive(false);
        //     //     i++;
        //     // }          

        //     // MeshFilter meshFilter = root.gameObject.AddComponent<MeshFilter>();
        //     // MeshRenderer meshRenderer = root.gameObject.AddComponent<MeshRenderer>();
        //     // meshFilter.mesh = new Mesh();
        //     // meshRenderer.material = material;
        //     // meshFilter.mesh.CombineMeshes( combine );
        // }
    }

}





//////////  암호화....

// using UnityEngine;
// using System.Security.Cryptography;
// using System.Text;
// using System.IO;
//  
// public class SecurityPlayerPrefs
// {
//     // http://www.codeproject.com/Articles/769741/Csharp-AES-bits-Encryption-Library-with-Salt
//     // http://ikpil.com/1342

//     private static string _saltForKey;

//     private static byte[] _keys;
//     private static byte[] _iv;
//     private static int keySize = 256;
//     private static int blockSize = 128;
//     private static int _hashLen = 32;

//     static SecurityPlayerPrefs()
//     {
//         // 8 바이트로 하고, 변경해서 쓸것
//         byte[] saltBytes = new byte[] { 25, 36, 77, 51, 43, 14, 75, 93 };

//         // 길이 상관 없고, 키를 만들기 위한 용도로 씀
//         string randomSeedForKey = "5b6fcb4aaa0a42acae649eba45a506ec";

//         // 길이 상관 없고, aes에 쓸 key 와 iv 를 만들 용도
//         string randomSeedForValue = "2e327725789841b5bb5c706d6b2ad897";

//         {
//             Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(randomSeedForKey, saltBytes, 1000);
//             _saltForKey = System.Convert.ToBase64String(key.GetBytes(blockSize / 8));
//         }

//         {
//             Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(randomSeedForValue, saltBytes, 1000);
//             _keys = key.GetBytes(keySize / 8);
//             _iv = key.GetBytes(blockSize / 8);
//         }
//     }

//     public static string MakeHash(string original)
//     {
//         using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
//         {
//             byte[] bytes = System.Text.Encoding.UTF8.GetBytes(original);
//             byte[] hashBytes = md5.ComputeHash(bytes);

//             string hashToString = "";
//             for (int i = 0; i < hashBytes.Length; ++i)
//                 hashToString += hashBytes[i].ToString("x2");

//             return hashToString;
//         }
//     }

//     public static byte[] Encrypt(byte[] bytesToBeEncrypted)
//     {
//         using (RijndaelManaged aes = new RijndaelManaged())
//         {
//             aes.KeySize = keySize;
//             aes.BlockSize = blockSize;

//             aes.Key = _keys;
//             aes.IV = _iv;

//             aes.Mode = CipherMode.CBC;
//             aes.Padding = PaddingMode.PKCS7;

//             using (ICryptoTransform ct = aes.CreateEncryptor())
//             {
//                 return ct.TransformFinalBlock(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
//             }
//         }
//     }

//     public static byte[] Decrypt(byte[] bytesToBeDecrypted)
//     {
//         using (RijndaelManaged aes = new RijndaelManaged())
//         {
//             aes.KeySize = keySize;
//             aes.BlockSize = blockSize;

//             aes.Key = _keys;
//             aes.IV = _iv;

//             aes.Mode = CipherMode.CBC;
//             aes.Padding = PaddingMode.PKCS7;

//             using (ICryptoTransform ct = aes.CreateDecryptor())
//             {
//                 return ct.TransformFinalBlock(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
//             }
//         }
//     }

//     public static string Encrypt(string input)
//     {
//         byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
//         byte[] bytesEncrypted = Encrypt(bytesToBeEncrypted);

//         return System.Convert.ToBase64String(bytesEncrypted);
//     }

//     public static string Decrypt(string input)
//     {
//         byte[] bytesToBeDecrypted = System.Convert.FromBase64String(input);
//         byte[] bytesDecrypted = Decrypt(bytesToBeDecrypted);

//         return Encoding.UTF8.GetString(bytesDecrypted);
//     }

//     private static void SetSecurityValue(string key, string value)
//     {
//         string hideKey = MakeHash(key + _saltForKey);
//         string encryptValue = Encrypt(value + MakeHash(value));

//         PlayerPrefs.SetString(hideKey, encryptValue);
//     }

//     private static string GetSecurityValue(string key)
//     {
//         string hideKey = MakeHash(key + _saltForKey);

//         string encryptValue = PlayerPrefs.GetString(hideKey);
//         if (true == string.IsNullOrEmpty(encryptValue))
//             return string.Empty;

//         string valueAndHash = Decrypt(encryptValue);
//         if (_hashLen > valueAndHash.Length)
//             return string.Empty;

//         string savedValue = valueAndHash.Substring(0, valueAndHash.Length - _hashLen);
//         string savedHash = valueAndHash.Substring(valueAndHash.Length - _hashLen);

//         if (MakeHash(savedValue) != savedHash)
//             return string.Empty;

//         return savedValue;
//     }

//     public static void DeleteKey(string key)
//     {
//         PlayerPrefs.DeleteKey(MakeHash(key + _saltForKey));
//     }

//     public static void DeleteAll()
//     {
//         PlayerPrefs.DeleteAll();
//     }

//     public static void Save()
//     {
//         PlayerPrefs.Save();
//     }

//     public static void SetInt(string key, int value)
//     {
//         SetSecurityValue(key, value.ToString());
//     }

//     public static void SetLong(string key, long value)
//     {
//         SetSecurityValue(key, value.ToString());
//     }

//     public static void SetFloat(string key, float value)
//     {
//         SetSecurityValue(key, value.ToString());
//     }

//     public static void SetString(string key, string value)
//     {
//         SetSecurityValue(key, value);
//     }

//     public static int GetInt(string key, int defaultValue)
//     {
//         string originalValue = GetSecurityValue(key);
//         if (true == string.IsNullOrEmpty(originalValue))
//             return defaultValue;

//         int result = defaultValue;
//         if (false == int.TryParse(originalValue, out result))
//             return defaultValue;

//         return result;
//     }

//     public static long GetLong(string key, long defaultValue)
//     {
//         string originalValue = GetSecurityValue(key);
//         if (true == string.IsNullOrEmpty(originalValue))
//             return defaultValue;

//         long result = defaultValue;
//         if (false == long.TryParse(originalValue, out result))
//             return defaultValue;

//         return result;
//     }

//     public static float GetFloat(string key, float defaultValue)
//     {
//         string originalValue = GetSecurityValue(key);
//         if (true == string.IsNullOrEmpty(originalValue))
//             return defaultValue;

//         float result = defaultValue;
//         if (false == float.TryParse(originalValue, out result))
//             return defaultValue;

//         return result;
//     }

//     public static string GetString(string key, string defaultValue)
//     {
//         string originalValue = GetSecurityValue(key);
//         if (true == string.IsNullOrEmpty(originalValue))
//             return defaultValue;

//         return originalValue;
//     }
// }


// using System;
// using System.Collections;
// using System.Net.Sockets;
// using System.IO;
// using UnityEngine;

// public class TimeManager : MonoBehaviour
// {

//     public static TimeManager Instance { get; private set; }

//     public static Action<bool> IsDone;

//     public static DateTime GetRealTime { get { return Instance.currentTime.AddSeconds(RealTime.time); } }


//     private DateTime currentTime = DateTime.MinValue;

//     private float timeOut = 100f;



//     void Awake()

//     {

//         Instance = this;

//         StartCoroutine(ReceiveRealTime());

//     }

    

//     IEnumerator ReceiveRealTime()

//     {

//         TcpClient tcpClient = new TcpClient("time.nist.gov", 13);

//         StreamReader sr = new StreamReader(tcpClient.GetStream());

        

//         // 형태 57486 16-04-08 08:53:18 50 0 0 737.0 UTC(NIST) * 
//         string readData = sr.ReadToEnd();
//         // 형태 16-04-08 08:57:07        
//         string _time = readData.Substring(readData.IndexOf(" ") + 1, 17);

//         // 대한민국은 UTC 기준 +9시간.

//         currentTime = Convert.ToDateTime(_time).AddHours(9);

//         // Debug.Log("현재 시간 : " + currentTime.ToString("yyyy-MM-dd HH:mm:ss"));

//         float waitTime = 0f;

//         while (true)
//         {   
//             if (currentTime.CompareTo(DateTime.MinValue) == 0)
//             {
//                 if (waitTime < timeOut)
//                     yield return null;
//                 else
//                 {
//                     InvokeEvent(false);
//                     yield break;
//                 }
//             }
//             else
//             {
//                 InvokeEvent(true);
//                 yield break;
//             }

//             waitTime += Time.deltaTime;
//         }
//     }

//     private void InvokeEvent(bool isDone)
//     {
//         if (IsDone == null) return;

//         IsDone.Invoke(isDone);

//     }

// }


// public static IEnumerator SetCountry()
// {
//     string ip = new System.Net.WebClient().DownloadString("https://api.ipify.org");
//     string uri = $"https://ipapi.co/{ip}/json/";


//     using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
//     {
//         yield return webRequest.SendWebRequest();

//         string[] pages = uri.Split('/');
//         int page = pages.Length - 1;

//         IpApiData ipApiData = IpApiData.CreateFromJSON(webRequest.downloadHandler.text);

//         Debug.Log(ipApiData.country_name);
//     }
// }



// using UnityEngine;
// using System.Collections;
 
// namespace UsefulUtilities
// {
//     /// <summary>
//     /// Makes the Transform snap to the specified snap values.
//     /// </summary>
//     public class TransformSnap : MonoBehaviour
//     {
 
//         // snap to this value
//         public float snap = 0.01f;
//         private Transform _transform;
 
//         // Use this for initialization
//         void Start()
//         {
//             _transform = transform;
//         }
 
//         // Update is called once per frame
//         void Update()
//         {
//             _transform.position = GetSharedSnapPosition(_transform.position, snap);
//         }
 
//         /// <summary>
//         /// Accepts a position, and sets each axis-value of the position to be snapped according to the value of snap
//         /// </summary>
//         public static Vector3 GetSharedSnapPosition(Vector3 originalPosition, float snap = 0.01f)
//         {
//             return new Vector3(GetSnapValue(originalPosition.x, snap), GetSnapValue(originalPosition.y, snap), GetSnapValue(originalPosition.z, snap));
//         }
 
//         /// <summary>
//         /// Accepts a value, and snaps it according to the value of snap
//         /// </summary>
//         public static float GetSnapValue(float value, float snap = 0.01f)
//         {
//             return (!Mathf.Approximately(snap, 0f)) ? Mathf.RoundToInt(value / snap) * snap : value;
//         }
//     }
// }