using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

public class Build
{

    //adapted from https://gist.github.com/Jawnnypoo/366bbcc7f65e85154a15

    static string[] SCENES = FindEnabledEditorScenes();

    static void PerformMacOSXBuild(string target_dir)
    {
        GenericBuild(SCENES, target_dir, BuildTarget.StandaloneOSXUniversal, BuildOptions.None);
    }

    [MenuItem("BuildyBuild/Windows")]
    static void Windows()
    {
        string targetDir = GetCommandLineStringValue("-buildPath", "");
        Windows(targetDir);
    }

    static void Windows(string target_dir)
    {
        GenericBuild(SCENES, target_dir, BuildTarget.StandaloneWindows, BuildOptions.None);
    }

    static void WebGL(string target_dir)
    {
        string targetDir = GetCommandLineStringValue("-buildPath", "");
        GenericBuild(SCENES, target_dir, BuildTarget.WebGL, BuildOptions.None);
    }

    //static void PerformAndroidBuild() {
    //    //Set the path to the Android SDK on the machine, since Unity cannot retain the state properly
    //    AndroidSDKFolder.Path = "${ANDROID_HOME}";
    //    string target_dir = APP_NAME + ".apk";
    //    GenericBuild(SCENES, TARGET_DIR + "/" + target_dir, BuildTarget.Android, BuildOptions.None);
    //}

    //static void PerformiOSBuild() {
    //    string target_dir = "iOS";
    //    //We do not build the xcodeproject in the target directory, since we do not want to archive the
    //    //entire xcode project, but instead build with XCode, then output the .ipa through Jenkins
    //    GenericBuild(SCENES, target_dir, BuildTarget.iOS, BuildOptions.None);
    //}

    private static string[] FindEnabledEditorScenes()
    {
        List<string> EditorScenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
            EditorScenes.Add(scene.path);
        }
        return EditorScenes.ToArray();
    }

    static void GenericBuild(string[] scenes, string target_dir, BuildTarget build_target, BuildOptions build_options)
    {

        if (string.IsNullOrEmpty(target_dir)) { return; }

        EditorUserBuildSettings.SwitchActiveBuildTarget(build_target);

        List<string> errors = new List<string>();
        Application.LogCallback logrecieved = null;
        logrecieved = (string c, string st, LogType t) => {
            if (t == LogType.Error && !c.StartsWith("Shader error") && !c.StartsWith("TerrainData is missing")) { errors.Add(c); }
        };
        Application.logMessageReceivedThreaded += logrecieved;

        string res = BuildPipeline.BuildPlayer(scenes, target_dir, build_target, build_options);

        Application.logMessageReceivedThreaded -= logrecieved;

        if (errors.Count > 0)
        {
            string allErrors = "";
            errors.ForEach(obj => allErrors += obj + ", ");
            throw new Exception("BuildPlayer failure: " + allErrors);
        }
    }

    public static bool HasCommandLineArg(string argumentName)
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Equals(argumentName))
            {
                return true;
            }
        }

        return false;
    }

    public static int GetCommandLineArgValue(string argumentName, int nDefaultValue)
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Equals(argumentName))
            {
                if (i == (args.Length - 1)) // Last arg, return default
                {
                    return nDefaultValue;
                }

                return Int32.Parse(args[i + 1]);
            }
        }

        return nDefaultValue;
    }

    public static string GetCommandLineStringValue(string argumentName, string nDefaultValue)
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Equals(argumentName))
            {
                if (i == (args.Length - 1)) // Last arg, return default
                {
                    return nDefaultValue;
                }

                return args[i + 1];
            }
        }

        return nDefaultValue;
    }

    public static float GetCommandLineArgValue(string argumentName, float flDefaultValue)
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Equals(argumentName))
            {
                if (i == (args.Length - 1)) // Last arg, return default
                {
                    return flDefaultValue;
                }

                return (float)Double.Parse(args[i + 1]);
            }
        }

        return flDefaultValue;
    }
}