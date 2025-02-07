#if UNITY_IPHONE || UNITY_IOS
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
#if !USE_HIVE
using AppleAuth.Editor;
#endif

public static class XcodePostProcess
{
    // [PostProcessBuildAttribute( 1 )]
    // static void ChangeBuildManifest ( BuildTarget buildTarget, string pathToBuiltProject ) 
    // {
    //     if (buildTarget == BuildTarget.iOS)
    //     {
    //         // //paths
    //         // string projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
    //         // projPath = projPath.Substring(0, projPath.LastIndexOf( "/" ) );
            
    //         // var xcSettingsPath = $"{projPath}/project.xcworkspace/xcshareddata/WorkspaceSettings.xcsettings";
    //         // UnityEngine.Debug.Log($"xCodeProjFolderPath: {xcSettingsPath}");
    //         // change the xcode project to use the new build system, without doing this can not compile and get an error in xcode, plus the legacy build system is now deprecated
    //         // var xcSettingsDoc = new PlistDocument();
    //         // xcSettingsDoc.ReadFromString( File.ReadAllText( xcSettingsPath ) );
    //         // var xcSettingsDict = xcSettingsDoc.root;
    //         // var xcSettingsValues = xcSettingsDict.values;
    //         // var buildSystemTypeKey = "BuildSystemType";
    //         // if ( xcSettingsValues.ContainsKey( buildSystemTypeKey ) ) 
    //         // {
    //         //     xcSettingsValues.Remove( buildSystemTypeKey ); // the removal of this key/value pair <key>BuildSystemType</key><string>Original</string> allows xcode to use the default new build system setting
    //         // }
    //         // File.WriteAllText( xcSettingsPath, xcSettingsDoc.WriteToString() );
    //     }
    // }

