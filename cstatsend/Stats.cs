using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Diagnostics;
using Microsoft.VisualBasic;

namespace cstatsend
{
    class Stats
    {
        public string uid { get; set; }
        public string key { get; set; }

        public string hostname { get
            {
                return Dns.GetHostName();
            }
        }

        public DiskCollection disk { get
            {
                DriveInfo[] di = DriveInfo.GetDrives();
                DiskCollection dc = new DiskCollection();

                foreach(DriveInfo i in di)
                {
                    Disk d = new Disk
                    {
                        fs = i.DriveFormat,
                        mount = i.Name,
                        avail = ToMByte(i.AvailableFreeSpace),
                        used = ToMByte(i.TotalSize - i.AvailableFreeSpace),
                        total = ToMByte(i.TotalSize),
                        type = i.DriveType.ToString()
                    };

                    dc.single.Add(d);
                }

                dc.total.avail = dc.single.Sum(d => d.avail);
                dc.total.total = dc.single.Sum(d => d.total);
                dc.total.used = dc.single.Sum(d => d.used);

                return dc;
            }
        }

        public List<IP> ips { get
            {
                List<IP> ips = new List<IP>();

                string strHostName = Dns.GetHostName();

                // Find host by name
                IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);

                // Enumerate IP addresses
                foreach (IPAddress ipaddress in iphostentry.AddressList)
                {
                    ips.Add(new IP()
                    {
                        ip = ipaddress.ToString(),
                        hostname = strHostName
                    });
                }

                return ips;
            }
        }

        public RAM ram { get
            {
                Microsoft.VisualBasic.Devices.ComputerInfo ci = new Microsoft.VisualBasic.Devices.ComputerInfo();
                RAM ram = new RAM()
                {
                    free = ToMByte((long)ci.AvailablePhysicalMemory),
                    total = ToMByte((long)ci.TotalPhysicalMemory),
                    used = ToMByte((long)ci.TotalPhysicalMemory - (long)ci.AvailablePhysicalMemory),
                    bufcac = 0
                };

                return ram;
            }
        }

        public CPU uplo { get
            {
                PerformanceCounter pc = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
                PerformanceCounter up = new PerformanceCounter("System", "System Up Time");
                decimal usage = (decimal)(pc.NextValue() * 0.01);

                up.NextValue();

                TimeSpan uptime = TimeSpan.FromSeconds(up.NextValue());

                CPU cpu = new CPU()
                {
                    load1 = usage,
                    load15 = usage,
                    load5 = usage,
                    uptime = CPU.FriendlyUptime(uptime)
                };

                return cpu;
            }
        }

        public static long ToMByte(long bytes)
        {
            return bytes / 1024 / 1024;
        }
    }

    class DiskCollection
    {
        public List<Disk> single { get; set; }
        public DiskStats total { get; set; }

        public DiskCollection()
        {
            single = new List<Disk>();
            total = new DiskStats();
        }
    }

    class Disk
    {
        public string fs { get; set; }
        public string mount { get; set; }
        public long avail { get; set; }
        public long used { get; set; }
        public long total { get; set; }
        public string type { get; set; }
    }

    class DiskStats
    {
        public long total { get; set; }
        public long avail { get; set; }
        public long used { get; set; }
    }

    class IP
    {
        public string ip { get; set; }
        public string hostname { get; set; }
    }

    class RAM
    {
        public long total { get; set; }
        public long used { get; set; }
        public long bufcac { get; set; }
        public long free { get; set; }
    }

    class CPU
    {
        public decimal load1 { get; set; }
        public decimal load15 { get; set; }
        public decimal load5 { get; set; }
        public string uptime { get; set; }

        public static string FriendlyUptime(TimeSpan uptime)
        {
            if (uptime.TotalDays > 1)
                return string.Format("{0} days", uptime.Days);
            if (uptime.TotalHours > 1)
                return string.Format("{0} hours", uptime.Hours);

            return string.Format("{0} minutes", uptime.Minutes);
        }
    }
}
