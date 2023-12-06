USE [Medius_Database]
GO

IF EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'ACCOUNTS')
BEGIN
    RAISERROR ('Schemas already exist. Script terminated.', 16, 1)
    RETURN
END
GO

/****** Object:  Schema [ACCOUNTS]    Script Date: 8/9/2022 8:53:13 AM ******/
CREATE SCHEMA [ACCOUNTS]
GO
/****** Object:  Schema [CLANS]    Script Date: 8/9/2022 8:53:13 AM ******/
CREATE SCHEMA [CLANS]
GO
/****** Object:  Schema [KEYS]    Script Date: 8/9/2022 8:53:13 AM ******/
CREATE SCHEMA [KEYS]
GO
/****** Object:  Schema [LOGS]    Script Date: 8/9/2022 8:53:13 AM ******/
CREATE SCHEMA [LOGS]
GO
/****** Object:  Schema [STATS]    Script Date: 8/9/2022 8:53:13 AM ******/
CREATE SCHEMA [STATS]
GO
/****** Object:  Schema [WORLD]    Script Date: 8/9/2022 8:53:13 AM ******/
CREATE SCHEMA [WORLD]
GO
/****** Object:  Table [ACCOUNTS].[account]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [ACCOUNTS].[account](
	[account_id] [int] IDENTITY(1,1) NOT NULL,
	[account_name] [nvarchar](32) NOT NULL,
	[account_password] [nvarchar](200) NOT NULL,
	[create_dt] [datetime2](7) NOT NULL,
	[modified_dt] [datetime2](7) NULL,
	[last_sign_in_dt] [datetime2](7) NULL,
	[machine_id] [nvarchar](100) NULL,
	[is_active] [bit] NOT NULL,
	[app_id] [int] NULL,
	[medius_stats] [nvarchar](350) NULL,
	[last_sign_in_ip] [nvarchar](50) NULL,
	[metadata] [nvarchar](max) NULL,
	[reset_password_on_next_login] [bit] NOT NULL,
 CONSTRAINT [PK_account] PRIMARY KEY CLUSTERED 
(
	[account_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [ACCOUNTS].[account_friend]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [ACCOUNTS].[account_friend](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[account_id] [int] NOT NULL,
	[friend_account_id] [int] NOT NULL,
	[create_dt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_account_friend] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [ACCOUNTS].[account_ignored]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [ACCOUNTS].[account_ignored](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[account_id] [int] NOT NULL,
	[ignored_account_id] [int] NOT NULL,
	[create_dt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_account_ignored] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [ACCOUNTS].[account_status]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [ACCOUNTS].[account_status](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[app_id] [int] NOT NULL,
	[account_id] [int] NOT NULL,
	[logged_in] [bit] NOT NULL,
	[game_id] [int] NULL,
	[channel_id] [int] NULL,
	[world_id] [int] NULL,
	[game_name] [nvarchar](32) NULL,
	[database_user] [nvarchar](32) NULL,
 CONSTRAINT [PK_account_status] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [ACCOUNTS].[banned]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [ACCOUNTS].[banned](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[account_id] [int] NOT NULL,
	[from_dt] [datetime2](7) NOT NULL,
	[to_dt] [datetime2](7) NULL,
 CONSTRAINT [PK_banned] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [ACCOUNTS].[banned_ip]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [ACCOUNTS].[banned_ip](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[ip_address] [nvarchar](50) NOT NULL,
	[from_dt] [datetime2](7) NOT NULL,
	[to_dt] [datetime2](7) NULL,
 CONSTRAINT [PK_banned_ip] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [ACCOUNTS].[banned_mac]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [ACCOUNTS].[banned_mac](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[mac_address] [nvarchar](50) NOT NULL,
	[from_dt] [datetime2](7) NOT NULL,
	[to_dt] [datetime2](7) NULL,
 CONSTRAINT [PK_banned_mac] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [ACCOUNTS].[user_role]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [ACCOUNTS].[user_role](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[account_id] [int] NOT NULL,
	[role_id] [int] NOT NULL,
	[create_dt] [datetime2](7) NOT NULL,
	[from_dt] [datetime2](7) NOT NULL,
	[to_dt] [datetime2](7) NULL,
 CONSTRAINT [PK_user_role] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [CLANS].[clan]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [CLANS].[clan](
	[clan_id] [int] IDENTITY(1,1) NOT NULL,
	[clan_name] [nvarchar](32) NOT NULL,
	[clan_leader_account_id] [int] NOT NULL,
	[app_id] [int] NULL,
	[is_active] [bit] NOT NULL,
	[medius_stats] [nvarchar](350) NULL,
	[create_dt] [datetime2](7) NOT NULL,
	[created_by] [int] NOT NULL,
	[modified_dt] [datetime2](7) NULL,
	[modified_by] [int] NULL,
 CONSTRAINT [PK_clan] PRIMARY KEY CLUSTERED 
(
	[clan_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [CLANS].[clan_invitation]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [CLANS].[clan_invitation](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[clan_id] [int] NOT NULL,
	[account_id] [int] NOT NULL,
	[response_id] [int] NULL,
	[response_msg] [nvarchar](50) NULL,
	[response_dt] [datetime2](7) NULL,
	[create_dt] [datetime2](7) NOT NULL,
	[created_by] [int] NULL,
	[modified_dt] [datetime2](7) NULL,
	[modified_by] [int] NULL,
	[is_active] [bit] NOT NULL,
	[invite_msg] [nvarchar](512) NULL,
 CONSTRAINT [PK_clan_invitation] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [CLANS].[clan_member]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [CLANS].[clan_member](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[clan_id] [int] NOT NULL,
	[account_id] [int] NOT NULL,
	[create_dt] [datetime2](7) NOT NULL,
	[modified_dt] [datetime2](7) NULL,
	[modified_by] [int] NULL,
	[is_active] [bit] NOT NULL,
 CONSTRAINT [PK_clan_member] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [CLANS].[clan_message]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [CLANS].[clan_message](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[clan_id] [int] NOT NULL,
	[message] [nvarchar](200) NOT NULL,
	[create_dt] [datetime2](7) NOT NULL,
	[created_by] [int] NOT NULL,
	[is_active] [bit] NOT NULL,
 CONSTRAINT [PK_clan_message] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [KEYS].[dim_announcements]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [KEYS].[dim_announcements](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[announcement_title] [nvarchar](50) NOT NULL,
	[announcement_body] [nvarchar](1000) NOT NULL,
	[create_dt] [datetime2](7) NOT NULL,
	[modified_dt] [datetime2](7) NULL,
	[from_dt] [datetime2](7) NOT NULL,
	[to_dt] [datetime2](7) NULL,
	[app_id] [int] NULL,
 CONSTRAINT [PK_dim_announcements] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [KEYS].[dim_app_groups]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [KEYS].[dim_app_groups](
	[group_id] [int] IDENTITY(1,1) NOT NULL,
	[group_name] [nvarchar](250) NOT NULL,
 CONSTRAINT [PK_app_groups] PRIMARY KEY CLUSTERED 
(
	[group_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [KEYS].[dim_app_ids]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [KEYS].[dim_app_ids](
	[app_id] [int] NOT NULL,
	[app_name] [nvarchar](250) NOT NULL,
	[group_id] [int] NULL,
 CONSTRAINT [PK_app_ids] PRIMARY KEY CLUSTERED 
(
	[app_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [KEYS].[dim_clan_custom_stats]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [KEYS].[dim_clan_custom_stats](
	[stat_id] [int] IDENTITY(1,1) NOT NULL,
	[stat_name] [nvarchar](100) NOT NULL,
	[default_value] [int] NOT NULL,
	[app_id] [int] NOT NULL,
 CONSTRAINT [PK_dim_clan_custom_stats] PRIMARY KEY CLUSTERED 
(
	[stat_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [KEYS].[dim_clan_stats]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [KEYS].[dim_clan_stats](
	[stat_id] [int] IDENTITY(1,1) NOT NULL,
	[stat_name] [nvarchar](100) NOT NULL,
	[default_value] [int] NOT NULL,
 CONSTRAINT [PK_dim_clan_stats] PRIMARY KEY CLUSTERED 
(
	[stat_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [KEYS].[dim_custom_stats]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [KEYS].[dim_custom_stats](
	[stat_id] [int] IDENTITY(1,1) NOT NULL,
	[stat_name] [nvarchar](100) NOT NULL,
	[default_value] [int] NOT NULL,
	[app_id] [int] NOT NULL,
 CONSTRAINT [PK_dim_custom_stats] PRIMARY KEY CLUSTERED 
(
	[stat_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [KEYS].[dim_eula]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [KEYS].[dim_eula](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[eula_title] [nvarchar](50) NOT NULL,
	[eula_body] [nvarchar](max) NOT NULL,
	[create_dt] [datetime2](7) NOT NULL,
	[modified_dt] [datetime2](7) NULL,
	[from_dt] [datetime2](7) NOT NULL,
	[to_dt] [datetime2](7) NULL,
 CONSTRAINT [PK_dim_eula] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [KEYS].[dim_stats]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [KEYS].[dim_stats](
	[stat_id] [int] IDENTITY(1,1) NOT NULL,
	[stat_name] [nvarchar](100) NOT NULL,
	[default_value] [int] NOT NULL,
 CONSTRAINT [PK_dim_stats] PRIMARY KEY CLUSTERED 
(
	[stat_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [KEYS].[roles]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [KEYS].[roles](
	[role_id] [int] IDENTITY(1,1) NOT NULL,
	[role_name] [nvarchar](50) NOT NULL,
	[create_dt] [datetime2](7) NOT NULL,
	[is_active] [bit] NOT NULL,
 CONSTRAINT [PK_roles] PRIMARY KEY CLUSTERED 
(
	[role_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [KEYS].[server_flags]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [KEYS].[server_flags](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[server_flag] [nvarchar](50) NOT NULL,
	[value] [nvarchar](100) NOT NULL,
	[from_dt] [datetime2](7) NULL,
	[to_dt] [datetime2](7) NULL,
 CONSTRAINT [PK_server_flags] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [KEYS].[server_settings]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [KEYS].[server_settings](
	[app_id] [int] NOT NULL,
	[name] [nvarchar](250) NOT NULL,
	[value] [nvarchar](max) NULL,
 CONSTRAINT [PK_server_settings] PRIMARY KEY CLUSTERED 
(
	[app_id] ASC,
	[name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [LOGS].[server_log]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [LOGS].[server_log](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[log_dt] [datetime2](7) NOT NULL,
	[account_id] [int] NULL,
	[method_name] [nvarchar](50) NULL,
	[log_title] [nvarchar](200) NOT NULL,
	[log_msg] [nvarchar](max) NOT NULL,
	[log_stacktrace] [nvarchar](max) NULL,
	[payload] [nvarchar](max) NULL,
 CONSTRAINT [PK_server_log] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [STATS].[account_custom_stat]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [STATS].[account_custom_stat](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[account_id] [int] NOT NULL,
	[stat_id] [int] NOT NULL,
	[stat_value] [int] NOT NULL,
	[modified_dt] [datetime2](7) NULL,
 CONSTRAINT [PK_account_custom_stat] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [STATS].[account_stat]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [STATS].[account_stat](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[account_id] [int] NOT NULL,
	[stat_id] [int] NOT NULL,
	[stat_value] [int] NOT NULL,
	[modified_dt] [datetime2](7) NULL,
 CONSTRAINT [PK_account_stat] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [STATS].[clan_custom_stat]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [STATS].[clan_custom_stat](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[clan_id] [int] NOT NULL,
	[stat_id] [int] NOT NULL,
	[stat_value] [int] NOT NULL,
	[modified_dt] [datetime2](7) NULL,
 CONSTRAINT [PK_clan_custom_stat] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [STATS].[clan_stat]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [STATS].[clan_stat](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[clan_id] [int] NOT NULL,
	[stat_id] [int] NOT NULL,
	[stat_value] [int] NOT NULL,
	[modified_dt] [datetime2](7) NULL,
 CONSTRAINT [PK_clan_stat] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [WORLD].[channels]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [WORLD].[channels](
	[id] [int] NOT NULL,
	[app_id] [int] NOT NULL,
	[name] [nvarchar](50) NOT NULL,
	[max_players] [int] NOT NULL,
	[generic_field_1] [int] NOT NULL,
	[generic_field_2] [int] NOT NULL,
	[generic_field_3] [int] NOT NULL,
	[generic_field_4] [int] NOT NULL,
	[generic_field_filter] [int] NOT NULL,
 CONSTRAINT [PK_channels] PRIMARY KEY CLUSTERED 
(
	[id] ASC,
	[app_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [WORLD].[game]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [WORLD].[game](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[game_id] [int] NOT NULL,
	[app_id] [int] NOT NULL,
	[min_players] [int] NOT NULL,
	[max_players] [int] NOT NULL,
	[player_count] [int] NOT NULL,
	[player_list_current] [nvarchar](250) NULL,
	[player_list_start] [nvarchar](250) NULL,
	[game_level] [int] NOT NULL,
	[player_skill_level] [int] NOT NULL,
	[game_stats] [binary](256) NOT NULL,
	[game_name] [nvarchar](64) NOT NULL,
	[rule_set] [int] NOT NULL,
	[generic_field_1] [int] NULL,
	[generic_field_2] [int] NULL,
	[generic_field_3] [int] NULL,
	[generic_field_4] [int] NULL,
	[generic_field_5] [int] NULL,
	[generic_field_6] [int] NULL,
	[generic_field_7] [int] NULL,
	[generic_field_8] [int] NULL,
	[world_status] [nvarchar](32) NOT NULL,
	[game_host_type] [nvarchar](32) NOT NULL,
	[metadata] [nvarchar](max) NULL,
	[game_create_dt] [datetime2](7) NULL,
	[game_start_dt] [datetime2](7) NULL,
	[database_user] [nvarchar](32) NULL,
 CONSTRAINT [PK_game] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [WORLD].[game_history]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [WORLD].[game_history](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[game_id] [int] NOT NULL,
	[app_id] [int] NOT NULL,
	[min_players] [int] NOT NULL,
	[max_players] [int] NOT NULL,
	[player_count] [int] NOT NULL,
	[player_list_current] [nvarchar](250) NULL,
	[player_list_start] [nvarchar](250) NULL,
	[game_level] [int] NOT NULL,
	[player_skill_level] [int] NOT NULL,
	[game_stats] [binary](256) NOT NULL,
	[game_name] [nvarchar](64) NOT NULL,
	[rule_set] [int] NOT NULL,
	[generic_field_1] [int] NULL,
	[generic_field_2] [int] NULL,
	[generic_field_3] [int] NULL,
	[generic_field_4] [int] NULL,
	[generic_field_5] [int] NULL,
	[generic_field_6] [int] NULL,
	[generic_field_7] [int] NULL,
	[generic_field_8] [int] NULL,
	[world_status] [nvarchar](32) NOT NULL,
	[game_host_type] [nvarchar](32) NOT NULL,
	[metadata] [nvarchar](max) NULL,
	[game_create_dt] [datetime2](7) NULL,
	[game_start_dt] [datetime2](7) NULL,
	[game_end_dt] [datetime2](7) NULL,
	[database_user] [nvarchar](32) NULL,
 CONSTRAINT [PK_game_history] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [WORLD].[locations]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [WORLD].[locations](
	[id] [int] NOT NULL,
	[app_id] [int] NOT NULL,
	[name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_channel] PRIMARY KEY CLUSTERED 
(
	[id] ASC,
	[app_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [ACCOUNTS].[account] ADD  CONSTRAINT [DF_account_create_dt]  DEFAULT (getutcdate()) FOR [create_dt]
GO
ALTER TABLE [ACCOUNTS].[account] ADD  CONSTRAINT [DF_account_is_active]  DEFAULT ((1)) FOR [is_active]
GO
ALTER TABLE [ACCOUNTS].[account_friend] ADD  CONSTRAINT [DF_account_friend_create_dt]  DEFAULT (getutcdate()) FOR [create_dt]
GO
ALTER TABLE [ACCOUNTS].[account_ignored] ADD  CONSTRAINT [DF_account_ignored_create_dt]  DEFAULT (getutcdate()) FOR [create_dt]
GO
ALTER TABLE [ACCOUNTS].[account_status] ADD  CONSTRAINT [DF_account_status_app_id]  DEFAULT ((0)) FOR [app_id]
GO
ALTER TABLE [ACCOUNTS].[account_status] ADD  CONSTRAINT [DF_account_status_logged_in]  DEFAULT ((0)) FOR [logged_in]
GO
ALTER TABLE [ACCOUNTS].[banned] ADD  CONSTRAINT [DF_banned_from_dt]  DEFAULT (getutcdate()) FOR [from_dt]
GO
ALTER TABLE [ACCOUNTS].[banned_ip] ADD  CONSTRAINT [DF_banned_ip_from_dt]  DEFAULT (getutcdate()) FOR [from_dt]
GO
ALTER TABLE [ACCOUNTS].[banned_mac] ADD  CONSTRAINT [DF_banned_mac_from_dt]  DEFAULT (getutcdate()) FOR [from_dt]
GO
ALTER TABLE [ACCOUNTS].[user_role] ADD  CONSTRAINT [DF_user_role_create_dt]  DEFAULT (getutcdate()) FOR [create_dt]
GO
ALTER TABLE [ACCOUNTS].[user_role] ADD  CONSTRAINT [DF_user_role_from_dt]  DEFAULT (getutcdate()) FOR [from_dt]
GO
ALTER TABLE [CLANS].[clan] ADD  CONSTRAINT [DF_clan_is_active]  DEFAULT ((1)) FOR [is_active]
GO
ALTER TABLE [CLANS].[clan] ADD  CONSTRAINT [DF_clan_create_dt]  DEFAULT (getutcdate()) FOR [create_dt]
GO
ALTER TABLE [CLANS].[clan_invitation] ADD  CONSTRAINT [DF_clan_invitation_create_dt]  DEFAULT (getutcdate()) FOR [create_dt]
GO
ALTER TABLE [CLANS].[clan_invitation] ADD  CONSTRAINT [DF_clan_invitation_is_active]  DEFAULT ((1)) FOR [is_active]
GO
ALTER TABLE [CLANS].[clan_member] ADD  CONSTRAINT [DF_clan_member_create_dt]  DEFAULT (getutcdate()) FOR [create_dt]
GO
ALTER TABLE [CLANS].[clan_member] ADD  CONSTRAINT [DF_clan_member_is_active]  DEFAULT ((1)) FOR [is_active]
GO
ALTER TABLE [CLANS].[clan_message] ADD  CONSTRAINT [DF_clan_message_create_dt]  DEFAULT (getutcdate()) FOR [create_dt]
GO
ALTER TABLE [CLANS].[clan_message] ADD  CONSTRAINT [DF_clan_message_is_active]  DEFAULT ((1)) FOR [is_active]
GO
ALTER TABLE [KEYS].[dim_announcements] ADD  CONSTRAINT [DF_dim_announcements_create_dt]  DEFAULT (getutcdate()) FOR [create_dt]
GO
ALTER TABLE [KEYS].[dim_announcements] ADD  CONSTRAINT [DF_dim_announcements_from_dt]  DEFAULT (getutcdate()) FOR [from_dt]
GO
ALTER TABLE [KEYS].[dim_clan_custom_stats] ADD  CONSTRAINT [DF_dim_clan_custom_stats_default_value]  DEFAULT ((0)) FOR [default_value]
GO
ALTER TABLE [KEYS].[dim_clan_custom_stats] ADD  CONSTRAINT [DF_dim_clan_custom_stats_app_id]  DEFAULT ((0)) FOR [app_id]
GO
ALTER TABLE [KEYS].[dim_clan_stats] ADD  CONSTRAINT [DF_dim_clan_stats_default_value]  DEFAULT ((0)) FOR [default_value]
GO
ALTER TABLE [KEYS].[dim_custom_stats] ADD  CONSTRAINT [DF_dim_custom_stats_default_value]  DEFAULT ((0)) FOR [default_value]
GO
ALTER TABLE [KEYS].[dim_custom_stats] ADD  CONSTRAINT [DF_dim_custom_stats_app_id]  DEFAULT ((0)) FOR [app_id]
GO
ALTER TABLE [KEYS].[dim_eula] ADD  CONSTRAINT [DF_dim_eula_create_dt]  DEFAULT (getutcdate()) FOR [create_dt]
GO
ALTER TABLE [KEYS].[dim_eula] ADD  CONSTRAINT [DF_dim_eula_from_dt]  DEFAULT (getutcdate()) FOR [from_dt]
GO
ALTER TABLE [KEYS].[dim_stats] ADD  CONSTRAINT [DF_dim_stats_default_value]  DEFAULT ((0)) FOR [default_value]
GO
ALTER TABLE [KEYS].[roles] ADD  CONSTRAINT [DF_roles_create_dt]  DEFAULT (getutcdate()) FOR [create_dt]
GO
ALTER TABLE [KEYS].[roles] ADD  CONSTRAINT [DF_roles_is_active]  DEFAULT ((1)) FOR [is_active]
GO
ALTER TABLE [LOGS].[server_log] ADD  CONSTRAINT [DF_server_log_log_dt]  DEFAULT (getutcdate()) FOR [log_dt]
GO
ALTER TABLE [WORLD].[channels] ADD  CONSTRAINT [DF_channels_max_players]  DEFAULT ((256)) FOR [max_players]
GO
ALTER TABLE [WORLD].[game] ADD  CONSTRAINT [DF_game_game_create_dt]  DEFAULT (getutcdate()) FOR [game_create_dt]
GO
ALTER TABLE [ACCOUNTS].[account_friend]  WITH CHECK ADD  CONSTRAINT [FK_account_friend_account] FOREIGN KEY([account_id])
REFERENCES [ACCOUNTS].[account] ([account_id])
GO
ALTER TABLE [ACCOUNTS].[account_friend] CHECK CONSTRAINT [FK_account_friend_account]
GO
ALTER TABLE [ACCOUNTS].[account_ignored]  WITH CHECK ADD  CONSTRAINT [FK_account_ignored_account] FOREIGN KEY([account_id])
REFERENCES [ACCOUNTS].[account] ([account_id])
GO
ALTER TABLE [ACCOUNTS].[account_ignored] CHECK CONSTRAINT [FK_account_ignored_account]
GO
ALTER TABLE [CLANS].[clan]  WITH CHECK ADD  CONSTRAINT [FK_clan_account] FOREIGN KEY([clan_leader_account_id])
REFERENCES [ACCOUNTS].[account] ([account_id])
GO
ALTER TABLE [CLANS].[clan] CHECK CONSTRAINT [FK_clan_account]
GO
ALTER TABLE [CLANS].[clan_invitation]  WITH CHECK ADD  CONSTRAINT [FK_clan_invitation_account] FOREIGN KEY([account_id])
REFERENCES [ACCOUNTS].[account] ([account_id])
GO
ALTER TABLE [CLANS].[clan_invitation] CHECK CONSTRAINT [FK_clan_invitation_account]
GO
ALTER TABLE [CLANS].[clan_invitation]  WITH CHECK ADD  CONSTRAINT [FK_clan_invitation_clan] FOREIGN KEY([clan_id])
REFERENCES [CLANS].[clan] ([clan_id])
GO
ALTER TABLE [CLANS].[clan_invitation] CHECK CONSTRAINT [FK_clan_invitation_clan]
GO
ALTER TABLE [CLANS].[clan_member]  WITH CHECK ADD  CONSTRAINT [FK_clan_member_account] FOREIGN KEY([account_id])
REFERENCES [ACCOUNTS].[account] ([account_id])
GO
ALTER TABLE [CLANS].[clan_member] CHECK CONSTRAINT [FK_clan_member_account]
GO
ALTER TABLE [CLANS].[clan_member]  WITH CHECK ADD  CONSTRAINT [FK_clan_member_clan] FOREIGN KEY([clan_id])
REFERENCES [CLANS].[clan] ([clan_id])
GO
ALTER TABLE [CLANS].[clan_member] CHECK CONSTRAINT [FK_clan_member_clan]
GO
ALTER TABLE [CLANS].[clan_message]  WITH CHECK ADD  CONSTRAINT [FK_clan_message_clan] FOREIGN KEY([clan_id])
REFERENCES [CLANS].[clan] ([clan_id])
GO
ALTER TABLE [CLANS].[clan_message] CHECK CONSTRAINT [FK_clan_message_clan]
GO
ALTER TABLE [STATS].[account_custom_stat]  WITH CHECK ADD  CONSTRAINT [FK_account_custom_stat_account] FOREIGN KEY([account_id])
REFERENCES [ACCOUNTS].[account] ([account_id])
GO
ALTER TABLE [STATS].[account_custom_stat] CHECK CONSTRAINT [FK_account_custom_stat_account]
GO
ALTER TABLE [STATS].[account_custom_stat]  WITH CHECK ADD  CONSTRAINT [FK_account_custom_stat_dim_stats] FOREIGN KEY([stat_id])
REFERENCES [KEYS].[dim_custom_stats] ([stat_id])
GO
ALTER TABLE [STATS].[account_custom_stat] CHECK CONSTRAINT [FK_account_custom_stat_dim_stats]
GO
ALTER TABLE [STATS].[account_stat]  WITH CHECK ADD  CONSTRAINT [FK_account_stat_account] FOREIGN KEY([account_id])
REFERENCES [ACCOUNTS].[account] ([account_id])
GO
ALTER TABLE [STATS].[account_stat] CHECK CONSTRAINT [FK_account_stat_account]
GO
ALTER TABLE [STATS].[account_stat]  WITH CHECK ADD  CONSTRAINT [FK_account_stat_dim_stats] FOREIGN KEY([stat_id])
REFERENCES [KEYS].[dim_stats] ([stat_id])
GO
ALTER TABLE [STATS].[account_stat] CHECK CONSTRAINT [FK_account_stat_dim_stats]
GO
ALTER TABLE [STATS].[clan_custom_stat]  WITH CHECK ADD  CONSTRAINT [FK_clan_custom_stat_clan] FOREIGN KEY([clan_id])
REFERENCES [CLANS].[clan] ([clan_id])
GO
ALTER TABLE [STATS].[clan_custom_stat] CHECK CONSTRAINT [FK_clan_custom_stat_clan]
GO
ALTER TABLE [STATS].[clan_custom_stat]  WITH CHECK ADD  CONSTRAINT [FK_clan_custom_stat_dim_stats] FOREIGN KEY([stat_id])
REFERENCES [KEYS].[dim_custom_stats] ([stat_id])
GO
ALTER TABLE [STATS].[clan_custom_stat] CHECK CONSTRAINT [FK_clan_custom_stat_dim_stats]
GO
ALTER TABLE [STATS].[clan_stat]  WITH CHECK ADD  CONSTRAINT [FK_clan_stat_clan] FOREIGN KEY([clan_id])
REFERENCES [CLANS].[clan] ([clan_id])
GO
ALTER TABLE [STATS].[clan_stat] CHECK CONSTRAINT [FK_clan_stat_clan]
GO
ALTER TABLE [STATS].[clan_stat]  WITH CHECK ADD  CONSTRAINT [FK_clan_stat_dim_stats] FOREIGN KEY([stat_id])
REFERENCES [KEYS].[dim_stats] ([stat_id])
GO
ALTER TABLE [STATS].[clan_stat] CHECK CONSTRAINT [FK_clan_stat_dim_stats]
GO
/****** Object:  StoredProcedure [dbo].[SyncAccountCustomStats]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Connor Williams
-- Create date: 03/01/2022
-- Description:	Inserts custom stat values for every account if it is missing a stat. I.e when a new custom stat is made.
-- =============================================
CREATE PROCEDURE [dbo].[SyncAccountCustomStats]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT INTO STATS.account_custom_stat (account_id, stat_id, stat_value, modified_dt)
	SELECT A.account_id, DCS.stat_id, DCS.default_value as stat_value, GETUTCDATE() as modified_dt
	FROM ACCOUNTS.account a
	INNER JOIN KEYS.dim_custom_stats DCS ON (DCS.app_id = a.app_id or DCS.app_id = 0)
	LEFT JOIN STATS.account_custom_stat ACS
		ON ACS.account_id = a.account_id
		AND ACS.stat_id = DCS.stat_id
	WHERE ACS.stat_value IS NULL
END
GO
/****** Object:  StoredProcedure [dbo].[SyncClanCustomStats]    Script Date: 8/9/2022 8:53:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Connor Williams
-- Create date: 03/01/2022
-- Description:	Inserts custom stat values for every clan if it is missing a stat. I.e when a new custom stat is made.
-- =============================================
CREATE PROCEDURE [dbo].[SyncClanCustomStats]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT INTO STATS.clan_custom_stat(clan_id, stat_id, stat_value, modified_dt)
	SELECT C.clan_id, DCS.stat_id, DCS.default_value as stat_value, GETUTCDATE() as modified_dt
	FROM CLANS.clan C
	INNER JOIN KEYS.dim_clan_custom_stats DCS ON (DCS.app_id = C.app_id or DCS.app_id = 0)
	LEFT JOIN STATS.clan_custom_stat CCS
		ON CCS.clan_id = c.clan_id
		AND CCS.stat_id = DCS.stat_id
	WHERE CCS.stat_value IS NULL
END
GO

/****** INITIALIZE DATABASE TABLES ******/
DECLARE @i AS INT = 1

