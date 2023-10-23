using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BaseParametersService;
using MySqlConnector;
using QSProjectsLib;
using QSWidgetLib;

namespace earchive
{
	public class DocFieldInfo
	{
		public int ID;
		public int ListStoreColumn;
		public string Name;
		public string DBName;
		public string Type;
		public bool Display;
		public bool Search;
	}

	public class DocumentInformation
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		public int TypeId {get; private set;}
		public string Name { get; private set; }
		public string TypeName {get; private set;}
		public string DBTableName{get; private set;}
		public bool DBTableExsist{get; private set;}
		public string Description{get; private set;}
		public int CountExtraFields{get; private set;}
		public List<DocFieldInfo> FieldsList{get; private set;}
		public RecognizeTemplate Template{get; private set;}

		public DocumentInformation(int Id)
		{
			TypeId = Id;
			GetDocInformation ();
			if(DBTableExsist)
				GetFieldsInformation ();
		}

		public DocumentInformation (string typeName)
		{
			TypeName = typeName;
			GetDocumentInformationByTypeName ();
			if (DBTableExsist)
				GetFieldsInformation ();
		}

		public void GetDocumentInformationByTypeName()
		{
			QSMain.CheckConnectionAlive();
			logger.Info("Запрос типа документа " + TypeName + "...");
			string sql = "SELECT doc_types.* FROM doc_types " +
				"WHERE doc_types.type_name = @type_name LIMIT 1";
			MySqlDataReader rdr = null;
			try {
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@type_name", TypeName);
				rdr = cmd.ExecuteReader();

				if (!rdr.Read())
					return;
				
				TypeId = rdr.GetInt32("id");

				Name = rdr["name"].ToString();
				if (rdr["table_name"] == DBNull.Value)
					DBTableExsist = false;
				else {
					DBTableName = rdr["table_name"].ToString ();
					DBTableExsist = true;
				}

				if (rdr["description"] != DBNull.Value)
					Description = rdr.GetString("description");

				if (rdr["template"] != DBNull.Value) {
					string str = rdr.GetString("template");
					Template = RecognizeTemplate.Load(str);
				}

				rdr.Close();
				logger.Info ("Ok");
			} catch(Exception ex) {
				logger.Warn(ex, "Ошибка получения информации о типе документа!");
			}
			finally{
				rdr?.Close();
			}
		}

		private void GetDocInformation()
		{
			QSMain.CheckConnectionAlive();
			logger.Info("Запрос типа документа №" + TypeId +"...");
			string sql = "SELECT doc_types.* FROM doc_types " +
				"WHERE doc_types.id = @id";
			try
			{
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@id", TypeId);
				MySqlDataReader rdr = cmd.ExecuteReader();
				
				if(!rdr.Read())
					return;
				
				Name = rdr["name"].ToString ();
				if(rdr["table_name"] == DBNull.Value)
					DBTableExsist = false;
				else
				{
					DBTableName = rdr["table_name"].ToString ();
					DBTableExsist = true;
				}

				TypeName = rdr ["type_name"].ToString();

				if(rdr["description"] != DBNull.Value)
					Description = rdr.GetString ("description");

				if(rdr["template"] != DBNull.Value)
				{
					string str = rdr.GetString("template");
					Template = RecognizeTemplate.Load(str);
				}
				
				rdr.Close();
				logger.Info("Ok");
			}
			catch (Exception ex)
			{
				logger.Warn(ex, "Ошибка получения информации о типе документа!");
			}
		}

