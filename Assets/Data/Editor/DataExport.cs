//#define LLOG_BMPOOL

using UnityEngine;
using System.IO;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

#if UNITY_EDITOR
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using UnityEditor.IMGUI.Controls;
using System.Reflection;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using ListView;
using TOOL.LISTVIEW;
#endif

namespace ALF
{
    public enum E_EXPORT_DATA_TYPE:byte 
    { 
        NONE,
        FBS,
        MAP,
        EXCEL,
    };
    public static class DataExport 
    {
        // static string MCS ="/Applications/Unity/Unity.app/Contents/Frameworks/MonoBleedingEdge/bin/smcs";
#if UNITY_EDITOR
        static string ROOT = Application.dataPath.Substring(0,Application.dataPath.IndexOf("Assets")).Replace("\\", "/");
        static readonly List<string> excludeList = new List<string>{"BotNationCity.xlsx","PlayerFace.xlsx","PlayerAbilityWeightSum.xlsx","PlayerCreate.xlsx","PlayerCreateAbility.xlsx","PlayerCreateForename.xlsx","PlayerCreatePosition.xlsx","PlayerCreateSurname.xlsx","RewardRatingData.xlsx","RewardRatingSet.xlsx","Attend.xlsx","LeagueFixture.xlsx","FastReward.xlsx"};
        
        public static string GetRootPath()
        {
            return ROOT;
        }
        public static List<string> GetExcludeFileList()
        {
            return excludeList;
        }
        public static void MakeExcelToFBS(ExportDataViewItem item, EditorWindow _editorWindow)
        {
            float progressBar = 0.0f;
            // EditorUtility.DisplayProgressBar("Placing Prefabs","Working...",progressBar);
       
            int hundredPercent = 10000;
            int amountDone = 0;
            
            List<string> parms = new List<string>();
            string dataPath = EditorPrefs.GetString($"{Application.productName}:dataPath");
            if(string.IsNullOrEmpty(dataPath))
            {
                dataPath = "DataFiles";
            }
            dataPath = Path.Combine(ROOT,dataPath, "ExcelData");
            FileInfo[] files = null;
            
            if(item != null)
            {
                files = new FileInfo[] { new FileInfo(Path.Combine(dataPath, item.displayName))};
            }
            else
            {
                DirectoryInfo dirInfo = new DirectoryInfo(dataPath);
			    files = dirInfo.GetFiles("*.xls*");
            }
			
            // try
            {
                foreach (FileInfo f in files) 
                {
                    if(excludeList.Contains(f.Name)) continue;

                    progressBar = amountDone++ / hundredPercent;
                    int percentage = (int)(progressBar * 100f);
                    string progressStr = percentage.ToString() + "% done...";
                    // EditorUtility.DisplayProgressBar("Placing Prefabs",progressStr,progressBar);

                    ConvertFBSFromSheetHeader(LoadBook(f.FullName),f);

                    // EditorUtility.ClearProgressBar();
                }
            }
            // catch (Exception e)
            // {
            //     UnityEngine.Debug.Log(e.Message);
            //     EditorUtility.ClearProgressBar();
            // }
            _editorWindow?.Close();
        }

