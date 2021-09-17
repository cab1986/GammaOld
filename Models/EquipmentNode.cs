﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;

namespace Gamma.Models
{
    public class EquipmentNode
    {
        public Guid EquipmentNodeID { get; set; }
        public string EquipmentNodeName { get; set; }
        public Guid? EquipmentNodeMasterID { get; set; }
    }
}
