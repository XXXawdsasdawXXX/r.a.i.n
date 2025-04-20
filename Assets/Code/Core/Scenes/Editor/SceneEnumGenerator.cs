#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Core.Scenes.Editor
{
    public class SceneEnumGenerator : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            GenerateEnum();
        }

        [MenuItem("Tools/Generate Scene Enum")]
        public static void GenerateEnum()
        {
            string[] scenePaths = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            List<string> sceneNames = scenePaths
                .Select(path => Path.GetFileNameWithoutExtension(path))
                .Distinct()
                .ToList();

            string outputPath = "Assets/Code/Core/Scenes/EScene.cs";
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? string.Empty);

            using (StreamWriter writer = new StreamWriter(outputPath))
            {
                writer.WriteLine("namespace Core.Scenes");
                writer.WriteLine("{");
                writer.WriteLine("    public enum EScene");
                writer.WriteLine("    {");

                foreach (string name in sceneNames)
                {
                    writer.WriteLine($"        {name},");
                }

                writer.WriteLine("    }");
                writer.WriteLine("}");
            }

            AssetDatabase.Refresh();
            
            Debug.Log($"SceneId enum generated: {sceneNames.Count} scenes → {outputPath}");
        }
    }
}


#endif
