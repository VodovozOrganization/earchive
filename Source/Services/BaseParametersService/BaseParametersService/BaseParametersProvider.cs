using QS.BaseParameters;
using QSProjectsLib;
using System;
using System.Collections.Generic;

namespace BaseParametersService
{
	public class BaseParametersProvider : IBaseParametersProvider
	{
		private static readonly ParametersService _parametersService = new ParametersService(QSMain.ConnectionDB);

		private readonly Dictionary<string, string> _allParameters = new Dictionary<string, string>();

		public BaseParametersProvider()
		{
			_allParameters = _parametersService.All;
		}

		public int ContractDocTypeId => GetContractDocTypeId();

		private int GetContractDocTypeId()
		{
			var contractDocTypeIdKey = "contract_doc_type_id";

			if (_allParameters == null || !_allParameters.ContainsKey(contractDocTypeIdKey))
			{
				throw new InvalidProgramException("Не найден параметр базы данных, устанавливающий значения Id типа документов \"Договор\"");
			}

			if (int.TryParse(_allParameters[contractDocTypeIdKey], out int id))
			{
				return id;
			}

			throw new InvalidProgramException("В таблице параметров БД значения Id типа документов \"Договор\" установлено в неверном формате");
		}
	}
}
