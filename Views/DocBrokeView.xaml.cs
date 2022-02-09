using System;
using Gamma.ViewModels;
using System.Windows.Data;
using System.Collections.Generic;
using Gamma.Models;

namespace Gamma.Views
{
    /// <summary>
    /// Логика взаимодействия для DocBrokeView.xaml
    /// </summary>
    public partial class DocBrokeView
    {
        public DocBrokeView(Guid docId, Guid? productId = null, bool isInFuturePeriod = false)
        {
            DataContext = new DocBrokeViewModel(docId, productId, isInFuturePeriod);
            InitializeComponent();
        }
    }
}