        static string ChangeFeildName(string fieldName,JObject jsonFeild)
        {
            StringBuilder builder = new StringBuilder();
            int i = 0;
            while(i < fieldName.Length)
            {
                if(Char.IsUpper(fieldName[i]))
                {
                    if(i > 0)
                    {
                        builder.Append('_');
                    }
                    builder.Append(fieldName[i]);
                }
                else
                {
                    builder.Append(fieldName[i]);
                }
                ++i;
            }
            string token = builder.ToString().ToLower();
            jsonFeild[fieldName] = token;
            
            return token;
        }
        static bool ConvertFBSFromSheetHeader(IWorkbook book,FileInfo f)
        {
            string fileName = Path.GetFileName(f.FullName).Split('.')[0];

            ISheet sheet = book.GetSheet("FBS");
            if(sheet == null) return false;

            string fbs = "";
            if(!string.IsNullOrEmpty(fileName))
            {
                fbs = "namespace " + fileName.ToUpper() + ";\n\n";
            }
            
            int i = 0;
            bool bOK = false;
            string rootTableName = "";
            Dictionary<string,List<string>> dic = new Dictionary<string,List<string>>();
            JObject jsonKey = new JObject();
            JObject jsonFeild = new JObject();
            string tableName = "";
            while(i <= sheet.LastRowNum)
            {
                IRow headerRow = sheet.GetRow(i);
                if(headerRow == null)
                {
                    ++i;
                    continue;
                }
                
                string value = GetCellValueByIndex(headerRow,0);
                if(value == null )
                {
                    ++i;
                }
                else
                {
                    List<string> list = new List<string>();

                    if(value == "root_type")
                    {
                        rootTableName = GetCellValueByIndex(headerRow,1);
                        fbs += string.Format("{0} {1};\nfile_identifier \"ALFD\";\nfile_extension \"bytes\";",value,GetCellValueByIndex(headerRow,1));
                        bOK = true;
                        break;
                    }
                    if(value == "enum")
                    {
                        fbs += string.Format("{0} {1}",value,GetCellValueByIndex(headerRow,1));
                        fbs +=" { ";
                        fbs += string.Format("{0}",GetCellValueByIndex(headerRow,2));
                        fbs +=" }\n";
                        ++i;
                        continue;
                    }
                    else if(value == "table")
                    {
                        fbs += string.Format("{0} {1}",value,GetCellValueByIndex(headerRow,1));
                        tableName = GetCellValueByIndex(headerRow,1);
                    }
                    
                    dic.Add(GetCellValueByIndex(headerRow,1),list);

                    fbs +="\n{\n";
                    ++i;
                    headerRow = sheet.GetRow(i);
                    ALFUtils.Assert(headerRow != null,"111 headerRow == null!!!");

                    for( int n = 0; n <= headerRow.LastCellNum; ++n )
                    {
                        value = GetCellValueByIndex(headerRow,n);
                        if(value == null )
                        {
                            break;
                        }
                        else
                        {
                            if(value.IndexOf("#") == 0)
                            {
                                continue;
                            }
                            list.Add(value);
                            string[] token = value.Split(':');
                            string temptoken = "";
                            if(token.Length < 2)
                            {
                                ALF.ALFUtils.Assert(false,"데이터 타입이 없습니다.!");
                            }
                            else if (token.Length > 2)
                            {
                                for(int t = 2; t < token.Length; ++t)
                                {
                                    if (token[t] != "map")
                                    {
                                        temptoken = $"{token[t]},";
                                    }
                                }
                            }
                            
                            fbs += string.Format("\t{0}:{1} ({2}id: {3});\n",ChangeFeildName(token[0],jsonFeild),token[1],temptoken, n);
                            
                            if(!token[1].StartsWith("["))
                            {
                                jsonKey[token[0]] = tableName;
                            }
                        }
                    }
                    fbs += "}\n\n";
                    ++i;
                }
            }

            // UnityEngine.Debug.Log(fbs);
            if(!bOK)
            {
                EditorUtility.ClearProgressBar();
                ALF.ALFUtils.Assert(bOK,"make fail !!! sheet:");
            }
            else
            {
                List<string> findStringList = new List<string>();

                string fbsPath = Path.Combine(f.DirectoryName, fileName + ".fbs");
                if(fileName == "Text") // 다운로드때문에 각 언어 데이터만 넣는다.
                {
                    int pos = fbs.IndexOf("id");
                    int nPos = fbs.IndexOf("\n",pos) +1;
                    string token = fbs.Substring(nPos,fbs.IndexOf("\n",nPos) - nPos);
                    string[] tokenList = token.Split(':');
                    findStringList.Add(tokenList[0].Substring(1));

                    tokenList[0] = "\tvalue";
                    token = tokenList[0];
                    for(int t = 1; t < tokenList.Length; ++t)
                    {
                        token += ":" + tokenList[t]; 
                    }
                    
                    int nPos1 = fbs.IndexOf("}",nPos);
                    string aString = fbs.Substring(0,nPos);
                    string bString = fbs.Substring(nPos1);

                    nPos = nPos+ token.Length +1;
                    string newToken = null;
                    string[] tokens = null;
                    while( nPos < nPos1 )
                    {
                        newToken = fbs.Substring(nPos,fbs.IndexOf("\n",nPos) - nPos);    
                        nPos = nPos + newToken.Length +1;
                        tokens = newToken.Split(':');
                        if(tokens.Length > 1)
                        {
                            findStringList.Add(tokens[0].Substring(1));
                        }
                    }

                    fbs = aString + token + bString;
                }
                
                if(fileName != "ConstValue")
                {
                    File.WriteAllText(fbsPath, fbs);
                }
                
                JObject rootJson = new JObject();
                
                List<string> list = dic[rootTableName];
                for(int n = 0; n < list.Count; ++n)
                {
                    string[] token = list[n].Split(':');
                    sheet = book.GetSheet(token[0]);

                    ALF.ALFUtils.Assert(sheet != null,string.Format(" {0}  sheet == null",token[0]) );
                    ALF.ALFUtils.Assert(token[1][0] == '[',"not Array !!!");

                    tableName = token[1].Substring(1,token[1].Length -2);
                    GetSheetDataByTable(jsonFeild,jsonKey,dic,(string)jsonFeild[token[0]],tableName,true,sheet,ref rootJson);
                }

                if(fileName == "Text") // 다운로드때문에 각 언어 데이터만 넣는다.
                {
                    for(int n =0; n < findStringList.Count; ++n)
                    {
                        List<string> tokenList = new List<string>();
                        for(int t =0; t < findStringList.Count; ++t)
                        {
                            if(t != n)
                            {
                                tokenList.Add(findStringList[t]);
                            }
                        }

                        string temp = rootJson.ToString();
                        temp = temp.Replace(findStringList[n],"value");
                        
                        JObject pJObject = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(temp, new Newtonsoft.Json.JsonSerializerSettings {DateParseHandling = Newtonsoft.Json.DateParseHandling.None});
                        for(int t =0; t < tokenList.Count; ++t)
                        {
                            var itr = pJObject.GetEnumerator();
                            while(itr.MoveNext())
                            {
                                if(itr.Current.Value.Type == JTokenType.Object)
                                {
                                    JObject obj = (JObject)itr.Current.Value;
                                    obj.Remove(tokenList[t]);
                                }
                                else if(itr.Current.Value.Type == JTokenType.Array)
                                {
                                    JArray arr = (JArray)itr.Current.Value;
                                    for(int x =0; x < arr.Count; ++x)
                                    {
                                        if(arr[x].Type == JTokenType.Object)
                                        {
                                            JObject obj = (JObject)arr[x];
                                            obj.Remove(tokenList[t]);
                                        }
                                    }
                                }
                            }
                        }
                        
                        string jsonPath = Path.Combine(f.DirectoryName, findStringList[n].Replace("_","").ToUpper() + ".json");
                        File.WriteAllText(jsonPath, pJObject.ToString());

                        List<string> parms = new List<string>();
                        parms.Add("-o "+ Path.Combine(ROOT, "Assets/Data/Bin"));
                        parms.Add("--csharp");
                        parms.Add("--binary");
                        parms.Add(fbsPath);
                        parms.Add(jsonPath);
                        DataExport.Export(parms,true);
                    }
                }
                // else if(fileName == "MileageMission") // 클럽 라이센스 데이터 분리
                // {
                //     JArray pMileageMission = new JArray();
                //     JArray pClubLicenseMission = new JArray();
                //     JArray pArray = (JArray)rootJson["mileage_mission"];
                //     for(int t = 0; t < pArray.Count; ++t)
                //     {
                //         if((uint)(pArray[t]["group"]) > 1000)
                //         {
                //             pClubLicenseMission.Add(pArray[t]);
                //         }
                //         else
                //         {
                //             pMileageMission.Add(pArray[t]);
                //         }
                //     }
                    
                //     JObject pJObject = new JObject();
                //     string jsonPath = null;
                //     List<string> parms = new List<string>();
                    
                //     parms.Add("-o "+ Path.Combine(ROOT, "Assets/Data/Bin"));
                //     parms.Add("--csharp");
                //     parms.Add("--binary");
                //     parms.Add(fbsPath);
                //     parms.Add("");

                //     UnityEngine.Debug.Log("=---------------------------------");
                //     UnityEngine.Debug.Log(pMileageMission.Count);
                //     UnityEngine.Debug.Log(pMileageMission.Count > 0);
                //     if(pMileageMission.Count > 0)
                //     {
                //         pJObject["mileage_mission"] = pMileageMission;
                //         jsonPath = Path.Combine(f.DirectoryName, fileName + ".json");
                //         File.WriteAllText(jsonPath, pJObject.ToString());
                //         parms[4]= jsonPath;
                //         DataExport.Export(parms,true);
                //     }

                //     if(pClubLicenseMission.Count > 0)
                //     {
                //         pJObject = new JObject();
                //         pJObject["mileage_mission"] = pClubLicenseMission;

                //         jsonPath = Path.Combine(f.DirectoryName, "MileageClubLicense.json");
                //         File.WriteAllText(jsonPath, pJObject.ToString());
                //         parms[4]= jsonPath;
                //         DataExport.Export(parms,true);
                //     }
                // }
                else if(fileName == "ConstValue")
                {
                    JArray pJArray = (JArray)rootJson["const_value"];
                    string token = "enum E_CONST_TYPE:ubyte { ";

                    for(int n =0; n < pJArray.Count; ++n)
                    {
                        JObject pJObjec = (JObject)pJArray[n];
                        if(n == 0)
                        {
                            token += pJObjec["id"].ToString();
                        }
                        else
                        {
                            token += "," + pJObjec["id"].ToString();
                        }
                    }

                    token += ",MAX }";

                    string[] tokenList = fbs.Split(';');
                    token = tokenList[0] + ";\n\n" + token;
                    for(int n =1; n < tokenList.Length; ++n)
                    {
                        if(n < tokenList.Length -1)
                        {
                            token += tokenList[n] + ";";
                        }
                    }                    
                    
                    File.WriteAllText(fbsPath, token);

                    string jsonPath = Path.Combine(f.DirectoryName, fileName + ".json");
                    File.WriteAllText(jsonPath, rootJson.ToString());

                    List<string> parms = new List<string>();
                    parms.Add("-o "+ Path.Combine(ROOT, "Assets/Data/Bin"));
                    parms.Add("--csharp");
                    parms.Add("--binary");
                    parms.Add(fbsPath);
                    parms.Add(jsonPath);
                    DataExport.Export(parms,true);
                }
                else
                {
                    // if(fileName == "ClubNationality")
                    // {
                    //     string token = "        - Key: #\n          Value: {fileID: 21300000, guid: %, type: 3}\n";
                    //     string dest_file = Path.Combine(Application.dataPath, "Image/Flag");

                    //     // string source_file = Path.Combine(Application.dataPath, "Image/Flag/ALB.png");
                    //     var stringBuilder = new StringBuilder();

                    //     JArray pArratList = (JArray)rootJson["club_nationality"];
                    //     for(int tt =0; tt < pArratList.Count; ++tt)
                    //     {
                    //         JObject pJObject = (JObject)pArratList[tt];
                    //         string _name = pJObject["nation"].ToString();
                    //         string _path = Path.Combine( "Assets/Image/Flag",_name +".png");
                    //         if(!File.Exists(Path.Combine( dest_file,_name +".png")))
                    //         {
                    //             continue;
                    //         }
                    //         Sprite temp = AssetDatabase.LoadAssetAtPath<Sprite>(_path);
                            
                    //         string guid;
                    //         long file;
                    //         UnityEngine.Debug.Log(File.Exists(_path));
                    //         if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(temp, out guid, out file))
                    //         {
                    //             string ttt = token.Replace("#",_name);
                    //             ttt = ttt.Replace("%",guid.ToString());
                    //             stringBuilder.Append(ttt);
                    //         }
                    //         // string temp = dest_file +"/" + pJObject["nation"].ToString() + ".png";
                    //         // UnityEngine.Debug.Log(File.Exists(temp));
                    //         // if(!File.Exists(temp))
                    //         // {
                    //         //     System.IO.File.Copy(source_file, temp, true);  
                    //         // }
                            
                    //     }

                    //     File.WriteAllText(Path.Combine(f.DirectoryName, fileName + ".txt"), stringBuilder.ToString());    
                    // }

                    // if(fileName == "PlayerNationality")
                    // {
                    //     string token = "        - Key: #\n          Value: {fileID: 21300000, guid: %, type: 3}\n";
                    //     string dest_file = Path.Combine(Application.dataPath, "Image/Flag");

                    //     // string source_file = Path.Combine(Application.dataPath, "Image/Flag/kkkor.png");
                    //     var stringBuilder = new StringBuilder();

                    //     JArray pArratList = (JArray)rootJson["player_nationality"];
                    //     for(int tt =0; tt < pArratList.Count; ++tt)
                    //     {
                    //         JObject pJObject = (JObject)pArratList[tt];
                    //         string _name = pJObject["nation"].ToString();
                    //         string _path = Path.Combine( "Assets/Image/Flag",_name +".png");
                    //         Sprite temp = AssetDatabase.LoadAssetAtPath<Sprite>(_path);
                            
                    //         string guid;
                    //         long file;

                    //         if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(temp, out guid, out file))
                    //         {
                                
                    //             string ttt = token.Replace("#",_name);
                    //             ttt = ttt.Replace("%",guid.ToString());
                    //             stringBuilder.Append(ttt);
                    //         }
                    //         // System.IO.File.Copy(source_file, dest_file + pJObject["nation"].ToString() + ".png", true);  
                    //     }

                    //     File.WriteAllText(Path.Combine(f.DirectoryName, fileName + ".txt"), stringBuilder.ToString());    
                    // }
                

                    string jsonPath = Path.Combine(f.DirectoryName, fileName + ".json");
                    File.WriteAllText(jsonPath, rootJson.ToString());

                    List<string> parms = new List<string>();
                    parms.Add("-o "+ Path.Combine(ROOT, "Assets/Data/Bin"));
                    parms.Add("--csharp");
                    parms.Add("--binary");
                    parms.Add(fbsPath);
                    parms.Add(jsonPath);
                    DataExport.Export(parms,true);
                }
                
                findStringList.Clear();
            }

            return bOK;
        }

