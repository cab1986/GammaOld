using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Gamma
{
    public enum ProductionTaskKinds { ProductionTaskPM, ProductionTaskRW, ProductionTaskWR, ProductionTaskConverting }
    public enum ProductKinds 
    { 
        [Description("Тамбура")]
        ProductSpool, 
        [Description("Паллеты")]
        ProductPallet
    }
    public enum PlaceGroups { PM, RW, Convertings }  // Группы переделов. Привязаны к таблице в базе, менять только вместе!
    public enum ProductStatesFilter
    {
        [Description("Не подтвержден")]
        NotConfirmed,
        [Description("Годная")]
        Good,
        [Description("На утилизацию")]
        Broke,
        [Description("Требует решения")]
        NeedDecision,
        [Description("Ограниченная партия")]
        Limited
    }
    public enum SpoolChangeState { FullyConverted, WithBroke, WithRemainder}
    
    public enum DocProductKinds
    {
        DocProductSpool,
        DocProductUnload,
        DocProductPallet
    }
    public enum DocTypes
    {
        DocProduction,
        DocWithdrawal,
        DocMovement
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
    public enum ProductionTaskStates
    {
        [Description("На рассмотрении")]
        NeedsDecision,
        [Description("В производстве")]
        InProduction,
        [Description("Выполнено")]
        Completed
    }
}
