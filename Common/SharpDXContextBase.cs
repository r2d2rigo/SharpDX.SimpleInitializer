// The MIT License (MIT)
// 
// Copyright (c) 2014 Rodrigo 'r2d2rigo' Diaz
// Portions of this code Copyright (c) 2010-2013 Alexandre Mutel
//
// See LICENSE for full license.

using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using Windows.Foundation;

namespace SharpDX.SimpleInitializer
{
    /// <summary>
    /// Base class for platform-specific SharpDXContext implementations.
    /// </summary>
    public abstract class SharpDXContextBase : IDisposable
    {
        private Device d3DDevice;
        private DeviceContext d3DContext;
        private RenderTargetView backBufferView;
        private DepthStencilView depthStencilView;
        private Texture2D backBuffer;
        private Size backBufferSize;

        /// <summary>
        /// Raised when the Direct3D device is recreated.
        /// </summary>
        public event EventHandler<DeviceResetEventArgs> DeviceReset;

        /// <summary>
        /// Raised when the control requests a new frame.
        /// </summary>
        public event EventHandler Render;

        /// <summary>
        /// The Direct3D device.
        /// </summary>
        public Device D3DDevice
        {
            get
            {
                this.ThrowIfDisposed();

                return this.d3DDevice;
            }
            private set
            {
                this.d3DDevice = value;
            }
        }

        /// <summary>
        /// The Direct3D device context.
        /// </summary>
        public DeviceContext D3DContext
        {
            get
            {
                this.ThrowIfDisposed();

                return this.d3DContext;
            }
            private set
            {
                this.d3DContext = value;
            }
        }

        /// <summary>
        /// The render target view that holds the reference to the current back buffer.
        /// </summary>
        public RenderTargetView BackBufferView
        {
            get
            {
                this.ThrowIfDisposed();

                return this.backBufferView;
            }
            internal set
            {
                this.backBufferView = value;
            }
        }

        /// <summary>
        /// The depth stencil target view that holds the reference to the current depth stencil buffer.
        /// </summary>
        public DepthStencilView DepthStencilView
        {
            get
            {
                this.ThrowIfDisposed();

                return this.depthStencilView;
            }
            internal set
            {
                this.depthStencilView = value;
            }
        }

        /// <summary>
        /// The texture of the current back buffer.
        /// </summary>
        public Texture2D BackBuffer
        {
            get
            {
                this.ThrowIfDisposed();

                return this.backBuffer;
            }
            internal set
            {
                this.backBuffer = value;
            }
        }

        /// <summary>
        /// The size in pixels of the current back buffer.
        /// </summary>
        public Size BackBufferSize
        {
            get
            {
                this.ThrowIfDisposed();

                return this.backBufferSize;
            }
            internal set
            {
                this.backBufferSize = value;
            }
        }

        /// <summary>
        /// Gets or sets if the instance has been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets if the instance has been bound to a control.
        /// </summary>
        public bool IsBound
        {
            get;
            protected set;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected SharpDXContextBase()
        {
            this.IsDisposed = false;
            this.IsBound = false;
        }

        /// <summary>
        /// Creates resources that depend on the current device.
        /// </summary>
        protected virtual void CreateDeviceDependentResources()
        {
        }

        /// <summary>
        /// Releases resources that depend on the current device.
        /// </summary>
        protected void ReleaseDeviceDependentResources()
        {
            Utilities.Dispose(ref this.d3DDevice);
            Utilities.Dispose(ref this.d3DContext);
        }

        /// <summary>
        /// Releases resources that depend on the current back buffer size.
        /// </summary>
        protected void ReleaseSizeDependentResources()
        {
            Utilities.Dispose(ref this.backBuffer);
            Utilities.Dispose(ref this.backBufferView);
            Utilities.Dispose(ref this.depthStencilView);
        }

        /// <summary>
        /// Recreates the back buffer texture and view based on the new size.
        /// </summary>
        /// <param name="backBufferSize">New back buffer size.</param>
        internal virtual void RecreateBackBuffer(Size backBufferSize)
        {
            Utilities.Dispose(ref this.backBuffer);
            Utilities.Dispose(ref this.backBufferView);
        }

        /// <summary>
        /// Recreates the depth stencil buffer view based on the new size.
        /// </summary>
        /// <param name="backBufferSize">New depth stencil size.</param>
        internal virtual void RecreateDepthStencil(Size depthStencilSize)
        {
            Utilities.Dispose(ref this.depthStencilView);
        }

        /// <summary>
        /// Raises the DeviceReset event.
        /// </summary>
        /// <param name="newDevice">New Direct3D11 device.</param>
        /// <param name="newContext">New Direct3D11 device context.</param>
        internal void OnDeviceReset(Device newDevice, DeviceContext newContext)
        {
            this.D3DDevice = newDevice;
            this.D3DContext = newContext;

            if (this.DeviceReset != null)
            {
                this.DeviceReset(this, new DeviceResetEventArgs(this.D3DDevice, this.D3DContext));
            }
        }

        /// <summary>
        /// Raises the Render event.
        /// </summary>
        internal void OnRender()
        {
            if (this.Render != null)
            {
                this.Render(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Releases all resources used by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes this object's resources.
        /// </summary>
        /// <param name="disposing">true if releasing managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ReleaseDeviceDependentResources();
                this.ReleaseSizeDependentResources();
            }

            this.IsBound = false;
            this.IsDisposed = true;
        }

        /// <summary>
        /// Throws an exception if the object has been disposed.
        /// </summary>
        protected void ThrowIfDisposed()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException("The object has already been disposed.");
            }
        }

        /// <summary>
        /// Throws an exception if the object has already been bound.
        /// </summary>
        protected void ThrowIfBound()
        {
            if (this.IsBound)
            {
                throw new InvalidOperationException("This instance has already been bound to a control.");
            }
        }
    }
}