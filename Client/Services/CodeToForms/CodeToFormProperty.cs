namespace Client.Services.CodeToForms;

public class CodeToFormProperty(string name, string type, string labelName, ComponentType componentType)
{
    public bool Checked { get; set; } = true;
    
    public string Name { get; } = name;
    public string Type { get;  } = type;
    public string LabelName { get; set; } = labelName;
    public ComponentType ComponentType { get; set; } = componentType;

    public void Deconstruct(out string name, out string type, out string labelName, out ComponentType componentType)
    {
        name = Name;
        type = Type;
        labelName = LabelName;
        componentType = ComponentType;
    }
}