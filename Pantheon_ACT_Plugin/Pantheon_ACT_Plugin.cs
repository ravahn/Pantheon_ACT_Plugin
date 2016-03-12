#region License
// ========================================================================
// Pantheon_ACT_Plugin.cs
// Advanced Combat Tracker Plugin for Pantheon ROTF
// https://github.com/ravahn/Pantheon_ACT_Plugin
// 
// The MIT License(MIT)
//
// Copyright(c) 2016 Ravahn
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ========================================================================
#endregion

using System;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.IO;

namespace Pantheon_ACT_Plugin
{
    #region ACT Plugin Code
    public class Pantheon_ACT_Plugin : UserControl, Advanced_Combat_Tracker.IActPluginV1
    {
        #region Designer Created Code (Avoid editing)
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            lstMessages = new System.Windows.Forms.ListBox();
            this.cmdClearMessages = new System.Windows.Forms.Button();
            this.cmdCopyProblematic = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 82;
            this.label1.Text = "Parser Messages";
            // 
            // lstMessages
            // 
            lstMessages.FormattingEnabled = true;
            lstMessages.Location = new System.Drawing.Point(14, 41);
            lstMessages.Name = "lstMessages";
            lstMessages.ScrollAlwaysVisible = true;
            lstMessages.Size = new System.Drawing.Size(700, 264);
            lstMessages.TabIndex = 81;
            // 
            // cmdClearMessages
            // 
            this.cmdClearMessages.Location = new System.Drawing.Point(88, 311);
            this.cmdClearMessages.Name = "cmdClearMessages";
            this.cmdClearMessages.Size = new System.Drawing.Size(106, 26);
            this.cmdClearMessages.TabIndex = 84;
            this.cmdClearMessages.Text = "Clear";
            this.cmdClearMessages.UseVisualStyleBackColor = true;
            this.cmdClearMessages.Click += new System.EventHandler(this.cmdClearMessages_Click);
            // 
            // cmdCopyProblematic
            // 
            this.cmdCopyProblematic.Location = new System.Drawing.Point(478, 311);
            this.cmdCopyProblematic.Name = "cmdCopyProblematic";
            this.cmdCopyProblematic.Size = new System.Drawing.Size(118, 26);
            this.cmdCopyProblematic.TabIndex = 85;
            this.cmdCopyProblematic.Text = "Copy to Clipboard";
            this.cmdCopyProblematic.UseVisualStyleBackColor = true;
            this.cmdCopyProblematic.Click += new System.EventHandler(this.cmdCopyProblematic_Click);
            // 
            // UserControl1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cmdCopyProblematic);
            this.Controls.Add(this.cmdClearMessages);
            this.Controls.Add(this.label1);
            this.Controls.Add(lstMessages);
            this.Name = "UserControl1";
            this.Size = new System.Drawing.Size(728, 356);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label label1;
        private static System.Windows.Forms.ListBox lstMessages;
        private System.Windows.Forms.Button cmdClearMessages;
        private System.Windows.Forms.Button cmdCopyProblematic;

        #endregion

        public Pantheon_ACT_Plugin()
        {
            InitializeComponent();
        }
        // reference to the ACT plugin status label
        private Label lblStatus = null;

        public void InitPlugin(System.Windows.Forms.TabPage pluginScreenSpace, System.Windows.Forms.Label pluginStatusText)
        {
            // store a reference to plugin's status label
            lblStatus = pluginStatusText;

            try
            {
                // Configure ACT for updates, and check for update.
                Advanced_Combat_Tracker.ActGlobals.oFormActMain.UpdateCheckClicked += new Advanced_Combat_Tracker.FormActMain.NullDelegate(UpdateCheckClicked);
                if (Advanced_Combat_Tracker.ActGlobals.oFormActMain.GetAutomaticUpdatesAllowed())
                {
                    Thread updateThread = new Thread(new ThreadStart(UpdateCheckClicked));
                    updateThread.IsBackground = true;
                    updateThread.Start();
                }

                // Update the listing of columns inside ACT.
                UpdateACTTables();

                pluginScreenSpace.Controls.Add(this);   // Add this UserControl to the tab ACT provides
                this.Dock = DockStyle.Fill; // Expand the UserControl to fill the tab's client space

                // character name cannot be parsed from logfile name
                Advanced_Combat_Tracker.ActGlobals.oFormActMain.LogPathHasCharName = false;
                Advanced_Combat_Tracker.ActGlobals.oFormActMain.LogFileFilter = "*.log";

                // Default Timestamp length, but this can be overridden in parser code.
                Advanced_Combat_Tracker.ActGlobals.oFormActMain.TimeStampLen = DateTime.Now.ToString("HH:mm:ss.fff").Length + 1;

                // Set Date time format parsing. 
                Advanced_Combat_Tracker.ActGlobals.oFormActMain.GetDateTimeFromLog = new Advanced_Combat_Tracker.FormActMain.DateTimeLogParser(LogParse.ParseLogDateTime);

                // Set primary parser delegate for processing data
                Advanced_Combat_Tracker.ActGlobals.oFormActMain.BeforeLogLineRead += LogParse.BeforeLogLineRead;

                // TODO: set up Zone Name

                lblStatus.Text = "Pantheon Plugin Started.";
            }
            catch (Exception ex)
            {
                LogParserMessage("Exception during InitPlugin: " + ex.ToString().Replace(Environment.NewLine, " "));
                lblStatus.Text = "InitPlugin Error.";
            }
        }

