//#define DEBUG_EXT
//#define LAYOUT_V2
#define LAYOUT_V3
//#define CAROUSEL

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.MediaCenter;
using Microsoft.MediaCenter.Hosting;
using Microsoft.MediaCenter.UI;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Timers;

using OMLEngine;
using OMLEngine.Settings;
using System;

namespace Library
{    
    /// <summary>
    /// Starting point for the OML
    /// </summary>
    public class OMLApplication : ModelItem
    {
        private bool isBusy = false;
        private bool isStartingTranscodingJob = false;
        private string transcodeStatus = string.Empty;
        private int currentFocusedItemIndex = 0;
        private int currentItemIndexPosition = 0;
        private int currentAngleDegrees;
        private Image primaryBackgroundImage;
        private int iCurrentBackgroundImage = 0;
        private int iTotalBackgroundImages = 0;
        public System.Timers.Timer mainBackgroundTimer;
        private Library.Code.V3.MoreInfoHooker2 hooker;

        public void FixRepeatRate(object scroller, uint val)
        {
            PropertyInfo pi = scroller.GetType().GetProperty("View", BindingFlags.Public | BindingFlags.Instance);
            object view = pi.GetValue(scroller, null);
            pi = view.GetType().GetProperty("Control", BindingFlags.Public | BindingFlags.Instance);
            object control = pi.GetValue(view, null);

            pi = control.GetType().GetProperty("KeyRepeatThreshold", BindingFlags.NonPublic | BindingFlags.Instance);
            pi.SetValue(control, val, null);

        }

