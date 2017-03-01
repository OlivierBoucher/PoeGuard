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

        private int activeProcess = -1;

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
                
                new Thread(() =>
                {
                    while(true)
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        var activeProcess = ProcessManager.GetActiveWindowPID();
                        if (activeProcess > 0 && this.activeProcess != activeProcess)
                        {
                            var oldProcess = this.activeProcess;
                            this.activeProcess = activeProcess;
                            ProcessChanged(this, oldProcess, activeProcess);
                        }

                        Thread.Sleep(500);
                    }
                }).Start();
            }
        }

        public void Stop()
        {
            this.cts?.Cancel();
        }
    }
}
