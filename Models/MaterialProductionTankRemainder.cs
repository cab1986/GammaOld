﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Attributes;
using Gamma.Entities;
using Gamma.Interfaces;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Collections.Generic;

namespace Gamma.Models
{
    public class MaterialProductionTankRemainder : ViewModelBase, ICheckedAccess
    {
        public MaterialProductionTankRemainder(int docMaterialTankID, string name, int volume, int? docMaterialProductionTypeID)
        {
            GammaBase = DB.GammaDbWithNoCheckConnection;
            DocMaterialTankID = docMaterialTankID;
            Volume = volume;
            Name = name + " V="+Volume+"м3";
            if (docMaterialProductionTypeID == (int)DocMaterialProductionTypes.InToCompositionTank)
            {
                IsNotRemainderAtEnd = true;
                Concentration = 100;
                Level = 100;
            };
            var docMaterialTank = WorkSession.DocMaterialTanks.Where(t => t.DocMaterialTankID == docMaterialTankID).FirstOrDefault();
            if (docMaterialTank != null)
            {
                DocMaterialTankGroupID = docMaterialTank.DocMaterialTankGroupID;
                var groups = WorkSession.DocMaterialTankGroups.Where(t => t.DocMaterialTankGroupID == docMaterialTank.DocMaterialTankGroups.DocMaterialTankGroupID).ToList();
                foreach (var group in groups)
                    foreach(var nomenclature in group.C1CNomenclature)
                        NomenclatureID.Add(nomenclature.C1CNomenclatureID);
                if (NomenclatureID != null && NomenclatureID?.Count > 0)
                {
                    var exceptTankGroupIDs = WorkSession.DocMaterialTankGroups.Where(t => t.PlaceID == docMaterialTank.DocMaterialTankGroups.PlaceID && t.DocMaterialProductionTypeID == docMaterialTank.DocMaterialTankGroups.DocMaterialProductionTypeID && t.DocMaterialTankGroupID != DocMaterialTankGroupID).Select(t => t.C1CNomenclature).ToList();
                    if (exceptTankGroupIDs != null && exceptTankGroupIDs?.Count > 0)
                    {
                        foreach (var item in exceptTankGroupIDs)
                        {
                            ExceptNomenclatureID.AddRange(item.Select(i => i.C1CNomenclatureID).ToList());
                        }
                    }
                }
            }
            //NomenclatureID = GammaBase.DocMaterialTankGroups.Where(t => t.DocMaterialTankGroupID == msg.DocMaterialTankGroupID);
            //ExceptNomenclatureID { get; private set; }
        }

        private int _docMaterialTankID { get; set; }
        public int DocMaterialTankID
        {
            get { return _docMaterialTankID; }
            set
            {
                if (_docMaterialTankID == value) return;
                _docMaterialTankID = value;
            }
        }
        private int DocMaterialTankGroupID { get; set; }
        private GammaEntities GammaBase { get; }
        public string Name { get; set; }
        public int Volume { get; set; }

        public List<Guid> NomenclatureID { get; private set; } = new List<Guid>();
        public List<Guid> ExceptNomenclatureID { get; private set; } = new List<Guid>();

        public bool IsNotRemainderAtEnd { get; set; }

        private decimal _concentration;
        [UIAuth(UIAuthLevel.ReadOnly)]
        public decimal Concentration
        {
            get { return _concentration; }
            set
            {
                if (_concentration == value) return;
                _concentration = value;
                Quantity = Volume * 1000 * (value / 100) * (Level / 100);
                RaisePropertyChanged("Concentration");
            }
        }

        private decimal _level;
        [UIAuth(UIAuthLevel.ReadOnly)]
        public decimal Level
        {
            get { return _level; }
            set
            {
                if (_level == value) return;
                _level = value;
                Quantity = Volume * 1000 * (Concentration/100) * (value/100);
                RaisePropertyChanged("Level");
            }
        }


        private decimal _quantity;
        [UIAuth(UIAuthLevel.ReadOnly)]
        public decimal Quantity
        {
            get { return _quantity; }
            set
            {
                if (_quantity == value) return;
                _quantity = IsNotRemainderAtEnd ? 0 : value;
                //MessageManager.RecalcQuantityFromTankReaminderEvent(DocMaterialTankID, DocMaterialTankGroupID, value, NomenclatureID, ExceptNomenclatureID);
                MessageManager.RecalcQuantityFromTankGroupReaminderEvent(DocMaterialTankGroupID);
                RaisePropertyChanged("Quantity");
            }
        }

        public bool IsReadOnly { get; set; }

    }
}
