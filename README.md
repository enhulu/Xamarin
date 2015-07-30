
Recently I implemented a Xamarin.Forms based application as the cross-platform client solution to demonstrate our image processing model to one of our key partners. This post is to share some lessons learned from implementing the image capture functionality using Xamarin Forms. 

## Objectives ##
We wanted to target all major platforms for the client and maximize the code reuse among platforms. Xamarin platform provides a solution to target iOS, Android, Windows and Mac with a single, shared C# codebase. Xamarin.Forms extends the code sharing further to the UI level. For image capture, we want to leverage with system camera experience instead of implementing capturing experiences for all platforms. Xamarin Inc. exposes some functionality through a component called "Xamarin.Mobile". Due to the early phase of Xamarin.Mobile and platform variety, image capture part of the API is not platform independent. Platform specific code is expected. The primary target is to deal with platform specific features while maximizing the code reuse.       

## Solutions ##
In order to keep platform specific code out of shared project, I utilized DependencyService provided by Xamarin.Forms. It allows developers to define and use a common interface in the shared project and the implementation of the interface is registered by platform specific projects via an assembly attribute. For image capture, a simple method is defined in an interface as below:
```language-csharp
    public interface IPhotoCapture
    {
        void Capture();
    }
```
When trying to capture an image, the following code is used to invoke the platform specific implementation.
```language-csharp
            IPhotoCapture camera = DependencyService.Get<IPhotoCapture>();
            camera.Capture();
```
Once the image is taken by the system camera app on a platform, then the captured image needs to be passed back to the application. Different platforms pass the images back to the application differently. In order to unify the behavior for the UI page based on Xamarin Forms, I took MessagingCenter into use and let it pass the file stream of the captured image to UI page. Therefore, I need to subscribe the message when the page is loaded and unsubscribe the message when the page is unloaded.
```language-csharp
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
```
Each platform specific project needs to send the stream to the UI page through MessagingCenter once they receive the stream of a captured image.

```language-csharp
MessagingCenter.Send<IPhotoCapture, Stream>(this, "photoStreamCaptured", stream);            
```

### iOS ###
On iOS side the steps are rather straightforward. First, Xamarin.Mobile component needs to be added to the iOS project. Then
a class needs to implement IPhotoCapture class and set the assembly attribute. The captured image will be returned to the application as the output of TakePhotoAsync function. Once the async function returns, then the stream can be passed to UI page through MessagingCenter.
```language-csharp
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
```
### Android ###
The android part of the story is a bit more complex as Android needs application context to launch the system camera with intent. Thus, MainActivity is the class to implement IPhotoCapture with the assembly attribute. After adding the Xamarin.Mobile, MediaPicker can be used to form the intent to start the system camera app. WRITE_EXTERNAL_STORAGE permission is need in this case.

```language-csharp
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
```
Once the image is taken by the system camera app, then the captured image is passed back to the application in  OnActivityResult function where the stream of the captured image can be sent to UI page for rendering.

```language-csharp
protected override async void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent data)
        {
            if (resultCode == Result.Canceled)
                return;
            var mediaFile = await data.GetMediaFileExtraAsync(Forms.Context);

            System.Diagnostics.Debug.WriteLine(mediaFile.Path);
            MessagingCenter.Send<IPhotoCapture,Stream>(this, "photoStreamCaptured",mediaFile.GetStream());
        }
```
### Windows Phone ###
As there is no official support to automatically add Xamarin components to Windows Phone (WP) project on VS2013, Xamarin.Mobile WP library has to be added manually. Considering the effort for later manual update and limited benefit it provides, I decided to use WP specific way to launch system camera for image capture. PhotoCapture_WinPhone is created to implement the Capture function of IPhotoCapture. Once the capture is done from system camera app, cameraCaptureTask_Completed function gets the captured image and the image file stream can be sent to UI page for rendering. 
```language-csharp
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
```
## Summary ##
This post has presented you how to create a cross-platform image capture solution by using Xamarin Forms. I have created a [Github project](https://github.com/enhulu/Xamarin.git) for anyone interested in the solution.
