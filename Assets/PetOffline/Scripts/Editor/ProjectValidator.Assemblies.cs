using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PetOffline.Editor
{
    public static partial class ProjectValidator
    {
        private static void ValidateAssemblies(List<string> errors)
        {
            Dictionary<string, AssemblyDefinitionData> definitions = LoadAssemblyDefinitions(errors);
            ValidateAssemblySet(definitions, errors);
            ValidateExactReferences(definitions, "PetOffline.Core", Array.Empty<string>(), errors);
            ValidateExactReferences(definitions, "PetOffline.Gameplay", new[] { "PetOffline.Core" }, errors);
            ValidateExactReferences(
                definitions,
                "PetOffline.UI",
                new[] { "PetOffline.Core", "Unity.ugui" },
                errors);
            ValidateExactReferences(
                definitions,
                "PetOffline.Editor",
                new[] { "PetOffline.Core", "PetOffline.Gameplay", "PetOffline.UI", "Unity.ugui" },
                errors);
            ValidateExactReferences(
                definitions,
                "PetOffline.Tests.Editor",
                new[] { "PetOffline.Core", "PetOffline.Gameplay" },
                errors);
            ValidateEditorOnly(definitions, "PetOffline.Editor", errors);
            ValidateTestAssembly(definitions, errors);
        }

        private static Dictionary<string, AssemblyDefinitionData> LoadAssemblyDefinitions(List<string> errors)
        {
            var result = new Dictionary<string, AssemblyDefinitionData>(StringComparer.Ordinal);
            if (!Directory.Exists("Assets/PetOffline"))
            {
                errors.Add("Assets/PetOffline is missing, so assembly definitions cannot be validated.");
                return result;
            }

            string[] paths = Directory.GetFiles("Assets/PetOffline", "*.asmdef", SearchOption.AllDirectories);
            foreach (string path in paths)
            {
                TryLoadAssemblyDefinition(path.Replace('\\', '/'), result, errors);
            }

            return result;
        }

        private static void TryLoadAssemblyDefinition(
            string path,
            IDictionary<string, AssemblyDefinitionData> definitions,
            ICollection<string> errors)
        {
            try
            {
                AssemblyDefinitionData definition = JsonUtility.FromJson<AssemblyDefinitionData>(File.ReadAllText(path));
                if (definition == null || string.IsNullOrWhiteSpace(definition.name))
                {
                    errors.Add($"Assembly definition has no valid name: {path}");
                    return;
                }

                if (definitions.ContainsKey(definition.name))
                {
                    errors.Add($"Duplicate assembly definition name '{definition.name}' at {path}.");
                    return;
                }

                definitions.Add(definition.name, definition);
            }
            catch (Exception exception)
            {
                errors.Add($"Cannot read assembly definition {path}: {exception.Message}");
            }
        }

        private static void ValidateAssemblySet(
            IReadOnlyDictionary<string, AssemblyDefinitionData> definitions,
            ICollection<string> errors)
        {
            foreach (string expectedName in ExpectedAssemblyNames)
            {
                if (!definitions.ContainsKey(expectedName))
                {
                    errors.Add($"Required assembly definition '{expectedName}' is missing.");
                }
            }

            foreach (string name in definitions.Keys)
            {
                bool isExpected = ExpectedAssemblyNames.Contains(name);
                if (name.StartsWith("PetOffline.", StringComparison.Ordinal) && !isExpected)
                {
                    errors.Add($"Unexpected Pet Offline assembly definition '{name}'; exactly five are expected.");
                }
            }
        }

        private static void ValidateExactReferences(
            IReadOnlyDictionary<string, AssemblyDefinitionData> definitions,
            string assemblyName,
            IEnumerable<string> expectedReferences,
            ICollection<string> errors)
        {
            if (!definitions.TryGetValue(assemblyName, out AssemblyDefinitionData definition))
            {
                return;
            }

            var expected = new HashSet<string>(expectedReferences, StringComparer.Ordinal);
            var actual = new HashSet<string>(
                GetNormalizedReferences(definition),
                StringComparer.Ordinal);
            if (!actual.SetEquals(expected))
            {
                errors.Add(
                    $"{assemblyName} references must be [{JoinNames(expected)}], " +
                    $"but are [{JoinNames(actual)}].");
            }
        }

        private static void ValidateEditorOnly(
            IReadOnlyDictionary<string, AssemblyDefinitionData> definitions,
            string assemblyName,
            ICollection<string> errors)
        {
            if (!definitions.TryGetValue(assemblyName, out AssemblyDefinitionData definition))
            {
                return;
            }

            string[] platforms = definition.includePlatforms ?? Array.Empty<string>();
            if (platforms.Length != 1 || !string.Equals(platforms[0], "Editor", StringComparison.Ordinal))
            {
                errors.Add($"{assemblyName} must include only the Editor platform.");
            }
        }

        private static void ValidateTestAssembly(
            IReadOnlyDictionary<string, AssemblyDefinitionData> definitions,
            ICollection<string> errors)
        {
            const string assemblyName = "PetOffline.Tests.Editor";
            ValidateEditorOnly(definitions, assemblyName, errors);
            if (!definitions.TryGetValue(assemblyName, out AssemblyDefinitionData definition))
            {
                return;
            }

            string[] optionalReferences = definition.optionalUnityReferences ?? Array.Empty<string>();
            if (!optionalReferences.Contains("TestAssemblies"))
            {
                errors.Add($"{assemblyName} must opt into TestAssemblies.");
            }
        }

        private static string[] GetNormalizedReferences(AssemblyDefinitionData definition)
        {
            return (definition.references ?? Array.Empty<string>())
                .Select(NormalizeAssemblyReference)
                .ToArray();
        }

        private static string NormalizeAssemblyReference(string reference)
        {
            if (string.IsNullOrEmpty(reference) ||
                !reference.StartsWith("GUID:", StringComparison.OrdinalIgnoreCase))
            {
                return reference;
            }

            string path = AssetDatabase.GUIDToAssetPath(reference.Substring(5));
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return reference;
            }

            AssemblyDefinitionData definition =
                JsonUtility.FromJson<AssemblyDefinitionData>(File.ReadAllText(path));
            return definition == null || string.IsNullOrEmpty(definition.name) ? reference : definition.name;
        }

        private static string JoinNames(IEnumerable<string> names)
        {
            return string.Join(", ", names.OrderBy(name => name, StringComparer.Ordinal));
        }

        [Serializable]
        private sealed class AssemblyDefinitionData
        {
            public string name;
            public string[] references;
            public string[] includePlatforms;
            public string[] optionalUnityReferences;
        }
    }
}
