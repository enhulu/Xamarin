using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin.Forms;
using Xamarin.Media;
using System.IO;


[assembly: Dependency(typeof(ImageCaptureXamarinForms.Droid.MainActivity))]
namespace ImageCaptureXamarinForms.Droid
{
	[Activity (Label = "ImageCaptureXamarinForms", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity, IPhotoCapture
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			global::Xamarin.Forms.Forms.Init (this, bundle);
			LoadApplication (new ImageCaptureXamarinForms.App ());
		}

        public void Capture()
        {
            var activity = Forms.Context as Activity;
            var picker = new MediaPicker(activity);
            if (!picker.IsCameraAvailable)
            {
                Console.WriteLine("No camera!");
            }
            else
            {
                var intent = picker.GetTakePhotoUI(new StoreCameraMediaOptions
                {
                    Name = String.Format("capture_{0}.jpg", Guid.NewGuid())
                });
                activity.StartActivityForResult(intent, 1);
            }            
        }

        protected override async void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent data)
        {
            if (resultCode == Result.Canceled)
                return;
            var mediaFile = await data.GetMediaFileExtraAsync(Forms.Context);           
            MessagingCenter.Send<IPhotoCapture, Stream>(this, "photoStreamCaptured", mediaFile.GetStream());
        }
	}
}

