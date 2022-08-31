// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Common;
using Gamma.ViewModels;
using DevExpress.Mvvm;

namespace Gamma.Models
{
    public class BrokeDecisionForAllProducts : DbEditItemWithNomenclatureViewModel
    {
        public BrokeDecisionForAllProducts()
        {
            //ProductState = productState;
            //Comment = comment;
            ProductPlaceList = WorkSession.Places.Where(p => ((p.IsProductionPlace ?? false) || (p.IsWarehouse ?? false)) && WorkSession.BranchIds.Contains(p.BranchID))
               .Select(p => new Place
               {
                   PlaceID = p.PlaceID,
                   PlaceName = p.Name
               })
               .ToList();
        }


        protected override bool CanChooseNomenclature()
        {
            return base.CanChooseNomenclature() && !IsReadOnly;
        }

        private bool _isReadOnly = true;

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                _isReadOnly = value;
                RaisePropertyChanged("IsReadOnly");
            }
        }

        public bool NomenclatureEnabled { get; private set; } = true;

        public string Comment { get; set; }

        public List<Place> ProductPlaceList { get; set; }
        private int? _placeID;

        public int? PlaceID
        {
            get { return _placeID; }
            set
            {
                _placeID = value;
                RaisePropertyChanged("PlaceID");
            }
        }

        private KeyValuePair<int, string> _productStateID { get; set; }
        public KeyValuePair<int, string> ProductStateID
        {
            get { return _productStateID; }
            set
            {
                _productStateID = value;
                if (value.Key == (int)ProductState.ForConversion)
                    if (IsReadOnly) IsReadOnly = false;
                else
                    if (!IsReadOnly) IsReadOnly = true;
                    RaisePropertyChanged("ProductStateID");
            }
        }


        private List<KeyValuePair<int, string>> _productStateList { get; set; }
        public List<KeyValuePair<int, string>> ProductStateList
        {
            get { return _productStateList; }
            set
            {
                _productStateList = value;
                RaisePropertyChanged("ProductStateList");
            }
        }

        public void RefreshProductStateList()
        {
            if (ProductStateList == null)
                ProductStateList = new List<KeyValuePair<int, string>>();
            if (ProductStateList?.Count == 0)
            {
                ProductStateList = Functions.EnumToDictionary(typeof(ProductState))
                    .Where(r => r.Key != (int)ProductState.NeedsDecision
                            && (WorkSession.PlaceGroup == PlaceGroup.Other || (WorkSession.PlaceGroup != PlaceGroup.Other && r.Key != (int)ProductState.InternalUsage && r.Key != (int)ProductState.Limited && r.Key != (int)ProductState.ForConversion)))
                    .OrderBy(r => r.Key)
                    .ToList();
            }
        }

    }
}