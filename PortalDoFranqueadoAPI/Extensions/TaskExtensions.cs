using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Extensions
{
    public static class TaskExtensions
    {
        public static ConfiguredTaskAwaitable AsNoContext(this Task task)
        {
            return task.ConfigureAwait(false);
        }

        public static ConfiguredValueTaskAwaitable AsNoContext(this ValueTask task)
        {
            return task.ConfigureAwait(false);
        }

        public static ConfiguredTaskAwaitable<TResult> AsNoContext<TResult>(this Task<TResult> task)
        {
            return task.ConfigureAwait(false);
        }

        public static ConfiguredValueTaskAwaitable<TResult> AsNoContext<TResult>(this ValueTask<TResult> task)
        {
            return task.ConfigureAwait(false);
        }
    }
}
