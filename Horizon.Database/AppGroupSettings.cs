using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class ServerSettings
{
    public string EnableEncryption { get; set; }
    public string CreateAccountOnNotFound { get; set; }
    public int ClientLongTimeoutSeconds { get; set; }
    public int ClientTimeoutSeconds { get; set; }
    public int DmeTimeoutSeconds { get; set; }
    public int KeepAliveGracePeriodSeconds { get; set; }
    public int GameTimeoutSeconds { get; set; }
}

public class TextBody
{
    public string Title { get; set; }
    public string Body { get; set; }
}

public class AppGroup
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class App
{
    public string Name { get; set; }
    public int Id { get; set; }
    public string GroupName { get; set; }
    public List<TextBody> Announcements { get; set; }
    public ServerSettings ServerSettings { get; set; }
}

public class Location
{
    public int Id { get; set; }
    public int AppId { get; set; }
    public string Name { get; set; }
}

public class Channel
{
    public int Id { get; set; }
    public int AppId { get; set; }
    public string Name { get; set; }
    public int MaxPlayers { get; set; }
    public int GenericField1 { get; set; }
    public int GenericField2 { get; set; }
    public int GenericField3 { get; set; }
    public int GenericField4 { get; set; }
    public int GenericFieldFilter { get; set; }
}

public class AppGroupSettings
{
    public List<AppGroup> AppGroups { get; set; }
    public List<App> Apps { get; set; }
    public TextBody Eula { get; set; }
    public List<Location> Locations { get; set; }
    public List<Channel> Channels { get; set; }
}
