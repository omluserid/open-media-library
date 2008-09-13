﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace OMLGetDVDInfo
{
    public class DVDTitle
    {
        #region -- Members --
        [XmlElement("Chapter")]
        public List<DVDChapter> Chapters;

        [XmlElement("AudioTrack")]
        public List<DVDAudioTrack> AudioTracks;

        [XmlElement("SubTitle")]
        public List<DVDSubtitle> Subtitles;

        [XmlAttribute]
        public string File;

        [XmlAttribute]
        public int TitleNumber;

        [XmlIgnore]
        public TimeSpan Duration;
        [XmlAttribute("Duration")]
        public string _Duration { get { return Duration.ToString(); } set { Duration = TimeSpan.Parse(value); } }

        [XmlElement]
        public Size Resolution;

        [XmlAttribute]
        public float AspectRatio;

        [XmlElement("AutoCrop")]
        public int[] AutoCrop;

        [XmlAttribute]
        public bool Main;
        #endregion

        public override bool Equals(object obj) { return this == (DVDTitle)obj; }
        public override int GetHashCode() { return base.GetHashCode(); }

        public static bool operator ==(DVDTitle a, DVDTitle b)
        {
            if ((object)a == (object)b) return true;
            if ((object)b == null) return false;
            if (a.TitleNumber != b.TitleNumber)
                return false;
            if (a.Chapters.Count != b.Chapters.Count)
                return false;
            if (a.AudioTracks.Count != b.AudioTracks.Count)
                return false;
            if (a.Subtitles.Count != b.Subtitles.Count)
                return false;
            for (int i = 0; i < a.Chapters.Count; ++i)
                if (a.Chapters[i] != b.Chapters[i])
                    return false;
            for (int i = 0; i < a.AudioTracks.Count; ++i)
                if (a.AudioTracks[i] != b.AudioTracks[i])
                    return false;
            for (int i = 0; i < a.Subtitles.Count; ++i)
                if (a.Subtitles[i] != b.Subtitles[i])
                    return false;
            if (a.AspectRatio != b.AspectRatio)
                return false;
            if (a.AspectRatio != b.AspectRatio)
                return false;
            if (a.File != b.File)
                return false;
            if (a.Resolution != b.Resolution)
                return false;
            // this.AutoCrop;
            return true;
        }
        public static bool operator !=(DVDTitle a, DVDTitle b) { return !(a == b); }

        internal static TimeSpan GetTotalTimeSpan(List<DVDChapter> chapters)
        {
            TimeSpan ret = TimeSpan.Zero;
            foreach (DVDChapter chapter in chapters)
                ret += chapter.Duration;
            return ret;
        }

        public int? FPS { get { return this.Chapters.Count > 0 ? (int?)this.Chapters[0].FPS : null; } }

        public override string ToString()
        {
            return string.Format("{0}, {1}, ({2:00}:{3:00}:{4:00}) {5}, {6}", this.File, this.TitleNumber, this.Duration.Hours,
                this.Duration.Minutes, this.Duration.Seconds, this.Resolution, this.AspectRatio);
        }

        #region -- Parsing --
        internal static DVDTitle Parse(TextReader output)
        {
            DVDTitle thisTitle = new DVDTitle();
            Match m = Regex.Match(output.ReadLine(), @"^\+ title ([0-9]*):");
            // Match track number for this title
            if (m.Success)
                thisTitle.TitleNumber = int.Parse(m.Groups[1].Value.Trim().ToString());

            m = Regex.Match(output.ReadLine(), @"^  \+ (vts [0-9]+), ttn [0-9]+, cells ([0-9]+)->([0-9]+) \(([0-9]+) blocks\)");
            // Match track number for this title
            if (m.Success)
                thisTitle.File = m.Groups[1].Value.Trim();

            // Get duration for this title

            m = Regex.Match(output.ReadLine(), @"^  \+ duration: ([0-9]{2}:[0-9]{2}:[0-9]{2})");
            if (m.Success)
                thisTitle.Duration = TimeSpan.Parse(m.Groups[1].Value);

            // Get resolution, aspect ratio and FPS for this title
            m = Regex.Match(output.ReadLine(), @"^  \+ size: ([0-9]*)x([0-9]*), aspect: ([0-9]*\.[0-9]*), ([0-9]*\.[0-9]*) fps");
            if (m.Success)
            {
                thisTitle.Resolution = new Size(int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value));
                thisTitle.AspectRatio = float.Parse(m.Groups[3].Value);
            }

            // Get autocrop region for this title
            m = Regex.Match(output.ReadLine(), @"^  \+ autocrop: ([0-9]*)/([0-9]*)/([0-9]*)/([0-9]*)");
            if (m.Success)
                thisTitle.AutoCrop = new int[4] { int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value), int.Parse(m.Groups[3].Value), int.Parse(m.Groups[4].Value) };

            thisTitle.Chapters = DVDChapter.ParseList(output);
            thisTitle.AudioTracks = DVDAudioTrack.ParseList(output);
            thisTitle.Subtitles = DVDSubtitle.ParseList(output);

            return thisTitle;
        }

        internal static List<DVDTitle> ParseList(string output)
        {
            List<DVDTitle> titles = new List<DVDTitle>();
            using (StringReader sr = new StringReader(output))
            {
                while ((char)sr.Peek() == '+')
                    titles.Add(DVDTitle.Parse(sr));
                return titles;
            }
        }
        #endregion
    }

    #region -- Enums --
    public enum VideoRes
    {
        Res_720X576,
        Res_704X576,
        Res_352X576,
        Res_352X288
    }

    public enum AudioEncoding
    {
        AC3,
        MPEG1,
        MPEG2,
        LPCM,
        DTS,
        Undefined
    }
    #endregion

    #region -- Audio/Video Values --
    public class AudioValues
    {
        public bool LanguageTypePresent { get; private set; }
        public AudioEncoding Encoding { get; private set; }
        public int Channels { get; private set; }
        public int SampleRate { get; private set; }
        public string Extension { get; private set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Language Type Contained: " + LanguageTypePresent);
            sb.AppendLine("Encoding: " + Encoding);
            sb.AppendLine("Channels: " + Channels);
            sb.AppendLine("Sample Rate (Kbps): " + SampleRate);
            return sb.ToString();
        }

        // http://dvd.sourceforge.net/dvdinfo/ifo.html#audatt
        public static AudioValues ReadAudioSpecs(byte b0, byte b1, byte b5)
        {
            int byte0_high = (b0 & 0xF0) >> 4;
            int byte0_low = (b0 & 0x0F);
            int byte1_high = (b1 & 0xF0) >> 4;
            int byte1_low = (b1 & 0x0F);

            AudioValues result = new AudioValues();

            switch (b5)
            {
                case 0: result.Extension = "Unspecified"; break;
                case 1: result.Extension = "Normal"; break;
                case 2: result.Extension = "For visually impaired"; break;
                case 3: result.Extension = "Director's comments"; break;
                case 4: result.Extension = "Alternate director's comments"; break;
            }

            result.LanguageTypePresent = (byte0_low & 0x04) == 0x04;
            result.Channels = (byte1_low & 0x07) + 1;

            if ((byte1_high & 0x03) == 0)
                result.SampleRate = 0x30;

            switch (byte0_high >> 1)
            {
                case 0: result.Encoding = AudioEncoding.AC3; break;
                case 1: result.Encoding = AudioEncoding.Undefined; break;
                case 2: result.Encoding = AudioEncoding.MPEG1; break;
                case 3: result.Encoding = AudioEncoding.MPEG2; break;
                case 4: result.Encoding = AudioEncoding.LPCM; break;
                case 5: result.Encoding = AudioEncoding.Undefined; break;
                case 6: result.Encoding = AudioEncoding.DTS; break;
                case 7: result.Encoding = AudioEncoding.Undefined; break;
            }
            return result;
        }
    }

    public class VideoValues
    {
        public bool DisallowAutoLetterbox { get; private set; }
        public bool DisallowAutoPanScan { get; private set; }
        public float AspectRatio { get; private set; }
        public bool Pal { get; private set; }
        public bool Ntsc { get; private set; }
        public bool Mpeg1 { get; private set; }
        public bool Mpeg2 { get; private set; }
        public bool Film { get; private set; }
        public bool LetterBoxedMode { get; private set; }
        public Size Resolution { get; private set; }
        public bool BitRate_Vbr { get; private set; }
        public bool BitRate_Cbr { get; private set; }
        public bool CC_line21_f2 { get; private set; }
        public bool CC_line21_f1 { get; private set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("Video Format: {0}", Pal ? "PAL" : Ntsc ? "NTSC" : "Unknown"));
            sb.AppendLine(string.Format("Aspect Ratio: {0}", AspectRatio));
            sb.AppendLine(string.Format("MPEG Ver.: {0}", Mpeg1 ? "MPEG-1" : Mpeg2 ? "MPEG-2" : "Unknown"));
            sb.AppendLine(string.Format("Mode: {0}", Film ? "Film" : "Camera"));
            sb.AppendLine(string.Format("LetterBoxed Mode: {0}", LetterBoxedMode));
            sb.AppendLine(string.Format("Video Resolution: {0}", Resolution));
            sb.AppendLine(string.Format("Bit Rate: {0}", BitRate_Vbr ? "Variable" : BitRate_Cbr ? "Constant" : "Unknown"));
            sb.AppendLine(string.Format("Disallow Auto Letterbox: {0}", DisallowAutoLetterbox));
            sb.AppendLine(string.Format("Disallow Auto PanScan: {0}", DisallowAutoPanScan));
            return sb.ToString();
        }

        // http://dvd.sourceforge.net/dvdinfo/ifo.html#vidatt
        public static VideoValues ReadVideoSpecs(byte b1, byte b2)
        {
            int byte0_high = (b1 & 0xF0) >> 4;
            int byte0_low = (b1 & 0x0F);
            int byte1_high = (b2 & 0xF0) >> 4;
            int byte1_low = (b2 & 0x0F);

            VideoValues result = new VideoValues();
            result.DisallowAutoLetterbox = (byte0_low & 1) != 0;
            result.DisallowAutoPanScan = (byte0_low & 2) != 0;
            result.AspectRatio = (byte0_low & 0x0C) == 0 ? 1.33f : 1.78f;
            if (((byte0_low & 0x0C) != 0) && ((byte0_low & 0x0C) != 0x0C))
                throw new Exception("the aspectRatio is not valid");
            result.Ntsc = (byte0_high & 1) == 0;
            result.Pal = (byte0_high & 1) != 0;
            result.Mpeg1 = (byte0_high & 0x0C) == 0;
            result.Mpeg2 = (byte0_high & 0x0C) == 4;
            result.Film = (byte1_low & 1) != 0;
            result.LetterBoxedMode = (byte1_low & 2) != 0;

            // 0 = 720x480 (720x576)
            // 1 = 704x480 (704x576)
            // 2 = 352x480 (352x576)
            // 3 = 352x240 (352x288)
            if (result.Ntsc)
                switch ((byte1_low >> 2) & 0x03)
                {
                    case 0: result.Resolution = new Size(720, 480); break;
                    case 1: result.Resolution = new Size(704, 480); break;
                    case 2: result.Resolution = new Size(352, 480); break;
                    case 3: result.Resolution = new Size(352, 240); break;
                }
            else
                switch ((byte1_low >> 2) & 0x03)
                {
                    case 0: result.Resolution = new Size(720, 576); break;
                    case 1: result.Resolution = new Size(704, 576); break;
                    case 2: result.Resolution = new Size(352, 576); break;
                    case 3: result.Resolution = new Size(352, 288); break;
                }
            result.BitRate_Vbr = (byte1_high & 1) == 0;
            result.BitRate_Cbr = (byte1_high & 1) != 0;
            result.CC_line21_f2 = (byte1_high & 4) != 0;
            result.CC_line21_f1 = (byte1_high & 8) != 0;
            return result;
        }
    }
    #endregion
}
