using CSharpClassHelper;
using Microsoft.CodeAnalysis;
using SyntaxTreeExtensions;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SourceGeneratorHelper
{
    public abstract class SourceGeneratorBase : ISourceGenerator
    {
        protected abstract bool IsDebugMode { get; }
        protected abstract string RootFolder { get; }
        protected abstract Dictionary<string, List<CSharpClassDefinition>> GenerateClassDefinitions(List<CSharpClassDefinition> existingClassDefinitions);

        public void Execute(GeneratorExecutionContext context)
        {
            var existingClassDefinitions = context.GetCSharpClassDefinitions();
            var generatedClassDefinitions = GenerateClassDefinitions(existingClassDefinitions);

            foreach (var generatedDetail in generatedClassDefinitions)
            {
                var primaryNamespace = generatedDetail.Key;
                var generatedClasses = generatedDetail.Value;

                var classNamespaces = generatedClasses.Select(x => GetNamespaceList(x.Namespace, primaryNamespace)).Where(x => x.Length > 1).ToList();

                CreateNamespaceFolders(classNamespaces, primaryNamespace);

                foreach (var classDefinition in generatedClasses)
                {
                    var namespaces = GetNamespaceList(classDefinition.Namespace, primaryNamespace);

                    var folder = GetFolderPathForNamespaceList(namespaces, primaryNamespace);
                    var path = $@"{folder}\{classDefinition.Name}.cs";

                    File.WriteAllText(path, classDefinition.ToString());
                }
            }
        }

        protected void CreateNamespaceFolders(List<string[]> generatedNamespaces, string primaryNamespace)
        {
            foreach (var generatedNamespace in generatedNamespaces)
            {
                var folderToCreate = GetFolderPathForNamespaceList(generatedNamespace, primaryNamespace);

                if (Directory.Exists(folderToCreate))
                {
                    Directory.Delete(folderToCreate, true);
                }

                Directory.CreateDirectory(folderToCreate);
            }
        }

        protected string[] GetNamespaceList(string classNamespace, string primaryNamespace)
        {
            return classNamespace.Replace(primaryNamespace, string.Empty).Split('.');
        }

        protected string GetFolderPathForNamespaceList(string[] namespaces, string primaryNamespace)
        {
            var folderToCreate = string.Join(@"\", namespaces);
            return $@"{RootFolder}\{primaryNamespace}{folderToCreate}";
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            if (IsDebugMode && !Debugger.IsAttached)
            {
                Debugger.Launch();
            }
        }
    }
}
