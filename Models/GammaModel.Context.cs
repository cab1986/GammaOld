﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gamma.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class GammaEntities : DbContext
    {
        public GammaEntities()
            : base("name=GammaEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<ProductPalletItems> ProductPalletItems { get; set; }
        public virtual DbSet<RolePermits> RolePermits { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<Permits> Permits { get; set; }
        public virtual DbSet<C1CCharacteristicProperties> C1CCharacteristicProperties { get; set; }
        public virtual DbSet<C1CMeasureUnitQualifiers> C1CMeasureUnitQualifiers { get; set; }
        public virtual DbSet<C1CMeasureUnits> C1CMeasureUnits { get; set; }
        public virtual DbSet<C1CNomenclatureProperties> C1CNomenclatureProperties { get; set; }
        public virtual DbSet<C1CProperties> C1CProperties { get; set; }
        public virtual DbSet<Reports> Reports { get; set; }
        public virtual DbSet<Templates> Templates { get; set; }
        public virtual DbSet<DocProducts> DocProducts { get; set; }
        public virtual DbSet<Products> Products { get; set; }
        public virtual DbSet<PermitTables> PermitTables { get; set; }
        public virtual DbSet<C1CEnumGroupTypes> C1CEnumGroupTypes { get; set; }
        public virtual DbSet<C1CNomenclatureGroups> C1CNomenclatureGroups { get; set; }
        public virtual DbSet<C1CPropertyValues> C1CPropertyValues { get; set; }
        public virtual DbSet<C1CCharacteristics> C1CCharacteristics { get; set; }
        public virtual DbSet<C1CNomenclature> C1CNomenclature { get; set; }
        public virtual DbSet<DocWithdrawalMaterials> DocWithdrawalMaterials { get; set; }
        public virtual DbSet<ProductPallets> ProductPallets { get; set; }
        public virtual DbSet<ProductGroupPacks> ProductGroupPacks { get; set; }
        public virtual DbSet<DocCloseShiftRemainders> DocCloseShiftRemainders { get; set; }
        public virtual DbSet<DocTypes> DocTypes { get; set; }
        public virtual DbSet<C1CRejectionReasons> C1CRejectionReasons { get; set; }
        public virtual DbSet<DocProductChangeState> DocProductChangeState { get; set; }
        public virtual DbSet<vProductsInfo> vProductsInfo { get; set; }
        public virtual DbSet<DocMovement> DocMovement { get; set; }
        public virtual DbSet<DocProduction> DocProduction { get; set; }
        public virtual DbSet<Docs> Docs { get; set; }
        public virtual DbSet<DocWithdrawal> DocWithdrawal { get; set; }
        public virtual DbSet<Places> Places { get; set; }
        public virtual DbSet<Rests> Rests { get; set; }
        public virtual DbSet<SourceSpools> SourceSpools { get; set; }
        public virtual DbSet<PlaceGroups> PlaceGroups { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<ProductionTaskStates> ProductionTaskStates { get; set; }
        public virtual DbSet<ProductionTaskWR> ProductionTaskWR { get; set; }
        public virtual DbSet<ProductionTaskSGB> ProductionTaskSGB { get; set; }
        public virtual DbSet<ProductionTasks> ProductionTasks { get; set; }
        public virtual DbSet<ProductionTaskRWCutting> ProductionTaskRWCutting { get; set; }
        public virtual DbSet<ProductionTaskBatches> ProductionTaskBatches { get; set; }
        public virtual DbSet<vCharacteristicSGBProperties> vCharacteristicSGBProperties { get; set; }
        public virtual DbSet<ProductSpools> ProductSpools { get; set; }
    
        public virtual ObjectResult<Nullable<byte>> UserPermit(string tableName)
        {
            var tableNameParameter = tableName != null ?
                new ObjectParameter("TableName", tableName) :
                new ObjectParameter("TableName", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<byte>>("UserPermit", tableNameParameter);
        }
    
        public virtual ObjectResult<Get1CNomenclature_Result> Get1CNomenclature(Nullable<decimal> parentID)
        {
            var parentIDParameter = parentID.HasValue ?
                new ObjectParameter("ParentID", parentID) :
                new ObjectParameter("ParentID", typeof(decimal));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Get1CNomenclature_Result>("Get1CNomenclature", parentIDParameter);
        }
    
        public virtual ObjectResult<GetProductionTaskProducts_Result3> GetProductionTaskProducts(Nullable<System.Guid> productionTaskID)
        {
            var productionTaskIDParameter = productionTaskID.HasValue ?
                new ObjectParameter("ProductionTaskID", productionTaskID) :
                new ObjectParameter("ProductionTaskID", typeof(System.Guid));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetProductionTaskProducts_Result3>("GetProductionTaskProducts", productionTaskIDParameter);
        }
    
        public virtual ObjectResult<GetCharPropsDescriptions_Result> GetCharPropsDescriptions(Nullable<System.Guid> characteristicID)
        {
            var characteristicIDParameter = characteristicID.HasValue ?
                new ObjectParameter("CharacteristicID", characteristicID) :
                new ObjectParameter("CharacteristicID", typeof(System.Guid));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetCharPropsDescriptions_Result>("GetCharPropsDescriptions", characteristicIDParameter);
        }
    
        public virtual ObjectResult<GetDocCloseShiftPMSpools_Result> GetDocCloseShiftPMSpools(Nullable<System.Guid> docID)
        {
            var docIDParameter = docID.HasValue ?
                new ObjectParameter("DocID", docID) :
                new ObjectParameter("DocID", typeof(System.Guid));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetDocCloseShiftPMSpools_Result>("GetDocCloseShiftPMSpools", docIDParameter);
        }
    
        public virtual ObjectResult<GetSpoolRejectionReasons_Result> GetSpoolRejectionReasons()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetSpoolRejectionReasons_Result>("GetSpoolRejectionReasons");
        }
    
        public virtual ObjectResult<Nullable<System.Guid>> GetActiveSourceSpools(Nullable<int> placeID)
        {
            var placeIDParameter = placeID.HasValue ?
                new ObjectParameter("PlaceID", placeID) :
                new ObjectParameter("PlaceID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<System.Guid>>("GetActiveSourceSpools", placeIDParameter);
        }
    
        public virtual ObjectResult<GetProductRelations_Result> GetProductRelations(Nullable<System.Guid> docID)
        {
            var docIDParameter = docID.HasValue ?
                new ObjectParameter("DocID", docID) :
                new ObjectParameter("DocID", typeof(System.Guid));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetProductRelations_Result>("GetProductRelations", docIDParameter);
        }
    
        public virtual ObjectResult<GetNomenclatureFolders_Result1> GetNomenclatureFolders(Nullable<int> placeGroupID)
        {
            var placeGroupIDParameter = placeGroupID.HasValue ?
                new ObjectParameter("PlaceGroupID", placeGroupID) :
                new ObjectParameter("PlaceGroupID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetNomenclatureFolders_Result1>("GetNomenclatureFolders", placeGroupIDParameter);
        }
    
        public virtual ObjectResult<Nullable<int>> GetCharSpoolLayerNumber(Nullable<System.Guid> characteristicID)
        {
            var characteristicIDParameter = characteristicID.HasValue ?
                new ObjectParameter("CharacteristicID", characteristicID) :
                new ObjectParameter("CharacteristicID", typeof(System.Guid));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<int>>("GetCharSpoolLayerNumber", characteristicIDParameter);
        }
    
        public virtual ObjectResult<GetGroupPackSpools_Result> GetGroupPackSpools(Nullable<System.Guid> productID)
        {
            var productIDParameter = productID.HasValue ?
                new ObjectParameter("ProductID", productID) :
                new ObjectParameter("ProductID", typeof(System.Guid));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetGroupPackSpools_Result>("GetGroupPackSpools", productIDParameter);
        }
    
        public virtual int GenerateNewNumbersForDoc(Nullable<System.Guid> docID)
        {
            var docIDParameter = docID.HasValue ?
                new ObjectParameter("DocID", docID) :
                new ObjectParameter("DocID", typeof(System.Guid));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("GenerateNewNumbersForDoc", docIDParameter);
        }
    
        public virtual ObjectResult<GetDocCloseShiftWRGroupPacks_Result> GetDocCloseShiftWRGroupPacks(Nullable<System.Guid> docID)
        {
            var docIDParameter = docID.HasValue ?
                new ObjectParameter("DocID", docID) :
                new ObjectParameter("DocID", typeof(System.Guid));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetDocCloseShiftWRGroupPacks_Result>("GetDocCloseShiftWRGroupPacks", docIDParameter);
        }
    
        public virtual ObjectResult<GetInputNomenclature_Result> GetInputNomenclature(Nullable<System.Guid> nomenclatureID)
        {
            var nomenclatureIDParameter = nomenclatureID.HasValue ?
                new ObjectParameter("NomenclatureID", nomenclatureID) :
                new ObjectParameter("NomenclatureID", typeof(System.Guid));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetInputNomenclature_Result>("GetInputNomenclature", nomenclatureIDParameter);
        }
    
        public virtual ObjectResult<GetProductionTaskByBatchID_Result> GetProductionTaskByBatchID(Nullable<System.Guid> productionTaskBatchID, Nullable<short> placeGroupID)
        {
            var productionTaskBatchIDParameter = productionTaskBatchID.HasValue ?
                new ObjectParameter("ProductionTaskBatchID", productionTaskBatchID) :
                new ObjectParameter("ProductionTaskBatchID", typeof(System.Guid));
    
            var placeGroupIDParameter = placeGroupID.HasValue ?
                new ObjectParameter("PlaceGroupID", placeGroupID) :
                new ObjectParameter("PlaceGroupID", typeof(short));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetProductionTaskByBatchID_Result>("GetProductionTaskByBatchID", productionTaskBatchIDParameter, placeGroupIDParameter);
        }
    
        public virtual ObjectResult<GetProductionTaskBatchSGBProperties_Result> GetProductionTaskBatchSGBProperties(Nullable<System.Guid> productionTaskBatchID)
        {
            var productionTaskBatchIDParameter = productionTaskBatchID.HasValue ?
                new ObjectParameter("ProductionTaskBatchID", productionTaskBatchID) :
                new ObjectParameter("ProductionTaskBatchID", typeof(System.Guid));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetProductionTaskBatchSGBProperties_Result>("GetProductionTaskBatchSGBProperties", productionTaskBatchIDParameter);
        }
    
        public virtual ObjectResult<GetProductionTaskBatchWRProperties_Result> GetProductionTaskBatchWRProperties(Nullable<System.Guid> productionTaskBatchID)
        {
            var productionTaskBatchIDParameter = productionTaskBatchID.HasValue ?
                new ObjectParameter("ProductionTaskBatchID", productionTaskBatchID) :
                new ObjectParameter("ProductionTaskBatchID", typeof(System.Guid));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetProductionTaskBatchWRProperties_Result>("GetProductionTaskBatchWRProperties", productionTaskBatchIDParameter);
        }
    
        public virtual ObjectResult<GetProductionTaskBatchSGBCuttings_Result> GetProductionTaskBatchSGBCuttings(Nullable<System.Guid> productionTaskBatchID)
        {
            var productionTaskBatchIDParameter = productionTaskBatchID.HasValue ?
                new ObjectParameter("ProductionTaskBatchID", productionTaskBatchID) :
                new ObjectParameter("ProductionTaskBatchID", typeof(System.Guid));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetProductionTaskBatchSGBCuttings_Result>("GetProductionTaskBatchSGBCuttings", productionTaskBatchIDParameter);
        }
    
        public virtual ObjectResult<GetProductionTasks_Result4> GetProductionTasks(Nullable<int> batchKindID)
        {
            var batchKindIDParameter = batchKindID.HasValue ?
                new ObjectParameter("BatchKindID", batchKindID) :
                new ObjectParameter("BatchKindID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetProductionTasks_Result4>("GetProductionTasks", batchKindIDParameter);
        }
    
        public virtual ObjectResult<GetProductionTaskBatchProducts_Result> GetProductionTaskBatchProducts(Nullable<System.Guid> productionTaskBatchID)
        {
            var productionTaskBatchIDParameter = productionTaskBatchID.HasValue ?
                new ObjectParameter("ProductionTaskBatchID", productionTaskBatchID) :
                new ObjectParameter("ProductionTaskBatchID", typeof(System.Guid));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetProductionTaskBatchProducts_Result>("GetProductionTaskBatchProducts", productionTaskBatchIDParameter);
        }
    
        public virtual ObjectResult<FindProductionTasks_Result1> FindProductionTasks(Nullable<int> batchKindID, Nullable<byte> productionTaskStateID, Nullable<System.DateTime> dateBegin, Nullable<System.DateTime> dateEnd, string number)
        {
            var batchKindIDParameter = batchKindID.HasValue ?
                new ObjectParameter("BatchKindID", batchKindID) :
                new ObjectParameter("BatchKindID", typeof(int));
    
            var productionTaskStateIDParameter = productionTaskStateID.HasValue ?
                new ObjectParameter("ProductionTaskStateID", productionTaskStateID) :
                new ObjectParameter("ProductionTaskStateID", typeof(byte));
    
            var dateBeginParameter = dateBegin.HasValue ?
                new ObjectParameter("DateBegin", dateBegin) :
                new ObjectParameter("DateBegin", typeof(System.DateTime));
    
            var dateEndParameter = dateEnd.HasValue ?
                new ObjectParameter("DateEnd", dateEnd) :
                new ObjectParameter("DateEnd", typeof(System.DateTime));
    
            var numberParameter = number != null ?
                new ObjectParameter("Number", number) :
                new ObjectParameter("Number", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<FindProductionTasks_Result1>("FindProductionTasks", batchKindIDParameter, productionTaskStateIDParameter, dateBeginParameter, dateEndParameter, numberParameter);
        }
    }
}
