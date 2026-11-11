using System;
using System.Collections.Generic;

namespace Horizon.Database.Helpers
{
    /// <summary>
    /// Classifies an account's MachineId as a real PS2, an emulator, the default id, or unknown.
    /// The prefix list is the set of known PS2 network adapter OUIs.
    /// </summary>
    public static class AltChecker
    {
        private const string EmuPrefix = "00-04-1F-82";

        private static readonly HashSet<string> Ps2MacPrefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "FC-F1-52",
            "FC-0F-E6",
            "F8-D0-AC",
            "F8-46-1C",
            "F0-BF-97",
            "E0-63-E5",
            "D8-D4-3C",
            "D4-38-9C",
            "D0-51-62",
            "CC-98-8B",
            "C8-63-F1",
            "C4-3A-BE",
            "BC-6E-64",
            "BC-60-A7",
            "B8-F9-34",
            "B4-52-7E",
            "B4-52-7D",
            "AC-9B-0A",
            "A8-E3-EE",
            "A0-E4-53",
            "9C-5C-F9",
            "94-DB-56",
            "94-CE-2C",
            "90-C1-15",
            "8C-64-22",
            "84-E0-F4",
            "84-C7-EA",
            "84-8E-DF",
            "84-00-D2",
            "78-C8-81",
            "78-84-3C",
            "70-9E-29",
            "70-26-05",
            "6C-B2-27",
            "6C-23-B9",
            "6C-0E-0D",
            "68-76-4F",
            "5C-B5-24",
            "58-48-22",
            "58-17-0C",
            "54-53-ED",
            "54-42-49",
            "4C-21-D0",
            "44-D4-E0",
            "44-74-6C",
            "40-B8-37",
            "40-40-A7",
            "40-2B-A1",
            "3C-07-71",
            "3C-01-EF",
            "38-78-62",
            "38-18-4C",
            "30-F9-ED",
            "30-A8-DB",
            "30-75-12",
            "30-39-26",
            "30-17-C8",
            "2C-CC-44",
            "2C-97-ED",
            "28-3F-69",
            "28-0D-FC",
            "24-21-AB",
            "20-54-76",
            "1C-7B-21",
            "18-00-2D",
            "10-4F-A8",
            "0C-FE-45",
            "08-00-46",
            "04-5D-4B",
            "00-EB-2D",
            "00-D9-D1",
            "00-25-E7",
            "00-24-EF",
            "00-24-BE",
            "00-24-8D",
            "00-23-F1",
            "00-23-45",
            "00-22-A6",
            "00-22-98",
            "00-21-9E",
            "00-1F-E4",
            "00-1F-A7",
            "00-1E-DC",
            "00-1E-45",
            "00-1D-BA",
            "00-1D-28",
            "00-1D-0D",
            "00-1C-A4",
            "00-1B-59",
            "00-1A-80",
            "00-1A-75",
            "00-19-C5",
            "00-19-63",
            "00-18-13",
            "00-16-B8",
            "00-16-20",
            "00-15-C1",
            "00-13-A9",
            "00-13-15",
            "00-12-EE",
            "00-0F-DE",
            "00-0E-07",
            "00-0A-D9",
            "00-04-1F",
            "00-01-4A",
            "00-00-95"
        };

        public static string EmuOrPs2(string machineId)
        {
            if (machineId == null)
                return "N/A";
            if (machineId.Length >= 8 && Ps2MacPrefixes.Contains(machineId.Substring(0, 8)) && !machineId.StartsWith(EmuPrefix, StringComparison.OrdinalIgnoreCase))
                return "PS2";
            if (machineId == "1")
                return "DEFAULT";
            return "EMU";
        }
    }
}
