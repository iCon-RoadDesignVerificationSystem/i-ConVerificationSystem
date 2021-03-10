using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static i_ConVerificationSystem.Structs.VerificationResult;

namespace i_ConVerificationSystem.Structs
{
    public class GGVerificationResultItem
    {
        private static GGVerificationResultItem instance = new GGVerificationResultItem();
        public static GGVerificationResultItem Instance
        {
            get
            {
                return instance;
            }
        }
        public void Clear()
        {
            instance = new GGVerificationResultItem();
        }

        /// <summary>
        /// 照査が実行されているか
        /// </summary>
        /// <returns></returns>
        public bool HasVerificationResult()
        {
            return ggvrPairs.Any();
        }

        /// <summary>
        /// エラーがあるか
        /// </summary>
        /// <returns></returns>
        public bool HasError()
        {
            foreach (var item in ggvrPairs)
            {
                if (item.Value.bResultType == VerifyResultType.NG ||
                    item.Value.eResultType == VerifyResultType.NG)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// エラーの数を返答
        /// </summary>
        /// <returns></returns>
        public int GetErrorCount()
        {
            int errCount = 0;
            foreach (var item in ggvrPairs)
            {
                if (item.Value.bResultType == VerifyResultType.NG)
                {
                    errCount++;
                }
                if (item.Value.eResultType == VerifyResultType.NG)
                {
                    errCount++;
                }
            }

            return errCount;
        }

        /// <summary>
        /// OK(確認)の数を返答
        /// </summary>
        /// <returns></returns>
        public int GetOK_CCount()
        {
            int okcCount = 0;
            foreach (var item in ggvrPairs)
            {
                if (item.Value.bResultType == VerifyResultType.OK_C)
                {
                    okcCount++;
                }
                if (item.Value.eResultType == VerifyResultType.OK_C)
                {
                    okcCount++;
                }
            }

            return okcCount;
        }

        public Dictionary<string, GGVerificationResultItems> ggvrPairs { get; set; }

        public class GGVerificationResultItems
        {
            public string alignmentName { get; set; }
            public VerifyResultType bResultType { get; set; }
            public string bMessage { get; set; }
            public string bMessageCode { get; set; }
            public VerifyResultType eResultType { get; set; }
            public string eMessage { get; set; }
            public string eMessageCode { get; set; }
        }

        private GGVerificationResultItem()
        {
            ggvrPairs = new Dictionary<string, GGVerificationResultItems>();
        }

        private void IsExistsKey(string ali)
        {
            if (!(ggvrPairs.ContainsKey(ali)))
            {
                ggvrPairs.Add(ali, new GGVerificationResultItems()
                {
                    alignmentName = ali
                });
            }
        }

        public void Update(string aliName, bool isBeginSide, VerificationResult vr)
        {
            IsExistsKey(aliName);

            if (isBeginSide)
            {
                ggvrPairs[aliName].bResultType = vr.ResultType;
                ggvrPairs[aliName].bMessage = vr.Message;
                ggvrPairs[aliName].bMessageCode = vr.MsgCode;
            }
            else
            {
                ggvrPairs[aliName].eResultType = vr.ResultType;
                ggvrPairs[aliName].eMessage = vr.Message;
                ggvrPairs[aliName].eMessageCode = vr.MsgCode;
            }
        }
    }
}
