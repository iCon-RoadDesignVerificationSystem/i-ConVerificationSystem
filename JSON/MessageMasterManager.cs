using i_ConVerificationSystem.Structs.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace i_ConVerificationSystem.JSON
{
    internal sealed class MessageMasterManager
    {
        /// <summary>
        /// Singletonインスタンス
        /// </summary>
        private static MessageMasterManager instance = new MessageMasterManager();
        public static MessageMasterManager Instance
        {
            get { return instance; }
        }

        private MessageMasterManager()
        {
            //初期設定
            messageMaster = new MessageMaster();
            LoadJsonFile(GetMessageMasterJsonFilePath());
        }
        private MessageMaster messageMaster { get; set; }
        private string jsonFilePath { get; set; }
        public bool IsLoaded
        {
            get { return (!(jsonFilePath is null)); }
        }
        private string GetJsonFilePath()
        {
            return jsonFilePath;
        }

        private bool LoadJsonFile(string jsonFilePath)
        {
            this.jsonFilePath = jsonFilePath;
            var jsonStr = File.ReadAllText(jsonFilePath);
            var jParser = new JsonParser<MessageMaster>();
            messageMaster = jParser.DeselializeObj(jsonStr);

            return true;
        }

        private string GetMessageMasterJsonFilePath()
        {
            Assembly myAssembly = Assembly.GetExecutingAssembly();
            string path = Path.GetDirectoryName(myAssembly.Location);
            return Path.Combine(path, "MessageMaster.json");
        }

        public string GetMessage(string ID, params string[] att)
        {
            var m = (from T in messageMaster.messages.message where T.ID == ID select T.MESSAGE).FirstOrDefault();
            if (m is null || m == string.Empty) return ID;
            
            for (int i = 0; i < att.Length; i++)
            {
                var replaceStr = $"{{{i}}}";
                string msgParam = att[i];

                m = m.Replace(replaceStr, msgParam);
            }

            return m;
        }
    }
}
