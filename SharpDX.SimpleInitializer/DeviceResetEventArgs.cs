using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDX.SimpleInitializer
{
    public class DeviceResetEventArgs : EventArgs
    {
        public Device Device { get; private set; }
        public DeviceContext DeviceContext { get; private set; }

        public DeviceResetEventArgs(Device device, DeviceContext context)
        {
            this.Device = device;
            this.DeviceContext = context;
        }
    }
}
