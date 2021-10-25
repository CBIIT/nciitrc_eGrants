using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;
using egrants_new.Integration.Models;

namespace egrants_new.Integration.Shared
{
    public static class SqlHelper
    {

        public static void MapDataToObject(Object obj, SqlDataReader reader)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                try
                {
                    if (!Convert.IsDBNull(reader[prop.Name]))
                    {
                        switch (prop.PropertyType.Name.ToLower())
                        {

                            case "string":
                                prop.SetValue(obj, reader[prop.Name].ToString());
                                break;
                            case "int32":
                                prop.SetValue(obj, (int)reader[prop.Name]);
                                break;

                            case "authenticationtype":
                                prop.SetValue(obj, (IntegrationEnums.AuthenticationType)reader[prop.Name]);
                                break;

                            case "datetimeoffset":
                                prop.SetValue(obj, (DateTimeOffset)reader[prop.Name]);
                                break;
                            case "datetimeunits":
                                prop.SetValue(obj, (IntegrationEnums.DateTimeUnits)reader[prop.Name]);
                                break;
                            case "boolean":
                                prop.SetValue(obj, (bool)reader[prop.Name]);
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        prop.SetValue(obj, null, null);
                    }
                }
                catch (Exception ex)
                {
                    //throw ex;
                    //TODO: Handle Exception here
                }
            }

        }



    }
}