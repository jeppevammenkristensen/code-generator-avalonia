using System.Threading.Tasks;

namespace Client.Services.CodeToGrid.Operations;

public interface ICodeGridOperation
{
    string Name { get; }
    
    ValueTask<string> Build(CodeToGridContext context);
}