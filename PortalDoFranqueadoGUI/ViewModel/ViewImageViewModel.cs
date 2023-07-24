using System.Windows.Media;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows.Controls;
using System;

namespace PortalDoFranqueado.ViewModel
{
    public class ViewImageViewModel : BaseViewModel
    {
        private double _zoomFactor = 1.0;

        public ImageSource? Source { get; }
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

        public ViewImageViewModel(FileView file)
        {
            if(!file.IsImage)
                return;

            Source = file.ImageData;
            ZoomInCommand = new RelayCommand(ZoomIn);
            ZoomOutCommand = new RelayCommand(ZoomOut);
            ResizeImageCommand = new RelayCommand<Image>(ResizeImage);
        }

        private void ZoomIn()
        {
            ZoomFactor *= 1.1;
        }

        private void ZoomOut()
        {
            ZoomFactor /= 1.1;
        }

        private void ResizeImage(Image image)
        {
            if (Source == null ||
                Me == null)
                return;

            var zoomFactorH = (Me.Height - 150) / Source.Height;
            var zoomFactorW = (Me.Width - 75) / Source.Width;

            ZoomFactor = Math.Min(zoomFactorH, zoomFactorW);
        }
    }
}