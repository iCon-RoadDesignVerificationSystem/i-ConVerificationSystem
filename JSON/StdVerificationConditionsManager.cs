using i_ConVerificationSystem.Structs.JSON;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace i_ConVerificationSystem.JSON
{
    internal sealed class StdVerificationConditionsManager
    {
        /// <summary>
        /// Singletonインスタンス
        /// </summary>
        private static StdVerificationConditionsManager instance = new StdVerificationConditionsManager();
        public static StdVerificationConditionsManager Instance
        {
            get { return instance; }
        }

        private StdVerificationConditionsManager()
        {
            //初期表示用
            InitializeStdConditions();
        }
        public StdVerificationConditions stdVerificationConditions { get; private set; }

        public ReactiveProperty<bool> IsLoaded { get; private set; } = new ReactiveProperty<bool>(false);

        public ReactiveProperty<string> JsonFileName { get; private set; } = new ReactiveProperty<string>();

        public ReactiveProperty<string> JsonFilePath { get; private set; } = new ReactiveProperty<string>();
        private void InitializeStdConditions()
        {
            stdVerificationConditions = new StdVerificationConditions();
            JsonFilePath.Value = null;
            JsonFileName.Value = null;
            IsLoaded.Value = false;
        }
        public void ClearStdConditions()
        {
            InitializeStdConditions();
        }
        private bool LoadJsonFile(string jsonFilePath)
        {
            var jsonStr = File.ReadAllText(jsonFilePath);
            var jParser = new JsonParser<StdVerificationConditions>();
            stdVerificationConditions = jParser.DeselializeObj(jsonStr);
            JsonFilePath.Value = jsonFilePath;
            JsonFileName.Value = Path.GetFileName(jsonFilePath);
            IsLoaded.Value = true;

            return true;
        }
        public bool LoadJsonFile()
        {
            var retVal = false;

            try
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.FileName = "*.json";
                    dialog.Filter = "JSONファイル|.json";
                    dialog.FilterIndex = 1;
                    dialog.Title = "基準値ファイルを開く";

                    if (dialog.ShowDialog() == DialogResult.Cancel)
                    {
                        retVal = false;
                    }
                    else
                    {
                        retVal = LoadJsonFile(dialog.FileName);
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("基準値ファイル読み込みでエラーが発生しました。再度選択し直してください。");
                retVal = LoadJsonFile();
            }

            return retVal;
        }
    }

    static class DataTableExtension
    {
        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            PropertyDescriptorCollection props =
            TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }
    }
}
