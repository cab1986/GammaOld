   M S B A M L   `   `   `  ???? 	  Gamma%    Gamma.Views.ComPortSettingsView   a ]DevExpress.Xpf.Core.v16.1, Version=16.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a?dxe5http://schemas.devexpress.com/winfx/2008/xaml/editors  \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     ;dx2http://schemas.devexpress.com/winfx/2008/xaml/core  j fDevExpress.Xpf.LayoutControl.v16.1, Version=16.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49aFdxlc;http://schemas.devexpress.com/winfx/2008/xaml/layoutcontrol    ? Title$'  !Настройка COM-порта??$	??475??$	??500??.???  ? RowDefinitions ?? ?? $??35 ?.+?&   DevExpress.Xpf.Core.DXTabControl  .@?#  DevExpress.Xpf.Core.DXTabItem  ?  ?DevExpress.Xpf.Core.Native.HeaderedSelectorItemBase`2[[DevExpress.Xpf.Core.DXTabControl, DevExpress.Xpf.Core.v16.1, Version=16.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a],[DevExpress.Xpf.Core.DXTabItem, DevExpress.Xpf.Core.v16.1, Version=16.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a]]   Header$ Сканер??.??? 
?????? .+?0  *DevExpress.Xpf.LayoutControl.LayoutControl  .  (DevExpress.Xpf.LayoutControl.LayoutGroup   Header$ Header??   Orientation$ Vertical=? ? UseLayoutRounding . .+?  $ LayoutGroup??   View$ Group=?$ Vertical=?.+?-  'DevExpress.Xpf.LayoutControl.LayoutItem     Label$ ComPort??   Content. )  #DevExpress.Xpf.Editors.ComboBoxEdit  +  %DevExpress.Xpf.Editors.LookUpEditBase	   ItemsSource	 ?? *ComPortsList+%	  DevExpress.Xpf.Editors.BaseEdit
 	  	EditValue
 ?? *ScannerComPortNumber+  $ BaudRate??.   	 ?? *BaudRatesList+
 ?? *ScannerBaudRate+  $ DataBits??.   	 ?? *DataBitsList+
 ?? *ScannerDataBits+  $ StopBits??.      ValueMember$	 Key??   DisplayMember$ Value??	 ?? *StopBitsList+
 ?? *ScannerStopBits+  $ Parity??.   $	 Key??$ Value??	 ?? *
ParityList+
 ?? *ScannerParity+  $ FlowControl??.   $	 Key??$ Value??	 ?? *HandShakeList+
 ?? *ScannerHandShake+  $ Весы??.??? 
?????? .+?  $ Header??$ Vertical=? . .+?  $ LayoutGroup??$ Group=?$ Vertical=?.+?  $ ComPort??.   	 ?? *ComPortsList+
 ?? *ScalesComPortNumber+  $ BaudRate??.   	 ?? *BaudRatesList+
 ?? *ScalesBaudRate+  $ DataBits??.   	 ?? *DataBitsList+
 ?? *ScalesDataBits+  $ StopBits??.   $	 Key??$ Value??	 ?? *StopBitsList+
 ?? *ScalesStopBits+  $ Parity??.   $	 Key??$ Value??	 ?? *
ParityList+
 ?? *ScalesParity+  $ FlowControl??.   $	 Key??$ Value??	 ?? *HandShakeList+
 ?? *ScalesHandShake+  $. (Тип получаемых данных??.   $	 Key??$ Value??	 ?? *TypeDateReadFromComPortList+
 ?? *TypeDateReadFromComPort+ ?? UpdateSourceTrigger$ PropertyChanged=?  $ Префикс??. %
  DevExpress.Xpf.Editors.TextEdit
  
 ?? *ScalesPrefix+{??? *" IsEnabledDependingOnTypeDataRead+  $ Постфикс??. 
  
 ?? *ScalesPostfix+{??? *" IsEnabledDependingOnTypeDataRead+  $ 
Horizontal=?.+??? $??40??$??80?????? *GetWeightCommand+.???? .+??? $??Получить??$??10(?$??Black"??? $??Вес??$??10(?$??Black"?  $ Вес, кг??$??Center=?$??40??$	??100??$??55,0,0,0q?. 
  $??Center=?
 ?? *Weight+  .+?  $' !Полученная строка??$??Center=?$??40??. 
  $??Center=?
 ?? *ReadLineFromSerialPort+?? $??OK??$??1??$??Right=?$??3,3,70,3q????? *SaveAndCloseCommand+?? $??Отмена??$??1??$??Right=?$??3q????? *CloseCommand+