using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WindowsPhoneApp.Resources;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
using Model;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

namespace WindowsPhoneApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        CameraCaptureTask cameraCapture = null;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            cameraCapture = new CameraCaptureTask();
            cameraCapture.Completed += cameraCapture_Completed;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            cameraCapture.Show();
        }

        async void cameraCapture_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult != TaskResult.OK) return;

            var capturedPhoto = new BitmapImage();
            capturedPhoto.SetSource(e.ChosenPhoto);
            image.Source = capturedPhoto;

            string fileName = Guid.NewGuid().ToString() + ".jpg"; // Create a random file name
            var photo = new Photo { Caption = captionTextBox.Text, ContainerName = "photos", ResourceName = fileName };

            // Save a new item, this will populate the SAS from the MobileService
            await App.MobileService.GetTable<Photo>().InsertAsync(photo);

            // Upload the image data to the Blob Storage
            using (var client = new HttpClient())
            {
                using (var memoryStream = new MemoryStream())
                {
                    // Get a stream of the captured photo
                    var writableBitmap = new WriteableBitmap(capturedPhoto);
                    writableBitmap.SaveJpeg(memoryStream, capturedPhoto.PixelWidth, capturedPhoto.PixelHeight, 0, 100);

                    // Now upload on the SAS
                    var content = new StreamContent(memoryStream);
                    using (var uploadResponse = await client.PutAsync(new Uri(photo.SAS), content))
                    {
                        uploadResponse.EnsureSuccessStatusCode();
                        var remoteUrl = photo.SAS.Split('?').First();
                        remoteImage.Source = new BitmapImage(new Uri(remoteUrl)); // Load the remote image
                    }
                }
            }
        }
    }
}