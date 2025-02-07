using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using System.IO;
using System;
using System.Xml;
// using SimpleJSON;

using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;


public static class BuildTool
{
    static void CopyFolder(string _SourceFolder, string _DestFolder)
    {
        if (!Directory.Exists(_DestFolder))
            Directory.CreateDirectory(_DestFolder);

        string[] _Files = Directory.GetFiles(_SourceFolder);
        string[] _Folders = Directory.GetDirectories(_SourceFolder);

        foreach (string _File in _Files)
        {
            string _Name = Path.GetFileName(_File);
            string _Dest = Path.Combine(_DestFolder, _Name);
            File.Move(_File, _Dest);
        }

        foreach (string _Folder in _Folders)
        {
            string _Name = Path.GetFileName(_Folder);
            string _Dest = Path.Combine(_DestFolder, _Name);
            CopyFolder(_Folder, _Dest);
        }
    }

    static void Android_Jenkins()
    {
        PlayerSettings.bundleVersion = GetCmdArgAsString("-bundleVer", "1.0.0");
        PlayerSettings.Android.bundleVersionCode = GetCmdArgAsInt("-buildNo", 1);
        
        Android(GetCmdArgAsString("-service","real"),GetCmdArgAsString("-store","google"),GetCmdArgAsString("-isAAB", "APK"),GetCmdArgAsInt("-count",0));
    }

    static XmlNode FindChildNode(XmlNode parent, string name)
    {
        XmlNode curr = parent.FirstChild;
        while (curr != null)
        {
            if (curr.Name.Equals(name))
            {
                return curr;
            }

            curr = curr.NextSibling;
        }

        return null;
    }

    static bool TryFindElementWithAndroidName(XmlNode parent,string attrNameValue,out XmlElement element,string elementType = "activity")
    {
        string ns = parent.GetNamespaceOfPrefix("android");
        var curr = parent.FirstChild;
        while (curr != null)
        {
            var currXmlElement = curr as XmlElement;
            if (currXmlElement != null &&
                currXmlElement.Name == elementType &&
                currXmlElement.GetAttribute("name", ns) == attrNameValue)
            {
                element = currXmlElement;
                return true;
            }

            curr = curr.NextSibling;
        }

        element = null;
        return false;
    }

    static void SetOrReplaceXmlElement(XmlNode parent,XmlElement newElement)
    {
        string attrNameValue = newElement.GetAttribute("name");
        string elementType = newElement.Name;

        XmlElement existingElment;
        if (TryFindElementWithAndroidName(parent, attrNameValue, out existingElment, elementType))
        {
            parent.ReplaceChild(newElement, existingElment);
        }
        else
        {
            parent.AppendChild(newElement);
        }
    }
    static void UpdateManifest(string fullPath,string appId)
    {
        XmlDocument doc = new XmlDocument();

        doc.Load(fullPath);

        if (doc == null)
        {
            Debug.LogError("Couldn't load " + fullPath);
            return;
        }

        XmlNode manifestNode = FindChildNode(doc, "manifest");
        XmlElement pXmlElement;
        // XmlElement pXmlElement = doc["manifest"];
        // pXmlElement.SetAttribute("package", PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android));

        XmlNode applicationNode = FindChildNode(manifestNode, "application");

        if (applicationNode == null)
        {
            Debug.LogError("Error parsing " + fullPath);
            return;
        }

        string ns = applicationNode.GetNamespaceOfPrefix("android");
    
        if (TryFindElementWithAndroidName(applicationNode, "com.google.android.gms.ads.APPLICATION_ID", out pXmlElement, "meta-data")) 
        {
            applicationNode.RemoveChild(pXmlElement);
        }

        pXmlElement = doc.CreateElement("meta-data");
        pXmlElement.SetAttribute("name", ns, "com.google.android.gms.ads.APPLICATION_ID" );
        pXmlElement.SetAttribute("value", ns, appId);
        SetOrReplaceXmlElement(applicationNode, pXmlElement);   
        
        // Save the document formatted
        XmlWriterSettings settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\r\n",
            NewLineHandling = NewLineHandling.Replace
        };

