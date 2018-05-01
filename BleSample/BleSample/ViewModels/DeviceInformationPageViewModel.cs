using BleSample.Services.DeviceBluetoothLe.Device;
using Plugin.BluetoothLE;
using Prism.Commands;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;


namespace BleSample.ViewModels
{
    public class DeviceInformationPageViewModel : BasePageViewModel, IDeviceInformationPageViewModel
    {
        private readonly IBleService bleService;
        private readonly IPageDialogService dialogService;

        private ObservableCollection<string> availableDevices;
        private string selectedeviceKey;
        private Dictionary<string, Services.DeviceBluetoothLe.Device.IDevice> foundDevices;
        private Services.DeviceBluetoothLe.Device.IDevice selectedDevice;

        private bool connecting;
        private bool disconnecting;
        private bool scanninfForDevices;
        private string commandResponse;
        private CancellationTokenSource ledToken;

        public DeviceInformationPageViewModel(IBleService bleService, IPageDialogService dialogService)
        {
            this.bleService = bleService;
            this.dialogService = dialogService;

            AvailableDevices = new ObservableCollection<string>();
            foundDevices = new Dictionary<string, Services.DeviceBluetoothLe.Device.IDevice>();
            ScanForDevicesCommand = new DelegateCommand(OnScanForDevicesCommand, () => !ScanningForDevices);
            ConnectDeviceCommand = new DelegateCommand(OnConnectDeviceCommand, () => !Connecting);
            DisconnectDeviceCommand = new DelegateCommand(OnDisconnectDeviceCommand, () => !Disconnecting);
            ToggleLedCommand = new DelegateCommand(OnToggleLedCommand);
        }

        private void OnToggleLedCommand()
        {
            LedToken  = new CancellationTokenSource();
            bleService.CtrlLed(true, 400, 500, ledToken.Token, t =>
            {
                if(t != null)
                {
                    CommandResponse = "Successful but invalid response";
                }
                else
                {
                    CommandResponse = "Successful";
                }
            }, () =>
            {
                CommandResponse = "Command Failed";
            });
        }

