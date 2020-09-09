using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace Haicro.Forms
{
    public partial class Form1 : Form
    {
        CancellationTokenSource source = new CancellationTokenSource();
        InputSimulator inputSimulator = new InputSimulator();
        public Form1()
        {
            InitializeComponent();
            foreach (GroupBox groupBox in Controls.OfType<GroupBox>())
            {
                foreach (TextBox textBox in groupBox.Controls.OfType<TextBox>())
                {
                    textBox.MouseClick += delegate
                    {
                        textBox.SelectAll();
                    };
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
            
        private async void StartMacro_btn_Click(object sender, EventArgs e)
        {
            string macroStatus = StartMacro_btn.Text;

            if (macroStatus == "START")
            {
                //disable all textbox on start
                foreach(GroupBox groupBox in Controls.OfType<GroupBox>())
                {
                    foreach (TextBox textBox in groupBox.Controls.OfType<TextBox>())
                    {
                        textBox.Enabled = false;
                    }
                }

                attackInterval_txt.Enabled = false;
                mobSelectorInterval_txt.Enabled = false;

                StartMacro_btn.Text = "3...";
                await Task.Delay(1000);
                StartMacro_btn.Text = "2...";
                await Task.Delay(1000);
                StartMacro_btn.Text = "1...";
                await Task.Delay(1000);

                List<string> attackKeys = new List<string>();
                foreach (TextBox key in attack_grp.Controls.OfType<TextBox>())
                {
                    if (!string.IsNullOrEmpty(key.Text))
                        attackKeys.Add(key.Text);
                }

                List<string> potionKeys = new List<string>();
                foreach (TextBox key in potions_grp.Controls.OfType<TextBox>())
                {
                    if (!string.IsNullOrEmpty(key.Text))
                        potionKeys.Add(key.Text);
                }

                source = new CancellationTokenSource();
                CancellationToken token = source.Token;

                var autoAttack = Task.Run(() => AutoAttack(token, attackKeys), token);
                var autoPotion = Task.Run(() => AutoPotion(token, potionKeys), token);

                if (!string.IsNullOrEmpty(mobSelectorKey_txt.Text))
                {
                    var autoSelector = Task.Run(() => AutoMobSelector(token), token);
                }

                StartMacro_btn.Text = "STOP";
            }
            else if (macroStatus == "STOP")
            {
                //enable all textbox on start
                foreach (GroupBox groupBox in Controls.OfType<GroupBox>())
                {
                    foreach (TextBox textBox in groupBox.Controls.OfType<TextBox>())
                    {
                        textBox.Enabled = true;
                    }
                }

                attackInterval_txt.Enabled = true;
                mobSelectorInterval_txt.Enabled = true;

                StartMacro_btn.Text = "START";
                source.Cancel();
            }
        }

        private void ResetMacroKeys_btn_Click(object sender, EventArgs e)
        {
            attackInterval_txt.Value = 1;
            mobSelectorInterval_txt.Value = 1;
            foreach (GroupBox groupBox in Controls.OfType<GroupBox>())
            {
                foreach (TextBox textBox in groupBox.Controls.OfType<TextBox>())
                {
                    textBox.Clear();
                }
            }
        }

        private async void AutoAttack(CancellationToken token, List<string> keys)
        {
            int interval = Convert.ToInt32(attackInterval_txt.Value) * 1000;
            for (; ;)
            {
                foreach(string key in keys)
                {
                    inputSimulator.Keyboard.KeyDown((VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), string.Format("VK_{0}", key)));
                    //SendKeys.SendWait(key);
                    await Task.Delay(interval);

                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                }
            }
        }

        private async void AutoPotion(CancellationToken token, List<string> contKeys)
        {
            for (; ; )
            {
                foreach (string key in contKeys)
                {
                    inputSimulator.Keyboard.KeyDown((VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), string.Format("VK_{0}", key)));
                    await Task.Delay(200);

                    if(token.IsCancellationRequested)
                    {
                        return;
                    }
                }
            }
        }

        private async void AutoMobSelector(CancellationToken token)
        {
            int interval = Convert.ToInt32(mobSelectorInterval_txt.Value) * 1000;
            for (; ; )
            {
                inputSimulator.Keyboard.KeyDown((VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), string.Format("VK_{0}", mobSelectorKey_txt.Text)));
                await Task.Delay(interval);

                if (token.IsCancellationRequested)
                {
                    return;
                }
            }
        }
    }
}
