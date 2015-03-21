/*
 * VoliBot GUI a.k.a. RitoBot GUI is part of the opensource VoliBot AutoQueuer project.
 * Credits to: shalzuth, Maufeat, imsosharp
 * Find assemblies for this AutoQueuer on LeagueSharp's official forum at:
 * http://www.joduska.me/
 * You are allowed to copy, edit and distribute this project,
 * as long as you don't touch this notice and you release your project with source.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using LoLLauncher;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
namespace RitoBot
{
    public partial class frm_MainWindow : Form
    {
        public frm_MainWindow()
        {
            InitializeComponent();

            loadConfiguration();
            Print("VoliBot GUI RC2 Loaded.");
            Print("Volibot's ready for Version: " + Program.cversion.Substring(0,4));
            Print("brought to you by imsosharp.", 4);
        }
        public bool validate()
        {
            bool result = true;
            if (LauncherPathInput.Text.Length==0||MaxBotsInput.Text.Length==0||MaxLevelInput.Text.Length==0||DefaultChampionInput.Text.Length==0||Spell1Input.Text.Length==0||Spell2Input.Text.Length==0||RegionInput.Text.Length==0||BuyBoostInput.Text.Length==0)
            {
                MessageBox.Show("Vui lòng nhập đầy đủ config");
                result=false;
            }
           
            return result;
        }
        public void loadConfiguration()
        {
            Program.loadConfiguration();
            var accountsTxtPath = AppDomain.CurrentDomain.BaseDirectory + "config\\accounts.txt";
            TextReader tr = File.OpenText(accountsTxtPath);
            
            string Acc = tr.ReadLine().ToString();
            tr.Close();
            string[] stringSeparators = new string[] { "|" };

            try
            {
                
                if (Acc != "")
                {
                    var result = Acc.Split(stringSeparators, StringSplitOptions.None);
                    newUserNameInput.Text = result[0];
                    QueueTypeInput.Text = result[1];
                    //ShowAccount(Acc.Split(stringSeparators, StringSplitOptions.None).ToString(), "", "");
                }
                LauncherPathInput.Text=Program.Path2;
                //.Text=Program.LoadGUI;
                MaxBotsInput.Text=Program.maxBots.ToString();
                MaxLevelInput.Text=Program.maxLevel.ToString();
                DefaultChampionInput.Text=Program.championId;
                Spell1Input.Text=Program.spell1;
                Spell2Input.Text=Program.spell2;
                //.Text=Program.rndSpell;
                //.Text=Program.replaceConfig;
                //.Text=Program.AutoUpdate;
                //Account
                
                RegionInput.Text=Program.Region;
                BuyBoostInput.Text = Program.buyBoost == false ? "NO" : "Need Price 3 win ip boost / regions";
               
            }
            catch (Exception e)
            {
                
                Thread.Sleep(10000);
                Application.Exit();
            }
        }
        public void Print(string text)
        {
            console.AppendText("[" + DateTime.Now + "] : " + text + "\n");
        }
        public void Print(string text, int newlines)
        {
            console.AppendText("[" + DateTime.Now + "] : " + text);
            for (int i = 0; i < newlines; i++)
            {
                console.AppendText("\n");
            }
        }
        public void ShowAccount(string account, string password, string queuetype)
        {
            LoadedAccounts.AppendText("A: " + account + " Pw: " + password + " Q: " + queuetype );
        }

        private void addAccountsBtn_Click(object sender, EventArgs e)
        {
            if (newUserNameInput.Text.Length == 0 || QueueTypeInput.Text.Length == 0)
            {
                MessageBox.Show("Username and Q type cannot be empty!!!");
            }
            
            else
            {
                if (QueueTypeInput.SelectedIndex == -1 && SelectChampionInput.SelectedIndex == -1) FileHandlers.AccountsTxt(newUserNameInput.Text, newPasswordInput.Text);
                else if (SelectChampionInput.SelectedIndex == -1) FileHandlers.AccountsTxt(newUserNameInput.Text, newPasswordInput.Text, QueueTypeInput.SelectedItem.ToString());
                else FileHandlers.AccountsTxt(newUserNameInput.Text, newPasswordInput.Text, QueueTypeInput.SelectedItem.ToString(), SelectChampionInput.SelectedItem.ToString());
                start();
                
                //queueLoop();
            }
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            FileHandlers.SettingsIni(LauncherPathInput.Text, MaxBotsInput.Text, MaxLevelInput.Text, DefaultChampionInput.SelectedItem.ToString(), Spell1Input.SelectedItem.ToString(), Spell2Input.SelectedItem.ToString(), RegionInput.SelectedItem.ToString(), BuyBoostInput.SelectedItem.ToString());
            Program.loadConfiguration();
        }

        private void replaceConfigBtn_Click(object sender, EventArgs e)
        {
            Print("Game configuration was optimized successfuly!");
            Program.gamecfg();
        }
        public void start()
        {
            Print("Starting Queue Loop");
            ReloadAccounts:
            Program.loadAccounts();
            Thread.Sleep(1000);
            int curRunning = 0;
            foreach (string acc in Program.accounts)
            {
                try
                {
                    Program.accounts2.RemoveAt(0);
                    string Accs = acc;
                    string[] stringSeparators = new string[] { "|" };
                    bool lead = false;
                    string token = "";
                    var result = Accs.Split(stringSeparators, StringSplitOptions.None);
                    curRunning += 1;
                    if (result[0].Contains("username"))
                    {
                        //Console.WriteLine("Please add your accounts into config\\accounts.txt");
                        goto ReloadAccounts;
                    }
                    /* if (result[3].Contains("Leader") || result.Contains("leader"))
                     {
                         lead = true;
                     }*/
                    Print("Đang chờ get token - Hãy mở game từ garena");

                    token = Program.GetGarenaToken();
                    Print("Get token thành công");
                    if (result[1] != null)
                    {
                        QueueTypes queuetype = (QueueTypes)System.Enum.Parse(typeof(QueueTypes), result[1]);
                        RiotBot ritoBot = new RiotBot(result[0], token, Program.Region, Program.Path2, curRunning, queuetype, lead);
                    }
                    else
                    {
                        QueueTypes queuetype = QueueTypes.ARAM;
                        RiotBot ritoBot = new RiotBot(result[0], token, Program.Region, Program.Path2, curRunning, queuetype, lead);
                    }
                    Print("RitoBot | Currently connected: " + Program.connectedAccs);
                    if (result[1] == "CUSTOM_HA_3x3")
                    {
                        while (!Program.IsGameCreated)
                            System.Threading.Thread.Sleep(1000);
                    }

                    if (curRunning == Program.maxBots)
                        break;
                }
                catch (Exception)
                {
                    Print("CountAccError: You may have an issue in your accounts.txt");
                    Application.Exit();
                }
            }
        }
        private void queueLoop()
        {
            foreach (string acc in Program.accounts)
            {
                int curRunning = 0;
                try
                {
                    Program.accounts2.RemoveAt(0);
                    string Accs = acc;
                    string[] stringSeparators = new string[] { "|" };
                    var result = Accs.Split(stringSeparators, StringSplitOptions.None);
                    console.ForeColor = Color.Lime;
                    curRunning += 1;
                    if (result[0].Contains("username"))
                    {
                        Print("No accounts found. Please add an account.", 2);
                    }
                    if (result[2] != null)
                    {
                        QueueTypes queuetype = (QueueTypes)System.Enum.Parse(typeof(QueueTypes), result[2]);
                       // RiotBot ritoBot = new RiotBot(result[0], result[1], Program.Region, Program.Path2, curRunning, queuetype, result[3]);
                        ShowAccount(result[0], result[1], result[2]);
                    }
                    else
                    {
                        QueueTypes queuetype = QueueTypes.ARAM;
                        //RiotBot ritoBot = new RiotBot(result[0], result[1], Program.Region, Program.Path2, curRunning, queuetype, result[3]);
                        ShowAccount(result[0], result[1], "ARAM");
                    }
                    Program.MainWindow.Text = " Volibot GUI | Currently connected: " + Program.connectedAccs;
                    if (curRunning == Program.maxBots)
                        break;
                }
                catch (Exception)
                {
                    console.ForeColor = Color.Red;
                    Print("You may have an error in accounts.txt.");
                    Print("If you just started Volibot for the first time,");
                    Print("add a new account on the leftside panel.");
                    Print("If you keep getting this error,");
                    Print("Delete accounts.txt and restart voli.", 2);
                }
            }
        }

        private void frm_MainWindow_Load(object sender, EventArgs e)
        {
            
            //start();
        }

        private void LauncherPathInput_Click(object sender, EventArgs e)
        {

        }


    }
}