        public void DeInitPlugin()
        {
            // remove event handler
            Advanced_Combat_Tracker.ActGlobals.oFormActMain.UpdateCheckClicked -= this.UpdateCheckClicked;
            Advanced_Combat_Tracker.ActGlobals.oFormActMain.BeforeLogLineRead -= LogParse.BeforeLogLineRead;

            if (lblStatus != null)
            {
                lblStatus.Text = "Pantheon Plugin Unloaded.";
                lblStatus = null;
            }
        }


        public void UpdateCheckClicked()
        {
            /*
            try
            {
                DateTime localDate = Advanced_Combat_Tracker.ActGlobals.oFormActMain.PluginGetSelfDateUtc(this);
                DateTime remoteDate = Advanced_Combat_Tracker.ActGlobals.oFormActMain.PluginGetRemoteDateUtc(m_PluginId);
                if (localDate.AddHours(2) < remoteDate)
                {
                    DialogResult result = MessageBox.Show("There is an updated version of the Pantheon Parsing Plugin.  Update it now?\n\n(If there is an update to ACT, you should click No and update ACT first.)", "New Version", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        Advanced_Combat_Tracker.ActPluginData pluginData = Advanced_Combat_Tracker.ActGlobals.oFormActMain.PluginGetSelfData(this);
                        System.IO.FileInfo updatedFile = Advanced_Combat_Tracker.ActGlobals.oFormActMain.PluginDownload(m_PluginId);
                        pluginData.pluginFile.Delete();
                        updatedFile.MoveTo(pluginData.pluginFile.FullName);
                        Advanced_Combat_Tracker.ThreadInvokes.CheckboxSetChecked(Advanced_Combat_Tracker.ActGlobals.oFormActMain, pluginData.cbEnabled, false);
                        Application.DoEvents();
                        Advanced_Combat_Tracker.ThreadInvokes.CheckboxSetChecked(Advanced_Combat_Tracker.ActGlobals.oFormActMain, pluginData.cbEnabled, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Advanced_Combat_Tracker.ActGlobals.oFormActMain.WriteExceptionLog(ex, "Pantheon Plugin Update Check.");
            }*/
        }

        private void UpdateACTTables()
        {

        }


        public static void LogParserMessage(string message)
        {
            lstMessages.Invoke(new Action(() => lstMessages.Items.Add(message)));
        }

        private void cmdClearMessages_Click(object sender, EventArgs e)
        {
            lstMessages.Items.Clear();
        }

        private void cmdCopyProblematic_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (object itm in lstMessages.Items)
                sb.AppendLine((itm ?? "").ToString());

