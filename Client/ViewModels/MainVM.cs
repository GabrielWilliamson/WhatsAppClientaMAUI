using BaileysCSharp.Core.Events;
using BaileysCSharp.Core.Helper;
using BaileysCSharp.Core.Logging;
using BaileysCSharp.Core.Models;
using BaileysCSharp.Core.Models.Sending.NonMedia;
using BaileysCSharp.Core.NoSQL;
using BaileysCSharp.Core.Sockets;
using BaileysCSharp.Core.Types;
using BaileysCSharp.Exceptions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.ViewModels
{
    public partial class MainVM : ObservableObject
    {
        [ObservableProperty]
        public string connectionStatus = "Disconnect";

        [ObservableProperty]
        public string qrCodeBase64;

        [ObservableProperty]
        public string sms;

        [ObservableProperty]
        public int code;

        [ObservableProperty]
        public string number;

        private WASocket socket;

        [RelayCommand]
        public void Connect()
        {
            ConnectionStatus = "Connecting...";

            var config = new SocketConfig()
            {
                SessionName = "27665458845745065",
            };

            var credsFile = Path.Join(config.CacheRoot, $"creds.json");
            AuthenticationCreds? authentication = null;
            if (File.Exists(credsFile))
            {
                authentication = AuthenticationCreds.Deserialize(File.ReadAllText(credsFile));
            }
            authentication = authentication ?? AuthenticationUtils.InitAuthCreds();

            BaseKeyStore keys = new FileKeyStore(config.CacheRoot);

            config.Logger.Level = LogLevel.Raw;
            config.Auth = new AuthenticationState()
            {
                Creds = authentication,
                Keys = keys
            };

            socket = new WASocket(config);


            socket.EV.Connection.Update += Connection_Update;
            socket.EV.Auth.Update += Auth_Update;

            socket.MakeSocket();
        }

        private void Auth_Update(object? sender, AuthenticationCreds e)
        {
            lock (this)
            {
                var credsFile = Path.Join(socket.SocketConfig.CacheRoot, "creds.json");
                var json = AuthenticationCreds.Serialize(e);
                File.WriteAllText(credsFile, json);
            }
        }

        private void Connection_Update(object? sender, ConnectionState e)
        {
            var Connection = e;


            if (Connection.QR != null)
            {
                ConnectionStatus = "Scan QR Code please";
                UpdateQRCode(Connection.QR);
            }


            if (Connection.Connection == WAConnectionState.Close)
            {
                if (Connection.LastDisconnect.Error is Boom boom && boom.Data?.StatusCode != (int)DisconnectReason.LoggedOut)
                {
                    try
                    {
                        Thread.Sleep(1000);
                        socket.MakeSocket();
                    }
                    catch (Exception)
                    {
                        Application.Current!.MainPage!.DisplayAlert("Error", "Error", "Cancelar");
                    }
                }
                else
                {
                    ConnectionStatus = "Disconnect";
                }
            }


            if (Connection.Connection == WAConnectionState.Open)
            {
                ConnectionStatus = "Connected";
            }
        }

        private void UpdateQRCode(string qrData)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeBytes = qrCode.GetGraphic(20);
            QrCodeBase64 = $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
        }

        [RelayCommand]
        public async Task Send()
        {
            string Phone = $"{Code}{Number}@s.whatsapp.net";


            try
            {
                await socket.SendMessage(Phone, new TextMessageContent()
                {
                    Text = Sms
                });
                await Application.Current!.MainPage!.DisplayAlert("Success", "Send Success", "Ok");

            }
            catch (Exception ex)
            {

                await Application.Current!.MainPage!.DisplayAlert("Error", ex.Message, "Ok");

            }
        }




    }
}
