using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TableEditorWindow<T> : EditorWindow where T : Object
{
    IList<Editor> _editors;

    void Load()
    {
        if (_editors != null)
        {
            return;
        }
        _editors = new List<Editor>();
        foreach (var guid in AssetDatabase.FindAssets("t:scriptableobject"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                _editors.Add(Editor.CreateEditor(asset));
            }
        }
    }

    static IEnumerable<SerializedProperty> GetVisibleProperties(SerializedProperty prop)
    {
        if(!prop.NextVisible(true))
        {
            yield break;
        }
        do
        {
            yield return prop;
        } while (prop.NextVisible(false));
    }

    protected virtual bool ValidProperty(SerializedProperty prop)
    {
        return prop.name != "m_Script";
    }

    void DrawHeader()
    {
        if (_editors.Count == 0)
        {
            return;
        }
        GUILayout.BeginHorizontal();
        foreach(var prop in GetVisibleProperties(_editors[0].serializedObject.GetIterator()))
        { 
            if(!ValidProperty(prop))
            {
                continue;
            }
            EditorGUILayout.LabelField(prop.displayName);
        }
        GUILayout.EndHorizontal();
    }

    void DrawTable()
    {
        foreach (var editor in _editors)
        {
            GUILayout.BeginHorizontal();
            foreach(var prop in GetVisibleProperties(editor.serializedObject.GetIterator()))
            {
                if (!ValidProperty(prop))
                {
                    continue;
                }
                EditorGUILayout.PropertyField(prop, GUIContent.none);
            }
            GUILayout.EndHorizontal();
        }
    }

    void OnGUI()
    {
        Load();
        DrawHeader();
        DrawTable();
    }
}