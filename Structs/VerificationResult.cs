using i_ConVerificationSystem.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_ConVerificationSystem.Structs
{
    public class VerificationResult
    {
        public string MsgCode { get; set; } = "";
        private object[] MsgParameter = new object[0];
        public VerifyResultType ResultType { get; set; } = VerifyResultType.SKIP;
        public string Message { get { return GetMessage(); } }

        public void SetMsgParameter(params object[] msgParameter)
        {
            MsgParameter = MsgParameter.Concat(msgParameter).ToArray();
        }

        private string GetMessage()
        {
            string[] att = (from T in MsgParameter select T.ToString()).ToArray();
            return MessageMasterManager.Instance.GetMessage(MsgCode, att);
        }

        public enum VerifyResultType
        {
            NG = 0,
            OK,
            OK_C,
            SKIP
        }

        public bool HasError
        {
            get
            {
                return ResultType == VerifyResultType.NG;
            }
        }

        public bool HasOK_C
        {
            get
            {
                return ResultType == VerifyResultType.OK_C;
            }
        }
    }
}
