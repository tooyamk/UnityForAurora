using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Aurora {
    public class Exporter : EditorWindow {
        [MenuItem("Mitoia/Exporter")]
        static void openExporter() {
            var exporter = (Exporter)GetWindow(typeof(Exporter));
        }

        public void OnGUI() {
            if (GUILayout.Button("Export Selection Mesh")) {
                _exportSelected();
            }
        }

        private void _exportSelected() {
            if (Selection.activeGameObject) {
                var meshFilter = Selection.activeGameObject.GetComponent<MeshFilter>();
                if (meshFilter) {
                    var path = EditorUtility.SaveFilePanel("Save Data", Application.dataPath, "", "arr");
                    if (path.Length > 0) {
                        var mesh = meshFilter.sharedMesh;
                        var vertices = mesh.vertices;
                        var normals = mesh.normals;
                        var uv = mesh.uv;
                        var triangles = mesh.triangles;
                        MemoryStream ms = new MemoryStream();
                        BinaryWriter writer = new BinaryWriter(ms);

                        writer.Write((int)vertices.Length);
                        foreach (var v in vertices) {
                            writer.Write((float)v.x);
                            writer.Write((float)v.y);
                            writer.Write((float)v.z);
                        }

                        writer.Write((int)normals.Length);
                        foreach (var v in normals) {
                            writer.Write((float)v.x);
                            writer.Write((float)v.y);
                            writer.Write((float)v.z);
                        }

                        writer.Write((int)uv.Length);
                        foreach (var v in uv) {
                            writer.Write((float)v.x);
                            writer.Write((float)v.y);
                        }

                        writer.Write((int)triangles.Length);
                        foreach (var i in triangles) {
                            writer.Write((int)i);
                        }

                        //if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                        Debug.Log(path);
                        Debug.Log(_formatFilePath(path, "arr"));
                        FileStream fs = File.Create(_formatFilePath(path, "arr"));

                        ms.WriteTo(fs);
                        fs.Flush();
                        fs.Close();
                        writer.Close();
                        ms.Close();
                    }
                }
            } else {
                EditorUtility.DisplayDialog("Error", "No Selection Object", "OK");
            }
        }

        private string _formatFilePath(string path, string extension) {
            if (path.Length < extension.Length + 1 || path.Substring(path.Length - extension.Length - 1) != "." + extension) {
                path += "." + extension;
            }
            return path;
        }
    }
}
