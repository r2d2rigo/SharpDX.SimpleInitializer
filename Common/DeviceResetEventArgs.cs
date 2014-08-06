// The MIT License (MIT)
// 
// Copyright (c) 2014 Rodrigo 'r2d2rigo' Diaz
// Portions of this code Copyright (c) 2010-2013 Alexandre Mutel
//
// See LICENSE for full license.

using SharpDX.Direct3D11;
using System;

namespace SharpDX.SimpleInitializer
{
    /// <summary>
    /// Provides data for the event raised when a Direct3D device is recreated.
    /// </summary>
    public class DeviceResetEventArgs : EventArgs
    {
        /// <summary>
        /// New Direct3D device.
        /// </summary>
        public Device Device { get; private set; }

        /// <summary>
        /// New Direct3D device context.
        /// </summary>
        public DeviceContext DeviceContext { get; private set; }

        internal DeviceResetEventArgs(Device device, DeviceContext context)
        {
            this.Device = device;
            this.DeviceContext = context;
        }
    }
}
