using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using Path = System.IO.Path;
using shvRadio.ItemsClass;
using System.Windows.Shapes;

namespace shvQRZ
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public List<AdifLogItem> QsoLog;

        private void ButtonMyLogImportFrom_Click(object sender, RoutedEventArgs e)
        {
            MyLogImport();
        }

        private void MyLogImport()
        {
            AdifLogReader af2 = new AdifLogReader();
            QsoLog = af2.read_from_string();
            DataGridMyLog.ItemsSource = QsoLog;
            Set_MyLog_Table();
            ButtonExportCSVfile.IsEnabled = true;
        }

        public void Set_MyLog_Table()
        {
            Style h_ctr = new Style();
            h_ctr.Setters.Add(new Setter(HorizontalAlignmentProperty, System.Windows.HorizontalAlignment.Center));

            Style h_right = new Style();
            h_right.Setters.Add(new Setter(HorizontalAlignmentProperty, System.Windows.HorizontalAlignment.Right));

            for (int i = 0; i < DataGridMyLog.Columns.Count; i++)
            {             
                DataGridMyLog.Columns[i].Width = DataGridLength.Auto;
                DataGridMyLog.Columns[i].HeaderStyle = h_ctr;
            }
        }

        private void ButtonExportCSVfile_Click(object sender, RoutedEventArgs e)
        {
            AdifLogItem a = new AdifLogItem();
            int numData = QsoLog.Count;
            Encoding enc = Encoding.GetEncoding("utf-8");
            string strToday = DateTime.Now.ToString("yyyy_MM_dd_HH_mm");
            string file_mylog_to_adi = "MyLog_Exported_" + strToday + ".csv";

            using (StreamWriter writer = new StreamWriter(file_mylog_to_adi, false, enc))
            {
                string[] strHeaders = {
                    "No", "QSO_Year", "Month","Day","Time_on", "Call", "Grid Square","DistanceKm",
                    "Band", "Freq", "Mode","Submode","RST_Rcvd" , "RST_Sent",
                    "Info RX","Info TX","QTH","QSL Via","Name","email","Comment",
                    "QSL Rcvd","QSL Rcvd Date","QSL Sent","QSL Sent Date",
                    "P16","P26","P36","P46","P56","P66"
                };
                string strS = ",";
                string strLine1 = string.Join(strS, strHeaders);
                writer.WriteLine(strLine1);

                for (int i = 0; i < numData; i++)
                {
                    a = QsoLog[i];
                    double douFreq = Convert.ToDouble(Gs(a.freq));
                    string freq = douFreq.ToString("F3");
                    string amode = Gs(a.mode);
                    string asubmode = Gs(a.submode);
                    bool isFT8 = amode.Contains("FT") | asubmode.Contains("FT"); // show GL only FT* (FT8,FT4), show Info-RX for FM AM SSB etc,.
                    string gridSquare = isFT8 ? a.gridsquare : "";

                    string callSign = Gs(a.call);
                    var aryL6 = new string[] { "", "", "", "", "", "" };
                    if (callSign.Length <= 6)
                    {
                        for (int j = 0; j < callSign.Length; j++)
                        {
                            aryL6[j] = callSign.Substring(j, 1);
                        }
                    }
                    string[] strData = {
                        a.no.ToString(),
                        Gd(Gs(a.qso_date),"year"), Gd(Gs(a.qso_date),"month"), Gd(Gs(a.qso_date),"day"), Gd(Gs(a.time_on),"hour"),
                        callSign, gridSquare,Gs(a.distancekm),
                        Gs(a.band),freq,amode,asubmode,
                        Gs(a.rst_rcvd),Gs(a.rst_sent),
                        Gs(a.srx_string),Gs(a.stx_string),Gs(a.qth),Gs(a.qsl_via),Gs(a.name),Gs(a.email),Gs(a.comment),
                        Gs(a.qsl_rcvd),Gs(a.qslrdate),Gs(a.qsl_sent),Gs(a.qslsdate),
                        aryL6[0],aryL6[1],aryL6[2],aryL6[3],aryL6[4],aryL6[5],
                    };
                    string strSep = ",";
                    string strLine = string.Join(strSep, strData);
                    writer.WriteLine(strLine);
                }
            }
            string fullPath = Path.GetFullPath(file_mylog_to_adi);
            OpenCurrentDirectory(fullPath);
        }

        private static void OpenCurrentDirectory(string cd = "current")
        {
            if (cd.Equals("current")) cd = Directory.GetCurrentDirectory();
            Process.Start("explorer.exe", cd);
        }

        private string Gd(string OrgString, string Param)
        {
            string strReturn = "";

            if (OrgString.Length == 8) // 20191201 >>> 2019 12 01
            {
                switch (Param)
                {
                    case "year":
                        strReturn = OrgString.Substring(0, 4);
                        break;
                    case "month":
                        strReturn = OrgString.Substring(4, 2);
                        break;
                    case "day":
                        strReturn = OrgString.Substring(6, 2);
                        break;
                    default:
                        break;
                }
            }
            else if (OrgString.Length == 6 || OrgString.Length == 4)  // 120100,1201 >>> 12:01
            {
                switch (Param)
                {
                    case "hour":
                        strReturn = OrgString.Substring(0, 2) + ":" + OrgString.Substring(2, 2);
                        break;
                    default:
                        break;
                }
            }
            return strReturn;
        }

        private string Gs(string OrgString)
        {
            if ((OrgString != null) && (OrgString.Trim().Length != 0))
            {
                string s = OrgString.Replace(",", ".");
                return s.Trim();
            }
            else
            {
                return "";
            }
        }

        private void DataGridMyLog_AutoGeneratedColumns(object sender, EventArgs e)
        {
            int[] added = new int[8];
            List<int> addedPos = new List<int>();
            Dictionary<string, int> topColumn = new Dictionary<string, int>
            {
                {"QSO Date", 1}, {"Time", 2},  {"Freq", 3}, {"Mode", 4},{"Call", 5}, {"Country", 6},{"County", 7},
                {"State", 8},{"Name", 9},{"QRZ QSL Date",10 },{"QSL Rcvd Date", 11},{"QSL Sent Date", 12},{"Comment",13}
            };
            List<DataGridTextColumn> col = DataGridMyLog.Columns.Cast<DataGridTextColumn>().ToList();

            foreach (DataGridTextColumn col1 in col)
            {
                if (topColumn.ContainsKey(col1.Header.ToString()))
                {
                    int thisPos = topColumn[col1.Header.ToString()];
                    int addedNumber = 0;
                    if (addedPos.Count.Equals(0))
                    {
                        col1.DisplayIndex = addedNumber;
                        addedPos.Insert(0, thisPos);
                    }
                    else
                    {
                        for (int i = 0; i < addedPos.Count; i++)
                        {
                            if (thisPos < addedPos[i])
                            {
                                col1.DisplayIndex = addedNumber;
                                addedPos.Insert(i, thisPos);
                                break;
                            }
                            addedNumber++;
                        }
                        if (addedPos[addedPos.Count - 1] < thisPos)
                        {
                            col1.DisplayIndex = addedNumber;
                            addedPos.Insert(addedPos.Count, thisPos);
                        }
                    }
                }
            }
            if (0 < DataGridMyLog.Items.Count)
            {
                var border = VisualTreeHelper.GetChild(DataGridMyLog, 0) as Decorator;
                if (border != null)
                {
                    var scroll = border.Child as ScrollViewer;
                    if (scroll != null) scroll.ScrollToEnd();
                }
            }
        }

        private void DataGridMyLog_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string headername = e.Column.Header.ToString();

            Style h_Center = new Style();
            h_Center.Setters.Add(new Setter(HorizontalAlignmentProperty, System.Windows.HorizontalAlignment.Center));
            Style h_Right = new Style();
            h_Right.Setters.Add(new Setter(HorizontalAlignmentProperty, System.Windows.HorizontalAlignment.Right));
            Style h_Stretch = new Style();
            h_Stretch.Setters.Add(new Setter(HorizontalAlignmentProperty, System.Windows.HorizontalAlignment.Stretch));

            Style v_ctr = new Style();
            v_ctr.Setters.Add(new Setter(VerticalAlignmentProperty, VerticalAlignment.Center));

            if (headername == "MiddleName")
            {
                e.Cancel = true;
            }
            string[] headerAligns = variableHeaderAlign[headername].Split(':').ToArray();
            e.Column.Header = headerAligns[0];
            switch (headerAligns[1])
            {
                case "Right": e.Column.CellStyle = h_Right; break;
                case "Stretch": e.Column.CellStyle = h_Stretch; break;
                default: e.Column.CellStyle = h_Center; break;
            }
        }

        private void ComboBoxMyLogFileName_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBoxMyLogFileName.SelectedIndex = 0;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string fn = "exported.adi";
            ComboBoxMyLogFileName.Items.Add(fn);
            ButtonMyLogImportFrom.IsEnabled = false;
            ButtonExportCSVfile.IsEnabled = false;
        }

        private void ButtonSelectMylogFile_Click(object sender, RoutedEventArgs e)
        {
            string file_mylog_from_adi = string.Empty;
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.InitialDirectory = Properties.Settings.Default.PathSelectMylogFile;
            dialog.Filter = "adi file (*.adi)|*.adi|All file (*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                Debug.WriteLine(dialog.FileName);
                file_mylog_from_adi = dialog.FileName;
                int iPosiotion = ComboBoxMyLogFileName.Items.Add(file_mylog_from_adi);
                ComboBoxMyLogFileName.SelectedIndex = iPosiotion;
                ButtonMyLogImportFrom.IsEnabled = true;
            }
            string file = Path.GetFileName(file_mylog_from_adi); 
            string dirName = System.IO.Path.GetDirectoryName(file_mylog_from_adi); 
            Properties.Settings.Default.PathSelectMylogFile = dirName;
            Properties.Settings.Default.Save();
        }

        public Dictionary<string, string> variableHeaderAlign = new Dictionary<string, string>
        {
            { "no", "No:Right"},
            { "call", "Call:Stretch"},
            { "gridsquare", "Grid Squre:Stretch"},
            { "distancekm", "Distance Km:Right"},
            { "mode", "Mode:Right"},
            { "submode", "Submode:Right"},
            { "rst_sent", "RST Sent:Right"},
            { "rst_rcvd", "RST Rcvd:Right"},
            { "qso_date", "QSO Date:Right"},
            { "time_on", "Time:Right"},
            { "qso_date_off", "QSO Date Off:Right"},
            { "time_off", "Time Off:Right"},
            { "band", "Band:Right"},
            { "freq", "Freq:Right"},
            { "station_callsign", "Station Call:Right"},
            { "my_gridsquare", "My Grid:Right"},
            { "operatorname", "Oper Name:Right"},
            { "app_qrzlog_logid", "QRZID:Right"},
            { "app_qrzlog_status", "Status:Right"},
            { "band_rx", "Band RX:Right"},
            { "cont", "Continent:Right"},
            { "country", "Country:Right"},
            { "cqz", "CQ Zone:Right"},
            { "dxcc", "DXCC:Right"},
            { "freq_rx", "Freq RX:Right"},
            { "ituz", "ITU Zone:Right"},
            { "lat", "LAT:Right"},
            { "lon", "LON:Right"},
            { "lotw_qsl_sent", "LOTW QSL Sent:Right"},
            { "my_city", "My City:Right"},
            { "my_country", "My Country:Right"},
            { "my_cq_zone", "my CQ Zone:Right"},
            { "my_iota", "My IOTA:Right"},
            { "my_itu_zone", "My ITU Zone:Right"},
            { "my_lat", "My LAT:Right"},
            { "my_lon", "My LON:Right"},
            { "my_name", "My Name:Right"},
            { "qrzcom_qso_upload_date", "QRZ Upload Date:Right"},
            { "qrzcom_qso_upload_status", "QRZ Upload Status:Right"},
            { "qsl_rcvd", "QSL Rcvd:Right"},
            { "qsl_sent", "QSL Sent:Right"},
            { "comment", "Comment:Right"},
            { "distance", "DistanceQRZ:Right"},
            { "email", "eMail:Right"},
            { "eqsl_qsl_rcvd", "eQSL Rcvd:Right"},
            { "eqsl_qsl_sent", "eQSL Sent:Right"},
            { "iota", "IOTA:Right"},
            { "lotw_qsl_rcvd", "LOTW QSL Rcvd:Right"},
            { "name", "Name:Right"},
            { "qsl_via", "QSL Via:Right"},
            { "qth", "QTH:Right"},
            { "srx_string", "Info RX:Right"},
            { "stx_string", "Info TX:Right"},
            { "app_qrzlog_qsldate", "QRZ QSL Date:Right"},
            { "lotw_qslrdate", "LOTW QSL Rcvd Date:Right"},
            { "qslrdate", "QSL Rcvd Date:Right"},
            { "qslsdate", "QSL Sent Date:Right"},
            { "cnty", "County:Right"},
            { "state", "State:Right"},
            { "rx_pwr", "RX Power:Right"},
            { "tx_pwr", "TX Power:Right"}
        };
    }
}
