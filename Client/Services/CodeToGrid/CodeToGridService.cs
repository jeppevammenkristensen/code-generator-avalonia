using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Client.Services.CodeToGrid.Operations;

namespace Client.Services.CodeToGrid;

public class CodeToGridService
{
    public static ICodeGridOperation[] Operations = [new DefaultTemplateOperation()];
    
    public CodeToGridContext? Analyze(string source, string sourceProperty)
    {
        var result = SyntaxFactory.ParseCompilationUnit(source);
        
        result = result.AddUsings(
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Linq")));
        // Generate data so i can get the semantic model
        
        // Step 2: Create a SyntaxTree from the result
        var syntaxTree = result.SyntaxTree;

        // Step 3: Create a Compilation with the SyntaxTree
        var compilation = CSharpCompilation.Create(
            "MyAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location) // Reference to mscorlib
            });
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        
        // Step 4: Get the SemanticModel
        if (result.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault() is { } firstClass)
        {
            if (semanticModel.GetDeclaredSymbol(firstClass) is {} declaredSymbol)
            {
                var properties = declaredSymbol.GetMembers().OfType<IPropertySymbol>().ToList();
                return new CodeToGridContext(declaredSymbol,sourceProperty,
                    properties
                        .Select(CreateGridProperty).ToList());
            }
        }

        return null;
    }

    private static CodeToGridProperty CreateGridProperty(IPropertySymbol x)
    {
        var codeToGridColumnType = CodeToGridColumnType.Text;
        
        if (x.Type.SpecialType == SpecialType.System_Boolean)
        {
            codeToGridColumnType = CodeToGridColumnType.CheckBox;
        }

        
        return new CodeToGridProperty(x, codeToGridColumnType);
    }

    public async ValueTask<string?> Process(CodeToGridContext codeToGridContext, string template)
    {
        ArgumentNullException.ThrowIfNull(codeToGridContext);

        if (Operations.SingleOrDefault(x => x.Name == template) is { } operation)
        {
            return XElement.Parse(await operation.Build(codeToGridContext)).ToString();
        }

        return null;
    }
}