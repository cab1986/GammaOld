   M S B A M L   `   `   `  ???? G  CGamma, Version=2.1.7454.29719, Culture=neutral, PublicKeyToken=null+clr-namespace:Gamma.CommonGamma.Common  (    "Gamma.Views.ProductionTasksSGBView   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35a ]DevExpress.Xpf.Core.v16.1, Version=16.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49aR NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     f bDevExpress.Xpf.Grid.v16.1.Core, Version=16.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49al hDevExpress.Xpf.Grid.v16.1.Extensions, Version=16.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49aa ]DevExpress.Xpf.Grid.v16.1, Version=16.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a@dxg2http://schemas.devexpress.com/winfx/2008/xaml/grid    ]	 YDevExpress.Mvvm.v16.1, Version=16.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49aAdxmvvm2http://schemas.devexpress.com/winfx/2008/xaml/mvvm  	 ?dxe5http://schemas.devexpress.com/winfx/2008/xaml/editors  ;dx2http://schemas.devexpress.com/winfx/2008/xaml/core  'commonclr-namespace:Gamma.Common  .??? .+?'   !Gamma.Common.GridControlWithState  )  #DevExpress.Xpf.Grid.DataControlBase#    EnableSmartColumnsGeneration  .  $??Stretch=?%  DevExpress.Xpf.Grid.GridControl   AllowLiveDataShaping$ FalseX?   SelectionMode$
 Cell=? ? UseLayoutRounding .     ItemsSource ?? *ProductionTaskBatchesSGB+   SelectedItem ?? *SelectedProductionTaskBatch+   Bands )  #DevExpress.Xpf.Grid.GridControlBand  $  DevExpress.Xpf.Grid.BaseColumn   Header)  #DevExpress.Xpf.Core.ObjectConverter$ 
Общие '    HorizontalHeaderContentAlignment$ Center=?	   VisibleIndex$	 0??
   Columns.
 $  DevExpress.Xpf.Grid.GridColumn  $  DevExpress.Xpf.Grid.ColumnBase   	FieldName$ Number??$ 
Номер    Width2	  ,DevExpress.Xpf.Grid.GridColumnWidthConverter$ 80	 $	 0??  $ 	DateBegin??$ Дата начала    AllowBestFit]
 YDevExpress.Data.v16.1, Version=16.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a5
 
 /DevExpress.Utils.Design.DefaultBooleanConverter$
 True
 $	 140	 $	 1??  $ Place??$ Передел $	 100	 $	 2??  $0 *Номенклатура и раскрой $ Center=?$	 1??.
   $ Nomenclature??$ Номенклатура $
 True
 $	 3??   EditSettings 6  0DevExpress.Xpf.Editors.Settings.MemoEditSettings     ShowIcon .  6  0DevExpress.Xpf.Editors.Settings.TextEditSettings   TextWrapping$
 Wrap=?   PrintTextWrapping$
 WrapX?   CellTemplate ?? .R?%  DevExpress.Xpf.Editors.TextEdit    Name$ PART_Editor??$??24(?  $ 01 "  DevExpress.Xpf.Grid.BandBase   GridRow$ 1??$
 True
 $ 50	    
FixedWidth . $	 4??   Binding ?? *NomenclatureKind[0]+ ?? Mode$ TwoWay=?  $ 02 $ 1??$
 True
 $ 50	  . $	 5?? ?? *NomenclatureKind[1]+$ TwoWay=?  $ 03 $ 1??$
 True
 $ 50	  . $	 6?? ?? *NomenclatureKind[2]+$ TwoWay=?  $ 04 $ 1??$
 True
 $ 50	  . $	 7?? ?? *NomenclatureKind[3]+$ TwoWay=?  $ 05 $ 1??$
 True
 $ 50	  . $	 8?? ?? *NomenclatureKind[4]+$ TwoWay=?  $ 06 $ 1??$
 True
 $ 50	  . $	 9?? ?? *NomenclatureKind[5]+$ TwoWay=?  $ 07 $ 1??$
 True
 $ 50	  . $	 10?? ?? *NomenclatureKind[6]+$ TwoWay=?  $ 08 $ 1??$
 True
 $ 50	  . $	 11?? ?? *NomenclatureKind[7]+$ TwoWay=?  $ 09 $ 1??$
 True
 $ 50	  . $	 12?? ?? *NomenclatureKind[8]+$ TwoWay=?  $ 10 $ 1??$
 True
 $ 50	  . $	 13?? ?? *NomenclatureKind[9]+$ TwoWay=?  $ 11 $ 1??$
 True
 $ 50	  . $	 14?? ?? *NomenclatureKind[10]+$ TwoWay=?  $ 12 $ 1??$
 True
 $ 50	  . $	 15?? ?? *NomenclatureKind[11]+$ TwoWay=?  $ 13 $ 1??$
 True
 $ 50	  . $	 16?? ?? *NomenclatureKind[12]+$ TwoWay=?  $ 14 $ 1??$
 True
 $ 50	  . $	 17?? ?? *NomenclatureKind[13]+$ TwoWay=?  $ 15 $ 1??$
 True
 $ 50	  . $	 18?? ?? *NomenclatureKind[14]+$ TwoWay=?  $ 16 $ 1??$
 True
 $ 50	  . $	 19?? ?? *NomenclatureKind[15]+$ TwoWay=?  $ 1??$	 20??  $ 01 $ 2??$
 True
 $ 50	  . $	 21?? ?? *	Format[0]+$ TwoWay=?  $ 02 $ 2??$
 True
 $ 50	  . $	 22?? ?? *	Format[1]+$ TwoWay=?  $ 03 $ 2??$
 True
 $ 50	  . $	 23?? ?? *	Format[2]+$ TwoWay=?  $ 04 $ 2??$
 True
 $ 50	  . $	 24?? ?? *	Format[3]+$ TwoWay=?  $ 05 $ 2??$
 True
 $ 50	  . $	 25?? ?? *	Format[4]+$ TwoWay=?  $ 06 $ 2??$
 True
 $ 50	  . $	 26?? ?? *	Format[5]+$ TwoWay=?  $ 07 $ 2??$
 True
 $ 50	  . $	 27?? ?? *	Format[6]+$ TwoWay=?  $ 08 $ 2??$
 True
 $ 50	  . $	 28?? ?? *	Format[7]+$ TwoWay=?  $ 09 $ 2??$
 True
 $ 50	  . $	 29?? ?? *	Format[8]+$ TwoWay=?  $ 10 $ 2??$
 True
 $ 50	  . $	 30?? ?? *	Format[9]+$ TwoWay=?  $ 11 $ 2??$
 True
 $ 50	  . $	 31?? ?? *
Format[10]+$ TwoWay=?  $ 12 $ 2??$
 True
 $ 50	  . $	 32?? ?? *
Format[11]+$ TwoWay=?  $ 13 $ 2??$
 True
 $ 50	  . $	 33?? ?? *
Format[12]+$ TwoWay=?  $ 14 $ 2??$
 True
 $ 50	  . $	 34?? ?? *
Format[13]+$ TwoWay=?  $ 15 $ 2??$
 True
 $ 50	  . $	 35?? ?? *
Format[14]+$ TwoWay=?  $ 16 $ 2??$
 True
 $ 50	  . $	 36?? ?? *
Format[15]+$ TwoWay=?  $ 2??$ Общий формат $	 37?? . $	 140	  ?? *TotalFormat+     Visible .   ?? *	EnumColor+  $ Выполнение $	 2??.
   $ TaskQuantity??$ Задание, тн $
 True
 $	 100	 $	 38?? ?? .R?  $ PART_Editor??$??24(?   MaskType$ Numeric=?   MaskUseAsDisplayFormat .   $ MadeQuantity??$" Изготовлено, тн $
 True
 $	 250	 $	 39??   	SortOrder$ 	Ascending=?   	SortIndex$ 0?? ?? .R?  $ PART_Editor??$??24(?   View #  DevExpress.Xpf.Grid.TableView     AllowPerPixelScrolling . &   DevExpress.Xpf.Grid.GridViewBase   ShowGroupPanel .      AllowBestFit  . !   BestFitMode$! VisibleRows=?"   BestFitArea$
" Rows=?&   DevExpress.Xpf.Grid.DataViewBase#   AllowEditing# .  $   	AutoWidth$ . $??18(? %   AllowMoveColumnToDropArea% .  &   FormatConditions& )  #DevExpress.Xpf.Grid.FormatCondition  -  'DevExpress.Xpf.Grid.FormatConditionBase'   
Expression$#' [RowData.Row.EnumColor] = '1'??(   	FieldName$( 	EnumColor??1  +DevExpress.Xpf.Grid.ExpressionConditionBase)   
ApplyToRow) . *   Format.* 6  0DevExpress.Xpf.Core.ConditionalFormatting.Format  +   
Background+ ?	#FF35D31B  $#' [RowData.Row.EnumColor] = '2'??) . $( 	EnumColor??.*   + ?	#FFECE20C  $#' [RowData.Row.EnumColor] = '3'??) . $( 	EnumColor??.*   
+ ??i??2  ,DevExpress.Mvvm.UI.Interactivity.Interaction,   	Behaviors, '  !DevExpress.Mvvm.UI.EventToCommand  ?  ?DevExpress.Mvvm.UI.Interactivity.EventTriggerBase`1[[System.Windows.DependencyObject, WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35]]-   	EventName$- MouseDoubleClick??+  %DevExpress.Mvvm.UI.EventToCommandBase.   Command. ?? *EditItemCommand+