using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom drawer for DialogueLine.
/// Highlights text field red if > 54 characters.
/// Shows both RU and EN text fields.
/// </summary>
[CustomPropertyDrawer(typeof(DialogueLine))]
public class DialogueLineDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var textProp     = property.FindPropertyRelative("text");
        var textEnProp   = property.FindPropertyRelative("textEn");
        var pauseProp    = property.FindPropertyRelative("pauseAfter");
        var activateProp = property.FindPropertyRelative("activateObject");

        float y    = position.y;
        float w    = position.width;
        float lineH = EditorGUIUtility.singleLineHeight;
        float pad  = 2f;

        // Text EN
        string textEn    = textEnProp.stringValue ?? "";
        bool   overflowEn = textEn.Length > DialogueSequence.MAX_CHARS;

        if (overflowEn)
        {
            var warningRectEn = new Rect(position.x, y, w, lineH * 3 + 4);
            EditorGUI.DrawRect(warningRectEn, new Color(1f, 0.2f, 0.2f, 0.15f));
        }

        Rect textEnRect = new Rect(position.x, y, w, lineH * 3);
        EditorGUI.PropertyField(textEnRect, textEnProp, new GUIContent("Text (EN)"));
        y += lineH * 3 + pad;

        // Character counter EN
        string counterEn = overflowEn
            ? $"⚠ {textEn.Length}/{DialogueSequence.MAX_CHARS} chars — split into multiple Lines!"
            : $"{textEn.Length}/{DialogueSequence.MAX_CHARS} chars";

        GUIStyle styleEn = new GUIStyle(EditorStyles.miniLabel);
        styleEn.normal.textColor = overflowEn ? new Color(1f, 0.3f, 0.3f) : Color.gray;
        EditorGUI.LabelField(new Rect(position.x, y, w, lineH), counterEn, styleEn);
        y += lineH + pad;

        // Text RU — red background if > max chars
        string text     = textProp.stringValue ?? "";
        bool   overflow = text.Length > DialogueSequence.MAX_CHARS;

        if (overflow)
        {
            var warningRect = new Rect(position.x, y, w, lineH * 3 + 4);
            EditorGUI.DrawRect(warningRect, new Color(1f, 0.2f, 0.2f, 0.15f));
        }

        Rect textRect = new Rect(position.x, y, w, lineH * 3);
        EditorGUI.PropertyField(textRect, textProp, new GUIContent("Text (RU)"));
        y += lineH * 3 + pad;

        // Character counter RU
        string counter = overflow
            ? $"⚠ {text.Length}/{DialogueSequence.MAX_CHARS} chars — split into multiple Lines!"
            : $"{text.Length}/{DialogueSequence.MAX_CHARS} chars";

        GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
        style.normal.textColor = overflow ? new Color(1f, 0.3f, 0.3f) : Color.gray;
        EditorGUI.LabelField(new Rect(position.x, y, w, lineH), counter, style);
        y += lineH + pad;

        // PauseAfter
        EditorGUI.PropertyField(new Rect(position.x, y, w, lineH), pauseProp, new GUIContent("Pause After"));
        y += lineH + pad;

        // Activate Object
        EditorGUI.PropertyField(new Rect(position.x, y, w, lineH), activateProp, new GUIContent("Activate Object"));

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float lineH = EditorGUIUtility.singleLineHeight;
        float pad   = 2f;
        return lineH * 3 + pad  // text RU
             + lineH + pad  // counter RU
             + lineH * 3 + pad  // text EN
             + lineH + pad  // counter EN
             + lineH + pad  // pause
             + lineH + pad; // activateObject
    }
}
