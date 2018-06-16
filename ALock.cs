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
        private volatile object _lock = null;

        public IDisposable Lock()
        {
            var dummyObject = new object();

            var spinWait = new SpinWait();

            while (Interlocked.CompareExchange(ref _lock, dummyObject, null) != dummyObject)
            {
                spinWait.SpinOnce();
            }

            return new ALockAcquired(this);
        }

        protected void Free()
        {
            Interlocked.Exchange(ref _lock, null);
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
