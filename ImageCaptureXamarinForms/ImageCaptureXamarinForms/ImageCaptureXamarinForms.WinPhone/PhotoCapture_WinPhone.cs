using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(ImageCaptureXamarinForms.WinPhone.PhotoCapture_WinPhone))]
namespace ImageCaptureXamarinForms.WinPhone
{
    class PhotoCapture_WinPhone : IPhotoCapture
    {
        public PhotoCapture_WinPhone() { }

        public void Capture()
        {
            var cameraCaptureTask = new CameraCaptureTask();
            cameraCaptureTask.Completed += new EventHandler<PhotoResult>(cameraCaptureTask_Completed);
            cameraCaptureTask.Show();
        }

        private void cameraCaptureTask_Completed(object sender, PhotoResult e)
        {
            MessagingCenter.Send<IPhotoCapture, Stream>(this, "photoStreamCaptured", e.ChosenPhoto);
        }
    }
}
