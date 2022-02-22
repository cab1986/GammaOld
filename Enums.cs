﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.ComponentModel;

namespace Gamma
{
    /// <summary>
    /// Виды номенклатуры (1,2,3 соответствует 1С, не рекомендуется использовать в программе, 
    /// лучше передавать SQL в качестве параметра)
    /// </summary>
    public enum NomenclatureKinds
    {
        Any, HalfStuff, Material, Product
    }
    /// <summary>
    /// Типы материалов, применяемых в производстве. Прямая привязка к БД.
    /// </summary>
    public enum MaterialType
    {
        [Description("Материалы СГБ")]
        MaterialsSGB,
        [Description("Материалы СГИ")]
        MaterialsSGI
    }
    public enum ProcessModels
    {
        [Description("БДМ")]
        PM,
        [Description("БДМ=>ПРС")]
        PMRw,
        [Description("БДМ=>Упаковка")]
        PMWr,
        [Description("БДМ=>ПРС=>Упаковка")]
        PMRwWr,
        [Description("ПРС")]
        Rw,
        [Description("ПРС=>Упаковка")]
        RwWr

    }
    public enum BatchKinds { SGB, SGI, Baler }

    public enum ProductKind
    {
        [Description("Тамбура")]
        ProductSpool,
        [Description("Паллеты")]
        ProductPallet,
        [Description("Групповые упаковки")]
        ProductGroupPack,
        [Description("Россыпь")]
        ProductPalletR,
        [Description("Кипы")]
        ProductBale
    }
    /// <summary>
    /// Группы переделов. Привязаны к таблице в базе, менять только вместе!
    /// </summary>
    public enum PlaceGroup { PM, Rw, Convertings, Wr, Other, Warehouses, Baler, Services }
    /// <summary>
    /// Качество продукта
    /// </summary>
    public enum ProductState
    {
        [Description("Годная")]
        Good,
        [Description("Требует решения")]
        NeedsDecision,
        [Description("На утилизацию")]
        Broke,
        [Description("Ограниченная партия")]
        Limited,
        [Description("Хоз. нужды")]
        InternalUsage,
        [Description("На переделку")]
        ForConversion,
        [Description("На переупаковку")]
        Repack
    }

    public enum ToughnessKinds
    {
        [Description("↑")]
        Higher,
        [Description("Цель")]
        Target,
        [Description("↓")]
        Lower
    }
    /// <summary>
    /// Фильтр качества для поиска
    /// </summary>
    public enum ProductStatesFilter
    {
        [Description("Годная")]
        Good,
        [Description("Требует решения")]
        NeedsDecision,
        [Description("На утилизацию")]
        Broke,
        [Description("Ограниченная партия")]
        Limited,
        [Description("Не подтвержден")]
        NotConfirmed
    }
    /// <summary>
    /// Как переработан тамбур
    /// </summary>
    public enum SpoolChangeState { FullyConverted, WithBroke, WithRemainder }

    public enum DocProductKinds
    {
        DocProductSpool,
        DocProductUnload,
        DocProductPallet,
        DocProductGroupPack,
        DocProductBale,
        DocProductPalletR
    }
    public enum DocTypes
    {
        [Description("Производство")]
        DocProduction,
        [Description("Списание")]
        DocWithdrawal,
        [Description("Премещение")]
        DocMovement,
        [Description("Закрытие смены")]
        DocCloseShift,
        [Description("Изменение состояния продукта")]
        DocChangeState,
        [Description("Отгрузка")]
        DocShipment,
        [Description("Распаковка")]
        DocUnpack,
        [Description("Акт о браке")]
        DocBroke,
        [Description("Заказ на перемещение")]
        DocMovementOrder,
        [Description("Приемка на склад")]
        DocWarehouseAccept,
        [Description("Инвентаризация")]
        DocInventarisation,
        [Description("Комплектация")]
        DocComplectation,
        [Description("Утилизация")]
        DocUtilization,
        [Description("Уточнение кол-ва")]
        DocSpecificstionQuantity,
        [Description("Остатки на раскате ")]
        DocUnwinderRemainder,
        [Description("Производство сырья и материалов")]
        DocMaterialProduction,
        [Description("Задание на производство")]
        ProductionTask,
        [Description("Акт на переупаковку")]
        DocRepack,
        [Description("Решение о браке")]
        DocBrokeDecision
    }
    public enum PermissionMark
    {
        [Description("Нет доступа")]
        NoAccess,
        [Description("Чтение")]
        Read,
        [Description("Чтение и запись")]
        ReadAndWrite
    }
    /// <summary>
    /// Состояние задания на производство
    /// </summary>
    public enum ProductionTaskStates
    {
        [Description("На рассмотрении")]
        NeedsDecision,
        [Description("В производстве")]
        InProduction,
        [Description("Выполнено")]
        Completed,
        [Description("На редактировании")]
        OnEditing,
        [Description("Отменено")]
        Canceled
    }

    public enum PersonTypes
    {
        Loader = 1
    }

    public enum OrderType
    {
        ShipmentOrer,
        InternalOrder,
        MovementOrder
    }

    public enum SourceSpoolsCheckResult
    {
        Correct,
        Block,
        Warning
    }

    public enum RemainderType
    {
        [Description("На начало периода")]
        Begin,
        [Description("на конец периода")]
        End
    }
    /// <summary>
    /// Тип возвращаемых данных из com порта
    /// </summary>
    public enum DateReadFromComPortType
    {
        Byte = 0,
        String = 1
    }

    public enum DocMaterialProductionTypes
    {
        NotUsed,
        RemainderAtBegin,
        RemainderAtEnd,
        Dismiss,
        In,
        Send,
        RemainderInGRVAtEnd,
        Standard,
        Utilization,
        Experimental,
        Out,
        SendAtBegin,
        SendAtEnd,
        IsNotSend,
        IsFullSend,
        IsNotCalculatedRemainderAtEnd,
        InToCompositionTank
    }

    public enum DocCloseShiftMaterialTypes
    {
        Withdrawal,
        In,
        Out,
        RemainderAtEnd,
        RemainderAtBegin,
        Utilization,
        Experimental,
        RePack,
        Standard,
        Dismiss
    }

    public enum CriticalLogTypes
    {
        [Description("Запуск программы")]
        StartProgramInformation,
        [Description("Информационное сообщение в процессе работы")]
        Information,
        [Description("Сообщение об ошибке")]
        Error
    }

    public enum WithdrawalTypes
    {
        [Description("По норме")]
        WithdrawalByFact,
        [Description("По факту")]
        WithdrawalByNormal
    }
}