    // [PostProcessBuild]
    [PostProcessBuild(600)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            ModifyFrameworks(path);
        }
    }
    private static void ModifyFrameworks(string path)
    {
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

        project.SetBuildProperty(mainTarget, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
        project.SetBuildProperty(unityFramework, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
        project.SetBuildProperty(mainTarget, "ENABLE_BITCODE", "NO");
        project.SetBuildProperty(unityFramework, "ENABLE_BITCODE", "NO");

        string fileGuid = Path.Combine(UnityEngine.Application.dataPath, "GoogleService-Info.plist");

        if (File.Exists(fileGuid))
        {
            File.Copy(fileGuid, path, true);
        } else 
        {
            Debug.Log("GoogleService-Info.plist is not exist");
        }

        fileGuid = project.AddFile(Path.Combine(path, "GoogleService-Info.plist"), "GoogleService-Info.plist", PBXSourceTree.Source);
        project.AddFileToBuild(mainTarget, fileGuid);

        // project.AddFileToBuild(mainTarget, project.AddFile(path + "/GoogleService-Info.plist", "GoogleService-Info.plist", PBXSourceTree.Source));
        // string fileGuid = project.FindFileGuidByProjectPath("GoogleService-Info.plist");
		// if (fileGuid != null)
		// {
        //     project.RemoveFileFromBuild(project.GetUnityFrameworkTargetGuid(), fileGuid);
		// }
        if(!project.ContainsFramework(mainTarget,"StoreKit.framework"))
        {
            project.AddFrameworkToProject(mainTarget, "StoreKit.framework", true);
        }

#if USE_HIVE

        // fileGuid = project.FindFileGuidByProjectPath("Libraries/HiveSDK/hive.androidlib");
        // fileGuid = project.FindFileGuidByRealPath("Libraries/HiveSDK/hive.androidlib",PBXSourceTree.Group);
        
        var itr = project.GetRealPathsOfAllFiles(PBXSourceTree.Source);
        foreach(var k in itr)
        {
            if(k.Contains("hive.androidlib"))
            {
                fileGuid = project.FindFileGuidByRealPath(k,PBXSourceTree.Source);
                if(fileGuid != null)
                {
                    project.RemoveFile(fileGuid);
                }
            }
        }
        
        string[] tempPathList = new string[]{"Pods/FBSDKCoreKit/XCFrameworks/FBSDKCoreKit.xcframework",
        "Pods/FBSDKGamingServicesKit/XCFrameworks/FBSDKGamingServicesKit.xcframework",
        "Pods/FBSDKCoreKit_Basics/XCFrameworks/FBSDKCoreKit_Basics.xcframework",
        "Pods/FBSDKLoginKit/XCFrameworks/FBSDKLoginKit.xcframework",
        "Pods/FBAEMKit/XCFrameworks/FBAEMKit.xcframework",
        "Pods/FBSDKShareKit/XCFrameworks/FBSDKShareKit.xcframework"};

        string[] tempFrameworkList = new string[]{"Frameworks/FBSDKCoreKit.xcframework",
        "Frameworks/FBSDKGamingServicesKit.xcframework",
        "Frameworks/FBSDKCoreKit_Basics.xcframework",
        "Frameworks/FBSDKLoginKit.xcframework",
        "Frameworks/FBAEMKit.xcframework",
        "Frameworks/FBSDKShareKit.xcframework"};
        string tempPath = null;
        for(int i = 0; i < tempPathList.Length; ++i)
        {
            tempPath = Path.Combine(path, tempPathList[i]);
            if (Directory.Exists(tempPath))
            {
                fileGuid = project.AddFile(tempPath, tempFrameworkList[i], PBXSourceTree.Source);
                project.AddFileToBuild(mainTarget, fileGuid);
                UnityEditor.iOS.Xcode.Extensions.PBXProjectExtensions.AddFileToEmbedFrameworks(project, mainTarget, fileGuid);
            }
            else
            {
                UnityEngine.Debug.LogError($"Directory.Exists == false :{tempPath}");
            }
        }

        project.AddShellScriptBuildPhase(mainTarget,"Run Script", "/bin/sh", "# Type a script or drag a script file from your workspace to insert its path.\ncd \"${CONFIGURATION_BUILD_DIR}/${UNLOCALIZED_RESOURCES_FOLDER_PATH}/\"\nif [[ -d \"FBSDKShareKit.framework\" ]]; then \n    rm -fr FBSDKShareKit.framework\nfi\n\nif [[ -d \"FBSDKLoginKit.framework\" ]]; then \n    rm -fr FBSDKLoginKit.framework\nfi\n\nif [[ -d \"FBSDKGamingServicesKit.framework\" ]]; then \n    rm -fr FBSDKGamingServicesKit.framework\nfi\n\nif [[ -d \"FBSDKCoreKit.framework\" ]]; then \n    rm -fr FBSDKCoreKit.framework\nfi\n\nif [[ -d \"FBSDKCoreKit_Basics.framework\" ]]; then \n    rm -fr FBSDKCoreKit_Basics.framework\nfi\n\nif [[ -d \"FBAEMKit.framework\" ]]; then \n    rm -fr FBAEMKit.framework\nfi\n\n\n\n\n");
#else
        if(project.ContainsFramework(project.GetUnityFrameworkTargetGuid(),"AuthenticationServices.framework"))
        {
            project.RemoveFrameworkFromProject(project.GetUnityFrameworkTargetGuid(),"AuthenticationServices.framework");
        }
        
        project.AddFrameworkToProject(project.GetUnityFrameworkTargetGuid(), "AuthenticationServices.framework", true);

#endif

        // project.AddFrameworkToProject(mainTarget, "StoreKit.framework", false);
        project.WriteToFile(projPath);

        ProjectCapabilityManager projCapability = new ProjectCapabilityManager( projPath,"Unity-iPhone/Unity-iPhone.entitlements","Unity-iPhone",mainTarget);

                        // Adds entitlement depending on the Unity version used
// #if UNITY_2019_3_OR_NEWER
//             project.ReadFromString(System.IO.File.ReadAllText(projPath));
//             var manager = new ProjectCapabilityManager(projPath, "Entitlements.entitlements", null, project.GetUnityMainTargetGuid());
//             manager.AddSignInWithAppleWithCompatibility(project.GetUnityFrameworkTargetGuid());
//             manager.WriteToFile();
// #else
//             var manager = new ProjectCapabilityManager(projPath, "Entitlements.entitlements", PBXProject.GetUnityTargetName());
//             manager.AddSignInWithAppleWithCompatibility();
//             manager.WriteToFile();
// #endif


        projCapability.AddSignInWithApple();
        projCapability.AddPushNotifications(false);
        projCapability.AddInAppPurchase();
        projCapability.WriteToFile();

        // Get Plist from Xcode project 
        string plistPath = path + "/Info.plist";

        // Read in Plist 
        PlistDocument plistObj = new PlistDocument();
        plistObj.ReadFromString(File.ReadAllText(plistPath));

        // set values from the root obj
        PlistElementDict plistRoot = plistObj.root;

        // Set value in plist
        plistRoot.SetString("FacebookClientToken","b2363e3b933a565057f7ee5dcd65a9b6");
        plistRoot.SetString("NSUserTrackingUsageDescription", "This identifier will be used to deliver personalized ads to you.");
        plistRoot.SetString("NSLocationWhenInUseUsageDescription", "Your location is used to provide more targeted advertising.");
        plistRoot.SetString("NSPhotoLibraryUsageDescription","문의를 위한 내 사진첩에 있는 사진을 업로드시 접근 권한이 필요합니다.");
        plistRoot.SetString("NSCameraUsageDescription","문의를 위한 카메라 사용시 접근 권한이 필요합니다.");
        plistRoot.SetBoolean("FirebaseAppStoreReceiptURLCheckEnabled", false);
        plistRoot.SetBoolean("FirebaseScreenReportingEnabled", true);
        plistRoot.SetBoolean("FirebaseAutomaticScreenReportingEnabled", true);
        plistRoot.SetBoolean("FacebookAutoLogAppEventsEnabled", true);
        // plistRoot.SetString("GADApplicationIdentifier", "ca-app-pub-5466681013076873~3535054124");
        // plistRoot.SetBoolean("GADIsAdManagerApp", true);
#if !FTM_LIVE    
        if (plistRoot.values.ContainsKey("NSAppTransportSecurity"))
        {
            try
            {
                PlistElement element;
                plistRoot.values.TryGetValue("NSAppTransportSecurity", out element);

                PlistElementDict pDict = element.AsDict();
                pDict = pDict.CreateDict("NSExceptionDomains");
                PlistElementDict pDict1 = pDict.CreateDict("office.woncomz.com");
                // PlistElementDict added = pDict.CreateDict("office.woncomz.com");
                pDict1.SetBoolean("NSIncludesSubdomains", true);
                // added = pDict.AddDict();
                pDict1.SetBoolean("NSExceptionRequiresForwardSecrecy", false);
                // added = pDict.AddDict();
                pDict1.SetBoolean("NSExceptionAllowsInsecureHTTPLoads", true);

                pDict1 = pDict.CreateDict("ftm.ncucu.com");
                // PlistElementDict added = pDict.CreateDict("office.woncomz.com");
                pDict1.SetBoolean("NSIncludesSubdomains", true);
                // added = pDict.AddDict();
                pDict1.SetBoolean("NSExceptionRequiresForwardSecrecy", false);
                // added = pDict.AddDict();
                pDict1.SetBoolean("NSExceptionAllowsInsecureHTTPLoads", true);

                
            }
#pragma warning disable 0168
            catch (Exception e)
#pragma warning restore 0168
            {
                
            }
        }
#endif

#if !USE_HIVE

        if (plistRoot.values.ContainsKey("CFBundleURLTypes"))
        {
            try
            {
                PlistElement element;
                plistRoot.values.TryGetValue("CFBundleURLTypes", out element);

                PlistElementArray pArray = element.AsArray();                
                PlistElementDict pDict = pArray.AddDict();
                pDict.SetString("CFBundleTypeRole","Editor");
                pDict.SetString("CFBundleURLName","google-login");

                pArray = pDict.CreateArray("CFBundleURLSchemes");
                pArray.AddString("com.googleusercontent.apps.194960407833-cmn9stpfad3ooes375braalpbm5tq22h");
                
            }
#pragma warning disable 0168
            catch (Exception e)
#pragma warning restore 0168
            {
                
            }
        }
#endif
        // save
        File.WriteAllText(plistPath, plistObj.WriteToString());
#if !USE_HIVE
        using (StreamWriter sw = File.AppendText(path + "/Podfile"))
        {
            sw.WriteLine("\n# Fix Xcode 14 bundle code signing issue");
            sw.WriteLine("post_install do |installer|");
            sw.WriteLine("  installer.pods_project.targets.each do |target|");
            sw.WriteLine("    target.build_configurations.each do |config|");
            sw.WriteLine("      if config.build_settings['WRAPPER_EXTENSION'] == 'bundle'");
            sw.WriteLine($"        config.build_settings['DEVELOPMENT_TEAM'] = '{PlayerSettings.iOS.appleDeveloperTeamID}'");
            sw.WriteLine("      end");
            sw.WriteLine("    end");
            sw.WriteLine("  end");
            sw.WriteLine("end");
        }
#endif
    }


/**
 * @file    HivePostprocess.cs
 * 
 * @author  nanomech
 * Copyright 2016 GAMEVILCom2USPlatform Corp.
 * @defgroup UnityEditor.HiveEditor
 * @{
 * @brief PostPrcessing on BuildTime <br/><br/>
 */


        // private static string getFacebookAppID(){
        //     //TODO: 페이스북 앱아이디 얻는 작업필요함.
		// 	#if UNITY_IOS
		// 	return Hive.Unity.Editor.HiveConfigXML.iOS.facebookAppID;
		// 	#elif UNITY_ANDROID
		// 	return Hive.Unity.Editor.HiveConfigXML.Android.facebookAppID;
		// 	#else
		// 	return null;
		// 	#endif
        // }

        // private static bool hasFacebookAppId() {
		// 	#if UNITY_IOS
		// 	return Hive.Unity.Editor.HiveConfigXML.iOS.IsValidFacebookAppId;
		// 	#elif UNITY_ANDROID
		// 	return Hive.Unity.Editor.HiveConfigXML.Android.IsValidFacebookAppId;
		// 	#else
		// 	return false;
		// 	#endif
        // }

        // private static string getBundleIdentifier(){
		// 	#if UNITY_IOS
		// 	return Hive.Unity.Editor.HiveConfigXML.iOS.HIVEAppID;
		// 	#elif UNITY_ANDROID
		// 	return Hive.Unity.Editor.HiveConfigXML.Android.HIVEAppID;
        //     #else
        //     return null;
        //     #endif
            
        // }

        // private static bool hasLineChannelId() {
        //     #if UNITY_IOS
        //     return Hive.Unity.Editor.HiveConfigXML.iOS.IsValidLineChannelId;
        //     #elif UNITY_ANDROID
        //     return Hive.Unity.Editor.HiveConfigXML.Android.IsValidLineChannelId;
        //     #else
        //     return false;
        //     #endif
        // }

        //xcode project 후반작업
        // private static void iOSPostBuild(string buildPath){
        //     #if UNITY_IOS
            
        //     iOSSettingProject(buildPath);
        //     iOSSettingInfoPlist(buildPath);

        //     #endif
        // }


//         private static void iOSSettingProject(string buildPath) {
//             #if UNITY_IOS

//             string framework_path = "$(SRCROOT)/Frameworks/Hive_SDK_v4/Plugins/iOS/framework";
//             //copy resource
//             string[] hive_res_path = {
//                 "Plugins/iOS/hive_config.xml"
//             };        
            
//             //Default Setting System Framework
//             string[] system_frameworks = {
//                 "libz.tbd",
//                 "libsqlite3.tbd",
//                 "AdSupport.framework",
//                 "CFNetwork.framework",
//                 "CoreData.framework",
//                 "CoreTelephony.framework",
//                 "Security.framework",
//                 "StoreKit.framework",
//                 "SystemConfiguration.framework",
//                 "UIKit.framework",
//                 "iAd.framework",
//                 "MobileCoreServices.framework",
// 				"WebKit.framework",
//                 "MapKit.framework",
//                 "Accelerate.framework"  // facebook framework 6.0 버전 이상부터 필요. 이미지 및 영상 처리에 대한 프레임워크
//             };

//             string[] optional_frameworks = {
//                 "SafariServices.framework",
//                 "CoreSpotlight.framework"
//             };

//             var path = Path.Combine(buildPath,"Unity-iPhone.xcodeproj/project.pbxproj");
//             var project = new PBXProject();
//             project.ReadFromFile(path);

//             // 프로젝트의 타겟들 
//             string mainTarget = "main";
//             string unityFrameworkTarget = "unityframework";
                        
// #if UNITY_2019_3_OR_NEWER
//             var targets = new Dictionary<string, string>(){
//                                                     {mainTarget, project.GetUnityMainTargetGuid()}, // 메인 타겟
//                                                     {unityFrameworkTarget, project.GetUnityFrameworkTargetGuid()} // 유니티 프레임워크 타겟
//                                                     }; 
// #else
//             var targetName = PBXProject.GetUnityTargetName();
//             var targets = new Dictionary<string, string>(){
//                                                     {mainTarget, project.TargetGuidByName(targetName)} // 메인 타겟
//                                                      };  
// #endif

//             //add system framework
//             for(int i=0;i<system_frameworks.Length;++i){
// #if UNITY_2019_3_OR_NEWER
//                 project.AddFrameworkToProject(targets[unityFrameworkTarget],system_frameworks[i],false);
// #else 
//                 project.AddFrameworkToProject(targets[mainTarget],system_frameworks[i],false);
// #endif
//             }

//             for(int i=0;i<optional_frameworks.Length;++i){
// #if UNITY_2019_3_OR_NEWER
//                 project.AddFrameworkToProject(targets[unityFrameworkTarget],optional_frameworks[i],true);
// #else
//                 project.AddFrameworkToProject(targets[mainTarget],optional_frameworks[i],true);
// #endif
//             }

//             //make resource directory
//             string project_res_directory = "Hive_SDK_v4/";
//             project_res_directory = Path.Combine(buildPath,project_res_directory);
//             if(!Directory.Exists(project_res_directory)){
//                 Directory.CreateDirectory(project_res_directory);
//             }
            
//             //add resource
//             for(int i=0;i<hive_res_path.Length;++i) {
//                 string res = hive_res_path[i];
//                 string project_res_path = Path.Combine("Hive_SDK_v4/",Path.GetFileName(res)); 
                
//                 string assetsPath = "Assets/"+res;
//                 string buildPathCombine = Path.Combine(buildPath,project_res_path);

//                 if(!Directory.Exists(Path.GetDirectoryName(buildPathCombine))){
//                     Directory.CreateDirectory(Path.GetDirectoryName(buildPathCombine));
//                 }

//                 var attr = File.GetAttributes(assetsPath);
//                 if((attr & FileAttributes.Directory) == FileAttributes.Directory){
//                     directoryCopy(assetsPath,buildPathCombine,true);
//                 }else {
//                     //파일은 무조건 복사해서 덮어쓴다.
//                     if( !assetsPath.EndsWith(".meta") )
//                         File.Copy(assetsPath,buildPathCombine, true);
//                 }

//                 //프로젝트 추가여부는 프로젝트에 추가되어있는지 확인후 결정
//                 if(!project.ContainsFileByProjectPath(project_res_path)){
//                     project.AddFileToBuild(
//                         targets[mainTarget],
//                         project.AddFile(project_res_path, project_res_path, PBXSourceTree.Source));
//                 }
//             }

            
// #if UNITY_2019_3_OR_NEWER
//             // resource 디렉토리 하위에 존재하는 .bundle 디렉토리를 메인 타겟으로 복사
//             DirectoryInfo resourceDirectory = new DirectoryInfo("Assets/Hive_SDK_v4/Plugins/iOS/resource");
//             DirectoryInfo[] bundleDirectories = resourceDirectory.GetDirectories("*.bundle", SearchOption.AllDirectories);

//             foreach (DirectoryInfo bundleDirectory in bundleDirectories) {
//                 project.AddFileToBuild(targets[mainTarget], project.AddFile(bundleDirectory.ToString(), bundleDirectory.ToString(), PBXSourceTree.Source));
//             }
// #endif
//             //linker flag setting
//             foreach (string target in targets.Values) {
//                 project.AddBuildProperty(target, "OTHER_CFLAGS", "-Wextern-initializer -Wunguarded-availability-new -Wmissing-declarations");
//                 project.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC -lz -fobjc-arc");
//                 //framework search path
//                 project.AddBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", framework_path);

//                 project.SetBuildProperty(target, "ENABLE_BITCODE","NO");

//                 project.SetBuildProperty(target, "SWIFT_VERSION", "5");
                
                
//                 if(project.GetUnityFrameworkTargetGuid() == target)
//                 {
//                     project.SetBuildProperty(target, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
//                 }
//                 else
//                 {
//                     project.SetBuildProperty(target, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
//                 }

//                 project.SetBuildProperty(target, "LD_RUNPATH_SEARCH_PATHS", "$(inherited) @executable_path/Frameworks");
                
//             }

//             //SAVE PROJECT
//             project.WriteToFile(path);

//             #endif
//         }

        // private static void iOSSettingInfoPlist(string buildPath) {
        //     #if UNITY_IOS

        //     var PlistPath = buildPath + "/Info.plist";
        //     PlistDocument plist = new PlistDocument();
        //     plist.ReadFromFile(PlistPath);

        //     var rootDict = plist.root;
        //     rootDict.SetBoolean("UIViewControllerBasedStatusBarAppearance",false);
        //     if( hasFacebookAppId() )
        //     {
        //         rootDict.SetString("FacebookAppID",getFacebookAppID());
        //         rootDict.SetString("FacebookClientToken","4002b6ea773c93ec4aa0bdd85e892c99");
        //         rootDict.SetString("FacebookDisplayName","Cats Island");
        //     }

        //     rootDict.SetString("CFBundleIdentifier",getBundleIdentifier());

        //     rootDict.SetString("NSCameraUsageDescription","카메라를 사용합니다.");
        //     rootDict.SetString("NSContactsUsageDescription","앱이 사용자 주소록에 접근하려고 합니다.");
        //     rootDict.SetString("NSLocationWhenInUseUsageDescription","Your location is used to provide more targeted advertising.");
        //     rootDict.SetString("NSPhotoLibraryUsageDescription","앱이 사용자 사진에 접근하려고 합니다.");
        //     rootDict.SetString("NSUserTrackingUsageDescription","개인의 취향에 맞는 맞춤식 콘텐츠 제공을 위해 앱이 디바이스 ID를 사용할 수 있도록 동의가 필요합니다. 동의하지 않더라도 정상 이용이 가능하며, 설정에서 변경 가능합니다.");

        //     //facebook white list
        //     var LSApplicationQueriesSchemes = rootDict.CreateArray("LSApplicationQueriesSchemes");
        //     LSApplicationQueriesSchemes.AddString("fbapi");
        //     LSApplicationQueriesSchemes.AddString("fbauth2");
        //     LSApplicationQueriesSchemes.AddString("fbauth");
        //     LSApplicationQueriesSchemes.AddString("fbapi20130214");
        //     LSApplicationQueriesSchemes.AddString("fbapi20130410");
        //     LSApplicationQueriesSchemes.AddString("fbapi20130702");
        //     LSApplicationQueriesSchemes.AddString("fbapi20131010");
        //     LSApplicationQueriesSchemes.AddString("fbapi20131219");
        //     LSApplicationQueriesSchemes.AddString("fbapi20140410");
        //     LSApplicationQueriesSchemes.AddString("fbapi20140116");
        //     LSApplicationQueriesSchemes.AddString("fbapi20150313");
        //     LSApplicationQueriesSchemes.AddString("fbapi20150629");
        //     LSApplicationQueriesSchemes.AddString("fbapi20160328");
        //     LSApplicationQueriesSchemes.AddString("fb-messenger-share-api");
        //     LSApplicationQueriesSchemes.AddString("fb-messenger-api");
        //     LSApplicationQueriesSchemes.AddString("fbshareextension");


            
        //     //URL Types settings
        //     var CFBundleURLTypes = rootDict.CreateArray("CFBundleURLTypes");

        //     var facebookURLType = CFBundleURLTypes.AddDict();
        //     facebookURLType.SetString("CFBundleTypeRole","Editor");
        //     if( hasFacebookAppId() )
        //         facebookURLType.CreateArray("CFBundleURLSchemes").AddString("fb"+getFacebookAppID());

		// 	// add Google reversed client id
		// 	if (hasGoogleReversedClientId()){
		// 		var googleReversedClientId = CFBundleURLTypes.AddDict();
		// 		googleReversedClientId.SetString("CFBundleTypeRole","Editor");
		// 		googleReversedClientId.CreateArray("CFBundleURLSchemes").AddString(getGoogleReversedClientId());
		// 	}
		// 	// add tencent appid
		// 	if (hasQQAppId()){
		// 		var qqAppId = CFBundleURLTypes.AddDict();
		// 		qqAppId.SetString("CFBundleTypeRole","Editor");
		// 		qqAppId.CreateArray("CFBundleURLSchemes").AddString("tencent"+getQQAppId());
		// 	}


        //     // add VK appid
        //     if (hasVKAppId()) {
        //         var vkAppId = CFBundleURLTypes.AddDict();
        //         vkAppId.SetString("CFBundleTypeRole","Editor");
        //         vkAppId.CreateArray("CFBundleURLSchemes").AddString("vk"+getVKAppId());
        //     }

        //     var urlSchemes = CFBundleURLTypes.AddDict();
        //     urlSchemes.SetString("CFBundleTypeRole","Editor");
        //     urlSchemes.SetString("CFBundleIdentifier",getBundleIdentifier());
        //     urlSchemes.CreateArray("CFBundleURLSchemes").AddString(getBundleIdentifier());

        //     // remove exit on suspend if it exists.
        //     string exitsOnSuspendKey = "UIApplicationExitsOnSuspend";
        //     if(rootDict.values.ContainsKey(exitsOnSuspendKey))
        //     {
        //         rootDict.values.Remove(exitsOnSuspendKey);
        //     }

        //     // Set encryption usage boolean
        //     string encryptKey = "ITSAppUsesNonExemptEncryption";
        //     rootDict.SetBoolean(encryptKey, false);

        //     // ATS
        //     var ATSDict = rootDict.CreateDict("NSAppTransportSecurity");
        //     ATSDict.SetBoolean("NSAllowsArbitraryLoads",true);

        //     plist.WriteToFile(PlistPath);

        //     #endif
        // }

        static void directoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        { 
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    directoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        // public static void UpdatePlist(string path)
        // {
        //     const string FileName = "Info.plist";
        //     string appId = FacebookSettings.AppId;
        //     string fullPath = Path.Combine(path, FileName);

        //     if (string.IsNullOrEmpty(appId) || appId.Equals("0"))
        //     {
        //         Debug.LogError("You didn't specify a Facebook app ID.  Please add one using the Facebook menu in the main Unity editor.");
        //         return;
        //     }

        //     var facebookParser = new PListParser(fullPath);
        //     facebookParser.UpdateFBSettings(
        //         appId,
        //         FacebookSettings.IosURLSuffix,
        //         FacebookSettings.AppLinkSchemes[FacebookSettings.SelectedAppIndex].Schemes);
        //     facebookParser.WriteToFile();
        // }
}

#endif