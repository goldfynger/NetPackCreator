using System;

namespace NetPackCreator.Controllers
{
    /// <summary></summary>
    internal sealed class DestinationGroup
    {
        /// <summary></summary>
        private readonly IPhysicalAddressController _physicalAddressController;
        /// <summary></summary>
        private readonly IDestinationIpAddressController _destinationIpAddressController;
        /// <summary></summary>
        private readonly IPortController _portController;

        /// <summary></summary>
        /// <param name="physicalAddressController"></param>
        /// <param name="sourceIpAddressController"></param>
        /// <param name="portController"></param>
        public DestinationGroup(IPhysicalAddressController physicalAddressController, IDestinationIpAddressController destinationIpAddressController, IPortController portController)
        {
            if (physicalAddressController == null) throw new ArgumentNullException("physicalAddressController");
            if (destinationIpAddressController == null) throw new ArgumentNullException("destinationIpAddressController");
            if (portController == null) throw new ArgumentNullException("portController");

            this._physicalAddressController = physicalAddressController;
            this._destinationIpAddressController = destinationIpAddressController;
            this._portController = portController;
        }

        /// <summary></summary>
        public IPhysicalAddressController PhysicalAddressController { get { return this._physicalAddressController; } }

        /// <summary></summary>
        public IDestinationIpAddressController DestinationIpAddressController { get { return this._destinationIpAddressController; } }

        /// <summary></summary>
        public IPortController PortController { get { return this._portController; } }
    }
}