        private async void OnDisconnectDeviceCommand()
        {
            if (!Disconnecting)
            {
                if (SelectedDevice != null)
                {
                    if (CrossBleAdapter.Current.Status == AdapterStatus.PoweredOn || CrossBleAdapter.Current.Status == AdapterStatus.Unsupported)
                    {
                        try
                        {
                            Disconnecting = true;
                            DisconnectDeviceCommand.RaiseCanExecuteChanged();
                            //BatteryToken?.Cancel();
                            //SystemToken?.Cancel();
                            //ProgramToken?.Cancel();
                            bleService.Disconnect();
                            var timeOut = 0;
                            while (SelectedDevice.ConnectState != DeviceState.Disconnected && timeOut < 30)
                            {
                                await Task.Delay(TimeSpan.FromSeconds(1));
                                timeOut++;
                            }
                            Disconnecting = false;
                            DisconnectDeviceCommand.RaiseCanExecuteChanged();
                            if (SelectedDevice.ConnectState != DeviceState.Disconnected)
                            {
                                await dialogService.DisplayAlertAsync("Bluetooth", "Device failed to disconnect", "Ok");
                            }
                            else
                            {
                                foundDevices.Clear();
                                AvailableDevices.Clear();
                                SelectedDevice = null;
                                // RaisePropertyChanged(nameof(SelectedDevice));

                            }

                        }
                        catch (Exception e)
                        {
                            //  Crashes.TrackError(e);
                            Disconnecting = false;
                            DisconnectDeviceCommand.RaiseCanExecuteChanged();
                        }
                    }
                    else
                    {
                        await dialogService.DisplayAlertAsync("Bluetooth", "Please check your Bluetooth Settings", "Ok");
                    }
                }
            }
        }
        private async void OnConnectDeviceCommand()
        {
            if (!Connecting)
            {
                if (SelectedDevice != null)
                {
                    if (CrossBleAdapter.Current.Status == AdapterStatus.PoweredOn || CrossBleAdapter.Current.Status == AdapterStatus.Unsupported)
                    {
                        try
                        {
                            Connecting = true;
                            ConnectDeviceCommand.RaiseCanExecuteChanged();

                            bleService.Connect();
                            var timeOut = 0;
                            while (SelectedDevice.ConnectState != DeviceState.Connected && timeOut < 30)
                            {
                                await Task.Delay(TimeSpan.FromSeconds(1));
                                timeOut++;
                            }
                            Connecting = false;
                            ConnectDeviceCommand.RaiseCanExecuteChanged();
                            if (SelectedDevice.ConnectState != DeviceState.Connected)
                            {
                                await dialogService.DisplayAlertAsync("Bluetooth", "Device failed to connect, please check if the device is On", "Ok");
                            }
                            else
                            {
                                RaisePropertyChanged(nameof(SelectedDevice));
                                //try
                                //{
                                //    BatteryToken = new CancellationTokenSource();
                                //    DeterminingBatteryLevel = true;
                                //    CheckBatteryLevelCommand.RaiseCanExecuteChanged();
                                //    BatteryLevel = double.NaN;
                                //    bleService.SendCommand(new GetProgramCommand(GetProgramCommand.BatteryVoltageLevel), BatteryToken.Token, 3, (a) =>
                                //    {
                                //        if (a is string blevel)
                                //        {
                                //            if (int.TryParse(blevel, out var r))
                                //            {
                                //                var percentage = bleService.ResolveBatteryLevel(r);
                                //                BatteryLevel = percentage;

                                //            }
                                //        }
                                //        DeterminingBatteryLevel = false;
                                //        CheckBatteryLevelCommand.RaiseCanExecuteChanged();

                                //    }, () =>
                                //    {
                                //        BatteryLevel = double.NaN;
                                //        DeterminingBatteryLevel = false;
                                //        CheckBatteryLevelCommand.RaiseCanExecuteChanged();
                                //    });

                                //}
                                //catch (Exception e)
                                //{
                                //    Crashes.TrackError(e);
                                //}
                                ////DeterminingSystemInfo = true;
                                //try
                                //{
                                //    SystemToken = new CancellationTokenSource();
                                //    DeviceSerialNumber = "";
                                //    bleService.SendCommand(new GetSystemCommand(GetSystemCommand.SystemSerialNumber), SystemToken.Token, 3, (a) =>
                                //      {
                                //          if (a is string serialNumber)
                                //          {
                                //              DeviceSerialNumber = serialNumber;
                                //          }

                                //      }, () =>
                                //     {
                                //         DeviceSerialNumber = "Failed To Fetch";
                                //     });
                                //    LoadCellSerialNumber = "";
                                //    bleService.SendCommand(new GetSystemCommand(GetSystemCommand.LoadCellSerialNumber), SystemToken.Token, 3, (a) =>
                                //    {
                                //        if (a is string serialNumber)
                                //        {
                                //            LoadCellSerialNumber = serialNumber;
                                //        }

                                //    }, () =>
                                //    {
                                //        LoadCellSerialNumber = "Failed To Fetch";
                                //    });
                                //    GeophoneOneSerialNumber = "";
                                //    bleService.SendCommand(new GetSystemCommand(GetSystemCommand.GeophoneOneSerialNumber), SystemToken.Token, 3, (a) =>
                                //    {
                                //        if (a is string serialNumber)
                                //        {
                                //            GeophoneOneSerialNumber = serialNumber;
                                //        }

                                //    }, () =>
                                //    {
                                //        GeophoneOneSerialNumber = "Failed To Fetch";
                                //    });
                                //    GeophoneTwoSerialNumber = "";
                                //    bleService.SendCommand(new GetSystemCommand(GetSystemCommand.GeophoneTwoSerialNumber), SystemToken.Token, 3, (a) =>
                                //    {
                                //        if (a is string serialNumber)
                                //        {
                                //            GeophoneTwoSerialNumber = serialNumber;
                                //        }

                                //    }, () =>
                                //    {
                                //        GeophoneTwoSerialNumber = "Failed To Fetch";
                                //    });
                                //    GeophoneThreeSerialNumber = "";
                                //    bleService.SendCommand(new GetSystemCommand(GetSystemCommand.GeophoneThreeSerialNumber), SystemToken.Token, 3, (a) =>
                                //    {
                                //        if (a is string serialNumber)
                                //        {
                                //            GeophoneThreeSerialNumber = serialNumber;
                                //        }

                                //    }, () =>
                                //    {
                                //        GeophoneThreeSerialNumber = "Failed To Fetch";
                                //    });

                                //}
                                //catch (Exception e)
                                //{
                                //    Crashes.TrackError(e);
                                //}

                                //try
                                //{
                                //    ProgramDate = "";
                                //    ProgramToken = new CancellationTokenSource();
                                //    bleService.SendCommand(new GetProgramCommand(GetProgramCommand.UpdateDate), ProgramToken.Token, 3, (a) =>
                                //    {
                                //        if (a is string date)
                                //        {
                                //            ProgramDate = date;
                                //        }

                                //    }, () =>
                                //    {
                                //        ProgramDate = "Failed To Fetch";
                                //    });
                                //    Version = "";
                                //    bleService.SendCommand(new GetProgramCommand(GetProgramCommand.Version), ProgramToken.Token, 3, (a) =>
                                //    {
                                //        if (a is string vs)
                                //        {
                                //            Version = vs;
                                //        }

                                //    }, () =>
                                //    {
                                //        Version = "Failed To Fetch";
                                //    });
                                //    DropCount = "";
                                //    bleService.SendCommand(new GetProgramCommand(GetProgramCommand.DropCount), ProgramToken.Token, 3, (a) =>
                                //    {
                                //        if (a is string dc)
                                //        {
                                //            DropCount = dc;
                                //        }

                                //    }, () =>
                                //    {
                                //        DropCount = "Failed To Fetch";
                                //    });
                                //}
                                //catch (Exception e)
                                //{
                                //    Crashes.TrackError(e);
                                //}


                            }
                        }
                        catch (Exception e)
                        {
                            // Crashes.TrackError(e);
                            Connecting = false;
                            ConnectDeviceCommand.RaiseCanExecuteChanged();
                        }
                    }
                    else
                    {
                        await dialogService.DisplayAlertAsync("Bluetooth", "Please check your Bluetooth Settings", "Ok");
                    }
                }
            }
        }
        private async void OnScanForDevicesCommand()
        {
            if (!ScanningForDevices)
            {
                ScanningForDevices = true;
                ScanForDevicesCommand.RaiseCanExecuteChanged();

                if (CrossBleAdapter.Current.Status == AdapterStatus.PoweredOn || CrossBleAdapter.Current.Status == AdapterStatus.Unsupported)
                {
                    AvailableDevices.Clear();
                    foundDevices.Clear();
                    try
                    {
                        bleService.ScanForDevicesCommand(TimeSpan.FromSeconds(10), device =>
                        {
                            if (device != null)
                            {
                                foundDevices.Add(device.Name, device);
                                AvailableDevices.Add(device.Name);
                            }
                        });
                        await Task.Delay(TimeSpan.FromSeconds(10));
                        ScanningForDevices = false;
                        ScanForDevicesCommand.RaiseCanExecuteChanged();
                    }
                    catch (Exception e)
                    {
                        // Crashes.TrackError(e);
                        ScanningForDevices = false;
                        ScanForDevicesCommand.RaiseCanExecuteChanged();
                    }

                }
                else
                {
                    await dialogService.DisplayAlertAsync("Bluetooth", "Please check your Bluetooth Settings", "Ok");
                }

            }
        }
        public ObservableCollection<string> AvailableDevices { get { return availableDevices; } set { SetProperty(ref availableDevices, value); } }
        public string SelectedDeviceKey
        {
            get
            {
                return selectedeviceKey;
            }
            set
            {
                SetProperty(ref selectedeviceKey, value);
                if (selectedeviceKey != null)
                {
                    if (foundDevices.TryGetValue(selectedeviceKey, out var device))
                    {
                        SelectedDevice = device;
                        bleService.SetSelectedDevice(SelectedDevice);
                    }
                }
            }
        }

        public CancellationTokenSource LedToken
        {
            get
            {
                return ledToken;
            }
            set
            {
                if (ledToken != null) ledToken.Cancel();
                SetProperty(ref ledToken, value);
            }
        }
        public Services.DeviceBluetoothLe.Device.IDevice SelectedDevice { get { return selectedDevice; } set { SetProperty(ref selectedDevice, value); } }
        public bool Connecting { get { return connecting; } set { SetProperty(ref connecting, value); } }
        public bool Disconnecting { get { return disconnecting; } set { SetProperty(ref disconnecting, value); } }
        public bool ScanningForDevices { get { return scanninfForDevices; } set { SetProperty(ref scanninfForDevices, value); } }
        public string CommandResponse { get { return commandResponse; } set { SetProperty(ref commandResponse, value); } }

        public DelegateCommand ScanForDevicesCommand { get; }

        public DelegateCommand ConnectDeviceCommand { get; }

        public DelegateCommand DisconnectDeviceCommand { get; }

        public DelegateCommand ToggleLedCommand { get; }
    }
}
