using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using egrants_new.Integration.Models;
using Hangfire.Common;

namespace egrants_new.Integration.WebServices
{
    public class WsScheduleManager
    {
        private readonly IntegrationRepository _repo;

        public WsScheduleManager()
        {
            _repo = new IntegrationRepository();
        }

        public void StartScheduledJobs()
        {
            var jobs = _repo.GetEgrantWebServiceDueToFire();

            foreach (var job in jobs)
            {
                //WebServiceHistory history;

                var histories = job.GetData();
                foreach (var history in histories)
                {
                    _repo.SaveData(history);
                }
     
            }
        }

    }
}