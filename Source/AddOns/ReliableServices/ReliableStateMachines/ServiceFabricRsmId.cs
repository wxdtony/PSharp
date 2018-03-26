﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PSharp.ReliableServices
{
    /// <summary>
    /// RsmId implementation for Service Fabric
    /// </summary>
    [DataContract]
    internal class ServiceFabricRsmId : IRsmId
    {
        /// <summary>
        /// Unique value
        /// </summary>
        [DataMember]
        long Value;

        /// <summary>
        /// Name
        /// </summary>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Partition hosting the RSM
        /// </summary>
        public string PartitionName { get; private set; }

        /// <summary>
        /// Creates a new ServiceFabricRsmId
        /// </summary>
        /// <param name="value">Unique value</param>
        /// <param name="name">Name</param>
        /// <param name="partitionName">Partition</param>
        internal ServiceFabricRsmId(long value, string name, string partitionName)
        {
            this.Value = value;
            this.Name = name;
            this.PartitionName = partitionName;
        }

        public int CompareTo(object obj)
        {
            var id = obj as ServiceFabricRsmId;
            return Value.CompareTo(id.Value);
        }
    }
}
