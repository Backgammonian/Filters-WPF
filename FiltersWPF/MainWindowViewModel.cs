using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;
using System.IO;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;

namespace FiltersWPF
{
    public class MainWindowViewModel : ObservableObject
    {
        private string _originalImagePath;
        private bool _isOriginalImageFullSize;
        private bool _isResultingImageFullSize;
        private BitmapImage _originalImage;
        private BitmapImage _resultingImage;
        private PreparedFilters _selectedPreparedFilter;
        private double[,] _matrix;
        private int _matrixDimension;
        private bool _isImageSelected;
        private bool _isResultingImageReady;
        private DirectBitmap _originalImageBitmap;
        private DirectBitmap _resultingImageBitmap;
        private double _factor;
        private double _bias;
        private int _selectedTab;
        private readonly BitmapImage[] _originalImageHistograms;
        private readonly BitmapImage[] _resultingImageHistograms;
        private const int _histogramPadding = 5;
        private const int _histogramBarScale = 3;
        private const int _histogramHeight = 200;
        private readonly Color _brightnessHistogramColor;
        private readonly Color _redHistogramColor;
        private readonly Color _greenHistogramColor;
        private readonly Color _blueHistogramColor;
        private readonly Color _transparencyHistogramColor;
        private readonly Stopwatch _sw;

        public MainWindowViewModel()
        {
            SelectImageCommand = new RelayCommand(SelectImage);
            ApplyFilterCommand = new RelayCommand(ApplyFilter);
            SaveResultingImageCommand = new RelayCommand(SaveResultingImage);

            OriginalImagePath = string.Empty;

            IsOriginalImageFullSize = false;
            IsResultingImageFullSize = false;

            MatrixDimension = 3;

            SelectedPreparedFilter = PreparedFilters.Blur3x3;
            PreparedFiltersList = new ObservableCollection<PreparedFilters>
            {
                PreparedFilters.Blur3x3,
                PreparedFilters.Blur5x5,
                PreparedFilters.Emboss,
                PreparedFilters.Emboss45Degrees,
                PreparedFilters.GaussianBlur3x3,
                PreparedFilters.GaussianBlur3x3_2,
                PreparedFilters.GaussianBlur5x5,
                PreparedFilters.HighPass3x3,
                PreparedFilters.IntenseEmboss,
                PreparedFilters.IntenseSharpen,
                PreparedFilters.Sharpen5x5,
                PreparedFilters.Soften,
                PreparedFilters.MotionBlur,
                PreparedFilters.HorizontalEdges,
                PreparedFilters.VerticalEdges,
                PreparedFilters.Edges45Degrees,
                PreparedFilters.EdgesAllDirections,
                PreparedFilters.Sharpen,
            };

            IsImageSelected = false;
            IsResultingImageReady = false;

            SelectedTabIndex = 0;

            _originalImageHistograms = new BitmapImage[5];
            _resultingImageHistograms = new BitmapImage[5];

            _brightnessHistogramColor = Color.FromArgb(20, 20, 20);
            _redHistogramColor = Color.FromArgb(240, 10, 10);
            _greenHistogramColor = Color.FromArgb(10, 240, 10);
            _blueHistogramColor = Color.FromArgb(10, 10, 240);
            _transparencyHistogramColor = Color.FromArgb(90, 90, 90);

            _sw = new Stopwatch();
        }

        public ICommand SelectImageCommand { get; }
        public ICommand ApplyFilterCommand { get; }
        public ICommand SaveResultingImageCommand { get; }
        public ObservableCollection<PreparedFilters> PreparedFiltersList { get; }
        public bool IsOriginalImageNotFullSize => !IsOriginalImageFullSize;
        public bool IsResultingImageNotFullSize => !IsResultingImageFullSize;

        public string OriginalImagePath
        {
            get => _originalImagePath;
            private set => SetProperty(ref _originalImagePath, value);
        }

        public bool IsOriginalImageFullSize
        {
            get => _isOriginalImageFullSize;
            set
            {
                SetProperty(ref _isOriginalImageFullSize, value);
                OnPropertyChanged(nameof(IsOriginalImageNotFullSize));
            }
        }

        public bool IsResultingImageFullSize
        {
            get => _isResultingImageFullSize;
            set
            {
                SetProperty(ref _isResultingImageFullSize, value);
                OnPropertyChanged(nameof(IsResultingImageNotFullSize));
            }
        }

        public bool IsImageSelected
        {
            get => _isImageSelected;
            private set => SetProperty(ref _isImageSelected, value);
        }

