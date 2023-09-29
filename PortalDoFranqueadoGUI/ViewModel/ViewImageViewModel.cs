using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Drawing;

namespace PortalDoFranqueado.ViewModel
{
    public class ViewImageViewModel : BaseViewModel
    {
        private double _zoomFactor = 1.0;
        private Size? _size;

        public string Source { get; }
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand ResizeImageCommand { get; }

        public double ZoomFactor
        {
            get => _zoomFactor;
            set
            {
                if (_zoomFactor != value)
                {
                    _zoomFactor = value;
                    OnPropertyChanged(nameof(ZoomFactor));
                }
            }
        }

        public Size Size
        {
            get
            {
                if (_size == null)
                    using (var image = Image.FromFile(Source))
                        _size = image.Size;

                return _size.Value;
            }
        }

        public ViewImageViewModel(FileView file)
        {
            if(!file.IsImage)
                return;

            Source = file.FilePath;
            ZoomInCommand = new RelayCommand(ZoomIn);
            ZoomOutCommand = new RelayCommand(ZoomOut);
            ResizeImageCommand = new RelayCommand(ResizeImage);
        }

        private void ZoomIn()
        {
            ZoomFactor *= 1.1;
        }

        private void ZoomOut()
        {
            ZoomFactor /= 1.1;
        }

        private void ResizeImage()
        {
            if (Source == null ||
                Me == null)
                return;

            var zoomFactorH = (Me.Height - 150) / Size.Height;
            var zoomFactorW = (Me.Width - 75) / Size.Width;

            ZoomFactor = Math.Min(zoomFactorH, zoomFactorW);
        }
    }
}