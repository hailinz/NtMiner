﻿using NTMiner.ServiceContracts.DataObjects;
using System;
using System.Runtime.Serialization;
using System.Text;

namespace NTMiner.ServiceContracts.MinerClient.DataObjects {
    [DataContract]
    public class StartMineRequest : RequestBase, ISignatureRequest {
        [DataMember]
        public string LoginName { get; set; }
        [DataMember]
        public Guid WorkId { get; set; }
        [DataMember]
        public string Sign { get; set; }

        public void SignIt(string password) {
            this.Sign = this.GetSign(password);
        }

        public string GetSign(string password) {
            StringBuilder sb = new StringBuilder();
            sb.Append(nameof(MessageId)).Append(MessageId)
                .Append(nameof(LoginName)).Append(LoginName)
                .Append(nameof(WorkId)).Append(WorkId)
                .Append(nameof(Timestamp)).Append(Timestamp.ToUlong())
                .Append(nameof(UserData.Password)).Append(password);
            return HashUtil.Sha1(sb.ToString());
        }
    }
}
