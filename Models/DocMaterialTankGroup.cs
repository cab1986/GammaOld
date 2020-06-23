using Gamma.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma.Models
{
    public class DocMaterialTankGroup
    {
        public DocMaterialTankGroup(int docMaterialTankGroupID)
        {
            GammaBase = DB.GammaDb;
            DocMaterialTankGroupID = docMaterialTankGroupID;
            var tankGroup = GammaBase.DocMaterialTankGroups.Where(t => t.DocMaterialTankGroupID == docMaterialTankGroupID).FirstOrDefault();
            if (tankGroup != null)
            {
                Name = tankGroup.Name;
                IsVisible = tankGroup.IsVisible ?? true;
                DocMaterialProductionTypeID = tankGroup.DocMaterialProductionTypeID;
                NextDocMaterialTankGroupID = tankGroup.NextDocMaterialTankGroupID;
                NomenclatureID = tankGroup.C1CNomenclature.Select(t => t.C1CNomenclatureID).ToList();
                /*if (NomenclatureID != null && NomenclatureID?.Count > 0)
                {
                    var exceptTankGroupIDs = GammaBase.DocMaterialTankGroups.Where(t => t.PlaceID == tankGroup.PlaceID && t.DocMaterialProductionTypeID == tankGroup.DocMaterialProductionTypeID && t.DocMaterialTankGroupID != DocMaterialTankGroupID).Select(t => t.C1CNomenclature).ToList();
                    if (exceptTankGroupIDs != null && exceptTankGroupIDs?.Count > 0)
                    {
                        foreach (var item in exceptTankGroupIDs)
                        {
                            ExceptNomenclatureID.AddRange(item.Select(i => i.C1CNomenclatureID).ToList());
                        }
                    }
                }*/
            }
        }
        public List<MaterialProductionTankRemainder> Tanks { get; set; } = new List<MaterialProductionTankRemainder>();
        public int DocMaterialTankGroupID { get; private set; }
        public string Name { get; set; }
        public bool IsVisible { get; set; } = false;
        public decimal Quantity { get; set; }
        public int? DocMaterialProductionTypeID { get; private set; }
        public int? NextDocMaterialTankGroupID { get; private set; }
        public List<Guid> NomenclatureID { get; private set; } = new List<Guid>();
        public List<Guid> ExceptNomenclatureID { get; private set; } = new List<Guid>();
        private GammaEntities GammaBase { get; }
        public Dictionary<Guid, decimal> Composition { get; set; } = new Dictionary<Guid, decimal>();

        /// <summary>
        /// Заполняем исключение групп номенклатур из композиции. 
        /// </summary>
        /// <param name="placeID"></param>
        public void FillExpectNomenclatureID(int placeID)
        {
            //Если в какой то из групп бассейнов есть заполненная разрешенная группа номенклатур (C1CNomenclature), то во всех остальных группах бассейнов данная группа номенклатур должна быть исключена из композиции
            if (DocMaterialProductionTypeID != null)
            {
                var exceptTankGroupIDs = GammaBase.DocMaterialTankGroups.Where(t => t.PlaceID == placeID && t.DocMaterialProductionTypeID == DocMaterialProductionTypeID && t.DocMaterialTankGroupID != DocMaterialTankGroupID).Select(t => t.C1CNomenclature).ToList();
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
}
