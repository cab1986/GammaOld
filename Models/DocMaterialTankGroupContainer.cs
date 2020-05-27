using DevExpress.Mvvm;
using Gamma.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma.Models
{
    public class DocMaterialTankGroupContainer : ViewModelBase
    {
        public DocMaterialTankGroupContainer()
        {
            Messenger.Default.Register<RecalcQuantityFromTankGroupReaminderMessage>(this, RecalcQuantityFromTankGroupReaminder);
            Messenger.Default.Register<DeleteNomenclatureInCompositionFromTankGroupMessage>(this, DeleteNomenclatureInCompositionFromTankGroup);
           
        }

        public DocMaterialTankGroupContainer(int placeID, GammaEntities gammaBase = null):this()
        {
            Fill(placeID);
        }

        public DocMaterialTankGroupContainer(Guid docID, int placeID, GammaEntities gammaBase = null):this(placeID)
        {
            gammaBase = gammaBase ?? DB.GammaDb;
            var tankRemainders = gammaBase.DocMaterialTankRemainders.Where(dr => dr.DocID == docID).ToList();
            if (tankRemainders != null)
                foreach (var tankRemainder in tankRemainders)
                {
                    var tankG = TankGroups.Where(g => g.DocMaterialTankGroupID == tankRemainder.DocMaterialTanks.DocMaterialTankGroupID).FirstOrDefault();
                    if (tankG != null)
                    {
                        var tank = tankG.Tanks.Where(t => t.DocMaterialTankID == tankRemainder.DocMaterialTankID).FirstOrDefault();
                        if (tank != null)
                        {
                            tank.Concentration = tankRemainder.Concentration;
                            tank.Level = tankRemainder.Level;
                        }          
                    };
                };
        }

        private List<DocMaterialTankGroup> _tankGroups { get; set; } = new List<DocMaterialTankGroup>();
        public List<DocMaterialTankGroup> TankGroups
        {
            get { return _tankGroups; }
            set
            {
                _tankGroups = value;
                RaisePropertiesChanged("TankGroups");
            }
        }

        public void AddComposition(Guid nomenclatureID, decimal? quantityDismiss, decimal? quantityIn)
        {
            //сначала для дочерних хранилищ
            foreach (var tankGroup in TankGroups.Where(p => (p.DocMaterialProductionTypeID != null && p.DocMaterialProductionTypeID != (int)DocMaterialProductionTypes.Send) && (p.NomenclatureID.Count == 0 || (p.NomenclatureID.Count > 0 && p.NomenclatureID.Contains(nomenclatureID))) && !p.ExceptNomenclatureID.Contains(nomenclatureID)))
            {
                if (tankGroup.DocMaterialProductionTypeID != null)
                {
                    switch (tankGroup.DocMaterialProductionTypeID)
                    {
                        case (3):
                            tankGroup.Composition.Add(nomenclatureID, quantityDismiss ?? 0);
                            break;
                        case (4):
                            tankGroup.Composition.Add(nomenclatureID, quantityIn ?? 0);
                            break;
                    }
                }
            }
            //затем для хранилищ с итоговой композицией
            decimal quantity = 0;
            foreach (var tankGroup in TankGroups.Where(p => p.DocMaterialProductionTypeID == (int)DocMaterialProductionTypes.Send))
            {
                var tankChildCompositions = TankGroups.Where(p => p.NextDocMaterialTankGroupID == tankGroup.DocMaterialTankGroupID).Select(p => p.Composition);
                foreach (var childComposition in tankChildCompositions)
                {
                    foreach (KeyValuePair<Guid, decimal> item in childComposition)
                    {
                       if (item.Key == nomenclatureID)
                            quantity = quantity + item.Value;
                    }
                }
                tankGroup.Composition.Add(nomenclatureID, quantity);
            }
        }

        public void DeleteComposition(Guid nomenclatureID)
        {
            foreach (var tankG in TankGroups)
            {
                tankG.Composition.Remove(nomenclatureID);
            }
            RecalcAllNomenclatureInComposition();
        }

        public void RefreshComposition(Guid nomenclatureID, decimal? quantityDismiss, decimal? quantityIn)
        {
            //сначала для дочерних хранилищ
            foreach (var tankGroup in TankGroups.Where(p => (p.DocMaterialProductionTypeID != null && p.DocMaterialProductionTypeID != (int)DocMaterialProductionTypes.Send) && (p.NomenclatureID.Count == 0 || (p.NomenclatureID.Count > 0 && p.NomenclatureID.Contains(nomenclatureID))) && !p.ExceptNomenclatureID.Contains(nomenclatureID)))
            {
                var composition = tankGroup.Composition.Where(c => c.Key == nomenclatureID).Count();
                if (composition == 0)
                    AddComposition(nomenclatureID, quantityDismiss, quantityIn);
                else
                    if (tankGroup.DocMaterialProductionTypeID != null)
                    {
                        switch (tankGroup.DocMaterialProductionTypeID)
                        {
                            case (3):
                                tankGroup.Composition[nomenclatureID] = quantityDismiss ?? 0;
                                break;
                            case (4):
                                tankGroup.Composition[nomenclatureID] = quantityIn ?? 0;
                                break;
                        }
                    }
            }
            //затем для хранилищ с итоговой композицией
            decimal quantity = 0;
            foreach (var tankGroup in TankGroups.Where(p => p.DocMaterialProductionTypeID == (int)DocMaterialProductionTypes.Send))
            {
                var tankChildCompositions = TankGroups.Where(p => p.NextDocMaterialTankGroupID == tankGroup.DocMaterialTankGroupID).Select(p => p.Composition);
                foreach (var childComposition in tankChildCompositions)
                {
                    foreach (KeyValuePair<Guid, decimal> item in childComposition)
                    {
                        if (item.Key == nomenclatureID)
                            quantity = quantity + item.Value;
                    }
                }
                tankGroup.Composition[nomenclatureID] = quantity;
            }
            //RecalcNomenclatureInComposition(nomenclatureID);
            RecalcAllNomenclatureInComposition();
        }

        public void RecalcNomenclatureInComposition(Guid nomenclatureID)
        {
                decimal quantity = 0;
                foreach (var tankG in TankGroups)
                {
                    if (tankG.Composition.Sum(c => c.Value) > 0)
                        quantity = quantity + ((tankG.Composition.FirstOrDefault(c => c.Key == nomenclatureID).Value) / tankG.Composition.Sum(c => c.Value)) * tankG.Tanks.Sum(t => t.Quantity);
                }
            //quantity = Math.Round(quantity, 0);
            MessageManager.RecalcMaterialProductionQuantityEndFromTankReaminderEvent(nomenclatureID, quantity);
            
        }

        public void RecalcAllNomenclatureInComposition()
        {
            var nomenclatureRecalced = new List<Guid>();
            foreach (var tankG in TankGroups)
            {
                foreach (var nomenclature in tankG.Composition)
                {
                    if (!nomenclatureRecalced.Contains(nomenclature.Key))
                    {
                        RecalcNomenclatureInComposition(nomenclature.Key);
                        nomenclatureRecalced.Add(nomenclature.Key);
                    }
                }
            }
        }

        public void RecalcQuantityFromTankGroupReaminder(RecalcQuantityFromTankGroupReaminderMessage msg)
        {
            var tankGroup = TankGroups.Where(t => t.DocMaterialTankGroupID == msg.DocMaterialTankGroupID).FirstOrDefault();
            if (tankGroup != null)
            {
                foreach (var composition in tankGroup.Composition)
                {
                    RecalcNomenclatureInComposition(composition.Key);
                }
                //MessageManager.RecalcQuantityFromTankReaminderEvent(msg.DocMaterialTankGroupID, tankGroup.DocMaterialProductionTypeID, tankGroup.Tanks.Sum(t => t.Quantity), tankGroup.NomenclatureID, tankGroup.ExceptNomenclatureID);
            }
            /*
            var quantityInMaterial = DocMaterialProductions.Where(p => (msg.NomenclatureID.Count == 0 || (msg.NomenclatureID.Count > 0 && msg.NomenclatureID.Contains((Guid)p.ParentID))) && !msg.ExceptNomenclatureID.Contains((Guid)p.ParentID)).Sum(p => p.QuantityIn);
            foreach (var item in DocMaterialProductions.Where(p => (msg.NomenclatureID.Count == 0 || (msg.NomenclatureID.Count > 0 && msg.NomenclatureID.Contains((Guid)p.ParentID))) && !msg.ExceptNomenclatureID.Contains((Guid)p.ParentID)))
            {
                //var item = DocMaterialProductions.FirstOrDefault(d => d.NomenclatureID == msg.NomenclatureID && (d.CharacteristicID == msg.CharacteristicID || (d.CharacteristicID == null && msg.CharacteristicID == null)));
                if ((quantityInMaterial ?? 0) != 0 && (item.QuantityIn ?? 0) != 0)
                    item.QuantityRemainderAtEnd = Math.Round((decimal)(item.QuantityIn / quantityInMaterial * msg.Quantity), 3);
                else
                    item.QuantityRemainderAtEnd = 0;
            }
            var n = new DocMaterialProductionItem { WithdrawByFact = false };
            DocMaterialProductions.Add(n);
            DocMaterialProductions.Remove(n);
            */
        }

        public void DeleteNomenclatureInCompositionFromTankGroup(DeleteNomenclatureInCompositionFromTankGroupMessage msg)
        {
            DeleteComposition(msg.NomenclatureID);
        }

        public void Clear()
        {
            /*foreach (var tankG in TankGroups)
            {
                tankG.Tanks.Clear();
            }
            TankGroups.Clear();*/
            var tankGroups = new List<DocMaterialTankGroup>();
            for (int i = tankGroups.Count(); i < 4; i++)
            {
                tankGroups.Add(new DocMaterialTankGroup(0));
            }
            TankGroups = tankGroups;
        }

        public void Fill(int placeID)
        {
            using (var gammaBase = DB.GammaDb)
            {
                if (TankGroups.Count > 0)
                    Clear();
                var tanks = gammaBase.DocMaterialTanks.Where(dr => dr.DocMaterialTankGroups.PlaceID == placeID).ToList();
                var groupNumber = 0;
                foreach (var groupID in tanks.Select(r => r.DocMaterialTankGroupID).Distinct())
                {
                    DocMaterialTankGroup tankGroup = new DocMaterialTankGroup(groupID);
                    foreach (var tank in tanks.Where(r => r.DocMaterialTankGroupID == groupID).Select(remainder => new MaterialProductionTankRemainder(remainder.DocMaterialTankID, remainder.Name, remainder.Volume)
                    { }))
                    {
                        tankGroup.Tanks.Add(tank);
                    };
                    tankGroup.Name = tankGroup.Name + " V=" + tankGroup.Tanks.Sum(t => t.Volume) + "м3";
                    TankGroups.Add(tankGroup);
                    groupNumber += 1;
                }
                for (int i = TankGroups.Count(); i < 4; i++)
                {
                    TankGroups.Add(new DocMaterialTankGroup(0));
                }
            }
        }
    }
}
