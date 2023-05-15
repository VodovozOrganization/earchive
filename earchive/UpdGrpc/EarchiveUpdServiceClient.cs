using EarchiveApi;
using GLib;
using Google.Protobuf;
using Grpc.Core;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ubiety.Dns.Core;
using ZXing.Aztec.Internal;

namespace earchive.UpdGrpc
{
    public class EarchiveUpdServiceClient : IDisposable
    {
        private static string ServiceAddress = "localhost";
        private uint ServicePort = 5000;
        private Channel _channel;
        private EarchiveUpd.EarchiveUpdClient _earchiveUpdClient;

        public bool IsNotificationActive => _channel.State == ChannelState.Ready;

        public event EventHandler<ConnectionStateEventArgs> ChannelStateChanged;

        public EarchiveUpdServiceClient()
        {
            _channel = new Channel($"{ServiceAddress}:{ServicePort}", ChannelCredentials.Insecure);
            _earchiveUpdClient = new EarchiveUpd.EarchiveUpdClient(_channel);
        }

        public List<CounterpartyInfo> GetCounterparties(string nameSubstring)
        {
            var counterparties = new List<CounterpartyInfo>();

            var response = _earchiveUpdClient.GetCounterparites(new NameSubstring { NamePart = nameSubstring });
            var responseReaderTask = Task.Run(async () =>
            {
                while (await response.ResponseStream.MoveNext())
                {
                    var counterparty = response.ResponseStream.Current;
                    counterparties.Add(counterparty);
                }
            });
            return counterparties;
        }

        private void Connect()
        {
            _channel = new Channel($"{ServiceAddress}:{ServicePort}", ChannelCredentials.Insecure);
            _earchiveUpdClient = new EarchiveUpd.EarchiveUpdClient(_channel);


            //notificationClient = new NotificationService.NotificationServiceClient(channel);

            //var request = new NotificationSubscribeRequest { Extension = extension };
            //var response = notificationClient.Subscribe(request);
            //var watcher = new NotificationConnectionWatcher(channel, OnChanalStateChanged);

            //var responseReaderTask = Task.Run(async () =>
            //{
            //    while (await response.ResponseStream.MoveNext(token))
            //    {
            //        FailSince = null;
            //        var message = response.ResponseStream.Current;
            //        logger.Debug($"extension:{extension} Received:{message}");
            //        OnAppearedMessage(message);
            //    }
            //    logger.Warn($"Соединение с NotificationService[{extension}] завершено.");
            //}, token).ContinueWith(task =>
            //{
            //    if (task.IsCanceled || (task.Exception?.InnerException as RpcException)?.StatusCode == StatusCode.Cancelled)
            //    {
            //        logger.Info($"Соединение с NotificationService[{extension}] отменено.");
            //    }
            //    else if (task.IsFaulted)
            //    {
            //        if (FailSince == null)
            //            FailSince = DateTime.Now;
            //        var failedTime = (DateTime.Now - FailSince).Value;
            //        if (failedTime.Seconds < 10)
            //            Thread.Sleep(1000);
            //        else if (failedTime.Minutes < 10)
            //            Thread.Sleep(4000);
            //        else
            //            Thread.Sleep(30000);
            //        logger.Error(task.Exception);
            //        logger.Info($"Соединение с NotificationService[{extension}] разорвано... Пробуем соединиться.");
            //        Connect();
            //    }
            //})
            //    ;
        }

        public void Dispose()
        {
            _channel.ShutdownAsync();
        }
    }

    public class ConnectionStateEventArgs : EventArgs
    {
        public ConnectionStateEventArgs(ChannelState channelState)
        {
            ChannelState = channelState;
        }

        public ChannelState ChannelState { get; }
    }
}
