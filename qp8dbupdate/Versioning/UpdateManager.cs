﻿using System;
using System.Data;
using System.Data.SqlClient;
using qp8dbupdate.Versioning;

namespace qp8dbupdate
{
    public class UpdateManager
    {
        #region SQL

        /// <summary>
        /// Создать таблицу, если надо
        /// </summary>
        static string CreateTableQuery = @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[XML_DB_UPDATE]')
AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
	CREATE TABLE [dbo].[XML_DB_UPDATE](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[Applied] [datetime] NOT NULL,
		[Hash] [nvarchar](100) NOT NULL,
		[FileName] [nvarchar](300) NULL,
		[USER_ID] [int] NOT NULL,
		[Body] [nvarchar](max) NULL,
		[Version] [nvarchar](10) NULL,
	 CONSTRAINT [PK_XML_DB_UPDATE] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

END";

        public static string TableQuery
        {
            get { return UpdateManager.CreateTableQuery; }
        }
        /// <summary>
        /// Сделать запись
        /// </summary>
        static string InsertLogEntryCommandText = @"INSERT INTO [dbo].[XML_DB_UPDATE]
           ([Applied]
           ,[Hash]
           ,[FileName]
           ,[USER_ID]
           ,[Body]
           ,[Version])
     VALUES (@applied,@hash, @filename, @userId, @body, @version)";

        /// <summary>
        /// Проверить, что обноврение не производилось
        /// </summary>
        static string GetUpdatesQuery = @"select top 1 [Applied],[FileName],[USER_ID] from [dbo].[XML_DB_UPDATE] x where x.[Hash] = @hash";

        #endregion

        private SqlConnection _connection;
        
        public UpdateManager(SqlConnection connection)
        {
            _connection = connection;
        }

        public void EnsureTableExists()
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            SqlCommand cmd = new SqlCommand(CreateTableQuery, _connection);
            cmd.ExecuteNonQuery();
        }

        public void SaveLogEntry(DBUpdateLogEntry entry)
        {
            if (entry == null) throw new ArgumentNullException("entry");
            ValidateEntry(entry);

            using (var cmd = new SqlCommand(InsertLogEntryCommandText, _connection))
            {
                cmd.Parameters.AddWithValue("@applied", entry.Applied);
                cmd.Parameters.AddWithValue("@hash", entry.GetHash());
                cmd.Parameters.AddWithValue("@userId", entry.UserId);
                cmd.Parameters.AddWithValue("@version", Ensure(entry.Version));
                cmd.Parameters.AddWithValue("@filename", Ensure(entry.FileName));

                var bodyParam = cmd.Parameters.Add("@body", SqlDbType.NVarChar, -1);
                bodyParam.Value = entry.Body;

                cmd.ExecuteNonQuery();
            }
        }
        public DBUpdateLogEntry FindSimilarEntry(DBUpdateLogEntry entry)
        {
            if (entry == null) throw new ArgumentNullException("entry");

            var hash = entry.GetHash();

            using (var cmd = new SqlCommand(GetUpdatesQuery, _connection))
            {
                cmd.Parameters.AddWithValue("@hash", entry.GetHash());
                
                var results = new DataTable();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return new DBUpdateLogEntry
                        {
                            Applied = Convert.ToDateTime(reader["Applied"]),
                            FileName = Convert.ToString(reader["FileName"]),
                            UserId = Convert.ToInt32(reader["USER_ID"])
                        };
                    }
                }
                
            }

            return null;
        }

        private static bool ValidateEntry(DBUpdateLogEntry entry)
        {
            if (string.IsNullOrEmpty(entry.Body))
                throw new ArgumentNullException("entry.Body");

            if (string.IsNullOrEmpty(entry.FileName))
                throw new ArgumentNullException("entry.FileName");

            if (entry.UserId == default(int))
                throw new ArgumentNullException("entry.UserId");

            if (entry.Applied == default(DateTime))
                throw new ArgumentNullException("entry.Applied");

            return true;
        }

        private static object Ensure(object value)
        {
            return value == null ? DBNull.Value : value;
        }
    }
}