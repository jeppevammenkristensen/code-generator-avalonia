using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Client.Services.CodeToForms;

public class CodeToFormsService
{
    public CodeToFormsContext Process(string source, CodeToFormsContext? context)
    {
        var result = SyntaxFactory.ParseCompilationUnit(source);
        if (result.Members.OfType<ClassDeclarationSyntax>().FirstOrDefault() is { } classDeclarationSyntax)
        {
            var codeToFormProperties = classDeclarationSyntax.Members
                .OfType<PropertyDeclarationSyntax>()
                .Select(x =>
                    new CodeToFormProperty(x.Identifier.Text, x.Type.ToString(), x.Identifier.Text,
                        ComponentType.TextBox)).ToArray();

            if (context is {Properties: {Length: > 0} properties})
            {
                var groups = from prop in codeToFormProperties
                    join existingProp in properties on prop.Name equals existingProp.Name
                        into groupmatch
                    from match in groupmatch.DefaultIfEmpty()
                    select new {prop, match};

                foreach (var item in groups)
                {
                    if (item.match is { } match)
                    {
                        item.prop.ComponentType = match.ComponentType;
                        item.prop.LabelName = match.LabelName;
                        item.prop.Checked = match.Checked;
                    }
                }
            }

            return new CodeToFormsContext(true, classDeclarationSyntax.Identifier.ToString(), codeToFormProperties,
                null);
        }

        return new CodeToFormsContext(false, string.Empty, [], ["Found no class in the code"]);

    }

    public string Build(CodeToFormsContext? context)
    {
        var builderOperation = new CodeToFormsWpfBuildOperation(context!);
        return builderOperation.Build();
    }
}

public class StartStopOperation : IDisposable
{
    private readonly Action _stop;

    public StartStopOperation(Action start, Action stop)
    {
        _stop = stop;
        start();
    }
    
    public void Dispose()
    {
        _stop();
    }
}