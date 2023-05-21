using EarchiveApi;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;
using System.Collections.Generic;
using NLog;

namespace UpdGrpcClientService
{
    public class UpdServiceClient : IDisposable
    {
        //private static Logger logger = LogManager.GetCurrentClassLogger();

        private Channel _channel;
        private EarchiveUpd.EarchiveUpdClient _earchiveUpdClient;
        private readonly string serviceAddress;
        private readonly ILogger _logger;

        public bool IsConnectionActive => _channel.State == ChannelState.Ready || _channel.State == ChannelState.Idle;

        public UpdServiceClient(string serviceAddress, ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _channel = new Channel($"{serviceAddress}", ChannelCredentials.Insecure);
            _earchiveUpdClient = new EarchiveUpd.EarchiveUpdClient(_channel);
        }

        public List<CounterpartyInfo> GetCounterparties(string nameSubstring)
        {
            var counterparties = new List<CounterpartyInfo>();

            var response = _earchiveUpdClient.GetCounterparites(new NameSubstring { NamePart = nameSubstring });

            while (IsConnectionActive && response.ResponseStream.MoveNext().Result)
            {
                var counterparty = response.ResponseStream.Current;
                counterparties.Add(counterparty);
            }

            _logger.Info($"Запрос поиска имени контрагента со значением подстроки \"{nameSubstring}\" вернул {counterparties.Count} результатов.");
            return counterparties;
        }

        public List<DeliveryPointInfo> GetDeliveryPoints(CounterpartyInfo counterparty)
        {
            var deliveryPoints = new List<DeliveryPointInfo>();

            var response = _earchiveUpdClient.GetAddresses(counterparty);

            while (IsConnectionActive && response.ResponseStream.MoveNext().Result)
            {
                var address = response.ResponseStream.Current;
                deliveryPoints.Add(address);
            }

            _logger.Info($"Запрос поиска точек доставки для контрагента {counterparty.Name} (id = {counterparty.Id}) вернул {deliveryPoints.Count} результатов.");
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

            while (IsConnectionActive && response.ResponseStream.MoveNext().Result)
            {
                var updCode = response.ResponseStream.Current;
                updCodes.Add(updCode);
            }

            _logger.Info($"Запрос поиска кодов УПД для контрагента id = {counterpartyId} и точки доставки id = {deliveryPointId}  вернул {updCodes.Count} результатов.");
            return updCodes;
        }

        public void Dispose()
        {
            _channel.ShutdownAsync();
        }
    }
}
