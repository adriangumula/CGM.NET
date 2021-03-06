﻿using CGM.Communication.Common.Serialize;
using CGM.Communication.Extensions;
using CGM.Communication.MiniMed.Responses.Events;
using CGM.Data.Nightscout.RestApi;
using CMG.Data.Sqlite;
using CMG.Data.Sqlite.Repository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CGM.LocalClient
{
    public class NightscoutUploadSqliteRestApi : UploadLogic
    {
        public NightscoutUploadSqliteRestApi(SerializerSession session) : base(session)
        {
        }

        protected async override void AddDeviceStatus(CancellationToken cancelToken)
        {
            if (string.IsNullOrEmpty(_session.Settings.NightscoutApiUrl) || string.IsNullOrEmpty(_session.Settings.NightscoutSecretkey))
            {
                throw new ArgumentException("Nightscout url or apikey is null.");
            }
            _client = new NightscoutClient(_session.Settings.NightscoutApiUrl, _session.Settings.NightscoutSecretkey);

            if (this.DeviceStatus != null && !string.IsNullOrEmpty(this.DeviceStatus.Device))
            {
                await _client.AddDeviceStatusAsync(new List<DeviceStatus>() { this.DeviceStatus }, cancelToken);
                Logger.LogInformation("DeviceStatus uploaded to Nightscout.");
            }
        }

        protected async override void AddEntries(CancellationToken cancelToken)
        {
            if (Entries.Count > 0)
            {
                try
                {

                    await _client.AddEntriesAsync(Entries, cancelToken);

                    Logger.LogInformation($"Entries uploaded to Nightscout. ({Entries.Count})");
                    //log uploads
                    using (CgmUnitOfWork uow = new CgmUnitOfWork())
                    {
                        uow.HistoryStatus.AddKeys(Entries.Select(e => e.Key).ToList(), 0);
                    }

                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        protected async override void AddTreatments(CancellationToken cancelToken)
        {
            if (this.Treatments.Count > 0)
            {
                try
                {
                    await _client.AddTreatmentsAsync(this.Treatments, cancelToken);
                    Logger.LogInformation($"Treatments uploaded to Nightscout. ({Treatments.Count})");
                    using (CgmUnitOfWork uow = new CgmUnitOfWork())
                    {
                        uow.HistoryStatus.AddKeys(Treatments.Select(e => e.Key).ToList(), 0);
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        protected override List<PumpEvent> GetHistoryWithNoStatus(List<int> eventFilter)
        {
            List<PumpEvent> eventsToHandle = new List<PumpEvent>();

            using (CgmUnitOfWork uow = new CgmUnitOfWork())
            {
                var NewEvents = uow.History.GetHistoryWithNoStatus(eventFilter);


                if (NewEvents.Count > 0)
                {
                    Serializer serializer = new Serializer(Session);

                    foreach (var item in NewEvents)
                    {
                        var pumpevent = serializer.Deserialize<PumpEvent>(item.HistoryBytes.GetBytes());
                        pumpevent.HistoryDataType = item.HistoryDataType;
                        eventsToHandle.Add(pumpevent);
                    }


                }
            }
            return eventsToHandle;
        }
    }
}
