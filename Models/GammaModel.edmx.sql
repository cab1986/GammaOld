
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 08/12/2015 16:49:47
-- Generated from EDMX file: D:\CProjects\Gamma\Gamma\Models\GammaModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [Gamma];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_1CCharacteristicProperties_1CCharacteristics]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[1CCharacteristicProperties] DROP CONSTRAINT [FK_1CCharacteristicProperties_1CCharacteristics];
GO
IF OBJECT_ID(N'[dbo].[FK_1CCharacteristicProperties_1CProperties]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[1CCharacteristicProperties] DROP CONSTRAINT [FK_1CCharacteristicProperties_1CProperties];
GO
IF OBJECT_ID(N'[dbo].[FK_1CCharacteristicProperties_1CValues]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[1CCharacteristicProperties] DROP CONSTRAINT [FK_1CCharacteristicProperties_1CValues];
GO
IF OBJECT_ID(N'[dbo].[FK_1CNomenclature_1CMeasureUnits]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[1CNomenclature] DROP CONSTRAINT [FK_1CNomenclature_1CMeasureUnits];
GO
IF OBJECT_ID(N'[dbo].[FK_1CNomenclature_1CNomenclature]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[1CNomenclature] DROP CONSTRAINT [FK_1CNomenclature_1CNomenclature];
GO
IF OBJECT_ID(N'[dbo].[FK_1CNomenclatureCharacteristics_1CCharacteristics]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[1CNomenclatureCharacteristics] DROP CONSTRAINT [FK_1CNomenclatureCharacteristics_1CCharacteristics];
GO
IF OBJECT_ID(N'[dbo].[FK_1CNomenclatureCharacteristics_1CNomenclature]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[1CNomenclatureCharacteristics] DROP CONSTRAINT [FK_1CNomenclatureCharacteristics_1CNomenclature];
GO
IF OBJECT_ID(N'[dbo].[FK_1CNomenclatureProperties_1CNomenclature]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[1CNomenclatureProperties] DROP CONSTRAINT [FK_1CNomenclatureProperties_1CNomenclature];
GO
IF OBJECT_ID(N'[dbo].[FK_1CNomenclatureProperties_1CProperties]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[1CNomenclatureProperties] DROP CONSTRAINT [FK_1CNomenclatureProperties_1CProperties];
GO
IF OBJECT_ID(N'[dbo].[FK_1CNomenclatureProperties_1CValues]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[1CNomenclatureProperties] DROP CONSTRAINT [FK_1CNomenclatureProperties_1CValues];
GO
IF OBJECT_ID(N'[dbo].[FK_1CPropertyValues_1CProperties]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[1CPropertyValues] DROP CONSTRAINT [FK_1CPropertyValues_1CProperties];
GO
IF OBJECT_ID(N'[dbo].[FK_1CPropertyValues_1CValues]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[1CPropertyValues] DROP CONSTRAINT [FK_1CPropertyValues_1CValues];
GO
IF OBJECT_ID(N'[dbo].[FK_DocMovement_Docs]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DocMovement] DROP CONSTRAINT [FK_DocMovement_Docs];
GO
IF OBJECT_ID(N'[dbo].[FK_DocMovement_Places]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DocMovement] DROP CONSTRAINT [FK_DocMovement_Places];
GO
IF OBJECT_ID(N'[dbo].[FK_DocMovement_Places1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DocMovement] DROP CONSTRAINT [FK_DocMovement_Places1];
GO
IF OBJECT_ID(N'[dbo].[FK_DocProduction_Docs]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DocProduction] DROP CONSTRAINT [FK_DocProduction_Docs];
GO
IF OBJECT_ID(N'[dbo].[FK_DocProduction_DocWithdrawal]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DocProduction] DROP CONSTRAINT [FK_DocProduction_DocWithdrawal];
GO
IF OBJECT_ID(N'[dbo].[FK_DocProduction_Places]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DocProduction] DROP CONSTRAINT [FK_DocProduction_Places];
GO
IF OBJECT_ID(N'[dbo].[FK_DocProducts_Docs]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DocProducts] DROP CONSTRAINT [FK_DocProducts_Docs];
GO
IF OBJECT_ID(N'[dbo].[FK_DocProducts_Products]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DocProducts] DROP CONSTRAINT [FK_DocProducts_Products];
GO
IF OBJECT_ID(N'[dbo].[FK_Docs_DocTypes1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Docs] DROP CONSTRAINT [FK_Docs_DocTypes1];
GO
IF OBJECT_ID(N'[dbo].[FK_Docs_Users]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Docs] DROP CONSTRAINT [FK_Docs_Users];
GO
IF OBJECT_ID(N'[dbo].[FK_DocWithdrawal_Docs1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DocWithdrawal] DROP CONSTRAINT [FK_DocWithdrawal_Docs1];
GO
IF OBJECT_ID(N'[dbo].[FK_DocWithdrawal_Places]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DocWithdrawal] DROP CONSTRAINT [FK_DocWithdrawal_Places];
GO
IF OBJECT_ID(N'[dbo].[FK_ProductGroupPacks_1CCharacteristics]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProductGroupPacks] DROP CONSTRAINT [FK_ProductGroupPacks_1CCharacteristics];
GO
IF OBJECT_ID(N'[dbo].[FK_ProductGroupPacks_1CNomenclature]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProductGroupPacks] DROP CONSTRAINT [FK_ProductGroupPacks_1CNomenclature];
GO
IF OBJECT_ID(N'[dbo].[FK_ProductGroupPacks_Products]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProductGroupPacks] DROP CONSTRAINT [FK_ProductGroupPacks_Products];
GO
IF OBJECT_ID(N'[dbo].[FK_ProductionTaskPlaces_Places]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProductionTaskPlaces] DROP CONSTRAINT [FK_ProductionTaskPlaces_Places];
GO
IF OBJECT_ID(N'[dbo].[FK_ProductionTaskPlaces_ProductionTasks]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProductionTaskPlaces] DROP CONSTRAINT [FK_ProductionTaskPlaces_ProductionTasks];
GO
IF OBJECT_ID(N'[dbo].[FK_ProductionTaskPM_ProductionTasks]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProductionTaskPM] DROP CONSTRAINT [FK_ProductionTaskPM_ProductionTasks];
GO
IF OBJECT_ID(N'[dbo].[FK_ProductionTaskQuantity_1CCharacteristics]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProductionTaskConfig] DROP CONSTRAINT [FK_ProductionTaskQuantity_1CCharacteristics];
GO
IF OBJECT_ID(N'[dbo].[FK_ProductionTaskQuantity_ProductionTasks]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProductionTaskConfig] DROP CONSTRAINT [FK_ProductionTaskQuantity_ProductionTasks];
GO
IF OBJECT_ID(N'[dbo].[FK_ProductionTaskRW_ProductionTasks]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProductionTaskRW] DROP CONSTRAINT [FK_ProductionTaskRW_ProductionTasks];
GO
IF OBJECT_ID(N'[dbo].[FK_ProductionTasks_1CNomenclature]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProductionTasks] DROP CONSTRAINT [FK_ProductionTasks_1CNomenclature];
GO
IF OBJECT_ID(N'[dbo].[FK_ProductionTasks_ProductionTaskStates]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProductionTasks] DROP CONSTRAINT [FK_ProductionTasks_ProductionTaskStates];
GO
IF OBJECT_ID(N'[dbo].[FK_ProductionTaskWR_ProductionTasks]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProductionTaskWR] DROP CONSTRAINT [FK_ProductionTaskWR_ProductionTasks];
GO
IF OBJECT_ID(N'[dbo].[FK_ProductPalletItems_1CCharacteristics]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProductPalletItems] DROP CONSTRAINT [FK_ProductPalletItems_1CCharacteristics];
GO
IF OBJECT_ID(N'[dbo].[FK_ProductPalletItems_1CNomenclature]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProductPalletItems] DROP CONSTRAINT [FK_ProductPalletItems_1CNomenclature];
GO
IF OBJECT_ID(N'[dbo].[FK_ProductPalletItems_ProductPallets]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProductPalletItems] DROP CONSTRAINT [FK_ProductPalletItems_ProductPallets];
GO
IF OBJECT_ID(N'[dbo].[FK_ProductPallets_Products1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProductPallets] DROP CONSTRAINT [FK_ProductPallets_Products1];
GO
IF OBJECT_ID(N'[dbo].[FK_ProductSpools_1CCharacteristics]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProductSpools] DROP CONSTRAINT [FK_ProductSpools_1CCharacteristics];
GO
IF OBJECT_ID(N'[dbo].[FK_ProductSpools_1CNomenclature]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProductSpools] DROP CONSTRAINT [FK_ProductSpools_1CNomenclature];
GO
IF OBJECT_ID(N'[dbo].[FK_ProductSpools_Products]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProductSpools] DROP CONSTRAINT [FK_ProductSpools_Products];
GO
IF OBJECT_ID(N'[dbo].[FK_Rests_Places]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Rests] DROP CONSTRAINT [FK_Rests_Places];
GO
IF OBJECT_ID(N'[dbo].[FK_Rests_Products]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Rests] DROP CONSTRAINT [FK_Rests_Products];
GO
IF OBJECT_ID(N'[dbo].[FK_RolePermits_Roles1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RolePermits] DROP CONSTRAINT [FK_RolePermits_Roles1];
GO
IF OBJECT_ID(N'[dbo].[FK_Users_Places]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Users] DROP CONSTRAINT [FK_Users_Places];
GO
IF OBJECT_ID(N'[dbo].[FK_Users_Roles1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Users] DROP CONSTRAINT [FK_Users_Roles1];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[1CCharacteristicProperties]', 'U') IS NOT NULL
    DROP TABLE [dbo].[1CCharacteristicProperties];
GO
IF OBJECT_ID(N'[dbo].[1CCharacteristics]', 'U') IS NOT NULL
    DROP TABLE [dbo].[1CCharacteristics];
GO
IF OBJECT_ID(N'[dbo].[1CMeasureUnits]', 'U') IS NOT NULL
    DROP TABLE [dbo].[1CMeasureUnits];
GO
IF OBJECT_ID(N'[dbo].[1CNomenclature]', 'U') IS NOT NULL
    DROP TABLE [dbo].[1CNomenclature];
GO
IF OBJECT_ID(N'[dbo].[1CNomenclatureCharacteristics]', 'U') IS NOT NULL
    DROP TABLE [dbo].[1CNomenclatureCharacteristics];
GO
IF OBJECT_ID(N'[dbo].[1CNomenclatureProperties]', 'U') IS NOT NULL
    DROP TABLE [dbo].[1CNomenclatureProperties];
GO
IF OBJECT_ID(N'[dbo].[1CProperties]', 'U') IS NOT NULL
    DROP TABLE [dbo].[1CProperties];
GO
IF OBJECT_ID(N'[dbo].[1CPropertyValues]', 'U') IS NOT NULL
    DROP TABLE [dbo].[1CPropertyValues];
GO
IF OBJECT_ID(N'[dbo].[1CValues]', 'U') IS NOT NULL
    DROP TABLE [dbo].[1CValues];
GO
IF OBJECT_ID(N'[dbo].[DocMovement]', 'U') IS NOT NULL
    DROP TABLE [dbo].[DocMovement];
GO
IF OBJECT_ID(N'[dbo].[DocProduction]', 'U') IS NOT NULL
    DROP TABLE [dbo].[DocProduction];
GO
IF OBJECT_ID(N'[dbo].[DocProducts]', 'U') IS NOT NULL
    DROP TABLE [dbo].[DocProducts];
GO
IF OBJECT_ID(N'[dbo].[Docs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Docs];
GO
IF OBJECT_ID(N'[dbo].[DocTypes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[DocTypes];
GO
IF OBJECT_ID(N'[dbo].[DocWithdrawal]', 'U') IS NOT NULL
    DROP TABLE [dbo].[DocWithdrawal];
GO
IF OBJECT_ID(N'[dbo].[Places]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Places];
GO
IF OBJECT_ID(N'[dbo].[ProductGroupPacks]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ProductGroupPacks];
GO
IF OBJECT_ID(N'[dbo].[ProductionTaskConfig]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ProductionTaskConfig];
GO
IF OBJECT_ID(N'[dbo].[ProductionTaskPlaces]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ProductionTaskPlaces];
GO
IF OBJECT_ID(N'[dbo].[ProductionTaskPM]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ProductionTaskPM];
GO
IF OBJECT_ID(N'[dbo].[ProductionTaskRW]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ProductionTaskRW];
GO
IF OBJECT_ID(N'[dbo].[ProductionTasks]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ProductionTasks];
GO
IF OBJECT_ID(N'[dbo].[ProductionTaskStates]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ProductionTaskStates];
GO
IF OBJECT_ID(N'[dbo].[ProductionTaskWR]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ProductionTaskWR];
GO
IF OBJECT_ID(N'[dbo].[ProductPalletItems]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ProductPalletItems];
GO
IF OBJECT_ID(N'[dbo].[ProductPallets]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ProductPallets];
GO
IF OBJECT_ID(N'[dbo].[Products]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Products];
GO
IF OBJECT_ID(N'[dbo].[ProductSpools]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ProductSpools];
GO
IF OBJECT_ID(N'[dbo].[Rests]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Rests];
GO
IF OBJECT_ID(N'[dbo].[RolePermits]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RolePermits];
GO
IF OBJECT_ID(N'[dbo].[Roles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Roles];
GO
IF OBJECT_ID(N'[dbo].[UserPlaces]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserPlaces];
GO
IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'C1CCharacteristicProperties'
CREATE TABLE [dbo].[C1CCharacteristicProperties] (
    [C1CCharacteristicID] uniqueidentifier  NOT NULL,
    [C1CPropertyID] uniqueidentifier  NOT NULL,
    [C1CValueID] uniqueidentifier  NULL,
    [Value] varchar(255)  NOT NULL,
    [PrintValue] varchar(255)  NULL
);
GO

-- Creating table 'C1CCharacteristics'
CREATE TABLE [dbo].[C1CCharacteristics] (
    [C1CCharacteristicID] uniqueidentifier  NOT NULL,
    [C1CCode] varchar(20)  NOT NULL,
    [Name] varchar(255)  NULL,
    [IsArchive] bit  NULL
);
GO

-- Creating table 'C1CMeasureUnits'
CREATE TABLE [dbo].[C1CMeasureUnits] (
    [C1CMeasureUnitID] uniqueidentifier  NOT NULL,
    [Name] varchar(50)  NULL
);
GO

-- Creating table 'C1CNomenclature'
CREATE TABLE [dbo].[C1CNomenclature] (
    [C1CNomenclatureID] uniqueidentifier  NOT NULL,
    [C1CCode] varchar(20)  NOT NULL,
    [C1CMeasureUnitID] uniqueidentifier  NULL,
    [C1CParentID] uniqueidentifier  NULL,
    [Name] varchar(255)  NULL,
    [IsArchive] bit  NULL,
    [IsFolder] bit  NOT NULL
);
GO

-- Creating table 'C1CNomenclatureProperties'
CREATE TABLE [dbo].[C1CNomenclatureProperties] (
    [C1CNomenclatureID] uniqueidentifier  NOT NULL,
    [C1CPropertyID] uniqueidentifier  NOT NULL,
    [Value] varchar(255)  NULL,
    [C1CValueID] uniqueidentifier  NULL,
    [PrintValue] varchar(255)  NULL
);
GO

-- Creating table 'C1CProperties'
CREATE TABLE [dbo].[C1CProperties] (
    [C1CPropertyID] uniqueidentifier  NOT NULL,
    [C1CCode] varchar(20)  NOT NULL,
    [Name] varchar(255)  NULL,
    [IsCharacteristicProperty] bit  NULL
);
GO

-- Creating table 'C1CValues'
CREATE TABLE [dbo].[C1CValues] (
    [C1CValueID] uniqueidentifier  NOT NULL,
    [C1CCode] varchar(20)  NOT NULL,
    [Value] varchar(255)  NULL,
    [IsArchive] bit  NULL
);
GO

-- Creating table 'ProductGroupPacks'
CREATE TABLE [dbo].[ProductGroupPacks] (
    [ProductID] uniqueidentifier  NOT NULL,
    [C1CNomenclatureID] uniqueidentifier  NULL,
    [C1CCharacteristicID] uniqueidentifier  NULL,
    [ProductKindID] tinyint  NULL,
    [Weight] decimal(10,4)  NULL,
    [GrossWeight] decimal(10,4)  NULL
);
GO

-- Creating table 'ProductionTaskConfig'
CREATE TABLE [dbo].[ProductionTaskConfig] (
    [ProductionTaskID] uniqueidentifier  NOT NULL,
    [C1CCharacteristicID] uniqueidentifier  NOT NULL,
    [Quantity] int  NOT NULL
);
GO

-- Creating table 'ProductionTaskPlaces'
CREATE TABLE [dbo].[ProductionTaskPlaces] (
    [ProductionTaskID] uniqueidentifier  NOT NULL,
    [PlaceID] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'ProductionTaskPM'
CREATE TABLE [dbo].[ProductionTaskPM] (
    [ProductionTaskID] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'ProductionTaskRW'
CREATE TABLE [dbo].[ProductionTaskRW] (
    [ProductionTaskID] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'ProductionTasks'
CREATE TABLE [dbo].[ProductionTasks] (
    [ProductionTaskID] uniqueidentifier  NOT NULL,
    [ProductionTaskStateID] uniqueidentifier  NULL,
    [C1CNomenclatureID] uniqueidentifier  NULL,
    [Quantity] int  NULL,
    [UserID] uniqueidentifier  NULL,
    [Number] varchar(64)  NULL,
    [Date] datetime  NULL,
    [DateBegin] datetime  NULL,
    [DateEnd] datetime  NULL,
    [Comment] varchar(8000)  NULL,
    [IsInPlan] bit  NULL
);
GO

-- Creating table 'ProductionTaskStates'
CREATE TABLE [dbo].[ProductionTaskStates] (
    [ProductionTaskStateID] uniqueidentifier  NOT NULL,
    [Name] varchar(128)  NULL,
    [IsActual] bit  NOT NULL
);
GO

-- Creating table 'ProductionTaskWR'
CREATE TABLE [dbo].[ProductionTaskWR] (
    [ProductionTaskID] uniqueidentifier  NOT NULL,
    [NumFilmLayers] tinyint  NULL,
    [IsWithCarton] bit  NULL,
    [IsEndProtected] bit  NULL,
    [GroupPackConfig] varchar(8000)  NULL
);
GO

-- Creating table 'ProductPalletItems'
CREATE TABLE [dbo].[ProductPalletItems] (
    [ProductPalletItemID] uniqueidentifier  NOT NULL,
    [ProductID] uniqueidentifier  NULL,
    [C1CNomenclatureID] uniqueidentifier  NULL,
    [C1CCharacteristicID] uniqueidentifier  NULL,
    [Quantity] int  NULL
);
GO

-- Creating table 'ProductPallets'
CREATE TABLE [dbo].[ProductPallets] (
    [ProductID] uniqueidentifier  NOT NULL,
    [ProductKindID] tinyint  NULL
);
GO

-- Creating table 'Products'
CREATE TABLE [dbo].[Products] (
    [ProductID] uniqueidentifier  NOT NULL,
    [Number] varchar(20)  NULL,
    [BarCode] varchar(20)  NULL,
    [ProductKindID] tinyint  NULL,
    [Comment] varchar(8000)  NULL
);
GO

-- Creating table 'ProductSpools'
CREATE TABLE [dbo].[ProductSpools] (
    [ProductID] uniqueidentifier  NOT NULL,
    [C1CNomenclatureID] uniqueidentifier  NULL,
    [C1CCharacteristicID] uniqueidentifier  NULL,
    [ProductKindID] tinyint  NULL,
    [RealFormat] int  NULL,
    [Diameter] decimal(10,4)  NOT NULL,
    [Weight] decimal(10,4)  NOT NULL,
    [Length] int  NULL,
    [RealBasisWeight] decimal(10,4)  NULL,
    [BreakNumber] tinyint  NULL
);
GO

-- Creating table 'UserPlaces'
CREATE TABLE [dbo].[UserPlaces] (
    [UserID] int  NOT NULL,
    [PlaceID] bigint  NOT NULL,
    [Mark] tinyint  NOT NULL
);
GO

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [UserID] uniqueidentifier  NOT NULL,
    [RoleID] uniqueidentifier  NULL,
    [Login] varchar(50)  NOT NULL,
    [Name] varchar(200)  NULL,
    [Post] varchar(200)  NULL,
    [PassChange] datetime  NULL,
    [PassMustChange] bit  NULL,
    [BlockBegin] datetime  NULL,
    [BlockEnd] datetime  NULL,
    [IsSignedOff] bit  NOT NULL,
    [ChkReport] bit  NOT NULL,
    [PlaceID] uniqueidentifier  NULL,
    [Exports] bit  NOT NULL,
    [ShowUserKind] tinyint  NOT NULL,
    [DBAdmin] bit  NOT NULL,
    [ShiftID] tinyint  NULL
);
GO

-- Creating table 'DocMovement'
CREATE TABLE [dbo].[DocMovement] (
    [DocID] uniqueidentifier  NOT NULL,
    [InPlaceID] uniqueidentifier  NULL,
    [OutPlaceID] uniqueidentifier  NULL
);
GO

-- Creating table 'DocProduction'
CREATE TABLE [dbo].[DocProduction] (
    [DocID] uniqueidentifier  NOT NULL,
    [InPlaceID] uniqueidentifier  NULL,
    [DocWithdrawalID] uniqueidentifier  NULL
);
GO

-- Creating table 'DocProducts'
CREATE TABLE [dbo].[DocProducts] (
    [DocID] uniqueidentifier  NOT NULL,
    [ProductID] uniqueidentifier  NOT NULL,
    [ProductStateID] tinyint  NULL,
    [IsInConfirmed] bit  NULL,
    [IsOutConfirmed] bit  NULL
);
GO

-- Creating table 'Docs'
CREATE TABLE [dbo].[Docs] (
    [DocID] uniqueidentifier  NOT NULL,
    [Number] varchar(50)  NULL,
    [IsConfirmed] bit  NULL,
    [DocTypeID] uniqueidentifier  NULL,
    [UserID] uniqueidentifier  NULL,
    [Date] datetime  NULL,
    [Comment] varchar(8000)  NULL
);
GO

-- Creating table 'DocTypes'
CREATE TABLE [dbo].[DocTypes] (
    [DocTypeID] uniqueidentifier  NOT NULL,
    [Name] varchar(255)  NOT NULL
);
GO

-- Creating table 'DocWithdrawal'
CREATE TABLE [dbo].[DocWithdrawal] (
    [DocID] uniqueidentifier  NOT NULL,
    [OutPlaceID] uniqueidentifier  NULL
);
GO

-- Creating table 'Places'
CREATE TABLE [dbo].[Places] (
    [PlaceID] uniqueidentifier  NOT NULL,
    [Name] varchar(64)  NULL,
    [NameEng] varchar(16)  NULL,
    [DepartmentID] smallint  NOT NULL,
    [PlaceGroupID] smallint  NULL,
    [IsWarehouse] bit  NULL
);
GO

-- Creating table 'Rests'
CREATE TABLE [dbo].[Rests] (
    [ProductID] uniqueidentifier  NOT NULL,
    [Quantity] tinyint  NULL,
    [PlaceID] uniqueidentifier  NULL
);
GO

-- Creating table 'RolePermits'
CREATE TABLE [dbo].[RolePermits] (
    [RoleID] uniqueidentifier  NOT NULL,
    [PermitID] uniqueidentifier  NOT NULL,
    [Mark] tinyint  NOT NULL
);
GO

-- Creating table 'Roles'
CREATE TABLE [dbo].[Roles] (
    [RoleID] uniqueidentifier  NOT NULL,
    [Name] varchar(32)  NOT NULL,
    [Comment] varchar(8000)  NULL,
    [IsDeleted] bit  NULL
);
GO

-- Creating table 'C1CNomenclatureCharacteristics'
CREATE TABLE [dbo].[C1CNomenclatureCharacteristics] (
    [C1CCharacteristics_C1CCharacteristicID] uniqueidentifier  NOT NULL,
    [C1CNomenclature_C1CNomenclatureID] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'C1CPropertyValues'
CREATE TABLE [dbo].[C1CPropertyValues] (
    [C1CProperties_C1CPropertyID] uniqueidentifier  NOT NULL,
    [C1CValues_C1CValueID] uniqueidentifier  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [C1CCharacteristicID], [C1CPropertyID] in table 'C1CCharacteristicProperties'
ALTER TABLE [dbo].[C1CCharacteristicProperties]
ADD CONSTRAINT [PK_C1CCharacteristicProperties]
    PRIMARY KEY CLUSTERED ([C1CCharacteristicID], [C1CPropertyID] ASC);
GO

-- Creating primary key on [C1CCharacteristicID] in table 'C1CCharacteristics'
ALTER TABLE [dbo].[C1CCharacteristics]
ADD CONSTRAINT [PK_C1CCharacteristics]
    PRIMARY KEY CLUSTERED ([C1CCharacteristicID] ASC);
GO

-- Creating primary key on [C1CMeasureUnitID] in table 'C1CMeasureUnits'
ALTER TABLE [dbo].[C1CMeasureUnits]
ADD CONSTRAINT [PK_C1CMeasureUnits]
    PRIMARY KEY CLUSTERED ([C1CMeasureUnitID] ASC);
GO

-- Creating primary key on [C1CNomenclatureID] in table 'C1CNomenclature'
ALTER TABLE [dbo].[C1CNomenclature]
ADD CONSTRAINT [PK_C1CNomenclature]
    PRIMARY KEY CLUSTERED ([C1CNomenclatureID] ASC);
GO

-- Creating primary key on [C1CNomenclatureID], [C1CPropertyID] in table 'C1CNomenclatureProperties'
ALTER TABLE [dbo].[C1CNomenclatureProperties]
ADD CONSTRAINT [PK_C1CNomenclatureProperties]
    PRIMARY KEY CLUSTERED ([C1CNomenclatureID], [C1CPropertyID] ASC);
GO

-- Creating primary key on [C1CPropertyID] in table 'C1CProperties'
ALTER TABLE [dbo].[C1CProperties]
ADD CONSTRAINT [PK_C1CProperties]
    PRIMARY KEY CLUSTERED ([C1CPropertyID] ASC);
GO

-- Creating primary key on [C1CValueID] in table 'C1CValues'
ALTER TABLE [dbo].[C1CValues]
ADD CONSTRAINT [PK_C1CValues]
    PRIMARY KEY CLUSTERED ([C1CValueID] ASC);
GO

-- Creating primary key on [ProductID] in table 'ProductGroupPacks'
ALTER TABLE [dbo].[ProductGroupPacks]
ADD CONSTRAINT [PK_ProductGroupPacks]
    PRIMARY KEY CLUSTERED ([ProductID] ASC);
GO

-- Creating primary key on [ProductionTaskID], [C1CCharacteristicID] in table 'ProductionTaskConfig'
ALTER TABLE [dbo].[ProductionTaskConfig]
ADD CONSTRAINT [PK_ProductionTaskConfig]
    PRIMARY KEY CLUSTERED ([ProductionTaskID], [C1CCharacteristicID] ASC);
GO

-- Creating primary key on [ProductionTaskID], [PlaceID] in table 'ProductionTaskPlaces'
ALTER TABLE [dbo].[ProductionTaskPlaces]
ADD CONSTRAINT [PK_ProductionTaskPlaces]
    PRIMARY KEY CLUSTERED ([ProductionTaskID], [PlaceID] ASC);
GO

-- Creating primary key on [ProductionTaskID] in table 'ProductionTaskPM'
ALTER TABLE [dbo].[ProductionTaskPM]
ADD CONSTRAINT [PK_ProductionTaskPM]
    PRIMARY KEY CLUSTERED ([ProductionTaskID] ASC);
GO

-- Creating primary key on [ProductionTaskID] in table 'ProductionTaskRW'
ALTER TABLE [dbo].[ProductionTaskRW]
ADD CONSTRAINT [PK_ProductionTaskRW]
    PRIMARY KEY CLUSTERED ([ProductionTaskID] ASC);
GO

-- Creating primary key on [ProductionTaskID] in table 'ProductionTasks'
ALTER TABLE [dbo].[ProductionTasks]
ADD CONSTRAINT [PK_ProductionTasks]
    PRIMARY KEY CLUSTERED ([ProductionTaskID] ASC);
GO

-- Creating primary key on [ProductionTaskStateID] in table 'ProductionTaskStates'
ALTER TABLE [dbo].[ProductionTaskStates]
ADD CONSTRAINT [PK_ProductionTaskStates]
    PRIMARY KEY CLUSTERED ([ProductionTaskStateID] ASC);
GO

-- Creating primary key on [ProductionTaskID] in table 'ProductionTaskWR'
ALTER TABLE [dbo].[ProductionTaskWR]
ADD CONSTRAINT [PK_ProductionTaskWR]
    PRIMARY KEY CLUSTERED ([ProductionTaskID] ASC);
GO

-- Creating primary key on [ProductPalletItemID] in table 'ProductPalletItems'
ALTER TABLE [dbo].[ProductPalletItems]
ADD CONSTRAINT [PK_ProductPalletItems]
    PRIMARY KEY CLUSTERED ([ProductPalletItemID] ASC);
GO

-- Creating primary key on [ProductID] in table 'ProductPallets'
ALTER TABLE [dbo].[ProductPallets]
ADD CONSTRAINT [PK_ProductPallets]
    PRIMARY KEY CLUSTERED ([ProductID] ASC);
GO

-- Creating primary key on [ProductID] in table 'Products'
ALTER TABLE [dbo].[Products]
ADD CONSTRAINT [PK_Products]
    PRIMARY KEY CLUSTERED ([ProductID] ASC);
GO

-- Creating primary key on [ProductID] in table 'ProductSpools'
ALTER TABLE [dbo].[ProductSpools]
ADD CONSTRAINT [PK_ProductSpools]
    PRIMARY KEY CLUSTERED ([ProductID] ASC);
GO

-- Creating primary key on [UserID], [PlaceID] in table 'UserPlaces'
ALTER TABLE [dbo].[UserPlaces]
ADD CONSTRAINT [PK_UserPlaces]
    PRIMARY KEY CLUSTERED ([UserID], [PlaceID] ASC);
GO

-- Creating primary key on [UserID] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([UserID] ASC);
GO

-- Creating primary key on [DocID] in table 'DocMovement'
ALTER TABLE [dbo].[DocMovement]
ADD CONSTRAINT [PK_DocMovement]
    PRIMARY KEY CLUSTERED ([DocID] ASC);
GO

-- Creating primary key on [DocID] in table 'DocProduction'
ALTER TABLE [dbo].[DocProduction]
ADD CONSTRAINT [PK_DocProduction]
    PRIMARY KEY CLUSTERED ([DocID] ASC);
GO

-- Creating primary key on [DocID], [ProductID] in table 'DocProducts'
ALTER TABLE [dbo].[DocProducts]
ADD CONSTRAINT [PK_DocProducts]
    PRIMARY KEY CLUSTERED ([DocID], [ProductID] ASC);
GO

-- Creating primary key on [DocID] in table 'Docs'
ALTER TABLE [dbo].[Docs]
ADD CONSTRAINT [PK_Docs]
    PRIMARY KEY CLUSTERED ([DocID] ASC);
GO

-- Creating primary key on [DocTypeID] in table 'DocTypes'
ALTER TABLE [dbo].[DocTypes]
ADD CONSTRAINT [PK_DocTypes]
    PRIMARY KEY CLUSTERED ([DocTypeID] ASC);
GO

-- Creating primary key on [DocID] in table 'DocWithdrawal'
ALTER TABLE [dbo].[DocWithdrawal]
ADD CONSTRAINT [PK_DocWithdrawal]
    PRIMARY KEY CLUSTERED ([DocID] ASC);
GO

-- Creating primary key on [PlaceID] in table 'Places'
ALTER TABLE [dbo].[Places]
ADD CONSTRAINT [PK_Places]
    PRIMARY KEY CLUSTERED ([PlaceID] ASC);
GO

-- Creating primary key on [ProductID] in table 'Rests'
ALTER TABLE [dbo].[Rests]
ADD CONSTRAINT [PK_Rests]
    PRIMARY KEY CLUSTERED ([ProductID] ASC);
GO

-- Creating primary key on [RoleID], [PermitID] in table 'RolePermits'
ALTER TABLE [dbo].[RolePermits]
ADD CONSTRAINT [PK_RolePermits]
    PRIMARY KEY CLUSTERED ([RoleID], [PermitID] ASC);
GO

-- Creating primary key on [RoleID] in table 'Roles'
ALTER TABLE [dbo].[Roles]
ADD CONSTRAINT [PK_Roles]
    PRIMARY KEY CLUSTERED ([RoleID] ASC);
GO

-- Creating primary key on [C1CCharacteristics_C1CCharacteristicID], [C1CNomenclature_C1CNomenclatureID] in table 'C1CNomenclatureCharacteristics'
ALTER TABLE [dbo].[C1CNomenclatureCharacteristics]
ADD CONSTRAINT [PK_C1CNomenclatureCharacteristics]
    PRIMARY KEY CLUSTERED ([C1CCharacteristics_C1CCharacteristicID], [C1CNomenclature_C1CNomenclatureID] ASC);
GO

-- Creating primary key on [C1CProperties_C1CPropertyID], [C1CValues_C1CValueID] in table 'C1CPropertyValues'
ALTER TABLE [dbo].[C1CPropertyValues]
ADD CONSTRAINT [PK_C1CPropertyValues]
    PRIMARY KEY CLUSTERED ([C1CProperties_C1CPropertyID], [C1CValues_C1CValueID] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [C1CCharacteristicID] in table 'C1CCharacteristicProperties'
ALTER TABLE [dbo].[C1CCharacteristicProperties]
ADD CONSTRAINT [FK_1CCharacteristicProperties_1CCharacteristics]
    FOREIGN KEY ([C1CCharacteristicID])
    REFERENCES [dbo].[C1CCharacteristics]
        ([C1CCharacteristicID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [C1CPropertyID] in table 'C1CCharacteristicProperties'
ALTER TABLE [dbo].[C1CCharacteristicProperties]
ADD CONSTRAINT [FK_1CCharacteristicProperties_1CProperties]
    FOREIGN KEY ([C1CPropertyID])
    REFERENCES [dbo].[C1CProperties]
        ([C1CPropertyID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_1CCharacteristicProperties_1CProperties'
CREATE INDEX [IX_FK_1CCharacteristicProperties_1CProperties]
ON [dbo].[C1CCharacteristicProperties]
    ([C1CPropertyID]);
GO

-- Creating foreign key on [C1CValueID] in table 'C1CCharacteristicProperties'
ALTER TABLE [dbo].[C1CCharacteristicProperties]
ADD CONSTRAINT [FK_1CCharacteristicProperties_1CValues]
    FOREIGN KEY ([C1CValueID])
    REFERENCES [dbo].[C1CValues]
        ([C1CValueID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_1CCharacteristicProperties_1CValues'
CREATE INDEX [IX_FK_1CCharacteristicProperties_1CValues]
ON [dbo].[C1CCharacteristicProperties]
    ([C1CValueID]);
GO

-- Creating foreign key on [C1CCharacteristicID] in table 'ProductGroupPacks'
ALTER TABLE [dbo].[ProductGroupPacks]
ADD CONSTRAINT [FK_ProductGroupPacks_1CCharacteristics]
    FOREIGN KEY ([C1CCharacteristicID])
    REFERENCES [dbo].[C1CCharacteristics]
        ([C1CCharacteristicID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProductGroupPacks_1CCharacteristics'
CREATE INDEX [IX_FK_ProductGroupPacks_1CCharacteristics]
ON [dbo].[ProductGroupPacks]
    ([C1CCharacteristicID]);
GO

-- Creating foreign key on [C1CCharacteristicID] in table 'ProductionTaskConfig'
ALTER TABLE [dbo].[ProductionTaskConfig]
ADD CONSTRAINT [FK_ProductionTaskQuantity_1CCharacteristics]
    FOREIGN KEY ([C1CCharacteristicID])
    REFERENCES [dbo].[C1CCharacteristics]
        ([C1CCharacteristicID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProductionTaskQuantity_1CCharacteristics'
CREATE INDEX [IX_FK_ProductionTaskQuantity_1CCharacteristics]
ON [dbo].[ProductionTaskConfig]
    ([C1CCharacteristicID]);
GO

-- Creating foreign key on [C1CCharacteristicID] in table 'ProductPalletItems'
ALTER TABLE [dbo].[ProductPalletItems]
ADD CONSTRAINT [FK_ProductPalletItems_1CCharacteristics]
    FOREIGN KEY ([C1CCharacteristicID])
    REFERENCES [dbo].[C1CCharacteristics]
        ([C1CCharacteristicID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProductPalletItems_1CCharacteristics'
CREATE INDEX [IX_FK_ProductPalletItems_1CCharacteristics]
ON [dbo].[ProductPalletItems]
    ([C1CCharacteristicID]);
GO

-- Creating foreign key on [C1CCharacteristicID] in table 'ProductSpools'
ALTER TABLE [dbo].[ProductSpools]
ADD CONSTRAINT [FK_ProductSpools_1CCharacteristics]
    FOREIGN KEY ([C1CCharacteristicID])
    REFERENCES [dbo].[C1CCharacteristics]
        ([C1CCharacteristicID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProductSpools_1CCharacteristics'
CREATE INDEX [IX_FK_ProductSpools_1CCharacteristics]
ON [dbo].[ProductSpools]
    ([C1CCharacteristicID]);
GO

-- Creating foreign key on [C1CMeasureUnitID] in table 'C1CNomenclature'
ALTER TABLE [dbo].[C1CNomenclature]
ADD CONSTRAINT [FK_1CNomenclature_1CMeasureUnits]
    FOREIGN KEY ([C1CMeasureUnitID])
    REFERENCES [dbo].[C1CMeasureUnits]
        ([C1CMeasureUnitID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_1CNomenclature_1CMeasureUnits'
CREATE INDEX [IX_FK_1CNomenclature_1CMeasureUnits]
ON [dbo].[C1CNomenclature]
    ([C1CMeasureUnitID]);
GO

-- Creating foreign key on [C1CParentID] in table 'C1CNomenclature'
ALTER TABLE [dbo].[C1CNomenclature]
ADD CONSTRAINT [FK_1CNomenclature_1CNomenclature]
    FOREIGN KEY ([C1CParentID])
    REFERENCES [dbo].[C1CNomenclature]
        ([C1CNomenclatureID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_1CNomenclature_1CNomenclature'
CREATE INDEX [IX_FK_1CNomenclature_1CNomenclature]
ON [dbo].[C1CNomenclature]
    ([C1CParentID]);
GO

-- Creating foreign key on [C1CNomenclatureID] in table 'C1CNomenclatureProperties'
ALTER TABLE [dbo].[C1CNomenclatureProperties]
ADD CONSTRAINT [FK_1CNomenclatureProperties_1CNomenclature]
    FOREIGN KEY ([C1CNomenclatureID])
    REFERENCES [dbo].[C1CNomenclature]
        ([C1CNomenclatureID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [C1CNomenclatureID] in table 'ProductGroupPacks'
ALTER TABLE [dbo].[ProductGroupPacks]
ADD CONSTRAINT [FK_ProductGroupPacks_1CNomenclature]
    FOREIGN KEY ([C1CNomenclatureID])
    REFERENCES [dbo].[C1CNomenclature]
        ([C1CNomenclatureID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProductGroupPacks_1CNomenclature'
CREATE INDEX [IX_FK_ProductGroupPacks_1CNomenclature]
ON [dbo].[ProductGroupPacks]
    ([C1CNomenclatureID]);
GO

-- Creating foreign key on [C1CNomenclatureID] in table 'ProductionTasks'
ALTER TABLE [dbo].[ProductionTasks]
ADD CONSTRAINT [FK_ProductionTasks_1CNomenclature]
    FOREIGN KEY ([C1CNomenclatureID])
    REFERENCES [dbo].[C1CNomenclature]
        ([C1CNomenclatureID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProductionTasks_1CNomenclature'
CREATE INDEX [IX_FK_ProductionTasks_1CNomenclature]
ON [dbo].[ProductionTasks]
    ([C1CNomenclatureID]);
GO

-- Creating foreign key on [C1CNomenclatureID] in table 'ProductPalletItems'
ALTER TABLE [dbo].[ProductPalletItems]
ADD CONSTRAINT [FK_ProductPalletItems_1CNomenclature]
    FOREIGN KEY ([C1CNomenclatureID])
    REFERENCES [dbo].[C1CNomenclature]
        ([C1CNomenclatureID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProductPalletItems_1CNomenclature'
CREATE INDEX [IX_FK_ProductPalletItems_1CNomenclature]
ON [dbo].[ProductPalletItems]
    ([C1CNomenclatureID]);
GO

-- Creating foreign key on [C1CNomenclatureID] in table 'ProductSpools'
ALTER TABLE [dbo].[ProductSpools]
ADD CONSTRAINT [FK_ProductSpools_1CNomenclature]
    FOREIGN KEY ([C1CNomenclatureID])
    REFERENCES [dbo].[C1CNomenclature]
        ([C1CNomenclatureID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProductSpools_1CNomenclature'
CREATE INDEX [IX_FK_ProductSpools_1CNomenclature]
ON [dbo].[ProductSpools]
    ([C1CNomenclatureID]);
GO

-- Creating foreign key on [C1CPropertyID] in table 'C1CNomenclatureProperties'
ALTER TABLE [dbo].[C1CNomenclatureProperties]
ADD CONSTRAINT [FK_1CNomenclatureProperties_1CProperties]
    FOREIGN KEY ([C1CPropertyID])
    REFERENCES [dbo].[C1CProperties]
        ([C1CPropertyID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_1CNomenclatureProperties_1CProperties'
CREATE INDEX [IX_FK_1CNomenclatureProperties_1CProperties]
ON [dbo].[C1CNomenclatureProperties]
    ([C1CPropertyID]);
GO

-- Creating foreign key on [C1CValueID] in table 'C1CNomenclatureProperties'
ALTER TABLE [dbo].[C1CNomenclatureProperties]
ADD CONSTRAINT [FK_1CNomenclatureProperties_1CValues]
    FOREIGN KEY ([C1CValueID])
    REFERENCES [dbo].[C1CValues]
        ([C1CValueID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_1CNomenclatureProperties_1CValues'
CREATE INDEX [IX_FK_1CNomenclatureProperties_1CValues]
ON [dbo].[C1CNomenclatureProperties]
    ([C1CValueID]);
GO

-- Creating foreign key on [ProductID] in table 'ProductGroupPacks'
ALTER TABLE [dbo].[ProductGroupPacks]
ADD CONSTRAINT [FK_ProductGroupPacks_Products]
    FOREIGN KEY ([ProductID])
    REFERENCES [dbo].[Products]
        ([ProductID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [ProductionTaskID] in table 'ProductionTaskConfig'
ALTER TABLE [dbo].[ProductionTaskConfig]
ADD CONSTRAINT [FK_ProductionTaskQuantity_ProductionTasks]
    FOREIGN KEY ([ProductionTaskID])
    REFERENCES [dbo].[ProductionTasks]
        ([ProductionTaskID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [ProductionTaskID] in table 'ProductionTaskPlaces'
ALTER TABLE [dbo].[ProductionTaskPlaces]
ADD CONSTRAINT [FK_ProductionTaskPlaces_ProductionTasks]
    FOREIGN KEY ([ProductionTaskID])
    REFERENCES [dbo].[ProductionTasks]
        ([ProductionTaskID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [ProductionTaskID] in table 'ProductionTaskPM'
ALTER TABLE [dbo].[ProductionTaskPM]
ADD CONSTRAINT [FK_ProductionTaskPM_ProductionTasks]
    FOREIGN KEY ([ProductionTaskID])
    REFERENCES [dbo].[ProductionTasks]
        ([ProductionTaskID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [ProductionTaskID] in table 'ProductionTaskRW'
ALTER TABLE [dbo].[ProductionTaskRW]
ADD CONSTRAINT [FK_ProductionTaskRW_ProductionTasks]
    FOREIGN KEY ([ProductionTaskID])
    REFERENCES [dbo].[ProductionTasks]
        ([ProductionTaskID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [ProductionTaskStateID] in table 'ProductionTasks'
ALTER TABLE [dbo].[ProductionTasks]
ADD CONSTRAINT [FK_ProductionTasks_ProductionTaskStates]
    FOREIGN KEY ([ProductionTaskStateID])
    REFERENCES [dbo].[ProductionTaskStates]
        ([ProductionTaskStateID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProductionTasks_ProductionTaskStates'
CREATE INDEX [IX_FK_ProductionTasks_ProductionTaskStates]
ON [dbo].[ProductionTasks]
    ([ProductionTaskStateID]);
GO

-- Creating foreign key on [ProductionTaskID] in table 'ProductionTaskWR'
ALTER TABLE [dbo].[ProductionTaskWR]
ADD CONSTRAINT [FK_ProductionTaskWR_ProductionTasks]
    FOREIGN KEY ([ProductionTaskID])
    REFERENCES [dbo].[ProductionTasks]
        ([ProductionTaskID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [ProductID] in table 'ProductPalletItems'
ALTER TABLE [dbo].[ProductPalletItems]
ADD CONSTRAINT [FK_ProductPalletItems_ProductPallets]
    FOREIGN KEY ([ProductID])
    REFERENCES [dbo].[ProductPallets]
        ([ProductID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProductPalletItems_ProductPallets'
CREATE INDEX [IX_FK_ProductPalletItems_ProductPallets]
ON [dbo].[ProductPalletItems]
    ([ProductID]);
GO

-- Creating foreign key on [ProductID] in table 'ProductPallets'
ALTER TABLE [dbo].[ProductPallets]
ADD CONSTRAINT [FK_ProductPallets_Products1]
    FOREIGN KEY ([ProductID])
    REFERENCES [dbo].[Products]
        ([ProductID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [ProductID] in table 'ProductSpools'
ALTER TABLE [dbo].[ProductSpools]
ADD CONSTRAINT [FK_ProductSpools_Products]
    FOREIGN KEY ([ProductID])
    REFERENCES [dbo].[Products]
        ([ProductID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [C1CCharacteristics_C1CCharacteristicID] in table 'C1CNomenclatureCharacteristics'
ALTER TABLE [dbo].[C1CNomenclatureCharacteristics]
ADD CONSTRAINT [FK_C1CNomenclatureCharacteristics_C1CCharacteristics]
    FOREIGN KEY ([C1CCharacteristics_C1CCharacteristicID])
    REFERENCES [dbo].[C1CCharacteristics]
        ([C1CCharacteristicID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [C1CNomenclature_C1CNomenclatureID] in table 'C1CNomenclatureCharacteristics'
ALTER TABLE [dbo].[C1CNomenclatureCharacteristics]
ADD CONSTRAINT [FK_C1CNomenclatureCharacteristics_C1CNomenclature]
    FOREIGN KEY ([C1CNomenclature_C1CNomenclatureID])
    REFERENCES [dbo].[C1CNomenclature]
        ([C1CNomenclatureID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_C1CNomenclatureCharacteristics_C1CNomenclature'
CREATE INDEX [IX_FK_C1CNomenclatureCharacteristics_C1CNomenclature]
ON [dbo].[C1CNomenclatureCharacteristics]
    ([C1CNomenclature_C1CNomenclatureID]);
GO

-- Creating foreign key on [C1CProperties_C1CPropertyID] in table 'C1CPropertyValues'
ALTER TABLE [dbo].[C1CPropertyValues]
ADD CONSTRAINT [FK_C1CPropertyValues_C1CProperties]
    FOREIGN KEY ([C1CProperties_C1CPropertyID])
    REFERENCES [dbo].[C1CProperties]
        ([C1CPropertyID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [C1CValues_C1CValueID] in table 'C1CPropertyValues'
ALTER TABLE [dbo].[C1CPropertyValues]
ADD CONSTRAINT [FK_C1CPropertyValues_C1CValues]
    FOREIGN KEY ([C1CValues_C1CValueID])
    REFERENCES [dbo].[C1CValues]
        ([C1CValueID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_C1CPropertyValues_C1CValues'
CREATE INDEX [IX_FK_C1CPropertyValues_C1CValues]
ON [dbo].[C1CPropertyValues]
    ([C1CValues_C1CValueID]);
GO

-- Creating foreign key on [DocID] in table 'DocMovement'
ALTER TABLE [dbo].[DocMovement]
ADD CONSTRAINT [FK_DocMovement_Docs]
    FOREIGN KEY ([DocID])
    REFERENCES [dbo].[Docs]
        ([DocID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [InPlaceID] in table 'DocMovement'
ALTER TABLE [dbo].[DocMovement]
ADD CONSTRAINT [FK_DocMovement_Places]
    FOREIGN KEY ([InPlaceID])
    REFERENCES [dbo].[Places]
        ([PlaceID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_DocMovement_Places'
CREATE INDEX [IX_FK_DocMovement_Places]
ON [dbo].[DocMovement]
    ([InPlaceID]);
GO

-- Creating foreign key on [OutPlaceID] in table 'DocMovement'
ALTER TABLE [dbo].[DocMovement]
ADD CONSTRAINT [FK_DocMovement_Places1]
    FOREIGN KEY ([OutPlaceID])
    REFERENCES [dbo].[Places]
        ([PlaceID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_DocMovement_Places1'
CREATE INDEX [IX_FK_DocMovement_Places1]
ON [dbo].[DocMovement]
    ([OutPlaceID]);
GO

-- Creating foreign key on [DocID] in table 'DocProduction'
ALTER TABLE [dbo].[DocProduction]
ADD CONSTRAINT [FK_DocProduction_Docs]
    FOREIGN KEY ([DocID])
    REFERENCES [dbo].[Docs]
        ([DocID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [DocWithdrawalID] in table 'DocProduction'
ALTER TABLE [dbo].[DocProduction]
ADD CONSTRAINT [FK_DocProduction_DocWithdrawal]
    FOREIGN KEY ([DocWithdrawalID])
    REFERENCES [dbo].[DocWithdrawal]
        ([DocID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_DocProduction_DocWithdrawal'
CREATE INDEX [IX_FK_DocProduction_DocWithdrawal]
ON [dbo].[DocProduction]
    ([DocWithdrawalID]);
GO

-- Creating foreign key on [InPlaceID] in table 'DocProduction'
ALTER TABLE [dbo].[DocProduction]
ADD CONSTRAINT [FK_DocProduction_Places]
    FOREIGN KEY ([InPlaceID])
    REFERENCES [dbo].[Places]
        ([PlaceID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_DocProduction_Places'
CREATE INDEX [IX_FK_DocProduction_Places]
ON [dbo].[DocProduction]
    ([InPlaceID]);
GO

-- Creating foreign key on [DocID] in table 'DocProducts'
ALTER TABLE [dbo].[DocProducts]
ADD CONSTRAINT [FK_DocProducts_Docs]
    FOREIGN KEY ([DocID])
    REFERENCES [dbo].[Docs]
        ([DocID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [ProductID] in table 'DocProducts'
ALTER TABLE [dbo].[DocProducts]
ADD CONSTRAINT [FK_DocProducts_Products]
    FOREIGN KEY ([ProductID])
    REFERENCES [dbo].[Products]
        ([ProductID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_DocProducts_Products'
CREATE INDEX [IX_FK_DocProducts_Products]
ON [dbo].[DocProducts]
    ([ProductID]);
GO

-- Creating foreign key on [DocTypeID] in table 'Docs'
ALTER TABLE [dbo].[Docs]
ADD CONSTRAINT [FK_Docs_DocTypes1]
    FOREIGN KEY ([DocTypeID])
    REFERENCES [dbo].[DocTypes]
        ([DocTypeID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Docs_DocTypes1'
CREATE INDEX [IX_FK_Docs_DocTypes1]
ON [dbo].[Docs]
    ([DocTypeID]);
GO

-- Creating foreign key on [UserID] in table 'Docs'
ALTER TABLE [dbo].[Docs]
ADD CONSTRAINT [FK_Docs_Users]
    FOREIGN KEY ([UserID])
    REFERENCES [dbo].[Users]
        ([UserID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Docs_Users'
CREATE INDEX [IX_FK_Docs_Users]
ON [dbo].[Docs]
    ([UserID]);
GO

-- Creating foreign key on [DocID] in table 'DocWithdrawal'
ALTER TABLE [dbo].[DocWithdrawal]
ADD CONSTRAINT [FK_DocWithdrawal_Docs1]
    FOREIGN KEY ([DocID])
    REFERENCES [dbo].[Docs]
        ([DocID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [OutPlaceID] in table 'DocWithdrawal'
ALTER TABLE [dbo].[DocWithdrawal]
ADD CONSTRAINT [FK_DocWithdrawal_Places]
    FOREIGN KEY ([OutPlaceID])
    REFERENCES [dbo].[Places]
        ([PlaceID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_DocWithdrawal_Places'
CREATE INDEX [IX_FK_DocWithdrawal_Places]
ON [dbo].[DocWithdrawal]
    ([OutPlaceID]);
GO

-- Creating foreign key on [ProductionTaskID] in table 'ProductionTaskPlaces'
ALTER TABLE [dbo].[ProductionTaskPlaces]
ADD CONSTRAINT [FK_ProductionTaskPlaces_Places]
    FOREIGN KEY ([ProductionTaskID])
    REFERENCES [dbo].[Places]
        ([PlaceID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [PlaceID] in table 'Rests'
ALTER TABLE [dbo].[Rests]
ADD CONSTRAINT [FK_Rests_Places]
    FOREIGN KEY ([PlaceID])
    REFERENCES [dbo].[Places]
        ([PlaceID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Rests_Places'
CREATE INDEX [IX_FK_Rests_Places]
ON [dbo].[Rests]
    ([PlaceID]);
GO

-- Creating foreign key on [PlaceID] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [FK_Users_Places]
    FOREIGN KEY ([PlaceID])
    REFERENCES [dbo].[Places]
        ([PlaceID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Users_Places'
CREATE INDEX [IX_FK_Users_Places]
ON [dbo].[Users]
    ([PlaceID]);
GO

-- Creating foreign key on [ProductID] in table 'Rests'
ALTER TABLE [dbo].[Rests]
ADD CONSTRAINT [FK_Rests_Products]
    FOREIGN KEY ([ProductID])
    REFERENCES [dbo].[Products]
        ([ProductID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [RoleID] in table 'RolePermits'
ALTER TABLE [dbo].[RolePermits]
ADD CONSTRAINT [FK_RolePermits_Roles1]
    FOREIGN KEY ([RoleID])
    REFERENCES [dbo].[Roles]
        ([RoleID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [RoleID] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [FK_Users_Roles1]
    FOREIGN KEY ([RoleID])
    REFERENCES [dbo].[Roles]
        ([RoleID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Users_Roles1'
CREATE INDEX [IX_FK_Users_Roles1]
ON [dbo].[Users]
    ([RoleID]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------