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
        ProductPallet,
        [Description("Групповые упаковки")]
        ProductGroupPack
    }
    public enum PlaceGroups { PM, RW, Convertings, WR, Other }  // Группы переделов. Привязаны к таблице в базе, менять только вместе!
    public enum ProductStates // Качество продукта
    {
        [Description("Годная")]
        Good,
        [Description("Требует решения")]
        NeedsDecision,
        [Description("На утилизацию")]
        Broke,
        [Description("Ограниченная партия")]
        Limited
    } 
    public enum ProductStatesFilter // Фильтр качества для поиска
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
    public enum SpoolChangeState { FullyConverted, WithBroke, WithRemainder} // Как переработан тамбур
    
    public enum DocProductKinds
    {
        DocProductSpool,
        DocProductUnload,
        DocProductPallet,
        DocProductGroupPack
    }
    public enum DocTypes
    {
        DocProduction,
        DocWithdrawal,
        DocMovement,
        DocCloseShift,
        DocChangeState
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
