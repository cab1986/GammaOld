using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Gamma.Models;

namespace Gamma.ViewModels
{
    class NomenclatureEditViewModel : SaveImplementedViewModel
    {
        public NomenclatureEditViewModel(Guid nomenclatureId, GammaEntities gammaBase = null) : base(gammaBase)
        {
            BarcodeTypes = GammaBase.BarcodeTypes.ToList();
            NomenclatureName = GammaBase.C1CNomenclature.First(n => n.C1CNomenclatureID == nomenclatureId).Name;
            NomenclatureBarcodes = new ObservableCollection<NomenclatureBarcode>
                (
                    GammaBase.C1CCharacteristics.Where(c => c.C1CNomenclatureID == nomenclatureId).Select(c => new NomenclatureBarcode
                    {
                        NomenclatureId = nomenclatureId,
                        CharacteristicId = c.C1CCharacteristicID,
                        CharacteristicName = c.Name,
                        BarcodeTypeId = 0
                    })
                );
            foreach (var nomenclatureBarcode in NomenclatureBarcodes)
            {
                var bcode = GammaBase.NomenclatureBarcodes
                    .FirstOrDefault(
                        b =>
                            b.C1CNomenclatureID == nomenclatureBarcode.NomenclatureId &&
                            b.C1CCharacteristicID == nomenclatureBarcode.CharacteristicId);
                if (bcode == null) continue;
                nomenclatureBarcode.Barcode = bcode.Barcode;
                nomenclatureBarcode.BarcodeTypeId = bcode.BarcodeTypeID??0;
            }
        }

        public string NomenclatureName { get; set; }

        public ObservableCollection<NomenclatureBarcode> NomenclatureBarcodes { get; set; }

        public List<BarcodeTypes> BarcodeTypes { get; }
      
        public override bool SaveToModel(GammaEntities gammaBase = null)
        {
            if (!DB.HaveWriteAccess("NomenclatureBarcodes")) return true;
            gammaBase = gammaBase ?? DB.GammaDb;
            foreach (var nomenclatureBarcode in NomenclatureBarcodes)
            {
                var bcode =
                    gammaBase.NomenclatureBarcodes.FirstOrDefault(
                        b =>
                            b.C1CNomenclatureID == nomenclatureBarcode.NomenclatureId &&
                            b.C1CCharacteristicID == nomenclatureBarcode.CharacteristicId);
                if (bcode == null)
                {
                    bcode = new NomenclatureBarcodes
                    {
                        C1CNomenclatureID = nomenclatureBarcode.NomenclatureId,
                        C1CCharacteristicID = nomenclatureBarcode.CharacteristicId
                    };
                    gammaBase.NomenclatureBarcodes.Add(bcode);
                }
                bcode.BarcodeTypeID = nomenclatureBarcode.BarcodeTypeId;
                bcode.Barcode = nomenclatureBarcode.Barcode;
            }
            gammaBase.SaveChanges();
            return true;
        }
    }
}