        static void GetSheetDataByTable(JObject jsonFeild,JObject jsonKey,Dictionary<string, List<string>> dic,string id, string type,bool bArray, ISheet sheet, ref JObject json)
        {
            if(bArray)
            {
                JArray jIndex = new JArray();
                int i =1;
                while(i <= sheet.LastRowNum)
                {
                    jIndex.Add(i);
                    ++i;
                }

                ConvertSheetToJsonArray(jsonFeild, jsonKey,dic, id, type, sheet, jIndex,ref json);
            }
        }

        static void ConvertSheetToJsonArray(JObject jsonFeild,JObject jsonKey,Dictionary<string, List<string>> dic, string id,string tableName, ISheet sheet,JArray jIndexList, ref JObject json)
        {
            bool bMap = false;
            int i = 0;
            string value = null;
            IRow headerRow = null;
            List<string> list = new List<string>();
            List<string> listType = new List<string>();
            bool bJsonObjectType = dic.ContainsKey(tableName);
            string mapTableName = null;
            string keyTableName = null;
            string key = null;
            if(bJsonObjectType)
            {                
                var itr = jsonKey.GetEnumerator();
                while(itr.MoveNext())
                {
                    if( (string)itr.Current.Value == tableName)
                    {
                        list.Add((string)itr.Current.Key);
                    }
                }

                for(int t = 0; t < dic[tableName].Count; ++t)
                {
                    string[] tokenID = dic[tableName][t].Split(':');
                    for(int n =2; n < tokenID.Length; ++n )
                    {
                        if (tokenID[n] == "map")
                        {
                            bMap = true;
                            key = tokenID[0];
                            break;
                        }
                    }

                    if(bMap)
                    {
                        if(tokenID[1][0] == '[')
                        {
                            mapTableName = tokenID[1].Substring(1,tokenID[1].Length -2);
                            keyTableName = tokenID[0];
                            break;
                        }
                    }
                }

                for(int t = 0; t < dic[tableName].Count; ++t)
                {
                    string[] tokenID = dic[tableName][t].Split(':');
                    if(tokenID[1][0] == '[')
                    {
                        listType.Add(tokenID[1].Substring(1,tokenID[1].Length -2));
                    }
                    else
                    {
                        listType.Add(tokenID[1]);
                    }
                }
            }
            else
            {
                
                if(dic.ContainsKey(tableName))
                {
                    var itr = jsonKey.GetEnumerator();
                    while(itr.MoveNext())
                    {
                        if( (string)itr.Current.Value == tableName)
                        {
                            list.Add((string)itr.Current.Key);
                        }
                    }

                    for(int t = 0; t < dic[tableName].Count; ++t)
                    {
                        string[] tokenID = dic[tableName][t].Split(':');
                        if(tokenID[1][0] == '[')
                        {
                            listType.Add(tokenID[1].Substring(1,tokenID[1].Length -2));
                        }
                        else
                        {
                            listType.Add(tokenID[1]);
                        }
                    }
                }
                else
                {
                    listType.Add(tableName);
                    list.Add(id);
                }
            }

            int index = 0;
            headerRow = sheet.GetRow(0);
            ALFUtils.Assert(headerRow != null,$"222 headerRow == null!!! ({tableName})");
            List<int> indexList = new List<int>();
            while(i <= headerRow.LastCellNum)
            {
                value = GetCellValueByIndex(headerRow,i);
                if(list.Count > index)
                {
                    if(value == list[index] )
                    {
                        indexList.Add(i);

                        if(index == list.Count -1)
                        {
                            break;
                        }
                        ++index;
                    }
                }
                
                ++i;
            }
            
            i = 1;
            JArray array = new JArray();
            string jsonFeildName = null;

            if(bJsonObjectType)
            {
                if(bMap)
                {
                    JObject jMap = new JObject();
                    JObject item = null;
                    JArray jArray = null;

                    for( int ttt =0; ttt < jIndexList.Count; ++ttt)
                    {
                        i = (int)jIndexList[ttt];
                        headerRow = sheet.GetRow(i);
                        ALFUtils.Assert(headerRow != null,$"333 headerRow == null!!! ({tableName})");
                        item = new JObject();
                        index = 0;
                        
                        for( int n = 0; n < indexList.Count; ++n)
                        {
                            bool isMap = key == list[n];
                            jsonFeildName = (string )jsonFeild[list[index]];
                            ICell cell = headerRow.GetCell(indexList[n]);
                            if(cell == null || cell.CellType == CellType.Blank )// if(cell == null || cell.CellType == CellType.Blank || (cell.CellType == CellType.String  && cell.StringCellValue.StartsWith("#")))
                            {
                                continue;
                            }
                            else
                            {
                                switch(listType[index])
                                {
                                    case "bool":
                                    {
                                        if(isMap)
                                        {
                                            if(!jMap.ContainsKey(cell.BooleanCellValue.ToString()))
                                            {
                                                item[jsonFeildName] = cell.BooleanCellValue;
                                                array.Add(item);
                                                jMap[cell.NumericCellValue.ToString()] = new JArray();
                                            }
                                            
                                            jArray = (JArray)jMap[cell.NumericCellValue.ToString()];
                                            jArray.Add(i);
                                        }
                                        else
                                        {
                                            item[jsonFeildName] = cell.BooleanCellValue;
                                        } 
                                    }
                                    break;
                                    case "uint":
                                    case "int":
                                    case "long":
                                    case "ulong":
                                    case "ubyte":
                                    case "byte":
                                    case "float":
                                    case "ushort":
                                    {
                                        if(isMap)
                                        {
                                            if(!jMap.ContainsKey(cell.NumericCellValue.ToString()))
                                            {
                                                if(listType[index] == "float" )
                                                {
                                                    item[jsonFeildName] = (float)cell.NumericCellValue;
                                                }
                                                else if(listType[index] == "long" )
                                                {
                                                    item[jsonFeildName] = (long)cell.NumericCellValue;
                                                }
                                                else if(listType[index] == "ulong" )
                                                {
                                                    item[jsonFeildName] = (ulong)cell.NumericCellValue;
                                                }
                                                else if(listType[index] == "uint" )
                                                {
                                                    item[jsonFeildName] = (uint)cell.NumericCellValue;
                                                }
                                                else if(listType[index] == "int" )
                                                {
                                                    item[jsonFeildName] = (int)cell.NumericCellValue;
                                                }
                                                else if(listType[index] == "byte" )
                                                {
                                                    item[jsonFeildName] = (sbyte)cell.NumericCellValue;
                                                }
                                                else if(listType[index] == "ubyte" )
                                                {
                                                    item[jsonFeildName] = (byte)cell.NumericCellValue;
                                                }
                                                else if(listType[index] == "ushort" )
                                                {
                                                    item[jsonFeildName] = (ushort)cell.NumericCellValue;
                                                }
                                                
                                                array.Add(item);
                                                jMap[cell.NumericCellValue.ToString()] = new JArray();
                                            }

                                            jArray = (JArray)jMap[cell.NumericCellValue.ToString()];
                                            jArray.Add(i);
                                        }
                                        else
                                        {
                                            if(listType[index] == "float" )
                                            {
                                                item[jsonFeildName] = (float)cell.NumericCellValue;
                                            }
                                            else if(listType[index] == "long" )
                                            {
                                                item[jsonFeildName] = (long)cell.NumericCellValue;
                                            }
                                            else if(listType[index] == "ulong" )
                                            {
                                                item[jsonFeildName] = (ulong)cell.NumericCellValue;
                                            }
                                            else if(listType[index] == "uint" )
                                            {
                                                item[jsonFeildName] = (uint)cell.NumericCellValue;
                                            }
                                            else if(listType[index] == "int" )
                                            {
                                                item[jsonFeildName] = (int)cell.NumericCellValue;
                                            }
                                            else if(listType[index] == "byte" )
                                            {
                                                item[jsonFeildName] = (sbyte)cell.NumericCellValue;
                                            }
                                            else if(listType[index] == "ubyte" )
                                            {
                                                item[jsonFeildName] = (byte)cell.NumericCellValue;
                                            }
                                            else if(listType[index] == "ushort" )
                                            {
                                                item[jsonFeildName] = (ushort)cell.NumericCellValue;
                                            }
                                        }
                                    }
                                    break;
                                    case "string":
                                    {
                                        if(isMap)
                                        {
                                            if(!jMap.ContainsKey(cell.StringCellValue))
                                            {
                                                item[jsonFeildName] = cell.StringCellValue;
                                                array.Add(item);
                                                jMap[cell.StringCellValue] = new JArray();
                                            }

                                            jArray = (JArray)jMap[cell.StringCellValue];
                                            jArray.Add(i);
                                        }
                                        else
                                        {
                                            item[jsonFeildName] = cell.StringCellValue;
                                        }
                                    }
                                    break;
                                    default:
                                        EditorUtility.ClearProgressBar();
                                        ALF.ALFUtils.Assert(false,string.Format("{1} {0} 해당 값이 없습니다.1 ({2})",cell.CellType,listType[n],sheet.SheetName));
                                    return;
                                }
                            }

                            ++index;
                        }
                        
                        // ++i;
                    }
                
                    if(bMap)
                    {
                        jsonFeildName = jsonFeild.ContainsKey(key) ? (string)jsonFeild[key] : key;
                        for( int t =0; t < array.Count; ++t)
                        {
                            item = (JObject)array[t];
                            jArray = (JArray)jMap[item[jsonFeildName].ToString()];
                            ConvertSheetToJsonArray(jsonFeild,jsonKey,dic, keyTableName,mapTableName, sheet,jArray,ref item); 
                        }
                    }
                }
                else
                {
                    JObject item = null;
                    
                    for(int ttt = 0; ttt < jIndexList.Count; ++ttt ) 
                    {
                        i = (int)jIndexList[ttt];
                        headerRow = sheet.GetRow(i);
                        ALFUtils.Assert(headerRow != null,"4444 headerRow == null!!!");
                        item = new JObject();
                        index = 0;
                        
                        for( int n = 0; n < indexList.Count; ++n)
                        {
                            jsonFeildName = (string )jsonFeild[list[index]];
                            ICell cell = headerRow.GetCell(indexList[n]);
                            if(cell == null || cell.CellType == CellType.Blank )//|| (cell.CellType == CellType.String  && cell.StringCellValue.StartsWith("#")))
                            {
                                // EditorUtility.ClearProgressBar();
                                // ALF.ALFUtils.Assert(false,string.Format("{0} cell = null  ({1})",list[index],sheet.SheetName));
                                switch(listType[index])
                                {
                                    case "bool":
                                        item[jsonFeildName] = false;
                                    break;
                                    case "long":
                                    case "uint":
                                    case "int":
                                    case "float":
                                    case "ubyte":
                                    case "byte":
                                    case "ulong":
                                    case "ushort":
                                    item[jsonFeildName] = 0;
                                    break;
                                    case "string":
                                        item[jsonFeildName] = "";
                                    break;
                                    default:
                                        EditorUtility.ClearProgressBar();
                                        ALF.ALFUtils.Assert(false,string.Format("111 {1} {0} 해당 값이 없습니다. ({2})",cell.CellType,listType[index],sheet.SheetName));
                                    return;
                                }
                            }
                            else
                            {
                                switch(listType[index])
                                {
                                    case "bool":
                                        item[jsonFeildName] = cell.BooleanCellValue;
                                    break;
                                    case "ushort":
                                        item[jsonFeildName] = (ushort)cell.NumericCellValue;
                                    break;
                                    case "uint":
                                        item[jsonFeildName] = (uint)cell.NumericCellValue;
                                    break;
                                    case "int":
                                        item[jsonFeildName] = (int)cell.NumericCellValue;
                                    break;
                                    case "long":
                                        item[jsonFeildName] = (long)cell.NumericCellValue;
                                    break;
                                    case "ulong":
                                        item[jsonFeildName] = (ulong)cell.NumericCellValue;
                                    break;
                                    case "ubyte":
                                        item[jsonFeildName] = (byte)cell.NumericCellValue;
                                        break;
                                    case "byte":
                                        item[jsonFeildName] = (sbyte)cell.NumericCellValue;
                                    break;
                                    case "float":
                                        item[jsonFeildName] = (float)cell.NumericCellValue;
                                    break;
                                    case "string":
                                    {
                                        try
                                        {
                                            item[jsonFeildName] = cell.StringCellValue;
                                        }
                                        catch{
                                            if(cell.CellType == CellType.Numeric)
                                            {
                                                item[jsonFeildName] = cell.NumericCellValue.ToString();
                                            }
                                            else
                                            {
                                                ALF.ALFUtils.Assert(false,string.Format("{5} {1} {0} 해당 값이 없습니다. ({2}) {3}  {4}",cell.CellType,listType[index],sheet.SheetName,jsonFeildName,indexList[n],i));
                                                return;
                                            }
                                        }
                                    }

                                    break;
                                    default:
                                        EditorUtility.ClearProgressBar();
                                        ALF.ALFUtils.Assert(false,string.Format("{5} {1} {0} 해당 값이 없습니다. ({2}) jsonFeildName:{3}  {4}",cell.CellType,listType[index],sheet.SheetName,jsonFeildName,indexList[n],i));
                                    return;
                                }
                            }

                            ++index;
                        }
                        
                        array.Add(item);
                        ++i;
                    }
                }
            }
            else
            {    
                for(int ttt = 0; ttt < jIndexList.Count; ++ttt ) 
                {
                    i = (int)jIndexList[ttt];
                    headerRow = sheet.GetRow(i);
                    ALFUtils.Assert(headerRow != null,"4444 headerRow == null!!!");
                    index = 0;
                    
                    for( int n = 0; n < indexList.Count; ++n)
                    {
                        ICell cell = headerRow.GetCell(indexList[n]);
                        if(cell == null || cell.CellType == CellType.Blank )//|| (cell.CellType == CellType.String  && cell.StringCellValue.StartsWith("#")))
                        {
                            // EditorUtility.ClearProgressBar();
                            // ALF.ALFUtils.Assert(false,string.Format("{0} cell = null  ({1})",list[index],sheet.SheetName));
                            switch(listType[index])
                            {
                                case "bool":
                                    array.Add(false);
                                break;
                                case "long":
                                case "uint":
                                case "int":
                                case "float":
                                case "ubyte":
                                case "byte":
                                case "ulong":
                                case "ushort":
                                array.Add(0);
                                break;
                                case "string":
                                array.Add("");
                                break;
                                default:
                                    EditorUtility.ClearProgressBar();
                                    ALF.ALFUtils.Assert(false,string.Format("111 {1} {0} 해당 값이 없습니다. ({2})",cell.CellType,listType[index],sheet.SheetName));
                                return;
                            }
                        }
                        else
                        {
                            switch(listType[index])
                            {
                                case "bool":
                                    array.Add(cell.BooleanCellValue);
                                break;
                                case "ushort":
                                    array.Add((ushort)cell.NumericCellValue);
                                break;
                                case "uint":
                                    array.Add((uint)cell.NumericCellValue);
                                break;
                                case "int":
                                    array.Add((int)cell.NumericCellValue);
                                break;
                                case "long":
                                    array.Add((long)cell.NumericCellValue);
                                break;
                                case "ulong":
                                    array.Add((ulong)cell.NumericCellValue);
                                break;
                                case "ubyte":
                                    array.Add((byte)cell.NumericCellValue);
                                    break;
                                case "byte":
                                    array.Add((sbyte)cell.NumericCellValue);
                                break;
                                case "float":
                                    array.Add((float)cell.NumericCellValue);
                                break;
                                case "string":
                                {
                                    try
                                    {
                                        array.Add(cell.StringCellValue);
                                    }
                                    catch{
                                        if(cell.CellType == CellType.Numeric)
                                        {
                                            array.Add(cell.NumericCellValue.ToString());
                                        }
                                        else
                                        {
                                            ALF.ALFUtils.Assert(false,string.Format("{5} {1} {0} 해당 값이 없습니다. ({2}) {3}  {4}",cell.CellType,listType[index],sheet.SheetName,jsonFeildName,indexList[n],i));
                                            return;
                                        }
                                    }
                                }

                                break;
                                default:
                                    EditorUtility.ClearProgressBar();
                                    ALF.ALFUtils.Assert(false,string.Format("{5} {1} {0} 해당 값이 없습니다. ({2}) jsonFeildName:{3}  {4}",cell.CellType,listType[index],sheet.SheetName,jsonFeildName,indexList[n],i));
                                return;
                            }
                        }
                        ++index;
                    }
                }
            }
            
            jsonFeildName = jsonFeild.ContainsKey(id) ? (string)jsonFeild[id] : id;
            json[jsonFeildName] = array;
        }

