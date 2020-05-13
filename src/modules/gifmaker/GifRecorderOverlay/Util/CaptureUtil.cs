using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Storage;

namespace GifRecorderOverlay.Util
{
    /// <summary>
    /// Taken from https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/screen-capture
    /// </summary>
    class CaptureUtil
    {
        private GraphicsCaptureItem _item;
        private Direct3D11CaptureFramePool _framePool;
        private CanvasDevice _canvasDevice;
        private GraphicsCaptureSession _session;


        public async Task StartCaptureAsync()
        {
            // The GraphicsCapturePicker follows the same pattern the
            // file pickers do.
            var picker = new GraphicsCapturePicker();
            GraphicsCaptureItem item = await picker.PickSingleItemAsync();

            // The item may be null if the user dismissed the
            // control without making a selection or hit Cancel.
            if (item != null)
            {
                // We'll define this method later in the document.
                StartCaptureInternal(item);
            }
        }
        private void StartCaptureInternal(GraphicsCaptureItem item)
        {
            _item = item;

            _framePool = Direct3D11CaptureFramePool.Create(
                _canvasDevice, // D3D device
                DirectXPixelFormat.B8G8R8A8UIntNormalized, // Pixel format
                2, // Number of frames
                _item.Size); // Size of the buffers

            _session = _framePool.CreateCaptureSession(_item);

            _session.StartCapture();

            _framePool.FrameArrived += (s, a) =>
            {
                // The FrameArrived event fires for every frame on the thread that
                // created the Direct3D11CaptureFramePool. This means we don't have to
                // do a null-check here, as we know we're the only one  
                // dequeueing frames in our application.  

                // NOTE: Disposing the frame retires it and returns  
                // the buffer to the pool.
                using (var frame = _framePool.TryGetNextFrame())
                {
                    // We'll define this method later in the document.
                    ProcessFrame(frame);
                }
            };
        }

        private async void ProcessFrame(Direct3D11CaptureFrame frame)
        {
            CanvasBitmap canvasBitmap = CanvasBitmap.CreateFromDirect3D11Surface(
                _canvasDevice,
                frame.Surface);

            StorageFolder pictureFolder = KnownFolders.SavedPictures;
            StorageFile file = await pictureFolder.CreateFileAsync("test.png", CreationCollisionOption.ReplaceExisting);

            using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await canvasBitmap.SaveAsync(fileStream, CanvasBitmapFileFormat.Png, 1f);
            }
        }
    }
}
