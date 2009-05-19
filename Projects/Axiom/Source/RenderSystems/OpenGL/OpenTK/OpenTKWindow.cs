#region Namespace Declarations

using System;

using Axiom.Graphics;

using OpenTK;
using OpenTK.Graphics;

#endregion Namespace Declarations

namespace Axiom.RenderSystems.OpenGL
{
    /// <summary>
    /// Summary description for OpenTKWindow.
    /// </summary>
    public class OpenTKWindow : RenderWindow
    {
        public class AxiomOTKGameWindow : GameWindow
        {
            public AxiomOTKGameWindow(int width, int height, GraphicsMode gm, string title) : base(width, height, gm, title) { }
            public override void OnRenderFrame(RenderFrameEventArgs e)
            {
                if ( this.IsExiting == false )
                    Exit();
            }
        }

        #region Fields

        public AxiomOTKGameWindow OTKGameWindow;

        private bool destroyed;
        private bool fullScreen;
        private DisplayDevice displayDevice = null;
        private bool lastVSyncModeSet = false;

        #endregion Fields

        public OpenTKWindow()
        {
        }

        #region RenderWindow Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="colorDepth"></param>
        /// <param name="fullScreen"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="depthBuffer"></param>
        /// <param name="miscParams"></param>
        public override void Create(string name, int width, int height, int colorDepth, bool fullScreen, int left, int top, bool depthBuffer, params object[] miscParams)
        {
            this.name = name;
            this.width = width;
            this.height = height;
            this.colorDepth = colorDepth;
            this.fullScreen = fullScreen;
            displayDevice = DisplayDevice.Default;

            // create window
            OTKGameWindow = new AxiomOTKGameWindow( width, height, GraphicsMode.Default, name );

            // full screen?
            if (fullScreen)
            {
                displayDevice.ChangeResolution( displayDevice.SelectResolution( width, height, colorDepth, 60f ) );
                OTKGameWindow.WindowState = WindowState.Fullscreen;
            }
            else
            {
                OTKGameWindow.WindowState = WindowState.Normal;
                OTKGameWindow.WindowBorder = WindowBorder.Fixed;
            }

            // lets get active!
            isActive = true;

            GL.Clear( ClearBufferMask.ColorBufferBit );
            SwapBuffers( false );
        }

        public override void Dispose()
        {
            if ( destroyed )
                return;
            Destroy();
            base.Dispose();
        }


        public void Destroy()
        {
            if ( !destroyed )
            {
                if ( fullScreen )
                    displayDevice.RestoreResolution();
                OTKGameWindow.Context.Dispose();
                OTKGameWindow.Exit();
                OTKGameWindow = null;
                destroyed = true;
            }
        }

        public override void Reposition(int left, int right)
        {
        }

        public override void Resize(int width, int height)
        {
            if ( destroyed )
                return;

            OTKGameWindow.Width = width;
            OTKGameWindow.Height = height;
        }

        public void SaveToFile(string fileName)
        {

        }

        public override void Update()
        {
            if ( destroyed )
                return;

            base.Update();
            OTKGameWindow.ProcessEvents();
        }

        /// <summary>
        ///		Update the render window.
        /// </summary>
        /// <param name="waitForVSync"></param>
        public override void SwapBuffers(bool waitForVSync)
        {
            if ( destroyed || OTKGameWindow.WindowState == WindowState.Minimized )
                return;

            if ( lastVSyncModeSet != waitForVSync )
            {
                OTKGameWindow.VSync = waitForVSync ? VSyncMode.On : VSyncMode.Off;
                lastVSyncModeSet = waitForVSync;
            }

            OTKGameWindow.SwapBuffers();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        public override void Save(System.IO.Stream stream)
        {
        }



        #endregion RenderWindow Members
    }
}