        static string GetCellValueByIndex(IRow row,int index)
        {
            ICell cell = row.GetCell(index);
            if(cell == null || cell.CellType == CellType.Blank || cell.CellType != CellType.String || cell.StringCellValue.StartsWith("#"))
                return null;
            
            return cell.StringCellValue;
        }

        static List<string> GetFieldNamesFromSheetHeader(ISheet sheet)
        {
            IRow headerRow = sheet.GetRow(0);
            ALFUtils.Assert(headerRow != null,"GetFieldNamesFromSheetHeader headerRow == null!!!");
            var fieldNames = new List<string>();
            for (int i = 0; i < headerRow.LastCellNum; i++)
            {
                var cell = headerRow.GetCell(i);
                if(cell == null || cell.CellType == CellType.Blank) break;
                fieldNames.Add(cell.StringCellValue);
            }
            return fieldNames;
        }

        static IWorkbook LoadBook(string excelPath)
        {
            using (FileStream stream = File.Open(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (Path.GetExtension(excelPath) == ".xls") return new HSSFWorkbook(stream);
                else return new XSSFWorkbook(stream);
            }
        }

        
        static string GetPathFLATC()
        {
            if(Application.platform == RuntimePlatform.WindowsEditor)
            {
                #if UNITY_EDITOR_64
                return Path.Combine(ROOT, "FlatBuffer/Win64/flatc.exe");
                #else
                return Path.Combine(ROOT, "FlatBuffer/Win32/flatc.exe");
                #endif
            }
            else if(Application.platform == RuntimePlatform.OSXEditor)
            {
                return Path.Combine(ROOT, "FlatBuffer/OSX/flatc");
            }

            return null;
        }

        [MenuItem("ALF/Data/Set Data Folder")]
        static void SetDataFolder() 
        {
            string path = EditorUtility.OpenFolderPanel("Select Data folder", EditorPrefs.GetString($"{Application.productName}:dataPath",""),"");
            if (!string.IsNullOrEmpty(path))
            {
                path = path.Substring(ROOT.Length).Replace("\\", "/");
                EditorPrefs.SetString($"{Application.productName}:dataPath",path);
            }
        }

        // [MenuItem("ALF/Data/Data Export1")]
        // static void BinExport1() 
        // {
        //     TextAsset tt = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/Untitled-1.txt", typeof(TextAsset));
        //     TextAsset tt1 = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/crc_pvrfnlist_167882_21281.txt", typeof(TextAsset));

        //     Dictionary<string,string> list = new Dictionary<string,string>();
        //     Dictionary<string,string> list1 = new Dictionary<string,string>();
            
        //     string[] templist = tt.text.Split('\n');

        //     for(int i = 0; i < templist.Length; ++i)
        //     {
        //         string[] templist1 = templist[i].Split(',');
        //         list.Add(templist1[0],templist1[2]);
        //     }

        //     string ttt = "";
        //     string[] templist2 = tt1.text.Split('\n');
            
        //     for(int i = 0; i < templist2.Length; ++i)
        //     {
        //         string[] templist1 = templist2[i].Split(',');
        //         if(templist1.Length >= 3)
        //         {
        //             if(list.ContainsKey(templist1[0]))
        //             {
        //                 ttt += string.Format("{0},{1},{2}",templist1[0],templist1[1],list[templist1[0]]);

        //                 for(int t = 3; t < templist1.Length; ++t)
        //                 {
        //                     ttt += string.Format(",{0}",templist1[t]);
        //                 }
        //                 ttt += "\n";
        //             }
        //             else
        //             {
        //                 ttt += string.Format("{0}\n",templist2[i]);
        //             }
        //         }
        //     }

        //     File.WriteAllText(string.Format("{0}/test",Application.dataPath), ttt);
        // }

        // static string ReplaceVersion(string strSource, string strReplace)
        // {
        //     string strStart = "public class GameContext : IBase\n{\n";
        //     string strEnd = "public readonly static int CHALLENGE_ID";
            
        //     if (strSource.Contains(strStart) && strSource.Contains(strEnd))
        //     {
        //         int Start = strSource.IndexOf(strStart, 0) + strStart.Length;
        //         int End = strSource.IndexOf(strEnd, Start);
        //         strStart = strSource.Substring(0,Start);
        //         strEnd = strSource.Substring(End);
        //         return strStart + "    public readonly static string Version = \"" + strReplace + "\";\n    " + strEnd;
        //     }

        //     ALFUtils.Assert(false, "string not find!!");
        //     return string.Empty;
        // }

        // [MenuItem("ALF/Data/Build Bundle")]
        // static void BuildBundle() 
        // {
        //     string filePath = Path.Combine(ROOT, "Assets/Script/Game/GameContext.cs");
        //     if(File.Exists(filePath))
        //     {
        //         string contents = File.ReadAllText(filePath);
        //         // UnityEngine.Debug.Log(contents);
        //         contents = ReplaceVersion(contents, "0.0.1");
        //         UnityEngine.Debug.Log(contents);
        //         File.WriteAllText( filePath, contents);
        //     }            
        // }

        [MenuItem("ALF/Data/UserData Export")]
        static void UserDataExport() 
        {
            List<string> parms = new List<string>();
            parms.Add("-o "+ Path.Combine(ROOT, "Assets/Data/Bin"));
            parms.Add("--csharp");
            parms.Add("--gen-object-api");
            parms.Add(Path.Combine(ROOT, "Assets/Data/Editor/TileEditor/Tools/UserData.fbs"));
            Export(parms,true);

            parms[3] =Path.Combine(ROOT, "Assets/Data/Editor/TileEditor/Tools/CachePlayerData.fbs");
            Export(parms,true);

            parms[3] =Path.Combine(ROOT, "Assets/Data/Editor/TileEditor/Tools/AdData.fbs");
            Export(parms,true);
            
            parms[3] =Path.Combine(ROOT, "Assets/Data/Editor/TileEditor/Tools/RestorePurchase.fbs");
            Export(parms,true);
        }

        [MenuItem("ALF/Data/MatchTeamData Export")]
        static void MatchTeamDataExport() 
        {
            List<string> parms = new List<string>();
            parms.Add("-o "+ Path.Combine(ROOT, "Assets/Data/Bin"));
            parms.Add("--csharp");
            parms.Add("--gen-object-api");
            parms.Add("--cs-gen-json-serializer");
            parms.Add(Path.Combine(ROOT, "Assets/Data/Editor/TileEditor/Tools/TeamData.fbs"));
            Export(parms,true);
            parms[4] = Path.Combine(ROOT, "Assets/Data/Editor/TileEditor/Tools/MatchTeamData.fbs");
            Export(parms,true);
            parms[4] = Path.Combine(ROOT, "Assets/Data/Editor/TileEditor/Tools/StatisticsRecord.fbs");
            Export(parms,true);
            parms[4] = Path.Combine(ROOT, "Assets/Data/Editor/TileEditor/Tools/StatisticsPlayers.fbs");
            Export(parms,true);
            parms[4] = Path.Combine(ROOT, "Assets/Data/Editor/TileEditor/Tools/StatisticsTeam.fbs");
            Export(parms,true);
            parms[4] = Path.Combine(ROOT, "Assets/Data/Editor/TileEditor/Tools/StatisticsData.fbs");
            Export(parms,true);
            parms[4] = Path.Combine(ROOT, "Assets/Data/Editor/TileEditor/Tools/MatchData.fbs");
            Export(parms,true);

            
            parms.Clear();
            parms.Add("--cpp");
            parms.Add(Path.Combine(ROOT, "Assets/Data/Editor/TileEditor/Tools/TeamData.fbs"));
            Export(parms,true);


            parms[1] = Path.Combine(ROOT, "Assets/Data/Editor/TileEditor/Tools/MatchTeamData.fbs");
            Export(parms,true);
            parms[1] = Path.Combine(ROOT, "Assets/Data/Editor/TileEditor/Tools/StatisticsPlayers.fbs");
            Export(parms,true);
            parms[1] = Path.Combine(ROOT, "Assets/Data/Editor/TileEditor/Tools/StatisticsTeam.fbs");
            Export(parms,true);
            parms[1] = Path.Combine(ROOT, "Assets/Data/Editor/TileEditor/Tools/StatisticsData.fbs");
            Export(parms,true);

            File.Copy(Path.Combine(ROOT, "StatisticsData_generated.h"),Path.Combine(ROOT, "../MatchEngine/MatchEngine/MatchEngine/StatisticsData_generated.h"),true);
            File.Copy(Path.Combine(ROOT, "StatisticsData_generated.h"),Path.Combine(ROOT, "../MatchEngine_cocos2dx/Classes/StatisticsData_generated.h"),true);

            File.Copy(Path.Combine(ROOT, "MatchTeamData_generated.h"),Path.Combine(ROOT, "../MatchEngine/MatchEngine/MatchEngine/MatchTeamData_generated.h"),true);
            File.Copy(Path.Combine(ROOT, "MatchTeamData_generated.h"),Path.Combine(ROOT, "../MatchEngine_cocos2dx/Classes/MatchTeamData_generated.h"),true);

            File.Copy(Path.Combine(ROOT, "StatisticsPlayers_generated.h"),Path.Combine(ROOT, "../MatchEngine/MatchEngine/MatchEngine/StatisticsPlayers_generated.h"),true);
            File.Copy(Path.Combine(ROOT, "StatisticsPlayers_generated.h"),Path.Combine(ROOT, "../MatchEngine_cocos2dx/Classes/StatisticsPlayers_generated.h"),true);

            File.Copy(Path.Combine(ROOT, "StatisticsTeam_generated.h"),Path.Combine(ROOT, "../MatchEngine/MatchEngine/MatchEngine/StatisticsTeam_generated.h"),true);
            File.Copy(Path.Combine(ROOT, "StatisticsTeam_generated.h"),Path.Combine(ROOT, "../MatchEngine_cocos2dx/Classes/StatisticsTeam_generated.h"),true);

            File.Copy(Path.Combine(ROOT, "TeamData_generated.h"),Path.Combine(ROOT, "../MatchEngine/MatchEngine/MatchEngine/TeamData_generated.h"),true);
            File.Copy(Path.Combine(ROOT, "TeamData_generated.h"),Path.Combine(ROOT, "../MatchEngine_cocos2dx/Classes/TeamData_generated.h"),true);

        }

        // [MenuItem("ALF/Data/FBS Export")]
        // static void FBSExport() 
        // {
        //     List<string> parms = new List<string>();
        //     // parms.Add("-o "+ Path.Combine(Application.dataPath, "Script/Game/Data"));
        //     parms.Add("-o "+ Path.Combine(ROOT, "Assets/Data/Bin"));
        //     parms.Add("--csharp");
        //     // parms.Add("--cs-gen-json-serializer");
        //     parms.Add(Path.Combine(ROOT, "Assets/Data/Editor/TileEditor/Tools/MazeMap.fbs"));
        //     parms.Add(Path.Combine(ROOT, "Assets/Data/Editor/TileEditor/Tools/String.fbs"));
        //     Export(parms,true);
        // }

        public static void Export(List<string> parms,bool bData)
        {
            if(!bData)
            {
                string outPath = Path.Combine(ROOT, "Assets/Data/Bin");

                string[] directories = Directory.GetDirectories(outPath);
                for(int i = 0; i< directories.Length; i++)
                {
                    DirectoryInfo di = new DirectoryInfo(directories[i]);
                    di.Delete(true);  
                }

                // string[] sources = Directory.GetFiles(outPath);
                // for(int i = 0; i< sources.Length; i++)
                // {
                //     File.Delete(sources[i]);
                // }
            }
            
            // Directory.Delete(path);
            // AssetDatabase.Refresh();
            // string root = Application.dataPath.Substring(0, Application.dataPath.Length-6);
            // UnityEngine.Debug.Log("-----root:"+root);
            ShellRunner.Run(GetPathFLATC(),string.Join(" ", parms.ToArray()) );
            AssetDatabase.Refresh();
        }


        [MenuItem("ALF/Data/Excel Export")]
        static void BinExcelExport() 
        {
            ExportListPopupWindowContent popupContent = (ExportListPopupWindowContent)System.Activator.CreateInstance(typeof(ExportListPopupWindowContent));
            popupContent.SetExportDataType(E_EXPORT_DATA_TYPE.EXCEL);

            UnityEditor.PopupWindow.Show(new Rect(100, 100, 100, 100), popupContent);
        }

        [MenuItem("ALF/Data Clear")]
        static void PlayerPrefsClear()
        {
            PlayerPrefs.DeleteAll();
        }

        [MenuItem("ALF/Guest Reset")]
        static void GuestReset()
        {
            string uuid = PlayerPrefs.GetString("uuid");
            PlayerPrefs.DeleteAll();

            if(string.IsNullOrEmpty(uuid))
            {
                uuid = SystemInfo.deviceUniqueIdentifier;
            }
            else
            {
                uuid = SystemInfo.deviceUniqueIdentifier + System.DateTime.UtcNow.ToString("u", System.Globalization.DateTimeFormatInfo.InvariantInfo);
            }
            UnityEngine.Debug.Log(uuid);
            PlayerPrefs.SetInt("LLSP",0);
            PlayerPrefs.SetString("uuid",uuid);
            PlayerPrefs.Save();
        }

#if UNITY_IOS
        [MenuItem("ALF/TestCode")]
        static void TestCode()
        {
            string path = "/Users/mac/Documents/work/UnityProject/VSM_work/VSM/Build/iOS";
            string projPath = PBXProject.GetPBXProjectPath(path);
            var project = new PBXProject();
            project.ReadFromFile(projPath);

    #if UNITY_2019_3_OR_NEWER
            string mainTarget = project.GetUnityMainTargetGuid();
    #else
            string targetName = PBXProject.GetUnityTargetName();
            string mainTarget = project.TargetGuidByName(targetName);
    #endif
            string unityFramework = project.GetUnityFrameworkTargetGuid();

            // string fileGuid = project.AddFile(Path.Combine(path, "GoogleService-Info.plist"), "GoogleService-Info.plist", PBXSourceTree.Source);
            
            // fileGuid = project.AddFile(Path.Combine(path, "GoogleService-Info.plist"), "GoogleService-Info.plist", PBXSourceTree.Source);


            string fileGuid = project.FindFileGuidByRealPath(Path.Combine(path,"Libraries/HiveSDK/hive.androidlib"), PBXSourceTree.Source);
        // fileGuid = project.FindFileGuidByRealPath("Libraries/HiveSDK/hive.androidlib",PBXSourceTree.Group);
            // fileGuid = "FD694A7DA531229FE32C546C";
            // project.RemoveFile(fileGuid);
            // project.RemoveFileFromBuild(unityFramework, fileGuid);

            // Debug.Log($"-------------fileGuid:{fileGuid} . :{project.ContainsFileByRealPath("Libraries/HiveSDK/hive.androidlib",PBXSourceTree.Source)}");
            var itr = project.GetRealPathsOfAllFiles(PBXSourceTree.Source);
            foreach(var k in itr)
            {
                if(k.Contains("hive.androidlib"))
                {
                    Debug.Log($"-------------GetRealPathsOfAllFiles:{k}");
                    fileGuid = project.FindFileGuidByRealPath(k,PBXSourceTree.Source);
                    Debug.Log(fileGuid);
                    project.RemoveFile(fileGuid);
                }
                
            }
            project.WriteToFile(projPath);
        }

    #endif

    }
#endif


