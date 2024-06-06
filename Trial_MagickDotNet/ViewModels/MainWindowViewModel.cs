using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageMagick;
using Prism.Mvvm;

namespace Trial_MagickDotNet.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MainWindowViewModel : BindableBase
    {
        private string title = "Prism Application";
        private ImageSource imageSource;

        public MainWindowViewModel()
        {
            OpenPsd("sample.psd");
        }

        public string Title { get => title; set => SetProperty(ref title, value); }

        public ImageSource ImageSource { get => imageSource; private set => SetProperty(ref imageSource, value); }

        private void OpenPsd(string filePath)
        {
            using var images = new MagickImageCollection(filePath);

            // index[0] には 全てのレイヤーが統合された状態の画像が入っていることを確認。
            // index[1] 以降は、それぞれのレイヤーの画像が保存されている　（未確認）
            // ushort の部分はピクセルフォーマットを表すらしい （未確認)
            // ReSharper disable once SuggestVarOrType_Elsewhere 型を読み手に明示するため、あえて記述している
            IMagickImage<ushort> preview = images[0];

            // Image コントロールにセット
            ImageSource = ConvertToImageSource(preview);

            // ImageSource に表示中の内容を png に出力も可能
            SaveImageToFile(ImageSource, "test.png");
        }

        private ImageSource ConvertToImageSource(IMagickImage image)
        {
            using var memoryStream = new MemoryStream();

            // MagickImage を PNG フォーマットでストリームに書き込む
            image.Write(memoryStream, MagickFormat.Png);

            // ストリームの先頭にシーク
            memoryStream.Seek(0, SeekOrigin.Begin);

            // WPF の BitmapImage を作成
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = memoryStream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze(); // フリーズしてスレッドセーフにする

            return bitmap;
        }

        private void SaveImageToFile(ImageSource src, string filePath)
        {
            if (src is not BitmapSource bitmapSource)
            {
                return;
            }

            // WriteableBitmap に変換
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            // ファイルに書き込み
            using var fileStream = new FileStream(filePath, FileMode.Create);
            encoder.Save(fileStream);
        }
    }
}