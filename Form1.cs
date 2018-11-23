using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using System.Speech.Synthesis;

namespace chart
{
   
    public partial class Form1 : Form
    {
        SpeechSynthesizer speech = new SpeechSynthesizer();

        
        public Form1()
        {
            InitializeComponent();
        }

        // create variable needed
        double sumHeartRate = 0; // add all the values end store the result 
        double avrHeartRate; // store the heartRate averge value 
        double avrPerHour = 60; // use this ti calculate average per hours 
        int counter = 1;
        int iNo = 0;
        private void ConnectToDatabase()
        {
            string conString = "Data Source=mydatabase.clhuhhzgfuc8.eu-west-2.rds.amazonaws.com;Initial Catalog=test_Database;User ID=Eduard;Password=qazwsxedc";           
            SqlConnection con = new SqlConnection(conString);
            SqlCommand cmd;                 
            con.Open();

           

               // to use for insert/update/delete
               /* string query = "";
                cmd = new SqlCommand(query, con); // initialize the command object with the constructor and pass the query and connection object 
                cmd.ExecuteNonQuery(); // execute the comand */
            
           // use to select 
                string query = "Select heart_rate FROM Heart_Rate_DB WHERE ID = " + iNo;
                cmd = new SqlCommand(query, con); // initialize the command object with the constructor and pass the query and connection object 
                SqlDataReader readData; // create readder object for database 

                readData = cmd.ExecuteReader(); // execute the reader 

                while (readData.Read()) // loop this until no value will be found within the query statement 
                {
                    sumHeartRate += Convert.ToDouble(readData["heart_rate"].ToString()); // add all the values together 
                    avrHeartRate = sumHeartRate / counter; // calculate the average heart rate for one day 
                    avrPerHour += 60; // use this in if statement to calculate the average pe hours

                   // heartRateBar.Value = Convert.ToInt32(readData["heart_rate"].ToString()); // show heart rate level on progress bar 
                   // lblHeartRate.Text = readData["heart_rate"].ToString(); // show heart rate level on text

                   // UpdateChart(iNo, Convert.ToDouble(readData["heart_rate"].ToString()));
                       // this.Invoke((MethodInvoker)delegate { UpdateChart(iNo, Convert.ToDouble(readData["heart_rate"].ToString())); });

            }
                iNo++;
                counter++;
            
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lblWarrning.Visible = false;
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            readCSV();
        }
        
        private void btnStart_Click(object sender, EventArgs e)
        {
            timer1.Start();
            timer1.Interval = 50;
            
        }
        int highValueXYZ = 0;
        int cNo = 0;

        int countWarning = 0;
        private void readCSV()
        {
            List<string> biomatricData = new List<string>();
            List<string> roomTempData = new List<string>();
            List<string> lightLuxData = new List<string>();

            System.Drawing.Point point = Control.MousePosition;

            string date, heartRate, bodyTemp, roomTemp, lux, line1;


            string filePathBiomatrics = @"C:\Users\Eduard\Desktop\Data from AWS\csv Files\Biometrics2.csv";
            try
            {
                if (File.Exists(filePathBiomatrics))
                {
                    var lineCount = File.ReadLines(filePathBiomatrics).Count();//read text file line count to establish length for array
                    if (counter < 2)
                    {

                    }

                    if (counter < lineCount && lineCount > 0)//if counter is less than lineCount keep reading lines
                    {
                        line1 = File.ReadLines(filePathBiomatrics).Skip(counter).Take(lineCount).First();

                        biomatricData = line1.Split(',').ToList();
                        heartRate = biomatricData[2];
                        bodyTemp = biomatricData[3];
                        lux = biomatricData[4];
                        roomTemp = biomatricData[5];


                        int[] sensor = new int[5];

                        sensor[0] = Convert.ToInt32(Convert.ToDouble(heartRate));
                        sensor[1] = Convert.ToInt32(Convert.ToDouble(bodyTemp));
                        sensor[2] = Convert.ToInt32(Convert.ToDouble(roomTemp));
                        sensor[3] = Convert.ToInt32(Convert.ToDouble(lux));

                        VisualStorageRepresentation(220, sensor[0], panelHartRateEmpty);
                        VisualStorageRepresentation(40, sensor[1], panelBodyTemp);
                        VisualStorageRepresentation(40, sensor[2], panelRoomTempEmpty);
                        VisualStorageRepresentation(1000, sensor[3], panelLuxEmpty);
                        VisualStorageRepresentation(1000, Convert.ToInt32(Convert.ToDouble(point.X)), panelAccXFull, false);
                        VisualStorageRepresentation(1000, Convert.ToInt32(Convert.ToDouble(point.Y)), panelAccYFull, false);
                        VisualStorageRepresentation(1000, Convert.ToInt32(Convert.ToDouble(point.Y + point.X / 2)), panelAccZFull, false);



                        lblHartRValue.Text = sensor[0].ToString() + " bpm";
                        lblBodyTempValue.Text = sensor[1].ToString() + " C";
                        lblLuxValue.Text = sensor[3].ToString() + " lux";
                        lblRoomTempValue.Text = sensor[2].ToString() + " C";

                        lblAccX.Text = "X " + point.X.ToString();
                        lblAccY.Text = "Y " + point.Y.ToString();
                        lblAccZ.Text = "Z " + Convert.ToString(point.Y + point.X);

                        if (point.X < 300)
                        {
                            highValueXYZ++;
                        }
                        int warn = 0;
                        if (highValueXYZ == 8)
                        {
                            warn = 1;

                            ListViewItem lst = new ListViewItem(sensor[0].ToString());
                            lst.SubItems.Add(sensor[1].ToString());
                            lst.SubItems.Add(sensor[3].ToString());
                            lst.SubItems.Add(sensor[2].ToString());

                            lstWarrnings.Items.Add(lst);

                            lblWarrning.Visible = true;
                            highValueXYZ = 0;
                            Speak();


                        }
                        countWarning++;

                        

                        if (countWarning == 10)
                        {
                            
                            lblWarrning.Visible = false;
                            countWarning = 0;
                           
                           

                        }
                        cNo++;
                        counter++;
                    }
                    else
                    {
                        counter = 0;
                        timer1.Enabled = false;
                    }
                }
            }
            catch
            {
                counter = 0;
                timer1.Enabled = false;
            }
           
        }