        public bool IsResultingImageReady
        {
            get => _isResultingImageReady;
            private set => SetProperty(ref _isResultingImageReady, value);
        }

        public BitmapImage OriginalImage
        {
            get => _originalImage;
            private set => SetProperty(ref _originalImage, value);
        }

        public BitmapImage OriginalImageBrightnessHistogram
        {
            get => _originalImageHistograms[0];
            private set => SetProperty(ref _originalImageHistograms[0], value);
        }

        public BitmapImage OriginalImageRedHistogram
        {
            get => _originalImageHistograms[1];
            private set => SetProperty(ref _originalImageHistograms[1], value);
        }

        public BitmapImage OriginalImageGreenHistogram
        {
            get => _originalImageHistograms[2];
            private set => SetProperty(ref _originalImageHistograms[2], value);
        }

        public BitmapImage OriginalImageBlueHistogram
        {
            get => _originalImageHistograms[3];
            private set => SetProperty(ref _originalImageHistograms[3], value);
        }

        public BitmapImage OriginalImageTransparencyHistogram
        {
            get => _originalImageHistograms[4];
            private set => SetProperty(ref _originalImageHistograms[4], value);
        }

        public BitmapImage ResultingImage
        {
            get => _resultingImage;
            private set => SetProperty(ref _resultingImage, value);
        }

        public BitmapImage ResultingImageBrightnessHistogram
        {
            get => _resultingImageHistograms[0];
            private set => SetProperty(ref _resultingImageHistograms[0], value);
        }

        public BitmapImage ResultingImageRedHistogram
        {
            get => _resultingImageHistograms[1];
            private set => SetProperty(ref _resultingImageHistograms[1], value);
        }

        public BitmapImage ResultingImageGreenHistogram
        {
            get => _resultingImageHistograms[2];
            private set => SetProperty(ref _resultingImageHistograms[2], value);
        }

        public BitmapImage ResultingImageBlueHistogram
        {
            get => _resultingImageHistograms[3];
            private set => SetProperty(ref _resultingImageHistograms[3], value);
        }

        public BitmapImage ResultingImageTransparencyHistogram
        {
            get => _resultingImageHistograms[4];
            private set => SetProperty(ref _resultingImageHistograms[4], value);
        }

        public double[,] Matrix
        {
            get => _matrix;
            private set => SetProperty(ref _matrix, value);
        }

        public int MatrixDimension
        {
            get => _matrixDimension;
            set
            {
                if (value >= 1 && value % 2 == 1)
                {
                    SetProperty(ref _matrixDimension, value);

                    Debug.WriteLine("MatrixDimension: " + MatrixDimension);

                    UpdateMatrix();
                }
            }
        }

        public string Factor
        {
            get => _factor.ToString();
            set
            {
                if (double.TryParse(value, out double result))
                {
                    _factor = result;
                    OnPropertyChanged(nameof(Factor));
                }
            }
        }

        public string Bias
        {
            get => _bias.ToString();
            set
            {
                if (double.TryParse(value, out double result))
                {
                    _bias = result;
                    OnPropertyChanged(nameof(Bias));
                }
            }
        }

        public int SelectedTabIndex
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }

