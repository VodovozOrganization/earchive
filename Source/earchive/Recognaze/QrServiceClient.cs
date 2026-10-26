using System;
using System.Collections.Generic;
using System.Net.Http;
using EarchiveQr;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using NLog;

namespace earchive
{
	/// <summary>
	/// Клиент gRPC-сервиса распознавания QR-кодов.
	/// Сервис использует движок WeChat QRCode (OpenCV wechat_qrcode) и умеет
	/// предварительно улучшать изображение (контраст, резкость, поворот).
	/// </summary>
	public sealed class QrServiceClient : IDisposable
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		// После сетевой ошибки не дергаем сервис некоторое время, чтобы пакетное
		// распознавание не ждало таймаут на каждой странице.
		private static readonly TimeSpan FailureCooldown = TimeSpan.FromMinutes(1);

		private const int MaxMessageSize = 32 * 1024 * 1024;

		private readonly GrpcChannel _channel;
		private readonly QrRecognition.QrRecognitionClient _client;
		private readonly TimeSpan _timeout;
		private readonly object _sync = new object();

		private DateTime _unavailableUntilUtc = DateTime.MinValue;
		private string _lastError;
		private bool _disposed;

		public QrServiceClient(string host, int port, bool useTls = false, int timeoutSeconds = 15)
		{
			if (string.IsNullOrWhiteSpace(host))
				throw new ArgumentException("Не задан адрес сервиса распознавания QR.", nameof(host));

			if (port <= 0 || port > 65535)
				throw new ArgumentOutOfRangeException(nameof(port), "Некорректный порт сервиса распознавания QR.");

			if (timeoutSeconds <= 0)
				timeoutSeconds = 15;

			var scheme = useTls ? "https" : "http";
			host = host.Trim();

			if (host.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
			{
				scheme = "http";
				host = host.Substring("http://".Length);
			}
			else if (host.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
			{
				scheme = "https";
				host = host.Substring("https://".Length);
			}

			host = host.TrimEnd('/');

			Address = string.Format("{0}://{1}:{2}", scheme, host, port);

			var channelOptions = new GrpcChannelOptions
			{
				HttpHandler = new GrpcWebHandler(new HttpClientHandler()),
				MaxSendMessageSize = MaxMessageSize,
				MaxReceiveMessageSize = MaxMessageSize
			};

			_channel = GrpcChannel.ForAddress(Address, channelOptions);
			_client = new QrRecognition.QrRecognitionClient(_channel);
			_timeout = TimeSpan.FromSeconds(timeoutSeconds);

			Logger.Info("Сервис распознавания QR: {0}", Address);
		}

		/// <summary>Адрес подключения к сервису.</summary>
		public string Address { get; private set; }

		/// <summary>false, если сервис недавно был недоступен и запросы временно не отправляются.</summary>
		public bool IsServiceAvailable
		{
			get
			{
				lock (_sync)
					return DateTime.UtcNow >= _unavailableUntilUtc;
			}
		}

		/// <summary>Текст последней ошибки обмена с сервисом.</summary>
		public string LastError
		{
			get
			{
				lock (_sync)
					return _lastError;
			}
		}

		/// <summary>
		/// Отправляет изображение на распознавание.
		/// </summary>
		/// <returns>
		/// Список текстов найденных QR-кодов. Пустой список — QR-код не найден.
		/// null — сервис недоступен (можно использовать локальный распознаватель).
		/// </returns>
		public IReadOnlyList<string> Recognize(byte[] image)
		{
			if (image == null || image.Length == 0)
				return Array.Empty<string>();

			if (_disposed)
				return null;

			lock (_sync)
			{
				if (DateTime.UtcNow < _unavailableUntilUtc)
					return null;
			}

			try
			{
				var request = new QrRecognizeRequest
				{
					Image = Google.Protobuf.ByteString.CopyFrom(image),
					ImageFormat = "jpeg"
				};

				var response = _client.Recognize(request, deadline: DateTime.UtcNow.Add(_timeout));

				var texts = new List<string>();
				foreach (var code in response.Codes)
				{
					if (!string.IsNullOrWhiteSpace(code.Text))
						texts.Add(code.Text);
				}

				lock (_sync)
					_lastError = null;

				return texts;
			}
			catch (RpcException ex)
			{
				var serviceFailure = ex.StatusCode == StatusCode.Unavailable
					|| ex.StatusCode == StatusCode.DeadlineExceeded
					|| ex.StatusCode == StatusCode.Internal;

				RegisterFailure(serviceFailure, ex.Status.Detail);
				return null;
			}
			catch (Exception ex)
			{
				RegisterFailure(true, ex.Message);
				return null;
			}
		}

		private void RegisterFailure(bool serviceFailure, string message)
		{
			lock (_sync)
			{
				_lastError = message;
				if (serviceFailure)
					_unavailableUntilUtc = DateTime.UtcNow.Add(FailureCooldown);
			}

			Logger.Warn("Ошибка распознавания QR через сервис {0}: {1}", Address, message);
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;

			try
			{
				_channel.Dispose();
			}
			catch (Exception ex)
			{
				Logger.Warn(ex, "Ошибка при остановке gRPC-канала распознавания QR.");
			}
		}
	}
}
