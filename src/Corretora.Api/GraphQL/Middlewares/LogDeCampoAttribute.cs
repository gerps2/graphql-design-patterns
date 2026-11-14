using System.Reflection;
using System.Runtime.CompilerServices;
using HotChocolate.Types.Descriptors;

namespace Corretora.Api.GraphQL.Middlewares;

// Liga o LogDeCampoMiddleware só no campo marcado com [LogDeCampo]: os campos que vão ao banco.
// Um middleware global rodaria em todo campo da resposta, até no nome do usuário, e custaria caro.
public sealed class LogDeCampoAttribute : ObjectFieldDescriptorAttribute
{
    public LogDeCampoAttribute([CallerLineNumber] int order = 0) => Order = order;

    protected override void OnConfigure(IDescriptorContext context, IObjectFieldDescriptor descriptor, MemberInfo? member) =>
        descriptor.Use<LogDeCampoMiddleware>();
}