        public PreparedFilters SelectedPreparedFilter
        {
            get => _selectedPreparedFilter;
            set
            {
                SetProperty(ref _selectedPreparedFilter, value);

                Debug.WriteLine("SelectedPreparedFilter: " + SelectedPreparedFilter);

                switch (SelectedPreparedFilter)
                {
                    case PreparedFilters.HighPass3x3:
                        MatrixDimension = 3;

                        Factor = 1.0 + "";
                        Bias = 0.0 + "";

                        Matrix[0, 0] = -1;
                        Matrix[0, 1] = -2;
                        Matrix[0, 2] = -1;

                        Matrix[1, 0] = -2;
                        Matrix[1, 1] = 12;
                        Matrix[1, 2] = -2;

                        Matrix[2, 0] = -1;
                        Matrix[2, 1] = -2;
                        Matrix[2, 2] = -1;
                        break;

                    case PreparedFilters.Blur3x3:
                        MatrixDimension = 3;

                        _factor = 1.0;
                        _bias = 0.0;

                        Matrix[0, 0] = 0;
                        Matrix[0, 1] = 0.2;
                        Matrix[0, 2] = 0;

                        Matrix[1, 0] = 0.2;
                        Matrix[1, 1] = 0.2;
                        Matrix[1, 2] = 0.2;

                        Matrix[2, 0] = 0;
                        Matrix[2, 1] = 0.2;
                        Matrix[2, 2] = 0;
                        break;

                    case PreparedFilters.Blur5x5:
                        MatrixDimension = 5;

                        Factor = (1.0 / 13.0) + "";
                        Bias = 0.0 + "";

                        Matrix[0, 0] = 0;
                        Matrix[0, 1] = 0;
                        Matrix[0, 2] = 1;
                        Matrix[0, 3] = 0;
                        Matrix[0, 4] = 0;

                        Matrix[1, 0] = 0;
                        Matrix[1, 1] = 1;
                        Matrix[1, 2] = 1;
                        Matrix[1, 3] = 1;
                        Matrix[1, 4] = 0;

                        Matrix[2, 0] = 1;
                        Matrix[2, 1] = 1;
                        Matrix[2, 2] = 1;
                        Matrix[2, 3] = 1;
                        Matrix[2, 4] = 1;

                        Matrix[3, 0] = 0;
                        Matrix[3, 1] = 1;
                        Matrix[3, 2] = 1;
                        Matrix[3, 3] = 1;
                        Matrix[3, 4] = 0;

                        Matrix[4, 0] = 0;
                        Matrix[4, 1] = 0;
                        Matrix[4, 2] = 1;
                        Matrix[4, 3] = 0;
                        Matrix[4, 4] = 0;
                        break;

                    case PreparedFilters.GaussianBlur3x3:
                        MatrixDimension = 3;

                        Factor = (1.0 / 16.0) + "";
                        Bias = 0.0 + "";

                        Matrix[0, 0] = 1;
                        Matrix[0, 1] = 2;
                        Matrix[0, 2] = 1;

                        Matrix[1, 0] = 2;
                        Matrix[1, 1] = 4;
                        Matrix[1, 2] = 2;

                        Matrix[2, 0] = 1;
                        Matrix[2, 1] = 2;
                        Matrix[2, 2] = 1;
                        break;

                    case PreparedFilters.GaussianBlur3x3_2:
                        MatrixDimension = 3;

                        Factor = 1.0 + "";
                        Bias = 0.0 + "";

                        Matrix[0, 0] = 0.077847;
                        Matrix[0, 1] = 0.123317;
                        Matrix[0, 2] = 0.077847;

                        Matrix[1, 0] = 0.123317;
                        Matrix[1, 1] = 0.195346;
                        Matrix[1, 2] = 0.123317;

                        Matrix[2, 0] = 0.077847;
                        Matrix[2, 1] = 0.123317;
                        Matrix[2, 2] = 0.077847;
                        break;

                    case PreparedFilters.GaussianBlur5x5:
                        MatrixDimension = 5;

                        Factor = (1.0 / 256.0) + "";
                        Bias = 0.0 + "";

                        Matrix[0, 0] = 1;
                        Matrix[0, 1] = 4;
                        Matrix[0, 2] = 6;
                        Matrix[0, 3] = 4;
                        Matrix[0, 4] = 1;

                        Matrix[1, 0] = 4;
                        Matrix[1, 1] = 16;
                        Matrix[1, 2] = 24;
                        Matrix[1, 3] = 16;
                        Matrix[1, 4] = 4;

                        Matrix[2, 0] = 6;
                        Matrix[2, 1] = 24;
                        Matrix[2, 2] = 36;
                        Matrix[2, 3] = 24;
                        Matrix[2, 4] = 6;

                        Matrix[3, 0] = 4;
                        Matrix[3, 1] = 16;
                        Matrix[3, 2] = 24;
                        Matrix[3, 3] = 16;
                        Matrix[3, 4] = 4;

                        Matrix[4, 0] = 1;
                        Matrix[4, 1] = 4;
                        Matrix[4, 2] = 6;
                        Matrix[4, 3] = 4;
                        Matrix[4, 4] = 1;
                        break;

                    case PreparedFilters.Sharpen5x5:
                        MatrixDimension = 5;

                        Factor = 1.0 + "";
                        Bias = 0.0 + "";

                        Matrix[0, 0] = -1;
                        Matrix[0, 1] = -1;
                        Matrix[0, 2] = -1;
                        Matrix[0, 3] = -1;
                        Matrix[0, 4] = -1;

                        Matrix[1, 0] = -1;
                        Matrix[1, 1] = 2;
                        Matrix[1, 2] = 2;
                        Matrix[1, 3] = 2;
                        Matrix[1, 4] = -1;

                        Matrix[2, 0] = -1;
                        Matrix[2, 1] = 2;
                        Matrix[2, 2] = 8;
                        Matrix[2, 3] = 2;
                        Matrix[2, 4] = -1;

                        Matrix[3, 0] = -1;
                        Matrix[3, 1] = 2;
                        Matrix[3, 2] = 2;
                        Matrix[3, 3] = 2;
                        Matrix[3, 4] = -1;

                        Matrix[4, 0] = -1;
                        Matrix[4, 1] = -1;
                        Matrix[4, 2] = -1;
                        Matrix[4, 3] = -1;
                        Matrix[4, 4] = -1;
                        break;

                    case PreparedFilters.IntenseSharpen:
                        MatrixDimension = 3;

                        Factor = 1.0 + "";
                        Bias = 0.0 + "";

                        Matrix[0, 0] = 1;
                        Matrix[0, 1] = 1;
                        Matrix[0, 2] = 1;

                        Matrix[1, 0] = 1;
                        Matrix[1, 1] = -7;
                        Matrix[1, 2] = 1;

                        Matrix[2, 0] = 1;
                        Matrix[2, 1] = 1;
                        Matrix[2, 2] = 1;
                        break;

                    case PreparedFilters.Emboss:
                        MatrixDimension = 3;

                        Factor = 1.0 + "";
                        Bias = 0.0 + "";

                        Matrix[0, 0] = 2;
                        Matrix[0, 1] = 0;
                        Matrix[0, 2] = 0;

                        Matrix[1, 0] = 0;
                        Matrix[1, 1] = -1;
                        Matrix[1, 2] = 0;

                        Matrix[2, 0] = 0;
                        Matrix[2, 1] = 0;
                        Matrix[2, 2] = -1;
                        break;

                    case PreparedFilters.Emboss45Degrees:
                        MatrixDimension = 3;

                        Factor = 1.0 + "";
                        Bias = 128.0 + "";

                        Matrix[0, 0] = -1;
                        Matrix[0, 1] = -1;
                        Matrix[0, 2] = 0;

                        Matrix[1, 0] = -1;
                        Matrix[1, 1] = 0;
                        Matrix[1, 2] = 1;

                        Matrix[2, 0] = 0;
                        Matrix[2, 1] = 1;
                        Matrix[2, 2] = 1;
                        break;

                    case PreparedFilters.IntenseEmboss:
                        MatrixDimension = 5;

                        Factor = 1.0 + "";
                        Bias = 128.0 + "";

                        Matrix[0, 0] = -1;
                        Matrix[0, 1] = -1;
                        Matrix[0, 2] = -1;
                        Matrix[0, 3] = -1;
                        Matrix[0, 4] = 0;

                        Matrix[1, 0] = -1;
                        Matrix[1, 1] = -1;
                        Matrix[1, 2] = -1;
                        Matrix[1, 3] = 0;
                        Matrix[1, 4] = 1;

                        Matrix[2, 0] = -1;
                        Matrix[2, 1] = -1;
                        Matrix[2, 2] = 0;
                        Matrix[2, 3] = 1;
                        Matrix[2, 4] = 1;

                        Matrix[3, 0] = -1;
                        Matrix[3, 1] = 0;
                        Matrix[3, 2] = 1;
                        Matrix[3, 3] = 1;
                        Matrix[3, 4] = 1;

                        Matrix[4, 0] = 0;
                        Matrix[4, 1] = 1;
                        Matrix[4, 2] = 1;
                        Matrix[4, 3] = 1;
                        Matrix[4, 4] = 1;
                        break;

                    case PreparedFilters.Soften:
                        MatrixDimension = 3;

                        Factor = (1.0 / 9.0) + "";
                        Bias = 0.0 + "";

                        Matrix[0, 0] = 1;
                        Matrix[0, 1] = 1;
                        Matrix[0, 2] = 1;

                        Matrix[1, 0] = 1;
                        Matrix[1, 1] = 1;
                        Matrix[1, 2] = 1;

                        Matrix[2, 0] = 1;
                        Matrix[2, 1] = 1;
                        Matrix[2, 2] = 1;
                        break;

                    case PreparedFilters.MotionBlur:
                        MatrixDimension = 9;

                        Factor = (1.0 / 9.0) + "";
                        Bias = 0.0 + "";

                        for (int i = 0; i < MatrixDimension; i++)
                        {
                            for (int j = 0; j < MatrixDimension; j++)
                            {
                                Matrix[i, j] = i == j ? 1 : 0;
                            }
                        }
                        break;

                    case PreparedFilters.HorizontalEdges:
                        MatrixDimension = 5;

                        Factor = 1.0 + "";
                        Bias = 0.0 + "";

                        Matrix[0, 2] = -1;
                        Matrix[1, 2] = -1;
                        Matrix[2, 2] = 2;
                        break;

                    case PreparedFilters.VerticalEdges:
                        MatrixDimension = 5;

                        Factor = 1.0 + "";
                        Bias = 0.0 + "";

                        Matrix[0, 2] = -1;
                        Matrix[1, 2] = -1;
                        Matrix[2, 2] = 4;
                        Matrix[3, 2] = -1;
                        Matrix[4, 2] = -1;
                        break;

                    case PreparedFilters.Edges45Degrees:
                        MatrixDimension = 5;

                        Factor = 1.0 + "";
                        Bias = 0.0 + "";

                        Matrix[0, 0] = -1;
                        Matrix[1, 1] = -2;
                        Matrix[2, 2] = 6;
                        Matrix[3, 3] = -2;
                        Matrix[4, 4] = -1;
                        break;

                    case PreparedFilters.EdgesAllDirections:
                        MatrixDimension = 3;

                        Factor = 1.0 + "";
                        Bias = 0.0 + "";

                        Matrix[0, 0] = -1;
                        Matrix[0, 1] = -1;
                        Matrix[0, 2] = -1;

                        Matrix[1, 0] = -1;
                        Matrix[1, 1] = 8;
                        Matrix[1, 2] = -1;

                        Matrix[2, 0] = -1;
                        Matrix[2, 1] = -1;
                        Matrix[2, 2] = -1;
                        break;

                    case PreparedFilters.Sharpen:
                        MatrixDimension = 3;

                        Factor = 1.0 + "";
                        Bias = 0.0 + "";

                        Matrix[0, 0] = -1;
                        Matrix[0, 1] = -1;
                        Matrix[0, 2] = -1;

                        Matrix[1, 0] = -1;
                        Matrix[1, 1] = 9;
                        Matrix[1, 2] = -1;

                        Matrix[2, 0] = -1;
                        Matrix[2, 1] = -1;
                        Matrix[2, 2] = -1;
                        break;
                }
            }
        }

