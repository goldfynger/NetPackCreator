using System.Net;
using System.Net.NetworkInformation;

namespace NetPackCreator
{
    /// <summary></summary>
    public partial class NetPacketCreator
    {
        public NetPacketCreator()
        {
            InitializeComponent();

            var mac = new PhysicalAddress(new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9});

            var mac1 = new PhysicalAddress(new byte[] { 1 });

            var mac2 = PhysicalAddress.Parse("123456");

            var ip = new IPAddress(new byte[] { 1, 2, 3, 4, 0, 0, 0, 0, 3, 4, 5, 6, 0, 0, 0, 0 });

            var v4Ip = IPAddress.Parse("0.1.2.3");
            var zIp = IPAddress.Parse("::");
            var oneIp = IPAddress.Parse("::1");
            var oldIp = IPAddress.Parse("::1.2.3.4");
            var newIp = IPAddress.Parse("::ffff:255.2.3.4");
            var simple1Ip = IPAddress.Parse("::feee");
            var simple2Ip = IPAddress.Parse("feee::");
            var simple3Ip = IPAddress.Parse("fee::feee");
        }
    }
}