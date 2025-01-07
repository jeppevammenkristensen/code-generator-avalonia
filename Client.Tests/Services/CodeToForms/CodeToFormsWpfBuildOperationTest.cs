using System.Linq;
using System.Xml.Linq;
using Client.Services.CodeToForms;
using FluentAssertions;
using Xunit;

namespace Client.Tests.Services.CodeToForms;

public class CodeToFormsWpfBuildOperationTest
{
    [Fact]
    public void Build_BuildExpected()
    {
        CodeToFormsContext context = new CodeToFormsContext(true, "class", [
            new CodeToFormProperty("FirstProperty", "string", "First Property", ComponentType.TextBox),
            new CodeToFormProperty("SecondProperty", "string", "Second Property", ComponentType.ComboBox)
        ],null);
        var sut = new CodeToFormsWpfBuildOperation(context);
        
        var result = sut.Build();
        var xDocument = XDocument.Parse(result);
        xDocument.Should().HaveRoot("Grid");
        xDocument.Root.Should().HaveElement("Grid.RowDefinitions")
            .Which.Should().HaveElement("RowDefinition", Exactly.Times(4));
        xDocument.Root.Should().HaveElement("Grid.ColumnDefinitions")
            .Which.Should().HaveElement("ColumnDefinition", Exactly.Times(3));

        xDocument.Root.Should().HaveElement("Label", Exactly.Twice());
        var labels = xDocument.Root!.Elements("Label").Should().HaveCount(2).And.Subject.ToList();

       labels[0].Should()
            .HaveAttribute("Grid.Row", "1").And
            .HaveAttribute("Grid.Column", "0").And
            .HaveAttribute("Content", "First Property");
        
        xDocument.Root.Should().HaveElement("TextBox")
            .Which.Should()
            .HaveAttribute("Grid.Row", "1").And
            .HaveAttribute("Grid.Column", "1").And
            .HaveAttribute("Text", "{Binding Path=FirstProperty, Mode=TwoWay}");
        
        labels[1].Should()
            .HaveAttribute("Grid.Row", "2").And
            .HaveAttribute("Grid.Column", "0").And
            .HaveAttribute("Content", "Second Property");

        xDocument.Root.Should().HaveElement("ComboBox")
            .Which.Should().HaveAttribute("Grid.Row", "2")
            .And.HaveAttribute("Grid.Column", "1")
            .And.HaveAttribute("ItemsSource", "{Binding SecondPropertySource}")
            .And.HaveAttribute("SelectedItem", "{Binding Path=SecondProperty, Mode=TwoWay}");

        xDocument.Root.Should().HaveElement("Button")
            .Which.Should().HaveAttribute("Grid.Row", "3")
            .And.HaveAttribute("Grid.Column", "2")
            .And.HaveAttribute("Content", "Submit")
            .And.HaveAttribute("Command", "{Binding SubmitCommand}");
            
    }
}