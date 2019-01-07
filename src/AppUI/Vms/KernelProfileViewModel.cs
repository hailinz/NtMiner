﻿using NTMiner.Core.Kernels;
using NTMiner.Core.Profiles;
using NTMiner.Views;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace NTMiner.Vms {
    public class KernelProfileViewModel : ViewModelBase, IKernelProfile {
        public static readonly KernelProfileViewModel Empty = new KernelProfileViewModel(KernelViewModel.Empty, NTMinerRoot.Current.KernelProfileSet.EmptyKernelProfile) {
            _cancelDownload = null,
            _downloadMessage = string.Empty,
            _downloadPercent = 0,
            _isDownloading = false,
        };

        private string _downloadMessage;
        private bool _isDownloading = false;
        private double _downloadPercent;

        private Action _cancelDownload;

        private readonly IKernelProfile _kernelProfile;
        public ICommand CancelDownload { get; private set; }
        public ICommand Install { get; private set; }
        public ICommand UnInstall { get; private set; }

        private KernelViewModel _kernelVm;
        public KernelProfileViewModel(KernelViewModel kernelVm, IKernelProfile kernelProfile) {
            _kernelVm = kernelVm;
            _kernelProfile = kernelProfile;
            this.CancelDownload = new DelegateCommand(() => {
                _cancelDownload?.Invoke();
            });
            this.Install = new DelegateCommand(() => {
                this.Download();
            });
            this.UnInstall = new DelegateCommand(() => {
                DialogWindow.ShowDialog(message: $"您确定卸载{_kernelVm.FullName}内核吗？", title: "确认", onYes: () => {
                    string processName = _kernelVm.GetProcessName();
                    if (!string.IsNullOrEmpty(processName)) {
                        Process[] processes = Process.GetProcessesByName(processName);
                        if (processes != null && processes.Length != 0) {
                            Windows.TaskKill.Kill(processName);
                        }
                        File.Delete(_kernelVm.GetPackageFileFullName());
                        if (Directory.Exists(_kernelVm.GetKernelDirFullName())) {
                            try {
                                Directory.Delete(_kernelVm.GetKernelDirFullName(), recursive: true);
                            }
                            catch (Exception e) {
                                Global.Logger.Error(e.Message, e);
                            }
                        }
                        File.Delete(_kernelVm.GetDownloadFileFullName());
                    }
                    Refresh();
                    KernelPageViewModel.Current.OnPropertyChanged(nameof(KernelPageViewModel.QueryResults));
                }, icon: "Icon_Confirm");
            });
        }

        public void Refresh() {
            OnPropertyChanged(nameof(InstallStatus));
            OnPropertyChanged(nameof(InstallStatusDescription));
            OnPropertyChanged(nameof(BtnInstallVisible));
            OnPropertyChanged(nameof(BtnUpdateVisible));
            OnPropertyChanged(nameof(BtnInstalledVisible));
        }

        public Guid KernelId {
            get => _kernelProfile.KernelId;
        }

        public InstallStatus InstallStatus {
            get => _kernelProfile.InstallStatus;
        }

        public string InstallStatusDescription {
            get {
                return InstallStatus.GetDescription();
            }
        }

        public Visibility BtnInstallVisible {
            get {
                if (InstallStatus == InstallStatus.Uninstalled) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility BtnUpdateVisible {
            get {
                if (InstallStatus == InstallStatus.CanUpdate) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility BtnInstalledVisible {
            get {
                if (InstallStatus == InstallStatus.Installed) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public bool IsDownloading {
            get { return _isDownloading; }
            set {
                _isDownloading = value;
                OnPropertyChanged(nameof(IsDownloading));
                Refresh();
                KernelPageViewModel.Current.OnPropertyChanged(nameof(KernelPageViewModel.DownloadingVms));
            }
        }

        public double DownloadPercent {
            get {
                return _downloadPercent;
            }
            set {
                _downloadPercent = value;
                OnPropertyChanged(nameof(DownloadPercent));
            }
        }

        public string DownloadMessage {
            get {
                return _downloadMessage;
            }
            set {
                _downloadMessage = value;
                OnPropertyChanged(nameof(DownloadMessage));
            }
        }

        #region Download
        public void Download(Action<bool, string> downloadComplete = null) {
            if (this.IsDownloading) {
                return;
            }
            this.IsDownloading = true;
            string package = _kernelVm.Package;
            NTMinerRoot.Current.PackageDownloader.Download(package, progressChanged: (percent) => {
                this.DownloadMessage = percent + "%";
                this.DownloadPercent = (double)percent / 100;
            }, downloadComplete: (isSuccess, message, saveFileFullName) => {
                this.DownloadMessage = message;
                this.DownloadPercent = 0;
                if (isSuccess) {
                    File.Copy(saveFileFullName, Path.Combine(SpecialPath.PackagesDirFullName, package), overwrite: true);
                    File.Delete(saveFileFullName);
                    this.IsDownloading = false;
                }
                else {
                    TimeSpan.FromSeconds(2).Delay().ContinueWith((t) => {
                        this.IsDownloading = false;
                    });
                }
                downloadComplete?.Invoke(isSuccess, message);
            }, cancel: out _cancelDownload);
        }
        #endregion
    }
}
