﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace V2RayW
{

    public partial class ConfigForm : Form
    {
        public static List<Profile> profiles = new List<Profile>();
        public static int selectedServerIndex;

        public ConfigForm()
        {
            InitializeComponent();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            // change profiles in memory
            Program.profiles.Clear();
            if (profiles.Count > 0)
            {
                foreach (Profile p in profiles)
                {
                    Program.profiles.Add(p.DeepCopy());
                }
            } else
            {
                Program.proxyIsOn = false;
            }
            Program.selectedServerIndex = ConfigForm.profiles.Count > 0 ? listBoxServers.SelectedIndex : -1;

            // save to file
            Properties.Settings.Default.useSocks5 = checkSocks5.Checked; // 0:socks, 1:http
            Properties.Settings.Default.socks5Port = Program.strToInt(textBoxSocks5Port.Text, 1080);
            Properties.Settings.Default.useHttp = checkHttp.Checked;
            Properties.Settings.Default.httpPort = Program.strToInt(textBoxHttpPort.Text, 1081);
            Properties.Settings.Default.udpSupport = checkBoxUDP.Checked;
            Properties.Settings.Default.dns = textBoxDNS.Text != "" ? textBoxDNS.Text : "localhost";
            var profileArray = Program.profiles.Select(p => Program.profileToStr(p));
            Properties.Settings.Default.profilesStr = String.Join("\t", profileArray);
            Properties.Settings.Default.alarmUnknown = checkBoxAlarm.Checked;
            Properties.Settings.Default.Save();

            Program.updateProxy();
            Program.mainForm.updateMenu();
            this.Close();
        }

        private void buttonTS_Click(object sender, EventArgs e)
        {
            var tsWindow = new FormTransSetting();
            tsWindow.ShowDialog(this);
        }

        private void ConfigForm_Load(object sender, EventArgs e)
        {
            checkBoxAlarm.Checked = Properties.Settings.Default.alarmUnknown;
            this.Icon = Properties.Resources.vw256;
            selectedServerIndex = Program.selectedServerIndex;
            profiles.Clear();
            foreach (Profile p in Program.profiles)
            {
                profiles.Add(p.DeepCopy());
            }

            //Properties.Settings.Default.Upgrade();
            checkSocks5.Checked = Properties.Settings.Default.useSocks5;
            textBoxSocks5Port.Text = Properties.Settings.Default.socks5Port.ToString();
            checkHttp.Checked = Properties.Settings.Default.useHttp;
            textBoxHttpPort.Text = Properties.Settings.Default.httpPort.ToString();
            checkBoxUDP.Checked = Properties.Settings.Default.udpSupport;
            textBoxDNS.Text = Properties.Settings.Default.dns;
            loadProfiles();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            profiles.Add(new Profile());
            selectedServerIndex = profiles.Count() - 1;
            /*
            for(int i = 0; i < profiles.Count(); i++)
            {
                Debug.WriteLine(profiles[i].address);
            }
            Debug.WriteLine(selectedServerIndex);*/
            this.loadProfiles();
        }

        private void loadProfiles()
        {
            listBoxServers.Items.Clear();
            foreach (var p in profiles)
            {
                listBoxServers.Items.Add(p.remark == "" ? p.address : p.remark);
            }
            if (selectedServerIndex >= 0)
            {
                listBoxServers.SelectedIndex = selectedServerIndex;
            } else
            {
                textBoxAddress.Text = "";
                textBoxPort.Text = "";
                textBoxUserId.Text = "";
                textBoxAlterID.Text = "";
                textBoxRemark.Text = "";
                checkBoxAllowP.Checked = false;
                comboBoxNetwork.SelectedIndex = 0;
                comboBoxSecurity.SelectedIndex = 0;
            }
        }

        private void listBoxServers_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedServerIndex = listBoxServers.SelectedIndex;
            var sp = profiles[selectedServerIndex];
            textBoxAddress.Text = sp.address;
            textBoxPort.Text = sp.port.ToString();
            textBoxUserId.Text = sp.userId;
            textBoxAlterID.Text = sp.alterId.ToString();
            textBoxRemark.Text = sp.remark;
            checkBoxAllowP.Checked = sp.allowPassive;
            comboBoxNetwork.SelectedIndex = sp.network;
            comboBoxSecurity.SelectedIndex = sp.security;
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (profiles.Count <= 0)
            {
                return;
            }
            profiles.RemoveAt(selectedServerIndex);
            if(selectedServerIndex >= profiles.Count())
            {
                selectedServerIndex -= 1;
            }
            loadProfiles();
        }

        private void textBoxAddress_TextChanged(object sender, EventArgs e)
        {
            if(selectedServerIndex >= 0)
            {
                profiles[selectedServerIndex].address = textBoxAddress.Text;
                loadProfiles();
            }
        }

        private void textBoxPort_TextChanged(object sender, EventArgs e)
        {
            if (selectedServerIndex >= 0)
            {
                profiles[selectedServerIndex].port = Program.strToInt(textBoxPort.Text, 10086);
            }
        }

        private void textBoxUserId_TextChanged(object sender, EventArgs e)
        {
            if (selectedServerIndex >= 0)
            {
                profiles[selectedServerIndex].userId = textBoxUserId.Text;
            }
        }

        private void textBoxAlterID_TextChanged(object sender, EventArgs e)
        {
            if (selectedServerIndex >= 0)
            {
                profiles[selectedServerIndex].alterId = Program.strToInt(textBoxAlterID.Text, 0);
            }
        }

        private void textBoxRemark_TextChanged(object sender, EventArgs e)
        {
            if (selectedServerIndex >= 0)
            {
                profiles[selectedServerIndex].remark = textBoxRemark.Text;
                loadProfiles();
            }
        }

        private void checkBoxAllowP_CheckedChanged(object sender, EventArgs e)
        {
            if (selectedServerIndex >= 0)
            {
                profiles[selectedServerIndex].allowPassive = checkBoxAllowP.Checked;
            }
        }

        private void comboBoxNetwork_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectedServerIndex >= 0)
            {
                profiles[selectedServerIndex].network = comboBoxNetwork.SelectedIndex;
            }
        }

        private void comboBoxSecurity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectedServerIndex >= 0)
            {
                profiles[selectedServerIndex].security = comboBoxSecurity.SelectedIndex;
            }
        }

        private void checkSocks5_CheckedChanged(object sender, EventArgs e)
        {
            if(!(checkSocks5.Checked || checkHttp.Checked))
            {
                checkHttp.Checked = true;
            }
            textBoxSocks5Port.Enabled = checkSocks5.Checked;
            checkBoxUDP.Enabled = checkSocks5.Checked;
        }

        private void checkHttp_CheckedChanged(object sender, EventArgs e)
        {
            if (!(checkSocks5.Checked || checkHttp.Checked))
            {
                checkSocks5.Checked = true;
            }
            textBoxHttpPort.Enabled = checkHttp.Checked;
        }
    }
}
