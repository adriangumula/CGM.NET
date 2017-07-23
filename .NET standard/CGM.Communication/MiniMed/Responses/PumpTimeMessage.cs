﻿using CGM.Communication.Common.Serialize;
using System;
using CGM.Communication.Extensions;
using System.Linq;
using System.Collections.Generic;

namespace CGM.Communication.MiniMed.Responses
{
    [BinaryType(IsLittleEndian = false)]
    public class PumpTimeMessage : IBinaryType, IBinaryDeserializationSetting
    {
        [BinaryElement(0)]
        public byte Unknown1 { get; set; }

        [BinaryElement(1)]
        public byte[] Rtc { get; set; }

        [BinaryElement(5)]
        public byte[] OffSet { get; set; }

        [BinaryElement(9)]
        public byte[] Unknown2 { get; set; }

        [BinaryElement(11)]
        public byte[] Crc16citt { get; set; }

        public DateTime? PumpDateTime
        {
            get
            {
               return DateTimeExtension.GetDateTime(this.Rtc, this.OffSet);
            }
        }

        public void OnDeserialization(byte[] bytes, SerializerSession settings)
        {
            settings.PumpTime = this;
        }

        public override string ToString()
        {
            return PumpDateTime?.ToString();
        }
    }
}
