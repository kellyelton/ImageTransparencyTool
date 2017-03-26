using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageTransparencyTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public double Percent {
            get { return (double)GetValue(PercentProperty); }
            set { SetValue(PercentProperty, value); }
        }

        public static readonly DependencyProperty PercentProperty =
            DependencyProperty.Register(nameof(Percent), typeof(double), typeof(MainWindow), new PropertyMetadata(100d, OnPerecentPropertyChanged));

        private static void OnPerecentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var win = d as MainWindow;
            win.ReprocessImage();
        }

        public System.Windows.Media.Color Color {
            get { return (System.Windows.Media.Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register(nameof(Color), typeof(System.Windows.Media.Color), typeof(MainWindow), new PropertyMetadata(System.Windows.Media.Colors.Black, OnColorPropertyChanged));

        private static void OnColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var win = d as MainWindow;
            win.ReprocessImage();
        }

        public BitmapImage CurrentImage {
            get { return (BitmapImage)GetValue(CurrentImageProperty); }
            set { SetValue(CurrentImageProperty, value); }
        }

        public static readonly DependencyProperty CurrentImageProperty =
            DependencyProperty.Register(nameof(CurrentImage), typeof(BitmapImage), typeof(MainWindow), new PropertyMetadata(null, OnCurrentImagePropertyChanged));

        private Bitmap _currentImage;
        private static void OnCurrentImagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var win = d as MainWindow;
            win.ReprocessImage();
        }

        public BitmapImage FilteredImage {
            get { return (BitmapImage)GetValue(FilteredImageProperty); }
            set { SetValue(FilteredImageProperty, value); }
        }

        public static readonly DependencyProperty FilteredImageProperty =
            DependencyProperty.Register(nameof(FilteredImage), typeof(BitmapImage), typeof(MainWindow), new PropertyMetadata(null));


        public MainWindow() {
            InitializeComponent();
        }

        private string _currentImageFilePath;

        private void Window_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                _currentImageFilePath = files[0];
                _currentImage = LoadImage(_currentImageFilePath);
                CurrentImage = _currentImage.ToBitmapImage();

                ReprocessImage();
            }
        }

        private void ReprocessImage() {
            if (CurrentImage == null) return;

            using (var bmp = (Bitmap)_currentImage.Clone()) {
                bmp.ChromoKey(System.Drawing.Color.FromArgb(Color.A, Color.R, Color.G, Color.B), (float)Percent);
                FilteredImage = bmp.ToBitmapImage();
            }
        }

        private static Bitmap LoadImage(string path) {
            Bitmap original = null;
            try {
                original = (Bitmap)Bitmap.FromFile(path);
                if (original.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb) {
                    Bitmap ret = null;
                    try { } finally {
                        ret = original;
                        original = null;
                    }
                    return ret;
                }
                return ConvertTo32bppArgb(original);
            } finally {
                original?.Dispose();
            }
        }

        private static Bitmap ConvertTo32bppArgb(Bitmap image) {
            Bitmap ret = null;
            try {
                ret = new Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(ret)) {
                    g.DrawImage(image, new System.Drawing.Rectangle(0, 0, image.Width, image.Height));
                }
                return ret;
            } catch {
                ret?.Dispose();
                throw;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            if (_currentImage == null) return;
            if (_currentImageFilePath == null) return;
            var savePath = _currentImageFilePath;

            using (var bmp = (Bitmap)_currentImage.Clone()) {
                bmp.ChromoKey(System.Drawing.Color.FromArgb(Color.A, Color.R, Color.G, Color.B), (float)Percent);
                bmp.Save(savePath);
            }

            _currentImage = LoadImage(_currentImageFilePath);
            CurrentImage = _currentImage.ToBitmapImage();

            ReprocessImage();
        }
    }
}
