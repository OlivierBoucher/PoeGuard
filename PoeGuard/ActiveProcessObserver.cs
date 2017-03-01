using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoeGuard
{
    public delegate void ActiveProcessChangedHandler(object sender, int oldValue, int newValue);

    class ActiveProcessObserver
    {
        public event ActiveProcessChangedHandler ProcessChanged;

        private CancellationTokenSource cts;

        private bool observing = false;

        private int activeProcess;

        public void Observe()
        {
            if(!this.observing)
            {
                this.cts?.Dispose();
                this.cts = new CancellationTokenSource();

                CancellationToken token = cts.Token;

                token.Register(() =>
                {
                    this.observing = false;
                });

                this.observing = true;
                
                new Task(() =>
                {
                    while(true)
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        var activeProcess = ProcessManager.GetActiveWindowPID();

                        if (this.activeProcess != activeProcess)
                        {
                            var oldProcess = this.activeProcess;
                            this.activeProcess = activeProcess;
                            ProcessChanged(this, activeProcess, oldProcess);
                        }

                        Task.Delay(100, token);
                    }
                }, token).Start();
            }
        }

        public void Stop()
        {
            this.cts?.Cancel();
        }
    }
}
