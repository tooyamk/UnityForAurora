﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Aurora {
    public class Exporter : EditorWindow {
        [MenuItem("Aurora/Exporter")]
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
                        var data = new ExportData();
                        data.meshes.Add(meshFilter.sharedMesh);

                        MemoryStream ms = data.encode();

                        //if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                        FileStream fs = File.Create(_formatFilePath(path, "arr"));

                        ms.WriteTo(fs);
                        fs.Flush();
                        fs.Close();
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
