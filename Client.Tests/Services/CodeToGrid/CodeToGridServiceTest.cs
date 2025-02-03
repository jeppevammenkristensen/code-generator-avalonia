using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Client.Services.CodeToGrid;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Client.Tests.Services.CodeToGrid;

[TestSubject(typeof(CodeToGridService))]
public class CodeToGridServiceTest
{

    [Fact]
    public void Analyze()
    {
        var sut = new CodeToGridService();
        var result =
            sut.Analyze("public class Something { public int[] Items {get;set; } public string Name {get;set;} public List<string> Names {get;set; }", "Items");

        result.Should().NotBeNull();
        result!.Properties.Should().HaveCount(3);
        result!.Properties[0].Should().BeEquivalentTo(new
        {
            Name = "Items",
            Type = "int[]",
            ColumnType = CodeToGridColumnType.Text,
            IsEnumerable = true
        });
        result!.Properties[0].UnderlyingTypeAsString.Should().Be("int");
        
        result!.Properties[1].Should().BeEquivalentTo(new
        {
            Name = "Name",
            Type = "string",
            ColumnType = CodeToGridColumnType.Text,
            IsEnumerable = false,
        });
        
        result!.Properties[1].UnderlyingTypeAsString.Should().BeNull();

        result!.Properties[2].Should().BeEquivalentTo(new
        {
            Name = "Names",
            Type = "System.Collections.Generic.List<string>",
            ColumnType = CodeToGridColumnType.Text,
            IsEnumerable = true,
        });
        
        result!.Properties[2].UnderlyingTypeAsString.Should().Be("string");
    }

    [Fact]
    public async Task Process_Default_GeneratesResultWithThreeColumns()
    {
        var sut = new CodeToGridService();
        var codeContext =   
            sut.Analyze("""
                        public class Something 
                        { 
                            public int[] Items {get;set; } 
                            public bool BoolValue {get;set;} 
                            public string Name {get;set;} 
                            public List<string> Skipped {get;set; }
                        """, "Items");
        
        codeContext!.Properties.First(x => x.Name == "Skipped").IsChecked = false;
        var result = await sut.Process(codeContext, "Default");
        result.Should().NotBeNull();
        var parsedXml = XDocument.Parse(result!);
        parsedXml.Should()
            .HaveRoot("DataGrid").Which.Should()
           ;

        int i = 0;
    }
}