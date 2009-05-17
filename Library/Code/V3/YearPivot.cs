﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Microsoft.MediaCenter.UI;
using System.Collections.Specialized;

namespace Library.Code.V3
{
    public class YearPivot : BrowsePivot
    {

        public YearPivot(IModelItemOwner owner, string stDescription, string stNoContentText, IList listContent)
            : base(owner, stDescription, stNoContentText, listContent)
        {
            this.SupportsItemContext = true;
            this.SetupContextMenu();
        }

        public override void UpdateContext(string newTemplate)
        {
            //change the buttons based on which view was invoked
            ICommand ctx0 = (ICommand)this.ContextMenu.SharedItems[0];
            ICommand ctx1 = (ICommand)this.ContextMenu.SharedItems[1];

            switch (newTemplate)
            {
                case "oneRowGalleryItemPoster":
                    this.ContentItemTemplate = "GalleryGroup";
                    this.SubContentItemTemplate = "oneRowPoster";
                    this.DetailTemplate = Library.Code.V3.BrowsePivot.ExtendedDetailTemplate;
                    ctx0.Description = "View Small";
                    ctx1.Description = "View List";
                    this.SupportsItemContext = true;
                    break;
                case "twoRowGalleryItemPoster":
                    this.ContentItemTemplate = "GalleryGroup";
                    this.SubContentItemTemplate = "twoRowPoster";
                    this.DetailTemplate = Library.Code.V3.BrowsePivot.StandardDetailTemplate;
                    ctx0.Description = "View Large";
                    ctx1.Description = "View List";
                    this.SupportsItemContext = true;
                    break;
                case "ListViewItem":
                    this.ContentItemTemplate = "ListViewItem";
                    this.DetailTemplate = Library.Code.V3.BrowsePivot.StandardDetailTemplate;
                    ctx0.Description = "View Large";
                    ctx1.Description = "View Small";
                    this.SupportsItemContext = false;
                    break;
            }
        }

        void viewCmd_Invoked(object sender, EventArgs e)
        {
            OMLApplication.Current.CatchMoreInfo();

            ICommand invokedCmd = (ICommand)sender;
            string template = "oneRowGalleryItemPoster";
            switch (invokedCmd.Description)
            {
                case "View Large":
                    template = "oneRowGalleryItemPoster";
                    break;
                case "View Small":
                    template = "twoRowGalleryItemPoster";
                    break;
                case "View List":
                    template = "ListViewItem";
                    break;
            }
            this.UpdateContext(template);
        }

        private void SetupContextMenu()
        {
            #region ctx menu
            //create the context menu
            Library.Code.V3.ContextMenuData ctx = new Library.Code.V3.ContextMenuData();
            //some ctx items
            Library.Code.V3.ThumbnailCommand viewFirstCmd = new Library.Code.V3.ThumbnailCommand(this);
            //if (this.SubContentItemTemplate == "twoRowPoster")
                viewFirstCmd.Description = "View Large";//hard for now-we default to two row
            //else
            //    viewFirstCmd.Description = "View Small";
            viewFirstCmd.Invoked += new EventHandler(viewCmd_Invoked);

            Library.Code.V3.ThumbnailCommand viewSecondCmd = new Library.Code.V3.ThumbnailCommand(this);
            viewSecondCmd.Invoked += new EventHandler(viewCmd_Invoked);
            viewSecondCmd.Description = "View List";

            Library.Code.V3.ThumbnailCommand viewSettingsCmd = new Library.Code.V3.ThumbnailCommand(this);
            viewSettingsCmd.Invoked += new EventHandler(viewSettingsCmd_Invoked);
            viewSettingsCmd.Description = "Settings";

            Library.Code.V3.ThumbnailCommand viewSearchCmd = new Library.Code.V3.ThumbnailCommand(this);
            viewSearchCmd.Invoked += new EventHandler(this.viewSearchCmd_Invoked);
            viewSearchCmd.Description = "Search";

            ctx.SharedItems.Add(viewFirstCmd);
            ctx.SharedItems.Add(viewSecondCmd);
            ctx.SharedItems.Add(viewSettingsCmd);
            ctx.SharedItems.Add(viewSearchCmd);

            Library.Code.V3.ThumbnailCommand viewMovieDetailsCmd = new Library.Code.V3.ThumbnailCommand(this);
            viewMovieDetailsCmd.Description = "Movie Details";
            viewMovieDetailsCmd.Invoked += new EventHandler(viewMovieDetailsCmd_Invoked);

            Library.Code.V3.ThumbnailCommand viewPlayCmd = new Library.Code.V3.ThumbnailCommand(this);
            viewPlayCmd.Description = "Play";
            viewPlayCmd.Invoked += new EventHandler(viewPlayCmd_Invoked);

            //Library.Code.V3.ThumbnailCommand viewDeleteCmd = new Library.Code.V3.ThumbnailCommand(this);
            //viewDeleteCmd.Description = "Delete";

            ctx.UniqueItems.Add(viewMovieDetailsCmd);
            ctx.UniqueItems.Add(viewPlayCmd);
            //ctx.UniqueItems.Add(viewDeleteCmd);

            //Command CommandContextPopOverlay = new Command();
            //properties.Add("CommandContextPopOverlay", CommandContextPopOverlay);

            //properties.Add("MenuData", ctx);
            this.ContextMenu = ctx;
            #endregion ctx menu
        }

        void viewSearchCmd_Invoked(object sender, EventArgs e)
        {
            OMLApplication.Current.CatchMoreInfo();
            if (this.Owner is GalleryPage)
                ((GalleryPage)this.Owner).searchCmd_Invoked(sender, e);
        }

        void viewSettingsCmd_Invoked(object sender, EventArgs e)
        {
            OMLApplication.Current.CatchMoreInfo();
            if (this.Owner is GalleryPage)
                ((GalleryPage)this.Owner).settingsCmd_Invoked(sender, e);
        }

        void viewPlayCmd_Invoked(object sender, EventArgs e)
        {
            OMLApplication.Current.CatchMoreInfo();
            if (this.Owner is GalleryPage && ((GalleryPage)this.Owner).SelectedItemCommand is MovieItem)
            {
                GalleryPage page = this.Owner as GalleryPage;
                MovieItem movie = page.SelectedItemCommand as MovieItem;
                movie.PlayAllDisks();
            }
        }

        void viewMovieDetailsCmd_Invoked(object sender, EventArgs e)
        {
            OMLApplication.Current.CatchMoreInfo();
            if (this.Owner is GalleryPage)
                ((GalleryPage)this.Owner).SelectedItemCommand.Invoke();
        }
    }
}