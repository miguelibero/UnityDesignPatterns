using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class UnityObjectDropdownPropertyDrawer<T> : PropertyDrawer where T : Object
{
    Dictionary<string, string> _options;

    protected virtual string GetLabel(T obj)
    {
        return obj.name;
    }

    protected virtual IEnumerable<string> FindAssets()
    {
        return AssetDatabase.FindAssets("");
    }

    Dictionary<string, string> GetOptions()
    {
        if(_options != null)
        {
            return _options;
        }
        _options = new Dictionary<string, string>();
        _options["<null>"] = null;
        foreach (var guid in FindAssets())
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (asset is T tasset)
            {
                _options[GetLabel(tasset)] = path;
            }
        }
        return _options;
    }

    static string GetValue(SerializedProperty property)
    {
        return AssetDatabase.GetAssetPath(property.objectReferenceValue);
    }

    void SetValue(SerializedProperty property, string value)
    {
        var obj = AssetDatabase.LoadAssetAtPath(value, typeof(Object));
        property.objectReferenceValue = obj;
    }

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        string selected = null;
        var value = GetValue(property);
        var options = GetOptions();
        foreach (var kvp in options)
        {
            if(kvp.Value == value)
            {
                selected = kvp.Key;
                break;
            }
        }

        var field = new DropdownField(new List<string>(options.Keys), selected);
        field.RegisterValueChangedCallback(ev => SetValue(property, options[ev.newValue]));
        return field;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var options = GetOptions();
        var keys = new List<string>(options.Keys);
        var selected = 0;
        var value = GetValue(property);
        foreach (var kvp in options)
        {
            if (kvp.Value == value)
            {
                selected = keys.IndexOf(kvp.Key);
                break;
            }
        }
        var newSelected = EditorGUI.Popup(position, selected, keys.ToArray());
        if (newSelected != selected)
        {
            SetValue(property, options[keys[newSelected]]);
        }
        EditorGUI.EndProperty();
    }
}
