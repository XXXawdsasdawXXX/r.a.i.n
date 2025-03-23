using UnityEditor;
using UnityEngine;

namespace Core.Audio.Editor
{

    [CustomPropertyDrawer(typeof(AudioEvent))]
    public class AudioEventDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Calculate rects
            var eventReferenceRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var playButtonRect = new Rect(position.x, position.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2, position.width, EditorGUIUtility.singleLineHeight);

            // Draw fields
            EditorGUI.PropertyField(eventReferenceRect, property.FindPropertyRelative("_eventReference"), new GUIContent("Event Reference"));

            // Draw Play button
            if (GUI.Button(playButtonRect, "Play Audio Event"))
            {
                AudioEvent audioEvent = fieldInfo.GetValue(property.serializedObject.targetObject) as AudioEvent;
                audioEvent?.PlayAudioEvent();
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3;
        }
    }
}