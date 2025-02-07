using UnityEngine;
using UnityEditor;
using System.Text;

public static class AndroidSDKFolder
{
    public static string Path
    {
        get {
            return EditorPrefs.GetString("AndroidSdkRoot");
        } set {
            EditorPrefs.SetString("AndroidSdkRoot", value);
        }
    }
}

public static class ShellRunner
{
    public static string HOME
    {
        get {
            return System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        }
    }

    public static string JENKINS_HOME
    {
        get {
            return System.Environment.GetEnvironmentVariable("JENKINS_HOME");
        }
    }

    public static string sh
    {
        get {
            if(Application.platform == RuntimePlatform.WindowsEditor)
                return "sh.exe";
            else
                return "sh";
        }
    }

    public static string python 
    {
        get {
            if(Application.platform == RuntimePlatform.WindowsEditor)
                return "python.exe";
            else
                return "python";
        }
    }
    public static string Run(string exe, string argu, string pwd = null)
    {
        var prc = new System.Diagnostics.Process() {
            StartInfo = new System.Diagnostics.ProcessStartInfo(exe) {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                FileName = exe,
                Arguments = argu,
                WorkingDirectory = pwd

            }
        };
        prc.StartInfo.FileName = exe;
        prc.StartInfo.Arguments = argu;

        prc.StartInfo.EnvironmentVariables["PATH"] =
            prc.StartInfo.EnvironmentVariables["PATH"] + ":" + AndroidSDKFolder.Path+"/platform-tools"
            + ":" + AndroidSDKFolder.Path+"/tools";

        //Debug.Log("ExecuteShell PATH : "+prc.StartInfo.EnvironmentVariables["PATH"]);
        prc.Start();
        prc.WaitForExit();
        StringBuilder stdout = new StringBuilder();
        StringBuilder stderr = new StringBuilder();
        if(prc.StandardOutput.Peek() > -1)
            stdout.Append(prc.StandardOutput.ReadToEnd());
        if(prc.StandardError.Peek() > -1)
            stderr.Append(prc.StandardError.ReadToEnd());
        if(stderr.Length > 0)
            stdout.AppendLine(stderr.ToString());

        if(prc.ExitCode != 0) {
            Debug.LogError("Shell error : "+exe+" "+argu+" at "+pwd+"\nstdout:"+stdout.ToString()+"\nstderr:"+stderr.ToString());
            return "ERR]"+stdout.ToString();
        } else
            Debug.Log("Shell success : "+exe+" "+argu+" at "+pwd+"\nstdout:"+stdout.ToString()+"\nstderr:"+stderr.ToString());
        return stdout.ToString();
    }
}

