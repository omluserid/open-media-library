/****** Object:  ForeignKey [FK_Disks_Disks]    Script Date: 01/02/2009 11:19:13 ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Disks_Disks]') AND parent_object_id = OBJECT_ID(N'[dbo].[Disks]'))
ALTER TABLE [dbo].[Disks] DROP CONSTRAINT [FK_Disks_Disks]
GO
/****** Object:  ForeignKey [FK_Genres_GenreMetaData]    Script Date: 01/02/2009 11:19:13 ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Genres_GenreMetaData]') AND parent_object_id = OBJECT_ID(N'[dbo].[Genres]'))
ALTER TABLE [dbo].[Genres] DROP CONSTRAINT [FK_Genres_GenreMetaData]
GO
/****** Object:  ForeignKey [FK_Genres_Titles]    Script Date: 01/02/2009 11:19:13 ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Genres_Titles]') AND parent_object_id = OBJECT_ID(N'[dbo].[Genres]'))
ALTER TABLE [dbo].[Genres] DROP CONSTRAINT [FK_Genres_Titles]
GO
/****** Object:  ForeignKey [FK_People_Bio]    Script Date: 01/02/2009 11:19:13 ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_People_Bio]') AND parent_object_id = OBJECT_ID(N'[dbo].[People]'))
ALTER TABLE [dbo].[People] DROP CONSTRAINT [FK_People_Bio]
GO
/****** Object:  ForeignKey [FK_People_Titles]    Script Date: 01/02/2009 11:19:13 ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_People_Titles]') AND parent_object_id = OBJECT_ID(N'[dbo].[People]'))
ALTER TABLE [dbo].[People] DROP CONSTRAINT [FK_People_Titles]
GO
/****** Object:  ForeignKey [FK_Tags_Titles]    Script Date: 01/02/2009 11:19:13 ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Tags_Titles]') AND parent_object_id = OBJECT_ID(N'[dbo].[Tags]'))
ALTER TABLE [dbo].[Tags] DROP CONSTRAINT [FK_Tags_Titles]
GO
/****** Object:  Table [dbo].[Disks]    Script Date: 01/02/2009 11:19:13 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Disks]') AND type in (N'U'))
DROP TABLE [dbo].[Disks]
GO
/****** Object:  Table [dbo].[People]    Script Date: 01/02/2009 11:19:13 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[People]') AND type in (N'U'))
DROP TABLE [dbo].[People]
GO
/****** Object:  Table [dbo].[Tags]    Script Date: 01/02/2009 11:19:13 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tags]') AND type in (N'U'))
DROP TABLE [dbo].[Tags]
GO
/****** Object:  Table [dbo].[Genres]    Script Date: 01/02/2009 11:19:13 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Genres]') AND type in (N'U'))
DROP TABLE [dbo].[Genres]
GO
/****** Object:  Table [dbo].[Titles]    Script Date: 01/02/2009 11:19:13 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Titles]') AND type in (N'U'))
DROP TABLE [dbo].[Titles]
GO
/****** Object:  Table [dbo].[GenreMetaData]    Script Date: 01/02/2009 11:19:13 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GenreMetaData]') AND type in (N'U'))
DROP TABLE [dbo].[GenreMetaData]
GO
/****** Object:  Table [dbo].[BioData]    Script Date: 01/02/2009 11:19:13 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BioData]') AND type in (N'U'))
DROP TABLE [dbo].[BioData]
GO
/****** Object:  Table [dbo].[BioData]    Script Date: 01/02/2009 11:19:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BioData]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[BioData](
	[FullName] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Photo] [image] NULL,
	[Id] [bigint] IDENTITY(-9223372036854775808,1) NOT NULL,
	[DateOfBirth] [smalldatetime] NULL,
 CONSTRAINT [PK_BioData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[BioData]') AND name = N'IX_BioData')
CREATE NONCLUSTERED INDEX [IX_BioData] ON [dbo].[BioData] 
(
	[FullName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
/****** Object:  Table [dbo].[GenreMetaData]    Script Date: 01/02/2009 11:19:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GenreMetaData]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[GenreMetaData](
	[Name] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Photo] [image] NULL,
	[Id] [bigint] IDENTITY(-9223372036854775808,1) NOT NULL,
 CONSTRAINT [PK_GenreMetaData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Table [dbo].[Titles]    Script Date: 01/02/2009 11:19:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Titles]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Titles](
	[Name] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Id] [int] IDENTITY(-2147483647,1) NOT NULL,
	[SortName] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[WatchedCount] [int] NULL,
	[MetaDataSource] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Runtime] [smallint] NULL,
	[ParentalRating] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Synopsis] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Studio] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CountryOfOrigin] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[WebsiteUrl] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ReleaseDate] [smalldatetime] NULL,
	[DateAdded] [smalldatetime] NULL,
	[AudioTracks] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[UserRating] [tinyint] NULL,
	[AspectRatio] [nvarchar](5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[VideoStandard] [nvarchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[UPC] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Trailers] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ParentalRatingReason] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[VideoDetails] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Subtitles] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[VideoResolution] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[FrontCoverImagePath] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[MenuImagePath] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[BackCoverImagePath] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[OriginalName] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ImporterSource] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[GroupId] [int] NULL,
 CONSTRAINT [PK_Titles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Table [dbo].[Genres]    Script Date: 01/02/2009 11:19:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Genres]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Genres](
	[TitleId] [int] NOT NULL,
	[GenreMetaDataId] [bigint] NOT NULL,
	[Id] [bigint] IDENTITY(-9223372036854775808,1) NOT NULL,
 CONSTRAINT [PK_Genres] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Table [dbo].[Tags]    Script Date: 01/02/2009 11:19:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tags]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Tags](
	[MovieId] [int] NOT NULL,
	[Name] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Id] [bigint] IDENTITY(-9223372036854775808,1) NOT NULL,
 CONSTRAINT [PK_Tags] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Table [dbo].[People]    Script Date: 01/02/2009 11:19:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[People]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[People](
	[CharacterName] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[TitleId] [int] NOT NULL,
	[Sort] [smallint] NOT NULL,
	[Role] [tinyint] NOT NULL,
	[BioId] [bigint] NOT NULL,
	[Id] [bigint] IDENTITY(-9223372036854775808,1) NOT NULL,
 CONSTRAINT [PK_People] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[People]') AND name = N'IX_People')
CREATE NONCLUSTERED INDEX [IX_People] ON [dbo].[People] 
(
	[BioId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
/****** Object:  Table [dbo].[Disks]    Script Date: 01/02/2009 11:19:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Disks]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Disks](
	[Name] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Path] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[VideoFormat] [tinyint] NOT NULL,
	[Id] [bigint] IDENTITY(-9223372036854775808,1) NOT NULL,
	[TitleId] [int] NOT NULL,
	[ExtraOptions] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_Disks] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  ForeignKey [FK_Disks_Disks]    Script Date: 01/02/2009 11:19:13 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Disks_Disks]') AND parent_object_id = OBJECT_ID(N'[dbo].[Disks]'))
ALTER TABLE [dbo].[Disks]  WITH CHECK ADD  CONSTRAINT [FK_Disks_Disks] FOREIGN KEY([TitleId])
REFERENCES [dbo].[Titles] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Disks_Disks]') AND parent_object_id = OBJECT_ID(N'[dbo].[Disks]'))
ALTER TABLE [dbo].[Disks] CHECK CONSTRAINT [FK_Disks_Disks]
GO
/****** Object:  ForeignKey [FK_Genres_GenreMetaData]    Script Date: 01/02/2009 11:19:13 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Genres_GenreMetaData]') AND parent_object_id = OBJECT_ID(N'[dbo].[Genres]'))
ALTER TABLE [dbo].[Genres]  WITH CHECK ADD  CONSTRAINT [FK_Genres_GenreMetaData] FOREIGN KEY([GenreMetaDataId])
REFERENCES [dbo].[GenreMetaData] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Genres_GenreMetaData]') AND parent_object_id = OBJECT_ID(N'[dbo].[Genres]'))
ALTER TABLE [dbo].[Genres] CHECK CONSTRAINT [FK_Genres_GenreMetaData]
GO
/****** Object:  ForeignKey [FK_Genres_Titles]    Script Date: 01/02/2009 11:19:13 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Genres_Titles]') AND parent_object_id = OBJECT_ID(N'[dbo].[Genres]'))
ALTER TABLE [dbo].[Genres]  WITH CHECK ADD  CONSTRAINT [FK_Genres_Titles] FOREIGN KEY([TitleId])
REFERENCES [dbo].[Titles] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Genres_Titles]') AND parent_object_id = OBJECT_ID(N'[dbo].[Genres]'))
ALTER TABLE [dbo].[Genres] CHECK CONSTRAINT [FK_Genres_Titles]
GO
/****** Object:  ForeignKey [FK_People_Bio]    Script Date: 01/02/2009 11:19:13 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_People_Bio]') AND parent_object_id = OBJECT_ID(N'[dbo].[People]'))
ALTER TABLE [dbo].[People]  WITH CHECK ADD  CONSTRAINT [FK_People_Bio] FOREIGN KEY([BioId])
REFERENCES [dbo].[BioData] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_People_Bio]') AND parent_object_id = OBJECT_ID(N'[dbo].[People]'))
ALTER TABLE [dbo].[People] CHECK CONSTRAINT [FK_People_Bio]
GO
/****** Object:  ForeignKey [FK_People_Titles]    Script Date: 01/02/2009 11:19:13 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_People_Titles]') AND parent_object_id = OBJECT_ID(N'[dbo].[People]'))
ALTER TABLE [dbo].[People]  WITH CHECK ADD  CONSTRAINT [FK_People_Titles] FOREIGN KEY([TitleId])
REFERENCES [dbo].[Titles] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_People_Titles]') AND parent_object_id = OBJECT_ID(N'[dbo].[People]'))
ALTER TABLE [dbo].[People] CHECK CONSTRAINT [FK_People_Titles]
GO
/****** Object:  ForeignKey [FK_Tags_Titles]    Script Date: 01/02/2009 11:19:13 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Tags_Titles]') AND parent_object_id = OBJECT_ID(N'[dbo].[Tags]'))
ALTER TABLE [dbo].[Tags]  WITH CHECK ADD  CONSTRAINT [FK_Tags_Titles] FOREIGN KEY([MovieId])
REFERENCES [dbo].[Titles] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Tags_Titles]') AND parent_object_id = OBJECT_ID(N'[dbo].[Tags]'))
ALTER TABLE [dbo].[Tags] CHECK CONSTRAINT [FK_Tags_Titles]
GO
