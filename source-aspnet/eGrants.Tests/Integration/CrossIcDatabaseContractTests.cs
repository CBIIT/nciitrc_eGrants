using System.Data;

using Microsoft.Data.SqlClient;

namespace eGrants.Tests.Integration
{
    public class CrossIcDatabaseContractTests
    {
        private static SqlConnection CreateConnection()
        {
            return new SqlConnection(TestDatabase.ConnectionString);
        }

        [DbFact]
        public void CrossIcStoredProcedures_ExposeAuthorityScopeParameters()
        {
            using var conn = CreateConnection();
            conn.Open();

            var requiredParams = new[] { "@access_scope", "@authority_ic" };
            var procedures = new[]
            {
                "sp_web_egrants_search_by_appl_id",
                "sp_web_egrants_doc_create",
                "sp_web_egrants_doc_modify",
                "sp_web_egrants_institutional_file_create",
                "sp_web_egrants_institutional_file_update"
            };

            foreach (var proc in procedures)
            {
                using var cmd = new SqlCommand(@"
SELECT p.name
FROM sys.parameters p
JOIN sys.objects o ON o.object_id = p.object_id
WHERE o.name = @procName AND o.type = 'P';", conn);
                cmd.Parameters.Add("@procName", SqlDbType.NVarChar, 128).Value = proc;

                var foundParams = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    foundParams.Add(rdr.GetString(0));
                }

                foreach (var param in requiredParams)
                {
                    Assert.Contains(param, foundParams);
                }
            }
        }

        [DbFact]
        public void SearchByApplId_ExecutesWithCrossIcParameters()
        {
            using var conn = CreateConnection();
            conn.Open();

            int applId;
            using (var applCmd = new SqlCommand("SELECT TOP 1 appl_id FROM egrants WHERE appl_id IS NOT NULL ORDER BY appl_id DESC", conn))
            {
                var applObj = applCmd.ExecuteScalar();
                Assert.NotNull(applObj);
                applId = Convert.ToInt32(applObj);
            }

            string operatorUser;
            using (var operatorCmd = new SqlCommand("SELECT TOP 1 userid FROM vw_people WHERE userid IS NOT NULL ORDER BY person_id DESC", conn))
            {
                var userObj = operatorCmd.ExecuteScalar();
                Assert.NotNull(userObj);
                operatorUser = Convert.ToString(userObj)!;
            }

            using var cmd = new SqlCommand("dbo.sp_web_egrants_search_by_appl_id", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 45
            };

            cmd.Parameters.Add("@appl_id", SqlDbType.Int).Value = applId;
            cmd.Parameters.Add("@search_type", SqlDbType.VarChar, 50).Value = DBNull.Value;
            cmd.Parameters.Add("@category_list", SqlDbType.VarChar, 500).Value = DBNull.Value;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar, 10).Value = "nci";
            cmd.Parameters.Add("@operator", SqlDbType.VarChar, 50).Value = operatorUser;
            cmd.Parameters.Add("@access_scope", SqlDbType.VarChar, 50).Value = "CrossIcSpecialistBaseline";
            cmd.Parameters.Add("@authority_ic", SqlDbType.VarChar, 10).Value = "nci";

            using var rdr = cmd.ExecuteReader();
            // At minimum verify execution succeeds and the result shape is readable.
            Assert.True(rdr.FieldCount > 0);
        }

        [DbFact]
        public void FnGetDocUrl_LocalPath_IsViewerIcIndependent()
        {
            using var conn = CreateConnection();
            conn.Open();

            int? docId = null;
            using (var findCmd = new SqlCommand(@"
SELECT TOP 1 document_id
FROM egrants
WHERE [url] LIKE 'data/%' OR [url] LIKE '/data%'
ORDER BY document_id DESC", conn))
            {
                var obj = findCmd.ExecuteScalar();
                if (obj != null && obj != DBNull.Value)
                {
                    docId = Convert.ToInt32(obj);
                }
            }

            if (!docId.HasValue)
            {
                // No local-path docs available in this environment; nothing to validate.
                return;
            }

            string? nciUrl;
            using (var nciCmd = new SqlCommand("SELECT dbo.fn_get_doc_url(@docId, @ic)", conn))
            {
                nciCmd.Parameters.Add("@docId", SqlDbType.Int).Value = docId.Value;
                nciCmd.Parameters.Add("@ic", SqlDbType.VarChar, 5).Value = "nci";
                nciUrl = Convert.ToString(nciCmd.ExecuteScalar());
            }

            string? csrUrl;
            using (var csrCmd = new SqlCommand("SELECT dbo.fn_get_doc_url(@docId, @ic)", conn))
            {
                csrCmd.Parameters.Add("@docId", SqlDbType.Int).Value = docId.Value;
                csrCmd.Parameters.Add("@ic", SqlDbType.VarChar, 5).Value = "csr";
                csrUrl = Convert.ToString(csrCmd.ExecuteScalar());
            }

            Assert.False(string.IsNullOrWhiteSpace(nciUrl));
            Assert.False(string.IsNullOrWhiteSpace(csrUrl));
            Assert.Equal(nciUrl, csrUrl);
        }
    }
}
