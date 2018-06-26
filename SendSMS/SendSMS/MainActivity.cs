using Android.App;
using Android.Widget;
using Android.OS;
using System.Data.SqlClient;
using Android.Telephony;
using System.Threading.Tasks;
using System.Text;
using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Syncfusion.XlsIO;
using Syncfusion.Compression;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Reflection;
using Android;
using Android.Content.PM;

namespace SendSMS
{
    [Activity(Label = "SendSMS", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        Button loadDataButton;
        Button sendSmsButton;
        Button stopDispatchButton;
        Button approveButton;

        EditText txtLineCount;
        EditText txtFileName;
        EditText txtColumnNumber;
        EditText txtText;

        TextView lblInfo;
        List<string> SmsRecievers;
        string message;        

        private static readonly Regex ifExcelFile = new Regex(@"^.*\.(XLS|xls|XLSX|xlsx)$");
        private static readonly Regex everythingButN = new Regex(@"\D+");
        public static bool isRecieved = false;

/*
         public async Task<List<Recipient>> Download(string connString)
         {
             btnLoad = FindViewById<Button>(Resource.Id.btnLoad);
             btnLoad.Enabled = false;
             var recievers = new List<Recipient>();
             txtData = FindViewById<TextView>(Resource.Id.txtData);
             txtData.Text = "Task started" + System.Environment.NewLine;
             using (var conn = new SqlConnection(connString))
             {

                 var cmdTxt = "SELECT * FROM SMS_test WHERE Sent IS NULL";
                 var cmnd = new SqlCommand(cmdTxt, conn);
                 conn.Open();
                 txtData.Text += "Connection opened" + System.Environment.NewLine;
                 SqlDataReader dr = cmnd.ExecuteReader();

                 txtData.Text += "Reading started" + System.Environment.NewLine;
                 txtData.Text += "Numbers:" + System.Environment.NewLine;
                 while (dr.Read())
                 {
                     if (dr.HasRows == true)
                     {
                         recievers.Add(new Recipient(new Guid(Convert.ToString(dr["Id"])), Convert.ToString(dr["Phone"]), Convert.ToString(dr["Message"])));
                         txtData.Text += Convert.ToString(dr["Phone"]) + System.Environment.NewLine;
                     }
                 }
                 txtData.Text += "Reading ended" + System.Environment.NewLine;
                 txtData.Text += $"Downloaded {recievers.Count} entries from DB";
                 Toast.MakeText(this, $"Downloaded {recievers.Count} entries from DB", ToastLength.Long).Show();
                 btnLoad.Enabled = true;
                 return recievers;
             }
         }*/




        private SmsManager _smsManager;
        private BroadcastReceiver _smsSentBroadcastReceiver, _smsDeliveredBroadcastReceiver;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.Main);
            
            SmsRecievers = new List<string>();

            loadDataButton = FindViewById(Resource.Id.btnLoad) as Button;
            sendSmsButton = FindViewById(Resource.Id.btnSend) as Button;
            stopDispatchButton = FindViewById(Resource.Id.btnStop) as Button;
            approveButton = FindViewById(Resource.Id.btnApprove) as Button;

            txtText = FindViewById(Resource.Id.txtText) as EditText;
            txtLineCount = FindViewById(Resource.Id.txtLines) as EditText;
            txtColumnNumber = FindViewById(Resource.Id.txtLines) as EditText;
            txtFileName = FindViewById(Resource.Id.txtFilename) as EditText;
            txtColumnNumber = FindViewById(Resource.Id.txtNumberClmn) as EditText;
            lblInfo = FindViewById(Resource.Id.lblInfo) as TextView;


            txtFileName.TextChanged += TxtFileName_TextChanged;
            txtColumnNumber.TextChanged += TxtColumnNumber_TextChanged;
            sendSmsButton.Click += SendSmsButton_Click;
            loadDataButton.Click += LoadDataButton_Click;
            stopDispatchButton.Click += StopDispatchButton_Click;
            approveButton.Click += ApproveButton_Click;

            _smsManager = SmsManager.Default;
        }

        private void ApproveButton_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void StopDispatchButton_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public static string ReplaceEButN(string input, string replacement)
        {
            Console.WriteLine("----Characters replaced----");
            return everythingButN.Replace(input, replacement);
        }

