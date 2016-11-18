﻿using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quantumart.QP8.BLL;

namespace QP8.WebMvc.NUnit.Tests.BLL
{
    [TestClass]
    public class UserQueryColumnTest
    {
        [TestMethod]
        public void IgnoreTableNameComparationTest()
        {
            var collection1 = new[]
            {
                new UserQueryColumn {ColumnName = "C1", DbType = "Datetime", TableName="T1"},
                new UserQueryColumn {ColumnName = "C2", DbType = "datetime", TableName="T1"},
                new UserQueryColumn {ColumnName = "C3", DbType = "nvarchar", TableName="T1"},
                new UserQueryColumn {ColumnName = null, DbType = "numeric", TableName=null}
            };

            var collection2 = new[]
            {
                new UserQueryColumn {ColumnName = "c1", DbType = "datetime", TableName="T2"},
                new UserQueryColumn {ColumnName = "C2", DbType = "numeric", TableName="T1"},
                new UserQueryColumn {ColumnName = "C4", DbType = "numeric", TableName="T1"},
                new UserQueryColumn {ColumnName = "c3", DbType = null, TableName="T1"}
            };

            var intersect = collection1.Intersect(collection2, UserQueryColumn.TableNameIgnoreEqualityComparer).ToArray();

            Assert.AreEqual(1, intersect.Length);
            Assert.AreEqual(intersect[0].ColumnName.ToLowerInvariant(), "c1");
            Assert.AreEqual(intersect[0].DbType.ToLowerInvariant(), "datetime");
        }

        [TestMethod]
        public void ComparationTest()
        {
            var collection1 = new[]
            {
                new UserQueryColumn {ColumnName = "C1", DbType = "Datetime", TableName="T1"},
                new UserQueryColumn {ColumnName = "C2", DbType = "datetime", TableName="T1"},
                new UserQueryColumn {ColumnName = "C3", DbType = "nvarchar", TableName="T1"},
                new UserQueryColumn {ColumnName = "C4", DbType = "numeric", TableName="T1"},
                new UserQueryColumn {ColumnName = null, DbType = null, TableName="T1"}
            };

            var collection2 = new[]
            {
                new UserQueryColumn {ColumnName = "C1", DbType = "datetime", TableName="T1"},
                new UserQueryColumn {ColumnName = "C2", DbType = "numeric", TableName="T1"},
                new UserQueryColumn {ColumnName = "C3", DbType = "nvarchar", TableName="T2"},
                new UserQueryColumn {ColumnName = "C5", DbType = "numeric", TableName="T1"},
                new UserQueryColumn {ColumnName = null, DbType = null, TableName="T2"}
            };

            var intersect = collection1.Intersect(collection2).ToArray();

            Assert.AreEqual(1, intersect.Length);
            Assert.AreEqual(intersect[0].ColumnName.ToLowerInvariant(), "c1");
            Assert.AreEqual(intersect[0].DbType.ToLowerInvariant(), "datetime");
        }

        [TestMethod]
        public void SelectColumnCompareToTest()
        {
            var source = new[]
            {
                new UserQueryColumn {TableName="content_10"},
                new UserQueryColumn {TableName="T1"},
                new UserQueryColumn {TableName= null},
                new UserQueryColumn {TableName="t0"},
                new UserQueryColumn {TableName="CONTENT_0"}
            };

            var result = source.OrderByDescending(c => c, UserQueryColumn.SelectBaseColumnComparer).ToArray();

            Assert.AreEqual(source.Length, result.Length);

            Assert.AreEqual("content_0", result[0].TableName, true);
            Assert.AreEqual("content_10", result[1].TableName, true);
            Assert.AreEqual("t1", result[2].TableName, true);
            Assert.AreEqual("t0", result[3].TableName, true);
            Assert.IsNull(result[4].TableName);
        }
    }
}