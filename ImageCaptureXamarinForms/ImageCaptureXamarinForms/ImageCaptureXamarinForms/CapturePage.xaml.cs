using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ImageCaptureXamarinForms
{
	public partial class CapturePage : ContentPage
	{
		public CapturePage ()
		{
            NavigationPage.SetHasNavigationBar(this, false);
			InitializeComponent ();
		}

        protected override void OnAppearing()
        {            
            base.OnAppearing();
            MessagingCenter.Subscribe<IPhotoCapture, Stream>(this, "photoStreamCaptured", (sender, args) =>
            {
                capturedImage.Source = ImageSource.FromStream(() => args);
            });
        }

        public void OnCaptureClicked(object sender, EventArgs args)
        {
            IPhotoCapture camera = DependencyService.Get<IPhotoCapture>();
            camera.Capture();
        }

        protected override void OnDisappearing()
        {
            MessagingCenter.Unsubscribe<IPhotoCapture, Stream>(this, "photoStreamCaptured");
            base.OnDisappearing();            
        }
	}
}
