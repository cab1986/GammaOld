﻿#pragma checksum "..\..\..\Dialogs\ChangeSpoolDialog.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "BC67DE73E9E9D39668B85FE803A7E40D"
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

using DevExpress.Xpf.DXBinding;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.DataPager;
using DevExpress.Xpf.Editors.DateNavigator;
using DevExpress.Xpf.Editors.ExpressionEditor;
using DevExpress.Xpf.Editors.Filtering;
using DevExpress.Xpf.Editors.Flyout;
using DevExpress.Xpf.Editors.Popups;
using DevExpress.Xpf.Editors.Popups.Calendar;
using DevExpress.Xpf.Editors.RangeControl;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Editors.Settings.Extension;
using DevExpress.Xpf.Editors.Validation;
using DevExpress.Xpf.LayoutControl;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace Gamma.Dialogs {
    
    
    /// <summary>
    /// ChangeSpoolDialog
    /// </summary>
    public partial class ChangeSpoolDialog : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 13 "..\..\..\Dialogs\ChangeSpoolDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton RadioCompletly;
        
        #line default
        #line hidden
        
        
        #line 15 "..\..\..\Dialogs\ChangeSpoolDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton RadioReminder;
        
        #line default
        #line hidden
        
        
        #line 20 "..\..\..\Dialogs\ChangeSpoolDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal DevExpress.Xpf.Editors.SpinEdit EdtRemainderWeight;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\..\Dialogs\ChangeSpoolDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton RadioBroke;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\..\Dialogs\ChangeSpoolDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal DevExpress.Xpf.Editors.SpinEdit EdtBrokeWeight;
        
        #line default
        #line hidden
        
        
        #line 36 "..\..\..\Dialogs\ChangeSpoolDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal DevExpress.Xpf.Editors.ComboBoxEdit LkpBrokeReason;
        
        #line default
        #line hidden
        
        
        #line 46 "..\..\..\Dialogs\ChangeSpoolDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnOk;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\..\Dialogs\ChangeSpoolDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnCancel;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/Gamma;component/dialogs/changespooldialog.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Dialogs\ChangeSpoolDialog.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.RadioCompletly = ((System.Windows.Controls.RadioButton)(target));
            
            #line 13 "..\..\..\Dialogs\ChangeSpoolDialog.xaml"
            this.RadioCompletly.Checked += new System.Windows.RoutedEventHandler(this.RadioCompletly_Checked);
            
            #line default
            #line hidden
            return;
            case 2:
            this.RadioReminder = ((System.Windows.Controls.RadioButton)(target));
            
            #line 15 "..\..\..\Dialogs\ChangeSpoolDialog.xaml"
            this.RadioReminder.Checked += new System.Windows.RoutedEventHandler(this.RadioReminder_Checked);
            
            #line default
            #line hidden
            return;
            case 3:
            this.EdtRemainderWeight = ((DevExpress.Xpf.Editors.SpinEdit)(target));
            return;
            case 4:
            this.RadioBroke = ((System.Windows.Controls.RadioButton)(target));
            
            #line 27 "..\..\..\Dialogs\ChangeSpoolDialog.xaml"
            this.RadioBroke.Checked += new System.Windows.RoutedEventHandler(this.RadioBroke_Checked);
            
            #line default
            #line hidden
            return;
            case 5:
            this.EdtBrokeWeight = ((DevExpress.Xpf.Editors.SpinEdit)(target));
            return;
            case 6:
            this.LkpBrokeReason = ((DevExpress.Xpf.Editors.ComboBoxEdit)(target));
            return;
            case 7:
            this.BtnOk = ((System.Windows.Controls.Button)(target));
            
            #line 46 "..\..\..\Dialogs\ChangeSpoolDialog.xaml"
            this.BtnOk.Click += new System.Windows.RoutedEventHandler(this.BtnOK_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.BtnCancel = ((System.Windows.Controls.Button)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

