﻿using Basler.Pylon;
using Emgu.CV;
using Emgu.CV.Structure;
using EventArgsLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Windows.Threading;

namespace CameraAdapter
{
    public class BaslerCameraAdapter
    {

        private Camera camera = null;
        //private PixelDataConverter converter = new PixelDataConverter();

        PixelDataConverter pxConvert = new PixelDataConverter();

        bool GrabOver = false;

        public void CameraInit()
        {
            // Ask the camera finder for a list of camera devices.
            List<ICameraInfo> allCameras = CameraFinder.Enumerate();

            if(allCameras.Count>0)
            {
                //There is at least 1 camera
                foreach(ICameraInfo camInf in allCameras)
                {
                    if(camInf[CameraInfoKey.SerialNumber]== "40032798")
                    {

                        DeviceAccessibilityInfo inf= CameraFinder.GetDeviceAccessibilityInfo(camInf);
                        if (inf.HasFlag(DeviceAccessibilityInfo.Ok) || inf.HasFlag(DeviceAccessibilityInfo.Opened))
                        {
                            camera = new Camera("40032798");
                            if(inf.HasFlag(DeviceAccessibilityInfo.Opened))
                            {
                                Process[] tab=Process.GetProcessesByName("conhost");

                                //foreach(Process pro in tab)
                                //{
                                //    //pro.Kill();
                                //}
                                camera.Close();
                            }
                        }
                        else
                        {
                            if (inf.HasFlag(DeviceAccessibilityInfo.OpenedExclusively))
                            {
                                Console.WriteLine("Camera deja utilisé par un autre processus");
                            }
                        }
                    }
                }
            }            

            if (camera != null)
            {
                // Print the model name of the camera. 
                Console.WriteLine("Using camera {0}.", camera.CameraInfo[CameraInfoKey.ModelName]);
                camera.CameraOpened += Configuration.AcquireContinuous;
                camera.ConnectionLost += Camera_ConnectionLost;
                camera.CameraClosed += Camera_CameraClosed;
                camera.StreamGrabber.GrabStarted += StreamGrabber_GrabStarted;
                camera.StreamGrabber.ImageGrabbed += StreamGrabber_ImageGrabbed;
                camera.StreamGrabber.GrabStopped += StreamGrabber_GrabStopped;

                camera.Open();                    

                camera.Parameters[PLCamera.GevSCPSPacketSize].SetValue(8192);       //Réglage du packet Size à 8192
                camera.Parameters[PLCamera.GevSCPD].SetValue(10000);                //Réglage de l'inter packet delay à 10000
                camera.Parameters[PLCamera.ExposureTimeAbs].SetValue(25000);        //Réglage du temps d'exposition à 40Hz - 25.000 us
                camera.Parameters[PLCamera.AcquisitionFrameRateAbs].SetValue(40);   //Réglage du framerate en fps
                camera.Parameters[PLCamera.LightSourceSelector].SetValue(PLCamera.LightSourceSelector.Daylight6500K);

            }
                //SetValue(PLCamera.AcquisitionMode.Continuous);
            KeepShot();
        }

        private void Camera_CameraClosed(object sender, EventArgs e)
        {
            //pas utile pour nous
        }

        private void StreamGrabber_GrabStarted(object sender, EventArgs e)
        {
            GrabOver = true;
        }
        private void StreamGrabber_ImageGrabbed(object sender, ImageGrabbedEventArgs e)
        {
            IGrabResult grabResult = e.GrabResult;

            if (grabResult.IsValid)
            {
                if (GrabOver)
                {
                    var handler = BitmapImageEvent;
                    if (handler != null)
                    {
                        Bitmap bitmap = GrabResult2Bmp(grabResult);
                        OnBitmapImageReceived(bitmap);
                    }
                }
            }
        }

        private void StreamGrabber_GrabStopped(object sender, GrabStopEventArgs e)
        {
            GrabOver = false;
            // If the grabbed stop due to an error, display the error message.
            if (e.Reason != GrabStopReason.UserRequest)
            {
                MessageBox.Show("A grab error occured:\n" + e.ErrorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Camera_ConnectionLost(object sender, EventArgs e)
        {
            camera.StreamGrabber.Stop();
            DestroyCamera();
        }

        public void OneShot()
        {
            if (camera != null)
            {
                camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.SingleFrame);
                camera.StreamGrabber.Start(1, GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
            }
        }

        public void KeepShot()
        {
            if (camera != null)
            {
                camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
                camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
            }
        }

        public void Stop()
        {
            if (camera != null)
            {
                camera.StreamGrabber.Stop();
            }
        }

        Bitmap GrabResult2Bmp(IGrabResult grabResult)
        {
            Bitmap b = new Bitmap(grabResult.Width, grabResult.Height, PixelFormat.Format32bppRgb);
            BitmapData bmpData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, b.PixelFormat);
            pxConvert.OutputPixelFormat = PixelType.BGRA8packed;
            IntPtr bmpIntpr = bmpData.Scan0;
            pxConvert.Convert(bmpIntpr, bmpData.Stride * b.Height, grabResult);
            b.UnlockBits(bmpData);
            return b;
        }

        public void DestroyCamera()
        {
            if (camera != null)
            {
                camera.Close();
                camera.Dispose();
                camera = null;
            }
        }

        //Output events
        //public delegate void CameraImageEventHandler(object sender, CameraImageArgs e);
        //public event EventHandler<CameraImageArgs> CameraImageEvent;

        //public virtual void OnCameraImageReceived(Bitmap image)
        //{
        //    var handler = CameraImageEvent;
        //    if (handler != null)
        //    {
        //        handler(this, new CameraImageArgs { ImageBmp = image });
        //    }
        //}
        public event EventHandler<BitmapImageArgs> BitmapImageEvent;
        public virtual void OnBitmapImageReceived(Bitmap bmp)
        {
            var handler = BitmapImageEvent;
            if (handler != null)
            {
                handler(this, new BitmapImageArgs { Bitmap = bmp, Descriptor = "ImageFromCamera" });
            }
        }

        //public event EventHandler<OpenCvMatImageArgs> OpenCvMatImageEvent;
        //public virtual void OnOpenCvMatImageReceived(Mat mat)
        //{
        //    var handler = OpenCvMatImageEvent;
        //    if (handler != null)
        //    {
        //        handler(this, new OpenCvMatImageArgs { Mat = mat , Descriptor= "ImageFromCamera"});
        //        }
        //}
    }
}
