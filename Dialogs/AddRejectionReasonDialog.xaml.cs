// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using Gamma.Entities;
using Gamma.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.ComponentModel;
using System.Windows.Controls;

namespace Gamma.Dialogs
{
    /// <summary>
    /// Interaction logic for AddRejectionReasonDialog.xaml
    /// </summary>
    public partial class AddRejectionReasonDialog : UserControl //Window, INotifyPropertyChanged
    {
        public AddRejectionReasonDialog()
        {
            InitializeComponent();
        }
    }
}
