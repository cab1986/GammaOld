   M S B A M L   `   `   `  ????	  Gamma%    Gamma.Views.ComPortSettingsView   5   	   a ]DevExpress.Xpf.Core.v16.1, Version=16.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a?dxe5http://schemas.devexpress.com/winfx/2008/xaml/editors  \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   ;dx2http://schemas.devexpress.com/winfx/2008/xaml/core  5   	   j fDevExpress.Xpf.LayoutControl.v16.1, Version=16.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49aFdxlc;http://schemas.devexpress.com/winfx/2008/xaml/layoutcontrol  6G     ? Title$'  !Настройка COM-порта??5   	   $	??475??6%   $	??500??62   .??5      ?  ? RowDefinitions 5   
   ?? 5      ?? 5	      $??35 ?6   .+?5   
   &   DevExpress.Xpf.Core.DXTabControl  .@?5      #  DevExpress.Xpf.Core.DXTabItem  ?  ?DevExpress.Xpf.Core.Native.HeaderedSelectorItemBase`2[[DevExpress.Xpf.Core.DXTabControl, DevExpress.Xpf.Core.v16.1, Version=16.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a],[DevExpress.Xpf.Core.DXTabItem, DevExpress.Xpf.Core.v16.1, Version=16.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a]]   Header$ Сканер??6   .??5      ? 
?????? .+?5      0  *DevExpress.Xpf.LayoutControl.LayoutControl  .  (DevExpress.Xpf.LayoutControl.LayoutGroup   Header$ Header??6)      Orientation$ Vertical=?69    ? UseLayoutRounding . .+?5        $ LayoutGroup??6+      View$ Group=?6@   $ Vertical=?6M   .+?5      -  'DevExpress.Xpf.LayoutControl.LayoutItem     Label$ ComPort??6.      Content. 5   "   )  #DevExpress.Xpf.Editors.ComboBoxEdit  +  %DevExpress.Xpf.Editors.LookUpEditBase	   ItemsSource	 63   ?? *ComPortsList+%	  DevExpress.Xpf.Editors.BaseEdit
 	  	EditValue
 6X   ?? *ScannerComPortNumber+5        5      $ BaudRate??6.   . 5   "     	 63   ?? *BaudRatesList+
 6Y   ?? *ScannerBaudRate+5        5      $ DataBits??6.   . 5   "     	 63   ?? *DataBitsList+
 6X   ?? *ScannerDataBits+5        5      $ StopBits??6.   . 5   "        ValueMember$	 Key??6~      DisplayMember$ Value??6?   	 63   ?? *StopBitsList+
 6X   ?? *ScannerStopBits+6?   5        5      $ Parity??6.   . 5   "     $	 Key??6z   $ Value??6?   	 63   ?? *
ParityList+
 6V   ?? *ScannerParity+6?   5        5      $ FlowControl??6.   . 5    "     $	 Key??6?   $ Value??6?   	 63   ?? *HandShakeList+
 6Y   ?? *ScannerHandShake+6?   5!      5"      5#      5$      5%        5&      $ Весы??6   .??5'      ? 
?????? .+?5(        $ Header??6)   $ Vertical=?69    . .+?5)        $ LayoutGroup??6+   $ Group=?6@   $ Vertical=?6M   .+?5*        $ ComPort??6.   . 5+   "     	 63   ?? *ComPortsList+
 6X   ?? *ScalesComPortNumber+5,        5-      $ BaudRate??6.   . 5.   "     	 63   ?? *BaudRatesList+
 6Y   ?? *ScalesBaudRate+5/        50      $ DataBits??6.   . 51   "     	 63   ?? *DataBitsList+
 6X   ?? *ScalesDataBits+52        53      $ StopBits??6.   . 54   "     $	 Key??6}   $ Value??6?   	 63   ?? *StopBitsList+
 6X   ?? *ScalesStopBits+6?   55        56      $ Parity??6.   . 57   "     $	 Key??6y   $ Value??6?   	 63   ?? *
ParityList+
 6V   ?? *ScalesParity+6?   58        59      $ FlowControl??6.   . 5:   "     $	 Key??6   $ Value??6?   	 63   ?? *HandShakeList+
 6Y   ?? *ScalesHandShake+6?   5;        5<      $. (Тип получаемых данных??6.   . 5=   "     $	 Key??6?   $ Value??6?   	 63   ?? *TypeDateReadFromComPortList+
 6g   ?? *TypeDateReadFromComPort+ ?? UpdateSourceTrigger$ PropertyChanged=?6?   5>        5?      $ Префикс??6.   . 5@   "   %
  DevExpress.Xpf.Editors.TextEdit
  
 6/   ?? *ScalesPrefix+{?6R   ?? *" IsEnabledDependingOnTypeDataRead+5A        5B      $ Постфикс??6.   . 5C   "   
  
 6/   ?? *ScalesPostfix+{?6T   ?? *" IsEnabledDependingOnTypeDataRead+5D      5E        5F      $ 
Horizontal=?6+   .+?5G      ?? $??40??6J   $??80??6V   ??6%   ?? *GetWeightCommand+.??5H   "   ?? .+?5I   &   ?? $??Получить??6,   $??10(?6?   $??Black"?6M   ?? 5J   &   $??Вес??6,   $??10(?6:   $??Black"?6H   5K   #   5L        5M      $ Вес, кг??6.   $??Center=?6>   $??40??6`   $	??100??6l   $??55,0,0,0q?6x   . 5N   "   
  $??Center=?6L   
 6/   ?? *Weight+6L   5O      5P        5Q      .+?5R        $' !Полученная строка??6.   $??Center=?6H   $??40??6j   . 5S   "   
  $??Center=?6\   
 6/   ?? *ReadLineFromSerialPort+6\   5T      5U      5V      5W      5X      5Y      ?? 5Z   
   $??OK??6   $??1??6G   $??Right=?6T   $??3,3,70,3q?6p   ??6   ?? *SaveAndCloseCommand+6p   ?? 5[   
   $??Отмена??6   $??1??6C   $??Right=?6P   $??3q?6l   ??6"   ?? *CloseCommand+6l   5\      5]      