using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Java.Util;

namespace AppBlutoothArduino
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        ToggleButton tgConnect;
        ToggleButton tgOffOn;
        TextView Result;
        private Java.Lang.String dataToSend;
        private BluetoothAdapter mBluetoothAdapter = null;
        private BluetoothSocket btSocket = null;
        private Stream outStream = null;
        private static string address = "HC-06";
        private static UUID MY_UUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        private Stream inStream = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            tgConnect = FindViewById<ToggleButton>(Resource.Id.toggleButton1);
            tgOffOn= FindViewById<ToggleButton>(Resource.Id.toggleButtonOn);
            Result = FindViewById<TextView>(Resource.Id.textView1);
            tgOffOn.CheckedChange+= tgOnOff_HandleCheckedChange;
            tgConnect.CheckedChange += tgConnect_HandleCheckedChange;
            CheckBt();



        }

        private void CheckBt()
        {
            mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;

            if (!mBluetoothAdapter.Enable())
            {
                Toast.MakeText(this, "Bluetooth Desactivado",
                    ToastLength.Short).Show();
            }

            if (mBluetoothAdapter == null)
            {
                Toast.MakeText(this,
                    "Bluetooth No Existe o esta Ocupado", ToastLength.Short)
                    .Show();
            }
        }

        void tgConnect_HandleCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (e.IsChecked)
            {
                Connect();
            }
            else
            {
                if (btSocket.IsConnected)
                {
                    try
                    {
                        btSocket.Close();
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        void tgOnOff_HandleCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (e.IsChecked)
            {
                dataToSend = new Java.Lang.String("1");
                writeData(dataToSend);
            }
            else
            {
                dataToSend = new Java.Lang.String("0");
                writeData(dataToSend);
            }
        }
        public void Connect()
        {
           
            BluetoothDevice device = (from bd in this.mBluetoothAdapter.BondedDevices where bd.Name == "HC-06" select bd).FirstOrDefault();
            System.Console.WriteLine("Conexion en curso" + device);
            mBluetoothAdapter.CancelDiscovery();
            try
            {
                btSocket = device.CreateRfcommSocketToServiceRecord(MY_UUID);
                btSocket.Connect();
                System.Console.WriteLine("Conexion Correcta");
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
                try
                {
                   // dataToSend = new Java.Lang.String("0");
                   // writeData(dataToSend);
                    btSocket.Close();
                }
                catch (System.Exception)
                {
                    System.Console.WriteLine("Imposible Conectar");
                }
                System.Console.WriteLine("Socket Creado");
            }
            beginListenForData();
            //dataToSend = new Java.Lang.String("0");
           // writeData(dataToSend);
           
        }

        public void beginListenForData()
        {
            try
            {
                inStream = btSocket.InputStream;
            }
            catch (System.IO.IOException ex)
            {
                Console.WriteLine(ex.Message);
            }
            Task.Factory.StartNew(async () => {
                byte[] buffer = new byte[1];
             
                int bytes;
                while (true)
                {
                    try
                    {
                        bytes = await inStream.ReadAsync(buffer, 0, buffer.Length);
                        while (bytes > 0)
                        {
                            
                            RunOnUiThread(() => {
                                // string valor = System.Text.Encoding.ASCII.GetString(buffer);
                                for(int i=0; i<bytes; i++)
                                {
                                    Result.Text = Result.Text + buffer[i].ToString() + "\t";

                                }
                              
                            });
                            bytes = await inStream.ReadAsync(buffer, 0, buffer.Length);
                            if(bytes<=0)
                            {
                                Result.Text = "\n";
                            }

                        }
                    }
                    catch (Java.IO.IOException)
                    {
                        
                    }
                }
            });
        }

        private void writeData(Java.Lang.String data)
        {
            try
            {
                outStream = btSocket.OutputStream;
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("Error al enviar" + e.Message);
            }

            Java.Lang.String message = data;

            byte[] msgBuffer = message.GetBytes();

            try
            {
                outStream.Write(msgBuffer, 0, msgBuffer.Length);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("Error al enviar" + e.Message);
            }
        }


    }
}

