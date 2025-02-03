using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Client.Services.CodeToForms;

namespace Client.Services.CodeToGrid.Operations;

public class DefaultTemplateOperation : ICodeGridOperation
{
    private CodeToGridContext _context;
    private StringBuilder _builder = new();

    public ValueTask<string> Build(CodeToGridContext context)
    {
        _context = context;
        _builder.Clear();

        using (new StartStopOperation(InitGrid, () => _builder.AppendLine("</DataGrid>")))
        {
            using (new StartStopOperation(() => _builder.AppendLine("<DataGrid.Columns>"),
                       () => _builder.AppendLine("</DataGrid.Columns>")))
            {
                BuildColumns();    
            }
        }
        
        return new ValueTask<string>(_builder.ToString());
    }

    private void BuildColumns()
    {
        foreach (var checkedProperty in _context.Properties.Where(x => x.IsChecked))
        {
            BuildColumn(checkedProperty);
        }
    }

    private void BuildColumn(CodeToGridProperty checkedProperty)
    {
        if (checkedProperty is {IsEnumerable: false})
        {
            switch (checkedProperty.ColumnType)
            {
                case CodeToGridColumnType.Text:
                    BuildTextColumn(checkedProperty);
                    break;
                case CodeToGridColumnType.CheckBox:
                    BuildCheckboxColumn(checkedProperty);
                    break;
                case CodeToGridColumnType.ComboBox:
                    BuildComboBox(checkedProperty);
                    break;
                case CodeToGridColumnType.Template:
                    BuildTemplate(checkedProperty);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void BuildTemplate(CodeToGridProperty checkedProperty)
    {
        var templateColumn =
            new XElement("DataGridTemplateColumn",
                new XAttribute("Header", checkedProperty.HeaderName));
        
        var cell = new XElement("DataGridTemplateColumn.CellTemplate");
        cell.SetAttributeValue("DataType", _context.Code.Name);

        if (checkedProperty.IsBoolean())
        {
            cell.Add(new XElement("CheckBox", new XAttribute("IsChecked", $$"""{Binding Path={{checkedProperty.Name}}}"""), new XAttribute("IsReadOnly", "true")));
        }
        else
        {
            cell.Add(new XElement("TextBox", new XAttribute("Text", $$"""{Binding Path={{checkedProperty.Name}}}"""), new XAttribute("IsReadOnly","true")));
        }
        
        templateColumn.Add(cell);
    
        if (!checkedProperty.IsReadonly)
        {
            var editCell = new XElement("DataGridTemplateColumn.CellEditingTemplate");

            if (checkedProperty.IsBoolean())
            {
                editCell.Add(new XElement("CheckBox", new XAttribute("IsChecked", $$"""{Binding Path={{checkedProperty.Name}}}""")));
            }
            else
            {
                editCell.Add(new XElement("TextBox", new XAttribute("Text", $$"""{Binding Path={{checkedProperty.Name}}, Mode=TwoWay}""")));
            }
            
            templateColumn.Add(editCell);
        }
        
        _builder.AppendLine(templateColumn.ToString());
        
    }

    private void BuildComboBox(CodeToGridProperty checkedProperty)
    {
        // trying something new
        var templateColumn = 
            new XElement("DataGridTemplateColumn", 
                new XAttribute("Header", checkedProperty.HeaderName));
        
        var cell = new XElement("DataGridTemplateColumn.CellTemplate");
        cell.SetAttributeValue("DataType", _context.Code.Name);
        cell.Add(new XElement("TextBlock", new XAttribute("Text", $$"""{Binding Path={{checkedProperty.Name}}, Mode=TwoWay}""")));
        templateColumn.Add(cell);

        if (!checkedProperty.IsReadonly)
        {
            var edit = new XElement("DataGridTemplateColumn.CellEditingTemplate", new XAttribute("Header", checkedProperty.HeaderName));
            edit.Add(new XElement("ComboBox", 
                new XAttribute("SelectedItem", $$"""{Binding Path={{checkedProperty.Name}}, Mode=TwoWay}"""), 
                new XAttribute("ItemsSource", """{Binding ..}""")));
        
            templateColumn.Add(edit);
        }
                
        _builder.AppendLine(templateColumn.ToString());
        
    }

    private void BuildCheckboxColumn(CodeToGridProperty checkedProperty)
    {
        AppendLine($$"""
                     <DataGridCheckBoxColumn
                         Binding="{Binding {{checkedProperty.Name}}}"
                         Header="{{checkedProperty.HeaderName}}"
                         IsReadOnly="{{(checkedProperty.IsReadonly ? bool.TrueString : bool.FalseString)}}"></DataGridCheckBoxColumn>
                     """);
    }
    
    private void BuildTextColumn(CodeToGridProperty checkedProperty)
    {
        AppendLine($$"""
                     <DataGridTextColumn
                         Binding="{Binding {{checkedProperty.Name}}}"
                         Header="{{checkedProperty.HeaderName}}"
                         IsReadOnly="{{(checkedProperty.IsReadonly ? bool.TrueString : bool.FalseString)}}"></DataGridTextColumn>
                     """);
    }

    private void InitGrid()
    {
        AppendLine("<DataGrid");
        AppendLine($$"""
                     	ItemsSource="{Binding {{_context.SourceProperty}}}"
                     	AutoGenerateColumns="False"
                        CanUserReorderColumns="True"
                        CanUserSortColumns="True" >
                     """);
    }

    public DefaultTemplateOperation AppendLine(string line)
    {
        _builder.AppendLine(line);
        return this;
    }

    public string Name { get; } = "Default";
}