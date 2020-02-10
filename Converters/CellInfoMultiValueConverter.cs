using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;

/*
 * [XAML]
 <dxmvvm:Interaction.Behaviors>  
                                    <!--  MultiBinding  -->  
                                    <dxmvvm:EventToCommand Command="{Binding ProcessCellCommand}" EventName="RowDoubleClick">  
                                        <dxmvvm:EventToCommand.CommandParameter>  
                                            <MultiBinding Converter="{local:CellInfoMultiValueConverter}">  
                                                <Binding ElementName="grid" Path="CurrentItem" />  
                                                <Binding ElementName="grid" Path="CurrentColumn.FieldName" />  
                                            </MultiBinding>  
                                        </dxmvvm:EventToCommand.CommandParameter>  
                                    </dxmvvm:EventToCommand>  
                                    <!--  EventArgsConverter  -->  
                                    <dxmvvm:EventToCommand Command="{Binding ProcessCellCommand}"  
                                                           EventArgsConverter="{local:RowDoubleClickEventArgsConverter}"  
                                                           EventName="RowDoubleClick"  
                                                           PassEventArgsToCommand="True" />  
                                </dxmvvm:Interaction.Behaviors>
                          */

namespace Gamma.Converters
{
    class CellInfoMultiValueConverter : MarkupExtensionBase, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return values.ToList();
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RowDoubleClickEventArgsConverter : MarkupExtensionBase, IEventArgsConverter
    {
        public object Convert(object obj, object e)
        {
            RowDoubleClickEventArgs args = e as RowDoubleClickEventArgs;
            if (args != null)
            {
                var view = (DataViewBase)obj;
                return new List<object> { view.DataControl.GetRow(args.HitInfo.RowHandle), args.HitInfo.Column.FieldName };
            }
            return null;
        }
    }

    public abstract class MarkupExtensionBase : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
