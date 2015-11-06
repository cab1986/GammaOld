using DevExpress.Xpf.Grid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Gamma
{
    public class GridControlWithBands : GridControl
    {
        public static readonly DependencyProperty BandsSourceProperty =
            DependencyProperty.Register("BandsSource", typeof(IList), typeof(GridControlWithBands), new PropertyMetadata(null, OnBandedSourcePropertyChanged));
        public static readonly DependencyProperty BandTemplateProperty =
            DependencyProperty.Register("BandTemplate", typeof(DataTemplate), typeof(GridControlWithBands), new PropertyMetadata(null));
        public IList BandsSource
        {
            get { return (IList)GetValue(BandsSourceProperty); }
            set { SetValue(BandsSourceProperty, value); }
        }
        public DataTemplate BandTemplate
        {
            get { return (DataTemplate)GetValue(BandTemplateProperty); }
            set { SetValue(BandTemplateProperty, value); }
        }

        private static void OnBandedSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GridControlWithBands)d).OnBandedSourceChanged(e);
        }
        private void OnBandedSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            Bands.Clear();
            foreach (var b in BandsSource)
            {
                ContentControl cc = BandTemplate.LoadContent() as ContentControl;
                if (cc == null)
                    continue;
                GridControlBand band = cc.Content as GridControlBand;
                cc.Content = null;
                if (band == null)
                    continue;
                band.DataContext = b;
                Bands.Add(band);
            }
        }
    }
    public class MyGridControlBand : GridControlBand
    {
        public static readonly DependencyProperty ColumnsSourceProperty =
            DependencyProperty.Register("ColumnsSource", typeof(IList), typeof(MyGridControlBand), new PropertyMetadata(null, OnColumnsSourcePropertyChanged));
        public static readonly DependencyProperty ColumnTemplateProperty =
            DependencyProperty.Register("ColumnTemplate", typeof(DataTemplate), typeof(MyGridControlBand), new PropertyMetadata(null));


        public MyColumnTemplateSelector ColumnTemplateSelector
        {
            get { return (MyColumnTemplateSelector)GetValue(ColumnTemplateSelectorProperty); }
            set { SetValue(ColumnTemplateSelectorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColumnTemplateSelector.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnTemplateSelectorProperty =
            DependencyProperty.Register("ColumnTemplateSelector", typeof(MyColumnTemplateSelector), typeof(MyGridControlBand), new PropertyMetadata(null));





        public IList ColumnsSource
        {
            get { return (IList)GetValue(ColumnsSourceProperty); }
            set { SetValue(ColumnsSourceProperty, value); }
        }
        public DataTemplate ColumnTemplate
        {
            get { return (DataTemplate)GetValue(ColumnTemplateProperty); }
            set { SetValue(ColumnTemplateProperty, value); }
        }

        private static void OnColumnsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MyGridControlBand)d).OnColumnsSourceChanged(e);
        }
        private void OnColumnsSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            Columns.Clear();
            foreach (var b in ColumnsSource)
            {
                DataTemplate custColumnTemplate = null;
                if (ColumnTemplateSelector != null)
                {
                    custColumnTemplate = ColumnTemplateSelector.SelectTemplate(b, null);
                }
                if (ColumnTemplate != null)
                {
                    custColumnTemplate = ColumnTemplate;
                }
                if (custColumnTemplate != null)
                {
                    ContentControl cc = custColumnTemplate.LoadContent() as ContentControl;
                    if (cc == null)
                        continue;
                    GridColumn column = cc.Content as GridColumn;
                    cc.Content = null;
                    if (column == null)
                        continue;
                    column.DataContext = b;
                    Columns.Add(column);
                }
            }
        }
    }

    public class MyColumnTemplateSelector : DataTemplateSelector
    {
        public DataTemplate template1 { get; set; }
        public DataTemplate template2 { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            ColumnItem ci = item as ColumnItem;
            if (ci != null)
            {
                if (ci.ColumnFieldName == "Name")
                {
                    return template2;
                }
                return template1;
            }

            return base.SelectTemplate(item, container);

        }
    }
    public class BandItem
    {
        public string BandHeader
        {
            get;
            set;
        }
        public List<ColumnItem> Columns
        {
            get;
            set;
        }
    }
    public class ColumnItem
    {
        public string ColumnFieldName
        {
            get;
            set;
        }
    }
}
