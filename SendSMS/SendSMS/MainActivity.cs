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
        EditText txtFileName;
        EditText txtColumnNumber;
        TextView lblInfo;
        List<string> SmsRecievers;

        private static readonly Regex ifExcelFile = new Regex(@"^.*\.(XLS|xls|XLSX|xlsx)$");
        private static readonly Regex everythingButN = new Regex(@"\D+");

        //const string SENT_SMS = "SENT_SMS";
        //Intent SentIntent = new Intent(SENT_SMS);
        //EditText edtxtNumber;
        //EditText edtxtText;


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
         }

         public void SendAndUpdate(List<Recipient> lst)
         {
             PendingIntent sentPI = PendingIntent.GetBroadcast(this, 100, SentIntent, 0);
             foreach (var item in lst)
             {

                 SmsManager.Default.SendTextMessage(item.Number, null, item.Text, sentPI, null);
                 sentPI.
             }
         }
         protected override void OnCreatea(Bundle savedInstanceState)
         {
             base.OnCreate(savedInstanceState);

             // Set our view from the "main" layout resource
             SetContentView(Resource.Layout.Main);

             SmsRecievers = new List<Recipient>();

             btnLoad = FindViewById<Button>(Resource.Id.btnLoad);
             txtData = FindViewById<TextView>(Resource.Id.txtData);

             var connString = "Server=tcp:wew7wzaijn.database.windows.net,1433;Database=booklocalservice;User ID=blsuser@wew7wzaijn;Password=YQfm5I1ZgPx3GkS;";

             btnLoad.Click += async delegate
             {
                 Task<List<Recipient>> loadListTask = Download(connString);
                 await loadListTask;
             };



         }*/


        string message = "Test message here";

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
            txtFileName = FindViewById(Resource.Id.txtFilename) as EditText;
            txtColumnNumber = FindViewById(Resource.Id.txtNumberClmn) as EditText;
            lblInfo = FindViewById(Resource.Id.lblInfo) as TextView;


            txtFileName.TextChanged += TxtFileName_TextChanged;
            txtColumnNumber.TextChanged += TxtColumnNumber_TextChanged;
            sendSmsButton.Click += SendSmsButton_Click;
            loadDataButton.Click += LoadDataButton_Click;


            _smsManager = SmsManager.Default;

        }

        public static string ReplaceEButN(string input, string replacement)
        {
            return "+"+everythingButN.Replace(input, replacement);
        }

        private void LoadDataButton_Click(object sender, EventArgs e)
        {
            //try
            //{
                string filename = txtFileName.Text;
                ushort columnNr;
                if(!UInt16.TryParse(txtColumnNumber.Text, out columnNr))
                {
                    lblInfo.Text = "Invalid column number";
                    return;
                }
                if (!ifExcelFile.IsMatch(filename))
                {
                    lblInfo.Text = filename+" is not an Excel document (maybe it lacks .xlsx extension?)";
                    return;
                }
                SmsRecievers = LoadNumbers(filename, columnNr);
                if (SmsRecievers.Count > 0)
                {
                    lblInfo.Text = "Data loaded succesfully";
                    sendSmsButton.Clickable = true;
                }
                else
                {
                    lblInfo.Text = "Cannot load the data";
                }

            //}
            //catch (Exception ex)
            //{

                //throw ex;
                //lblInfo.Text = "Catchen an exception: " + ex.Message;
            //}



           /* string content = "Jason rules";
            string filename = "file.txt";

            var documents = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            if (!Directory.Exists(documents))
            {
                Console.WriteLine("Directory does not exist.");
            }
            else
            {
                Console.WriteLine("Directory exists.");

                File.WriteAllText(documents + @"/" + filename, content);

                if (!File.Exists(documents + @"/" + filename))
                {
                    Console.WriteLine("Document not found.");
                }
                else
                {
                    string newContent = File.ReadAllText(documents + @"/" + filename);

                    //TextView viewer = FindViewById<TextView>(Resource.Id.textView1);
                    if (lblInfo != null)
                    {
                        lblInfo.Text = newContent;
                    }
                }
            }*/


        }

        private void SendSmsButton_Click(object sender, EventArgs e)
        {
            //var phone = phoneNumberEditText.Text;
            //var message = messageEditText.Text;
            if (!(SmsRecievers.Count > 0))
            {
                lblInfo.Text = "Unable to send messages: no numbers loaded";
                return;
            }


            var piSent = PendingIntent.GetBroadcast(this, 0, new Intent("SMS_SENT"), 0);
            var piDelivered = PendingIntent.GetBroadcast(this, 0, new Intent("SMS_DELIVERED"), 0);


            foreach (var item in SmsRecievers)
            {
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
            
            var numbers = new List<string>();

            //Creates a new instance for ExcelEngine.
            ExcelEngine excelEngine = new ExcelEngine();


            var pathFile = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads);
            var absolutePath = pathFile.AbsolutePath;

            var pth = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).ToString();

            var documentsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments);

            var filePath = global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            //var filePath = "/storage/emulated/0/Download";
            var path = Path.Combine(pth, filename);
            Console.WriteLine("=============Trynig to load data: path - "+path+" column number: "+clmnNumber+" ===============\n");



            var fs = System.IO.File.Open(path, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);



            //Loads or open an existing workbook through Open method of IWorkbooks
            try
            {
                Console.WriteLine("------------- inside try-catch ---------------\n");
                IWorkbook workbook = excelEngine.Excel.Workbooks.Open(fs);
                Console.WriteLine("Path to file is: " + path);
                IWorksheet sheet = workbook.Worksheets[0];
                string num;
                for (int i = 1; i < 10000; i++)
                {
                    //replace everything but numbers
                    num = ReplaceEButN(sheet.GetText(i, clmnNumber), "");
                    if (num != "")
                    {
                        numbers.Add(num);
                    }
                }
            }
            catch (Exception ex)
            {
                lblInfo.Text = "Unable to load data. Message: " + ex.Message;
            }
            
            
     

            return numbers;
        }


        private void CheckAppPermissions()
        {
            /*var thisActivity = Forms.Context as Activity;
            ActivityCompat.RequestPermissions(thisActivity, new string[] {
Manifest.Permission.AccessFineLocation }, 1);
            ActivityCompat.RequestPermissions(thisActivity,
            new String[] { Manifest.Permission.AccessFineLocation },
            1);*/

            Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads);

            /*if ((int)Build.VERSION.SdkInt < 23)
            {
                return;
            }
            else
            {
                if (PackageManager.CheckPermission(Manifest.Permission.ReadExternalStorage, PackageName) != Permission.Granted
                    && PackageManager.CheckPermission(Manifest.Permission.WriteExternalStorage, PackageName) != Permission.Granted)
                {
                    var permissions = new string[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage };
                    RequestPermissions(permissions, 1);
                }
            }*/
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


    /*[BroadcastReceiver]
    public class SMSBroadcast : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            switch (this.ResultCode)
            {
                case Result.Ok:
                    Console.Write("==========SMS SENT!!!==================");
                    
                    break;
            }
            if (ResultCode == (Result)SmsResultError.GenericFailure)
            {
                Console.WriteLine("senging failed");
            }
        }
    }*/

}

