using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using shvRadio.ItemsClass;
//using System.Windows.Forms;

namespace shvQRZ
{
    class   AdifLogReader
    {
        public static MainWindow mW = (MainWindow)App.Current.MainWindow;
        public List<AdifLogItem> read_from_string()
        {
            List<AdifLogItem> listQSOLog = new List<AdifLogItem>();
            Dictionary<string, object> dictLog = new Dictionary<string, object>();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string FileNameImportFrom = mW.ComboBoxMyLogFileName.Text;

            string adif_string;
            using (TextReader reader = new StreamReader(FileNameImportFrom, Encoding.GetEncoding("Shift_JIS"), true))
            {
                adif_string = reader.ReadToEnd();
            }
            string mString = @"<(eor|eoh)|(\w+)\:(\d+)>([^<]+)";
            Regex rx = new Regex(mString, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            MatchCollection matches = rx.Matches(adif_string);
            bool isBody = false;
            foreach (Match match in matches)
            {
                GroupCollection groups = match.Groups;
                if (groups[1].Value.Equals("eoh"))
                {
                    isBody = true;
                    continue;
                }
                if (isBody)
                {
                    if (groups[1].Value.Equals("eor"))
                    {
                        AdifLogItem itemAdifLog = (AdifLogItem)GetObject(dictLog, typeof(AdifLogItem));
                        listQSOLog.Add(itemAdifLog);
                        dictLog.Clear();
                    }
                    else
                    {
                        dictLog.Add(groups[2].Value.Trim(), groups[4].Value.Trim());
                    }
                }
            }
            return listQSOLog;
        }

        public static Object  GetObject( Dictionary<string, object> dict, Type type)
        {
            var obj = Activator.CreateInstance(type);

            foreach (var kv in dict)
            {
                var prop = type.GetProperty(kv.Key);
                if (prop == null) continue;

                object value = kv.Value;
                if (value is Dictionary<string, object>)
                {
                    value = GetObject((Dictionary<string, object>)value, prop.PropertyType); 
                }

                prop.SetValue(obj, value, null);
            }
            return obj;
        }
    }
}