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

namespace SendSMS
{
    [Activity(Label = "SendSMS", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        Button btnLoad;
        TextView txtData;
        List<Recipient> SmsRecievers;
        //EditText edtxtNumber;
        //EditText edtxtText;

        protected override void OnCreate(Bundle savedInstanceState)
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
            
            

        }

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
            foreach (var item in lst)
            {
                PendingIntent sentPI = PendingIntent.GetBroadcast(this, 100, new Intent(this, typeof(SMSBroadcast)), 0);
                SmsManager.Default.SendTextMessage(item.Number, null, item.Text, sentPI, null);
                sentPI.
            }
        }
    }


    [BroadcastReceiver]
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
    }

}

