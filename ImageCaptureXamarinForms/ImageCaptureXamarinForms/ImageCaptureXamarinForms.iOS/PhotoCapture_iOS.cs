using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Media;
using System.IO;

[assembly: Dependency(typeof(ImageCaptureXamarinForms.iOS.PhotoCapture_iOS))]
namespace ImageCaptureXamarinForms.iOS
{
    class PhotoCapture_iOS : IPhotoCapture
    {
        public async void Capture()
        {
            var picker = new MediaPicker();
            var mediaFile = await picker.TakePhotoAsync(new StoreCameraMediaOptions
            {
                DefaultCamera = CameraDevice.Rear
            });
            MessagingCenter.Send<IPhotoCapture, Stream>(this, "photoStreamCaptured", mediaFile.GetStream());
        }
    }
}