﻿using CGM.Communication.Common;
using CGM.Communication.Log;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace  CMG.Data.Sqlite.Repository
{
    public class SettingRepository : BaseRepository<SqliteSetting>
    {

        public SettingRepository(CgmUnitOfWork uow):base(uow)
        {

        }

        private SqliteSetting GetCurrent()
        {
            var query = _uow.Connection.Table<SqliteSetting>().Where(v => v.SettingId == 1);
            SqliteSetting setting = query.FirstOrDefault();

            if (setting == null)
            {
                setting = new SqliteSetting();
                setting.SettingId = 1;
                this.Add(setting);
                
            };
            return setting;
        }

        public Configuration GetSettings()
        {
            var sqlitesetting = GetCurrent();
            if (!string.IsNullOrEmpty(sqlitesetting.SettingJson))
            {
                return JsonConvert.DeserializeObject<Configuration>(sqlitesetting.SettingJson);
            }
            else
            {
                Configuration setting = new Configuration();
                Update(setting);
                return setting;
            }
            
        }

        public void Update(Configuration setting)
        {
            SqliteSetting sqlitesetting = new SqliteSetting();
            sqlitesetting.SettingId = 1;
            sqlitesetting.SettingJson = JsonConvert.SerializeObject(setting);
            base.Update(sqlitesetting);
        }

        public void CheckSettings()
        {
            var settings = GetSettings();
            if (string.IsNullOrEmpty(settings.NightscoutUrl) || string.IsNullOrEmpty(settings.NightscoutSecretkey))
            {
                Logger.LogInformation("No Nightscout settings entered. Please enter Nightscout settings in the settings page.");
            }
        }
    }
}
