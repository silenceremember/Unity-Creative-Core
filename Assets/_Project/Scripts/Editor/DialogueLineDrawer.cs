using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom drawer for DialogueLine.
/// Highlights text field red if > 54 characters.
/// </summary>
[CustomPropertyDrawer(typeof(DialogueLine))]
public class DialogueLineDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var textProp     = property.FindPropertyRelative("text");
        var durationProp = property.FindPropertyRelative("duration");
        var pauseProp    = property.FindPropertyRelative("pauseAfter");
        var activateProp = property.FindPropertyRelative("activateObject");

        float y    = position.y;
        float w    = position.width;
        float lineH = EditorGUIUtility.singleLineHeight;
        float pad  = 2f;

        // Text — red background if > max chars
        string text     = textProp.stringValue ?? "";
        bool   overflow = text.Length > DialogueSequence.MAX_CHARS;

        if (overflow)
        {
            var warningRect = new Rect(position.x, y, w, lineH * 3 + 4);
            EditorGUI.DrawRect(warningRect, new Color(1f, 0.2f, 0.2f, 0.15f));
        }

        Rect textRect = new Rect(position.x, y, w, lineH * 3);
        EditorGUI.PropertyField(textRect, textProp, new GUIContent("Text"));
        y += lineH * 3 + pad;

        // Character counter
        string counter = overflow
            ? $"⚠ {text.Length}/{DialogueSequence.MAX_CHARS} chars — split into multiple Lines!"
            : $"{text.Length}/{DialogueSequence.MAX_CHARS} chars";

        GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
        style.normal.textColor = overflow ? new Color(1f, 0.3f, 0.3f) : Color.gray;
        EditorGUI.LabelField(new Rect(position.x, y, w, lineH), counter, style);
        y += lineH + pad;

        // Duration + PauseAfter on one line
        float half = (w - 4) / 2f;
        EditorGUI.PropertyField(new Rect(position.x, y, half, lineH), durationProp, new GUIContent("Duration"));
        EditorGUI.PropertyField(new Rect(position.x + half + 4, y, half, lineH), pauseProp, new GUIContent("Pause After"));
        y += lineH + pad;

        // Activate Object
        EditorGUI.PropertyField(new Rect(position.x, y, w, lineH), activateProp, new GUIContent("Activate Object"));

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float lineH = EditorGUIUtility.singleLineHeight;
        float pad   = 2f;
        return lineH * 3 + pad  // text
             + lineH + pad  // counter
             + lineH + pad  // duration/pause
             + lineH + pad; // activateObject
    }
}
