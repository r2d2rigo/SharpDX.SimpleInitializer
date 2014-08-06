// The MIT License (MIT)
// 
// Copyright (c) 2014 Rodrigo 'r2d2rigo' Diaz
// Portions of this code Copyright (c) 2010-2013 Alexandre Mutel
//
// See LICENSE for full license.

using System;
using Windows.Phone.Input.Interop;

namespace SharpDX.SimpleInitializer.Silverlight
{
    /// <summary>
    /// Implements a manipulation handler for the drawing surfaces.
    /// </summary>
    internal class DrawingSurfaceManipulationHandler : IDrawingSurfaceManipulationHandler
    {
        public event EventHandler<DrawingSurfaceManipulationHost> ManipulationHostChanged;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DrawingSurfaceManipulationHandler()
        {
        }

        /// <summary>
        /// Sets the manipulation host.
        /// </summary>
        /// <param name="manipulationHost">Manipulation host object.</param>
        public void SetManipulationHost(DrawingSurfaceManipulationHost manipulationHost)
        {
            if (this.ManipulationHostChanged != null)
            {
                this.ManipulationHostChanged(this, manipulationHost);
            }
        }
    }
}
