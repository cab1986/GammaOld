﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using DevExpress.Mvvm;

namespace Gamma.Models
{
    public class PaperBase : Product // ViewModelBase
    {
        public byte? BreakNumber { get; set; }
        public int? PlaceProductionid { get; set; }
        //public DateTime Date { get; set; }
        /*public Guid DocID { get; set; }
        public Guid ProductID { get; set; }
        public Guid CharacteristicID { get; set; }
        public Guid NomenclatureID { get; set; }
        public string Number { get; set; }
        public string Nomenclature { get; set; }*/
        public string Nomenclature
        {
            get { return NomenclatureName; }
            set
            {
                NomenclatureName = value;
            }
        }
        /// <summary>
        /// Качество бумаги-основы
        /// </summary>
        public ProductState ProductState { get; set; }
        public decimal Weight
        {
            get { return Quantity; }
            set
            {
                Quantity = value;
            }
        }
        /*private decimal _weight;
        public decimal Weight
        {
            get { return _weight; }
            set
            {
                _weight = value;
                RaisePropertyChanged("Weight");
            }
        }*/
        public int Diameter { get; set; }
        public decimal Length { get; set; }
        public Guid BaseMeasureUnitID { get; set; }
        public string BaseMeasureUnit { get; set; }
        public int? RealFormat { get; set; }
        public decimal? RealBasisWeight { get; set; }
    }
}
