using UnityEditor;
using UnityEngine;

namespace Core.Data.RangeInt.Editor
{
    [CustomPropertyDrawer(typeof(RangedInt), true)]
    public class RangedIntDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            SerializedProperty minProp = property.FindPropertyRelative("MinValue");
            SerializedProperty maxProp = property.FindPropertyRelative("MaxValue");

            var minValue = minProp.intValue;
            var maxValue = maxProp.intValue;

            var rangeMin = 0;
            var rangeMax = 1;

            var ranges =
                (MinMaxRangeIntAttribute[])fieldInfo.GetCustomAttributes(typeof(MinMaxRangeIntAttribute), true);
            if (ranges.Length > 0)
            {
                rangeMin = ranges[0].Min;
                rangeMax = ranges[0].Max;
            }

            const float rangeBoundsLabelWidth = 40f;

            var rangeBoundsLabel1Rect = new Rect(position);
            rangeBoundsLabel1Rect.width = rangeBoundsLabelWidth;
            GUI.Label(rangeBoundsLabel1Rect, new GUIContent(minValue.ToString("F2")));
            position.xMin += rangeBoundsLabelWidth;

            var rangeBoundsLabel2Rect = new Rect(position);
            rangeBoundsLabel2Rect.xMin = rangeBoundsLabel2Rect.xMax - rangeBoundsLabelWidth;
            GUI.Label(rangeBoundsLabel2Rect, new GUIContent(maxValue.ToString("F2")));
            position.xMax -= rangeBoundsLabelWidth;

            EditorGUI.BeginChangeCheck();
            float minFloatValue = (float)minValue;
            float maxFloatValue = (float)maxValue;
            EditorGUI.MinMaxSlider(position, ref minFloatValue, ref maxFloatValue, (float)rangeMin, (float)rangeMax);
            if (EditorGUI.EndChangeCheck())
            {
                minProp.intValue = Mathf.RoundToInt(minFloatValue);
                maxProp.intValue = Mathf.RoundToInt(maxFloatValue);
            }

            EditorGUI.EndProperty();
        }
    }
}