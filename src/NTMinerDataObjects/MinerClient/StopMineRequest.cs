﻿using System.Text;

namespace NTMiner.MinerClient {
    public class StopMineRequest : RequestBase, ISignatureRequest {
        public StopMineRequest() { }
        public string LoginName { get; set; }
        public string ClientIp { get; set; }
        public string Sign { get; set; }

        public void SignIt(string password) {
            this.Sign = this.GetSign(password);
        }

        public string GetSign(string password) {
            StringBuilder sb = new StringBuilder();
            sb.Append(nameof(MessageId)).Append(MessageId)
                .Append(nameof(LoginName)).Append(LoginName)
                .Append(nameof(ClientIp)).Append(ClientIp)
                .Append(nameof(Timestamp)).Append(Timestamp.ToUlong())
                .Append(nameof(UserData.Password)).Append(password);
            return HashUtil.Sha1(sb.ToString());
        }
    }
}
