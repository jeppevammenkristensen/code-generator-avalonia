using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Client.Services.CodeToForms;

public class CodeToFormsWpfBuildOperation(CodeToFormsContext context)
{
    private readonly StringBuilder _builder = new();

    public string Build()
    {
        using (new StartStopOperation(InitGrid, CloseGrid))
        {
            BuildInputElements();
            BuildButton();
        }
        
        var result = _builder.ToString();
        var parsed = XDocument.Parse(result);
        result = parsed.ToString();
        _builder.Clear();
        return result;
    }

    private CodeToFormsWpfBuildOperation BuildButton()
    {
        var row = context.GetCheckedProperties().Count() + 1;
        return AddLine($$"""<Button Grid.Row="{{row}}" Grid.Column="2" Content="Submit" Command="{Binding SubmitCommand}" />""");
    }

    private void BuildInputElements()
    {
        foreach (var (codeToFormProperty, row) in context.GetCheckedProperties().Select((x,i) => (x,i+1)))
        {
            BuildLabel(codeToFormProperty, row, 0);
                
            BuildInputElement(codeToFormProperty, row,1);
        }
    }

    private CodeToFormsWpfBuildOperation BuildInputElement(CodeToFormProperty codeToFormProperty, int row, int column)
    {
        switch (codeToFormProperty.ComponentType)
        {
            case ComponentType.TextBox:
                return BuildTextBox(codeToFormProperty, row, column);
            case ComponentType.ComboBox:
                return BuildComboBox(codeToFormProperty, row, column);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private CodeToFormsWpfBuildOperation BuildComboBox(CodeToFormProperty codeToFormProperty, int row, int column)
    {
        return AddLine($$"""<ComboBox Grid.Row="{{row}}" Grid.Column="{{column}}" SelectedItem="{Binding Path={{codeToFormProperty.Name}}, Mode=TwoWay}" ItemsSource="{Binding {{ string.Concat(codeToFormProperty.Name, "Source")}}}"  />""");
    }

    private CodeToFormsWpfBuildOperation BuildTextBox(CodeToFormProperty codeToFormProperty, int row, int column)
    {
        return AddLine($$"""<TextBox Grid.Row="{{row}}" Grid.Column="{{column}}" Text="{Binding Path={{codeToFormProperty.Name}}, Mode=TwoWay}"  />""");
    }

    private CodeToFormsWpfBuildOperation BuildLabel(CodeToFormProperty codeToFormProperty, int row, int column)
    {
        return AddLine($"""<Label Grid.Row="{row}" Grid.Column="{column}" Content="{codeToFormProperty.LabelName}"/>""");
    }

    private void InitGrid()
    {
        AddLine("<Grid>").
            AddLine("<Grid.RowDefinitions>")
            .AddRowDefinition(); // For header
        foreach (var _ in context.Properties.Where(x => x.Checked))
        {
            AddRowDefinition();
        }

        AddRowDefinition() // bottom
            .AddLine("</Grid.RowDefinitions>");

        AddLine("<Grid.ColumnDefinitions>")
            .AddColumnDefinition()
            .AddColumnDefinition("*")
            .AddColumnDefinition();
        
        AddLine("</Grid.ColumnDefinitions>");
    }

    private void CloseGrid()
    {
        AddLine("</Grid>");
    }

    private CodeToFormsWpfBuildOperation AddRowDefinition(string height= "Auto")
    {
        return AddLine($"""<RowDefinition Height="{height}"/>""");
    }
    
    private CodeToFormsWpfBuildOperation AddColumnDefinition(string height= "Auto")
    {
        return AddLine($"""<ColumnDefinition Width="{height}"/>""");
    }

    private CodeToFormsWpfBuildOperation AddLine(string line)
    {
        _builder.AppendLine(line);
        return this;
    }
}