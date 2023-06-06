using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client.Web;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using EarchiveApi;
using Grpc.Core;

namespace UpdGrpcClientService.Framework
{
	public class UpdServiceClient : IDisposable
	{
		private GrpcChannel _channel;
		private EarchiveUpd.EarchiveUpdClient _earchiveUpdClient;

		public UpdServiceClient(
			string serviceUrl, 
			int servicePort)
		{
			var serviceAddress = $"https://{serviceUrl}:{servicePort}";
			var grpcChannelOptions = new GrpcChannelOptions
			{
				HttpHandler = new GrpcWebHandler(new HttpClientHandler())
			};

			_channel = GrpcChannel.ForAddress(serviceAddress, grpcChannelOptions);

			_earchiveUpdClient = new EarchiveUpd.EarchiveUpdClient(_channel);
		}

		public List<CounterpartyInfo> GetCounterparties(string nameSubstring)
		{
			var counterparties = new List<CounterpartyInfo>();

			var response = _earchiveUpdClient.GetCounterparties(new NameSubstring { NamePart = nameSubstring });

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

		public List<UpdResponseInfo> GetUpdCodes(long counterpartyId, long deliveryPointId, DateTime startDate, DateTime endDate)
		{
			var updCodes = new List<UpdResponseInfo>();

			var startDateUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
			var endDateUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

			var requestInfo = new UpdRequestInfo
			{
				CounterpartyId = counterpartyId,
				DeliveryPointId = deliveryPointId,
				StartDate = Timestamp.FromDateTime(startDateUtc),
				EndDate = Timestamp.FromDateTime(endDateUtc)
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
}
