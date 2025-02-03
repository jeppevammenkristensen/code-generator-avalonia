using System;
using System.Linq;
using Client.Services.CodeToGrid;

namespace Client.Tests.Extensions;

public static class Extensions
{
    public static void ModifyProperty(this CodeToGridContext context,
        string property,
        Action<CodeToGridProperty> action)
    {
        var res = context.Properties.First(x => x.Name == property);
        action(res);
    }
}