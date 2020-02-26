﻿using Dicom.Imaging;
using Dicom.Log;
using DicomViewer.DotNetExtensions;
using DicomViewer.IO;
using DicomViewer.Presentation;
using DicomViewer.Properties;
using Entities;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAPICodePack.Dialogs;
using RenderEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace DicomViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainViewModel _viewModel;
        private Scan _scan;        
        private ILogger _logger;
        private ScanPresenter2D _presenter2d;
        private ScanPresenter3D _presenter;

        protected override void OnStartup(StartupEventArgs e)
        {
            LogManager.SetImplementation(ConsoleLogManager.Instance);
            ImageManager.SetImplementation(WPFImageManager.Instance);
            _logger = Logging.GetLogger<App>();
            _logger.LogInformation("Dicom Viewer started");

            Current.DispatcherUnhandledException += OnUnhandledException;
        }

        private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.LogCritical(e.Exception, $"'{e.Exception.Message}' was unhandled \n {e.Exception.StackTrace}");
        }

        protected override void OnActivated(EventArgs e)
        {
            if (_viewModel != null) { return; }
            _viewModel = (MainViewModel)MainWindow.DataContext;

            _viewModel.LoadDatasetCommand = new BindableCommand(LoadFolder);
            _viewModel.LoadFileCommand = new BindableCommand(LoadFile);

            new PropertySubscription(() => _viewModel.SelectedSeries, () =>
            {
                LoadSeries();
            });
        }

        private void LoadSeries()
        {
            _viewModel.ImageViewer.Visuals.Clear();
            DisposeScan();
            if (_viewModel.SelectedSeries == null) { return; }

            _scan = DicomVolumeLoader.Load(_viewModel.SelectedSeries as DicomSeries);
            if (_scan == null) { return; }
            _presenter = new ScanPresenter3D(_viewModel, _viewModel.VolumeViewer);
            _presenter.Present(_scan);

            _presenter2d = new ScanPresenter2D(_viewModel);
            _presenter2d.Present(_scan);
        }

        private void LoadFile()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = false,
                InitialDirectory = Settings.Default.LastUsedFolder
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Open(dialog.FileName);
            }
        }

        private void LoadFolder()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                InitialDirectory = Settings.Default.LastUsedFolder
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Open(dialog.FileName);
            }
        }

        private void Open(string path)
        {
            _viewModel.ImageViewer.Visuals.Clear();

            DisposeScan();

            IEnumerable<Series> series = null;
            if (Directory.Exists(path))
            {
                _logger.LogInformation("User opened directory");
                series = DicomSeriesExtractor.ExtractSeriesFromDirectory(path);
            }
            else
            {
                if (string.Compare(Path.GetFileName(path), "DICOMDIR", true, CultureInfo.InvariantCulture) == 0)
                {
                    _logger.LogInformation("Opened dicomdir");
                    series = DicomSeriesExtractor.ExtractSeriesFromDicomDir(path);
                }
                else
                {
                    _logger.LogInformation("Opened single dicom file");
                    series = DicomSeriesExtractor.ExtractSeriesFromSingleFile(path);
                }
            }

            if (series == null || series.Count() == 0)
            {
                _logger.LogWarning("Series null or empty");
                return;
            }

            _viewModel.Series = series;
            _viewModel.SelectedSeries = series.First();

            Settings.Default.LastUsedFolder = path;
            Settings.Default.Save();
        }

        private void DisposeScan()
        {
            if (_presenter2d != null)
            {
                _presenter2d.Dispose();
                _presenter2d = null;
            }

            if (_presenter != null)
            {
                _presenter.Dispose();
                _presenter = null;
            }
            if (_scan != null)
            {
                _scan.Volume.Slices.ForEach(slice => slice.Dispose());
                _scan.Volume.Slices.Clear();
                _scan = null;
            }
        }
    }
}
