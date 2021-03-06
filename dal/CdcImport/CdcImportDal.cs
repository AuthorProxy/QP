﻿using System;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace Quantumart.QP8.DAL.CdcImport
{
    [SuppressMessage("ReSharper", "RedundantVerbatimPrefix")]
    public static class CdcImportDal
    {
        private const string @LastExecutedLsn = "@lastExecutedLsn";
        private const string @LastPushedLsn = "@lastPushedLsn";
        private const string @ProviderName = "@providerName";
        private const string @ProviderUrl = "@providerUrl";

        public static string GetLastExecutedLsn(SqlConnection connection, string providerUrl)
        {
            using (var cmd = SqlCommandFactory.Create($"SELECT TOP(1) LastExecutedLsn FROM [dbo].[CdcLastExecutedLsn] WHERE ProviderUrl = {@ProviderUrl};", connection))
            {
                cmd.Parameters.AddWithValue(@ProviderUrl, providerUrl);
                return (string)cmd.ExecuteScalar();
            }
        }

        public static string GetMaxLsn(SqlConnection connection)
        {
            using (var cmd = SqlCommandFactory.Create("SELECT CONVERT(varchar(22), sys.fn_cdc_get_max_lsn(), 1);", connection))
            {
                return (string)cmd.ExecuteScalar();
            }
        }

        public static int PostLastExecutedLsn(SqlConnection connection, string providerName, string providerUrl, string lastPushedLsn, string lastExecutedLsn)
        {
            var query = $@"
IF EXISTS (SELECT * FROM [dbo].[CdcLastExecutedLsn] WHERE providerUrl = {@ProviderUrl})
    UPDATE [dbo].[CdcLastExecutedLsn]
    SET
        ProviderName = {@ProviderName},
        TransactionLsn = COALESCE({@LastPushedLsn}, TransactionLsn),
        TransactionDate = sys.fn_cdc_map_lsn_to_time(CONVERT(binary, COALESCE({@LastPushedLsn}, TransactionLsn), 1)),
        LastExecutedLsn = {@LastExecutedLsn}
    OUTPUT inserted.id
    WHERE providerUrl = {@ProviderUrl}
ELSE
    INSERT INTO [dbo].[CdcLastExecutedLsn] (
        [ProviderName],
        [ProviderUrl],
        [TransactionLsn],
        [TransactionDate],
        [LastExecutedLsn]
    )
    OUTPUT inserted.id
    VALUES (
        {@ProviderName},
        {@ProviderUrl},
        {@LastPushedLsn},
        sys.fn_cdc_map_lsn_to_time(CONVERT(binary, {@LastPushedLsn}, 1)),
        {@LastExecutedLsn}
    )
";

            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.Parameters.AddWithValue(@ProviderName, providerName);
                cmd.Parameters.AddWithValue(@ProviderUrl, providerUrl);
                cmd.Parameters.AddWithValue(@LastPushedLsn, (object)lastPushedLsn ?? DBNull.Value);
                cmd.Parameters.AddWithValue(@LastExecutedLsn, lastExecutedLsn);
                return (int)cmd.ExecuteScalar();
            }
        }
    }
}
