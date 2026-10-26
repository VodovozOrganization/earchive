using System.Threading.Tasks;
using EarchiveQr;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Earchive.QrService
{

    /// <summary>
    /// gRPC-обёртка над распознавателем WeChat QR.
    /// </summary>
    public sealed class QrRecognitionGrpcService : QrRecognition.QrRecognitionBase
    {
        private readonly WeChatQrRecognizer _recognizer;
        private readonly ILogger<QrRecognitionGrpcService> _logger;

        public QrRecognitionGrpcService(
            WeChatQrRecognizer recognizer,
            ILogger<QrRecognitionGrpcService> logger)
        {
            _recognizer = recognizer;
            _logger = logger;
        }

        public override Task<QrRecognizeResponse> Recognize(
            QrRecognizeRequest request,
            ServerCallContext context)
        {
            var response = new QrRecognizeResponse();

            if (request.Image.IsEmpty)
            {
                _logger.LogWarning("Получен запрос с пустым изображением.");
                return Task.FromResult(response);
            }

            byte[] imageBytes = request.Image.ToByteArray();
            var codes = _recognizer.Recognize(imageBytes, context.CancellationToken);

            foreach (var text in codes)
                response.Codes.Add(new QrCode { Text = text });

            _logger.LogInformation(
                "Распознавание QR: изображение {Size} байт, найдено кодов: {Count}.",
                imageBytes.Length,
                response.Codes.Count);

            return Task.FromResult(response);
        }
    }

}
