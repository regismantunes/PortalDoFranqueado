using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PortalDoFranqueadoGUI.Repository
{
    public static class Worker
    {
        private static readonly List<Task> _tasks = new();

        public static void StartWork(Task task)
        {
            if (task.Status == TaskStatus.Created)
                task.Start();
            
            _tasks.Add(task);
        }
        
        public static void StartWork(DispatcherOperation dispatcher)
        {
            _tasks.Add(dispatcher.Task);
        }

        public static void StartWork(Action action)
        {
            var task = Task.Factory.StartNew(action);
            _tasks.Add(task);
        }

        public static int GetActiveWorks()
            => _tasks.Count(t => !t.IsCompleted);
    }
}