    public class ExportListPopupWindowContent : PopupWindowContent
    {
        // static string ROOT = Application.dataPath.Substring(0,Application.dataPath.IndexOf("Assets")).Replace("\\", "/");
        // protected bool bClickOk = false;
        protected string searchString = string.Empty;
        protected SearchField searchField = null;

        ListView<ExportDataViewItem> exportFileListView;
        ExportListViewDelegate exportFileListViewDelegate;
        E_EXPORT_DATA_TYPE exportType = E_EXPORT_DATA_TYPE.NONE;

        List<ExportDataViewItem> fileDataList = new List<ExportDataViewItem>();

        ExportDataViewItem selectItem = null;

        public void SetExportDataType(E_EXPORT_DATA_TYPE type) 
        {
            exportType = type;
        }

        public override void OnGUI(Rect rect)
        {
            EditorGUILayout.LabelField ("List:");
            // MazeGeneratorEditor.DrawUILine(Color.black);
            GUILayout.Label("Search", EditorStyles.boldLabel);
            
            Draw(asToolbar:false);
            Rect controlRect = EditorGUILayout.GetControlRect(GUILayout.Height(150), GUILayout.ExpandWidth(true));
            exportFileListView.OnGUI(controlRect);
            
            // MazeGeneratorEditor.DrawUILine(Color.grey);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button( selectItem == null ? "Export All" :"Export"))
            {
                if(exportType == E_EXPORT_DATA_TYPE.FBS)
                {

                }
                else if(exportType == E_EXPORT_DATA_TYPE.MAP)
                {
                    MakeDataBin(selectItem);
                }
                else if(exportType == E_EXPORT_DATA_TYPE.EXCEL)
                {
                    DataExport.MakeExcelToFBS(selectItem,editorWindow);
                }

                // string msg = null;
                // if(exportType == E_EXPORT_DATA_TYPE.NONE)
                // {
                //     msg = "E_EXPORT_DATA_TYPE == NONE !!";
                // }

                // if(!string.IsNullOrEmpty(msg))
                // {
                //     EditorUtility.DisplayDialog("확인해주세요!!",msg, "확인");
                // }
                // else
                // {
                //     bClickOk = true;
                //     // editorWindow.Close();
                // }   
            }
            if(GUILayout.Button( "Close"))
            {
                editorWindow.Close();
            }
            EditorGUILayout.EndHorizontal();
        }
        
