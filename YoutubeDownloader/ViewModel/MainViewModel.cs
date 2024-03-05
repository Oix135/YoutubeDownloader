using Ookii.Dialogs.Wpf;
using ReactiveUI;
using System.Diagnostics;
using System.Reactive;
using System.Windows;
using YoutubeDownloader.Model;

namespace YoutubeDownloader.ViewModel
{
    internal class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            LstBitrate = new List<int>();

            var canDownload = this.WhenAnyValue(a => a.CanDownload);
            var canGetDescription = this.WhenAnyValue(a => a.CanGetDEscription);

            OpenFolderCommand = ReactiveCommand.Create(OpenFolder);
            DownloadCommand = ReactiveCommand.CreateFromTask(DownloadAsync, canDownload);
            GetDescriptionCommand = ReactiveCommand.CreateFromTask(GetDescription, canGetDescription);


        }

        #region Properties

        public bool CanDownload => !string.IsNullOrEmpty(Path) && !string.IsNullOrEmpty(Link);
        public bool CanGetDEscription => !string.IsNullOrEmpty(Link);

        private string _path;
        public string Path
        {
            get => _path;
            set
            {
                this.RaiseAndSetIfChanged(ref _path, value);
                this.RaisePropertyChanged(nameof(CanDownload));
            }
        }

        private string _link;
        public string Link
        {
            get => _link;
            set
            {
                this.RaiseAndSetIfChanged(ref _link, value);
                this.RaisePropertyChanged(nameof(CanDownload));
                this.RaisePropertyChanged(nameof(CanGetDEscription));
                ProgressText = string.Empty;
            }
        }
        private string _description;
        public string Description
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value);
        }
        private string _progressText;
        public string ProgressText
        {
            get => _progressText;
            set => this.RaiseAndSetIfChanged(ref _progressText, value);
        }

        private bool _downloading;
        public bool Downloading
        {
            get => _downloading;
            set
            {
                this.RaiseAndSetIfChanged(ref _downloading, value);
                if(value)
                {
                    ShowProgress();
                }
                else
                {
                    HideProgress();
                }
            }
        }
        public List<int> LstBitrate { get; }

        private int _selectedBitrate;
        public int SelectedBitrate
        {
            get => _selectedBitrate;
            set => this.RaiseAndSetIfChanged(ref _selectedBitrate, value);
        }

        private void HideProgress()
        {
            ProgressText = "Завершено";
        }

        private async void ShowProgress()
        {
            ProgressText = "Скачивание";
            while (Downloading)
            {
               
                if (!ProgressText.EndsWith("..."))
                {
                    ProgressText += ".";
                }
                else
                {
                    ProgressText = "Скачивание";
                }
                await Task.Delay(1000);
            }

        }

        #endregion

        #region Commands
        public ReactiveCommand<Unit, Unit> OpenFolderCommand { get; }
        public ReactiveCommand<Unit, Unit> DownloadCommand { get; }
        public ReactiveCommand<Unit, Unit> GetDescriptionCommand { get; }

        #endregion

        #region Methods

        private void OpenFolder()
        {
            var dlg = new VistaFolderBrowserDialog();

            if (dlg.ShowDialog().GetValueOrDefault())
            {
                Path = dlg.SelectedPath;
            }
        }

        private async Task DownloadAsync()
        {
            var downloader = new Downloader();
            try
            {
                Downloading = true;


                var filename = await downloader.Download(Link, Path, SelectedBitrate);


                OpenResultFolder(filename);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Downloading = false;
            }
        }

        private async Task GetDescription()
        {
            var downloader = new Downloader();
            try
            {
                var tuple = await downloader.GetDescription(Link);

                Description = tuple.Item2;

                LstBitrate.Clear();
                tuple.Item1.ForEach(a=>LstBitrate.Add(a));
                this.RaisePropertyChanged(nameof(LstBitrate));
                SelectedBitrate = LstBitrate.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private static void OpenResultFolder(string filename)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer",
                    Arguments = $"/n, /select, {filename}"
                });
            }
        }

        #endregion
    }
}
