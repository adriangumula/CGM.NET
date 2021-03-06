﻿using CGM.Communication.Common.Serialize;
using CGM.Communication.Extensions;
using CGM.Communication.MiniMed.Infrastructur;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CGM.Communication.MiniMed.Responses.Events
{
    [BinaryType(IsLittleEndian = false)]
    public class PumpEvent : IBinaryType, IBinaryDeserializationSetting
    {
        [BinaryElement(0)]
        public byte EventTypeRaw { get; set; }

        private EventTypeEnum _eventType;

        [BsonRepresentation(BsonType.String)]
        public EventTypeEnum EventType { get { _eventType=(EventTypeEnum)EventTypeRaw;
                return _eventType;
            }
            set { _eventType = value; }
        }
        [BinaryElement(1)]
        public byte Source { get; set; }

        [BinaryElement(2)]
        public byte Length { get; set; }
        [BinaryElement(3)]
        public int Rtc { get; set; }
        [BinaryElement(7)]
        public int Offset { get; set; }
        public DateTime? Timestamp { get { return DateTimeExtension.GetDateTime(this.Rtc, this.Offset); } }



        [BsonIgnore]
        [JsonIgnore]
        [BinaryElement(11)]
        [MessageType(typeof(SENSOR_GLUCOSE_READINGS_EXTENDED_Event), nameof(EventType), EventTypeEnum.SENSOR_GLUCOSE_READINGS_EXTENDED)]
        [MessageType(typeof(BOLUS_WIZARD_ESTIMATE_Event), nameof(EventType), EventTypeEnum.BOLUS_WIZARD_ESTIMATE)]
        [MessageType(typeof(CANNULA_FILL_DELIVERED_Event), nameof(EventType), EventTypeEnum.CANNULA_FILL_DELIVERED)]
        [MessageType(typeof(ALARM_NOTIFICATION_Event), nameof(EventType), EventTypeEnum.ALARM_NOTIFICATION)]
        [MessageType(typeof(BG_READING_Event), nameof(EventType), EventTypeEnum.BG_READING)]
        [MessageType(typeof(PLGM_CONTROLLER_STATE_Event), nameof(EventType), EventTypeEnum.PLGM_CONTROLLER_STATE)]
        [MessageType(typeof(TEMP_BASAL_PROGRAMMED_Event), nameof(EventType), EventTypeEnum.TEMP_BASAL_PROGRAMMED)]
        [MessageType(typeof(BaseEvent))]
        [BinaryPropertyValueTransfer(ChildPropertyName = nameof(Timestamp), ParentPropertyName = nameof(Timestamp))]
        public BaseEvent Message { get; set; }


        [BsonIgnore]
        [JsonIgnore]
        public byte[] AllBytes { get; set; }

        [BsonIgnore]
        [JsonIgnore]
        public byte[] AllBytesE { get; set; }
        [BsonId]
        public string BytesAsString { get; set; }
        public int Index { get; set; }

        private string _key;
        public string Key
        {
            get
            {

                if (string.IsNullOrEmpty(_key))
                {
                    string bytes = BytesAsString.Remove(20, 12);
                    _key = string.Format("{0}_{1}", HistoryDataType, bytes);
                }
                return _key;
            }
        }

        public int HistoryDataType { get; set; }

        public void OnDeserialization(byte[] bytes, SerializerSession settings)
        {
            this.AllBytes = bytes;
            this.AllBytesE = bytes.Reverse().ToArray();
            this.BytesAsString = BitConverter.ToString(AllBytes);
            if (this.Message!=null && this.Message.GetType().Equals(typeof(SENSOR_GLUCOSE_READINGS_EXTENDED_Event)))
            {
                try
                {
                    var reading = (SENSOR_GLUCOSE_READINGS_EXTENDED_Event)this.Message;
                    for (int i = 0; i < reading.Details.Count; i++)
                    {
                        if (reading.Timestamp.HasValue)
                        {
                            var read = reading.Details[i];
                            var readingRtc = this.Rtc - (i * reading.MinutesBetweenReadings * 60);
                            read.Timestamp = DateTimeExtension.GetDateTime(readingRtc, this.Offset);
                            read.PredictedSg = reading.PredictedSg;
                        }
                    }
                }
                catch (Exception)
                {

                    throw;
                }
    
            }
        }

        public override string ToString()
        {
            if (this.Message != null)
            {
                return $"{Timestamp.Value} - {EventType.ToString()} - {this.Message.ToString()}";
            }
            else
            {
                return $"{Timestamp.Value} - {EventType.ToString()}";
            }

        }

    }

    [BinaryType(IsLittleEndian = false)]
    public class BaseEvent : IBinaryType, IBinaryDeserializationSetting
    {
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? Timestamp { get; set; }

        [BsonIgnore]
        [JsonIgnore]
        public byte[] AllBytes { get; set; }
        [BsonIgnore]
        [JsonIgnore]
        public byte[] AllBytesE { get; set; }
        public string BytesAsString { get; set; }
        public virtual void OnDeserialization(byte[] bytes, SerializerSession settings)
        {
            this.AllBytes = bytes;
            this.AllBytesE = bytes.Reverse().ToArray();
            this.BytesAsString = BitConverter.ToString(AllBytes);
        }
    }

}