        using (XmlWriter xmlWriter = XmlWriter.Create(fullPath, settings))
        {
            doc.Save(xmlWriter);
        }
    }

    static void Android(string service = "real",string strStore = "google", string strAAB = "APK",int no = 0)
    {
        // getSettingsObject(settings_asset);

        if(EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }

        PlayerSettings.Android.resizableWindow = true;

        string androidManifest = Path.Combine(Application.dataPath, "Plugins/Android/AndroidManifest.xml");
        string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup ( BuildTargetGroup.Android );
        List<string> allDefines = definesString.Split ( ';' ).ToList();
        string appId = null;
        string ROOT = Application.dataPath.Substring(0,Application.dataPath.IndexOf("Assets")).Replace("\\", "/");
        if(strStore == "google")
        {
            // PlayerSettings.productName = "FTM_1";
            PlayerSettings.Android.keystoreName = Path.Combine(ROOT,"ftm.keystore");
            appId = "ca-app-pub-5466681013076873~4149094691";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.woncomz.ftm.google");
            if(allDefines.Contains("ONESTORE"))
            {
                allDefines.Remove("ONESTORE");
            }
        }
        else if(strStore == "onestore")
        {
            // PlayerSettings.productName = "FTM_2";
            PlayerSettings.Android.keystoreName = Path.Combine(ROOT,"ftm_one.keystore");
            appId = "ca-app-pub-5466681013076873~1968023608";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.woncomz.ftm.one");

            if(!allDefines.Contains("ONESTORE"))
            {
                allDefines.Add("ONESTORE");
            }
        }

        if(!allDefines.Contains("USE_NETWORK"))
        {
            allDefines.Add("USE_NETWORK");
        }

        if(service == "real")
        {
#if USE_HIVE
            Hive.Unity.Editor.HiveConfigXML.Android.zone = Hive.Unity.Editor.HiveConfigXML.ZoneType.real;
            Hive.Unity.Editor.HiveConfigXML.Android.useLog = false;
#endif
            if(!allDefines.Contains("FTM_LIVE"))
            {
                allDefines.Add("FTM_LIVE");
            }
        }
        else
        {
#if USE_HIVE
            Hive.Unity.Editor.HiveConfigXML.Android.zone = Hive.Unity.Editor.HiveConfigXML.ZoneType.sandbox;
            Hive.Unity.Editor.HiveConfigXML.Android.useLog = true;
#endif
            if(allDefines.Contains("FTM_LIVE"))
            {
                allDefines.Remove("FTM_LIVE");
            }
        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup (BuildTargetGroup.Android, string.Join ( ";", allDefines.ToArray ()));

#if USE_HIVE
        Hive.Unity.Editor.HiveConfigXML.Android.HIVEAppID = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
        Hive.Unity.Editor.HiveConfigXML.Android.commit();
#endif

        UpdateManifest(androidManifest, appId);

        string _Path = string.Format("{0}/Build/Android/", Directory.GetCurrentDirectory());
        string _FileName = string.Format("FTM_{0}({4})_{1}_{2}_({3})",service,PlayerSettings.bundleVersion,System.DateTime.Now.ToString("yyMMdd"),no,PlayerSettings.Android.bundleVersionCode);
        
        if (!Directory.Exists(_Path))
        {
            Directory.CreateDirectory(_Path);
        }

        string[] _Level = new string[EditorBuildSettings.scenes.Length];

        for (int i = 0; i < _Level.Length; i++)
        {
            if(EditorBuildSettings.scenes[i].enabled)
            {
                _Level[i] = EditorBuildSettings.scenes[i].path;
            }
        }

        PlayerSettings.Android.useCustomKeystore = true;

        PlayerSettings.Android.keystorePass = "woncomz0913!";
        PlayerSettings.Android.keyaliasName = "ftm";
        PlayerSettings.Android.keyaliasPass = "woncomz0913!";

        bool isAAb = strAAB == "AAB";//GetCmdArgAsInt("-isAAB", 1) == 1;
        EditorUserBuildSettings.buildAppBundle = isAAb;
        if(isAAb)
        {
            BuildPipeline.BuildPlayer(_Level, $"{_Path}{_FileName}_{strStore}.aab", BuildTarget.Android, BuildOptions.None);
        }
        else
        {
            BuildPipeline.BuildPlayer(_Level, $"{_Path}{_FileName}_{strStore}.apk", BuildTarget.Android, BuildOptions.None);
        }
        
        // settings.OverridePlayerVersion = "{GameContext.PatchVersion}";
        // AssetDatabase.Refresh();

        // if (PlayerSettings.Android.useAPKExpansionFiles)
        // {
        //     string _OBBPath = string.Format("{0}/Build/{1}/", Directory.GetCurrentDirectory(), PlayerSettings.applicationIdentifier);
        //     string _OBBName = string.Format("{0}/Build/{1}.main.obb", Directory.GetCurrentDirectory(), _FileName);
        //     string _OBBFile = string.Format("{0}/Build/{1}/main.{2}.{3}.obb", Directory.GetCurrentDirectory(), PlayerSettings.applicationIdentifier, PlayerSettings.Android.bundleVersionCode, PlayerSettings.applicationIdentifier);

        //     if (!Directory.Exists(_OBBPath))
        //     {
        //         Directory.CreateDirectory(_OBBPath);
        //     }

        //     if (File.Exists(_OBBFile))
        //     {
        //         File.Delete(_OBBFile);
        //     }

        //     FileInfo _FileInfo = new FileInfo(_OBBName);
        //     _FileInfo.MoveTo(_OBBFile);
        // }
    }

    [MenuItem("ALF/Build/Android ABB")]
    static void AndroidAPK_REAL()
    {
        Android( "real","google","ABB");
    }
    
    [MenuItem("ALF/Build/Android(one) APK")]
    static void AndroidAPK_ONE_DEV()
    {
        Android("dev","onestore","APK");
    }

    [MenuItem("ALF/Build/Android(google) APK")]
    static void AndroidAPK_ANDROID_DEV()
    {
        Android( "dev","google","APK");
    }

    [MenuItem("ALF/Build/iOS")]
    static void iOS()
    {
        // getSettingsObject(settings_asset);

        string _iOS = string.Format("{0}/Build/iOS", Directory.GetCurrentDirectory());
        if(Directory.Exists(_iOS))
        {
            Directory.Delete(_iOS, true);
        }

        Directory.CreateDirectory(_iOS);

        if(EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
        }

        PlayerSettings.SplashScreen.showUnityLogo = false;
        string bundleVersion = GetCmdArgAsString("-bundleVer", "");
        bool doShell = true;
        if(!string.IsNullOrEmpty(bundleVersion))
        {
            PlayerSettings.bundleVersion = bundleVersion;
            doShell = false;
        }

        bundleVersion = string.Format("{0}",GetCmdArgAsInt("-buildNo", 0));
        if(bundleVersion != "0")
        {
            PlayerSettings.iOS.buildNumber = bundleVersion;
            doShell = false;
        }

        // string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup ( EditorUserBuildSettings.selectedBuildTargetGroup );
        string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup ( BuildTargetGroup.iOS );
        List<string> allDefines = definesString.Split ( ';' ).ToList();
        
        if(!allDefines.Contains("USE_NETWORK"))
        {
            allDefines.Add("USE_NETWORK");
        }
        string service = GetCmdArgAsString("-service","dev");
        Debug.Log($" -service:---------------:{service}");
        if(service == "real")
        {
#if USE_HIVE
            Hive.Unity.Editor.HiveConfigXML.iOS.zone = Hive.Unity.Editor.HiveConfigXML.ZoneType.real;
            Hive.Unity.Editor.HiveConfigXML.iOS.useLog = false;
            Hive.Unity.Editor.HiveConfigXML.iOS.commit();
#endif
            if(!allDefines.Contains("FTM_LIVE"))
            {
                allDefines.Add("FTM_LIVE");
            }
        }
        else
        {
#if USE_HIVE
            Hive.Unity.Editor.HiveConfigXML.iOS.zone = Hive.Unity.Editor.HiveConfigXML.ZoneType.sandbox;
            Hive.Unity.Editor.HiveConfigXML.iOS.useLog = true;
            Hive.Unity.Editor.HiveConfigXML.iOS.commit();
#endif

            if(allDefines.Contains("FTM_LIVE"))
            {
                allDefines.Remove("FTM_LIVE");
            }
        }
        PlayerSettings.SetScriptingDefineSymbolsForGroup (BuildTargetGroup.iOS, string.Join ( ";", allDefines.ToArray ()));

        string[] _Level = new string[EditorBuildSettings.scenes.Length];

        for (int i = 0; i < _Level.Length; i++)
        {
            if(EditorBuildSettings.scenes[i].enabled)
            {
                _Level[i] = EditorBuildSettings.scenes[i].path;
            }
        }
        
        // Debug.Log($"public static string appleDeveloperTeamID {PlayerSettings.iOS.appleDeveloperTeamID }");
        // Debug.Log($"public static string bundleIdentifier {UnityEditor.PlayerSettings.applicationIdentifier }");
        
        BuildPipeline.BuildPlayer(_Level, "Build/iOS",BuildTarget.iOS,BuildOptions.None);
        
        // settings.OverridePlayerVersion = "{GameContext.PatchVersion}";
        // AssetDatabase.Refresh();
        if(doShell)
        {
            string ROOT = UnityEngine.Application.dataPath.Substring(0,UnityEngine.Application.dataPath.IndexOf("Assets")).Replace("\\", "/");
            if(Directory.Exists("/opt/homebrew/Cellar/cocoapods/"))
            {
                ShellRunner.Run("/opt/homebrew/Cellar/cocoapods/1.11.3/bin/pod","update --project-directory=" + ROOT +"Build/iOS" );
            }
            else if(Directory.Exists("/usr/local/Cellar/cocoapods/"))
            {
                ShellRunner.Run("/usr/local/Cellar/cocoapods/1.11.2_1/bin/pod","update --project-directory=" + ROOT +"Build/iOS" );
            }
            else
            {
                UnityEngine.Debug.Log("------------------- pod is not!!");
            } 
        }        
    }
    static int FindIndex<T>(this T[] source, Predicate<T> match)
	{
		if (source == null)
			return -1;
		return Array.FindIndex<T>(source, match);
	}

    static bool GetCmdArgAsBool(string argKey, bool defaultVal = false)
	{
		string[] args = Environment.GetCommandLineArgs();
		

		int argIdx = args.FindIndex(a => string.Compare(a, argKey, StringComparison.OrdinalIgnoreCase) == 0);
		if (argIdx != -1 && args.Length > argIdx + 1)
		{
			if (args[argIdx + 1] == "true")
				return true;
		}

		return defaultVal;
	}

    static int GetCmdArgAsInt(string argKey, int defaultVal = 0)
	{
		string[] args = Environment.GetCommandLineArgs();

		int argIdx = args.FindIndex(a => string.Compare(a, argKey, StringComparison.OrdinalIgnoreCase) == 0);
		if (argIdx != -1 && args.Length > argIdx + 1)
			return int.Parse(args[argIdx + 1]);

		return defaultVal;
	}

    static string GetCmdArgAsString(string argKey, string defaultVal = null)
	{
		string[] args = Environment.GetCommandLineArgs();

		int argIdx = args.FindIndex(a => string.Compare(a, argKey, StringComparison.OrdinalIgnoreCase) == 0);
		if (argIdx != -1 && args.Length > argIdx + 1)
		{
			string ret = args[argIdx + 1];
			return ret.StartsWith("-") ? defaultVal : args[argIdx + 1];
		}
		return defaultVal;
	}

    static void CopyGoogleServices(string _GoogleServices)
    {
        AssetDatabase.DeleteAsset("Assets/google-services");
        AssetDatabase.Refresh();
        AssetDatabase.CopyAsset("Assets/GoogleServices/" + _GoogleServices, "Assets/google-services.json");
    }

    static void CopyAndroidManifest(string _AndroidManifest)
    {
        AssetDatabase.DeleteAsset("Assets/Plugins/Android/AndroidManifest");
        AssetDatabase.Refresh();
        AssetDatabase.CopyAsset("Assets/AndroidManifest/" + _AndroidManifest, "Assets/Plugins/Android/AndroidManifest.xml");
    }

    static string build_script = "Assets/AddressableAssetsData/DataBuilders/BuildScriptPackedMode.asset";
    static string settings_asset = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
    static string profile_name = "Remote";
    private static AddressableAssetSettings settings;

    static void getSettingsObject(string settingsAsset) 
    {
        // This step is optional, you can also use the default settings:
        //settings = AddressableAssetSettingsDefaultObject.Settings;

        settings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(settingsAsset) as AddressableAssetSettings;
        
        if (settings == null)
            Debug.LogError($"{settingsAsset} couldn't be found or isn't " + $"a settings object.");

        settings.OverridePlayerVersion = "[GameContext.PatchVersion]";
        AssetDatabase.Refresh();
    }

    static void setProfile(string profile) 
    {
        string profileId = settings.profileSettings.GetProfileId(profile);
        if (String.IsNullOrEmpty(profileId))
            Debug.LogWarning($"Couldn't find a profile named, {profile}, " +
                                $"using current profile instead.");
        else
            settings.activeProfileId = profileId;
    }

    static void setBuilder(IDataBuilder builder) 
    {
        int index = settings.DataBuilders.IndexOf((ScriptableObject)builder);

        if (index > 0)
            settings.ActivePlayerDataBuilderIndex = index;
        else
            Debug.LogWarning($"{builder} must be added to the " +
                                $"DataBuilders list before it can be made " +
                                $"active. Using last run builder instead.");
    }

    static bool buildAddressableContent() 
    {
        AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
        bool success = string.IsNullOrEmpty(result.Error);

        settings.OverridePlayerVersion = "{GameContext.PatchVersion}";
        AssetDatabase.Refresh();
        if (!success) {
            Debug.LogError("Addressables build error encountered: " + result.Error);
        }
        return success;
    }

    [MenuItem("Window/Asset Management/Addressables/Build Addressables only")]
    static bool BuildAddressables()
    {
        string strStore = GetCmdArgAsString("-store", "");
        if(!string.IsNullOrEmpty(strStore))
        {
            if(strStore == "google" && EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }
            else if(strStore == "apple" && EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
            }
        }

        if(!string.IsNullOrEmpty(GetCmdArgAsString("-export", "")))
        {
            ALF.DataExport.MakeExcelToFBS(null, null);
        }

        GameContext.PatchVersion = GetCmdArgAsString("-patchVersion", GameContext.PatchVersion);

        getSettingsObject(settings_asset);
        setProfile(profile_name);
        IDataBuilder builderScript = AssetDatabase.LoadAssetAtPath<ScriptableObject>(build_script) as IDataBuilder;

        if (builderScript == null) {
            Debug.LogError(build_script + " couldn't be found or isn't a build script.");
            return false;
        }

        setBuilder(builderScript);

        return buildAddressableContent();
    }

    // [MenuItem("Window/Asset Management/Addressables/Build Addressables and Player")]
    // public static void BuildAddressablesAndPlayer() {
    //     bool contentBuildSucceeded = BuildAddressables();

    //     if (contentBuildSucceeded) {
    //         var options = new BuildPlayerOptions();
    //         BuildPlayerOptions playerSettings
    //             = BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(options);

    //         BuildPipeline.BuildPlayer(playerSettings);
    //     }
    // }
}
