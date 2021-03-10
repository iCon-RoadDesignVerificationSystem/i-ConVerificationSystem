using i_ConVerificationSystem.Structs.JSON;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_ConVerificationSystem.JSON
{
    class JsonParser<T>
    {
        /// <summary>
        /// ファイル読込のためのデシリアライズ jsonStr -> jsonObj
        /// </summary>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        public T DeselializeObj(string jsonStr)
        {
            //ロード処理
            T jsonObj = JsonConvert.DeserializeObject<T>(jsonStr);
            return jsonObj;
        }

        /// <summary>
        /// ファイル保存のためのシリアライズ jsonObj -> jsonStr
        /// </summary>
        /// <param name="jsonObj"></param>
        /// <returns></returns>
        public string SelializeStr(T jsonObj)
        {
            //セーブ処理
            string jsonStr = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
            return jsonStr;
        }

    }
}