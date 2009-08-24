﻿using System;
using System.Collections;
using Microsoft.MediaCenter.UI;
using System.Text;
using System.Threading;
using System.Data;
using System.Collections.Generic;


namespace Library.Code.V3
{
    /// <summary>
    /// This object contains the standard set of information displayed in the 
    /// details page UI.
    /// </summary>
    public class GeneralSettings : BaseModelItem
    {
        public GeneralSettings()
            : base()
        {
            this.title = "GENERAL SETTINGS";
            this.Commands = new ArrayListDataSet();

            //gallery
            Command galleryCmd = new Command();
            galleryCmd.Description = "Gallery";
            galleryCmd.Invoked += new EventHandler(galleryCmd_Invoked);
            this.Commands.Add(galleryCmd);

            //theme
            Command mediaChangersCmd = new Command();
            mediaChangersCmd.Description = "Theme";
            this.Commands.Add(mediaChangersCmd);

            //background
            Command backgroundCmd = new Command();
            backgroundCmd.Description = "Background";
            backgroundCmd.Invoked+=new EventHandler(backgroundCmd_Invoked);
            this.Commands.Add(backgroundCmd);

            //rerun setup
            Command firstrunCmd = new Command();
            firstrunCmd.Description = "Open Media Library Setup";
            firstrunCmd.Invoked += new EventHandler(firstrunCmd_Invoked);
            this.Commands.Add(firstrunCmd);

            //alpha
            Command alphaCmd = new Command();
            alphaCmd.Description = "Additional Settings";
            alphaCmd.Invoked += new EventHandler(alphaCmd_Invoked);
            this.Commands.Add(alphaCmd);

            //start menu
            Command startMenuCmd = new Command();
            startMenuCmd.Description = "Start Menu";
            startMenuCmd.Invoked += new EventHandler(startMenuCmd_Invoked);
            this.Commands.Add(startMenuCmd);

            //automatic updates
            Command automaticUpdateCmd = new Command();
            automaticUpdateCmd.Description = "Automatic Updates";
            automaticUpdateCmd.Invoked += new EventHandler(automaticUpdateCmd_Invoked);
            this.Commands.Add(automaticUpdateCmd);
        }

        void automaticUpdateCmd_Invoked(object sender, EventArgs e)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            Library.Code.V3.UpdaterSettings page = new Library.Code.V3.UpdaterSettings();
            properties["Page"] = page;
            properties["Application"] = OMLApplication.Current;

            OMLApplication.Current.Session.GoToPage("resx://Library/Library.Resources/V3_UpdaterSettings", properties);
        }

        void firstrunCmd_Invoked(object sender, EventArgs e)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            Library.Code.V3.FirstRun page = new Library.Code.V3.FirstRun();
            properties["Page"] = page;

            OMLApplication.Current.Session.GoToPage("resx://Library/Library.Resources/V3_FirstRunBackground", properties);
        }

        void startMenuCmd_Invoked(object sender, EventArgs e)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            Library.Code.V3.StartMenuSettings page = new Library.Code.V3.StartMenuSettings();
            properties["Page"] = page;
            properties["Application"] = OMLApplication.Current;

            OMLApplication.Current.Session.GoToPage("resx://Library/Library.Resources/V3_StartMenuSettings", properties);
        }

        void alphaCmd_Invoked(object sender, EventArgs e)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            Library.Code.V3.GallerySettings page = new Library.Code.V3.GallerySettings();
            properties["Page"] = page;
            properties["Application"] = OMLApplication.Current;

            OMLApplication.Current.Session.GoToPage("resx://Library/Library.Resources/V3_AdditionalSettings", properties);
        }

        void galleryCmd_Invoked(object sender, EventArgs e)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            Library.Code.V3.GallerySettings page = new Library.Code.V3.GallerySettings();
            properties["Page"] = page;
            properties["Application"] = OMLApplication.Current;

            OMLApplication.Current.Session.GoToPage("resx://Library/Library.Resources/V3_GallerySettings", properties);
        }

        void backgroundCmd_Invoked(object sender, EventArgs e)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            Library.Code.V3.BackgroundSettings page = new Library.Code.V3.BackgroundSettings();
            properties["Page"] = page;
            properties["Application"] = OMLApplication.Current;

            OMLApplication.Current.Session.GoToPage("resx://Library/Library.Resources/V3_BackgroundSettings", properties);
        }

        /// <summary>
        /// The primary title of the object.
        /// </summary>
        public string Title
        {
            get { return title; }
            set
            {
                if (title != value)
                {
                    title = value;
                    FirePropertyChanged("Title");
                }
            }
        }

        /// <summary>
        /// A list of actions that can be performed on this object.
        /// This list should only contain objects of type Command.
        /// </summary>
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

        private string title;
        private IList commands;

    }
}
