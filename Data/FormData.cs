namespace Data;

public class FormData
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string Source { get; set; }
    public List<FormDataProperty> Properties { get; set; } = new();
}