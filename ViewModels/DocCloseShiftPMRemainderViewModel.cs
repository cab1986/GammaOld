using GalaSoft.MvvmLight;
using System;
using Gamma.Models;
using System.Linq;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DocCloseShiftPMRemainderViewModel : SaveImplementedViewModel
    {
        /// <summary>
        /// Initializes a new instance of the DocCloseShiftPMRemainderViewModel class.
        /// </summary>
        public DocCloseShiftPMRemainderViewModel()
        {
        }
        public DocCloseShiftPMRemainderViewModel(Guid docID)
        {
            DocCloseShiftRemainder = DB.GammaBase.DocCloseShiftRemainders.Where(d => d.DocID == docID).Select(d => d).FirstOrDefault();
            Quantity = DocCloseShiftRemainder.Quantity;
        }
        private DocCloseShiftRemainders DocCloseShiftRemainder { get; set; }
        public decimal Quantity { get; set; }
        public override void SaveToModel(Guid ItemID)
        {
            base.SaveToModel();
            if (DocCloseShiftRemainder == null)
            {
                DocCloseShiftRemainder = new DocCloseShiftRemainders()
                {
                    DocCloseShiftRemainderID = SQLGuidUtil.NewSequentialId(),
                    DocID = ItemID,
                    Quantity = Quantity
                };
                DB.GammaBase.DocCloseShiftRemainders.Add(DocCloseShiftRemainder);
            }
            else 
            {
                DocCloseShiftRemainder.Quantity = Quantity;
            }
            DB.GammaBase.SaveChanges();
        }
    }
}