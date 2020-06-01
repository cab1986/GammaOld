// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
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
        public MaterialProductionTankRemainder(int docMaterialTankID, string name, int volume)
        {
            GammaBase = DB.GammaDb;
            DocMaterialTankID = docMaterialTankID;
            Name = name + " V="+volume+"м3";
            Volume = volume;
            var docMaterialTank = GammaBase.DocMaterialTanks.Where(t => t.DocMaterialTankID == docMaterialTankID).FirstOrDefault();
            if (docMaterialTank != null)
            {
                DocMaterialTankGroupID = docMaterialTank.DocMaterialTankGroupID;
                NomenclatureID = docMaterialTank.DocMaterialTankGroups.C1CNomenclature.Select(t => t.C1CNomenclatureID).ToList();
                if (NomenclatureID != null && NomenclatureID?.Count > 0)
                {
                    var exceptTankGroupIDs = GammaBase.DocMaterialTankGroups.Where(t => t.PlaceID == docMaterialTank.DocMaterialTankGroups.PlaceID && t.DocMaterialProductionTypeID == docMaterialTank.DocMaterialTankGroups.DocMaterialProductionTypeID && t.DocMaterialTankGroupID != DocMaterialTankGroupID).Select(t => t.C1CNomenclature).ToList();
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

        public MaterialProductionTankRemainder(int docMaterialTankID, string name, int volume, decimal concentration, decimal level)
        {
            GammaBase = DB.GammaDb;
            DocMaterialTankID = docMaterialTankID;
            Volume = volume;
            Concentration = concentration;
            Level = level;
            Name = name;
            var docMaterialTank = GammaBase.DocMaterialTanks.Where(t => t.DocMaterialTankID == docMaterialTankID).FirstOrDefault();
            if (docMaterialTank != null)
            {
                DocMaterialTankGroupID = docMaterialTank.DocMaterialTankGroupID;
                NomenclatureID = docMaterialTank.DocMaterialTankGroups.C1CNomenclature.Select(t => t.C1CNomenclatureID).ToList();
                if (NomenclatureID != null && NomenclatureID?.Count > 0)
                {
                    var exceptTankGroupIDs = GammaBase.DocMaterialTankGroups.Where(t => t.PlaceID == docMaterialTank.DocMaterialTankGroups.PlaceID && t.DocMaterialProductionTypeID == docMaterialTank.DocMaterialTankGroups.DocMaterialProductionTypeID && t.DocMaterialTankGroupID != DocMaterialTankGroupID).Select(t => t.C1CNomenclature).ToList();
                    if (exceptTankGroupIDs != null && exceptTankGroupIDs?.Count > 0)
                    {
                        foreach (var item in exceptTankGroupIDs)
                        {
                            ExceptNomenclatureID.AddRange(item.Select(i => i.C1CNomenclatureID).ToList());
                        }
                    }
                }
            }
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
                _quantity = value;
                //MessageManager.RecalcQuantityFromTankReaminderEvent(DocMaterialTankID, DocMaterialTankGroupID, value, NomenclatureID, ExceptNomenclatureID);
                MessageManager.RecalcQuantityFromTankGroupReaminderEvent(DocMaterialTankGroupID);
                RaisePropertyChanged("Quantity");
            }
        }

        public bool IsReadOnly { get; set; }

    }
}
