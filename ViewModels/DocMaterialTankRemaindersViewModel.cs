// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Models;
using System.Data.Entity;
using DevExpress.Mvvm;
using Gamma.Interfaces;
using Gamma.Common;
using Gamma.Entities;

namespace Gamma.ViewModels
{
    public class DocMaterialTankRemaindersViewModel : SaveImplementedViewModel, ICheckedAccess, IFillClearGrid
    {
        /// <summary>
        /// Конструктор для новых остатков на раскатах
        /// </summary>
        /// <param name="placeID">ID передела</param>
        /// <param name="gammaBase">Контекст базы данных</param>
        public DocMaterialTankRemaindersViewModel(int placeID, bool isConfirmed, DocMaterialTankGroupContainer tankGroupContainer, GammaEntities gammaBase = null)
        {
            gammaBase = gammaBase ?? DB.GammaDb;
            PlaceID = placeID;
            IsConfirmed = isConfirmed;
            //FillGrid();
            TankGroupContainer = tankGroupContainer;
        }

        public DocMaterialTankGroupContainer TankGroupContainer { get; set; }
        private int PlaceID { get; set; }
        private byte? ShiftID { get; set; }
        private DateTime DocDate { get; set; }

        public bool IsVisibledCompozitionTab => WorkSession.ShiftID == 0;

        /// <summary>
        /// Сохранение остатков в БД
        /// </summary>
        /// <param name="itemID">ID документа закрытия смены</param>
        public override bool SaveToModel(Guid itemID)
        {
#if DEBUG
            Console.WriteLine(IsReadOnly.ToString());
#endif
            if (IsReadOnly) return true;
            UIServices.SetBusyState();
            
            using (var gammaBase = DB.GammaDb)
            {
                //gammaBase.DocMaterialTankRemainders.RemoveRange(gammaBase.DocMaterialTankRemainders.Where(r => r.DocID == itemID));
                var tankIds = new List<int>();
                foreach (var tankGroup in TankGroupContainer.TankGroups)
                {
                    foreach (var tank in tankGroup.Tanks)
                    {
                        var tankRemainder = gammaBase.DocMaterialTankRemainders.Where(t => t.DocID == itemID && t.DocMaterialTankID == tank.DocMaterialTankID).FirstOrDefault();
                        if (tankRemainder == null)
                        {
                            gammaBase.DocMaterialTankRemainders.Add(new DocMaterialTankRemainders()
                            {
                                DocMaterialTankRemainderID = SqlGuidUtil.NewSequentialid(),
                                DocMaterialTankID = tank.DocMaterialTankID,
                                Concentration = tank.Concentration,
                                Level = (int)tank.Level,
                                DocID = itemID
                            });
                        }
                        else
                        {
                            tankRemainder.Concentration = tank.Concentration;
                            tankRemainder.Level = (int)tank.Level;
                        }
                        tankIds.Add(tank.DocMaterialTankID);
                    }
                }
                gammaBase.DocMaterialTankRemainders.RemoveRange(gammaBase.DocMaterialTankRemainders.Where(r => r.DocID == itemID && !tankIds.Contains(r.DocMaterialTankID)));

                gammaBase.SaveChanges();
            }
            return true;
        }

        public bool IsChanged { get; private set; }

        /// <summary>
        /// Очистка тамбуров с раската
        /// </summary>
        public void ClearGrid()
        {
            /*foreach (var items in TankGroupContainer.TankGroups)
            {
                items.Composition.Clear();
            }*/
            /*var tankGroups = new List<DocMaterialTankGroup>();
            for (int i = tankGroups.Count(); i < 4; i++)
            {
                tankGroups.Add(new DocMaterialTankGroup(0));
            }
            TankGroupContainer.TankGroups = tankGroups;*/
            TankGroupContainer.Clear();
        }

        public void ClearGridWithIndex(byte index)
        {
            var i = 1;
        }

        public void FillGridWithNoFillEnd()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Перезаполнение тамбуров с раската
        /// </summary>
        public void FillGrid()
        {
            using (var gammaBase = DB.GammaDb)
            {
                //    ClearGrid();
                if (TankGroupContainer.TankGroups[0].Tanks?.Count > 0)
                {
                    foreach (var tankG in TankGroupContainer.TankGroups)
                    {
                        tankG.Composition?.Clear();
                    }
                }
                else
                {
                    TankGroupContainer.TankGroups = new DocMaterialTankGroupContainer(PlaceID).TankGroups;
                }

            }
        }

        public bool TankGroup0Visible { get; set; } = false;
        public string TankGroup0Name { get; set; }
        private List<MaterialProductionTankRemainder> _tankGroup0Remainders = new List<MaterialProductionTankRemainder>();
        public List<MaterialProductionTankRemainder> TankGroup0Remainders
        {
            get { return _tankGroup0Remainders; }
            set
            {
                _tankGroup0Remainders = value;
                RaisePropertyChanged("TankGroup0Remainders");
            }
        }

        public bool TankGroup1Visible { get; set; } = false;
        public string TankGroup1Name { get; set; }
        private List<MaterialProductionTankRemainder> _tankGroup1Remainders = new List<MaterialProductionTankRemainder>();
        public List<MaterialProductionTankRemainder> TankGroup1Remainders
        {
            get { return _tankGroup1Remainders; }
            set
            {
                _tankGroup1Remainders = value;
                RaisePropertyChanged("TankGroup1Remainders");
            }
        }

        public bool TankGroup2Visible { get; set; } = false;
        public string TankGroup2Name { get; set; }
        private List<MaterialProductionTankRemainder> _tankGroup2Remainders = new List<MaterialProductionTankRemainder>();
        public List<MaterialProductionTankRemainder> TankGroup2Remainders
        {
            get { return _tankGroup2Remainders; }
            set
            {
                _tankGroup2Remainders = value;
                RaisePropertyChanged("TankGroup2Remainders");
            }
        }

        public bool TankGroup3Visible { get; set; } = false;
        public string TankGroup3Name { get; set; }
        private List<MaterialProductionTankRemainder> _tankGroup3Remainders = new List<MaterialProductionTankRemainder>();
        public List<MaterialProductionTankRemainder> TankGroup3Remainders
        {
            get { return _tankGroup3Remainders; }
            set
            {
                _tankGroup3Remainders = value;
                RaisePropertyChanged("TankGroup3Remainders");
            }
        }

        public DelegateCommand<int> ShowProductCommand { get; set; }


        private bool IsConfirmed { get; set; } = false;
        public void ChangeConfirmed(bool isConfirmed)
        {
            IsConfirmed = isConfirmed;
            RaisePropertyChanged("IsReadOnly");
        }
        public bool IsReadOnly => !DB.HaveWriteAccess("DocMaterialTankRemainders") || IsConfirmed;
    }
}