        private void UpdateMatrix()
        {
            var oldMatrix = Matrix;

            Matrix = new double[MatrixDimension, MatrixDimension];
            for (int i = 0; i < Matrix.GetLength(0); i++)
            {
                for (int j = 0; j < Matrix.GetLength(1); j++)
                {
                    if (oldMatrix != null &&
                        i < oldMatrix.GetLength(0) &&
                        j < oldMatrix.GetLength(1))
                    {
                        Matrix[i, j] = oldMatrix[i, j];
                    }
                    else
                    {
                        Matrix[i, j] = 0;
                    }
                }
            }
        }

        private void SelectImage()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "All images types|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff",
                Title = "Select image"
            };

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            IsImageSelected = true;
            OriginalImagePath = openFileDialog.FileName;

            if (_originalImageBitmap != null)
            {
                _originalImageBitmap.Dispose();
            }

            if (_resultingImageBitmap != null)
            {
                _resultingImageBitmap.Dispose();
            }

            using (var image = new Bitmap(openFileDialog.FileName))
            {
                _originalImageBitmap = new DirectBitmap(image.Width, image.Height);
                _originalImageBitmap.DrawImage(image);

                _resultingImageBitmap = new DirectBitmap(image.Width, image.Height);
            }

            OriginalImage = _originalImageBitmap.ToBitmapImage();

            _sw.Restart();

            CreateOriginalImageHistogram();

            _sw.Stop();
            Debug.WriteLine("(CreateOriginalImageHistogram) Loaded histograms in " + _sw.Elapsed);
        }

        private void CreateOriginalImageHistogram()
        {
            var histograms = _originalImageBitmap.GetHistograms(
                _histogramPadding,
                _histogramBarScale,
                _histogramHeight,
                (_brightnessHistogramColor,
                    _redHistogramColor,
                    _greenHistogramColor,
                    _blueHistogramColor,
                    _transparencyHistogramColor));

            _originalImageHistograms[0] = histograms.Item1;
            _originalImageHistograms[1] = histograms.Item2;
            _originalImageHistograms[2] = histograms.Item3;
            _originalImageHistograms[3] = histograms.Item4;
            _originalImageHistograms[4] = histograms.Item5;

            OnPropertyChanged(nameof(OriginalImageBrightnessHistogram));
            OnPropertyChanged(nameof(OriginalImageRedHistogram));
            OnPropertyChanged(nameof(OriginalImageGreenHistogram));
            OnPropertyChanged(nameof(OriginalImageBlueHistogram));
            OnPropertyChanged(nameof(OriginalImageTransparencyHistogram));
        }

        private void ApplyFilter()
        {
            for (int i = 0; i < Matrix.GetLength(0); i++)
            {
                for (int j = 0; j < Matrix.GetLength(1); j++)
                {
                    Debug.Write(Matrix[i, j] + " ");
                }

                Debug.WriteLine("");
            }

            if (!IsImageSelected)
            {
                MessageBox.Show("Image is not selected!",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            var w = _originalImageBitmap.Width;
            var h = _originalImageBitmap.Height;

            _sw.Restart();

            Parallel.For(0, w - 1, x =>
            { 
                Parallel.For(0, h - 1, y =>
                {
                    var red = 0.0;
                    var green = 0.0;
                    var blue = 0.0;

                    for (int filterX = 0; filterX < MatrixDimension; filterX++)
                    {
                        for (int filterY = 0; filterY < MatrixDimension; filterY++)
                        {
                            var imageX = (x - MatrixDimension / 2 + filterX + w) % w;
                            var imageY = (y - MatrixDimension / 2 + filterY + h) % h;

                            var color = _originalImageBitmap.GetPixel(imageX, imageY);

                            red += color.R * Matrix[filterX, filterY];
                            green += color.G * Matrix[filterX, filterY];
                            blue += color.B * Matrix[filterX, filterY];
                        }
                    }

                    var newRed = Math.Min(Math.Max((int)(_factor * red + _bias), 0), 255);
                    var newGreen = Math.Min(Math.Max((int)(_factor * green + _bias), 0), 255);
                    var newBlue = Math.Min(Math.Max((int)(_factor * blue + _bias), 0), 255);

                    var newColor = Color.FromArgb(255, newRed, newGreen, newBlue);

                    _resultingImageBitmap.DrawPoint(x, y, newColor);
                });
            });

            _sw.Stop();
            Debug.WriteLine("(ApplyFilter) Time taken: " + _sw.Elapsed);

            IsResultingImageReady = true;
            ResultingImage = _resultingImageBitmap.ToBitmapImage();

            _sw.Restart();

            CreateResultingImageHistogram();

            _sw.Stop();
            Debug.WriteLine("(CreateResultingImageHistogram) Loaded histograms in " + _sw.Elapsed);

            SelectedTabIndex = 3;
        }

        private void CreateResultingImageHistogram()
        {
            var histograms = _resultingImageBitmap.GetHistograms(
                _histogramPadding,
                _histogramBarScale,
                _histogramHeight,
                (_brightnessHistogramColor,
                    _redHistogramColor,
                    _greenHistogramColor,
                    _blueHistogramColor,
                    _transparencyHistogramColor));

            _resultingImageHistograms[0] = histograms.Item1;
            _resultingImageHistograms[1] = histograms.Item2;
            _resultingImageHistograms[2] = histograms.Item3;
            _resultingImageHistograms[3] = histograms.Item4;
            _resultingImageHistograms[4] = histograms.Item5;

            OnPropertyChanged(nameof(ResultingImageBrightnessHistogram));
            OnPropertyChanged(nameof(ResultingImageRedHistogram));
            OnPropertyChanged(nameof(ResultingImageGreenHistogram));
            OnPropertyChanged(nameof(ResultingImageBlueHistogram));
            OnPropertyChanged(nameof(ResultingImageTransparencyHistogram));
        }

        private void SaveResultingImage()
        {
            var defaultFileName = "Resulting image";
            var defaultExtension = ".png";

            var saveFileDialog = new SaveFileDialog()
            {
                FileName = defaultFileName,
                DefaultExt = Path.GetExtension(defaultFileName),
                ValidateNames = true,
                Filter = string.Format("{1} files (*{0})|*{0}|All files (*.*)|*.*", defaultExtension, defaultExtension.Remove(0, 1).ToUpper())
            };

            var dialogResult = saveFileDialog.ShowDialog();

            if (!dialogResult.HasValue ||
                !dialogResult.Value)
            {
                return;
            }

            _resultingImageBitmap.Bitmap.Save(saveFileDialog.FileName);

            MessageBox.Show("Image " + Path.GetFileName(saveFileDialog.FileName) + " is saved", "Image is saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
