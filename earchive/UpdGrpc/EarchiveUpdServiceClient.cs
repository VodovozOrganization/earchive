using EarchiveApi;
using GLib;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
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

			while (response.ResponseStream.MoveNext().Result)
			{
				var counterparty = response.ResponseStream.Current;
				counterparties.Add(counterparty);
			}
			return counterparties;
		}

		public List<DeliveryPointInfo> GetDeliveryPoints(CounterpartyInfo counterparty) 
		{ 
			var deliveryPoints = new List<DeliveryPointInfo>();

			var response = _earchiveUpdClient.GetAddresses(counterparty);

			while (response.ResponseStream.MoveNext().Result)
			{
				var address = response.ResponseStream.Current;
				deliveryPoints.Add(address);
			}

			return deliveryPoints;
		}

		public List<UpdResponseInfo> GetUpdCodes(int counterpartyId, int deliveryPointId, DateTime startDate, DateTime endDate)
		{
			var updCodes = new List<UpdResponseInfo>();
			var requestInfo = new UpdRequestInfo
			{
				CounterpartyId = counterpartyId,
				DeliveryPointId = deliveryPointId,
				StartDate = Timestamp.FromDateTime(DateTime.Now.AddYears(-2)),
				EndDate = Timestamp.FromDateTime(DateTime.Now)
			};

            var response = _earchiveUpdClient.GetUpdCode(requestInfo);

            while (response.ResponseStream.MoveNext().Result)
            {
                var updCode = response.ResponseStream.Current;
                updCodes.Add(updCode);
            }

            return updCodes;
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
