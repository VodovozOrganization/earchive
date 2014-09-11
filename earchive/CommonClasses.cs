using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using QSProjectsLib;
using QSWidgetLib;

namespace earchive
{
	class DocFieldInfo
	{
		public int ID;
		public int ListStoreColumn;
		public string Name;
		public string DBName;
		public string Type;
		public bool Display;
		public bool Search;
	}

	class DocumentInformation
	{
		public int TypeId {get; private set;}
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

		private void GetDocInformation()
		{
			MainClass.StatusMessage("Запрос типа документа №" + TypeId +"...");
			string sql = "SELECT doc_types.* FROM doc_types " +
				"WHERE doc_types.id = @id";
			try
			{
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@id", TypeId);
				MySqlDataReader rdr = cmd.ExecuteReader();
				
				if(!rdr.Read())
					return;
				
				TypeName = rdr["name"].ToString ();
				if(rdr["table_name"] == DBNull.Value)
					DBTableExsist = false;
				else
				{
					DBTableName = rdr["table_name"].ToString ();
					DBTableExsist = true;
				}

				if(rdr["description"] != DBNull.Value)
					Description = rdr.GetString ("description");

				if(rdr["template"] != DBNull.Value)
				{
					string str = rdr.GetString("template");
					Template = RecognizeTemplate.Load(str);
				}
				
				rdr.Close();
				MainClass.StatusMessage("Ok");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка получения информации о типе документа!");
			}
		}

		void GetFieldsInformation()
		{
			//Очистка
			FieldsList = new List<DocFieldInfo>();
			CountExtraFields = 0;
			
			//Загрузка информации о дополнительных полях
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

	class Document : earchive.DocumentInformation
	{
		public Dictionary<int, object> FieldValues;
		public Dictionary<int, float> FieldConfidence;
		public string DocNumber;
		public float DocNumberConfidence;
		public DateTime DocDate;
		public float DocDateConfidence;

		public Document (int TypeId) : base(TypeId)
		{
			FieldValues = new Dictionary<int, object>();
			FieldConfidence = new Dictionary<int, float>();
			DocNumber = "";
			DocNumberConfidence = -1;
			DocDateConfidence = -1;
			if(DBTableExsist)
			{
				//FIXME Возможно в этом случае объекты выше не надо создавать
				foreach (DocFieldInfo Field in FieldsList)
				{
					FieldValues.Add (Field.ID, null);
					FieldConfidence.Add(Field.ID, -1);
				}
			}
		}

		public bool CanSave{
		get{
				bool Numberok = DocNumber != "";
				bool Dateok = DocDate.Year != 1;
				return Numberok && Dateok;
			}
		}

		public DocState State{
			get{
				DocState temp;
				float[] Conf = new float[] {DocNumberConfidence, DocDateConfidence};
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
				return temp;
			}
		}
	}

	public enum DocState {
		New,
		Good,
		Bad,
		Attention};

	class DocumentImage
	{
		public int id;
		public Gdk.Pixbuf Image;
		public int order;
		public string type;
		public long size;
		public byte[] file;
		public bool Changed;
		public ImageViewer Widget;
	}
	
}
