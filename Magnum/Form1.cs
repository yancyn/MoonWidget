using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Magnum
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso>http://www.my.com.my/oap/10/my_10k/how.asp</seealso>
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.my.com.my/oap/10/my_10k/how.asp");
        }
        private void BindListBox(object sender, List<string> source)
        {
            (sender as ListBox).Items.Clear();
            foreach (string s in source)
                (sender as ListBox).Items.Add(s);

        }

        // x1+x2+x3+x4 case
        private void numericUpDown2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int[] sources = new int[10];
                List<string> output = new List<string>();//desire output string
                string sourceText = numericUpDown1.Value.ToString();
                for (int i = 0; i < sources.Length; i++) //ignore the rest of character if too long
                    sources[i] = Convert.ToInt32(sourceText.Substring(i, 1));
                int answer = (int)(sender as NumericUpDown).Value;
                string result = string.Empty;
                int counter = 0;
                //apply permutation calculation here
                System.Diagnostics.Debug.WriteLine("Start: " + System.DateTime.Now);
                for (int i = 0; i < sources.Length; i++)
                {
                    result = sources[i].ToString();
                    for (int j = 0; j < sources.Length; j++)
                    {
                        if (j == i) continue;
                        result = sources[i].ToString() + sources[j].ToString();
                        for (int k = 0; k < sources.Length; k++)
                        {
                            if (k == j) continue;
                            if (k == i) continue;//if (k != j && k != i)
                            result = sources[i].ToString() + sources[j].ToString() + sources[k].ToString();
                            for (int l = 0; l < sources.Length; l++)
                            {
                                if (l == i) continue;
                                if (l == j) continue;
                                if (l == k) continue;
                                //if (l != k && l != j && l != i)

                                counter += 1;
                                result = sources[i].ToString() + sources[j].ToString() + sources[k].ToString() + sources[l].ToString();
                                int newResult = SumAllString(result);
                                //System.Diagnostics.Debug.WriteLine(sources[i] + "+" + sources[j] + "+" + sources[k] + "+" + sources[l] + "=" + newResult);
                                if (newResult == answer)
                                {
                                    //System.Diagnostics.Debug.WriteLine(sources[i] + "+" + sources[j] + "+" + sources[k] + "+" + sources[l] + "=" + newResult);
                                    output.Add(result);
                                }
                            }
                        }
                    }
                }
                System.Diagnostics.Debug.WriteLine("End: " + System.DateTime.Now + " count: " + counter);

                //bind to listbox
                BindListBox(listBox2, output);
                label3.Text = output.Count + " found";
            }
        }
        private int SumAllString(string sender)
        {
            int output = 0;
            for (int i = 0; i < sender.Length; i++)
                output += Convert.ToInt32(sender.Substring(i, 1));
            return output;
        }

        //x1-x2-x3-x4 case
        private void numericUpDown3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int[] sources = new int[10];
                List<string> output = new List<string>();//desire output string
                string sourceText = numericUpDown1.Value.ToString();
                for (int i = 0; i < sources.Length; i++) //ignore the rest of character if too long
                    sources[i] = Convert.ToInt32(sourceText.Substring(i, 1));
                int answer = (int)(sender as NumericUpDown).Value;
                string result = string.Empty;
                int counter = 0;
                //apply permutation calculation here
                System.Diagnostics.Debug.WriteLine("Start: " + System.DateTime.Now);
                for (int i = 0; i < sources.Length; i++)
                {
                    result = sources[i].ToString();
                    for (int j = 0; j < sources.Length; j++)
                    {
                        if (j == i) continue;
                        result = sources[i].ToString() + sources[j].ToString();
                        for (int k = 0; k < sources.Length; k++)
                        {
                            if (k == j) continue;
                            if (k == i) continue;//if (k != j && k != i)
                            result = sources[i].ToString() + sources[j].ToString() + sources[k].ToString();
                            for (int l = 0; l < sources.Length; l++)
                            {
                                if (l == i) continue;
                                if (l == j) continue;
                                if (l == k) continue;
                                //if (l != k && l != j && l != i)

                                counter += 1;
                                result = sources[i].ToString() + sources[j].ToString() + sources[k].ToString() + sources[l].ToString();
                                int newResult = MinusAllString(result);
                                //System.Diagnostics.Debug.WriteLine(sources[i] + "+" + sources[j] + "+" + sources[k] + "+" + sources[l] + "=" + newResult);
                                if (newResult == answer)
                                {
                                    //System.Diagnostics.Debug.WriteLine(sources[i] + "+" + sources[j] + "+" + sources[k] + "+" + sources[l] + "=" + newResult);
                                    output.Add(result);
                                }
                            }
                        }
                    }
                }
                System.Diagnostics.Debug.WriteLine("End: " + System.DateTime.Now + " count: " + counter);

                //bind to listbox
                BindListBox(listBox3, output);
                label4.Text = output.Count + " found";
            }
        }
        private int MinusAllString(string sender)
        {
            int output = Convert.ToInt32(sender.Substring(0, 1));
            for (int i = 1; i < sender.Length; i++)
                output -= Convert.ToInt32(sender.Substring(i, 1));
            return output;
        }

        //x1+x2-x3+x4 case
        private void numericUpDown4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int[] sources = new int[10];
                List<string> output = new List<string>();//desire output string
                string sourceText = numericUpDown1.Value.ToString();
                for (int i = 0; i < sources.Length; i++) //ignore the rest of character if too long
                    sources[i] = Convert.ToInt32(sourceText.Substring(i, 1));
                int answer = (int)(sender as NumericUpDown).Value;
                string result = string.Empty;
                int counter = 0;
                //apply permutation calculation here
                System.Diagnostics.Debug.WriteLine("Start: " + System.DateTime.Now);
                for (int i = 0; i < sources.Length; i++)
                {
                    result = sources[i].ToString();
                    for (int j = 0; j < sources.Length; j++)
                    {
                        if (j == i) continue;
                        result = sources[i].ToString() + sources[j].ToString();
                        for (int k = 0; k < sources.Length; k++)
                        {
                            if (k == j) continue;
                            if (k == i) continue;//if (k != j && k != i)
                            result = sources[i].ToString() + sources[j].ToString() + sources[k].ToString();
                            for (int l = 0; l < sources.Length; l++)
                            {
                                if (l == i) continue;
                                if (l == j) continue;
                                if (l == k) continue;
                                //if (l != k && l != j && l != i)

                                counter += 1;
                                result = sources[i].ToString() + sources[j].ToString() + sources[k].ToString() + sources[l].ToString();
                                int newResult = PlusMinusPlusString(result);
                                //System.Diagnostics.Debug.WriteLine(sources[i] + "+" + sources[j] + "+" + sources[k] + "+" + sources[l] + "=" + newResult);
                                if (newResult == answer)
                                {
                                    //System.Diagnostics.Debug.WriteLine(sources[i] + "+" + sources[j] + "+" + sources[k] + "+" + sources[l] + "=" + newResult);
                                    output.Add(result);
                                }
                            }
                        }
                    }
                }
                System.Diagnostics.Debug.WriteLine("End: " + System.DateTime.Now + " count: " + counter);

                //bind to listbox
                BindListBox(listBox4, output);
                label6.Text = output.Count + " found";
            }
        }
        private int PlusMinusPlusString(string sender)
        {
            int x1 = Convert.ToInt32(sender.Substring(0, 1));
            int x2 = Convert.ToInt32(sender.Substring(1, 1));
            int x3 = Convert.ToInt32(sender.Substring(2, 1));
            int x4 = Convert.ToInt32(sender.Substring(3, 1));
            return x1 + x2 - x3 + x4;
        }
    }
}
