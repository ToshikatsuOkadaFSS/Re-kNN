using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Win32.SafeHandles;
using RekNNUtility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RekNN_MNIST_Sample
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        private WriteableBitmap _bitmap;
        private bool _isDrawing = false;
        private const int Width = 560;
        private const int Height = 560;

        private int _penSize = 20; // ペンの太さ（半径）

        RekNNUtility.RekNNUtility utility = new RekNNUtility.RekNNUtility(ModeEnum.MNIST, 1);


        public class ImageItem : INotifyPropertyChanged
        {
            public string Title { get; set; }
            // WriteableBitmap または BitmapImage を保持する
            public WriteableBitmap Bitmap { get; set; }


            /// <summary>
            /// イベント実装
            /// </summary>
            public event PropertyChangedEventHandler? PropertyChanged;

            /// <summary>
            /// 変数の変更通知
            /// </summary>
            /// <param name="propertyName"></param>
            protected void OnPropertyChanged(string propertyName)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }


            public ImageItem(string title, byte[] data)
            {
                Title = title;
                Bitmap = new WriteableBitmap(28, 28);

                using (var stream = Bitmap.PixelBuffer.AsStream())
                {
                    for (int x = 0; x < 28; x++)
                    {
                        for (int y = 0; y < 28; y++)
                        {
                            int idx = y * 28 + x;
                            byte value = data[idx];
                            // BGRAの順番で書き込み (白色にする場合)
                            byte[] color = new byte[] { value, value, value, 255 };

                            long pixelOffset = (y * 28 + x) * 4;

                            stream.Seek(pixelOffset, System.IO.SeekOrigin.Begin);

                            // 青, 緑, 赤, アルファ の順で書き込み (黒色にする場合)
                            stream.Write(color, 0, 4);
                        }
                    }
                }

                // 最後に一度だけ画面を更新
                Bitmap.Invalidate();
            }
        }

        /// <summary>
        /// 判定閾値
        /// </summary>
        private int Threshold = 60;

        /// <summary>
        /// 投票数
        /// </summary>
        private int Vote = 5;



        ObservableCollection<ImageItem> displayImages { get; } = new();

        ImageData[] imgData = new ImageData[0];
        byte[] labelData = new byte[0];
        ImageData[] testImgData = new ImageData[0];
        byte[] testLabelData = new byte[0];

        public MainWindow()
        {
            InitializeComponent();

            RekNN_MNIST_Sample.CommonValues.LoadSettings();
            RekNN_MNIST_Sample.CommonValues.SaveSettings();

            RekNNUtility.RekNNUtility.DisplayMessage = DisplayMessage;

            utility.LoadModel(0, RekNN_MNIST_Sample.CommonValues.Settings.DatabasePath);

            int imageNum = 60000;
            ImageHeader imgHeader = new ImageHeader();
            imgData = new ImageData[imageNum];
            LabelHeader labelHeader = new LabelHeader();
            labelData = new byte[imageNum];

            unsafe
            {
                byte* _data = null;
                long totalBytes = sizeof(ImageHeader) + sizeof(ImageData) * imageNum;
                using SafeFileHandle fileHandle =
                    File.OpenHandle(RekNN_MNIST_Sample.CommonValues.Settings.TrainImageFilePath, FileMode.Open, FileAccess.Read);
                _data = (byte*)NativeMemory.Alloc((nuint)totalBytes);
                Span<byte> dest = new Span<byte>(_data, (int)totalBytes);
                RandomAccess.Read(fileHandle, dest, 0);

                ImageHeader* header = (ImageHeader*)_data;
                ImageData* images = (ImageData*)(_data + sizeof(ImageHeader));

                imgHeader = *header;

                for (int i = 0; i < imageNum; i++)
                {
                    imgData[i] = images[i];
                }

                NativeMemory.Free(_data);
            }

            unsafe
            {
                byte* _data = null;
                long totalBytes = sizeof(LabelHeader) + sizeof(byte) * imageNum;
                using SafeFileHandle fileHandle =
                    File.OpenHandle(RekNN_MNIST_Sample.CommonValues.Settings.TrainLabelFilePath, FileMode.Open, FileAccess.Read);
                _data = (byte*)NativeMemory.Alloc((nuint)totalBytes);
                Span<byte> dest = new Span<byte>(_data, (int)totalBytes);
                RandomAccess.Read(fileHandle, dest, 0);

                LabelHeader* header = (LabelHeader*)_data;
                byte* labels = _data + sizeof(LabelHeader);

                labelHeader = *header;

                for (int i = 0; i < imageNum; i++)
                {
                    labelData[i] = labels[i];
                }

                NativeMemory.Free(_data);
            }

            int testNum = 10000;
            ImageHeader testImgHeader = new ImageHeader();
            testImgData = new ImageData[testNum];
            LabelHeader testLabelHeader = new LabelHeader();
            testLabelData = new byte[testNum];

            unsafe
            {
                byte* _data = null;
                long totalBytes = sizeof(ImageHeader) + sizeof(ImageData) * testNum;
                using SafeFileHandle fileHandle =
                    File.OpenHandle(RekNN_MNIST_Sample.CommonValues.Settings.TestImageFilePath, FileMode.Open, FileAccess.Read);
                _data = (byte*)NativeMemory.Alloc((nuint)totalBytes);
                Span<byte> dest = new Span<byte>(_data, (int)totalBytes);
                RandomAccess.Read(fileHandle, dest, 0);

                ImageHeader* header = (ImageHeader*)_data;
                ImageData* images = (ImageData*)(_data + sizeof(ImageHeader));

                testImgHeader = *header;

                for (int i = 0; i < testNum; i++)
                {
                    testImgData[i] = images[i];
                }

                NativeMemory.Free(_data);
            }

            unsafe
            {
                byte* _data = null;
                long totalBytes = sizeof(LabelHeader) + sizeof(byte) * testNum;
                using SafeFileHandle fileHandle =
                    File.OpenHandle(RekNN_MNIST_Sample.CommonValues.Settings.TestLabelFilePath, FileMode.Open, FileAccess.Read);
                _data = (byte*)NativeMemory.Alloc((nuint)totalBytes);
                Span<byte> dest = new Span<byte>(_data, (int)totalBytes);
                RandomAccess.Read(fileHandle, dest, 0);

                LabelHeader* header = (LabelHeader*)_data;
                byte* labels = _data + sizeof(LabelHeader);

                testLabelHeader = *header;

                for (int i = 0; i < testNum; i++)
                {
                    testLabelData[i] = labels[i];
                }

                NativeMemory.Free(_data);
            }

            utility.GetTrainVector = (pos) =>
            {
                float[][] vector = new float[1][];
                vector[0] = new float[28 * 28];
                unsafe
                {
                    for (int j = 0; j < 28 * 28; j++)
                    {
                        vector[0][j] = imgData[pos].Image[j] / 255.0f;
                    }
                }
                return vector;
            };

            utility.GetTrainLabel = (pos) =>
            {
                return labelData[pos];
            };
            utility.GetTrainNum = () => imageNum;

            utility.GetTestVector = (pos) =>
            {
                float[][] vector = new float[1][];
                vector[0] = new float[28 * 28];
                unsafe
                {
                    for (int j = 0; j < 28 * 28; j++)
                    {
                        vector[0][j] = testImgData[pos].Image[j] / 255.0f;
                    }
                }
                return vector;
            };

            utility.GetTestLabel = (pos) =>
            {
                return testLabelData[pos];
            };

            utility.GetTestNum = () => testNum;

            utility.GetTrainVector = (pos) =>
            {
                float[][] vector = new float[1][];
                vector[0] = new float[28 * 28];
                unsafe
                {
                    for (int j = 0; j < 28 * 28; j++)
                    {
                        vector[0][j] = imgData[pos].Image[j] / 255.0f;
                    }
                }
                return vector;
            };

            utility.GetTrainLabel = (pos) =>
            {
                return labelData[pos];
            };

            utility.GetTestVector = (pos) =>
            {
                float[][] vector = new float[1][];
                vector[0] = new float[28 * 28];
                unsafe
                {
                    for (int j = 0; j < 28 * 28; j++)
                    {
                        vector[0][j] = testImgData[pos].Image[j] / 255.0f;
                    }
                }
                return vector;
            };

            utility.GetTestLabel = (pos) =>
            {
                return testLabelData[pos];
            };


            ListViewResult.ItemsSource = displayImages;

            // 1. Bitmapの初期化 (BGRA8 形式)
            _bitmap = new WriteableBitmap(Width, Height);

            // サンプル描画
            Random rnd = new();
            int pos = rnd.Next(utility.GetTestNum());
            float[][] vector = utility.GetTrainVector(pos);

            using (var stream = _bitmap.PixelBuffer.AsStream())
            {
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        int posX = x * 28 / Width;
                        int posY = y * 28 / Height;
                        byte value = (byte)(vector[0][posY * 28 + posX] * 255);
                        // BGRAの順番で書き込み (白色にする場合)
                        byte[] color = new byte[] { value, value, value, 255 };

                        long pixelOffset = (y * Width + x) * 4;

                        if (pixelOffset + 4 <= stream.Length)
                        {
                            stream.Seek(pixelOffset, System.IO.SeekOrigin.Begin);

                            // 青, 緑, 赤, アルファ の順で書き込み (黒色にする場合)
                            stream.Write(color, 0, 4);
                        }
                    }
                }
            }

            // 最後に一度だけ画面を更新
            _bitmap.Invalidate();

            TargetImage.Source = _bitmap;
        }

        /// <summary>
        /// マウスボタンが押された
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _isDrawing = true;
            DrawAtPoint(e);
        }

        /// <summary>
        /// マウスポインタが移動した
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isDrawing)
            {
                DrawAtPoint(e);
            }
        }

        /// <summary>
        /// マウスのボタンが離された
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _isDrawing = false;
        }

        private void DrawAtPoint(PointerRoutedEventArgs e)
        {
            // マウスの相対座標を取得
            var point = e.GetCurrentPoint(TargetImage).Position;
            int x = (int)point.X;
            int y = (int)point.Y;

            // 範囲チェック
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;

            // 2. ピクセルデータの書き換え(色は白色, BGRAの順番で指定)
            UpdatePixel(x, y, new byte[] { 255, 255, 255, 255 });
        }

        private void UpdatePixel(int centerX, int centerY, byte[] color)
        {
            using (var stream = _bitmap.PixelBuffer.AsStream())
            {
                // 中心の周囲 (centerX - _penSize) ～ (centerX + _penSize) を塗る
                for (int y = centerY - _penSize; y <= centerY + _penSize; y++)
                {
                    if (y < 0) continue;
                    if (y >= Height) continue;

                    for (int x = centerX - _penSize; x <= centerX + _penSize; x++)
                    {
                        if (x < 0) continue;
                        if (x >= Width) continue;

                        double distance = Math.Sqrt(Math.Pow(x - centerX, 2) + Math.Pow(y - centerY, 2));
                        if (distance <= _penSize)
                        {
                            // 半径以内なら塗る
                            long pixelOffset = (y * Width + x) * 4;
                            stream.Seek(pixelOffset, System.IO.SeekOrigin.Begin);
                            stream.Write(color, 0, 4);
                        }
                    }
                }
            }

            // 最後に一度だけ画面を更新
            _bitmap.Invalidate();
        }

        private void UpdatePixelDot(int x, int y, byte[] color)
        {
            using (var stream = _bitmap.PixelBuffer.AsStream())
            {
                long pixelOffset = (y * Width + x) * 4;

                if (pixelOffset + 4 <= stream.Length)
                {
                    stream.Seek(pixelOffset, System.IO.SeekOrigin.Begin);

                    // 青, 緑, 赤, アルファ の順で書き込み (黒色にする場合)
                    stream.Write(color, 0, 4);
                }
            }

            // 最後に一度だけ画面を更新
            _bitmap.Invalidate();
        }

        private void ButtonStudy_Click(object sender, RoutedEventArgs e)
        {
            RootContent.IsEnabled = false;

            Task.Run(() =>
            {
                utility.LearnAllLabelsForCurrentModel(
                    RekNN_MNIST_Sample.CommonValues.Settings.SearchMaxNumForAddVector,
                    RekNN_MNIST_Sample.CommonValues.Settings.SimilarityThreshold);

                utility.Save(0, RekNN_MNIST_Sample.CommonValues.Settings.DatabasePath);

                this.DispatcherQueue.TryEnqueue(() =>
                {
                    RootContent.IsEnabled = true;
                });
            });
        }

        private void ButtonRefine_Click(object sender, RoutedEventArgs e)
        {
            RootContent.IsEnabled = false;

            DisplayMessage($"Start Refile. Please wait...");

            Task.Run(() =>
            {
                utility.RefineDatabaseForCurrentModel(-1);

                utility.Save(0, RekNN_MNIST_Sample.CommonValues.Settings.DatabasePath);

                this.DispatcherQueue.TryEnqueue(() =>
                {
                    RootContent.IsEnabled = true;
                    DisplayMessage($"Refine completed.");
                });
            });
        }

        private void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            utility.LoadModel(0, RekNN_MNIST_Sample.CommonValues.Settings.DatabasePath);
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            utility.Save(0, RekNN_MNIST_Sample.CommonValues.Settings.DatabasePath);
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    UpdatePixelDot(x, y, new byte[] { 0, 0, 0, 255 });
                }
            }
        }

        private unsafe void ButtonPredict_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = _bitmap.PixelBuffer.ToArray();

            // これを縮小する
            int sizeX = 28;
            int sizeY = 28;
            int[][] sum = new int[sizeX][];
            int[][] count = new int[sizeX][];
            for (int i = 0; i < sizeX; i++)
            {
                sum[i] = new int[sizeY];
                count[i] = new int[sizeY];
            }

            int stepX = Width / sizeX;
            int stepY = Height / sizeY;
            for (int x = 0; x < Width; x++)
            {
                int posX = x / stepX;
                for (int y = 0; y < Height; y++)
                {
                    int posY = y / stepY;

                    sum[posX][posY] += data[x * 4 + y * Width * 4 + 0]; // 青
                    count[posX][posY] += 1;
                }
            }

            float[][] chk = new float[sizeX][];
            for (int i = 0; i < sizeX; i++)
            {
                chk[i] = new float[sizeY];
            }

            float[][] vec = new float[1][];
            vec[0] = new float[sizeX * sizeY];
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    chk[x][y] = ((float)sum[x][y] / (float)count[x][y]) / 255.0f;
                    vec[0][x+y*sizeX] = chk[x][y];
                }
            }

            //vec = utility.GetTestVector?.Invoke(0) ?? vec;

            double th = (double)Threshold / 100.0;

            ((int, double)?, List<ResultItemMainAndSub>) result = utility.Predict(0, Vote, th, vec);

            // これを表示
            displayImages.Clear();

            TextBlockResult.Text = $"Predict Result: {result.Item1?.Item1.ToString() ?? "Unknown"} : Score = {result.Item1?.Item2.ToString("0.0000") ?? "null"}";

            for (int i = 0; i < result.Item2.Count; i++)
            {
                int mainId = result.Item2[i].MainId;
                int subId = result.Item2[i].SubId;
                double score = result.Item2[i].Score;
                byte[] img = new byte[28 * 28];
                for (int j = 0; j < 28 * 28; j++)
                {
                    img[j] = imgData[subId].Image[j];
                }
                displayImages.Add(new ImageItem($"MainId: {mainId}, SubId: {subId}, Score: {score}", img));
            }
        }


        List<string> logs = new();

        public void DisplayMessage(string? line)
        {
            if (line is null) return;

            this.DispatcherQueue.TryEnqueue(() =>
            {
                int maxLogCount = 100; // 最大ログ数
                logs.Add(line);
                if (logs.Count > maxLogCount)
                {
                    logs.RemoveRange(0, logs.Count - maxLogCount);
                }
                TextBoxLog.Text = string.Join("\n", logs);

                // TextBoxの中身（Visual Tree）から ScrollViewer を探す
                var grid = VisualTreeHelper.GetChild(TextBoxLog, 0) as Grid;
                if (grid == null) return;

                foreach (var child in grid.Children)
                {
                    if (child is ScrollViewer scrollViewer)
                    {
                        // 垂直スクロール位置を最大（下端）へ移動
                        scrollViewer.ChangeView(null, scrollViewer.ScrollableHeight, null);
                        break;
                    }
                }
            });
        }


        /// <summary>
        /// スライダーの値が変わった
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PenSizeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Threshold = (int)e.NewValue;

            if (TextBlockThreshold != null)
            {
                TextBlockThreshold.Text = Threshold.ToString();
            }

        }

        /// <summary>
        /// 投票数スライダーの値が変わった
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VoteSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Vote = (int)e.NewValue;

            if (TextBlockVote != null)
            {
                TextBlockVote.Text = Vote.ToString();
            }
        }
    }
}