        protected void Draw(bool asToolbar)
		{
			var rect = GUILayoutUtility.GetRect(1, 1, 18, 18, GUILayout.ExpandWidth(true));
			GUILayout.BeginHorizontal();
			DoSearchField(rect, asToolbar);
			GUILayout.EndHorizontal();
			rect.y += 18;
			// DoResults(rect);
		}
        protected void DoSearchField(Rect rect, bool asToolbar)
		{
			if(searchField == null)
			{
				searchField = new SearchField();
				// searchField.downOrUpArrowKeyPressed += OnDownOrUpArrowKeyPressed;
			}

			string resultString = asToolbar
				? searchField.OnToolbarGUI(rect, searchString)
				: searchField.OnGUI(rect, searchString);

			if (resultString != searchString )
			{
                OnSearchInputChanged(resultString);
			}

			searchString = resultString;
		}

        public override void OnOpen()
        {
            base.OnOpen();

            ALFUtils.Assert(exportType != E_EXPORT_DATA_TYPE.NONE, "exportType == E_EXPORT_DATA_TYPE.NONE !!");
            
            string dataPath = EditorPrefs.GetString($"{Application.productName}:dataPath");
            ALFUtils.Assert(!string.IsNullOrEmpty(dataPath), "dataPath == null !!");

            exportFileListViewDelegate = ExportListViewDelegate.Create();
            exportFileListView = new ListView<ExportDataViewItem>(exportFileListViewDelegate);
            exportFileListViewDelegate.SetClickCallback(this.OnListViewClick);
            selectItem = null;
            if(exportType == E_EXPORT_DATA_TYPE.FBS)
            {

            }
            else if(exportType == E_EXPORT_DATA_TYPE.MAP)
            {
                dataPath = Path.Combine(dataPath, "MapData");
                ExportDataViewItem item = null;
			    DirectoryInfo dirInfo = new DirectoryInfo(dataPath);
                List<DirectoryInfo> directoryList = dirInfo.GetDirectories().ToList<DirectoryInfo>();
                
                int type = 0;
                int id = 0;
                
                if(directoryList.Count > 0)
                {
                    directoryList.Sort(delegate(DirectoryInfo x, DirectoryInfo y)
                    {
                        string[] names = x.Name.Split('x');
                        string[] names1 = y.Name.Split('x');
                        int value = int.Parse(names[0]) - int.Parse(names1[0]);
                        if(value == 0)
                        {
                            value = int.Parse(names[1]) - int.Parse(names1[1]);
                        }
                        
                        return value;
                    });

                    for(int i = 0; i < directoryList.Count; ++i)
                    {
                        DirectoryInfo[] directory = directoryList[i].GetDirectories();
                        
                        for(int t = 0; t < directory.Length; ++t)
                        {
                            if (Int32.TryParse (directory[t].Name, out type)) 
                            {
                                List<FileInfo> files = directory[t].GetFiles("*.json").ToList<FileInfo>();
                                files.Sort(delegate(FileInfo x, FileInfo y)
                                {                                    
                                    string[] names = (x.Name.Substring(0, x.Name.LastIndexOf("."))).Split('_');
                                    string[] names1 = (y.Name.Substring(0, y.Name.LastIndexOf("."))).Split('_');
                                    return int.Parse(names[1]) - int.Parse(names1[1]);

                                });

                                for(int n = 0; n < files.Count; ++n)
                                {
                                    item = new ExportDataViewItem(id,directoryList[i].Name,type,files[n].Name);
                                    fileDataList.Add(item);
                                    exportFileListViewDelegate.Add(item);
                                    ++id;
                                }       
                            }
                        }
                    }

                }
            }
            else if(exportType == E_EXPORT_DATA_TYPE.EXCEL)
            {
                dataPath = Path.Combine(dataPath, "ExcelData");

			    DirectoryInfo dirInfo = new DirectoryInfo(dataPath);
			    FileInfo[] files = dirInfo.GetFiles("*.xls*");
                List<string> excludeList = DataExport.GetExcludeFileList();
                for(int i = 0; i < files.Length; ++i)
                {
                    if(excludeList.Contains(files[i].Name)) continue;
                    fileDataList.Add(new ExportDataViewItem(fileDataList.Count,"", fileDataList.Count, files[i].Name));
                    exportFileListViewDelegate.Add(fileDataList[fileDataList.Count -1]);
                }
            }
            
            exportFileListView.Refresh();
        }

