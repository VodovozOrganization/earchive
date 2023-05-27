using EarchiveApi;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;
using System.Collections.Generic;
using NLog;
using Grpc.Net.Client;
using System.Net.Http;
using Grpc.Net.Client.Web;

namespace UpdGrpcClientService
{
	public class UpdServiceClient : IDisposable
	{
		private GrpcChannel _channel;
		private EarchiveUpd.EarchiveUpdClient _earchiveUpdClient;
		private readonly ILogger _logger;
		private HttpClient _httpClient;

		public bool IsConnectionActive => true; // _channel.State == ChannelState.Ready || _channel.State == ChannelState.Idle;

		public UpdServiceClient(string serviceAddress, int servicePort, ILogger logger)
		{
            //_logger = logger ?? throw new ArgumentNullException(nameof(logger));
            //_channel = new Channel(serviceAddress, servicePort, ChannelCredentials.Insecure);
            //         //_channel = new Channel("localhost", 5001, ChannelCredentials.SecureSsl);
            //         _earchiveUpdClient = new EarchiveUpd.EarchiveUpdClient(_channel);




            //var handler = new GrpcWebHandler(new HttpClientHandler());
            //_httpClient = new HttpClient(handler);
            ////_httpClient.DefaultRequestHeaders.Add("ApiKey", apiKey);

            //var options = new GrpcChannelOptions();
            //options.HttpClient = _httpClient;

            //var channel = GrpcChannel.ForAddress("https://localhost:7101", options);
            //_channel = channel;
            //_earchiveUpdClient = new EarchiveUpd.EarchiveUpdClient(_channel);

            var channel = GrpcChannel.ForAddress("https://earchive.vod.qsolution.ru:7101", new GrpcChannelOptions
            {
                HttpHandler = new GrpcWebHandler(new HttpClientHandler())
            });

            _earchiveUpdClient = new EarchiveUpd.EarchiveUpdClient(channel);
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

			_logger.Info(
				"Запрос поиска имени контрагента со значением подстроки \"{NameSubstring}\" вернул {CounterpartiesCount} результатов.",
				nameSubstring,
				counterparties.Count);

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

			_logger.Info(
				"Запрос поиска точек доставки для контрагента {CounterpartyName} (id = {CounterpartyId}) вернул {DeliveryPointsCount} результатов.",
				counterparty.Name,
				counterparty.Id,
				deliveryPoints.Count);

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

			_logger.Info(
				"Запрос поиска кодов УПД для контрагента id = {CounterpartyId} и точки доставки id = {DeliveryPointId}  вернул {UpdCodesCount} результатов.",
				counterpartyId,
				deliveryPointId,
				updCodes.Count);

			return updCodes;
		}

		public void Dispose()
		{
			_channel.ShutdownAsync();
		}
	}
}