/* accounts stats need exactly 100 rows with stat_id in range [1,100] */
WHILE(@i <= 100)
BEGIN
    INSERT INTO [KEYS].[dim_stats] VALUES('STAT', 0)
    SET @i += 1
END

/* clan stats need exactly 100 rows with stat_id in range [1,100] */
SET @i = 1
WHILE(@i <= 100)
BEGIN
    INSERT INTO [KEYS].[dim_clan_stats] VALUES('STAT', 0)
    SET @i += 1
END

/* middleware has a few roles it relies on */
BEGIN
	IF NOT EXISTS (SELECT * FROM [KEYS].[roles] where role_name = 'database')
	BEGIN
		INSERT INTO [KEYS].[roles] VALUES('database', GETDATE(), 1)
	END
END
BEGIN
	IF NOT EXISTS (SELECT * FROM [KEYS].[roles] where role_name = 'admin')
	BEGIN
		INSERT INTO [KEYS].[roles] VALUES('admin', GETDATE(), 1)
	END
END
BEGIN
	IF NOT EXISTS (SELECT * FROM [KEYS].[roles] where role_name = 'moderator')
	BEGIN
		INSERT INTO [KEYS].[roles] VALUES('moderator', GETDATE(), 1)
	END
END
BEGIN
	IF NOT EXISTS (SELECT * FROM [KEYS].[roles] where role_name = 'discord_bot')
	BEGIN
		INSERT INTO [KEYS].[roles] VALUES('discord_bot', GETDATE(), 1)
	END
END
BEGIN
	IF NOT EXISTS (SELECT * FROM [KEYS].[roles] where role_name = 'stats_bot')
	BEGIN
		INSERT INTO [KEYS].[roles] VALUES('stats_bot', GETDATE(), 1)
	END
END