        public override void OnClose()
        {
            exportFileListViewDelegate.OnDestroy();
            exportFileListViewDelegate = null;
            exportFileListView = null;
            fileDataList.Clear();
            searchString = null;
            searchField = null;
            selectItem = null;
            base.OnClose();
        }
        
        public override Vector2 GetWindowSize()
        {
            //Popup 의 사이즈
            return new Vector2(500, 400);
        }

        void OnListViewClick(ExportDataViewItem item)
        {
            selectItem = item;
        }

        void OnSearchInputChanged(string searchString)
        {
            exportFileListViewDelegate.ClearAll();
            bool bAdd = string.IsNullOrEmpty(searchString);
            
            foreach (ExportDataViewItem item in fileDataList)
            {
                if(bAdd || item.displayName.IndexOf(searchString) == 0 )
                {
                    exportFileListViewDelegate.Add(item);
                }
            }

            exportFileListView.Refresh();
        }

        void SetupJsonData(ExportDataViewItem item,string dataPath,ref Dictionary<string, JObject> dic)
        {
            if(item != null)
            {
                string size = item.Size;
                int type = item.depth;
                JObject temp = null;
                JArray tempArray = null;
                
                if(dic.ContainsKey(size))
                {
                    temp = dic[size]; 
                    JArray jArray = (JArray)(temp["list"]);
                    for(int i = 0; i < jArray.Count; ++i)
                    {
                        if((int)(jArray[i]["type"]) == type)
                        {
                            tempArray = (JArray)(jArray[i]["list"]);
                            break;
                        }
                    }

                    if(tempArray == null)
                    {
                        tempArray = new JArray();
                        JObject _obj = new JObject();
                        
                        _obj["type"] = type;
                        _obj["list"] = tempArray;
                        jArray.Add(_obj);
                    }
                }
                else
                {
                    temp = new JObject();
                    JObject _obj = new JObject();
                    tempArray = new JArray();
                    
                    _obj["type"] = type;
                    _obj["list"] = tempArray;
                    
                    JArray jArray = new JArray();
                    jArray.Add(_obj);
                    temp["list"] = jArray; 
                    
                    dic.Add(size, temp);
                }

                JObject obj = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(File.ReadAllText(Path.Combine(dataPath, size,$"{type}",item.displayName)), new Newtonsoft.Json.JsonSerializerSettings {DateParseHandling = Newtonsoft.Json.DateParseHandling.None});
                
                if(!temp.ContainsKey("size"))
                {
                    temp["size"] = obj["size"];
                }
                tempArray.Add(obj);
            }
        }