        private void LoadDataButton_Click(object sender, EventArgs e)
        {
            loadDataButton.Clickable = false;
            try
            {
                string filename = txtFileName.Text;
                if (!UInt16.TryParse(txtColumnNumber.Text, out ushort columnNr))
                {
                    lblInfo.Text += "Invalid column number\n";
                    return;
                }
                if (!ifExcelFile.IsMatch(filename))
                {
                    lblInfo.Text += filename + " is not an Excel document (maybe it lacks .xlsx extension?)\n";
                    return;
                }
                SmsRecievers = LoadNumbers(filename, columnNr);
                if (SmsRecievers.Count > 0)
                {
                    lblInfo.Text += "Data loaded succesfully\n";
                    sendSmsButton.Clickable = true;
                }
                else
                {
                    lblInfo.Text += "Cannot load the data\n";
                    return;
                }

            }
            catch (Exception ex)
            {
                lblInfo.Text += "Catched an exception: " + ex.Message + "\n";
            }


            loadDataButton.Clickable = true;
        }

        private void SendSmsButton_Click(object sender, EventArgs e)
        {
            if (!(SmsRecievers.Count > 0))
            {
                lblInfo.Text += "Unable to send messages: no numbers loaded\n";
                return;
            }
            if(txtText.Text != "")
            {
                message = txtText.Text;
            }
            else
            {
                lblInfo.Text += "Please enter message text\n";
            }
            Console.WriteLine("Numbers are: ");
            foreach (var item in SmsRecievers)
            {
                Console.WriteLine(item);
            }

            var piSent = PendingIntent.GetBroadcast(this, 0, new Intent("SMS_SENT"), 0);
            var piDelivered = PendingIntent.GetBroadcast(this, 0, new Intent("SMS_DELIVERED"), 0);

            foreach (var item in SmsRecievers)
            {
                Console.WriteLine($"Sending sms to the number {item}");
                _smsManager.SendTextMessage(item, null, message, piSent, piDelivered);
            }
        }

        private void TxtColumnNumber_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            
        }

        private void TxtFileName_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {

        }


        private List<string> LoadNumbers(string filename, ushort clmnNumber)
        {
            Console.WriteLine("=========Load ");
            var numbers = new List<string>();

            //Creates a new instance for ExcelEngine.
            ExcelEngine excelEngine = new ExcelEngine();


            //var pathFile = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads);
            //var absolutePath = pathFile.AbsolutePath;

            var pth = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).ToString();

            var documentsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments);

            var filePath = global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            //var filePath = "/storage/emulated/0/Download";
            var path = Path.Combine(pth, filename);
            Console.WriteLine("=============Trynig to load data: path - " + path + " column number: " + clmnNumber + " ===============\n");

            //Loads or open an existing workbook through Open method of IWorkbooks
            try
            {
                using (var fs = System.IO.File.Open(path, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None))
                {
                    Console.WriteLine("------------- inside try-catch ---------------\n");
                    IWorkbook workbook = excelEngine.Excel.Workbooks.Open(fs);
                    Console.WriteLine("Path to file is: " + path);
                    IWorksheet sheet = workbook.Worksheets[0];
                    string num;
                    for (int i = 1; i < 10; i++)
                    {
                        //replace everything but numbers
                        if (sheet.GetText(i, clmnNumber) == null)
                        {
                            Console.WriteLine($"Cell {i} is empty");
                            continue;
                        }
                        num = ReplaceEButN(sheet.GetText(i, clmnNumber), "");
                        if (num != "")
                        {
                            numbers.Add(num);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblInfo.Text += "Unable to load data. Message: " + ex.Message + "\n";

            }


            return numbers;
        }

        protected override void OnResume()
        {
            base.OnResume();

            _smsSentBroadcastReceiver = new SMSSentReceiver();
            _smsDeliveredBroadcastReceiver = new SMSDeliveredReceiver();

            RegisterReceiver(_smsSentBroadcastReceiver, new IntentFilter("SMS_SENT"));
            RegisterReceiver(_smsDeliveredBroadcastReceiver, new IntentFilter("SMS_DELIVERED"));
        }

        protected override void OnPause()
        {
            base.OnPause();

            UnregisterReceiver(_smsSentBroadcastReceiver);
            UnregisterReceiver(_smsDeliveredBroadcastReceiver);
        }


    }

}

