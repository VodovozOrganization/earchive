using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace FileStorageService
{
	/// <summary>
	/// Хранилище изображений в S3
	/// </summary>
	public sealed class S3FileStorage : IS3FileStorage
	{
		/// <summary>
		/// Ключ пользовательских метаданных с id изображения (заголовок x-amz-meta-earchive-image-id)
		/// </summary>
		private const string OwnerMetadataKey = "earchive-image-id";

		private readonly IAmazonS3 _client;
		private readonly string _bucket;

		/// <summary>
		/// Создаёт клиент S3 по настройкам
		/// </summary>
		/// <param name="settings">Настройки подключения к S3</param>
		public S3FileStorage(S3Settings settings)
		{
			if(settings == null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			if(string.IsNullOrWhiteSpace(settings.BucketName))
			{
				throw new ArgumentException("BucketName is required", nameof(settings));
			}

			if(string.IsNullOrWhiteSpace(settings.AccessKey))
			{
				throw new ArgumentException("AccessKey is required", nameof(settings));
			}

			if(string.IsNullOrWhiteSpace(settings.SecretKey))
			{
				throw new ArgumentException("SecretKey is required", nameof(settings));
			}

			_bucket = settings.BucketName;

			var config = new AmazonS3Config();
			if(!string.IsNullOrWhiteSpace(settings.ServiceUrl))
			{
				config.ServiceURL = settings.ServiceUrl.TrimEnd('/');
				config.ForcePathStyle = true;
			}
			else if(!string.IsNullOrWhiteSpace(settings.Region))
			{
				config.RegionEndpoint = RegionEndpoint.GetBySystemName(settings.Region);
			}
			else
			{
				throw new ArgumentException("Укажите адрес хранилища S3.ServiceUrl или регион AWS S3.Region");
			}

			var credentials = new BasicAWSCredentials(settings.AccessKey, settings.SecretKey);
			_client = new AmazonS3Client(credentials, config);
		}

		/// <inheritdoc/>
		public async Task UploadAsync(string key, Stream data, string contentType, long ownerImageId)
		{
			if(string.IsNullOrWhiteSpace(key))
			{
				throw new ArgumentException("key is required", nameof(key));
			}

			if(data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			var request = new PutObjectRequest
			{
				BucketName = _bucket,
				Key = key,
				InputStream = data,
				ContentType = contentType,
				AutoCloseStream = false
			};
			request.Metadata.Add(OwnerMetadataKey, ownerImageId.ToString(CultureInfo.InvariantCulture));

			await _client.PutObjectAsync(request).ConfigureAwait(false);
		}

		/// <inheritdoc/>
		public async Task<byte[]> DownloadAsync(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
			{
				throw new ArgumentException("key is required", nameof(key));
			}

			using(var response = await _client.GetObjectAsync(_bucket, key).ConfigureAwait(false))
			using(var ms = new MemoryStream())
			{
				await response.ResponseStream.CopyToAsync(ms).ConfigureAwait(false);
				return ms.ToArray();
			}
		}

		/// <inheritdoc/>
		public async Task DeleteAsync(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
			{
				throw new ArgumentException("key is required", nameof(key));
			}

			await _client.DeleteObjectAsync(_bucket, key).ConfigureAwait(false);
		}

		/// <inheritdoc/>
		public async Task<S3ObjectInfo> GetInfoAsync(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
			{
				throw new ArgumentException("key is required", nameof(key));
			}

			try
			{
				var response = await _client.GetObjectMetadataAsync(_bucket, key).ConfigureAwait(false);
				var owner = response.Metadata[OwnerMetadataKey];

				return new S3ObjectInfo
				{
					OwnerImageId = long.TryParse(owner, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ownerId)
						? ownerId
						: (long?)null
				};
			}
			catch(AmazonS3Exception ex) when(ex.StatusCode == HttpStatusCode.NotFound)
			{
				return null;
			}
		}
	}
}