        void MakeDataBin(ExportDataViewItem item)
        {
            float progressBar = 0.0f;
            EditorUtility.DisplayProgressBar("Placing Prefabs","Working...",progressBar);
            
            try
            {
                string dataPath = EditorPrefs.GetString($"{Application.productName}:dataPath");
                dataPath = Path.Combine(DataExport.GetRootPath(), dataPath, "MapData");
                
                Dictionary<string, JObject> dic = new Dictionary<string, JObject>();

                if(item != null)
                {
                    SetupJsonData(item, dataPath,ref dic);
                }
                else
                {
                    for(int i = 0; i < fileDataList.Count; ++i)
                    {
                        SetupJsonData(fileDataList[i], dataPath,ref dic);                        
                    }
                }

                JObject jObject = new JObject();
                JArray pJArray = new JArray();
                
                List<string> keys = dic.Keys.ToList();
                for(int i =0; i < keys.Count; ++i)
                {
                    pJArray.Add(dic[keys[i]]); 
                }

                jObject["maze_list"] = pJArray;
                
                string filePath = Path.Combine(dataPath, "MazeAll.json");
                File.WriteAllText( filePath, jObject.ToString());

                EditorUtility.DisplayProgressBar("Placing Prefabs","100% done...",progressBar);

                List<string> parms = new List<string>();
                parms.Add("-o "+ Path.Combine(DataExport.GetRootPath(), "Assets/Data/Bin"));
                parms.Add("--csharp");
                parms.Add("--binary");
                parms.Add(Path.Combine(DataExport.GetRootPath(), "Assets/Data/Editor/TileEditor/Tools/MazeMap.fbs"));
                parms.Add(filePath);
                DataExport.Export(parms,true);
                
                File.Delete(filePath);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e.Message);
            }
            
            EditorUtility.ClearProgressBar();
            editorWindow.Close();
        }

        // void ChangeKey(JObject parent, JToken token, string newKey) 
        // {
        //     var tokenProp = token as JProperty;
        //     var oldKeyName = tokenProp.Name;
        //     parent[newKey] = tokenProp.Value;
        //     parent.Remove(oldKeyName);
        // }

        // void ConverFeildName(JObject parent)
        // {
        //     var itr = parent.GetEnumerator();
        //     while(itr.MoveNext())
        //     {
        //         string oldString = (string)itr.Current.Key;
        //         string newString = ChangeFeildName(oldString);
        //         if(oldString != newString)
        //         {
        //             ChangeKey(parent, itr.Current.Value, newString); 
        //         }
        //     }
        // }

    }

}