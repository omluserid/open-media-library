﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.MediaCenter.UI;
using Microsoft.MediaCenter;
using OMLEngine.Settings;
using System.Collections;
using System.Threading;

namespace Library.Code.V3
{
    public class ExtenderSettings : ModelItem
    {
        private Command goToImpersonation;
        public Command GoToImpersonation
        {
            get { return this.goToImpersonation; }
        }
        private Command goToTranscode;
        public Command GoToTranscode
        {
            get { return this.goToTranscode; }
        }

        public ExtenderSettings()
        {
            this.commands = new ArrayListDataSet(this);

            //save command
            Command saveCmd = new Command();
            saveCmd.Description = "Save";
            saveCmd.Invoked += new EventHandler(saveCmd_Invoked);
            this.commands.Add(saveCmd);

            //cancel command
            Command cancelCmd = new Command();
            cancelCmd.Description = "Cancel";
            cancelCmd.Invoked += new EventHandler(cancelCmd_Invoked);
            this.commands.Add(cancelCmd);

            this.goToImpersonation = new Command();
            this.goToImpersonation.Description = "Impersonation";
            this.goToImpersonation.Invoked += new EventHandler(goToImpersonation_Invoked);

            this.goToTranscode = new Command();
            this.goToTranscode.Description = "Transcoding";
            this.goToTranscode.Invoked += new EventHandler(goToTranscode_Invoked);

            this.transcodingDelays = new Choice(this);

            this.SetupTranscodingOptions();
            this.SetupImpersonation();
            this.SetupTranscodingDelays();
            
        }

        void goToTranscode_Invoked(object sender, EventArgs e)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            Library.Code.V3.ExtenderSettings page = this;
            properties["Page"] = page;
            properties["Application"] = OMLApplication.Current;

            OMLApplication.Current.Session.GoToPage("resx://Library/Library.Resources/V3_ExtenderSettingsTranscode", properties);
        }

        void goToImpersonation_Invoked(object sender, EventArgs e)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            Library.Code.V3.ExtenderSettings page = this;
            properties["Page"] = page;
            properties["Application"] = OMLApplication.Current;

