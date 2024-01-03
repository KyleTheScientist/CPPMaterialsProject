using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using UnityEngine.UI;
public class CompileNoteWindow : EditorWindow
{

    private GorillaMaterialDescriptor[] notes;
    [MenuItem("Window/Material Exporter")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(CompileNoteWindow), false, "Material Exporter", false);
    }
    public Vector2 scrollPosition = Vector2.zero;

    private void OnFocus()
    {
        notes = GameObject.FindObjectsOfType<GorillaMaterialDescriptor>();
    }
    void OnGUI()
    {
        var window = EditorWindow.GetWindow(typeof(CompileNoteWindow), false, "Material Exporter", false);

        int ScrollSpace = (16 + 20) + (16 + 17 + 17 + 20 + 20);
        foreach (GorillaMaterialDescriptor note in notes)
        {
            if (note != null)
            {

                ScrollSpace += (16 + 17 + 17 + 20 + 20);

            }
        }
        float currentWindowWidth = EditorGUIUtility.currentViewWidth;
        float windowWidthIncludingScrollbar = currentWindowWidth;
        if (window.position.size.y >= ScrollSpace)
        {
            windowWidthIncludingScrollbar += 30;
        }
        scrollPosition = GUI.BeginScrollView(new Rect(0, 0, EditorGUIUtility.currentViewWidth, window.position.size.y), scrollPosition, new Rect(0, 0, EditorGUIUtility.currentViewWidth - 20, ScrollSpace), false, false);

        //GUILayout.ScrollViewScope
        GUILayout.Label("Notes", EditorStyles.boldLabel, GUILayout.Height(16));
        GUILayout.Space(20);

        foreach (GorillaMaterialDescriptor note in notes)
        {
            if (note != null)
            {
                GUILayout.Label("GameObject : " + note.gameObject.name, EditorStyles.boldLabel, GUILayout.Height(16));
                note.AuthorName = EditorGUILayout.TextField("Author name", note.AuthorName, GUILayout.Width(windowWidthIncludingScrollbar - 40), GUILayout.Height(17));
                note.Name = EditorGUILayout.TextField("Material name", note.Name, GUILayout.Width(windowWidthIncludingScrollbar - 40), GUILayout.Height(17));

                if (GUILayout.Button("Export " + note.Name, GUILayout.Width(windowWidthIncludingScrollbar - 40), GUILayout.Height(20)))
                {
                    GameObject noteObject = note.gameObject;
                    if (noteObject != null && note != null)
                    {
                        string path = EditorUtility.SaveFilePanel("Save material file", "", note.Name + ".gmatplus", "gmatplus");
                        Debug.Log(path == "");

                        if (path != "")
                        {
                            EditorUtility.SetDirty(note);

                            GameObject tempMaterialObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            tempMaterialObject.GetComponent<MeshRenderer>().sharedMaterial = noteObject.GetComponent<MeshRenderer>().sharedMaterial;

                            // do stuff 
                            ExporterUtils.ExportPackage(tempMaterialObject, path, "Material", ExporterUtils.MaterialDescriptorToJSON(note));
                            EditorUtility.DisplayDialog("Export Successful", "Export Successful", "OK");
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Export Failed", "Path is invalid.", "OK");
                        }
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Export Failed", "Note GameObject is missing.", "OK");
                    }
                }
                GUILayout.Space(20);
            }
        }
        GUI.EndScrollView();
    }

}
