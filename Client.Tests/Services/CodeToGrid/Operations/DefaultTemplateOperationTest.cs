using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Xml.Linq;
using Client.Services.CodeToGrid;
using Client.Services.CodeToGrid.Operations;
using Client.Tests.Extensions;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace Client.Tests.Services.CodeToGrid.Operations;


public abstract class AbstractCodeToGridTemplateOperationTest<T> where T : ICodeGridOperation
{

    protected abstract T GetOperation();

    protected async ValueTask<XDocument> ProcessFromCode(string code)
    {
        return await ProcessFromCode(GenerateContext(code));
    }
    
    protected async ValueTask<XDocument> ProcessFromCode(CodeToGridContext code)
    {
        var operation = GetOperation();
        var result = await operation.Build(code);
        return XDocument.Parse(result);
    }

    protected CodeToGridContext GenerateContext(string code)
    {
        var service = new CodeToGridService();
        return service.Analyze(code, "Property") ?? throw new InvalidOperationException("Could not analyze code");

    }
    
    public class TestHarness
    {
        public CodeToGridService Service { get; } = new CodeToGridService();
        
        public TestHarness()
        {
            
        }

        public CodeToGridContext GetContext(string code)
        {
            return Service.Analyze(code, "Property");
        }
    }
}

[TestSubject(typeof(DefaultTemplateOperation))]
public class DefaultTemplateOperationTest : AbstractCodeToGridTemplateOperationTest<DefaultTemplateOperation>
{

    [Fact]
    public async Task Build_GeneratesCorrectSetup()
    {
        var result =  await this.ProcessFromCode("""public class Test { public string Property { get; set; } }""");
        result.Should().HaveRoot("DataGrid")
            .Which.Should().HaveElement("DataGrid.Columns", Exactly.Once())
            .Which.Elements().Should().HaveCount(1);
    }
    
    [Fact]
    public async Task Build_WithTextProperty_WithHeader()
    {
        var codeToGridContext = GenerateContext("public class Test { public string Property {get;set;} public CustomValue OtherProperty {get;set;} }");
       codeToGridContext.ModifyProperty("Property", x =>
       {
           x.IsReadonly = true;
           x.HeaderName = "Custom Header";
       });
       
       var result = await ProcessFromCode(codeToGridContext);
       var textColumns = result.Descendants("DataGridTextColumn").ToList();
       textColumns.Should().HaveCount(2);
       
       textColumns[0].Should()
           .HaveAttribute("Header", "Custom Header")
           .And.HaveAttribute("Binding", "{Binding Property}")
           .And.HaveAttribute("IsReadOnly", "True");

       textColumns[1].Should()
           .HaveAttribute("Header", "OtherProperty")
           .And.HaveAttribute("IsReadOnly", "False")
           .And.HaveAttribute("Binding", "{Binding OtherProperty}");
    }

    [Fact]
    public async Task Build_WithTemplate_GeneratesCorrectSetup()
    {
        var context = GenerateContext("public class Test { public string Property {get;set;} public bool OtherProperty {get;set;} public double DoubleProperty {get;set;} }");
        context.ModifyProperty("Property", x => x.ColumnType = CodeToGridColumnType.Template);
        context.ModifyProperty("OtherProperty", x => x.ColumnType = CodeToGridColumnType.Template);
        context.ModifyProperty("DoubleProperty", x => x.ColumnType = CodeToGridColumnType.Template);
        
        var result = await ProcessFromCode(context);
        result.Should().HaveElement("DataGrid.Columns").Which.Should().HaveElement("DataGridTemplateColumn", Exactly.Times(3));
        var textColumns = result.Descendants("DataGridTemplateColumn").ToList();
        textColumns.Should().HaveCount(3);
        textColumns[0].Should()
            .HaveElement("DataGridTemplateColumn.CellEditingTemplate")
            .Which.Should().HaveElement("TextBox");
        
        textColumns[1].Should()
            .HaveElement("DataGridTemplateColumn.CellEditingTemplate")
            .Which.Should().HaveElement("CheckBox");
        textColumns[1].Should().HaveElement("DataGridTemplateColumn.CellTemplate")
            .Which.Should().HaveElement("CheckBox").Which.Should().HaveAttribute("IsReadOnly", "true");

        textColumns[2].Should().HaveElement("DataGridTemplateColumn.CellEditingTemplate")
            .Which.Should().HaveElement("TextBox");
    }

    [Fact]
    public async Task Build_WithComboBox_GeneratesCorrectSetup()
    {
        var context = GenerateContext("public class Test { " +
                                      "public CustomValue Property {get;set;} public string ReadOnlyProperty {get;set;} }");
        context.ModifyProperty("Property", x => x.ColumnType = CodeToGridColumnType.ComboBox);
        context.ModifyProperty("ReadOnlyProperty", x =>
        {
            x.IsReadonly = true;
            x.ColumnType = CodeToGridColumnType.ComboBox;
        });
        
        var result = await ProcessFromCode(context);
        var columns = result.Descendants("DataGridTemplateColumn").ToList();
        columns.Should().HaveCount(2);
        columns[0].Should()
            .HaveAttribute("Header", "Property").And
            .HaveElement("DataGridTemplateColumn.CellTemplate")
            .And.HaveElement("DataGridTemplateColumn.CellEditingTemplate");
        columns[1].Should()
            .HaveAttribute("Header", "ReadOnlyProperty").And
            .HaveElement("DataGridTemplateColumn.CellTemplate")
            .And.HaveElement("DataGridTemplateColumn.CellEditingTemplate", Exactly.Times(0));
            ;

    }

    protected override DefaultTemplateOperation GetOperation()
    {
        return new DefaultTemplateOperation();
    }
}