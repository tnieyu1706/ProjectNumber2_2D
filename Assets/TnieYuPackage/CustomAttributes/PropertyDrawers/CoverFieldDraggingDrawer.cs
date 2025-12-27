// using System;
// using System.Reflection;
// using UnityEditor;
// using UnityEditor.Search;
// using UnityEngine;
// using UnityEngine.UIElements;
//
// namespace TnieYuPackage.CustomAttributes.PropertyDrawers
// {
//     [CustomPropertyDrawer(typeof(CoverFieldDraggingAttribute))]
//     public class CoverFieldDraggingDrawer : PropertyDrawer
//     {
//         public override VisualElement CreatePropertyGUI(SerializedProperty property)
//         {
//             var root = new VisualElement();
//             var attr = (CoverFieldDraggingAttribute)attribute;
//
//             // Label cho field
//             var label = new Label(property.displayName);
//             label.style.unityFontStyleAndWeight = FontStyle.Bold;
//             label.style.marginBottom = 2;
//             root.Add(label);
//
//             // ObjectField UITK
//             var objectField = new ObjectField("Drag Data")
//             {
//                 objectType = attr.DraggingType
//             };
//
//             root.Add(objectField);
//
//             // Khi drag vào objectField
//             objectField.RegisterValueChangedCallback(evt =>
//             {
//                 if (evt.newValue != null)
//                 {
//                     AssignValue(property, evt.newValue, attr.MemberName);
//                 }
//             });
//
//             return root;
//         }
//
//         private void AssignValue(SerializedProperty property, object draggedObj, string fieldName)
//         {
//             var type = draggedObj.GetType();
//
//             object value = null;
//
//             // ---- Field ----
//             FieldInfo field = type.GetField(fieldName,
//                 BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
//
//             if (field != null)
//             {
//                 value = field.GetValue(draggedObj);
//             }
//             else
//             {
//                 // ---- Property ----
//                 PropertyInfo prop = type.GetProperty(fieldName,
//                     BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
//
//                 if (prop != null && prop.CanRead)
//                     value = prop.GetValue(draggedObj);
//             }
//
//             if (value == null)
//             {
//                 Debug.LogWarning($"[CoverFieldDragging] Cannot find {fieldName} in {draggedObj}");
//                 return;
//             }
//
//             ApplyValueToSerializedProperty(property, value);
//         }
//
//         private void ApplyValueToSerializedProperty(SerializedProperty property, object value)
//         {
//             property.serializedObject.Update();
//
//             object target = property.serializedObject.targetObject;
//             Type hostType = target.GetType();
//
//             // Tìm field thật theo property name
//             FieldInfo fieldInfo = hostType.GetField(property.propertyPath,
//                 BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
//
//             if (fieldInfo == null)
//             {
//                 Debug.LogWarning($"[CoverFieldDragging] Cannot find field '{property.propertyPath}' on {hostType.Name}");
//                 return;
//             }
//
//             Type fieldType = fieldInfo.FieldType;
//             Type valueType = value.GetType();
//
//             // Kiểm tra type
//             if (!fieldType.IsAssignableFrom(valueType))
//             {
//                 Debug.LogWarning(
//                     $"[CoverFieldDragging] Type mismatch: Trying to assign {valueType.Name} → {fieldType.Name}"
//                 );
//                 return;
//             }
//
//             // Nếu là object reference: assign qua SerializedProperty
//             if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
//             {
//                 property.objectReferenceValue = value as UnityEngine.Object;
//             }
//             else
//             {
//                 // Gán qua reflection
//                 fieldInfo.SetValue(target, value);
//             }
//
//             property.serializedObject.ApplyModifiedProperties();
//         }
//     }
// }