		void GetFieldsInformation()
		{
			//Очистка
			FieldsList = new List<DocFieldInfo>();
			CountExtraFields = 0;
			
			//Загрузка информации о дополнительных полях
			QSMain.CheckConnectionAlive();
			System.Data.DataTable schema = QSMain.connectionDB.GetSchema("Columns", new string[4] { null, QSMain.connectionDB.Database, "extra_" + DBTableName, null});
			
			string  sql = "SELECT * FROM extra_fields WHERE doc_type_id = @id";
			
			MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
			cmd.Parameters.AddWithValue ("@id", TypeId);
			MySqlDataReader rdr = cmd.ExecuteReader ();
			
			while(rdr.Read ())
			{
				DocFieldInfo item = new DocFieldInfo();
				item.ID = rdr.GetInt32 ("id");
				item.Name = rdr.GetString ("name");
				item.DBName = rdr.GetString ("db_name");
				item.Display = rdr.GetBoolean ("display");
				item.Search = rdr.GetBoolean ("search");

				foreach (System.Data.DataRow row in schema.Rows)
				{
					if(row["COLUMN_NAME"].ToString () == item.DBName)
						item.Type = row["DATA_TYPE"].ToString ();
				}
				CountExtraFields++;
				FieldsList.Add (item);
			}
			rdr.Close ();
		}
	}

	public class Document : DocumentInformation
	{
		private const string _legalInnRegEx = @"^[0-9]{10}$";
		private const string _naturalInnRegEx = @"^[0-9]{12}$";

		private static IBaseParametersProvider _baseParametersProvider = new BaseParametersProvider();

		public Dictionary<int, object> FieldValues;
		public Dictionary<int, float> FieldConfidence;

		private string _docNumber;
		private float _docNumberConfidence;
		private DateTime _docDate;
		private float _docDateConfidence;
		private string _docInn;
		private float _docInnConfidence;
		private int _contractDocumentTypeId;

		public Document (string typeName) : base (typeName)
		{
			Init();
		}

		public Document (int TypeId) : base(TypeId)
		{
			Init();
		}

		#region Свойства

		public string DocNumber
		{
			get => _docNumber;
			set => _docNumber = value;
		}

		public float DocNumberConfidence
		{
			get => _docNumberConfidence;
			set => _docNumberConfidence = value;
		}

		public DateTime DocDate
		{
			get => _docDate;
			set => _docDate = value;
		}
		public float DocDateConfidence
		{
			get => _docDateConfidence;
			set => _docDateConfidence = value;
		}

		public string DocInn
		{
			get => _docInn;
			set => _docInn = value;
		}

		public float DocInnConfidence
		{
			get => _docInnConfidence;
			set => _docInnConfidence = value;
		}

		#endregion Свойства

		private void Init()
		{
			FieldValues = new Dictionary<int, object> ();
			FieldConfidence = new Dictionary<int, float> ();
			DocNumber = "";
			DocNumberConfidence = -1;
			DocDateConfidence = -1;
			DocInn = "";
			DocInnConfidence = -1;

			_contractDocumentTypeId = _baseParametersProvider.ContractDocTypeId;

			if (DBTableExsist) {
				//FIXME Возможно в этом случае объекты выше не надо создавать
				foreach (DocFieldInfo Field in FieldsList) {
					FieldValues.Add (Field.ID, null);
					FieldConfidence.Add (Field.ID, -1);
				}
			}
		}

		public bool CanSave{
		get{
				bool numberIsOk = DocNumber != "";
				bool dateIsOk = DocDate.Year != 1;
				bool innIsOk = 
					TypeId != _contractDocumentTypeId
					|| InnIsValid();

				return numberIsOk && dateIsOk && innIsOk;
			}
		}

		public bool InnIsValid()
		{
			bool isValid = Regex.IsMatch(DocInn, _legalInnRegEx)
				|| Regex.IsMatch(DocInn, _naturalInnRegEx);

			return isValid;
		}

		public DocState State{
			get{
				DocState temp;

				float[] Conf =
					TypeId == _contractDocumentTypeId
					? new float[] { DocNumberConfidence, DocDateConfidence, DocInnConfidence }
					: new float[] {DocNumberConfidence, DocDateConfidence };
				//FIXME Добавить в обработку значения дополнительных полей.
				float Min = 5;

				foreach(float val in Conf)
				{
					if(val < Min)
						Min = val;
				}

				if(Min >= 0.8 && CanSave)
					temp = DocState.Good;
				else if(Min >= 0)
					temp = DocState.Attention;
				else if (Min >= -1)
					temp = DocState.New;
				else 
					temp = DocState.Bad;

				if (TypeId < 0) {
					temp = DocState.Bad;
				}
				return temp;
			}
		}
	}

	public enum DocState {
		New,
		Good,
		Bad,
		Attention};

	public class DocumentImage
	{
		public int Id { get; set; }
		public Gdk.Pixbuf Image { get; set; }
		public int Order { get; set; }
		public string Type { get; set; }
		public long Size { get; set; }
		public byte[] File { get; set; }
		public bool IsChanged { get; set; }
		public ImageViewer Widget { get; set; }
	}
	
}
