using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StoreX.Tests;

public class ModuleBoundaryTests
{
    private static readonly Regex ModuleInternalReferenceRegex = new(
        @"StoreX\.Api\.Modules\.(?<module>[A-Za-z]\w*)\.(Application|Configurations|Domain|Endpoints|Entities|Seeding)(\.|$)",
        RegexOptions.Compiled);


    [Fact]
    public void Modules_ShouldNotDependOnOtherModulesInternals()
    {
        var modulesDirectory = GetModulesDirectory();
        var moduleDirectories = Directory.GetDirectories(modulesDirectory);
        var violations = new List<string>();

        foreach (var moduleDirectory in moduleDirectories)
        {
            var currentModule = Path.GetFileName(moduleDirectory);

            foreach (var sourceFile in Directory.GetFiles(
                         moduleDirectory,
                         "*.cs",
                         SearchOption.AllDirectories))
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    File.ReadAllText(sourceFile));

                var root = syntaxTree.GetRoot();

                var referencedNames = root.DescendantNodes()
                    .OfType<NameSyntax>()
                    .Select(name => name.ToString())
                    .Select(name => name.Replace("global::", string.Empty));

                foreach (var referencedName in referencedNames)
                {
                    var match = ModuleInternalReferenceRegex.Match(
                        referencedName);

                    if (!match.Success)
                        continue;

                    var referencedModule =
                        match.Groups["module"].Value;

                    if (!moduleDirectories.Any(directory =>
                            string.Equals(
                                Path.GetFileName(directory),
                                referencedModule,
                                StringComparison.Ordinal)))
                    {
                        continue;
                    }

                    if (string.Equals(
                            currentModule,
                            referencedModule,
                            StringComparison.Ordinal))
                    {
                        continue;
                    }

                    violations.Add(
                        $"{currentModule} depends on internal code of " +
                        $"{referencedModule}: " +
                        Path.GetRelativePath(modulesDirectory, sourceFile));
                }
            }
        }

        Assert.True(
            violations.Count == 0,
            "Module boundary violations were found:" +
            Environment.NewLine +
            string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void Modules_ShouldNotAccessCentralDbContextDirectly()
    {
        var modulesDirectory = GetModulesDirectory();
        var violations = new List<string>();

        foreach (var sourceFile in Directory.GetFiles(
                     modulesDirectory,
                     "*.cs",
                     SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(
                modulesDirectory,
                sourceFile);

            var currentModule = relativePath
                .Split(Path.DirectorySeparatorChar)[0];

            var syntaxTree = CSharpSyntaxTree.ParseText(
                File.ReadAllText(sourceFile));

            var root = syntaxTree.GetRoot();

            foreach (var usingDirective in root.DescendantNodes()
                         .OfType<UsingDirectiveSyntax>())
            {
                var namespaceName = usingDirective.Name?
                    .ToString()
                    .Replace("global::", string.Empty);

                const string dbContextNamespace =
                    "StoreX.Api.Infrastructure.Persistence";

                if (namespaceName is null ||
                    !(namespaceName.Equals(
                          dbContextNamespace,
                          StringComparison.Ordinal) ||
                      namespaceName.StartsWith(
                          dbContextNamespace + ".",
                          StringComparison.Ordinal)))
                {
                    continue;
                }

                var isAllowedBranchServiceUsing =
                    string.Equals(
                        currentModule,
                        "Organization",
                        StringComparison.Ordinal) &&
                    string.Equals(
                        relativePath.Replace('\\', '/'),
                        "Organization/Application/BranchService.cs",
                        StringComparison.OrdinalIgnoreCase);

                if (!isAllowedBranchServiceUsing)
                    violations.Add(relativePath);
            }

            foreach (var identifier in root.DescendantNodes()
                         .OfType<IdentifierNameSyntax>()
                         .Where(node =>
                             node.Identifier.ValueText == "StoreXDbContext"))
            {
                var containingClass = identifier
                    .Ancestors()
                    .OfType<ClassDeclarationSyntax>()
                    .FirstOrDefault();

                var isAllowedBranchServiceReference =
                    string.Equals(
                        currentModule,
                        "Organization",
                        StringComparison.Ordinal) &&
                    string.Equals(
                        relativePath.Replace(Path.DirectorySeparatorChar, '/'),
                        "Organization/Application/BranchService.cs",
                        StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(
                        containingClass?.Identifier.ValueText,
                        "BranchService",
                        StringComparison.Ordinal);

                if (!isAllowedBranchServiceReference)
                    violations.Add(relativePath);
            }
        }

        Assert.True(
            violations.Count == 0,
            "Modules must not access StoreXDbContext directly. " +
            "Use module contracts instead." +
            Environment.NewLine +
            string.Join(Environment.NewLine, violations.Distinct()));
    }

    private static string GetModulesDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(
                directory.FullName,
                "src",
                "StoreX.Api",
                "Modules");

            if (Directory.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the StoreX modules directory.");
    }
}




