using Android.App;
using Android.Widget;
using Android.OS;
using System.Data.SqlClient;
using Android.Telephony;
using System.Threading.Tasks;
using System.Text;
using System;
using System.Collections.Generic;

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
            //edtxtNumber = FindViewById<EditText>(Resource.Id.edtxtNumber);
            //edtxtText = FindViewById<EditText>(Resource.Id.edtxtSMSText);

            var connString = "Server=tcp:wew7wzaijn.database.windows.net,1433;Database=booklocalservice;User ID=blsuser@wew7wzaijn;Password=YQfm5I1ZgPx3GkS;";
            StringBuilder data = new StringBuilder("Before DB");
            btnLoad.Click += delegate
            {
                btnLoad.Activated = false;
                using (var conn = new SqlConnection(connString))
                {
                    var cmdTxt = "SELECT * FROM SMS_test WHERE Sent IS NULL";
                    var cmnd = new SqlCommand(cmdTxt, conn);
                    conn.Open();
                    SqlDataReader dr = cmnd.ExecuteReader();
                    data.Clear();
                    while(dr.Read())
                    {
                        if (dr.HasRows == true)
                        {
                            SmsRecievers.Add(new Recipient(new Guid(Convert.ToString(dr["Id"])), Convert.ToString(dr["Phone"]), Convert.ToString(dr["Message"])));
                        }
                    }
                }
                txtData.Text = data.ToString();
                //Task.Run(() =>
                //{

                //});
                //SmsManager.Default.SendTextMessage(edtxtNumber.Text, null, edtxtText.Text, null, null);
                //btnSendSms.Activated = true;
            };

        }
    }
}