        private void Speak()
        {
           
                speech.SpeakAsync("Warning!");
            
           
        }

        private void VisualStorageRepresentation(int maxCapacity, int actualCapacity, Panel panel,bool height = true)
        {
            // variables to storage the result
            double capacityPercentage;
            double heightPercentage;

            // this calculate the percentage of the actual capacity
            capacityPercentage = (double)actualCapacity / (double)maxCapacity * 100;

            // with this calculation i want to find out how much means the capacity percentage in panels max height 
            heightPercentage = (double)428 / (double)100 * capacityPercentage;

            if (height)
            {
                // visual representation of the storage 
                panel.Height = 428 - (int)heightPercentage;
            }
            else
            {
                panel.Width = 492 - (int)heightPercentage;
            }
           

        }

        private void btnAnalyseData_Click(object sender, EventArgs e)
        {
            double sHrValue1, sHrValue2,sBtValue1,sBtValue2,sLuxValue1,sLuxValue2,sRtValue1,sRtValue2;
            int[] count = new int[4];

            for (int i = 0; i < count.Length; i++)
            {
                count[i] = 0;
            }

            Dictionary<double, int> analyseHeartRate= new Dictionary<double, int>();
            Dictionary<double, int> analyseBodyTemp = new Dictionary<double, int>();
            Dictionary<double, int> analyseLux = new Dictionary<double, int>();
            Dictionary<double, int> analyseRoomTemp = new Dictionary<double, int>();

           

             sHrValue1 = Convert.ToInt32(lstWarrnings.Items[0].SubItems[0].Text);
             sHrValue2 = Convert.ToDouble(lstWarrnings.Items[1].SubItems[0].Text);
             sBtValue1 = Convert.ToDouble(lstWarrnings.Items[0].SubItems[1].Text);
             sBtValue2 = Convert.ToDouble(lstWarrnings.Items[1].SubItems[1].Text);
             sLuxValue1 = Convert.ToDouble(lstWarrnings.Items[0].SubItems[2].Text);
             sLuxValue2 = Convert.ToDouble(lstWarrnings.Items[0].SubItems[2].Text);
             sRtValue1 = Convert.ToDouble(lstWarrnings.Items[0].SubItems[3].Text);
             sRtValue2 = Convert.ToDouble(lstWarrnings.Items[0].SubItems[3].Text);




             for (int i = 2; i < lstWarrnings.Items.Count; i++)
             {
                 if (sHrValue1 == sHrValue2)
                 {
                     count[0]++;
                     sHrValue2 = Convert.ToDouble(lstWarrnings.Items[i].SubItems[0].Text);
                 }
                 else
                 {
                     analyseHeartRate.Add(sHrValue1, count[0]);
                     count[0] = 0;
                     sHrValue1 = Convert.ToDouble(lstWarrnings.Items[i].SubItems[0].Text);
                 }

                 if (sBtValue1 == sBtValue2)
                 {
                     count[1]++;
                     sBtValue2 = Convert.ToDouble(lstWarrnings.Items[i].SubItems[1].Text);
                 }
                 else
                 {
                     analyseBodyTemp.Add(sBtValue1, count[1]);
                     count[1] = 0;
                     sBtValue1 = Convert.ToDouble(lstWarrnings.Items[i].SubItems[1].Text);
                 }

                 if (sLuxValue1 == sLuxValue2)
                 {
                     count[2]++;
                     sLuxValue2 = Convert.ToDouble(lstWarrnings.Items[i].SubItems[2].Text);
                 }
                 else
                 {
                     analyseLux.Add(sLuxValue1, count[2]);
                     count[2] = 0;
                     sLuxValue1 = Convert.ToDouble(lstWarrnings.Items[i].SubItems[2].Text);
                 }

                 if (sRtValue1 == sRtValue2)
                 {
                     count[3]++;
                     sRtValue2 = Convert.ToDouble(lstWarrnings.Items[i].SubItems[3].Text);
                 }
                 else
                 {
                     analyseRoomTemp.Add(sRtValue1, count[3]);
                     count[3] = 0;
                     sRtValue1 = Convert.ToDouble(lstWarrnings.Items[i].SubItems[3].Text);

                 }


             }
             

        }

        //Analyse Data
        private double TheBiggesValue(Dictionary<double, int> dic)
        {
            double compareA, compareB, saveC = 0;
            bool runOnes = false;
            

            List<double> dKeys = new List<double>();
            List<int> dValues = new List<int>();

            foreach (KeyValuePair<double, int> item in dic)
            {
                dKeys.Add(item.Key);
                dValues.Add(item.Value);
            }
            
            if (dKeys.Count() != 1)
            {
                compareA = dValues[0];

                compareB = dValues[1];

                for (int i = 2; i < dValues.Count(); i++)
                {
                    if (compareA < compareB)
                    {
                        compareA = dValues[i];
                    }
                    else if (compareA > compareB)
                    {
                        saveC = dKeys[i];
                        compareB = dValues[i];
                    }
                }
            }
            return saveC;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
        }
    }
}
