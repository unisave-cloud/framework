using System;
using TinyIoC;

namespace Unisave.Foundation
{
    public class ExternalInstanceLifetimeProvider : TinyIoCContainer.ITinyIoCObjectLifetimeProvider
    {
        private readonly object instance;
        
        public ExternalInstanceLifetimeProvider(object instance)
        {
            this.instance = instance;
        }
        
        public object GetObject()
        {
            return instance;
        }

        public void SetObject(object value)
        {
            throw new InvalidOperationException(
                "External instance registration should not be constructed."
            );
        }

        public void ReleaseObject()
        {
            // do nothing, since we don't own the instance
        }
    }
}