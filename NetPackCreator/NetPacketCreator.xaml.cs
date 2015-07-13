using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Windows;

using NetPackCreator.Controllers;
using NetPackCreator.Controls;

namespace NetPackCreator
{
    /// <summary></summary>
    public partial class NetPacketCreator
    {
        #region Regex templates

        /// <summary></summary>
        private const string FullMacRegexTemplate = @"^(?:(?:[0-9a-fA-F]{2}){0,}|(?:[0-9a-fA-F]{2}\:){0,}|(?:[0-9a-fA-F]{2}\-){0,})[0-9a-fA-F]{2}$";
        /// <summary></summary>
        private const string FullIpRegexTemplate = @"^(?:
            (?: (?:(?:0|(?:[1-9a-fA-F][0-9a-fA-F]{0,3}))\:){7}                                                     (?:0|(?:[1-9a-fA-F][0-9a-fA-F]{0,3})) )       |
            (?: (\:{2}(?:(?:[1-9a-fA-F][0-9a-fA-F]{0,3})\:){0,6}                                                   (?:0|(?:[1-9a-fA-F][0-9a-fA-F]{0,3})) ) | \:) |
            (?: (?:(?:0|(?:[1-9a-fA-F][0-9a-fA-F]{0,3}))\:){1,7}                                                                                             \:) |
            
            (?: (?:(?:0|(?:[1-9a-fA-F][0-9a-fA-F]{0,3}))\:)    \: (?:(?:0|(?:[1-9a-fA-F][0-9a-fA-F]{0,3}))\:){0,5} (?:0|(?:[1-9a-fA-F][0-9a-fA-F]{0,3})) )       |
            (?: (?:(?:0|(?:[1-9a-fA-F][0-9a-fA-F]{0,3}))\:){2} \: (?:(?:0|(?:[1-9a-fA-F][0-9a-fA-F]{0,3}))\:){0,4} (?:0|(?:[1-9a-fA-F][0-9a-fA-F]{0,3})) )       |
            (?: (?:(?:0|(?:[1-9a-fA-F][0-9a-fA-F]{0,3}))\:){3} \: (?:(?:0|(?:[1-9a-fA-F][0-9a-fA-F]{0,3}))\:){0,3} (?:0|(?:[1-9a-fA-F][0-9a-fA-F]{0,3})) )       |
            (?: (?:(?:0|(?:[1-9a-fA-F][0-9a-fA-F]{0,3}))\:){4} \: (?:(?:0|(?:[1-9a-fA-F][0-9a-fA-F]{0,3}))\:){0,2} (?:0|(?:[1-9a-fA-F][0-9a-fA-F]{0,3})) )       |
            (?: (?:(?:0|(?:[1-9a-fA-F][0-9a-fA-F]{0,3}))\:){5} \: (?:(?:0|(?:[1-9a-fA-F][0-9a-fA-F]{0,3}))\:){0,1} (?:0|(?:[1-9a-fA-F][0-9a-fA-F]{0,3})) )       |
            (?: (?:(?:0|(?:[1-9a-fA-F][0-9a-fA-F]{0,3}))\:){6} \:                                                  (?:0|(?:[1-9a-fA-F][0-9a-fA-F]{0,3})) )       |
            
            (?: [fF][eE]80\: (?:(?:\:(?:0|(?:[1-9a-fA-F][0-9a-fA-F]{0,3}))){1,4})                                                   %(?:[0-9a-zA-Z]{1,}) )       |
            
