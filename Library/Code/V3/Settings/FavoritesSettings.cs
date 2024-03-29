﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.MediaCenter.UI;
using Microsoft.MediaCenter;
using OMLEngine.Settings;
using System.Collections;

namespace Library.Code.V3
{
    public class FavoritesSettings : ModelItem
    {
        private string emptyContentMessage;
        public string EmptyContentMessage
        {
            get { return emptyContentMessage; }
            set
            {
                emptyContentMessage = value;
                FirePropertyChanged("EmptyContentMessage");
            }
        }

        private Boolean showEmptyContentMessage = false;
        public Boolean ShowEmptyContentMessage
        {
            get { return showEmptyContentMessage; }
            set
            {
                showEmptyContentMessage = value;
                FirePropertyChanged("ShowEmptyContentMessage");
            }
        }

        public FavoritesSettings()
        {
            this.commands = new ArrayListDataSet(this);

            ////save command
            //Command saveCmd = new Command();
            //saveCmd.Description = "Save";
            //saveCmd.Invoked += new EventHandler(saveCmd_Invoked);
            //this.commands.Add(saveCmd);

            //cancel command
            Command cancelCmd = new Command();
            cancelCmd.Description = "Done";
            cancelCmd.Invoked += new EventHandler(cancelCmd_Invoked);
            this.commands.Add(cancelCmd);

            this.SetupFavorites();
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
            return false;
        }

        /// <summary>
        /// saving settings
        /// </summary>
        public void Save()
        {
            
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

        private ArrayListDataSet favoritesArray = new ArrayListDataSet();
        public ArrayListDataSet FavoritesArray
        {
            get { return favoritesArray; }
            set
            {
                if (favoritesArray != value)
                {
                    favoritesArray = value;
                    FirePropertyChanged("FavoritesArray");
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

        private void SetupFavorites()
        {
            foreach (UserFilter filt in OMLSettings.UserFilters)
            {
                FavoritesCommand cmd = new FavoritesCommand(this, filt);
                cmd.Description = filt.Name;
                cmd.Invoked += delegate(object favoritesSender, EventArgs favoritesArgs)
                {
                    if (favoritesSender is FavoritesCommand)
                    {
                        string strDelete = string.Format("Are you sure you want to delete {0}?", ((FavoritesCommand)favoritesSender).Name);
                        DialogResult res = OMLApplication.Current.MediaCenterEnvironment.Dialog(strDelete, "DELETE FAVORITE", DialogButtons.Yes | DialogButtons.No, -1, true);
                        if (res == DialogResult.Yes)
                        {
                            //delete
                            IList<UserFilter> oldFilters = OMLSettings.UserFilters;
                            UserFilter[] newFilters = new UserFilter[oldFilters.Count -1];
                            //deal with the existing userfilters
                            this.favoritesArray.Remove(cmd);

                            for (int i = 0; i < oldFilters.Count-1; i++)
                            {
                                if (oldFilters[i] != ((FavoritesCommand)favoritesSender).Filter)
                                    newFilters[i] = ((FavoritesCommand)this.favoritesArray[i]).Filter;
                            }

                            OMLSettings.UserFilters = newFilters;
                            
                        }
                    }
                };
                this.favoritesArray.Add(cmd);
            }
        }

        public static bool DeleteFavorite()
        {
            return true;
        }
    }

    public class FavoritesCommand : Command
    {
        private Command edit;
        public Command Edit
        {
            get
            {
                return this.edit;
            }
            set
            {
                this.edit = value;
            }
        }
        public FavoritesCommand(IModelItem Owner, UserFilter Filter)
            : base(Owner)
        {
            this.filter = Filter;
            this.edit = new Command(this);
            this.edit.Invoked += delegate(object Sender, EventArgs Args)
            {
                //navigate to edit page
                Dictionary<string, object> properties = new Dictionary<string, object>();

                Library.Code.V3.FavoritesItemSettings page = new Library.Code.V3.FavoritesItemSettings(this.filter, false);
                properties["Page"] = page;
                properties["Application"] = OMLApplication.Current;

                OMLApplication.Current.Session.GoToPage("resx://Library/Library.Resources/V3_FavoritesItemSettings", properties);
            };
        }

        private UserFilter filter;
        public UserFilter Filter
        {
            get { return this.filter; }
            set
            {
                if (this.filter != value)
                {
                    this.filter = value;
                    FirePropertyChanged("Filter");
                }
            }
        }

        public string Name
        {
            get
            {
                return this.filter.Name;
            }
        }
    }
}
