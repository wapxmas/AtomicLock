using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AtomicLock
{
    public class ALock
    {
        private volatile int _lock = 0;

        public IDisposable Lock(int threadId)
        {
            var spinWait = new SpinWait();

            while (Interlocked.CompareExchange(ref _lock, threadId, 0) != threadId)
            {
                spinWait.SpinOnce();
            }

            return new ALockAcquired(this);
        }

        protected void Free()
        {
            Interlocked.Exchange(ref _lock, 0);
        }

        private class ALockAcquired : IDisposable
        {
            private readonly ALock _lockedObject;

            public ALockAcquired(ALock lockedObject)
            {
                _lockedObject = lockedObject;
            }

            public void Dispose()
            {
                _lockedObject.Free();
            }
        }
    }
}
