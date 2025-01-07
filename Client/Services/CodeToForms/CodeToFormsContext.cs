using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Client.Services.CodeToForms;

public record CodeToFormsContext(bool IsValid,string ClassCode, CodeToFormProperty[] Properties, string[]? Errors)
{
    public IEnumerable<CodeToFormProperty> GetCheckedProperties() => Properties.Where(x => x.Checked);
}