﻿using System;
using System.Runtime.InteropServices;

namespace Entities
{
    public class ImageData: Disposable
    {
        private GCHandle _handle;

        public ImageData(ushort[] pixels)
        {
            SetPixels(pixels);
            IsSigned = false;
            BytesPerPixel = 2;
        }

        public ImageData(short[] pixels)
        {
            SetPixels(pixels);
            IsSigned = true;
            BytesPerPixel = 2;
        }

        public ImageData(byte[] data)
        {
            SetPixels(data);
            IsSigned = false;
            BytesPerPixel = 1;
        }

        public Vector3 PositionPatient { get; set; } = new Vector3();

        public Vector3 XAxisPatient { get; set; } = new Vector3();
        public Vector3 YAxisPatient { get; set; } = new Vector3();

        public double Intercept { get; set; }
        public double Slope { get; set; } = 1;

        public bool DefaultWindowingAvailable { get; set; }
        public double WindowWidth { get; set; }
        public double WindowLevel { get; set; }

        public double MinValue { get; set; }
        public double MaxValue { get; set; }

        public double MinRescaledValue
        {
            get
            {
                return Slope * MinValue + Intercept;
            }
        }

        public double MaxRescaledValue
        {
            get
            {
                return Slope * MaxValue + Intercept;
            }
        }

        public int BytesPerPixel { get; }

        public Vector3 CenterPatient
        {
            get
            {                
                var widthInMilimeter = Width * PixelSpacing.X;
                var heightInMilimeter = Height * PixelSpacing.Y;
                return PositionPatient + 0.5 * (XAxisPatient * widthInMilimeter + YAxisPatient * heightInMilimeter);
            }
        }

        public IntPtr Pixels { get; private set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Spacing2 PixelSpacing { get; set; } = new Spacing2 { X = 1, Y = 1 };

        public Vector3 SizePatient
        {
            get
            {
                return new Vector3(Width * PixelSpacing.X, Height * PixelSpacing.Y, 0);
            }
        }

        public bool IsSigned { get; private set; }

        private void SetPixels(object pixels)
        {
            _handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            Pixels = _handle.AddrOfPinnedObject();
        }

        protected override void OnDispose()
        {
            _handle.Free();
        }
    }
}