        private void SetPrimaryBackgroundImage()
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.MainPageBackDropFile))
            {
                if (primaryBackgroundImage == null)
                {
                    string lockedFileName = Properties.Settings.Default.MainPageBackDropFile;
                    if (File.Exists(Path.Combine(FileSystemWalker.MainBackDropDirectory, lockedFileName)))
                    {
                        primaryBackgroundImage = new Image(
                            string.Format("file://{0}", Path.Combine(FileSystemWalker.MainBackDropDirectory, lockedFileName))
                        );
                        return;
                    }
                }
            }

            // a specific file is NOT set for the main backdrop, lets see how many we find.
            DirectoryInfo dirInfo = new DirectoryInfo(FileSystemWalker.MainBackDropDirectory);
            FileInfo[] files = dirInfo.GetFiles("*.jpg", SearchOption.TopDirectoryOnly);
            iTotalBackgroundImages = files.Length;
            if (iTotalBackgroundImages > 0)
            {
                if (iCurrentBackgroundImage >= iTotalBackgroundImages)
                    iCurrentBackgroundImage = 0;

                PrimaryBackgroundImage = new Image(string.Format("file://{0}",
                        Path.Combine(FileSystemWalker.MainBackDropDirectory,
                        files[iCurrentBackgroundImage].FullName)));
                iCurrentBackgroundImage++;
                if (mainBackgroundTimer == null)
                {
                    mainBackgroundTimer = new System.Timers.Timer();
                    mainBackgroundTimer.AutoReset = true;
                    mainBackgroundTimer.Elapsed += new ElapsedEventHandler(mainBackgroundTimer_Elapsed);
                    int rotationInSeconds = Properties.Settings.Default.MainPageBackDropRotationInSeconds;
                    double rotationInMilliseconds = rotationInSeconds * 1000;
                    mainBackgroundTimer.Interval = rotationInMilliseconds;
                    mainBackgroundTimer.Enabled = true;
                    mainBackgroundTimer.Start();
                    GC.KeepAlive(mainBackgroundTimer);
                }

                return;
            }
        }

        void mainBackgroundTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SetPrimaryBackgroundImage();
        }

        public Image PrimaryBackgroundImage
        {
            get { return primaryBackgroundImage; }
            set
            {
                primaryBackgroundImage = value;
                FirePropertyChanged("PrimaryBackgroundImage");
            }
        }

        public string RevisionNumber
        {
            get
            {
                try
                {
                    Assembly _assembly = Assembly.GetExecutingAssembly();
                    Stream _txtStream = _assembly.GetManifestResourceStream("Library.Revision.txt");
                    StreamReader _txtStreamReader = new StreamReader(_txtStream);
                    string revisionNumber = _txtStreamReader.ReadToEnd();
                    _txtStreamReader.Close();
                    _txtStream.Close();
                    return revisionNumber;
                }
                catch (Exception)
                {
                    return @"Unknown";
                }
            }
        }

        public int DistanceToMoveCloserBasedOnAngleOrRotation
        {
            get
            {
                int originalHeight = Properties.Settings.Default.CarouselItemWidth;
                double newWidth = originalHeight * Math.Cos(Convert.ToDouble(currentAngleDegrees));
                int distanceToMove = originalHeight - Convert.ToInt32(newWidth);

                return distanceToMove;
            }
        }

        public Inset MoveToInset
        {
            get { return new Inset(0, 0, DistanceToMoveCloserBasedOnAngleOrRotation, 0); }
        }

        public int CurrentAngleDegrees
        {
            get { return currentAngleDegrees; }
            set { currentAngleDegrees = value; }
        }

        public int CurrentFocusedItemIndex
        {
            get { return currentFocusedItemIndex; }
            set { currentFocusedItemIndex = value; }
        }

        public int CurrentItemIndexPosition
        {
            get { return currentItemIndexPosition; }
            set { currentItemIndexPosition = value; }
        }

        public string TranscodeStatus
        {
            get { return transcodeStatus; }
            set
            {
                transcodeStatus = value;
                FirePropertyChanged("TranscodeStatus");
            }
        }

        public Boolean IsBusy
        {
            get { return isBusy; }
            set
            {
                isBusy = value;
                FirePropertyChanged("IsBusy");
            }
        }

        public Boolean IsStartingTranscodingJob
        {
            get { return isStartingTranscodingJob; }
            set
            {
                isStartingTranscodingJob = value;
                //FirePropertyChanged("IsStartingTranscodingJob");
            }
        }       

        public bool ShowDetailsPopup
        {
            get { return true; }
        }
        

        public OMLApplication()
            : this(null, null)
        {
            DebugLine("[OMLApplication] Empty Constructor called");
        }

        ~OMLApplication()
        {
            if (mainBackgroundTimer != null)
            {
                mainBackgroundTimer.Stop();
                mainBackgroundTimer.Enabled = false;
                mainBackgroundTimer.Dispose();
            }
        }

        public OMLApplication(HistoryOrientedPageSession session, AddInHost host)
        {           
            #if LAYOUT_V3
            this.hooker = new Library.Code.V3.MoreInfoHooker2();
            #endif

            this._session = session;
            AddInHost.Current.MediaCenterEnvironment.PropertyChanged +=new PropertyChangedEventHandler(MediaCenterEnvironment_PropertyChanged);

            try { // borrowed from vmcNetFlix project on google-code
                bool isConsole = false;
                if (host.MediaCenterEnvironment.Capabilities.ContainsKey("Console"))
                {
                    isConsole = (bool)host.MediaCenterEnvironment.Capabilities["Console"];
                }
                bool isVideo = false;
                if (host.MediaCenterEnvironment.Capabilities.ContainsKey("Video"))
                {
                    isVideo = (bool)host.MediaCenterEnvironment.Capabilities["Video"];
                }
                if (isConsole == false)
                {
                    if (isVideo == true)
                        _isExtender = true;
                }
            } catch (Exception)
            {
                _isExtender = false;
            }
#if DEBUG_EXT
            System.Diagnostics.Debugger.Launch();
            _isExtender = true;
#endif
#if DEBUG
            OMLApplication.DebugLine("[OMLApplication] MediaCenterEnvironment.Capabilities:");
            foreach (KeyValuePair<string, object> cap in host.MediaCenterEnvironment.Capabilities)
                try { DebugLine("  ['{0}'] = '{1}'", cap.Key, cap.Value); }
                catch { }
#else
            OMLApplication.DebugLine("[OMLApplication] constructor called");
#endif
            this._host = host;
            _singleApplicationInstance = this;
            I18n.InitializeResourceManager();
            string uiCulture = Properties.Settings.Default.UILanguage;
            if (!string.IsNullOrEmpty(uiCulture)) I18n.SetCulture(new CultureInfo(uiCulture));
            _nowPlayingMovieName = "Playing an unknown movie";
        }

        void MediaExperience_PropertyChanged(IPropertyObject sender, string property)
        {
            DebugLine("[OMLApplication] MediaExperience Property {0} changed", property);
        }
        
        // this is the context from the Media Center menu
        public void Startup(string context)
        {
            OMLApplication.DebugLine("[OMLApplication] Startup({0}) {1}", context, IsExtender ? "Extender" : "Native");
#if CAROUSEL
            _session.GoToPage(@"resx://Library/Library.Resources/Trailers");
            return;
#endif
#if LAYOUT_V2
            OMLProperties properties = new OMLProperties();
            properties.Add("Application", this);
            properties.Add("UISettings", new UISettings());
            properties.Add("Settings", new Settings());
            properties.Add("I18n", I18n.Instance);
            properties.Add("Gallery", new GalleryV2(properties, _titles));
            //properties.Add("Gallery", new BaseGallery(properties, _titles));
            _session.GoToPage(@"resx://Library/Library.Resources/NewMenu", properties);
            return;
#endif
#if LAYOUT_V3
            //hooker.ButtonPressed += new Library.Code.V3.MoreInfoHooker2.RemoteControlDeviceEventHandler(hooker_ButtonPressed);
            //hooker.remo
            //Library.Code.V3.KeystrokeParser kp = new Library.Code.V3.KeystrokeParser();
            
            SetPrimaryBackgroundImage();
            //primaryBackgroundImage
            //this is temp to test controls
            OMLProperties properties = new OMLProperties();
            properties.Add("Application", this);
            properties.Add("UISettings", new UISettings());
            properties.Add("Settings", new Settings());
            properties.Add("I18n", I18n.Instance);
            //v3 main gallery
            Library.Code.V3.GalleryPage gallery = new Library.Code.V3.GalleryPage();
            //description
            gallery.Description = "OML";
            //size of the galleryitems
            gallery.ItemSize = Library.Code.V3.GalleryItemSize.Small;
            gallery.Model = new Library.Code.V3.BrowseModel(gallery);
            //commands at top of screen
            gallery.Model.Commands = new ArrayListDataSet(gallery);

            //create the context menu
            Library.Code.V3.ContextMenuData ctx = new Library.Code.V3.ContextMenuData();

            //create the settings cmd
            Library.Code.V3.ThumbnailCommand settingsCmd = new Library.Code.V3.ThumbnailCommand(gallery);
            settingsCmd.Description = "settings";
            settingsCmd.DefaultImage = new Image("resx://Library/Library.Resources/V3_Controls_Common_BrowseCmd_Settings");
            settingsCmd.DormantImage = new Image("resx://Library/Library.Resources/V3_Controls_Common_BrowseCmd_Settings_Dormant");
            settingsCmd.FocusImage = new Image("resx://Library/Library.Resources/V3_Controls_Common_BrowseCmd_Settings_Focus");
            //no invoke for now
            settingsCmd.Invoked += new EventHandler(this.SettingsHandler);
            gallery.Model.Commands.Add(settingsCmd);


            //some ctx items
            Library.Code.V3.ThumbnailCommand viewListCmd = new Library.Code.V3.ThumbnailCommand(gallery);
            viewListCmd.Description = "View List";
            viewListCmd.Invoked += new EventHandler(viewListCmd_Invoked);
            Library.Code.V3.ThumbnailCommand viewSmallCmd = new Library.Code.V3.ThumbnailCommand(gallery);
            viewSmallCmd.Invoked += new EventHandler(viewSmallCmd_Invoked);
            viewSmallCmd.Description = "View Small";
            ctx.SharedItems.Add(viewListCmd);
            ctx.SharedItems.Add(viewSmallCmd);
            ctx.SharedItems.Add(settingsCmd);
            //ctx.UniqueItems.Add(settingsCmd);
            Command CommandContextPopOverlay = new Command();
            properties.Add("CommandContextPopOverlay", CommandContextPopOverlay);

            properties.Add("MenuData", ctx);

            //the pivots
            gallery.Model.Pivots = new Choice(gallery, "desc", new ArrayListDataSet(gallery));

            //twoRowGalleryItemPoster
            #region oneRowGalleryItemPoster
            VirtualList galleryList = new VirtualList(gallery, null);
            foreach (Title t in _titles)
            {
                galleryList.Add(this.CreateGalleryItem(t));
            }


            Library.Code.V3.BrowsePivot p = new Library.Code.V3.BrowsePivot(gallery, "one row", "loading titles...", galleryList);
            p.ContentLabel = "OML";
            p.SupportsJIL = true;
            p.ContentTemplate = "resx://Library/Library.Resources/V3_Controls_BrowseGallery#Gallery";
            p.ContentItemTemplate = "oneRowGalleryItemPoster";
            p.DetailTemplate = "resx://Library/Library.Resources/V3_Controls_BrowseDetails#Details";
            gallery.Model.Pivots.Options.Add(p);
            #endregion oneRowGalleryItemPoster

            //twoRowGalleryItemPoster
            #region twoRowGalleryItemPoster
            VirtualList galleryListGenres = new VirtualList(gallery, null);
            foreach (Title t in _titles)
            {
                galleryListGenres.Add(this.CreateGalleryItem(t));
            }

            Library.Code.V3.BrowsePivot p2 = new Library.Code.V3.BrowsePivot(gallery, "two row", "loading genres...", galleryListGenres);
            p2.ContentLabel = "OML";
            p2.SupportsJIL = true;
            p2.ContentTemplate = "resx://Library/Library.Resources/V3_Controls_BrowseGallery#Gallery";
            p2.ContentItemTemplate = "twoRowGalleryItemPoster";
            p2.DetailTemplate = "resx://Library/Library.Resources/V3_Controls_BrowseDetails#Details";
            gallery.Model.Pivots.Options.Add(p2);
            #endregion twoRowGalleryItemPoster

            //ListViewItem
            #region ListViewItem
            VirtualList galleryListListViewItem = new VirtualList(gallery, null);
            foreach (Title t in _titles)
            {
                galleryListListViewItem.Add(this.CreateGalleryItem(t));
            }

            Library.Code.V3.BrowsePivot p3 = new Library.Code.V3.BrowsePivot(gallery, "list", "loading genres...", galleryListListViewItem);
            p3.ContentLabel = "OML";
            p3.SupportsJIL = true;
            p3.ContentTemplate = "resx://Library/Library.Resources/V3_Controls_BrowseGallery#Gallery";
            p3.ContentItemTemplate = "ListViewItem";
            p3.DetailTemplate = "resx://Library/Library.Resources/V3_Controls_BrowseDetails#Details";
            gallery.Model.Pivots.Options.Add(p3);
            #endregion ListViewItem

            //threeRowGalleryItemPoster
            #region ListViewItem
            //VirtualList galleryListthreeRowGalleryItemPoster = new VirtualList(gallery, null);
            //foreach (Title t in _titles)
            //{
            //    galleryListthreeRowGalleryItemPoster.Add(this.CreateGalleryItem(t));
            //}

            //Library.Code.V3.BrowsePivot p4 = new Library.Code.V3.BrowsePivot(gallery, "three row", "loading genres...", galleryListthreeRowGalleryItemPoster);
            //p4.ContentLabel = "OML";
            //p4.SupportsJIL = true;
            //p4.ContentTemplate = "resx://Library/Library.Resources/V3_Controls_BrowseGallery#Gallery";
            //p4.ContentItemTemplate = "ListViewItemPoster";
            //p4.DetailTemplate = "resx://Library/Library.Resources/V3_Controls_BrowseDetails#Details";
            //gallery.Model.Pivots.Options.Add(p4);
            #endregion threeRowGalleryItemPoster

            //properties.Add("Gallery", new GalleryV2(properties, _titles));
            properties.Add("Page", gallery);


            Library.Code.V3.MovieDetailsSlideDeck deck = new Library.Code.V3.MovieDetailsSlideDeck();
            //Choice c = new Choice();
            VirtualList Options = new VirtualList();
            Library.Code.V3.SlideBlueprint bp = new Library.Code.V3.SlideBlueprint(@"resx://Library/Library.Resources/V3_Slide_Movie_Details_Synopsis", "Synopsis", DateTime.MinValue, DateTime.Now);
            Library.Code.V3.SlideBlueprint bp2 = new Library.Code.V3.SlideBlueprint(@"resx://Library/Library.Resources/V3_Slide_Movie_Details_Actions", "Actions", DateTime.MinValue, DateTime.Now);
            Options.Add(bp);
            Options.Add(bp2);
            deck.Options = Options;
            deck.Commands = new ArrayListDataSet();
            
            //dummy up some cmds
            Library.Code.V3.ThumbnailCommand deleteCmd = new Library.Code.V3.ThumbnailCommand(deck);
            deleteCmd.Description = "Delete";
            deleteCmd.DefaultImage = new Image("resx://Library/Library.Resources/V3_Controls_Common_Browse_Cmd_Remove");
            deleteCmd.DormantImage = new Image("resx://Library/Library.Resources/V3_Controls_Common_Browse_Cmd_Remove_Dormant");
            deleteCmd.FocusImage = new Image("resx://Library/Library.Resources/V3_Controls_Common_Browse_Cmd_Remove_Focus");
            deck.Commands.Add(deleteCmd);

            Library.Code.V3.ThumbnailCommand playCmd = new Library.Code.V3.ThumbnailCommand(deck);
            playCmd.Description = "Play";
            playCmd.DefaultImage = new Image("resx://Library/Library.Resources/V3_Controls_Common_Browse_Cmd_Play");
            playCmd.DormantImage = new Image("resx://Library/Library.Resources/V3_Controls_Common_Browse_Cmd_Play_Dormant");
            playCmd.FocusImage = new Image("resx://Library/Library.Resources/V3_Controls_Common_Browse_Cmd_Play_Focus");
            deck.Commands.Add(playCmd);

            deck.Description = "descrip";
            deck.Synopsis = "this is a syn adfge rh rhyr yhyr hr hr ge ge gtwt rgwe tgew gr ewg weg ewg wetg wrt g rhtytjuhytgfr er gtwrt her  etju ktjy hgt efr erfgetw";
            deck.AdditionalCommands = new ArrayListDataSet();
            deck.CommandPopOverlay = new Command();
            deck.CommandPopOverlay.Invoked += new EventHandler(CommandPopOverlay_Invoked);
            deck.CommandClearOverlays = new Command();
            deck.CommandClearOverlays.Invoked += new EventHandler(CommandClearOverlays_Invoked);
            deck.CommandPushOverlay = new Command();
            deck.CommandPushOverlay.Invoked += new EventHandler(CommandPushOverlay_Invoked);

            //deck.AdditionalCommands.Add(cmd);
            properties.Add("SlideDeck", deck);
            properties.Add("CommandPopOverlay", deck.CommandPopOverlay);
            properties.Add("CommandClearOverlays", deck.CommandClearOverlays);
            properties.Add("CommandPushOverlay", deck.CommandPushOverlay);

            deck.Context = "hi";
            //_session.GoToPage(@"resx://Library/Library.Resources/V3_SlideDeck_Movie_Details", properties);

            _session.GoToPage(@"resx://Library/Library.Resources/V3_GalleryPage", properties);
            _page = gallery;
            _deck = deck;
            return;
#endif

            // DISABLE THIS UNTIL ITS READY -- DJShultz 01/13/2009
            //OMLUpdater updater = new OMLUpdater();
            //ThreadPool.QueueUserWorkItem(new WaitCallback(updater.checkUpdate));

            //TheMovieDbBackDropDownloader downloader = new TheMovieDbBackDropDownloader();
            //foreach (Title t in Titles)
            //{
            //    downloader.SearchForTitle(t);
            //}
            SetPrimaryBackgroundImage();

            switch (context)
            {
                case "Menu":
                    // Before running strait to the menu, check to see if we want to run the first-time setup
                    if (!Properties.Settings.Default.HasRunSetup)
                    {
                        OMLApplication.DebugLine("[OMLApplication] firstrun, going to setup");
                        GoToSetup(new MovieGallery());
                        Properties.Settings.Default.HasRunSetup = true;
                        Properties.Settings.Default.Save();
                        return;
                    }

                    // we want the movie library - check the startup page
                    if (Properties.Settings.Default.StartPage == Filter.Home)
                    {
                        OMLApplication.DebugLine("[OMLApplication] going to Menu Page");
                        GoToMenu(new MovieGallery());
                    }
                    else
                    {                                                
                        // see if they've selected a subfilter
                        // the unwatched is a special case until we add a user setting to determine the subfilter
                        if (!string.IsNullOrEmpty(Properties.Settings.Default.StartPageSubFilter)
                            || Properties.Settings.Default.StartPage == Filter.Unwatched)
                        {
                            // go to the subfilter
                            GoToMenu(
                                new MovieGallery(
                                    new TitleFilter(Filter.FilterStringToTitleType(Properties.Settings.Default.StartPage),
                                        Properties.Settings.Default.StartPageSubFilter)));
                        }
                        else
                        {
                            // go to the selection list
                            GoToSelectionList(new Filter(new MovieGallery(), Filter.FilterStringToTitleType(Properties.Settings.Default.StartPage), null));               
                        }
                    }
                    return;
                case "Settings":
                    OMLApplication.DebugLine("[OMLApplication] going to Settings Page");
                    GoToSettingsPage(new MovieGallery(TitleCollectionManager.GetAllTitles(), Filter.Settings));
                    return;
                case "Trailers":
                    OMLApplication.DebugLine("[OMLApplication] going to Trailers Page");
                    GoToTrailersPage();
                    return;
                case "About":
                    OMLApplication.DebugLine("[OMLApplication] going to About Page");
                    GoToAboutPage(new MovieGallery(TitleCollectionManager.GetAllTitles(), Filter.About));
                    return;
                default:
                    OMLApplication.DebugLine("[OMLApplication] going to Default (Menu) Page");
                    GoToMenu(new MovieGallery());
                    return;
            }
        }

        void viewListCmd_Invoked(object sender, EventArgs e)
        {
            Library.Code.V3.BrowsePivot p = (Library.Code.V3.BrowsePivot)this._page.Model.Pivots.Chosen;
            base.FirePropertyChanged("MoreInfo");
            p.ContentItemTemplate = "ListViewItem";
        }

        void viewSmallCmd_Invoked(object sender, EventArgs e)
        {
            Library.Code.V3.BrowsePivot p = (Library.Code.V3.BrowsePivot)this._page.Model.Pivots.Chosen;
            base.FirePropertyChanged("MoreInfo");
            p.ContentItemTemplate = "twoRowGalleryItemPoster";
        }

        //void hooker_ButtonPressed(object sender, Library.Code.V3.RemoteControlEventArgs e)
        //{
        //    //throw new NotImplementedException();
        //    this.CatchMoreInfo();
        //}

        public void CatchMoreInfo()
        {
            if (this._moreInfo == true)
                this._moreInfo = false;
            else
                this._moreInfo = true;
            base.FirePropertyChanged("MoreInfo");
        }

        private bool _moreInfo = false;
        public bool MoreInfo
        {
            get { return this._moreInfo; }
            set { this._moreInfo = value; }
        }

        void CommandPushOverlay_Invoked(object sender, EventArgs e)
        {
            
        }

        void CommandClearOverlays_Invoked(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        Library.Code.V3.MovieDetailsSlideDeck _deck;
        void CommandPopOverlay_Invoked(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            
        }

        Library.Code.V3.GalleryPage _page;
        internal Library.Code.V3.GalleryItem CreateGalleryItem(Title t)
        {
            
            Library.Code.V3.GalleryItem item = new Library.Code.V3.GalleryItem();

            item.InternalMovieItem = new MovieItem(t, null);
            item.ItemType = 0;
            string imageName = null;
            string moviePath = null;
            DateTime releaseDate = new DateTime(2000, 1, 1);
            item.MetadataTop = releaseDate.Year.ToString();


            //item.Description = "This is a Test";
            item.ItemId = 1;
            string starRating = "5";
            item.StarRating = starRating;
            string extendedMetadata = string.Empty;


            item.MetadataTop = releaseDate.Year.ToString();

            item.Metadata = "PG-13, 22 minutes\r\nblahblah";

            if (!string.IsNullOrEmpty(t.FrontCoverPath) && File.Exists(t.FrontCoverPath))
            {
                item.DefaultImage = new Image("file://" + t.FrontCoverPath);
            }
            item.Description = t.Name;

            item.Invoked += delegate(object sender, EventArgs args)
            {
                _deck.CommandPushOverlay.Invoke();
                //if (this._page != null)
                //    this._page.PageState.TransitionState = Library.Code.V3.PageTransitionState.NavigatingAwayForward;

                //Library.Code.V3.GalleryItem galleryItem = (Library.Code.V3.GalleryItem)sender;

                //// Navigate to a details page for this item.
                //MovieDetailsPage page = galleryItem.InternalMovieItem.CreateDetailsPage(galleryItem.InternalMovieItem);
                //OMLApplication.Current.GoToDetails(page);
            };

            return item;
        }

        //public DetailsPage CreateDetailsPage(int movieId)
        //{
        //    DetailsPage page = new DetailsPage();
        //    DataRow movieData = this.GetMovieData(movieId);
        //    MovieMetadata metadata = this.ExtractMetadata(movieData, movieId);
        //    page.Description = "movie details";
        //    page.Title = metadata.Title;
        //    page.Summary = metadata.Summary;
        //    page.Background = LoadImage(metadata.ImagePath);
        //    string cast = string.Empty;
        //    foreach (string actor in metadata.Actors)
        //    {
        //        DataSetHelpers.AppendCommaSeparatedValue(ref cast, actor);
        //    }
        //    page.Metadata = string.Format(Resources.Movies_Details_Metadata, new object[] { metadata.Genre, cast, metadata.Length, metadata.CountryShortName, metadata.ReleaseDate.Year });
        //    this.CreateDetailsCommands(page, movieData, movieId);
        //    return page;
        //}
        
        public void Uninitialize()
        {
            try
            {
                ExtenderDVDPlayer.Uninitialize(TitleCollectionManager.GetAllTitles());
            }
            catch (Exception err)
            {
                DebugLine("Unhandled Exception: {0}", err);
            }
            finally
            {
                // close the db connections
                TitleCollectionManager.CloseDBConnection();
                WatcherSettingsManager.CloseDBConnection();
            }            
        }

        public static OMLApplication Current
        {
            get { return _singleApplicationInstance; }
        }

        public HistoryOrientedPageSession Session
        {
            get { return _session; }
        }

        public MediaCenterEnvironment MediaCenterEnvironment
        {
            get
            {
                if (_host == null) return null;
                return _host.MediaCenterEnvironment;
            }
        }

        public void GoToSetup(MovieGallery gallery)
        {
            DebugLine("[OMLApplication] GoToSetup()");
            if (_session != null)
            {
                _session.GoToPage("resx://Library/Library.Resources/FirstRunSetup", CreateProperties(true, false, gallery));
            }
        }

        public void GoToBackPage()
        {
            DebugLine("[OMLApplication] GoToBackPage()");
            if (_session != null)
            {
                _session.BackPage();
            }
        }

        public void SettingsHandler(object sender, EventArgs e)
        {
            _session.GoToPage("resx://Library/Library.Resources/Settings_Main", CreateProperties(true, true, null));
        }

        public void GoToSettingsPage(MovieGallery gallery)
        {
            DebugLine("[OMLApplication] GoToSettingsPage()");
            if (_session != null)
            {
               _session.GoToPage("resx://Library/Library.Resources/Settings", CreateProperties(true, true, gallery));
            }
        }

        public void GoToSettings_AppearancePage(SettingsUIWrapper classSettingsUIWrapper)
        {
            DebugLine("[OMLApplication] GoToSettings_AppearancePage()");
            if (_session != null)
            {
                Dictionary<string, object> properties = new Dictionary<string, object>();

                Settings settings = new Settings();
                classSettingsUIWrapper.Init(settings);
                properties["ClassSettingsUIWrapper"] = classSettingsUIWrapper;
                properties["Settings"] = settings;
                properties["Application"] = this;

                _session.GoToPage("resx://Library/Library.Resources/Settings_Appearance", properties);
            }
        }

        public void GoToSettings_Appearance_GeneralPage(SettingsUIWrapper classSettingsUIWrapper)
        {
            DebugLine("[OMLApplication] GoToSettings_Appearance_GeneralPage()");
            if (_session != null)
            {
                Dictionary<string, object> properties = new Dictionary<string, object>();

                Settings settings = new Settings();
                classSettingsUIWrapper.Init(settings);
                properties["ClassSettingsUIWrapper"] = classSettingsUIWrapper;
                properties["Settings"] = settings;
                properties["Application"] = this;

                _session.GoToPage("resx://Library/Library.Resources/Settings_Appearance_General", properties);
            }
        }

        public void GoToSettings_Appearance_DetailViewPage(SettingsUIWrapper classSettingsUIWrapper)
        {
            DebugLine("[OMLApplication] GoToSettings_Appearance_GeneralPage()");
            if (_session != null)
            {
                Dictionary<string, object> properties = new Dictionary<string, object>();

                Settings settings = new Settings();
                classSettingsUIWrapper.Init(settings);
                properties["ClassSettingsUIWrapper"] = classSettingsUIWrapper;
                properties["Settings"] = settings;
                properties["Application"] = this;

                _session.GoToPage("resx://Library/Library.Resources/Settings_Appearance_DetailView", properties);
            }
        }

        public void GoToSettings_Appearance_GalleryViewPage(SettingsUIWrapper classSettingsUIWrapper)
        {
            DebugLine("[OMLApplication] GoToSettings_Appearance_GalleryViewPage()");
            if (_session != null)
            {
                Dictionary<string, object> properties = new Dictionary<string, object>();

                Settings settings = new Settings();
                classSettingsUIWrapper.Init(settings);
                properties["ClassSettingsUIWrapper"] = classSettingsUIWrapper;
                properties["Settings"] = settings;
                properties["Application"] = this;

                _session.GoToPage("resx://Library/Library.Resources/Settings_Appearance_GalleryView", properties);
            }
        }

        public void GoToSettings_ExternalInterfacePage(SettingsUIWrapper classSettingsUIWrapper)
        {
            DebugLine("[OMLApplication] GoToSettings_ExternalInterfacePage()");
            if (_session != null)
            {
                Dictionary<string, object> properties = new Dictionary<string, object>();

                Settings settings = new Settings();
                classSettingsUIWrapper.Init(settings);
                properties["ClassSettingsUIWrapper"] = classSettingsUIWrapper;
                properties["Settings"] = settings;
                properties["Application"] = this;

                _session.GoToPage("resx://Library/Library.Resources/Settings_ExternalInterface", properties);
            }
        }

        public void GoToSettings_FiltersPage(SettingsUIWrapper classSettingsUIWrapper)
        {
            DebugLine("[OMLApplication] GoToSettings_FiltersPage()");
            if (_session != null)
            {
                Dictionary<string, object> properties = new Dictionary<string, object>();

                Settings settings = new Settings();
                classSettingsUIWrapper.Init(settings);
                properties["ClassSettingsUIWrapper"] = classSettingsUIWrapper;
                properties["Settings"] = settings;
                properties["Application"] = this;

                _session.GoToPage("resx://Library/Library.Resources/Settings_Filters", properties);
            }
        }

        public void GoToSettings_TrailersPage(SettingsUIWrapper classSettingsUIWrapper)
        {
            DebugLine("[OMLApplication] GoToSettings_TrailersPage()");
            if (_session != null)
            {
                Dictionary<string, object> properties = new Dictionary<string, object>();

                Settings settings = new Settings();
                classSettingsUIWrapper.Init(settings);
                properties["ClassSettingsUIWrapper"] = classSettingsUIWrapper;
                properties["Settings"] = settings;
                properties["Application"] = this;

                _session.GoToPage("resx://Library/Library.Resources/Settings_Trailers", properties);
            }
        }

        public void GoToSettings_AboutPage()
        {
            DebugLine("[OMLApplication] GoToSettings_AboutPage()");
            if (_session != null)
            {
                Dictionary<string, object> properties = new Dictionary<string, object>();
                properties["Application"] = this;

                _session.GoToPage("resx://Library/Library.Resources/Settings_About", properties);
            }
        }

        public void GoToTrailersPage()
        {
            DebugLine("[OMLApplication] GoToTrailersPage()");
            if (_session != null)
            {
                _session.GoToPage("resx://Library/Library.Resources/Trailers", CreateProperties(true, true, null));
            }
        }

        public void GoToAboutPage(MovieGallery gallery)
        {
            DebugLine("[OMLApplication] GotoAboutPage()");
            if (_session != null)
                _session.GoToPage("resx://Library/Library.Resources/About", CreateProperties(true, false, gallery));
        }        

        public void GoToMenu(MovieGallery gallery)
        {
            DebugLine("[OMLApplication] GoToMenu(Gallery, #{0} Movies)", gallery.Movies.Count);
            Dictionary<string, object> properties = CreateProperties(true, false, gallery);

            if (Properties.Settings.Default.MovieView == GalleryView.CoverArtWithAlpha &&
                gallery.Movies.Count < 30)
            {
                // alpha falls back to cover art view if there's not enough items
                properties["GalleryView"] = GalleryView.CoverArt;
            }
            else
            {
                properties["GalleryView"] = Properties.Settings.Default.MovieView;
            }

            if (_session != null)
            {
                _session.GoToPage("resx://Library/Library.Resources/Menu", properties);
            }
            //IsBusy = true; why do this?
        }

        public void GoToSelectionList(Filter filter)
        {
            // currently the selection list page uses the same gallery object as the previous page
            // we shoud look into refactoring this so the selection logic is in a different object than
            // the movie list so we can deal with selections across pages better
            MovieGallery gallery = filter.Gallery;

            // reset the index
            gallery.FocusIndex.Value = 0;

            //DebugLine("[OMLApplication] GoToSelectionList(#{0} items, list name: {1}, gallery: {2})", list.Count, listName, galleryView);
            Dictionary<string, object> properties = CreateProperties(true, false, gallery);
            properties["MovieBrowser"] = gallery;
            properties["List"] = filter.GetGalleryItems();
            properties["ListName"] = filter.Title;
            properties["GalleryView"] = filter.GetViewForFilter();

            if (_session != null)
            {
                _session.GoToPage("resx://Library/Library.Resources/SelectionList", properties);
            }            
        }

        public void GoToDetails(MovieDetailsPage page)
        {
            DebugLine("[OMLApplication] GoToDetails({0})", page);
            if (page == null)
                throw new System.Exception("The method or operation is not implemented.");

            //
            // Construct the arguments dictionary and then navigate to the
            // details page template.
            //
            Dictionary<string, object> properties = CreateProperties(true, false, null);
            properties["DetailsPage"] = page;

            // If we have no page session, just spit out a trace statement.
            if (_session != null)
            {
                //FirePropertyChanged("ShowDetailsPopup");
                switch (Properties.Settings.Default.DetailsView)
                {
                    case "Background Boxes":
                        _session.GoToPage("resx://Library/Library.Resources/DetailsPage_Boxes", properties);
                        break;

                    case "Original":
                    default:
                        _session.GoToPage("resx://Library/Library.Resources/DetailsPage", properties);
                        break;
                }                
            }
        }

        public static void DebugLine(string msg, params object[] paramArray)
        {
            Utilities.DebugLine(msg, paramArray);
        }

        // properties
        public bool IsExtender
        {
            get { return _isExtender; }
        }

        public string NowPlayingMovieName
        {
            get { return _nowPlayingMovieName; }
            set
            {
                if (_nowPlayingMovieName == value)
                    return;
                Utilities.DebugLine("[OMLApplication] NowPlayingMovieName {0}", value);
                _nowPlayingMovieName = value;
                FirePropertyChanged("NowPlaying");
            }
        }

        public PlayState NowPlayingStatus
        {
            get { return _nowPlayingStatus; }
            set 
            {
                if (_nowPlayingStatus == value)
                    return;
                Utilities.DebugLine("[OMLApplication] NowPlayingStatus {0}", value);
                FirePropertyChanged("NowPlaying");  
                _nowPlayingStatus = value; 
            }
        }

        public string NowPlaying
        {
            get { return NowPlayingStatus.ToString() + ": " + NowPlayingMovieName; }
        }

        /*public TitleCollection ReloadTitleCollection()
        {
            DebugLine("[OMLApplication] ReloadTitleCollection()");
            _titles.loadTitleCollection();
            return _titles;
        }*/

        /*public void SaveTitles()
        {
            //_titles.saveTitleCollection();
        }*/

        public static void ExecuteSafe(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                DebugLine("Unhandled Exception: {0}", ex);
                DialogResult res = Current.MediaCenterEnvironment.Dialog("To rethrow exception select yes\n" + ex.ToString(), 
                    "Unhandled Exception logged in debug.txt", DialogButtons.Yes | DialogButtons.No, 10, true);
                if (res == DialogResult.Yes)
                    throw;
            }
        }

        private Dictionary<string, object> CreateProperties(bool uiSettings, bool settings, MovieGallery gallery)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties["Application"] = this;
            properties["I18n"] = I18n.Instance;
            if (uiSettings)
                properties["UISettings"] = new UISettings();
            if (settings)
                properties["Settings"] = new Settings();
            if (gallery != null)
                properties["MovieBrowser"] = gallery;
            return properties;
        }

        static public void MediaCenterEnvironment_PropertyChanged(IPropertyObject sender, string property)
        {
            DebugLine("[OMLApplication] Property {0} changed on the MediaCenterEnvironment", property);
        }

        static public void AddInHost_PropertyChanged(IPropertyObject sender, string property)
        {
            DebugLine("[OMLApplication] Property {0} changed on the AddInHost", property);
        }
        // private data
        private string _nowPlayingMovieName;
        private PlayState _nowPlayingStatus;

        private static OMLApplication _singleApplicationInstance;
        private AddInHost _host;
        private HistoryOrientedPageSession _session;

        private bool _isExtender;
    }
}