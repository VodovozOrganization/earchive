using QS.BaseParameters;
using QSProjectsLib;
using System;
using System.Collections.Generic;

namespace BaseParametersService
{
	public class BaseParametersProvider : IBaseParametersProvider
	{
		private static readonly ParametersService _parametersService = new ParametersService(QSMain.ConnectionDB);

		private readonly Dictionary<string, string> _allParameters;

		public BaseParametersProvider()
		{
			_allParameters = _parametersService.All;
		}

		public int ContractDocTypeId => GetContractDocTypeId();

		public int UpdDocTypeId => GetUpdDocTypeId();

		/// <summary>
		/// Ключ доступа к S3 (параметр S3.AccessKey, обязательный)
		/// </summary>
		public string S3AccessKey => GetRequiredString("S3.AccessKey");

		/// <summary>
		/// Секретный ключ S3 (параметр S3.SecretKey, обязательный)
		/// </summary>
		public string S3SecretKey => GetRequiredString("S3.SecretKey");

		/// <summary>
		/// Бакет S3 с изображениями (параметр S3.Bucket, обязательный)
		/// </summary>
		public string S3Bucket => GetRequiredString("S3.Bucket");

		/// <summary>
		/// Адрес S3-хранилища (параметр S3.ServiceUrl).
		/// Не задаётся, если используется регион AWS
		/// </summary>
		public string S3ServiceUrl => GetOptionalString("S3.ServiceUrl");

		/// <summary>
		/// Регион AWS (параметр S3.Region).
		/// Не задаётся, если указан адрес хранилища
		/// </summary>
		public string S3Region => GetOptionalString("S3.Region");

		private int GetContractDocTypeId()
		{
			const string key = "contract_doc_type_id";

			if (_allParameters == null || !_allParameters.ContainsKey(key))
			{
				throw new InvalidProgramException(
					"Не найден параметр базы данных, устанавливающий значение Id типа документов \"Договор\"");
			}

			if (int.TryParse(_allParameters[key], out int id))
			{
				return id;
			}

			throw new InvalidProgramException(
				"В таблице параметров БД значение Id типа документов \"Договор\" установлено в неверном формате");
		}

		private int GetUpdDocTypeId()
		{
			const string key = "upd_doc_type_id";

			if (_allParameters == null || !_allParameters.ContainsKey(key))
			{
				throw new InvalidProgramException(
					"Не найден параметр базы данных, устанавливающий значение Id типа документов \"УПД\"");
			}

			if (int.TryParse(_allParameters[key], out int id))
			{
				return id;
			}

			throw new InvalidProgramException(
				"В таблице параметров БД значение Id типа документов \"УПД\" установлено в неверном формате");
		}

		/// <summary>
		/// Значение обязательного строкового параметра
		/// </summary>
		/// <param name="key">Имя параметра в base_parameters</param>
		/// <returns>Значение без пробелов по краям</returns>
		/// <exception cref="InvalidProgramException">Параметр не задан или пустой</exception>
		private string GetRequiredString(string key)
		{
			if (_allParameters == null || !_allParameters.ContainsKey(key)
				|| string.IsNullOrWhiteSpace(_allParameters[key]))
			{
				throw new InvalidProgramException(
					string.Format("Не найден обязательный параметр БД \"{0}\" (S3)", key));
			}

			return _allParameters[key].Trim();
		}

		/// <summary>
		/// Значение необязательного строкового параметра
		/// </summary>
		/// <param name="key">Имя параметра в base_parameters</param>
		/// <returns>Значение без пробелов по краям или null, если параметр не задан или пустой</returns>
		private string GetOptionalString(string key)
		{
			if (_allParameters == null || !_allParameters.ContainsKey(key))
			{
				return null;
			}

			var value = _allParameters[key];
			return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
		}
	}
}