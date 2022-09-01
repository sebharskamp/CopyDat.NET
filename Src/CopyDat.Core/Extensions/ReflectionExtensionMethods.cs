using System.Reflection;
using System.Threading.Tasks;
using System;

namespace CopyDat.Core.Extensions
{
    public static class ReflectionExtensionMethods
    {
        public static async Task<object?> InvokeAsync(this MethodInfo methodInfo, object obj, params object?[] parameters)
        {
            var task = (Task?)methodInfo.Invoke(obj, parameters);
            if (task is null) throw new MethodAccessException();
            await task.ConfigureAwait(false);
            var resultProperty = task.GetType().GetProperty("Result");
            if (resultProperty is null) throw new MemberAccessException();
            return resultProperty.GetValue(task);
        }
    }
}
