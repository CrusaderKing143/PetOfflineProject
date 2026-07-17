using System;
using System.Collections.Generic;
using System.Reflection;
using PetOffline.Core;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PetOffline.Editor
{
    public static partial class ProjectValidator
    {
        private static void ValidateReferenceOwners(
            string scenePath,
            IEnumerable<MonoBehaviour> behaviours,
            ICollection<string> errors)
        {
            foreach (MonoBehaviour behaviour in behaviours)
            {
                string fullName = behaviour.GetType().FullName;
                if (ReferenceOwnerNames.Contains(fullName) || behaviour is ILevelRuntime)
                {
                    ValidateSerializedObjectReferences(scenePath, behaviour, errors);
                }
            }
        }

        private static void ValidateSerializedObjectReferences(
            string scenePath,
            MonoBehaviour owner,
            ICollection<string> errors)
        {
            try
            {
                var serializedObject = new SerializedObject(owner);
                serializedObject.UpdateIfRequiredOrScript();
                foreach (FieldInfo field in GetSerializedObjectReferenceFields(owner.GetType()))
                {
                    ValidateSerializedObjectReference(scenePath, owner, serializedObject, field, errors);
                }
            }
            catch (Exception exception)
            {
                errors.Add($"{scenePath}: could not inspect {owner.GetType().FullName}: {exception.Message}");
            }
        }

        private static void ValidateSerializedObjectReference(
            string scenePath,
            MonoBehaviour owner,
            SerializedObject serializedObject,
            FieldInfo field,
            ICollection<string> errors)
        {
            if (IsAllowedEmptyReference(scenePath, owner, field))
            {
                return;
            }

            SerializedProperty property = serializedObject.FindProperty(field.Name);
            if (property == null)
            {
                return;
            }

            if (field.FieldType.IsArray)
            {
                ValidateSerializedObjectArray(scenePath, owner, property, field.Name, errors);
                return;
            }

            if (property.propertyType != SerializedPropertyType.ObjectReference ||
                property.objectReferenceValue != null)
            {
                return;
            }

            errors.Add(
                $"{scenePath}: {owner.GetType().FullName} at {GetHierarchyPath(owner.transform)} " +
                $"has an unassigned serialized reference '{field.Name}'.");
        }

        private static bool IsAllowedEmptyReference(
            string scenePath,
            MonoBehaviour owner,
            FieldInfo field)
        {
            return string.Equals(scenePath, Main1Path, StringComparison.Ordinal)
                && string.Equals(
                    owner.GetType().FullName,
                    "PetOffline.Gameplay.BananaSlipZone",
                    StringComparison.Ordinal)
                && string.Equals(field.Name, "banana", StringComparison.Ordinal);
        }

        private static void ValidateSerializedObjectArray(
            string scenePath,
            MonoBehaviour owner,
            SerializedProperty property,
            string fieldName,
            ICollection<string> errors)
        {
            string ownerPath = GetHierarchyPath(owner.transform);
            if (!property.isArray || property.arraySize == 0)
            {
                errors.Add(
                    $"{scenePath}: {owner.GetType().FullName} at {ownerPath} " +
                    $"has an empty serialized reference array '{fieldName}'.");
                return;
            }

            for (var index = 0; index < property.arraySize; index++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(index);
                if (element.objectReferenceValue == null)
                {
                    errors.Add(
                        $"{scenePath}: {owner.GetType().FullName} at {ownerPath} " +
                        $"has an unassigned reference '{fieldName}[{index}]'.");
                }
            }
        }

        private static IEnumerable<FieldInfo> GetSerializedObjectReferenceFields(Type type)
        {
            const BindingFlags flags =
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            for (Type current = type; current != null && current != typeof(MonoBehaviour); current = current.BaseType)
            {
                foreach (FieldInfo field in current.GetFields(flags))
                {
                    bool hasSerializeField = field.GetCustomAttributes(typeof(SerializeField), true).Length > 0;
                    if (!field.IsStatic && hasSerializeField && IsObjectReferenceField(field.FieldType))
                    {
                        yield return field;
                    }
                }
            }
        }

        private static bool IsObjectReferenceField(Type fieldType)
        {
            if (typeof(Object).IsAssignableFrom(fieldType))
            {
                return true;
            }

            Type elementType = fieldType.IsArray ? fieldType.GetElementType() : null;
            return elementType != null && typeof(Object).IsAssignableFrom(elementType);
        }
    }
}
