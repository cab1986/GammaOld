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

        public decimal Quantity
        {
            get
            {
                decimal quantity = 0;
                foreach (var tankGroup in TankGroups)
                {
                    quantity = quantity + tankGroup.Quantity;
                }
                return quantity;
            }
        }

        public void AddComposition(Guid nomenclatureID, Guid? parentID, decimal? quantityDismiss, decimal? quantityIn)
        {
            //сначала для дочерних хранилищ
            foreach (var tankGroup in TankGroups.Where(p => (p.DocMaterialProductionTypeID != null && p.DocMaterialProductionTypeID != (int)DocMaterialProductionTypes.Send) && (p.NomenclatureID.Count == 0 || (p.NomenclatureID.Count > 0 && p.NomenclatureID.Contains(parentID ?? Guid.Empty))) && !p.ExceptNomenclatureID.Contains(parentID ?? Guid.Empty)))
            {
                if (tankGroup.DocMaterialProductionTypeID != null)
                {
                    switch (tankGroup.DocMaterialProductionTypeID)
                    {
                        case ((int)DocMaterialProductionTypes.Dismiss):
                            tankGroup.Composition.Add(nomenclatureID, quantityDismiss ?? 0);
                            break;
                        case ((int)DocMaterialProductionTypes.In):
                            tankGroup.Composition.Add(nomenclatureID, quantityIn ?? 0);
                            break;
                        case ((int)DocMaterialProductionTypes.InToCompositionTank):
                            tankGroup.Composition.Add(nomenclatureID, 0);
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
            RecalcAllNomenclatureInComposition(true);
        }

        public void RefreshComposition(Guid nomenclatureID, Guid? parentID, decimal? quantityDismiss, decimal? quantityIn, bool? isNotSendNomenclature, bool isRecalcMaterialProduction)
        {
            decimal sumQuantityTanksWhereExistNomenclatureInComposition = 0;
            decimal quantityNotSendNomenclature = (quantityDismiss ?? 0) + (quantityIn ?? 0);
            List<DocMaterialTankGroup> TanksWhereExistNomenclatureInComposition = new List<DocMaterialTankGroup>();
            //сначала для дочерних хранилищ
            foreach (var tankGroup in TankGroups.Where(p => (p.DocMaterialProductionTypeID != null && p.DocMaterialProductionTypeID != (int)DocMaterialProductionTypes.Send) && (p.NomenclatureID.Count == 0 || (p.NomenclatureID.Count > 0 && p.NomenclatureID.Contains(parentID ?? Guid.Empty)))
                && !p.ExceptNomenclatureID.Contains(parentID ?? Guid.Empty)))
                
            {
                if (isNotSendNomenclature == true)
                {
                    TanksWhereExistNomenclatureInComposition.Add(tankGroup);
                    quantityDismiss = 0;
                    quantityIn = 0;
                }
                else
                    tankGroup.NotSendNomenclature.Remove(nomenclatureID);

                sumQuantityTanksWhereExistNomenclatureInComposition = sumQuantityTanksWhereExistNomenclatureInComposition + tankGroup.Tanks.Sum(t => t.Quantity);

                var composition = tankGroup.Composition.Where(c => c.Key == nomenclatureID).Count();
                if (composition == 0)
                    AddComposition(nomenclatureID, parentID, quantityDismiss, quantityIn);
                else
                    if (tankGroup.DocMaterialProductionTypeID != null)
                    {
                        switch (tankGroup.DocMaterialProductionTypeID)
                        {
                            case ((int)DocMaterialProductionTypes.Dismiss):
                                tankGroup.Composition[nomenclatureID] = quantityDismiss ?? 0;
                                break;
                            case ((int)DocMaterialProductionTypes.In):
                            tankGroup.Composition[nomenclatureID] = quantityIn ?? 0;
                            break;
                        case ((int)DocMaterialProductionTypes.InToCompositionTank):
                            tankGroup.Composition[nomenclatureID] = 0;
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
                if (isNotSendNomenclature == true)
                    TanksWhereExistNomenclatureInComposition.Add(tankGroup);
                else
                    tankGroup.NotSendNomenclature.Remove(nomenclatureID);

                sumQuantityTanksWhereExistNomenclatureInComposition = sumQuantityTanksWhereExistNomenclatureInComposition + tankGroup.Tanks.Sum(t => t.Quantity);
            }


            foreach (var tankGroup in TanksWhereExistNomenclatureInComposition)
            {
                var quantityNotSendNomenclatureInTankGroup = Math.Round(tankGroup.Tanks.Sum(t => t.Quantity) / sumQuantityTanksWhereExistNomenclatureInComposition * quantityNotSendNomenclature, 2);
                var notSendNomenclature = tankGroup.NotSendNomenclature.Where(c => c.Key == nomenclatureID).Count();
                if (notSendNomenclature == 0)
                {
                    if (isNotSendNomenclature == true)
                        tankGroup.NotSendNomenclature.Add(nomenclatureID, quantityNotSendNomenclatureInTankGroup);
                }
                else
                {
                    if (isNotSendNomenclature == true)
                        tankGroup.NotSendNomenclature[nomenclatureID] = quantityNotSendNomenclatureInTankGroup;
                }
                //tankGroup.NotSendNomenclature[nomenclatureID] = Math.Round(tankGroup.Tanks.Sum(t => t.Quantity) / sumQuantityTanksWhereExistNomenclatureInComposition * quantityNotSendNomenclature, 2);
            }
            //RecalcNomenclatureInComposition(nomenclatureID);
            RecalcAllNomenclatureInComposition(isRecalcMaterialProduction);
        }

        public void RecalcNomenclatureInComposition(Guid nomenclatureID, bool isRecalcMaterialProduction)
        {
                decimal quantity = 0;
                foreach (var tankG in TankGroups)
                {
                    if (tankG.Composition.Sum(c => c.Value) > 0)
                        quantity = quantity + Math.Round(((tankG.Composition.FirstOrDefault(c => c.Key == nomenclatureID).Value) / tankG.Composition.Sum(c => c.Value)) * (tankG.Tanks.Sum(t => t.Quantity) - tankG.NotSendNomenclature.Sum(n => n.Value)),0);
                }
            //quantity = Math.Round(quantity, 0);
            if (isRecalcMaterialProduction)
                MessageManager.RecalcMaterialProductionQuantityEndFromTankReaminderEvent(nomenclatureID, quantity);
            
        }

        public void RecalcAllNomenclatureInComposition(bool isRecalcMaterialProduction)
        {
            var nomenclatureRecalced = new List<Guid>();
            foreach (var tankG in TankGroups)
            {
                foreach (var nomenclature in tankG.Composition)
                {
                    if (!nomenclatureRecalced.Contains(nomenclature.Key))
                    {
                        RecalcNomenclatureInComposition(nomenclature.Key, isRecalcMaterialProduction);
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
                    RecalcNomenclatureInComposition(composition.Key, true);
                }
                //MessageManager.RecalcQuantityFromTankReaminderEvent(msg.DocMaterialTankGroupID, tankGroup.DocMaterialProductionTypeID, tankGroup.Tanks.Sum(t => t.Quantity), tankGroup.NomenclatureID, tankGroup.ExceptNomenclatureID);
            }
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
                    foreach (var tank in tanks.Where(r => r.DocMaterialTankGroupID == groupID).Select(remainder => new MaterialProductionTankRemainder(remainder.DocMaterialTankID, remainder.Name, remainder.Volume, tankGroup?.DocMaterialProductionTypeID)
                    { }))
                    {
                        tankGroup.Tanks.Add(tank);
                    };
                    tankGroup.Name = tankGroup.Name + " V=" + tankGroup.Tanks.Sum(t => t.Volume) + "м3";
                    TankGroups.Add(tankGroup);
                    groupNumber += 1;
                }
                foreach (var tankGroup in TankGroups)
                {
                    tankGroup.FillExpectNomenclatureID(placeID);
                }
                for (int i = TankGroups.Count(); i < 4; i++)
                {
                    TankGroups.Add(new DocMaterialTankGroup(0));
                }
            }
        }
    }
}