            (?: (?: (?:\:{2}) | (?:\:{2}[fF]{4}\:(0\:)?) | (?:(?:(?:0|(?:[1-9a-fA-F][0-9a-fA-F]{0,3}))\:){1,4}\:) )?
                (?: (?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])\.){3}                    (?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9]) )
            )$";

        #endregion

        /// <summary></summary>
        private Manager _manager;

        /// <summary></summary>
        public NetPacketCreator()
        {
            InitializeComponent();

            var interfaceHelper = new ComboBoxSelectionChangedHelper();
            this.cbInterfaces.SelectionChanged += (sender, e) => interfaceHelper.OnSelectionChanged(sender, e); // helper.OnSelectionChanged() должен вызываться только по событию, см. описание класса.

            var interfaceController = new InterfaceController(this.cbInterfaces, interfaceHelper);

            var fullMacRegex = new Regex(FullMacRegexTemplate, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
            Func<string, PhysicalAddress> parsePhysicalAddressFunc = s => PhysicalAddress.Parse(s.ToUpper().Replace(":", ""));
            Func<string, bool> validatePhysicalAddressFunc = s => fullMacRegex.IsMatch(s);
            Func<PhysicalAddress, string> physicalAddressToStringFunc = a => string.Join(":", (from b in a.GetAddressBytes() select b.ToString("X2")).ToArray());

            var fullIpRegex = new Regex(FullIpRegexTemplate, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
            Func<string, IPAddress> parseIPAddressFunc = s => IPAddress.Parse(s);
            Func<string, bool> validateIPAddressFunc = s => fullIpRegex.IsMatch(s);

            Func<string, ushort?> parsePortFunc = s => ushort.Parse(s);
            Func<string, bool> validatePortFunc = s =>
            {
                ushort value;
                return (ushort.TryParse(s, out value) && value > 0);
            };

            var sourceIpHelper = new ComboBoxSelectionChangedHelper();
            this.cbSourceIP.SelectionChanged += (sender, e) => sourceIpHelper.OnSelectionChanged(sender, e);
            
            var sourceMacHelper = new TextBoxChangeCanceledHelper();
            this.ctbSourceMAC.AddHandler(Masking.ChangeCanceledEvent, new RoutedEventHandler((sender, e) => sourceMacHelper.OnChangeCanceled(sender)));

            var sourcePortHelper = new TextBoxChangeCanceledHelper();
            this.ctbSourcePort.AddHandler(IntegerRange.ChangeCanceledEvent, new RoutedEventHandler((sender, e) => sourcePortHelper.OnChangeCanceled(sender)));

            var destinationIpHelper = new TextBoxChangeCanceledHelper();
            this.ctbDestinationIP.AddHandler(Masking.ChangeCanceledEvent, new RoutedEventHandler((sender, e) => destinationIpHelper.OnChangeCanceled(sender)));

            var destinationMacHelper = new TextBoxChangeCanceledHelper();
            this.ctbDestinationMAC.AddHandler(Masking.ChangeCanceledEvent, new RoutedEventHandler((sender, e) => destinationMacHelper.OnChangeCanceled(sender)));

            var destinationPortHelper = new TextBoxChangeCanceledHelper();
            this.ctbDestinationPort.AddHandler(IntegerRange.ChangeCanceledEvent, new RoutedEventHandler((sender, e) => destinationPortHelper.OnChangeCanceled(sender)));

            var sourceMacAddressController = new PhysicalAddressController(this.ctbSourceMAC, sourceMacHelper, parsePhysicalAddressFunc, validatePhysicalAddressFunc, physicalAddressToStringFunc);
            var sourceIpAddressController = new SourceIpAddressController(this.cbSourceIP, sourceIpHelper, null, null);
            var sourcePortController = new PortController(this.ctbSourcePort, sourcePortHelper, parsePortFunc, validatePortFunc, null);

            var destinationMacAddressController = new PhysicalAddressController(this.ctbDestinationMAC, destinationMacHelper, parsePhysicalAddressFunc, validatePhysicalAddressFunc, physicalAddressToStringFunc);
            var destinationIpAddressController = new DestinationIpAddressController(this.ctbDestinationIP, destinationIpHelper, parseIPAddressFunc, validateIPAddressFunc, null);
            var destinationPortController = new PortController(this.ctbDestinationPort, destinationPortHelper, parsePortFunc, validatePortFunc, null);

            var connectionModeHelper = new ComboBoxSelectionChangedHelper();
            this.cbConnectionMode.SelectionChanged += (sender, e) => connectionModeHelper.OnSelectionChanged(sender, e);

            var exchangeConnectionModeController = new ConnectionModeController(this.cbConnectionMode, connectionModeHelper);
            var exchangeConnectionStateController = new ConnectionStateController(this.bConnection);
            var exchangeSendCommandController = new SendCommandController(this.bSend);
            var exchangeWriteController = new WriteController();
            var exchangeViewController = new ExchangeViewController();

            var sourceGroup = new SourceGroup(interfaceController, sourceMacAddressController, sourceIpAddressController, sourcePortController);
            var destinationGroup = new DestinationGroup(destinationMacAddressController, destinationIpAddressController, destinationPortController);
            var exchangeGroup = new ExchangeGroup(exchangeConnectionModeController, exchangeConnectionStateController, exchangeSendCommandController, exchangeWriteController, exchangeViewController);

            this._manager = new Manager(sourceGroup, destinationGroup, exchangeGroup);
        }
    }
}