            if (sb.Length > 0)
                System.Windows.Forms.Clipboard.SetText(sb.ToString());
        }
    }
    #endregion

    #region Parser Code

    public static class LogParse
    {
        //Joppa hits A Halnir Wolf for 14 damage! [hate 1:2]
        //Aughen hits An Orc Bruiser for 41 points of non-melee damage.
        //Kilsin stabs An Orc Bruiser for 5 damage.
        //An Orc Gladiator bashes Aradune for 3 damage.
        public static Regex regex_damage = new Regex(@"(?<actor>.+?) (?<hittype>hit|hits|stab|stabs|bash|bashes|kick|kicks|strike|strikes|backstab|backstabs) (?<target>.+) for (?<damage>\d+)( points of (?<damagetype>non-melee))? damage(\.|!)(?<hate> \[hate \d:\d\])?", RegexOptions.Compiled);

        //Joppa heals Aradune for 15 points.
        public static Regex regex_heal = new Regex(@"(?<actor>.+?) (heal|heals) (?<target>.+?) for (?<heal>\d*) points(\.|!)");

        // A Halnir Wolf's corpse twitches for a moment then slumps to the ground.
        // you died!
        //public static Regex regex_death = new Regex(@"(?<actor>.+?)'s corpse twitches for a moment then slumps to the ground\.");

        // NOT HANDLED yet:
        //A Halnir Wolf interrupts Mont's Ember Shock
        //A Halnir Wolf misses you!
        //Aradune dodges!
        //An Orc brigand scored a critical hit!

        public static DateTime ParseLogDateTime(string message)
        {
            DateTime ret = DateTime.MinValue;

            try
            {
                if (message == null || message.IndexOf(' ') < 5)
                    return ret;

                if (!DateTime.TryParse(message.Substring(0, message.IndexOf(' ')), out ret))
                    return DateTime.MinValue;
            }
            catch (Exception ex)
            {
                Pantheon_ACT_Plugin.LogParserMessage("Error [ParseLogDateTime] " + ex.ToString().Replace(Environment.NewLine, " "));
            }
            return ret;
        }

        public static void BeforeLogLineRead(bool isImport, Advanced_Combat_Tracker.LogLineEventArgs logInfo)
        {
            string logLine = logInfo.logLine;

            try
            {
                // TODO: DateTime in loglines needed.
                DateTime timestamp = DateTime.Now;
                //DateTime timestamp = ParseLogDateTime(logLine);
                //if (logLine.IndexOf(' ') >= 5)
                //logLine = logLine.Substring(logLine.IndexOf(' '));

                // TODO: reformat logline
                //logInfo.logLine = "[" + timestamp.ToString("HH:mm:ss.fff") + "] " + logLine;
                // timestamp = DateTime.ParseExact(logLine.Substring(1, logLine.IndexOf(']') - 1), "HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);

                Match m;

                m = regex_damage.Match(logLine);
                if (m.Success)
                {
                    string actor = m.Groups["actor"].Success ? DecodeString(m.Groups["actor"].Value) : "";
                    string target = m.Groups["target"].Success ? DecodeString(m.Groups["target"].Value) : "";
                    bool nonMelee = m.Groups["damagetype"].Value == "non-melee";

                    if (Advanced_Combat_Tracker.ActGlobals.oFormActMain.SetEncounter(timestamp, actor, target))
                    {
                        Advanced_Combat_Tracker.ActGlobals.oFormActMain.AddCombatAction(
                            nonMelee ? (int)Advanced_Combat_Tracker.SwingTypeEnum.NonMelee : (int)Advanced_Combat_Tracker.SwingTypeEnum.Melee,
                            false, // todo: critical hits
                            DecodeString(m.Groups["hittype"].Value),
                            actor,
                            nonMelee ? "Non-Melee" : "Melee",
                            new Advanced_Combat_Tracker.Dnum(int.Parse(m.Groups["damage"].Value, System.Globalization.NumberStyles.AllowThousands)),
                            timestamp,
                            Advanced_Combat_Tracker.ActGlobals.oFormActMain.GlobalTimeSorter,
                            target,
                            "");
                    }

                    return;
                }

                m = regex_heal.Match(logLine);
                if (m.Success)
                {
                    string actor = m.Groups["actor"].Success ? DecodeString(m.Groups["actor"].Value) : "";
                    string target = m.Groups["target"].Success ? DecodeString(m.Groups["target"].Value) : "";

                    if (Advanced_Combat_Tracker.ActGlobals.oFormActMain.SetEncounter(timestamp, actor, target))
                    {
                        Advanced_Combat_Tracker.ActGlobals.oFormActMain.AddCombatAction(
                            (int)Advanced_Combat_Tracker.SwingTypeEnum.Healing,
                            false, // todo: heal crit
                            "",
                            actor,
                            "",
                            new Advanced_Combat_Tracker.Dnum(int.Parse(m.Groups["heal"].Value, System.Globalization.NumberStyles.AllowThousands)),
                            timestamp,
                            Advanced_Combat_Tracker.ActGlobals.oFormActMain.GlobalTimeSorter,
                            target,
                            "");

                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                string exception = ex.ToString().Replace(Environment.NewLine, " ");
                if (ex.InnerException != null)
                    exception += " " + ex.InnerException.ToString().Replace(Environment.NewLine, " ");

                Pantheon_ACT_Plugin.LogParserMessage("Error [LogParse.BeforeLogLineRead] " + exception + " " + logInfo.logLine);
            }

            // For debugging
            if (!string.IsNullOrWhiteSpace(logLine))
                Pantheon_ACT_Plugin.LogParserMessage("Unhandled Line: " + logInfo.logLine);
        }

        private static string DecodeString(string data)
        {
            string ret = data.Replace("&apos;", "'")
                .Replace("&amp;", "&");

            return ret;
        }
    }

    #endregion


}