            OMLApplication.Current.Session.GoToPage("resx://Library/Library.Resources/V3_ExtenderSettingsImpersonation", properties);
        }
        private void SetupTranscodingOptions()
        {
            this.transcodeAVI = new BooleanChoice(this);
            this.transcodeAVI.Description = "Transcode AVI files";
            this.transcodeMKV = new BooleanChoice(this);
            this.transcodeMKV.Description = "Transcode MKV files";
            this.transcodeOGM = new BooleanChoice(this);
            this.transcodeOGM.Description = "Transcode OGM files";
            this.preserveAudioFormat = new BooleanChoice(this);
            this.preserveAudioFormat.Description = "Preserve audio format";

            this.transcodeAVI.Chosen = OMLSettings.TranscodeAVIFiles;
            this.transcodeMKV.Chosen = OMLSettings.TranscodeMKVFiles;
            this.transcodeOGM.Chosen = OMLSettings.TranscodeOGMFiles;
            this.preserveAudioFormat.Chosen = OMLSettings.PreserveAudioOnTranscode;
        }

        private BooleanChoice transcodeAVI;
        public BooleanChoice TranscodeAVI
        {
            get
            {
                return this.transcodeAVI;
            }
            set
            {
                this.transcodeAVI = value;
            }
        }
        private BooleanChoice transcodeMKV;
        public BooleanChoice TranscodeMKV
        {
            get
            {
                return this.transcodeMKV;
            }
            set
            {
                this.transcodeMKV = value;
            }
        }
        private BooleanChoice transcodeOGM;
        public BooleanChoice TranscodeOGM
        {
            get
            {
                return this.transcodeOGM;
            }
            set
            {
                this.transcodeOGM = value;
            }
        }
        private BooleanChoice preserveAudioFormat;
        public BooleanChoice PreserveAudioFormat
        {
            get
            {
                return this.preserveAudioFormat;
            }
            set
            {
                this.preserveAudioFormat = value;
            }
        }
        private void SetupImpersonation()
        {
            this.impersonationUserName = new EditableText(this);
            this.impersonationPassword = new EditableText(this);
            this.impersonationUserName.Value = OMLSettings.ImpersonationUsername;
            this.impersonationPassword.Value = OMLSettings.ImpersonationPassword;

        }
        private EditableText impersonationUserName;
        public EditableText ImpersonationUserName
        {
            get { return this.impersonationUserName; }
            set { this.impersonationUserName = value; }
        }

        private EditableText impersonationPassword;
        public EditableText ImpersonationPassword
        {
            get { return this.impersonationPassword; }
            set { this.impersonationPassword = value; }
        }

        private void SetupTranscodingDelays()
        {
            List<string> delays = new List<string>();

            for (int i = 0; i < 51; i++)
            {
                delays.Add(string.Format("{0} seconds", i.ToString()));
            }
            
            transcodingDelays.Options = delays;
            transcodingDelays.Chosen = string.Format("{0} seconds", OMLSettings.TranscodeBufferDelay.ToString());
            this.selectedTranscodingDelay = string.Format("{0} seconds", OMLSettings.TranscodeBufferDelay.ToString());
        }

        private string selectedTranscodingDelay;
        private Choice transcodingDelays;
        public Choice TranscodingDelays
        {
            get { return transcodingDelays; }
            set { transcodingDelays = value; }
        }

        private Boolean isBusy = false;
        public Boolean IsBusy
        {
            get { return isBusy; }
            set
            {
                isBusy = value;
                FirePropertyChanged("IsBusy");
            }
        }

        /// <summary>
        /// detecting changes
        /// </summary>
        /// <returns></returns>
        public bool IsDirty()
        {
            if (this.impersonationUserName.Value != OMLSettings.ImpersonationUsername)
                return true;
            if (this.impersonationPassword.Value != OMLSettings.ImpersonationPassword)
                return true;

            if (((string)transcodingDelays.Chosen).Replace(" seconds", "") != OMLSettings.TranscodeBufferDelay.ToString())
                return true;

            if ((bool)this.transcodeAVI.Chosen != OMLSettings.TranscodeAVIFiles)
                return true;
            if ((bool)this.transcodeMKV.Chosen != OMLSettings.TranscodeMKVFiles)
                return true;
            if ((bool)this.transcodeOGM.Chosen != OMLSettings.TranscodeOGMFiles)
                return true;
            if ((bool)this.preserveAudioFormat.Chosen != OMLSettings.PreserveAudioOnTranscode)
                return true;

            return false;
        }

        /// <summary>
        /// saving settings
        /// </summary>
        public void Save()
        {
            OMLSettings.ImpersonationUsername = this.impersonationUserName.Value;
            OMLSettings.ImpersonationPassword = this.impersonationPassword.Value;
            OMLSettings.TranscodeBufferDelay = Convert.ToInt32(((string)transcodingDelays.Chosen).Replace(" seconds", ""));
            OMLSettings.TranscodeAVIFiles = (bool)this.transcodeAVI.Chosen;
            OMLSettings.TranscodeMKVFiles = (bool)this.transcodeMKV.Chosen;
            OMLSettings.TranscodeOGMFiles = (bool)this.transcodeOGM.Chosen;
            OMLSettings.PreserveAudioOnTranscode = (bool)this.preserveAudioFormat.Chosen;
        }

        /// <summary>
        /// Detects if the settings are dirty 
        /// and prompts the user to save if they have not already
        /// </summary>
        public void ConfirmSave()
        {
            if (this.IsDirty())
            {
                DialogResult res = OMLApplication.Current.MediaCenterEnvironment.Dialog("Do you want to save the changes that you have made to these settings?", "SAVE CHANGES", DialogButtons.Yes | DialogButtons.No, -1, true);
                if (res == DialogResult.Yes)
                {
                    //save!
                    this.Save();
                }
            }
            OMLApplication.Current.Session.BackPage();

        }

        /// <summary>
        /// A list of actions that can be performed on this object.
        /// This list should only contain objects of type Command.
        /// </summary>
        private IList commands;
        public IList Commands
        {
            get { return commands; }
            set
            {
                if (commands != value)
                {
                    commands = value;
                    FirePropertyChanged("Commands");
                }
            }
        }

        void cancelCmd_Invoked(object sender, EventArgs e)
        {
            OMLApplication.Current.Session.BackPage();
        }

        void saveCmd_Invoked(object sender, EventArgs e)
        {
            this.Save();
            OMLApplication.Current.Session.BackPage();
        }
    }
}
